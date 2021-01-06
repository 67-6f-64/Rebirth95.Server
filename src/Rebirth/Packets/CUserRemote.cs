using Rebirth.Characters;
using Rebirth.Characters.Combat;
using Rebirth.Common.Tools;
using Rebirth.Network;
using Rebirth.Tools;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CUserRemote
		{
			public static COutPacket UserHit(int dwCharId, byte nAttackIdx, int nDamageRaw, int dwTemplateID, bool bLeft, byte nReflect, bool bPowerGuard, int dwMobID, byte nHitAction, TagPoint tagPOINT, bool bGuard, byte nStanceFlag, int nShadowSifterID, int nDamageShownToRemote)
			{
				var p = new COutPacket(SendOps.LP_UserHit);
				p.Encode4(dwCharId);
				p.Encode1(nAttackIdx); //skill
				p.Encode4(nDamageRaw);

				// normally the check is nAttackIdx > -2 but we cant cast the byte or itd turn into 254 which would always be false
				if (nAttackIdx != 0xFD && nAttackIdx != 0xFC)
				{
					p.Encode4(dwTemplateID);
					p.Encode1(bLeft);
					p.Encode1(nReflect);
					if (nReflect > 0)
					{
						p.Encode1(bPowerGuard);
						p.Encode4(dwMobID);

						p.Encode1(nHitAction);
						tagPOINT.Encode(p);// p.Encode4(0); // encode tagPOINT -> ptHit = CInPacket::Decode4(v3);
					}
					p.Encode1(bGuard);
					p.Encode1(nStanceFlag); // 1 or 2 -> 2 means its wildhunter jaguar boost for some reason, otherwise it uses the get_stance_skill_id
				}
				p.Encode4(nDamageShownToRemote);

				if (nDamageShownToRemote < 0) // 0 is miss, -1 is shadow sifter dodge (fake in odin)
				{
					p.Encode4(nShadowSifterID);
				}

				return p;
			}

			public static COutPacket UserMeleeAttack(int cid, MapleAttack a, byte nSLV, byte nLevel)
			{
				var p = new COutPacket(SendOps.LP_UserMeleeAttack);
				p.Encode4(cid);
				a.EncodeAttackInfo(null, p, nSLV, nLevel, SendOps.LP_UserMeleeAttack);
				return p;
			}

			public static COutPacket UserShootAttack(Character c, MapleAttack a, byte nSLV, byte nLevel)
			{
				var p = new COutPacket(SendOps.LP_UserShootAttack);
				p.Encode4(c.dwId);
				a.EncodeAttackInfo(c, p, nSLV, nLevel, SendOps.LP_UserShootAttack);
				return p;
			}

			public static COutPacket UserMagicAttack(int cid, MapleAttack a, byte nSLV, byte nLevel)
			{
				var p = new COutPacket(SendOps.LP_UserMagicAttack);
				p.Encode4(cid);
				a.EncodeAttackInfo(null, p, nSLV, nLevel, SendOps.LP_UserMagicAttack);
				return p;
			}

			public static COutPacket UserBodyAttack(int cid, MapleAttack a, byte nSLV, byte nLevel)
			{
				var p = new COutPacket(SendOps.LP_UserBodyAttack);
				p.Encode4(cid);
				a.EncodeAttackInfo(null, p, nSLV, nLevel, SendOps.LP_UserBodyAttack);
				return p;
			}

			public static COutPacket UserMovingShootAttackPrepare(int dwCharId, byte nCharLevel, byte nSLV, int nSkillID, short nMoveAction, byte nActionSpeed)
			{
				var p = new COutPacket(SendOps.LP_UserMovingShootAttackPrepare);
				p.Encode4(dwCharId);
				p.Encode1(nCharLevel);
				p.Encode1(nSLV);

				if (nSLV > 0)
					p.Encode4(nSkillID);

				p.Encode2(nMoveAction);
				p.Encode1(nActionSpeed);

				return p;
			}
		}
	}
}
