using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Entities.PlayerData;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Server.Shop;
using Rebirth.Server.Shop.Commodity;
using System;
using System.Collections.Generic;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CCashShop
		{

			// TODO phase all the error packets into this instead of having individual packets for them
			public static COutPacket RequestFailPacket(CashItemOps nRetCode, CashItemFailed nReason, int nCommSN = 0)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)nRetCode);
				p.Encode1((byte)nReason);

				if (nCommSN > 0)
					p.Encode4(nCommSN);

				return p;
			}

			public static COutPacket CashItemGachaponSuccess(long liItemSN, int nCountLeft, CashItemInfo item, bool bJackpot)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemGachaponResult);
				p.Encode1((byte)CashItemOps.CashItemRes_CashItemGachapon_Done);
				p.Encode8(liItemSN);
				p.Encode4(nCountLeft);
				item.Encode(p);
				p.Encode4(item.nItemID);
				p.Encode1((byte)nCountLeft);
				p.Encode1(bJackpot);
				return p;
			}

			public static COutPacket IncCharSlotCountResponse(short amount)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_IncCharSlotCount_Done);
				p.Encode2(amount);
				return p;
			}

			/**
             * CCashShop::OnCashItemResLoadGiftDone
             */
			public static COutPacket LoadGiftDone()
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode2(0);


				return p;
			}

			/**
             * CCashShop::OnChargeParamResult
             */
			public static COutPacket ChargeParam(string sNexonClubID)
			{
				var p = new COutPacket(SendOps.LP_CashShopChargeParamResult);

				p.EncodeString(sNexonClubID);

				return p;
			}

			public static COutPacket MigrateIn(Character c)
			{
				var p = new COutPacket(SendOps.LP_SetCashShop);

				DbCharFlags flag = DbCharFlags.CHARACTER;
				flag |= DbCharFlags.MONEY;
				flag |= DbCharFlags.ITEMSLOTEQUIP;
				flag |= DbCharFlags.ITEMSLOTCONSUME;
				flag |= DbCharFlags.ITEMSLOTINSTALL;
				flag |= DbCharFlags.ITEMSLOTETC;
				flag |= DbCharFlags.ITEMSLOTCASH;
				flag |= DbCharFlags.INVENTORYSIZE;

				c.Encode(p, flag); // m_character

				p.Encode1(true); // m_bCashShopAuthorized
				p.EncodeString(c.Account.Username); // m_sNexonClubID

				// CWvsContext::SetSaleInfo -> v95 || CShopInfo::EncodeSaleInfo -> BMS
				p.Encode4(CashShopConstants.DisabledItems.Length);
				foreach (var item in CashShopConstants.DisabledItems)
				{
					p.Encode4(item);
				}

				var modifiedCommodities = new CommodityCollection();

				CashShopConstants.ModifiedCommodities
					.ForEach(mc => modifiedCommodities.Add(mc));

				modifiedCommodities.Encode(p);

				// modified items -- Rebirth.Server.Shop.Commodity.CommodityData.Encode(...)
				//p.Encode2(0); // count
				//			  // loop modified items

				// category discount rate -> foreach: byte (category), byte (tab), byte (rate) 
				p.Encode1(0); // count
							  // loop discounted items
							  // (byte) Main tab (1 = event, 2 = equip, 3 = use, 4 = special, 5 = etc, 6 = pet, 7 = package)
							  // (byte) Sub tab (Starts at 0)
							  // (byte) Discount rate

				p.EncodeBuffer(new byte[1080], 0, 1080);

				// VERTISY
				//// aBest(buffer of 1080)
				//for (int j = 0; j < 2; j++)
				//{// gender
				//	int[] aBest = { 50200004, 50200069, 50200117, 50100008, 50000047 };
				//	int index = 0;
				//	List<BestItem> bestItems = new ArrayList<>(CashItemFactory.bestItems.values());
				//	Collections.sort(bestItems);
				//	for (BestItem bItem : bestItems)
				//	{
				//		if ((bItem.nCommodityGender == 2 || bItem.nCommodityGender == j))
				//		{
				//			aBest[index++] = bItem.sn;
				//		}
				//		if (index == aBest.length) break;
				//	}
				//	for (int i = 0; i < 9; i++)
				//	{// category
				//		for (int sn : aBest)
				//		{
				//			mplew.writeInt(i);// category
				//			mplew.writeInt(j);// gender
				//			mplew.writeInt(sn);// sn
				//		}
				//	}
				//	bestItems.clear();
				//}

				CLimitSell.Encode(p);

				//p.Encode2(0); // limit goods
				//p.Encode2(1);
				//{
				//	p.Encode4(1052017);
				//	p.Encode4(10002462);
				//	foreach (var i in new int[9])
				//	{
				//		p.Encode4(i);
				//	}

				//	p.Encode4(-1);
				//	p.Encode4(100);
				//	p.Encode4(50);
				//	p.Encode2(1);
				//	p.Encode4(0);
				//	p.Encode4(31);
				//	p.Encode4(0);
				//	p.Encode4(24);
				//	foreach(var item in new int[7])
				//	{
				//		p.Encode4(item + 1);
				//	}
				//}

				p.Encode2(0); // zero goods

				p.Encode1(false); // event

				p.Encode4(c.Account.HighestLevelChar().Item1); // highest char level in account


				return p;
			}

			public static COutPacket CouponFail(CashItemFailed result)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				p.Encode1((byte)CashItemOps.CashItemRes_UseCoupon_Failed);
				p.Encode1((byte)result);

				return p;
			}

			public static COutPacket CouponAddItem(int itemId)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				p.Encode1((byte)CashItemOps.CashItemRes_UseCoupon_Done);
				p.Encode4(0);
				p.Encode4(1);
				p.Encode2(1);
				p.Encode2(0x1A);
				p.Encode4(itemId);
				p.Encode4(0);

				return p;
			}

			public static COutPacket LockerData(WvsShopClient c)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				var data = c.Account.AccountData;

				p.Encode1((byte)CashItemOps.CashItemRes_LoadLocker_Done);
				p.Encode2((short)data.Locker.Count);
				foreach (var item in data.Locker) // m_aCashItemInfo
				{
					data.Locker.EncodeItem(item, p); // CInPacket::DecodeBuffer(v2, v3->m_aCashItemInfo.a, 55 * v4);
				}
				p.Encode2(data.Locker.SlotMax); // m_nTrunkCount
				p.Encode2((short)data.CharacterSlots); // m_nCharacterSlotCount
				p.Encode2((short)(data.CharacterSlots - 3)); // m_nBuyCharacterCount
				p.Encode2((short)c.Account.LoadCharIdList().Count); // m_nCharacterCount

				return p;
			}

			public static COutPacket MoveSToLResponse(CashLocker locker, CashItemInfo item)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				p.Encode1((byte)CashItemOps.CashItemRes_MoveStoL_Done);
				locker.EncodeItem(item, p);

				return p;
			}

			public static COutPacket MoveLtoSResponse(CashItemInfo item, short slot)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				p.Encode1((byte)CashItemOps.CashItemRes_MoveLtoS_Done);
				p.Encode2(slot);
				item.Item.RawEncode(p, true);

				return p;
			}

			public static COutPacket WishListData(WvsShopClient c, bool update)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);

				p.Encode1((byte)(update
					? CashItemOps.CashItemRes_SetWish_Done
					: CashItemOps.CashItemRes_LoadWish_Done));

				for (int i = 0; i < 10; i++) // wishlist is always int[10]
				{
					p.Encode4(c.Character.Wishlist[i]);
				}

				return p;
			}

			public static COutPacket EnableShopActions(WvsShopClient c)
			{
				var p = new COutPacket(SendOps.LP_CashShopQueryCashResult);

				p.Encode4(c.Account.AccountData.NX_Credit); // nx cash
				p.Encode4(c.Account.AccountData.NX_Maplepoint); // maple point
				p.Encode4(c.Account.AccountData.NX_Prepaid); // prepaid nx

				return p;
			}

			public static COutPacket BuyResponse(CashItemInfo pItem)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_Buy_Done);
				pItem.Encode(p);
				return p;
			}

			public static COutPacket BuyPackageResponse(List<CashItemInfo> aItems)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_BuyPackage_Done);
				p.Encode1((byte)aItems.Count);
				aItems.ForEach(item => item.Encode(p));
				p.Encode2(0);
				return p;
			}

			public static COutPacket BuyNormalResponse(int nItemID)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_BuyNormal_Done);
				p.Encode4(1);
				p.Encode2(1);
				p.Encode1(0x0B);
				p.Encode1(0);
				p.Encode4(nItemID);
				return p;
			}

			public static COutPacket CashItemResFriendShipDone(CashItemInfo pItem, string sRcvCharacterName)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_Friendship_Done);
				pItem.Encode(p);
				p.EncodeString(sRcvCharacterName);
				p.Encode4(pItem.nItemID);
				p.Encode2(pItem.Item.nNumber);
				return p;
			}

			public static COutPacket CashItemResIncSlotCountDone(InventoryType nInvType, short nNewSlotAmount)
			{
				var p = new COutPacket(SendOps.LP_CashShopCashItemResult);
				p.Encode1((byte)CashItemOps.CashItemRes_IncSlotCount_Done);
				p.Encode1(nInvType);
				p.Encode2(nNewSlotAmount);
				return p;
			}
		}
	}
}
