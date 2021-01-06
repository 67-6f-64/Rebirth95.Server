using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Characters;
using Rebirth.Characters.Actions;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Stat;
using Rebirth.Common.Types;
using Rebirth.Entities;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.Life;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth
{
	public partial class CPacket
	{
		public static COutPacket MigrateCommand(byte[] ip, short port)
		{
			var p = new COutPacket(SendOps.LP_MigrateCommand);
			p.Encode1(true); //if false and !CClientSocket->m_bIsGuestID throw CMSEXception 0x21000002
			p.EncodeBuffer(ip, 0, ip.Length);
			p.Encode2(port);
			return p;
		}

		public static COutPacket AliveReq()
		{
			var p = new COutPacket(SendOps.LP_AliveReq);
			p.Encode1(0xDE);
			p.Encode1(0xAD);
			p.Encode1(0xBE);
			p.Encode1(0xEF);
			return p;
		}

		public static COutPacket CheckClientIntegrityRequest(byte[] aAhnHS_Request)
		{
			var p = new COutPacket(SendOps.LP_SecurityPacket);
			p.Encode1(2);
			p.Encode2((short)aAhnHS_Request.Length);
			p.EncodeBuffer(aAhnHS_Request, 0, aAhnHS_Request.Length);
			return p;
		}

		//WvsGame-----------------------------------------------------------------------------------------------------

		public static COutPacket SetField(Character c, int nChannel, int nWorld, LogoutGiftConfig g, DbCharFlags dbFlag = DbCharFlags.ALL)
		{
			return SetField(p =>
			{
				var s1 = Constants.Rand.Next();
				var s2 = Constants.Rand.Next();
				var s3 = Constants.Rand.Next();

				c.CalcDamage.SetSeed((uint)s1, (uint)s2, (uint)s3);
				c.m_RndActionMan.Seed((uint)s1, (uint)s2, (uint)s3);

				// have to encode raw values, cant use CRand32.Encode cuz its not raw
				p.Encode4(s1);
				p.Encode4(s2);
				p.Encode4(s3);

				//c.SendMessage($"S1: {c.m_RndActionMan.m_s1} | S2: {c.m_RndActionMan.m_s2} | S3: {c.m_RndActionMan.m_s3}");

				c.Encode(p, dbFlag); //CharacterData::Decode
				g.Encode(p); //CWvsContext::OnSetLogoutGiftConfig
			}, nChannel, nWorld, true);
		}

		public static COutPacket SetField(Character c, int nChannel, int nWorld)
		{
			return SetField(p =>
			{
				p.Encode1(0); // Revival Shit
				p.Encode4(c.Stats.dwPosMap);
				p.Encode1(c.Stats.nPortal);
				p.Encode4(c.Stats.nHP);
				p.Encode1(0); //Enables two Encode4's
			}, nChannel, nWorld, false);
		}

		private static COutPacket SetField(Action<COutPacket> action, int nChannel, int nWorld, bool bCharacterData)
		{
			var p = new COutPacket(SendOps.LP_SetField);

			CClientOptMan__EncodeOpt(p, 0);

			p.Encode4(nChannel);
			p.Encode4(nWorld);

			p.Encode1(1); //sNotifierMessage
			p.Encode1(bCharacterData); //  bCharacterData
			p.Encode2(0); //  nNotifierCheck     

			action?.Invoke(p);

			p.EncodeDateTime(DateTime.Now);
			return p;
		}

		public static COutPacket UserEmoticon(int uid, int nEmotion, int tDuration, byte bByItemOption)
		{
			var p = new COutPacket(SendOps.LP_UserEmotion);
			p.Encode4(uid);
			p.Encode4(nEmotion); //nEmoticon
			p.Encode4(tDuration); //tDuration
			p.Encode1(bByItemOption); //CUser->m_bEmotionByItemOption
			return p;
		}

		public static COutPacket GroupMessage(GroupMessageType nType, string sFrom, string sText)
		{
			var p = new COutPacket(SendOps.LP_GroupMessage);
			p.Encode1((byte)nType);
			p.EncodeString(sFrom);
			p.EncodeString(sText);
			return p;
		}

		public static COutPacket InventoryOperation(InventoryModifier ctx, bool bExcelReq = true)
		{
			var p = new COutPacket(SendOps.LP_InventoryOperation);
			p.Encode1(bExcelReq);
			ctx.Encode(p);
			return p;
		}
		public static COutPacket SortItemResult(byte nTypeInventory)
		{
			var p = new COutPacket(SendOps.LP_SortItemResult);
			p.Encode1(0); //Unused by client
			p.Encode1(nTypeInventory);
			return p;
		}

		public static COutPacket GatherItemResult(byte nTypeInventory)
		{
			var p = new COutPacket(SendOps.LP_GatherItemResult);
			p.Encode1(0);
			p.Encode1(nTypeInventory);
			return p;
		}

		public static COutPacket InventoryGrow(byte nTypeInventory, byte nNewLimit)
		{
			//TODO: Test!
			var p = new COutPacket(SendOps.LP_InventoryGrow);
			p.Encode1(nTypeInventory);
			p.Encode1(nNewLimit);
			return p;
		}

		public static COutPacket UserAvatarModified(Character c, byte nFlag = 1)
		{
			var p = new COutPacket(SendOps.LP_UserAvatarModified);
			p.Encode4(c.dwId);

			p.Encode1(nFlag); // flags actually |

			switch (nFlag)
			{
				case 1: // AvatarLook::Decode
					c.GetLook().Encode(p);
					break;
				case 2: // nSpeed
					p.Encode1(0);
					break;
				case 4: // CUser::SetCarryItemEffect
					p.Encode1(0);
					break;
			}

			c.RingInfo.EncodeEquippedRings(p);
			p.Encode4(c.nCompletedSetItemID);
			return p;
		}

		public static COutPacket Teleport(byte nPortal)
		{
			var p = new COutPacket(SendOps.LP_UserTeleport);
			p.Encode1(0); //bExclRequest
			p.Encode1(nPortal);
			return p;
		}

		public static COutPacket StatChanged(StatModifier mod, bool exclRequest = false)
		{
			var p = new COutPacket(SendOps.LP_StatChanged);
			p.Encode1(exclRequest);
			mod.Encode(p);
			p.Encode1(0);
			p.Encode1(0);
			return p;
		}

		public static COutPacket StatChanged(bool exclRequest = false)
		{
			var p = new COutPacket(SendOps.LP_StatChanged);
			p.Encode1(exclRequest);
			p.Encode4(0); //mod.Encode(p);
			p.Encode1(0);
			p.Encode1(0);
			return p;
		}

		public static COutPacket ChangeSkillRecord(SkillModifier ctx, bool exclRequest = true)
		{
			var p = new COutPacket(SendOps.LP_ChangeSkillRecordResult);
			p.Encode1(exclRequest);
			ctx.Encode(p);
			p.Encode1(3); //CMovePath::SetStatChangedPoint(CMovePath *this, char bStat)
			return p;
		}

		public static COutPacket StalkResult(List<Stalkee> apStalkees)
		{
			var p = new COutPacket(SendOps.LP_StalkResult);

			p.Encode4(apStalkees.Count);

			foreach (var pStalkee in apStalkees)
			{
				p.Encode4(pStalkee.dwId);

				if (pStalkee.bRemove)
				{
					p.Encode1(1);
				}
				else
				{
					p.Encode1(0);
					p.EncodeString(pStalkee.sName);
					p.Encode4(pStalkee.nX);
					p.Encode4(pStalkee.nY);
				}
			}

			return p;
		}

		public static COutPacket MiniMapOnOff(bool bMiniMapOnOff)
		{
			var p = new COutPacket(SendOps.LP_MiniMapOnOff);
			p.Encode1(bMiniMapOnOff);
			return p;
		}

		public static COutPacket SetPotionDiscountRate(byte nPotionDiscountRate)
		{
			var p = new COutPacket(SendOps.LP_SetPotionDiscountRate);
			p.Encode1(nPotionDiscountRate);
			return p;
		}

		public static COutPacket SendOpenFullClientLink()
		{
			return new COutPacket(SendOps.LP_SendOpenFullClientLink);
		}

		public static COutPacket TransferChannelReqIgnore(byte nReason)
		{
			//1: You cannot move that channel. Please try again later.
			//2: You cannot go into the cash shop. Please try again later.
			//3: The Item-Trading Shop is currently unavailable. Please try again later.
			//4: You cannot go into the trade shop, due to the limitation of user count.
			//5: You do not meet the minimum level requirement to access the Trade Shop.

			var p = new COutPacket(SendOps.LP_TransferChannelReqIgnored);
			p.Encode1(nReason);
			return p;
		}

		public static COutPacket SetGender(byte nGender)
		{
			var p = new COutPacket(SendOps.LP_SetGender);
			p.Encode1(nGender);
			return p;
		}

		public static COutPacket MapTransferResult(MapTransferRes nRes, bool bExt, IEnumerable<int> adwMapTransfer = null)
		{
			var p = new COutPacket(SendOps.LP_MapTransferResult);
			p.Encode1((byte)nRes);
			p.Encode1(bExt);

			if (adwMapTransfer != null) //Only for DeleteList | RegisterList
			{
				//5 or 10 fields depending on bExt
				foreach (var dwMapId in adwMapTransfer)
					p.Encode4(dwMapId);
			}

			return p;
		}

		public static COutPacket AntiMacroResult(byte nOperation, AntiMacroType nType, string sSnicth)
		{
			var p = new COutPacket(SendOps.LP_AntiMacroResult);
			p.Encode1(nOperation);

			//Crashes in the client i need to fix it
			//CWnd::CreateWnd((CWnd *)&v1->vfptr, -40 - v19, 10, v10, w, 10, 1, 0, 0, Origin_CT);

			p.Encode1((byte)nType);
			p.Encode1(1); // the time >0 is always 1 minute


			//if (image == null)
			//{
			p.Encode4(0);

			//}
			//mplew.writeInt(image.length);
			//mplew.write(image);

			p.EncodeString(sSnicth);

			p.Encode8(0); //pad

			return p;
		}

		public static COutPacket ClaimResult(ClaimRes nResult)
		{
			var p = new COutPacket(SendOps.LP_ClaimResult);

			if (nResult == ClaimRes.Success_Sender)
				throw new InvalidOperationException();

			p.Encode1((byte)nResult);

			//0x03 | You have been reported by a user.

			//0x41 | Please try again later.
			//0x42 | Please re-check the character name, then try again.
			//0x43 | You do not have enough mesos to report.
			//0x44 | Unable to connect to the server.
			//0x45 | You have exceeded\r\nthe number of reports available.
			//0x47 | You may only report from %d to %d.
			//0x48 | Unable to report due to\n\npreviously being cited for a false report.

			return p;
		}

		public static COutPacket ClaimResult_Sender(bool bCanReportMore, int nRemainReport)
		{
			//Totally wrong param names btw

			var p = new COutPacket(SendOps.LP_ClaimResult);
			p.Encode1((byte)ClaimRes.Success_Sender);

			//bCanReportMore = true
			//  nRemainReport > 0
			//      Your report has been successfully registered.\r\nYou have %d reports left this week.
			//  else
			//      Your report has been successfully registered.\r\nYou have no report left this week.

			//bCanReportMore = false
			//  The report has been successfully made.\r\nYou may not report for the rest of the day.

			p.Encode1(bCanReportMore);
			p.Encode4(nRemainReport);

			return p;
		}

		public static COutPacket SetClaimSvrAvailableTime(byte nClaimSvrOpenTime, byte nClaimSvrCloseTime)
		{
			var p = new COutPacket(SendOps.LP_SetClaimSvrAvailableTime);
			p.Encode1(nClaimSvrOpenTime);
			p.Encode1(nClaimSvrCloseTime);
			return p;
		}

		public static COutPacket ClaimSvrStatusChanged(bool bClaimSvrConnected)
		{
			var p = new COutPacket(SendOps.LP_ClaimSvrStatusChanged);
			p.Encode1(bClaimSvrConnected);
			return p;
		}

		public static COutPacket ForcedStatSet(ForcedStats stat)
		{
			var p = new COutPacket(SendOps.LP_ForcedStatSet);
			stat.Encode(p);
			return p;
		}

		public static COutPacket ForcedStatReset()
		{
			return new COutPacket(SendOps.LP_ForcedStatReset);
		}

		public static COutPacket CashItemExpireMessage(int nItemId)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.CashItemExpireMessage);
			p.Encode4(nItemId);
			return p;
		}

		public static COutPacket GeneralItemExpireMessage(int nItemId)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.GeneralItemExpireMessage);
			p.Encode4(nItemId); // untested
			return p;
		}

		public static COutPacket IncSPMessage(byte nAmount, short nJobId)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncSPMessage);
			p.Encode2(nJobId);
			p.Encode1(nAmount);
			return p;
		}

		public static COutPacket IncPOPMessage(int nAmount)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncPOPMessage);
			p.Encode4(nAmount);
			return p;
		}

		/// <summary>
		/// Chat packet for meso inc/dec in chat (not on side bar).
		/// </summary>
		/// <param name="nAmount"></param>
		/// <returns></returns>
		public static COutPacket IncMoneyMessage(int nAmount)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncMoneyMessage);
			p.Encode4(nAmount);
			p.Encode2(0);
			return p;
		}

		public static COutPacket GiveBuffMessage(int nItemId)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.GiveBuffMessage);
			p.Encode4(nItemId);
			return p;
		}

		public static COutPacket IncGPMessage(int nAmount)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncGPMessage);
			p.Encode4(nAmount);
			return p;
		}

		public static COutPacket SystemMessage(string message)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.SystemMessage);
			p.EncodeString(message);
			return p;
		}

		public static COutPacket IncEXPMessage(int nEXP, int partyBonusAmount)
		{
			//03 
			//01 
			//30 00 00 00 
			//00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00

			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncEXPMessage);

			p.Encode1(1);//sMsg._m_pStr
			p.Encode4(nEXP);
			p.Encode1(false); //bOnQuest
							  //p.Encode4(0); //SelectedMobBonus // bOnQuest == 0 ? (this) : 0
							  //p.Encode1(0); //nMobEventBonusPercentage
							  //p.Skip(1);
							  //p.Encode4(0); //nWeddingBonusEXP


			p.Encode4(0); // bonus event
			p.Encode1(0);  // bonus exp for 3rd mob killed notice
			p.Encode1(0);  // ??
			p.Encode4(0); //nWeddingBonusEXP
			p.Encode1(0);  // party bonus event rate
			p.Encode4(partyBonusAmount); // party bonus
			p.Encode4(0); // equip item bonus
			p.Encode4(0); // internet cafe bonus 
			p.Encode4(0); // rainbow week bonus
			p.Encode4(0); // party ring bonus
			p.Encode4(0); // cake vs pie


			//if (v6 <= 0)
			//    nPlayTimeHour = 0;
			//else
			//    nPlayTimeHour = (unsigned __int8)CInPacket::Decode1(v2);

			//if (false) // nMobEventBonusPercentage <= 0
			//{
			//    p.Encode1(0); //nPlayTimeHour
			//}

			//if (false)  //bOnQuest
			//{
			//    p.Encode1(0); //nQuestBonusRemainCount i think

			//    if(false) // nQuestBonusRemainCount > 0
			//    {
			//        p.Encode2(0); //nQuestBonusRemainCount = (unsigned __int8)CInPacket::Decode1(v2);
			//    }
			//}

			//p.Skip(25);
			//nPartyBonusEventRate = (unsigned __int8)CInPacket::Decode1(v2);
			//nPartyBonusExp = CInPacket::Decode4(v2);
			//nItemBonusEXP = CInPacket::Decode4(v2);
			//nPremiumIPExp = CInPacket::Decode4(v2);
			//nRainbowWeekEventEXP = CInPacket::Decode4(v2);
			//nPartyEXPRingEXP = CInPacket::Decode4(v2);
			//nCakePieEventBonus = CInPacket::Decode4(v2);

			return p;
		}

		public static COutPacket DropPickUpMessage_Item(int nItemId, int nQuantity, bool bInChat)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.DropPickUpMessage);
			p.Encode1((byte)(bInChat ? 2 : 0));
			p.Encode4(nItemId);
			p.Encode4(nQuantity);
			return p;
		}

		public static COutPacket DropPickUpMessage_Meso(int nMoney)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.DropPickUpMessage);
			p.Encode1(1);
			p.Encode1(0); // (bool) -- A portion was not found after falling on the ground.
			p.Encode4(nMoney);
			p.Skip(2); // Internet Cafe Meso Bonus (+%d)
			return p;
		}

		public static COutPacket OnIncGPMessage(int nAmount)
		{
			var p = new COutPacket(SendOps.LP_Message);
			p.Encode1((byte)MessageType.IncGPMessage);
			p.Encode4(nAmount);
			return p;
		}

		public static COutPacket MonsterBookSetCard_Success(int nCardID, int nCardCount)
		{
			var p = new COutPacket(SendOps.LP_MonsterBookSetCard);
			p.Encode1(true);
			p.Encode4(nCardID);
			p.Encode4(nCardCount);
			return p;
		}

		public static COutPacket MonsterBookSetCard_Failed()
		{
			//Adds to the chatlog: "This card is already full in the Monster Book. This card will disappear."
			var p = new COutPacket(SendOps.LP_MonsterBookSetCard);
			p.Encode1(false);
			return p;
		}

		public static COutPacket MonsterBookSetCover(int nCardID)
		{
			var p = new COutPacket(SendOps.LP_MonsterBookSetCover);
			p.Encode4(nCardID);
			return p;
		}

		public static COutPacket UserAskAPSPEvent(int dwCharacterId)
		{
			var p = new COutPacket(SendOps.LP_UserAskAPSPEvent);
			p.Encode4(dwCharacterId);
			p.Encode4(13); //Idk
			return p;
		}

		//WvsGame::UserPool--------------------------------------------------------------------------------------------

		public static COutPacket UserEnterField(Character c)
		{
			// CUserPool::OnUserEnterField(CUserPool *this, CInPacket *iPacket)
			var p = new COutPacket(SendOps.LP_UserEnterField);
			p.Encode4(c.dwId);

			// CUserRemote::Init(v12, iPacket, v13, 1);
			p.Encode1(c.Stats.nLevel);
			p.EncodeString(c.Stats.sCharacterName);

			var guild = MasterManager.GuildManager.GetGuild(c);

			if (guild is null)
			{
				p.Skip(8);
			}
			else
			{
				p.EncodeString(guild.GuildName);
				p.Encode2(guild.GuildMarkBg);
				p.Encode1(guild.GuildMarkBgColor);
				p.Encode2(guild.GuildMark);
				p.Encode1(guild.GuildMarkColor);
			}

			c.Buffs.GetSecondaryStatCollection()
				.EncodeForRemote(p);

			p.Encode2(c.Stats.nJob); //v4->m_nJobCode;
			c.GetLook().Encode(p); //AvatarLook::AvatarLook(&v87, iPacket);

			p.Encode4(0); //  v4->m_dwDriverID
			p.Encode4(0); //  v4->m_dwPassenserID
			p.Encode4(0); //  nChocoCount
			p.Encode4(c.nActiveEffectItemID); //  nActiveEffectItemID
			p.Encode4(c.nCompletedSetItemID); //  v4->m_nCompletedSetItemID
			p.Encode4(c.nActivePortableChairID); //  v4->m_nPortableChairID

			c.Position.Encode(p);

			p.Encode1(0); //bShowAdminEffect

			for (var i = 0; i < 3; i++)
			{
				if (c.Pets.Pets[i] is CPet pet)
				{
					p.Encode1(1);

					pet.Encode(p);
				}
			}

			p.Encode1(0);

			c.TamingMob.Encode(p);

			p.Encode1(0); // todo theres a whole decode function for this miniroom stuff

			// p.Encode1((byte)(c.CurMiniRoom?.nMiniRoomType ?? 0)); // m_nMiniRoomType (Flag)

			p.Encode1(c.sADBoard.Length > 0);
			if (c.sADBoard.Length > 0)
			{
				p.EncodeString(c.sADBoard);
			}

			c.RingInfo.EncodeEquippedRings(p);

			byte nActiveUserEffect = 0;

			if (c.Skills.bDarkForce) nActiveUserEffect |= 1;
			if (c.Skills.nDragonFury != 0) nActiveUserEffect |= 2;
			if (c.m_dwSwallowMobID > 0) nActiveUserEffect |= 4;

			p.Encode1(nActiveUserEffect);

			p.Encode1(0); //CUserPool::OnNewYearCardRecordAdd loop
			p.Encode4(0); //m_nPhase
			return p;
		}
		public static COutPacket UserLeaveField(Character c)
		{
			var p = new COutPacket(SendOps.LP_UserLeaveField);
			p.Encode4(c.dwId);
			return p;
		}

		public static COutPacket UserChat(int dwUserId, string szText, bool bAdmin, bool bOnlyBalloon)
		{
			var p = new COutPacket(SendOps.LP_UserChat);
			p.Encode4(dwUserId);
			p.Encode1(bAdmin);
			p.EncodeString(szText);
			p.Encode1(bOnlyBalloon);
			return p;
		}

		public static COutPacket UserSetActivePortableChair(int uid, int dwTemplateId)
		{
			var p = new COutPacket(SendOps.LP_UserSetActivePortableChair);
			p.Encode4(uid);
			p.Encode4(dwTemplateId);
			return p;
		}

		public static COutPacket UserSitResult(short nSeatId)
		{
			var p = new COutPacket(SendOps.LP_UserSitResult);

			if (nSeatId == -1)
			{
				p.Encode1(0);
			}
			else
			{
				p.Encode1(1);
				p.Encode2(nSeatId);
			}

			return p;
		}

		public static COutPacket SetPassengerRequest(int dwCharacterID)
		{
			var p = new COutPacket(SendOps.LP_SetPassenserRequest);
			p.Encode4(dwCharacterID);
			return p;
		}

		public static COutPacket UserFollowCharacter(int dwCharacterID)
		{
			var p = new COutPacket(SendOps.LP_UserFollowCharacter);
			p.Encode4(dwCharacterID);
			p.Encode4(0); //dwDriverID
			return p;
		}

		public static COutPacket UserFollowCharacter(int dwCharacterID, int dwDriverID, byte bTransferField, int nX, int nY)
		{
			var p = new COutPacket(SendOps.LP_UserFollowCharacter);
			p.Encode4(dwCharacterID);
			p.Encode4(dwDriverID);
			p.Encode1(bTransferField);
			p.Encode4(nX);
			p.Encode4(nY);
			return p;
		}

		//WvsGame::DropPool--------------------------------------------------------------------------------------------
		public static COutPacket DropEnterField(CDrop drop)
		{
			var p = new COutPacket(SendOps.LP_DropEnterField);
			p.Encode1((byte)drop.nEnterType); // Sub OpCode
			p.Encode4(drop.dwId);
			p.Encode1(drop.bIsMoney); // 1 mesos, 0 item, 2 and above all item meso bag,
			p.Encode4(drop.ItemId); // drop Item ID

			p.Encode4(drop.DropOwnerID); //v6 // Owner of drop Id
			p.Encode1((byte)drop.DropOwnType); //v4->m_nOwnType // Drop Type

			drop.Position.EncodePos(p); // end point

			p.Encode4(drop.DropOwnType == DropOwnType.UserOwn ? drop.DropOwnerID : 0); // Owned picked, OwnerId = only owner pick up, 0 = anyone pick up

			if (drop.nEnterType != DropEnterType.OnFoothold)
			{
				p.EncodePos(drop.StartPosX, drop.StartPosY); // starting point
				p.Encode2(drop.tDelay);
			}

			if (drop.bIsMoney == 0)
			{
				p.EncodeDateTime(drop.SpawnTime.AddSeconds(drop.DropExpirySeconds));
			}

			p.Encode1(drop.OwnerCharId == 0); // enables pets to pick the drop up
			p.Encode1(0); //If true triggers | IWzGr2DLayer::Putz(v84, 0xC0041F15);

			return p;
		}
		public static COutPacket DropLeaveField(CDrop drop)
		{
			var p = new COutPacket(SendOps.LP_DropLeaveField);
			p.Encode1((byte)drop.nLeaveType);
			p.Encode4(drop.dwId); // dwDropID

			switch (drop.nLeaveType)
			{
				case DropLeaveType.UserPickup:
				case DropLeaveType.MobPickup:
					p.Encode4(drop.OwnerCharId); // picker up char id or zero
					break;
				case DropLeaveType.PetPickup:
					p.Encode4(drop.OwnerCharId); // picker up char id or zero
					p.Encode4(drop.PetIdx);
					break;
				case DropLeaveType.Explode:
					p.Encode2(0); // pr.p->tLeaveTime = get_update_time() + CInPacket::Decode2(iPacket);
					return p;
			}

			return p;
		}

		//WvsGame::NpcPool--------------------------------------------------------------------------------------------

		public static COutPacket ImitatedNPCData(CNpcImitate[] npcs)
		{
			var p = new COutPacket(SendOps.LP_ImitatedNPCData);

			p.Encode1((byte)npcs.Length);

			foreach (var npc in npcs)
			{
				npc.EncodeImitateData(p);
			}
			return p;
		}

		public static COutPacket NpcEnterField(CNpc npc)
		{
			var p = new COutPacket(SendOps.LP_NpcEnterField);
			p.Encode4(npc.dwId);
			p.Encode4(npc.TemplateId);

			//CNpc::Init
			npc.EncodeInitData(p);

			return p;
		}

		public static COutPacket NpcLeaveField(CNpc npc)
		{
			var p = new COutPacket(SendOps.LP_NpcLeaveField);
			p.Encode4(npc.dwId);
			return p;
		}

		public static COutPacket NpcChangeController(CNpc npc, byte nLevel)
		{
			var p = new COutPacket(SendOps.LP_NpcChangeController);
			p.Encode1(nLevel); // 1 = remove i think
			p.Encode4(npc.dwId);

			if (nLevel > 0)
			{
				p.Encode4(npc.TemplateId);
				npc.EncodeInitData(p);
			}

			return p;
		}

		public static COutPacket NpcMove(CNpc npc, byte nAct1, byte nAct2, byte[] aBuf)
		{
			var p = new COutPacket(SendOps.LP_NpcMove);
			p.Encode4(npc.dwId);
			p.Encode1(nAct1);
			p.Encode1(nAct2);

			if (aBuf?.Length > 0)
				p.EncodeBuffer(aBuf, 0, aBuf.Length);

			return p;
		}

		public static COutPacket SetNpcScript(int dwNpcTemplateId, string sDesc, int tStart = 0, int tEnd = int.MaxValue)
		{
			var p = new COutPacket(SendOps.LP_NpcSetScript);
			p.Encode1(1); //Loop ItemCount

			// MODSCRIPT::Decode
			p.Encode4(dwNpcTemplateId);

			// SCRIPTINFO::Decode

			p.EncodeString(sDesc); //If the NPC has quests, this will be the text of the menu item
			p.Encode4(tStart);  //Zero
			p.Encode4(tEnd);    //Max Int!

			return p;
		}

		//--------------------------------------------------------------------------------------------

		public static COutPacket BroadcastPinkNotice(string msg)
		{
			return BroadcastMsg(5, msg);
		}
		public static COutPacket BroadcastServerMsg(string msg)
		{
			return BroadcastMsg(4, msg);
		}
		public static COutPacket BroadcastPopupMsg(string msg)
		{
			return BroadcastMsg(1, msg);
		}
		private static COutPacket BroadcastMsg(byte nType, string message)
		{
			var p = new COutPacket(SendOps.LP_BroadcastMsg);
			p.Encode1(nType);

			// 0: [Notice] <Msg>
			// 1: Popup <Msg>
			// 2: Megaphone
			// 3: Super Megaphone 
			// 4: Server Message
			// 5: Pink Text
			// 6: LightBlue Text ({} as Item)
			// 7: [int] -> Keep Wz Error
			// 8: Item Megaphone
			// 9: Item Megaphone
			// 10: Three Line Megaphone
			// 11: Weather Effect
			// 12: Green Gachapon Message
			// 13: Yellow Twin Dragon's Egg
			// 14: Green Twin Dragon's Egg
			// 15: Lightblue Text
			// 16: Lightblue Text
			// 18: LightBlue Text ({} as Item)
			// 20: (Red Message) : Skull?

			if (nType == 4)
			{
				p.Encode1(true); // Server Message
			}

			p.EncodeString(message);

			//switch (nType)
			//{
			//    case 3: // Super Megaphone
			//    case 20: // Skull Megaphone
			//        mplew.write(channel - 1);
			//        mplew.write(whisper ? 1 : 0);
			//        break;
			//    case 9: // Like Item Megaphone (Without Item)
			//        mplew.write(channel - 1);
			//        break;
			//    case 11: // Weather Effect
			//        mplew.writeInt(channel); // item id
			//        break;
			//    case 13: // Yellow Twin Dragon's Egg
			//    case 14: // Green Twin Dragon's Egg
			//        mplew.writeMapleAsciiString("NULL"); // Name
			//        PacketHelper.addItemInfo(mplew, null, true, true);
			//        break;
			//    case 6:
			//    case 18:
			//        mplew.writeInt(channel >= 1000000 && channel < 6000000 ? channel : 0); // Item Id
			//        //E.G. All new EXP coupon {Ruby EXP Coupon} is now available in the Cash Shop!
			//        break;
			//}

			return p;
		}

		public static COutPacket UserNoticeMsg(string sMessage)
		{
			var p = new COutPacket(SendOps.LP_UserNoticeMsg);
			p.EncodeString(sMessage);
			return p;
		}

		public static COutPacket TradeMoneyLimit(bool ShowDlg)
		{
			var p = new COutPacket(SendOps.LP_TradeMoneyLimit);
			p.Encode1(!ShowDlg);
			return p;
		}

		public static COutPacket Desc(bool bEnable)
		{
			var p = new COutPacket(SendOps.LP_Desc);
			p.Encode1(bEnable);
			return p;
		}

		public static COutPacket StalkResult(int dwCharacterID, string sName, int nX, int nY)
		{
			var p = new COutPacket(SendOps.LP_StalkResult);
			p.Encode4(1); //Loop to be honest

			p.Encode4(dwCharacterID);
			p.EncodeString(sName);
			p.Encode4(nX);
			p.Encode4(nY);

			return p;
		}

		public static COutPacket PlayJukeBox(string sName)
		{
			var p = new COutPacket(SendOps.LP_PlayJukeBox);
			p.Encode4(5100000); //Congratulatory Song
			p.EncodeString(sName);
			return p;
		}

		public static COutPacket BlowWeather(byte nBlowType, int nItemID, string sMsg)
		{
			var p = new COutPacket(SendOps.LP_BlowWeather);
			//BlowType != 0 | No message
			p.Encode1(0); //BlowWeather_User = 0 | BlowWeather_Admin = 1
			p.Encode4(nItemID); //5120000 | Snowy Snow
			p.EncodeString(sMsg);
			return p;
		}

		public static COutPacket WarnMessage(string msg)
		{
			var p = new COutPacket(SendOps.LP_Warn);
			p.EncodeString(msg);
			return p;
		}

		public static COutPacket AdminShopCommodity(int npcId)
		{
			var p = new COutPacket(SendOps.LP_AdminShopCommodity);
			p.Encode4(npcId);
			p.Encode2(0);
			return p;
		}


		public static COutPacket UserOpenUI(UIWindow nUIType)
		{
			var p = new COutPacket(SendOps.LP_UserOpenUI);
			p.Encode1((byte)nUIType);
			return p;
		}
		public static COutPacket UserOpenUIWithOption(UIWindow nUIType, int nDefaultTab)
		{
			var p = new COutPacket(SendOps.LP_UserOpenUIWithOption);
			p.Encode4((int)nUIType);
			p.Encode4(nDefaultTab);
			return p;
		}

		public static COutPacket FieldObstacleOnOff(string sName, int nState)
		{
			var p = new COutPacket(SendOps.LP_FieldObstacleOnOff);
			p.EncodeString(sName);
			p.Encode4(nState);
			return p;
		}
		public static COutPacket FieldObstacleOnOffStatus(string sName, int nState)
		{
			var p = new COutPacket(SendOps.LP_FieldObstacleOnOffStatus);
			p.Encode4(1); //ItemCount
			p.EncodeString(sName);
			p.Encode4(nState);
			return p;
		}
		public static COutPacket FieldObstacleAllReset()
		{
			return new COutPacket(SendOps.LP_FieldObstacleAllReset);
		}


		public static COutPacket SetDirectionMode(bool bSet, int tDelay)
		{
			var p = new COutPacket(SendOps.LP_SetDirectionMode);
			p.Encode1(bSet);
			p.Encode4(tDelay);
			return p;
		}
		public static COutPacket StandAloneMode(bool bLock)
		{
			var p = new COutPacket(SendOps.LP_SetStandAloneMode);
			p.Encode1(bLock);
			return p;
		}
		public static COutPacket UserHireTutor(bool bSpawn)
		{
			var p = new COutPacket(SendOps.LP_UserHireTutor);
			p.Encode1(bSpawn);
			return p;
		}
		public static COutPacket UserTutorMsg(int nIdx, int nDuration)
		{
			var p = new COutPacket(SendOps.LP_UserTutorMsg);
			p.Encode1(true);
			p.Encode4(nIdx);
			p.Encode4(nDuration);
			return p;
		}
		public static COutPacket UserTutorMsg(string sMsg, int nWidth, int nDuration)
		{
			var p = new COutPacket(SendOps.LP_UserTutorMsg);
			p.Encode1(false);
			p.EncodeString(sMsg);
			p.Encode4(nWidth);
			p.Encode4(nDuration);
			return p;
		}

		public static COutPacket IncCombo(int nCombo)
		{
			var p = new COutPacket(SendOps.LP_IncCombo);
			p.Encode4(nCombo);
			return p;
		}
		public static COutPacket UserRandomEmotion(int nItemID)
		{
			var p = new COutPacket(SendOps.LP_UserRandomEmotion);
			p.Encode4(nItemID);
			return p;
		}
		public static COutPacket UserOpenSkillGuide()
		{
			var p = new COutPacket(SendOps.LP_UserOpenSkillGuide);
			return p;
		}

		public static COutPacket UserGoToCommoditySN(int nUnk)
		{
			var p = new COutPacket(SendOps.LP_UserGoToCommoditySN);
			p.Encode4(nUnk);
			return p;
		}
		public static COutPacket UserDamageMeter(int nDuration)
		{
			var p = new COutPacket(SendOps.LP_UserDamageMeter);
			p.Encode4(nDuration);
			return p;
		}

		//void __thiscall CUserLocal::OnFollowCharacterFailed(CUserLocal *this, CInPacket *iPacket)

		//BowMasterSkill
		public static COutPacket UserRequestVengeance(int nSkillID = 3120010)
		{
			var p = new COutPacket(SendOps.LP_UserRequestVengeance);
			p.Encode4(nSkillID);
			return p;
		}
		public static COutPacket UserRequestExJablin()
		{
			var p = new COutPacket(SendOps.LP_UserRequestExJablin);
			return p;
		}
		public static COutPacket UserDeliveryQuest(int nItemPos, int nItemID)
		{
			var p = new COutPacket(SendOps.LP_UserDeliveryQuest);
			p.Encode4(nItemPos);
			p.Encode4(nItemID);

			return p;
		}

		//public static COutPacket UserAskAPSPEvent()
		//{
		//    var p = new COutPacket(SendOps.LP_UserAskAPSPEvent);
		//    return p;
		//}

		//BuffZoneATtack

		public static COutPacket UserADBoard(int cid, string msg = null)
		{
			var p = new COutPacket(SendOps.LP_UserADBoard);
			p.Encode4(cid);

			var bOpen = msg != null;

			p.Encode1(bOpen);

			if (bOpen)
				p.EncodeString(msg);

			return p;
		}

		public static COutPacket CheckCrcResult(bool bSuccess)
		{
			//Makes MapleStory client throw CMSException 0x2200000A
			var p = new COutPacket(SendOps.LP_CheckCrcResult);
			p.Encode1(bSuccess);
			return p;
		}

		public static COutPacket DataCRCCheckFailed(string sMessage)
		{
			//Makes a GM police message appear
			var p = new COutPacket(SendOps.LP_DataCRCCheckFailed);
			p.EncodeString(sMessage);
			return p;
		}

		public static COutPacket UpdateGMBoard(int nWebOpBoardIndex, string sWebOpBoardURL)
		{
			var p = new COutPacket(SendOps.LP_UpdateGMBoard);
			p.Encode4(nWebOpBoardIndex);
			p.EncodeString(sWebOpBoardURL);
			return p;
		}



		public static COutPacket TransferChannel(int tRemainTime, bool bShowUI, bool bCloseUI, bool bAbleToSummon, int nOrb)
		{
			//Last int is a conditional check CWvsContext::OnDragonBallBox
			var p = new COutPacket(SendOps.LP_TransferChannel);
			p.Encode4(tRemainTime);
			p.Encode1(bShowUI);
			p.Encode1(bCloseUI);
			p.Encode1(bAbleToSummon);
			p.Encode4(nOrb);

			return p;
		}

		public static COutPacket AskUserWhetherUsePamsSong()
		{
			var p = new COutPacket(SendOps.LP_AskUserWhetherUsePamsSong);
			return p;
		}

		public static COutPacket TransferChannel(int nTargetChannel, string sMessage)
		{
			var p = new COutPacket(SendOps.LP_TransferChannel);
			p.Encode4(nTargetChannel);
			p.EncodeString(sMessage);
			return p;
		}


		public static COutPacket MacroSysDataInit(CharacterMacros macros)
		{
			var p = new COutPacket(SendOps.LP_MacroSysDataInit);
			macros.Encode(p);
			return p;
		}

		public static COutPacket SetBuyEquipExt(bool bBuyEquipExt)
		{
			var p = new COutPacket(SendOps.LP_SetBuyEquipExt);
			p.Encode1(bBuyEquipExt);
			return p;
		}

		//WvsCommon---------------------------------------------------------------------------------------------------

		private static void CClientOptMan__EncodeOpt(COutPacket p, short optCount)
		{
			p.Encode2(optCount); // if we ever do this we need to xref CClientOptMan::GetOpt and add the decode/encodes for it

			for (int i = 0; i < optCount; i++)
			{
				p.Encode8(i + 1);
				//dwType = CInPacket::Decode4(v3);
				//iPacket = (CInPacket*)CInPacket::Decode4(v3);
			}
		}

		public static COutPacket LogoutGift() => new COutPacket(SendOps.LP_LogoutGift);

		public static COutPacket HontaleTimer(byte nOption, byte nVal)
		{
			//0 - The Horntail Expedition will end if it does not start within % d min(s) of entering.
			//1 - The Horntail Expedition will end in %d min(s).
			//2 - The Horntail Expedition has ended.

			var p = new COutPacket(SendOps.LP_HontaleTimer);
			p.Encode1(nOption);
			p.Encode1(nVal);
			return p;
		}

		public static COutPacket HontailTimer(byte nOption, int nVal)
		{
			//nOption
			//0 - The Horntail's Cave will close if you do not summon Horntail in %d minutes.
			//1 - The Horntail's Cave will close in %d minutes.

			//nVal
			//0 - The Horntail's Cave has closed.
			//X - %d for nOption

			var p = new COutPacket(SendOps.LP_HontailTimer);
			p.Encode1(nOption);
			p.Encode4(nVal);
			return p;
		}

		public static COutPacket ZakumTimer(byte nOption, int nVal)
		{
			//nOption
			//0 - The Zakum Shrine will close if you do not summon Zakum in %d minutes.
			//1 - The Zakum Shrine will close in %d minutes.

			//nVal
			//0 - The Zakum Shrine has closed.
			//X - %d for nOption

			var p = new COutPacket(SendOps.LP_ZakumTimer);
			p.Encode1(nOption);
			p.Encode4(nVal);
			return p;
		}
		public static COutPacket ChaosZakumTimer(byte nOption, int nVal)
		{
			//nOption
			//0 - The Zakum Shrine will close if you do not summon Zakum in %d minutes.
			//1 - The Zakum Shrine will close in %d minutes.

			//nVal
			//0 - The Zakum Shrine has closed.
			//X - %d for nOption

			var p = new COutPacket(SendOps.LP_ChaosZakumTimer);
			p.Encode1(nOption);
			p.Encode4(nVal);
			return p;
		}

		public static COutPacket StageChange(string sStageTheme, byte nStartPeriod)
		{
			var p = new COutPacket(SendOps.LP_StageChange);
			p.EncodeString(sStageTheme);
			p.Encode1(nStartPeriod);
			return p;
		}

		//CBattleRecordMan::OnPacket
		public static COutPacket BattleRecordDotDamageInfo(int nCurDamage)
		{
			//   CBattleRecordMan::OnDotDamageInfo(CBattleRecordMan *this, CInPacket *iPacket)

			var p = new COutPacket(SendOps.LP_BattleRecordDotDamageInfo);
			p.Encode4(nCurDamage);
			//TODO : COMP[LETE THIS
			return p;
		}
		public static COutPacket BattleRecordRequestResult(bool bServerOnCalc)
		{
			//   CBattleRecordMan::OnServerOnCalcRequestResult(this, iPacket);

			var p = new COutPacket(SendOps.LP_BattleRecordRequestResult);
			p.Encode1(bServerOnCalc);
			return p;
		}

		//CMapleTVMan::OnPacket
		public static COutPacket MapleTVUpdateMessage(byte nMsgType, string[] asMsg, int nTotalWaitTime, Character aSender, Character aRemote)
		{
			var p = new COutPacket(SendOps.LP_MapleTVUpdateMessage);

			byte nFlag = 1;

			if (aRemote != null)
				nFlag |= 2;

			p.Encode1(nFlag);
			p.Encode1(nMsgType);

			aSender.GetLook().Encode(p);
			p.EncodeString(aSender.Stats.sCharacterName);

			if (aRemote == null)
				p.Encode2(0);
			else
				p.EncodeString(aRemote.Stats.sCharacterName);

			for (int i = 0; i < 5; i++)
				p.EncodeString(asMsg[i]);

			p.Encode4(nTotalWaitTime);

			if ((nFlag & 2) != 0)
			{
				aRemote.GetLook().Encode(p);
			}

			return p;
		}
		public static COutPacket MapleTVClearMessage()
		{
			return new COutPacket(SendOps.LP_MapleTVClearMessage);
		}

		public static COutPacket MapleTVSendMessageResult(byte nRes, byte nOpt2)
		{
			var p = new COutPacket(SendOps.LP_MapleTVSendMessageResult);
			p.Encode4(nRes);
			p.Encode1(nOpt2);

			return p;
		}

		//CMapLoadable::OnPacket
		public static COutPacket SetBackgroundEffect(/*BACKEFFECT*/ byte bEffect, int nFieldID, byte nPageID, int tDuration)
		{
			var p = new COutPacket(SendOps.LP_SetBackgroundEffect);
			p.Encode1(bEffect);
			p.Encode4(nFieldID);
			p.Encode1(nPageID);
			p.Encode4(tDuration);

			return p;
		}

		public static COutPacket SetMapObjectVisible(string sTag, bool bVisible)
		{
			var p = new COutPacket(SendOps.LP_SetMapObjectVisible);
			p.Encode1(1); //Loop count to be honest i got lazy
			p.EncodeString(sTag);
			p.Encode1(bVisible);

			return p;
		}

		public static COutPacket ClearBackgroundEffect()
		{
			return new COutPacket(SendOps.LP_ClearBackgroundEffect);
		}

		//CTownPortalPool::OnPacket
		public static COutPacket TownPortalCreated(CTownPortal town, byte nEnterType)
		{
			var p = new COutPacket(SendOps.LP_TownPortalCreated);
			p.Encode1(nEnterType);
			p.Encode4(town.dwCharacterID);
			town.Position.EncodePos(p);
			return p;
		}
		public static COutPacket TownPortalRemoved(int dwCharacterID, byte nLeaveType)
		{
			var p = new COutPacket(SendOps.LP_TownPortalRemoved);
			p.Encode1(nLeaveType);
			p.Encode4(dwCharacterID);
			return p;
		}

		//CMessageBoxPool::OnPacket

		public static COutPacket CreateMessageBoxFailed()
		{
			return new COutPacket(SendOps.LP_CreateMessgaeBoxFailed);
		}

		public static COutPacket MessageBoxEnterField(CMessageBox box)
		{
			var p = new COutPacket(SendOps.LP_MessageBoxEnterField);
			p.Encode4(box.dwId);
			p.Encode4(box.nItemID);
			p.EncodeString(box.sMessage);
			p.EncodeString(box.sCharacterName);
			box.Position.EncodePos(p);
			return p;
		}

		public static COutPacket MessageBoxLeaveField(int dwId, bool bFadeOut)
		{
			var p = new COutPacket(SendOps.LP_MessageBoxLeaveField);
			p.Encode1(!bFadeOut);
			p.Encode4(dwId);
			return p;
		}

		public static COutPacket OpenClassCompetitionPage()
		{
			var p = new COutPacket(SendOps.LP_UserOpenClassCompetitionPage);
			return p;
		}

		public static COutPacket TutorMessage(string sHintMessage)
		{
			var p = new COutPacket(SendOps.LP_UserTutorMsg);

			p.Encode1(0);
			p.EncodeString(sHintMessage);
			var byteBuffer = new byte[] { 0xC8, 0, 0, 0, 0xA0, 0x0F, 0, 0 };
			p.EncodeBuffer(byteBuffer, 0, byteBuffer.Length);
			return p;
		}

		public static COutPacket TutorMessage(int nHintMessage)
		{
			var p = new COutPacket(SendOps.LP_UserTutorMsg);
			p.Encode1(1);
			p.Encode4(nHintMessage);
			p.Encode4(7000);
			return p;
		}

		/**
         * Sends box-hint to player.
         */
		public static COutPacket BalloonMessage(string sHint, short nWidth)
		{
			if (nWidth <= 0)
				nWidth = (short)(sHint.Length * 10);

			var p = new COutPacket(SendOps.LP_UserBalloonMsg);

			p.EncodeString(sHint);
			p.Encode2(nWidth);
			p.Encode2(5); //Duration time Seconds

			const bool bDefaultPos = true;

			p.Encode1(bDefaultPos);

			if (bDefaultPos == false)
			{
				p.Encode4(0); //nX
				p.Encode4(0); //nY
			}

			return p;
		}

		public static COutPacket TutorialEffect(int number)
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			p.Encode1((byte)UserEffect.AvatarOriented);
			p.EncodeString("UI/tutorial.img/" + number);
			p.Encode4(1);
			return p;
		}

		/**
         * earnTitleMessage in odin
         */
		public static COutPacket ScriptProgressMessage(string message)
		{
			var p = new COutPacket(SendOps.LP_ScriptProgressMessage);
			p.EncodeString(message);
			return p;
		}

		/**
         * ShowIntro in odin
         */
		public static COutPacket ShowReservedEffect(string path)
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			p.Encode1((byte)UserEffect.ReservedEffect);
			p.EncodeString(path);
			return p;
		}

		/**
         * Type:
         *      0 - Light and Long
         *      1 - Heavy and Short
         *  Delay:
         *      Time in seconds
         */
		public static COutPacket ShowTrembleEffect(byte type, int delay)
		{
			var p = new COutPacket(SendOps.LP_UserEffectRemote);
			p.Encode1(1); // opcode
			p.Encode1(type);
			p.Encode4(delay);
			return p;
		}

		public static COutPacket ShowMapEffect(string path)
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			p.Encode1(3); // opcode
			p.EncodeString(path);
			return p;
		}

		public static COutPacket PlayMapSound(string path)
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			p.Encode1(4); // opcode
			p.EncodeString(path);
			return p;
		}

		public static COutPacket ShowBossHP(int mapObjectID, int currentHP, int maxHP, byte tagColor, byte tagBgColor)
		{
			var p = new COutPacket(SendOps.LP_UserEffectRemote);
			p.Encode1(5); // opcode
			p.Encode4(mapObjectID);
			p.Encode4(currentHP);
			p.Encode4(maxHP);
			p.Encode1(tagColor);
			p.Encode1(tagBgColor);
			return p;
		}

		public static COutPacket ToggleUIDisable(bool disabled)
		{
			var p = new COutPacket(SendOps.LP_SetStandAloneMode);
			p.Encode1((byte)(disabled ? 0 : 1)); // opcode
			return p;
		}

		public static COutPacket ToggleUILock(bool locked)
		{
			var p = new COutPacket(SendOps.LP_SetDirectionMode);
			p.Encode1((byte)(locked ? 0 : 1)); // opcode
			p.Encode4(0);
			return p;
		}

		public static COutPacket RandomMesoBagSucceeded(byte nRank, int nAmount)
		{
			var p = new COutPacket(SendOps.LP_Random_Mesobag_Succeed);
			p.Encode1(nRank);
			p.Encode4(nAmount);
			return p;
		}

		public static COutPacket AvatarMegaphoneRes(AvatarMegaphoneResCode nResCode)
		{
			var p = new COutPacket(SendOps.LP_AvatarMegaphoneRes);
			p.Encode1((byte)nResCode);
			return p;
		}

		public static COutPacket AvatarMegaphoneRes(string sMsg)
		{
			var p = new COutPacket(SendOps.LP_AvatarMegaphoneRes);
			p.Encode1(100); // anything above 97 does the same thing
			p.EncodeString(sMsg);
			return p;
		}

		public static COutPacket SetAvatarMegaphone(AvatarMegaphone item)
		{
			var p = new COutPacket(SendOps.LP_AvatarMegaphoneUpdateMessage);
			item.Encode(p);
			return p;
		}

		public static COutPacket ClearAvatarMegapone()
		{
			var p = new COutPacket(SendOps.LP_AvatarMegaphoneClearMessage);
			return p;
		}

		public static COutPacket CreateClock(int tDurationSeconds)
		{
			var p = new COutPacket(SendOps.LP_Clock);
			p.Encode1(2); // type
			p.Encode4(tDurationSeconds);
			return p;
		}

		public static COutPacket DestroyClock()
		{
			return new COutPacket(SendOps.LP_DestroyClock);
		}

		public static COutPacket SkillLearnItemResult(int dwUserID, bool bIsMasterbook, bool bUsed, bool bSuccess)
		{
			var p = new COutPacket(SendOps.LP_SkillLearnItemResult);
			p.Encode1(false); // bOnExclRequest
			p.Encode4(dwUserID);
			p.Encode1(bIsMasterbook); // bIsMaterbook -> Mastery Book true, Skill Book false
			p.Encode4(0);// nSkillID);
			p.Encode4(0);// nSLV);
			p.Encode1(bUsed); // bUsed
			p.Encode1(bSuccess); // bSucceed
			return p;
		}

		public static COutPacket SetActiveEffectItem(int dwCharId, int nItemID)
		{
			var p = new COutPacket(SendOps.LP_UserSetActiveEffectItem);
			p.Encode4(dwCharId);
			p.Encode4(nItemID);
			return p;
		}
	}
}