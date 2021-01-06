using Autofac;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Redis;
using Rebirth.Server.Center;
using System;
using System.Linq;
using System.Net.Sockets;
using Rebirth.Common.Types;

namespace Rebirth.Server.Shop
{
	public sealed class WvsShop : WvsServerBase<WvsShopClient>
	{
		public BestItemManager BestItems { get; set; }

		public WvsShop(WvsCenter parent) : base("WvsShop", Constants.ShopPort, parent)
		{
			BestItems = new BestItemManager();

			PacketHandler.Add((short)RecvOps.CP_MigrateIn, Handle_MigrateIn, false);
			PacketHandler.Add((short)RecvOps.CP_UserTransferFieldRequest, Handle_UserTransferFieldRequest);
			//PacketHandler.Add((short)RecvOps.CP_CashShopChargeParamRequest, Handle_CP_CashShopChargeParamRequest);
			PacketHandler.Add((short)RecvOps.CP_CashShopQueryCashRequest, Handle_CashShopQueryCashRequest);
			PacketHandler.Add((short)RecvOps.CP_CashShopCashItemRequest, Handle_CashShopCashItemRequest);
			PacketHandler.Add((short)RecvOps.CP_CashShopCheckCouponRequest, Handle_CashShopCheckCouponRequest);
			//PacketHandler.Add((short)RecvOps.CP_CashShopGiftMateInfoRequest, Handle_CashShopGiftMateInfoRequest);
			PacketHandler.Add((short)RecvOps.CP_CheckTransferWorldPossible, Handle_CheckTransferWorldPossible);
			PacketHandler.Add((short)RecvOps.CP_CheckNameChangePossible, Handle_CheckNameChangePossible);
			PacketHandler.Add((short)RecvOps.CP_CashItemGachaponRequest, Handle_CashItemGachaponRequest);
			/**
			 * LP_CashShopChargeParamResult = 0x17E, // v--- CCashShop::OnPacket
        LP_CashShopQueryCashResult = 0x17F,
        LP_CashShopCashItemResult = 0x180,
        LP_CashShopPurchaseExpChanged = 0x181,
        LP_CashShopGiftMateInfoResult = 0x182,
        LP_CashShopCheckDuplicatedIDResult = 0x183,
        LP_CashShopCheckNameChangePossibleResult = 0x184,
        LP_CashShopRegisterNewCharacterResult = 0x185,
        LP_CashShopCheckTransferWorldPossibleResult = 0x186,
        LP_CashShopGachaponStampItemResult = 0x187,
        LP_CashShopCashItemGachaponResult = 0x188,
        LP_CashShopCashGachaponOpenResult = 0x189,
        LP_ChangeMaplePointResult = 0x18A,
        LP_CashShopOneADay = 0x18B,
        LP_CashShopNoticeFreeCashItem = 0x18C,
        LP_CashShopMemberShopResult = 0x18D,*/
		}

		//-----------------------------------------------------------------------------

		protected override WvsShopClient CreateClient(Socket socket)
		{
			return new WvsShopClient(this, socket);
		}

		protected override void HandleDisconnect(WvsShopClient client)
		{
			base.HandleDisconnect(client);

			var acc = client.Account;

			if (acc != null)
			{
				acc.Save();

				if (client.Character != null)
				{
					client.Character.Stats.Channel = 0; // wait why are we assigning this
					client.Character.Save();
				}

				var storage = ServerApp.Container.Resolve<CenterStorage>();

				if (!storage.IsCharacterMigrate(client.Character.dwId))
				{
					storage.RemoveAccountOnline(acc.ID);
					storage.RemoveCharacterCSITC(client.Character.dwId);
					storage.RemoveCharacterOnline(client.Character.dwId);

					client.Character.NotifySocialChannels(SocialNotiflag.LogOut);

					// removing object from pool is the last thing we do to ensure no null pointer errors
					MasterManager.CharacterPool.Remove(client.Character);
				}

				// social channels are notified when char enters wvsgame

				BestItems.Sort();
			}
		}

		//-----------------------------------------------------------------------------

		private void Handle_UserTransferFieldRequest(WvsShopClient c, CInPacket p)
		{
			var center = ServerApp.Container.Resolve<WvsCenter>();

			var port = (short)center.WvsGames[c.Character.ChannelID].Port;

			var storage = ServerApp.Container.Resolve<CenterStorage>();
			storage.AddCharacterMigrate(c.Character.dwId);
			storage.RemoveCharacterCSITC(c.Character.dwId);

			// social channels are notified when char enters wvsgame

			c.SendPacket(CPacket.MigrateCommand(Constants.ServerAddress, port));
		}

		// THANKS MAXCLOUD

		private void Handle_MigrateIn(WvsShopClient c, CInPacket p)
		{
			var charId = p.Decode4();
			var adminClient = p.Decode1();

			// TODO this could be hijacked so we need extra validation here

			var storage = ServerApp.Container.Resolve<CenterStorage>();

			var charMigrate = storage.IsCharacterMigrate(charId);
			var charOnline = storage.IsCharacterOnline(charId);
			var migrateCsItc = storage.IsCharacterCSITC(charId);

			if (!charMigrate || !charOnline || !migrateCsItc)
			{
				c.Disconnect(); // TODO notify social channels?? but this shouldn't happen to normal players
				return;
			}

			c.Load(charId);

			storage.RemoveCharacterMigrate(charId);

			// social channels are notified when char leaves wvsgame

			// ----------------------------------

			if (c.Character is null)
			{
				Log.Error("Char object is null");
				c.Disconnect();
				return;
			}

			// ----------------------------------

			c.SendPacket(CPacket.CCashShop.MigrateIn(c.Character));

			c.SendPacket(CPacket.CCashShop.LockerData(c));
			c.SendPacket(CPacket.CCashShop.WishListData(c, false));
			c.EnableActions();
		}


		private void Handle_CashShopQueryCashRequest(WvsShopClient c, CInPacket p)
		{
			c.EnableActions();
		}

		private void Handle_CashShopCheckCouponRequest(WvsShopClient c, CInPacket p)
		{
			p.Decode2(); // something

			var code = p.DecodeString();

			var coupon = new CouponCode(code, c);

			if (coupon.Invalid)
			{
				c.SendPacket(CPacket.CCashShop.CouponFail(CashItemFailed.InvalidCoupon));
			}
			else if (coupon.Used)
			{
				c.SendPacket(CPacket.CCashShop.CouponFail(CashItemFailed.UsedCoupon));
			}
			else if (coupon.IncorrectAccount)
			{
				c.SendPacket(CPacket.CCashShop.CouponFail(CashItemFailed.NotAvailableCoupon));
			}
			else if (coupon.Expired)
			{
				c.SendPacket(CPacket.CCashShop.CouponFail(CashItemFailed.ExpiredCoupon));
			}
			else if (coupon.Items != null && !InventoryManipulator.HasSpace(c.Character, coupon.Items))
			{
				c.SendPacket(CPacket.CCashShop.CouponFail(CashItemFailed.NoEmptyPos)); // TODO fix this
			}
			else
			{
				if (coupon.Items != null)
					foreach (var item in coupon.Items)
					{
						InventoryManipulator.InsertInto(c.Character, MasterManager.CreateItem(item, false));
						CPacket.CCashShop.CouponAddItem(item);
					}

				c.Account.AccountData.NX_Prepaid += coupon.NX;
				coupon.Dispose();
				c.Account.Save();
			}

			c.EnableActions();
		}

		// TODO crush ring: [13 01] [1F] [06 00] [30 30 30 30 30 30] [01 00 00 00] [BC E8 3E 01] [04 00] [74 65 73 74] [05 00] [74 65 73 74 0A]
		private void Handle_CashShopCashItemRequest(WvsShopClient c, CInPacket p)
		{
			// v95: CCashShop::ProcessBuy

			var op = (CashItemOps) p.Decode1();
			switch (op)
			{
				case CashItemOps.CashItemReq_FriendShip:
					CashOperation.FriendShip(c, p);
					break;
				case CashItemOps.CashItemReq_Buy:
					CashOperation.Buy(c, p);
					break;
				case CashItemOps.CashItemReq_BuyPackage:
					CashOperation.BuyPackage(c, p);
					break;
				case CashItemOps.CashItemReq_Gift:
					CashOperation.Gift(c, p);
					break;
				case CashItemOps.CashItemReq_GiftPackage:
					CashOperation.GiftPackage(c, p);
					break;
				case CashItemOps.CashItemReq_SetWish:
					CashOperation.ModifyWishList(c, p);
					break;
				case CashItemOps.CashItemReq_MoveLtoS:
					CashOperation.MoveLToS(c, p);
					break;
				case CashItemOps.CashItemReq_MoveStoL:
					CashOperation.MoveSToL(c, p);
					break;
				case CashItemOps.CashItemReq_IncSlotCount:
					CashOperation.IncSlotCount(c, p);
					break;
				case CashItemOps.CashItemReq_IncTrunkCount: // TODO storage slots
					CashOperation.IncTrunkCount(c, p);
					break;
				case CashItemOps.CashItemReq_IncCharSlotCount:
					CashOperation.IncCharSlotCount(c, p);
					break;
				case CashItemOps.CashItemReq_BuyNormal:
					CashOperation.BuyNormal(c, p);
					break;

				// UNHANDLED BELOW
				case CashItemOps.CashItemReq_BuyTransferWorld:
					c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_TransferWorld_Failed, CashItemFailed.NotAvailableTime));
					break;
				case CashItemOps.CashItemReq_BuyNameChange:
					c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_NameChangeBuy_Failed, CashItemFailed.NotAvailableTime));
					break;
				case CashItemOps.CashItemReq_CancelChangeName:
					c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_CancelNameChangeFail, CashItemFailed.NotAvailableTime));
					break;
			}
			c.EnableActions();
		}

		private void Handle_CashItemGachaponRequest(WvsShopClient c, CInPacket p)
		{
			// Recv [CP_CashItemGachaponRequest] [B9 00] [02 5A 62 02 00 00 00 00]
			var liItemSN = p.Decode8();

			if (c.Account.AccountData.Locker.GetBySN(liItemSN) is CashItemInfo item && item.nItemID == 5222000)
			{
				return; // not working right now
				//var commodity = MasterManager.EtcTemplates.commodities
				//		.Where(cm => cm.OnSale 
				//		             && cm.Count <= 1 
				//		             && cm.ItemID / 10000 != 910
				//		             && cm.ItemID / 100 != 11128)
				//		.Random();

				//var rand_item = MasterManager.CreateCashCommodityItem(commodity.CashItemSN);
				//c.Account.AccountData.Locker.Add(rand_item);
				//c.SendPacket(CPacket.CCashShop.CashItemGachaponSuccess(liItemSN, item.Count - 1, rand_item, false));
				//c.Account.AccountData.Locker.Remove(item);
			}
			else
			{
				c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_GashItemGachapon_Failed, CashItemFailed.Unknown));
			}
		}

		private void Handle_CheckNameChangePossible(WvsShopClient c, CInPacket p)
		{
			// BMS
			//	if ((signed int)(unsigned __int8)CUser::GetLevel(this) >= 10 )
			//{
			//		dwID = CInPacket::Decode4(iPacket);
			//		nBirthDate = CInPacket::Decode4(iPacket);
			//		if (nBirthDate == CUser::GetBirthDate(thisa))
			//		{
			//			COutPacket::COutPacket(&oPacket, 7, 0);
			//			v7 = 0;
			//			COutPacket::Encode4(&oPacket, thisa->m_uLocalSocketSN);
			//			COutPacket::Encode4(&oPacket, dwID);
			//			COutPacket::Encode4(&oPacket, nBirthDate);
			//			v2 = TSingleton < CCenter >::GetInstance();
			//			CCenter::SendPacket(v2, &oPacket);
			//			v7 = -1;
			//			COutPacket::~COutPacket(&oPacket);
			//		}
			//		else
			//		{
			//			CUser::SendRequestFailPacket(thisa, 64, 147, 0);
			//		}
			//	}
			//else
			//	{
			//		CUser::SendRequestFailPacket(thisa, 64, 154, 0);
			//	}
			c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_NameChangeBuy_Failed, CashItemFailed.NotAvailableTime));
		}

		private void Handle_CheckTransferWorldPossible(WvsShopClient c, CInPacket p)
		{
			// BMS
			//		dwCharacterID = CInPacket::Decode4(iPacket);
			//		nBirthDate = CInPacket::Decode4(iPacket);
			//		if (nBirthDate == CUser::GetBirthDate(thisa))
			//		{
			//			if ((signed int)(unsigned __int8)CUser::GetLevel(thisa) >= 20 )
			//   {
			//				COutPacket::COutPacket(&oPacket, 9, 0);
			//				v7 = 0;
			//				COutPacket::Encode4(&oPacket, thisa->m_uLocalSocketSN);
			//				COutPacket::Encode4(&oPacket, dwCharacterID);
			//				COutPacket::Encode4(&oPacket, nBirthDate);
			//				v2 = TSingleton < CCenter >::GetInstance();
			//				CCenter::SendPacket(v2, &oPacket);
			//				v7 = -1;
			//				COutPacket::~COutPacket(&oPacket);
			//			}

			//else
			//			{
			//				CUser::SendRequestFailPacket(thisa, 119, 161, 0);
			//			}
			//		}
			//		else
			//		{
			//			CUser::SendRequestFailPacket(thisa, 119, 147, 0);
			//		}
			c.SendPacket(CPacket.CCashShop.RequestFailPacket(CashItemOps.CashItemRes_TransferWorld_Failed, CashItemFailed.NotAvailableTime));
		}
	}
}