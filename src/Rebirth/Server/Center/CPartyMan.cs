using log4net;
using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Game;
using Rebirth.Network;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Server.Center
{
	// Phase out to use an internal dictionary

	// packets: OnChangeLevelOrJob, OnUserMigration
	public class CPartyMan : NumericKeyedCollection<Party>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CPartyMan));

		private readonly LoopingID PartyIDs = new LoopingID();

		public void Destroy(int partyId)
			=> Remove(partyId);

		/// <summary>
		/// Sends hp update packets to party members that are on the same map
		/// </summary>
		public void Update()
		{
			var toRemove = new List<Party>();
			//Log.Debug("Sending update packet.");
			foreach (var p in this)
			{
				if (p.Count <= 0)
				{
					toRemove.Add(p);
					Log.Debug("Removing party from party manager.");
					continue;
				}

				foreach (var member in p)
				{
					if (p.Count <= 1) continue;

					var pChar = member.CharObj;

					if (pChar is null || (!pChar?.Initialized ?? false) || pChar.Field is null) // this means theyre either not online or in cash shop/itc
						continue;

					member.Update();

					var exclude = new List<int> { pChar.dwId };

					foreach (var member2 in p)
					{
						var pOtherChar = member2.CharObj;

						if (pOtherChar is null) continue;

						if (!member2.InSameMap(pChar.Field.dwUniqueId))
						{
							exclude.Add(pOtherChar.dwId);
						}
					}

					if (exclude.Count >= p.Count) continue;

					var hpPacket = new COutPacket(SendOps.LP_UserHP);
					hpPacket.Encode4(pChar.dwId);
					hpPacket.Encode4(pChar.Stats.nHP);
					hpPacket.Encode4(pChar.BasicStats.nMHP);

					p.Broadcast(hpPacket, exclude.ToArray());
				}
			}

			foreach (var entry in toRemove)
			{
				Remove(entry);
			}
		}

		public void UpdatePartyMember(Character c, int dwPartyID, SocialNotiflag nFlag)
		{
			if (this[dwPartyID] is Party party && party.Contains(c.dwId))
			{
				var member = party[c.dwId];
				member.Update();

				switch (nFlag)
				{
					case SocialNotiflag.ChangeChannel:
						party.Broadcast(CPacket.CPartyMan.UserMigration(party));
						break;
					case SocialNotiflag.ChangeLevel:
					case SocialNotiflag.ChangeJob:
						party.Broadcast(CPacket.CPartyMan.ChangeLevelOrJob(member));
						break;
					case SocialNotiflag.LogOut:
						party.PlayerDC(c.dwId);
						break;
					default:
						party.Broadcast(CPacket.CPartyMan.LoadParty(party));
						break;
				}
			}
		}

		/// <summary>
		/// Gets party by character.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public Party GetParty(Character target)
			=> this.FirstOrDefault(p => p.Contains(target.dwId));

		//========================

		public void OnRequestPacket(WvsGameClient c, CInPacket p)
		{
			var pChar = c.Character;
			var opCode = (PartyOps)p.Decode1();

			switch (opCode)
			{
				case PartyOps.CreateParty:
					Create(pChar, p);
					break;
				case PartyOps.LeaveParty:
					Leave(pChar, p);
					break;
				//case PartyOps.JoinParty:
				//    Join(pChar, p);
				//    break;
				case PartyOps.InviteParty:
					Invite(pChar, p);
					break;
				case PartyOps.KickParty:
					Kick(pChar, p);
					break;
				case PartyOps.ChangePartyLeader:
					ChangePartyLeader(pChar, p);
					break;
			}
			c.Character.Action.Enable();
		}

		private void Create(Character c, CInPacket p)
		{
			if (c.Stats.nLevel < 10)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_CreateNewParty_Beginner));
				return;
			}

			if (GetParty(c) is null)
			{
				var party = new Party
				{
					PartyID = PartyIDs.NextValue(),
					dwOwnerId = c.dwId
				};

				party.Add(new PartyMember(c.dwId));
				Add(party);
				party[c.dwId].Update();

				c.SendPacket(CPacket.CPartyMan.CreateNewParty_Done(party));
			}
			else
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_AlreadyJoined));
			}
		}

		private void Leave(Character c, CInPacket p)
		{
			var party = GetParty(c);

			if (party is null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_WithdrawParty_NotJoined));
			}
			else if (c.Field.Template.HasPartyBossChangeLimit())
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_Unknown));
			}
			else
			{
				party.PlayerLeave(c.dwId, c.Stats.sCharacterName, false);
			}
		}

		// Recv [CP_PartyRequest] [91 00] [04] [06 00] [70 6F 6F 6D 61 6E]
		private void Invite(Character c, CInPacket p)
		{
			var inviteName = p.DecodeString();

			var invitePlayer = MasterManager.CharacterPool.Get(inviteName);

			if (invitePlayer is null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_UnknownUser));
			}
			else if (invitePlayer.Party != null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_AlreadyJoined));
			}
			else if (GetParty(c)?.dwOwnerId != c.dwId && GetParty(c) != null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_Unknown));
			}
			else
			{
				if (GetParty(c) is null)
				{
					Create(c, p);
				}

				if (GetParty(c).Count >= 6)
				{
					c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_AlreadyFull));
				}
				else if (GetParty(c).Contains(invitePlayer.dwId))
				{
					c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_AlreadyJoined));
				}
				else
				{
					GetParty(c).AddInvite(invitePlayer.dwId);
					invitePlayer?.SendPacket(CPacket.CPartyMan.InviteParty(c, GetParty(c)));
					c.SendPacket(CPacket.CPartyMan.InviteParty_Sent(inviteName));
				}
			}
		}

		private void Kick(Character c, CInPacket p)
		{
			int targetCharId = p.Decode4();

			var party = GetParty(c);

			if (party is null)
			{
				// do nothing
			}
			else if (party.dwOwnerId != c.dwId) // validate kicker is owner
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_KickParty_Unknown));
			}
			else if (!party.Contains(targetCharId))
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_UnknownUser));
			}
			else
			{
				var remote = MasterManager.CharacterPool.Get(targetCharId);
				party.PlayerLeave(remote.dwId, remote.Stats.sCharacterName, true);
			}
		}

		private void ChangePartyLeader(Character c, CInPacket p)
		{
			var charId = p.Decode4();

			if (GetParty(c) is null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_Unknown));
			}
			else if (GetParty(c).dwOwnerId != c.dwId)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_Unknown));
			}
			else if (!GetParty(c).Contains(charId))
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_Unknown));
			}
			else
			{
				GetParty(c).ChangePartyBoss(charId, false);
			}
		}

		//========================      

		public void OnResultPacket(WvsGameClient c, CInPacket p)
		{
			var pChar = c.Character;
			var opCode = (PartyOps)p.Decode1();

			switch (opCode)
			{
				case PartyOps.PartyRes_InviteParty_Rejected:
					InviteParty_Rejected(pChar, p);
					break;
				case PartyOps.PartyRes_InviteParty_Accepted:
					InviteParty_Accepted(pChar, p);
					break;
			}
			c.Character.Action.Enable();
		}

		private void InviteParty_Accepted(Character c, CInPacket p)
		{
			var partyID = p.Decode4();

			if (this[partyID] is null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_Unknown));
			}
			else if (!this[partyID].Invited(c.dwId))
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_Unknown));
			}
			else if (this[partyID].Count >= 6)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_AlreadyFull));
			}
			else
			{
				this[partyID].PlayerJoin(c.dwId, c.Stats.sCharacterName);
			}
		}

		private void InviteParty_Rejected(Character c, CInPacket p)
		{
			var partyID = p.Decode4();

			if (this[partyID] is null)
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_Unknown));
			}
			else if (!this[partyID].Invited(c.dwId))
			{
				c.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_JoinParty_Unknown));
			}
			else
			{
				this[partyID].Invites.Remove(c.dwId);

				var owner = MasterManager.CharacterPool.Get(this[partyID].dwOwnerId);
				owner.SendPacket(CPacket.CPartyMan.InviteParty_Rejected(c.Stats.sCharacterName));
			}
		}

		protected override int GetKeyForItem(Party item) => item.PartyID;
	}
}
