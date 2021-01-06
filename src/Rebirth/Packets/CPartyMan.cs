using Rebirth.Characters;
using Rebirth.Game;
using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Common.Types;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CPartyMan
		{
			private static COutPacket CreateResultPacket(PartyOps header)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)header);
				return p;
			}

			public static COutPacket Party_Error(PartyOps error)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)error);
				return p;
			}

			public static COutPacket InviteParty_Rejected(string charName)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_InviteParty_Rejected);
				p.EncodeString(charName);

				return p;
			}

			public static COutPacket CreateNewParty_Done(Party party)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_CreateNewParty_Done);
				p.Encode4(party.PartyID);
				p.Encode4(0); //Town Id
				p.Encode4(0); //Field Id
				p.Encode4(0); //nSkillId 

				p.Encode2(0); //No Clue
				p.Encode2(0); //No Clue

				return p;
			}

			public static COutPacket InviteParty_Sent(string name)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_InviteParty_Sent);
				p.EncodeString(name);
				return p;
			}

			public static COutPacket InviteParty(Character c, Party party)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.InviteParty);
				p.Encode4(party.PartyID);
				p.EncodeString(c.Stats.sCharacterName);
				p.Encode4(c.Stats.nLevel);
				p.Encode4(c.Stats.nJob);
				p.Encode1(0);
				return p;
			}

			public static COutPacket ChangePartyBoss_Done(Party party, int dwCharId, bool bDisconnected)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_ChangePartyBoss_Done);
				p.Encode4(dwCharId);
				p.Encode1(bDisconnected);
				return p;
			}

			public static COutPacket Party_Error(Party party, PartyOps nErrorType)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)nErrorType);
				return p;
			}

			public static COutPacket LoadParty(Party party)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_LoadParty_Done);
				p.Encode4(party.PartyID);
				party.EncodePartyData(p);
				return p;
			}

			//wasKicked | true = kick | false = left the party
			public static COutPacket WithdrawParty_Done(Party party, int dwCharId, string sName, bool bKicked)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1(PartyOps.PartyRes_WithdrawParty_Done);
				p.Encode4(party.PartyID);
				p.Encode4(dwCharId);
				p.Encode1(party.dwOwnerId != dwCharId);
				if (party.dwOwnerId != dwCharId)
				{
					p.Encode1(bKicked);
					p.EncodeString(sName);
					party.EncodePartyData(p);
				}
				return p;
			}

			public static COutPacket JoinParty_Done(Party party, string sName)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_JoinParty_Done);
				p.Encode4(party.PartyID);
				p.EncodeString(sName);
				party.EncodePartyData(p);
				return p;
			}

			public static COutPacket UserMigration(Party party)
			{
				var p = new COutPacket(SendOps.LP_PartyResult);
				p.Encode1((byte)PartyOps.PartyRes_UserMigration);
				p.Encode4(party.PartyID);
				party.EncodePartyData(p);
				return p;
			}

			public static COutPacket ChangeLevelOrJob(PartyMember pm)
			{
				var p = CreateResultPacket(PartyOps.PartyRes_ChangeLevelOrJob);
				p.Encode4(pm.dwCharId);
				p.Encode4(pm.nLevel);
				p.Encode4(pm.nJob);
				return p;
			}
		}
	}
}
