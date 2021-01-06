namespace Rebirth.Game
{
	public static class MobSkill
	{
		public enum MobSkillID
		{
			POWERUP = 100,
			MAGICUP = 0x65,
			PGUARDUP = 0x66,
			MGUARDUP = 0x67,
			HASTE = 0x68,
			POWERUP_M = 0x6E,
			MAGICUP_M = 0x6F,
			PGUARDUP_M = 0x70,
			MGUARDUP_M = 0x71,
			HEAL_M = 0x72,
			HASTE_M = 0x73,
			SEAL = 120,
			DARKNESS = 121,
			WEAKNESS = 0x7A,
			STUN = 0x7B,
			CURSE = 124,
			POISON = 125,
			SLOW = 126,
			DISPEL = 127,
			ATTRACT = 0x80,
			BANMAP = 0x81,
			AREA_FIRE = 0x82,
			AREA_POISON = 0x83,
			REVERSE_INPUT = 0x84,
			UNDEAD = 0x85,
			STOPPORTION = 0x86,
			STOPMOTION = 0x87,
			FEAR = 0x88,
			FROZEN = 0x89,
			PHYSICAL_IMMUNE = 0x8C,
			MAGIC_IMMUNE = 0x8D,
			HARDSKIN = 142,

			PCOUNTER = 0x8F,
			MCOUNTER = 0x90,
			PMCOUNTER = 0x91,
			PAD = 0x96,
			MAD = 0x97,
			PDR = 0x98,
			MDR = 0x99,
			ACC = 0x9A,
			EVA = 0x9B,
			SPEED = 0x9C,
			SEALSKILL = 0x9D,
			BALROGCOUNTER = 0x9E,
			MOBSILLL_SPREADSKILLFROMUSER = 0xA0,
			HEALBYDAMAGE = 0xA1,
			BIND = 0xA2,
			SUMMON = 0xC8,
			SUMMON_CUBE = 0xC9
		}

		public static bool IsStatChange(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.POWERUP:
				case MobSkillID.MAGICUP:
				case MobSkillID.PGUARDUP:
				case MobSkillID.MGUARDUP:
				case MobSkillID.HASTE:
				case MobSkillID.SPEED:
				case MobSkillID.PHYSICAL_IMMUNE:
				case MobSkillID.MAGIC_IMMUNE:
				case MobSkillID.HARDSKIN:
				case MobSkillID.PAD:
				case MobSkillID.MAD:
				case MobSkillID.PDR:
				case MobSkillID.MDR:
				case MobSkillID.ACC:
				case MobSkillID.EVA:
				case MobSkillID.SEALSKILL:
				case MobSkillID.PCOUNTER:
				case MobSkillID.MCOUNTER:
				case MobSkillID.PMCOUNTER:
					return true;
				default: return false;
			}
		}

		public static bool IsUserStatChange(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.SEAL:
				case MobSkillID.DARKNESS:
				case MobSkillID.WEAKNESS:
				case MobSkillID.STUN:
				case MobSkillID.CURSE:
				case MobSkillID.POISON:
				case MobSkillID.SLOW:
				case MobSkillID.ATTRACT:
				case MobSkillID.BANMAP:
				case MobSkillID.DISPEL:
				case MobSkillID.REVERSE_INPUT:
					return true;
				default: return false;
			}
		}

		public static bool IsPartizanStatChange(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.POWERUP_M:
				case MobSkillID.MAGICUP_M:
				case MobSkillID.PGUARDUP_M:
				case MobSkillID.MGUARDUP_M:
				case MobSkillID.HASTE_M:
					return true;
				default: return false;
			}
		}

		public static bool IsPartizanOneTimeStatChange(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.HEAL_M:
					return true;
				default: return false;
			}
		}

		public static bool IsSummon(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.SUMMON:
					return true;
				default: return false;
			}
		}

		public static bool IsAffectArea(int nSkillID)
		{
			switch ((MobSkillID)nSkillID)
			{
				case MobSkillID.AREA_FIRE:
				case MobSkillID.AREA_POISON:
					return true;
				default: return false;
			}
		}
	}
}
