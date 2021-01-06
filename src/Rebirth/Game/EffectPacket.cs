using Rebirth.Characters;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Game
{
	public class UserEffectPacket
	{
		private readonly UserEffect effectType;

		public int nSkillID { get; set; }
		public byte nCharLevel { get; private set; }
		public byte nSLV { get; set; }
		public int dwMobId { get; set; }
		public bool bDarkForceActive { get; set; }
		public bool bDragonFuryActive { get; set; }
		public bool bLeft { get; set; }
		public short ptX { get; set; }
		public short ptY { get; set; }
		public byte nCaptureMsg { get; set; }
		public byte nSelect { get; set; } // used for dice
		public byte nQuestInfo { get; set; } // unsure
		public int nItemID { get; set; }
		public int nQuestItemFormat { get; set; } // unsure
		public string sName { get; set; }
		public int nEffect { get; set; }

		public byte cPet { get; set; }
		public byte nType { get; set; }

		public byte nDelta { get; set; }

		public UserEffectPacket(UserEffect type)
		{
			effectType = type;
		}

		public void BroadcastEffect(Character c, bool bLocal = true, bool bRemote = true)
		{
			nCharLevel = c.Stats.nLevel;

			if (bLocal)
			{
				c.SendPacket(GenerateForLocal());
			}

			if (bRemote)
			{
				c.Field.Broadcast(GenerateForRemote(c.dwId), c);
			}
		}

		public COutPacket GenerateForLocal()
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			Encode(p);
			return p;
		}

		public COutPacket GenerateForRemote(int dwParentId)
		{
			var p = new COutPacket(SendOps.LP_UserEffectRemote);
			p.Encode4(dwParentId);
			Encode(p);
			return p;
		}

		private void Encode(COutPacket p)
		{
			p.Encode1((byte)effectType);
			switch (effectType)
			{
				case UserEffect.LevelUp:
				case UserEffect.PortalSoundEffect:
				case UserEffect.JobChanged:
				case UserEffect.QuestComplete:
				case UserEffect.MonsterBookCard:
					return;

				case UserEffect.SkillUse:
					p.Encode4(nSkillID);
					p.Encode1(nCharLevel);
					p.Encode1(nSLV);

					switch ((Skills)nSkillID)
					{
						case Skills.DARKKNIGHT_DARK_FORCE:
							p.Encode1(bDarkForceActive);
							break;
						case Skills.EVAN_DRAGON_FURY:
							p.Encode1(bDragonFuryActive);
							break;
						case Skills.DUAL5_ASSASSINATION:
							p.Encode1(bLeft);
							p.Encode4(dwMobId);
							break;
						case Skills.WILDHUNTER_CAPTURE:
							p.Encode1(nCaptureMsg);
							break;
						case Skills.WILDHUNTER_SUMMON_MONSTER:
							p.Encode1(bLeft);
							p.Encode2(ptX);
							p.Encode2(ptY);
							break;
					}

					return;

				case UserEffect.SkillAffected:
				case UserEffect.SkillAffectedSpecial:
					p.Encode4(nSkillID);
					p.Encode1(nCharLevel);
					return;

				case UserEffect.SkillAffectedSelect:
					p.Encode4(nSelect);
					p.Encode4(nSkillID);
					p.Encode1(nSLV);
					return;

				case UserEffect.Quest: // idk this one is weird
					p.Encode1(nQuestInfo);
					p.Encode4(nItemID);
					p.Encode4(nQuestItemFormat);
					p.EncodeString(sName);
					p.Encode4(nEffect);
					return;

				case UserEffect.PetShowEffect:
					p.Encode1(nType);
					p.Encode1(cPet);
					return;

				case UserEffect.ShowSkillSpecialEffect:
					p.Encode4(nSkillID);
					if (nSkillID == (int)Skills.DUAL5_MONSTER_BOMB)
					{
						p.Encode4(ptX);
						p.Encode4(ptY);
						p.Encode4(nSLV);
						p.Encode4(0); // unsure
					}
					return;

				case UserEffect.ProtectOnDieItemUse: // todo
					p.Encode1(0);
					p.Encode1(0);
					p.Encode1(0);
					return;

				case UserEffect.IncDecHPEffect:
					p.Encode1(nDelta);
					return;

				case UserEffect.BuffItemEffect:
					p.Encode4(nItemID);
					return;

				case UserEffect.SquibEffect:
					p.EncodeString(sName);
					return;

				case UserEffect.ReservedEffect:
					p.EncodeString(sName);
					break;
			}
		}
	}
}
