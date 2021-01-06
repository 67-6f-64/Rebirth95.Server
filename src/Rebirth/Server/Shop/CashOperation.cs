using System;
using log4net;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Entities;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Server.Shop
{
	public sealed class CashOperation
	{
		public static ILog Log = LogManager.GetLogger(typeof(CashOperation));

		/**
     * Client sends cash item SN.
     * We index cash items by their serial number.
     * WZ files indexes cash items by a commodity ID.
     * The commodity ID (~12000 total) is not used for anything.
     */
		public static void Buy(WvsShopClient c, CInPacket p)
		{
			// validate packet length
			if (p.Available < 9) return;

			p.Decode1(); // 00
			var cashType = (CashType)p.Decode4();
			var nCommoditySN = p.Decode4();

			var commodityInfo = MasterManager.CommodityProvider[nCommoditySN];

			if (commodityInfo == null) return;
			if (!commodityInfo.OnSale) return; // TODO proper error code/response

			var item = MasterManager.CreateCashCommodityItem(nCommoditySN);

			if (item is null) return;

			if (!c.Account.HasCash(cashType, item.NXCost)) return; // TODO proper error code/response

			if (ItemConstants.IsPet(item.nItemID) &&
				InventoryManipulator.GetItemByCashSN(c.Character, InventoryType.Cash, item.SN).Item2 != null)
				return; // cant have two of the same pet cuz it screws up our indexing

			c.CashLocker.Add(item);
#if DEBUG
			c.Account.ModifyCash(cashType, 10000);
			Log.Info($"{commodityInfo.CashItemSN}");
#else
			c.Account.ModifyCash(cashType, -commodityInfo.Price);
#endif
			item.dwAccountID = c.Account.ID;
			item.dwCharacterID = c.dwCharId;

			c.SendPacket(CPacket.CCashShop.BuyResponse(item));
			// do best items/limited goods handling here
		}

		/**
     * Cash packages have data in both the Commodity and Package section of Etc.wz
     * Client sends package SN which is stored in the commodity section of WZ files (one of the ~12000 entries)
     * The commodity itemid (not SN) contains the SN for the CashPackage
     * The CashPackage (one of ~450) contains the cash item SN's of the package items
     */
		public static void BuyPackage(WvsShopClient c, CInPacket p)
		{
			if (p.Available < 9) return;

			p.Decode1(); // 00
			var cashType = (CashType)p.Decode4();
			var nCommoditySN = p.Decode4();

			var commodityInfo = MasterManager.CommodityProvider[nCommoditySN];

			if (commodityInfo is null) return;
			if (!commodityInfo.OnSale) return; // TODO proper error code/response
			if (!c.Account.HasCash(cashType, commodityInfo.Price)) return; // TODO proper error code/response

			var packagedata = MasterManager.CreateCashPackageItems(commodityInfo.ItemID);

			packagedata.ForEach(item => { item.dwAccountID = c.Account.ID; item.dwCharacterID = c.dwCharId; });

#if RELEASE
		c.Account.ModifyCash(cashType, -commodityInfo.Price);
#else
			foreach (var item in packagedata)
			{
				item.dwAccountID = c.Account.ID;
				item.dwCharacterID = c.dwCharId;
				c.Account.AccountData.Locker.Add(item);
			}
			Log.Info($"================COMMODITYSN {nCommoditySN}"); // temporary so i can quickly find a buncha serial numbers
			return;
#endif

			c.SendPacket(CPacket.CCashShop.BuyPackageResponse(packagedata));
		}

		public static void Gift(WvsShopClient c, CInPacket p)
		{
			// TODO
		}

		public static void GiftPackage(WvsShopClient c, CInPacket p)
		{
			// TODO
		}

		/// <summary>
		/// Client sends an updated wish list when something is changed.
		/// Wish list max size is 10.
		/// Client expects to receive a full wish list packet back.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>
		public static void ModifyWishList(WvsShopClient c, CInPacket p)
		{
			if (p.Available < 40) return;

			c.Character.Stats.WishList = p.DecodeIntArray(10);

			c.SendPacket(CPacket.CCashShop.WishListData(c, true));
			c.EnableActions();
		}

		/// <summary>
		/// Move an item from the character's account cash locker to the character's storage.
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>
		public static void MoveLToS(WvsShopClient c, CInPacket p)
		{
			if (p.Available < 8) return;

			var sn = p.Decode8();

			var cashItem = c.CashLocker.GetBySN(sn);

			// unable to find item
			if (cashItem == null) return;

			var nTargetSlot = InventoryManipulator.InsertInto(c.Character, cashItem.Item);

			// item insertion failed
			if (nTargetSlot == 0) return; // TODO proper error code/response

			c.CashLocker.Remove(cashItem);

			c.SendPacket(CPacket.CCashShop.MoveLtoSResponse(cashItem, nTargetSlot));
		}

		/// <summary>
		/// Move an item from the character's storage to the character's account cash locker
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>
		public static void MoveSToL(WvsShopClient c, CInPacket p)
		{
			if (p.Available < 9) return;

			var cashCommodityId = p.Decode8();
			var nTI = (InventoryType)p.Decode1();

			var (itemSlot, item) = InventoryManipulator.GetItemByCashSN(c.Character, nTI, cashCommodityId);

			// unable to find item in inventory
			if (item is null) return;

			if (ItemConstants.IsRing(item.nItemID))
			{
				c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_MoveStoL_Failed, CashItemFailed.NotAvailableTime));
				return;
			}

			var newItem = MasterManager.CreateCashCommodityItem(item.liCashItemSN);
			newItem.Item.nNumber = item.nNumber;
			newItem.Item.liSN = item.liSN;
			newItem.Item.tDateExpire = item.tDateExpire;
			//newItem.dwCharacterID = // TODO
			newItem.dwAccountID = c.Character.Account.ID;

			c.CashLocker.Add(newItem);
			InventoryManipulator.RemoveFrom(c.Character, nTI, itemSlot, -1);

			c.SendPacket(CPacket.CCashShop.MoveSToLResponse(c.CashLocker, newItem));
		}

		public static void IncTrunkCount(WvsShopClient c, CInPacket p)
		{
			// storage space increase
		}

		public static void IncCharSlotCount(WvsShopClient c, CInPacket p)
		{
			// validate packet length
			if (p.Available < 9) return;

			p.Decode1();
			var cashType = p.Decode4();
			var commodityId = p.Decode4();

			//Log.Debug("CASH COMMODITY ID: " + commodityId);

			if (!c.Account.HasCash((CashType)cashType, 6900) ||
				(c.Account.AccountData.CharacterSlots + 3) >
#if DEBUG
				27
#else
				Constants.MaxCharSlot
#endif
				)
			{
				c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_Buy_Failed, CashItemFailed.PurchaseLimitOver));
			}
			else
			{
				c.Account.AccountData.CharacterSlots += 3;
				c.SendPacket(CPacket.CCashShop.IncCharSlotCountResponse((short)c.Account.AccountData.CharacterSlots));
			}
		}

		public static void IncSlotCount(WvsShopClient c, CInPacket p)
		{
			// [13 01] [06] [00] [01 00 00 00] [01] [1F FE FD 02]
			// [13 01] [06] [00] [04 00 00 00] [00] [01]

			p.Skip(1);

			short nInventorySlots;

			var nCashType = (CashType) p.Decode4();
			if (p.Decode1() != 0)
			{
				var nCommSN = p.Decode4();

				switch (nCommSN)
				{
					case 50200093: // equip
						break;
					case 50200094: // use
					case 50200197: // install
					case 50200095: // etc
					default:

						break;
				}

				return;
			}
			else
			{
				var nType = (InventoryType)p.Decode1();
				switch (nType)
				{
					case InventoryType.Equip:
						c.Character.InventoryEquip.SlotLimit += 4;
						nInventorySlots = c.Character.InventoryEquip.SlotLimit;
						break;
					case InventoryType.Consume:
						c.Character.InventoryConsume.SlotLimit += 4;
						nInventorySlots = c.Character.InventoryEquip.SlotLimit;
						break;
					case InventoryType.Etc:
						c.Character.InventoryEtc.SlotLimit += 4;
						nInventorySlots = c.Character.InventoryEquip.SlotLimit;
						break;
					case InventoryType.Install:
						c.Character.InventoryInstall.SlotLimit += 4;
						nInventorySlots = c.Character.InventoryEquip.SlotLimit;
						break;
					default: 
						return;
				}

				c.SendPacket(CPacket.CCashShop.CashItemResIncSlotCountDone(nType, nInventorySlots));
			}
		}

		/// <summary>
		/// 1 Meso item purchases
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>
		public static void BuyNormal(WvsShopClient c, CInPacket p)
		{
			if (p.Available < 4) return;

			var cashItemSN = p.Decode4();
			var item = MasterManager.CreateCashCommodityItem(cashItemSN);

			if (item is null) return;
			if (c.Character.Stats.nMoney < 1) return;

			c.Character.Modify.GainMeso(-1);

			InventoryManipulator.InsertInto(c.Character, item.Item);
			c.SendPacket(CPacket.CCashShop.BuyNormalResponse(item.nItemID));
		}

		public static void FriendShip(WvsShopClient c, CInPacket p)
		{
			//COutPacket::EncodeStr(&oPacket, v25);
			//COutPacket::Encode4(&oPacket, dwOption);
			//COutPacket::Encode4(&oPacket, nCommSN);
			//ZXString<char>::ZXString<char>(&v25, &sGiveTo);
			//COutPacket::EncodeStr(&oPacket, v25);
			//ZXString<char>::ZXString<char>(&v25, &sText);
			//COutPacket::EncodeStr(&oPacket, v25);

			var sSPW = p.DecodeString(); // pic
			var nCashType = (CashType)p.Decode4();
			var nCommoditySN = p.Decode4();
			var sTargetCharacterName = p.DecodeString();
			var sGiftMessage = p.DecodeString();

			var commodityInfo = MasterManager.CommodityProvider[nCommoditySN];
			var item = MasterManager.CreateCashCommodityItem(nCommoditySN);

			if (commodityInfo == null) return;
			if (!commodityInfo.OnSale) return; // TODO proper error code/response
			if (!c.Account.HasCash(nCashType, item.NXCost)) return; // TODO proper error code/response
			if (!ItemConstants.is_friendship_equip_item(item.nItemID)) return; // hax 

			var pTargetChar = MasterManager.CharacterPool.Get(sTargetCharacterName, false);

			if (pTargetChar is null || pTargetChar.dwId == c.Character.dwId)
			{
				c.SendPacket(CPacket.CCashShop
					.RequestFailPacket(CashItemOps.CashItemRes_Friendship_Failed, CashItemFailed.GiftUnknownRecipient));
			}
			else
			{
				var sn = DateTime.Now.Ticks;

				{
					var targetitem = MasterManager.CreateCashCommodityItem(nCommoditySN);

					targetitem.GiftMessage = sGiftMessage; // uhhhh TODO make this actually do something
					targetitem.BuyerCharName = c.Character.Stats.sCharacterName;
					targetitem.dwAccountID = c.Account.ID;
					targetitem.dwCharacterID = c.Character.dwId;
					targetitem.Item.liSN = sn;
					targetitem.Item.liCashItemSN = targetitem.SN;

					pTargetChar.Account.AccountData.Locker.Add(targetitem);
					pTargetChar.SendMessage("You have received a gift! Go check it out in your cash shop locker!");
					pTargetChar.RingInfo.Insert(new GW_FriendRecord
					{
						dwPairCharacterID = c.Character.dwId,
						dwFriendItemID = item.nItemID,
						liPairSN = sn + 1,
						liSN = sn,
						sPairCharacterName = c.Character.Stats.sCharacterName
					});
				}

				{
					item.dwAccountID = c.Account.ID;
					item.dwCharacterID = c.Character.dwId;
					item.Item.liSN = sn + 1;
					item.Item.liCashItemSN = item.SN;

					c.SendPacket(CPacket.CCashShop.CashItemResFriendShipDone(item, pTargetChar.Stats.sCharacterName));
					c.Account.AccountData.Locker.Add(item);
					c.Account.ModifyCash(nCashType, commodityInfo.Price);

					c.Character.RingInfo.Insert(new GW_FriendRecord
					{
						dwPairCharacterID = pTargetChar.dwId,
						dwFriendItemID = item.nItemID,
						liSN = sn + 1,
						liPairSN = sn,
						sPairCharacterName = pTargetChar.Stats.sCharacterName
					});
				}
			}
		}
	}
}