using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Common.Types;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CGuildMan
		{
			/// <summary>
			/// Helper function for encoding guild op header
			/// </summary>
			/// <param name="p"></param>
			/// <param name="nType"></param>
			private static void EncodeHeader(COutPacket p, GuildOps nType)
				=> p.Encode1((byte)nType); // DONE

			/// <summary>
			/// Helper function for generating guild packets that only contain a guild op byte.
			/// </summary>
			/// <param name="nType"></param>
			/// <returns></returns>
			private static COutPacket EmptyGuildPacket(GuildOps nType)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, nType);
				return p;
			} // DONE

			/// <summary>
			/// Generic error packet
			/// </summary>
			/// <returns></returns>
			public static COutPacket GuildRes_Error(GuildOps nType)
				=> EmptyGuildPacket(nType); // DONE

			/// <summary>
			/// Initialize guild NPC to prompt for guild name
			/// </summary>
			/// <returns></returns>
			public static COutPacket InputGuildName()
				=> EmptyGuildPacket(GuildOps.GuildReq_InputGuildName); // DONE

			/// <summary>
			/// Request packet for selecting a guild name.
			/// Sent after the party leader has chosen a name and the party will decide if they agree to the guild name.
			/// </summary>
			/// <returns></returns>
			public static COutPacket CreateGuildAgree(int nPartyID, string sMasterName, string sGuildName)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildReq_CreateGuildAgree);
				p.Encode4(nPartyID);
				p.EncodeString(sMasterName);
				p.EncodeString(sGuildName);
				return p;
			} // DONE

			/// <summary>
			/// Invite packet sent to the invitee
			/// </summary>
			/// <returns></returns>
			public static COutPacket InviteGuild(int dwInviterID, string sInviter, int nLevel, int nJobCode)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildReq_InviteGuild);
				p.Encode4(dwInviterID);
				p.EncodeString(sInviter);
				p.Encode4(nLevel);
				p.Encode4(nJobCode);
				return p;
			} // DONE

			/// <summary>
			/// Initialize guild NPC to prompt changing the guild mark
			/// Note: must be a guild master in order for it to work
			/// </summary>
			/// <returns></returns>
			public static COutPacket InputMark()
				=> EmptyGuildPacket(GuildOps.GuildReq_InputMark); // DONE (o hi marc)

			/// <summary>
			/// TODO determine which scenarios this is sent
			/// </summary>
			/// <returns></returns>
			public static COutPacket LoadGuild_Done(bool bEncodeGuildData, Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_LoadGuild_Done);
				p.Encode1(bEncodeGuildData);

				if (bEncodeGuildData)
				{
					g.EncodeGuildData(p);
				}

				// if guild data has alliance ID then itll request alliance info after receiving this packet

				return p;
			} // DONE

			/// <summary>
			/// Response packet when a guild has been successfully created.
			/// Packet is sent to all creating members (party members)
			/// Note: Guild must already contain all party members for the packet to work
			/// </summary>
			/// <returns></returns>
			public static COutPacket CreateNewGuild_Done(Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_CreateNewGuild_Done);
				g.EncodeGuildData(p); // GUILDDATA::Decode(&v2->m_guild, iPacket);
				return p;
			} // DONE

			/// <summary>
			/// Packet sent to all guild members after a character has joined the guild.
			/// </summary>
			/// <returns></returns>
			public static COutPacket JoinGuild_Done(GuildMember g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_JoinGuild_Done);
				p.Encode4(g.Guild.GuildID);
				p.Encode4(g.dwParentID);
				g.EncodeGuildMember(p);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all members of the guild after a character leaves the guild or is expelled
			/// </summary>
			/// <returns></returns>
			public static COutPacket GuildRes_WithdrawGuild_Done(int nGuildID, int dwCharID, string sCharacterName)
				=> LeaveGuildPacket(GuildOps.GuildRes_WithdrawGuild_Done, nGuildID, dwCharID, sCharacterName); // DONE

			/// <summary>
			/// Gets sent to all members of the guild after a character leaves the guild or is expelled
			/// </summary>
			/// <returns></returns>
			public static COutPacket GuildRes_KickGuild_Done(int nGuildID, int dwCharID, string sCharacterName)
				=> LeaveGuildPacket(GuildOps.GuildRes_KickGuild_Done, nGuildID, dwCharID, sCharacterName); // DONE

			/// <summary>
			/// Helper function for the kick/withdraw packets
			/// </summary>
			/// <param name="nType"></param>
			/// <param name="nGuildID"></param>
			/// <param name="dwCharID"></param>
			/// <param name="sCharacterName"></param>
			/// <returns></returns>
			private static COutPacket LeaveGuildPacket(GuildOps nType, int nGuildID, int dwCharID, string sCharacterName)
			{
				if (nType != GuildOps.GuildRes_WithdrawGuild_Done && nType != GuildOps.GuildRes_KickGuild_Done)
					throw new InvalidOperationException($"Attempting to encode leaveguild info with operation { Enum.GetName(typeof(GuildOps), nType) }");

				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)nType);
				p.Encode4(nGuildID);
				p.Encode4(dwCharID);
				p.EncodeString(sCharacterName);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all characters in guild when guild is disbanded
			/// </summary>
			/// <returns></returns>
			public static COutPacket DisbandGuild(int nGuildID)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildRes_RemoveGuild_Done);
				p.Encode4(nGuildID);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to inviter when the invitee has blocked them
			/// </summary>
			/// <param name="sName"></param>
			/// <returns></returns>
			public static COutPacket GuildRes_InviteGuild_BlockedUser(string sName)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildRes_InviteGuild_BlockedUser);
				p.EncodeString(sName);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to the inviter when the invitee has already been invited
			/// </summary>
			/// <param name="sName"></param>
			/// <returns></returns>
			public static COutPacket InviteGuild_AlreadyInvited(string sName)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildRes_InviteGuild_AlreadyInvited);
				p.EncodeString(sName);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all guild masters when an invite is rejected
			/// </summary>
			/// <param name="sName"></param>
			/// <returns></returns>
			public static COutPacket InviteGuild_Rejected(string sName)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_InviteGuild_Rejected);
				p.EncodeString(sName);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all chars in guild after capacity is increased
			/// </summary>
			/// <returns></returns>
			public static COutPacket IncMaxMemberNum_Done(int nGuildID, byte nMaxMemberNum)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				p.Encode1((byte)GuildOps.GuildRes_IncMaxMemberNum_Done);
				p.Encode4(nGuildID);
				p.Encode1(nMaxMemberNum);
				return p;
			} // DONE

			/// <summary>
			/// Sent to all guild members after a member changes level or job
			/// </summary>
			/// <returns></returns>
			public static COutPacket ChangeLevelOrJob(GuildMember g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_ChangeLevelOrJob);
				p.Encode4(g.Guild.GuildID);
				p.Encode4(g.dwParentID);
				p.Encode4(g.ParentLevel);
				p.Encode4(g.ParentJob);
				return p;
			} // DONE

			/// <summary>
			/// Sent to all guild members after a member has logged in or out.
			/// </summary>
			/// <param name="g"></param>
			/// <returns></returns>
			public static COutPacket NotifyLoginOrLogout(GuildMember g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_NotifyLoginOrLogout);
				p.Encode4(g.Guild.GuildID);
				p.Encode4(g.dwParentID);
				p.Encode1(g.bOnline);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all guild members after the rank titles are changed
			/// </summary>
			/// <returns></returns>
			public static COutPacket SetGradeName_Done(Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_SetGradeName_Done);
				p.Encode4(g.GuildID);
				p.EncodeString(g.Rankt1Title);
				p.EncodeString(g.Rankt2Title);
				p.EncodeString(g.Rankt3Title);
				p.EncodeString(g.Rankt4Title);
				p.EncodeString(g.Rankt5Title);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all guild members after a guild members rank has changed
			/// </summary>
			/// <param name="g"></param>
			/// <returns></returns>
			public static COutPacket SetMemberGrade_Done(GuildMember g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_SetMemberGrade_Done);
				p.Encode4(g.Guild.GuildID);
				p.Encode4(g.dwParentID);
				p.Encode1(g.GuildRank);
				return p;
			} // DONE

			/// <summary>
			/// Sent to all guild members after the guild mark is changed
			/// </summary>
			/// <returns></returns>
			public static COutPacket SetMark_Done(Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_SetMark_Done);
				p.Encode4(g.GuildID);
				p.Encode2(g.GuildMarkBg);
				p.Encode1(g.GuildMarkBgColor);
				p.Encode2(g.GuildMark);
				p.Encode1(g.GuildMarkColor);
				return p;
			} // DONE

			/// <summary>
			/// Sent to all guild members after the guild notice has been changed
			/// </summary>
			/// <returns></returns>
			public static COutPacket SetNotice_Done(Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_SetNotice_Done);
				p.Encode4(g.GuildID);
				p.EncodeString(g.Notice);
				return p;
			} // DONE

			/// <summary>
			/// Gets sent to all members after guild points have been changed
			/// </summary>
			/// <param name="g"></param>
			/// <returns></returns>
			public static COutPacket IncPoint_Done(Guild g)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_IncPoint_Done);
				p.Encode4(g.GuildID);
				p.Encode4(g.GuildPoints);
				p.Encode4(g.GuildLevel);
				return p;
			} // DONE

			/// <summary>
			/// Guild ranking leaderboard.
			/// </summary>
			/// <param name="dwCharacterID"></param>
			/// <param name="dwNPCTemplateID"></param>
			/// <param name="gMan"></param>
			/// <returns></returns>
			public static COutPacket ShowGuildRanking(int dwCharacterID, int dwNPCTemplateID, GuildManager gMan)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_ShowGuildRanking);
				p.Encode4(dwCharacterID);
				p.Encode4(dwNPCTemplateID);
				var guildLeaderBoard = gMan.OrderByDescending(g => g.GuildPoints);
				var count = guildLeaderBoard.Count() > 10 ? 10 : guildLeaderBoard.Count();
				p.Encode4(count);
				foreach (var item in guildLeaderBoard)
				{
					p.EncodeString(item.GuildName);
					p.Encode4(item.GuildPoints);
					p.Encode4(item.GuildMark);
					p.Encode4(item.GuildMarkColor);
					p.Encode4(item.GuildMarkBg);
					p.Encode4(item.GuildMarkBgColor);
					count -= 1;
					if (count <= 0) break;
				}
				return p;
			} // DONE

			/// <summary>
			/// TODO figure out what this does
			/// </summary>
			/// <returns></returns>
			public static COutPacket GuildQuest_NoticeOrder()
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_GuildQuest_NoticeOrder);
				// byte
				// int
				return p;
			} // TODO

			/// <summary>
			/// TODO figure out what this does
			/// </summary>
			/// <param name="sGuildBoardAuthkey"></param>
			/// <returns></returns>
			public static COutPacket Authkey_Update(string sGuildBoardAuthkey)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_Authkey_Update);
				p.EncodeString(sGuildBoardAuthkey);
				return p;
			} // DONE

			/// <summary>
			/// TODO handle guild skills
			/// </summary>
			/// <param name="g"></param>
			/// <param name="nSkillID"></param>
			/// <returns></returns>
			public static COutPacket SetSkill_Done(Guild g, int nSkillID)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_SetSkill_Done);
				p.Encode4(g.GuildID);
				p.Encode4(nSkillID);

				// GUILDDATA::SKILLENTRY::Decode(&value, iPacket);
				{
					// this->nLevel = CInPacket::Decode2(iPacket);
					// CInPacket::DecodeBuffer(v3, &v2->dateExpire, 8u);
					// v4 = CInPacket::DecodeStr(v3, &iPacket);
					// ZXString<char>::operator=(&v2->strBuyCharacterName, v4);
				}

				return p;
			} // DONEISH

			/// <summary>
			/// Essentially a server msg with a guild header
			/// </summary>
			/// <param name="bDefaultMsg"></param>
			/// <param name="sMsg"></param>
			/// <returns></returns>
			public static COutPacket ServerMsg(bool bDefaultMsg, string sMsg)
			{
				var p = new COutPacket(SendOps.LP_GuildResult);
				EncodeHeader(p, GuildOps.GuildRes_ServerMsg);
				p.Encode1(!bDefaultMsg);
				if (!bDefaultMsg)
				{
					p.EncodeString(sMsg);
				}
				return p;
			} // DONE

			/// <summary>
			/// Broadcasted to every character in every map that contains a member of the guild
			/// </summary>
			/// <param name="dwCharID"></param>
			/// <param name="sGuildName"></param>
			/// <returns></returns>
			public static COutPacket GuildNameChanged(int dwCharID, string sGuildName) // CUserRemote::OnGuildMarkChanged
			{
				var p = new COutPacket(SendOps.LP_UserGuildNameChanged);
				p.Encode4(dwCharID);
				p.EncodeString(sGuildName);
				return p;
			} // DONE

			/// <summary>
			/// Broadcasted to every character in every map that contains a member of the guild
			/// </summary>
			/// <param name="dwCharID"></param>
			/// <param name="g"></param>
			/// <returns></returns>
			public static COutPacket GuildMarkChanged(int dwCharID, Guild g) // CUserRemote::OnGuildNameChanged
			{
				var p = new COutPacket(SendOps.LP_UserGuildMarkChanged);
				p.Encode4(dwCharID);
				p.Encode2(g.GuildMarkBg);
				p.Encode1(g.GuildMarkBgColor);
				p.Encode2(g.GuildMark);
				p.Encode1(g.GuildMarkColor);
				return p;
			} // DONE
		}
	}
}
