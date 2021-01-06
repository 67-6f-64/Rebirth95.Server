using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Common.Types;

namespace Rebirth.Common.GameLogic
{
	public static class SkillLogic
	{
		public static short EnergyMax = 10000; // CC_EnergyMax  = 2710h
		public static bool IsMobCaptureSkill(int nSkillID) => nSkillID == (int)Skills.WILDHUNTER_CAPTURE;
		public static bool IsSummonCapturedMobSkill(int nSkillID) => nSkillID == (int)Skills.WILDHUNTER_SUMMON_MONSTER;
		public static bool IsHookAndHitSkill(int nSkillID) => nSkillID == (int)Skills.DARKKNIGHT_MONSTER_MAGNET || nSkillID == (int)Skills.HERO_MONSTER_MAGNET;
		public static bool IsSmokeShellSkill(int nSkillID) => nSkillID == (int)Skills.SHADOWER_SMOKE_SHELL;
		public static bool IsDamageMeterSkill(int nSkillID, short nJob) => get_novice_skill_as_race(NoviceSkillID.DamageMeter, nJob) == nSkillID;
		public static bool IsFlyingSkill(int nSkillID, short nJob) => get_novice_skill_as_race(NoviceSkillID.Soaring, nJob) == nSkillID;
		public static bool IsClericHealSkill(int nSkillID) => nSkillID == (int)Skills.CLERIC_HEAL;
		public static bool IsMesoExplosionSkill(int nSkillID) => nSkillID == (int)Skills.THIEFMASTER_MESO_EXPLOSION;
		public static bool IsOpenGateSkill(int nSkillID) => nSkillID == (int)Skills.MECHANIC_OPEN_GATE;
		public static bool IsRecoveryAura(int nSkillID) => nSkillID == (int)Skills.EVAN_RECOVERY_AURA;
		public static bool IsMysticDoorSkill(int nSkillID, short nJob) => get_novice_skill_as_race(NoviceSkillID.DecentMysticDoor, nJob) == nSkillID;
		public static bool IsStatChangeAdminSkill(int nSkillID, short nJob) => get_novice_skill_as_race(NoviceSkillID.StatChangeAdmin, nJob) == nSkillID;
		public static bool is_guided_bullet_skill(int nSkillID) => nSkillID == 5211006 || nSkillID == 5220011 || nSkillID == 22151002;

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static short get_required_combo_count(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.CRUSADER_SWORD_PANIC:
				case Skills.CRUSADER_SWORD_COMA:
				case Skills.SOULMASTER_PANIC_SWORD:
				case Skills.SOULMASTER_COMA_SWORD:
					return 1;
				case Skills.HERO_ENRAGE:
					return 10;
			}

			if (nSkillID > 21110004)
			{
				if (nSkillID >= 21120006 && nSkillID <= 21120007)
					return 200;
			}
			else
			{
				if (nSkillID == 21110004)
					return 100;
				if (nSkillID >= 21100004 && nSkillID <= 21100005)
					return 30;
			}
			return 0;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsWeaponBoosterSkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.ARAN_POLEARM_BOOSTER:
				case Skills.ASSASSIN_JAVELIN_BOOSTER:
				case Skills.BMAGE_STAFF_BOOSTER:
				case Skills.CROSSBOWMAN_CROSSBOW_BOOSTER:
				case Skills.DUAL1_DUAL_BOOSTER:
				case Skills.EVAN_MAGIC_BOOSTER:
				case Skills.FIGHTER_WEAPON_BOOSTER:
				case Skills.FLAMEWIZARD_MAGIC_BOOSTER:
				case Skills.GUNSLINGER_GUN_BOOSTER:
				case Skills.HUNTER_BOW_BOOSTER:
				case Skills.INFIGHTER_KNUCKLE_BOOSTER:
				case Skills.MAGE1_MAGIC_BOOSTER:
				case Skills.MAGE2_MAGIC_BOOSTER:
				case Skills.MECHANIC_BOOSTER:
				case Skills.NIGHTWALKER_JAVELIN_BOOSTER:
				case Skills.PAGE_WEAPON_BOOSTER:
				case Skills.SOULMASTER_SWORD_BOOSTER:
				case Skills.SPEARMAN_WEAPON_BOOSTER:
				case Skills.STRIKER_KNUCKLE_BOOSTER:
				case Skills.THIEF_DAGGER_BOOSTER:
				case Skills.WILDHUNTER_CROSSBOW_BOOSTER:
				case Skills.WINDBREAKER_BOW_BOOSTER:
					return true;
				default:
					return false;
			}
		}

		public static int get_summoned_attack_delay(int nSkillID, byte nSLV, int xEffect)
		{
			var v0 = false;
			if (nSkillID <= 35111002)
			{
				if (nSkillID != 35111002)
				{
					if (nSkillID == 5211002 || nSkillID == 5220002)
						return 1500;
					if (nSkillID != 33101008)
						return 3000;
				}
				v0 = true;
			}

			if (v0 || nSkillID == 35121009)
			{
				if (nSLV <= 0)
					return 3000;

				return 1000 * xEffect;
			}

			if (nSkillID != 35121011)
				return 3000;

			return 300;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsWeaponChargeSkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.ARAN_SNOW_CHARGE:
				case Skills.KNIGHT_FIRE_CHARGE:
				case Skills.KNIGHT_ICE_CHARGE:
				case Skills.KNIGHT_LIGHTNING_CHARGE:
				case Skills.PALADIN_DIVINE_CHARGE:
				case Skills.PALADIN_ADVANCED_CHARGE:
				case Skills.SOULMASTER_SOUL_CHARGE:
				case Skills.STRIKER_LIGHTNING_CHARGE:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsSharpEyesSkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				// real sharp eyes
				case Skills.BOWMASTER_SHARP_EYES:
				case Skills.CROSSBOWMASTER_SHARP_EYES:
				case Skills.WILDHUNTER_SHARP_EYES:
				// potential skill sharp eyes
				case Skills.CITIZEN_SHARP_EYES:
				case Skills.EVANJR_SHARP_EYES:
				case Skills.LEGEND_SHARP_EYES:
				case Skills.NOBLESSE_SHARP_EYES:
				case Skills.NOVICE_SHARP_EYES:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsHyperBodySkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.SPEARMAN_HYPER_BODY:
				case Skills.ADMIN_HYPER_BODY:
				case Skills.CITIZEN_HYPER_BODY:
				case Skills.EVANJR_HYPER_BODY:
				case Skills.LEGEND_HYPER_BODY:
				case Skills.NOBLESSE_HYPER_BODY:
				case Skills.NOVICE_HYPER_BODY:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsDashSkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.EVANJR_SPACE_EVENT_RIDING_DASH:
				case Skills.LEGEND_SPACE_EVENT_RIDING_DASH:
				case Skills.NOBLESSE_SPACE_EVENT_RIDING_DASH:
				case Skills.NOVICE_SPACE_EVENT_RIDING_DASH:
				case Skills.STRIKER_DASH:
				case Skills.PIRATE_DASH:
				case Skills.DUAL3_HUSTLE_DASH:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsMapleWarriorSkill(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.ARAN_MAPLE_HERO:
				case Skills.ARCHMAGE1_MAPLE_HERO:
				case Skills.ARCHMAGE2_MAPLE_HERO:
				case Skills.BISHOP_MAPLE_HERO:
				case Skills.BMAGE_MAPLE_HERO:
				case Skills.BOWMASTER_MAPLE_HERO:
				case Skills.CAPTAIN_MAPLE_HERO:
				case Skills.CROSSBOWMASTER_MAPLE_HERO:
				case Skills.DARKKNIGHT_MAPLE_HERO:
				case Skills.DUAL5_MAPLE_HERO:
				case Skills.EVAN_MAPLE_HERO:
				case Skills.HERO_MAPLE_HERO:
				case Skills.MECHANIC_MAPLE_HERO:
				case Skills.NIGHTLORD_MAPLE_HERO:
				case Skills.PALADIN_MAPLE_HERO:
				case Skills.SHADOWER_MAPLE_HERO:
				case Skills.VIPER_MAPLE_HERO:
				case Skills.WILDHUNTER_MAPLE_HERO:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// Not a comprehensive list, just includes items that have a bufftime but arent actually buffs.
		/// </summary>
		public static bool IsNotBuff(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.ARAN_ROLLING_SPIN:
				case Skills.SHADOWER_BOOMERANG_STEP:
				case Skills.CRUSADER_SWORD_COMA:
				case Skills.MECHANIC_ROCKET_PUNCH:
				case Skills.MECHANIC_EARTH_SLUG: // atomic hammer
				case Skills.WILDHUNTER_CROSS_ROAD: // dash n slash
				case Skills.WILDHUNTER_JAGUAR_NUCKBACK: // jaguar rawr
				case Skills.WILDHUNTER_ELRECTRONICSHOCK: // sonic roar
				case Skills.WILDHUNTER_BOMB_SHOOT: // ricochet
				case Skills.BMAGE_NEMESIS: // dark genesis
				case Skills.BMAGE_ADVENCED_DARK_CHAIN:
				case Skills.BMAGE_DARK_BOW: // dark chain
				case Skills.EVAN_BLAZE:
				case Skills.EVAN_BREATH: // fire breath
				case Skills.SOULMASTER_COMA_SWORD:
				case Skills.GUNSLINGER_FAKE_SHOT:
				case Skills.VIPER_SNATCH:
				case Skills.BUCCANEER_ENERGY_BURSTER:
				case Skills.INFIGHTER_DOUBLE_UPPER:
				case Skills.INFIGHTER_BACKSPIN_BLOW:
				case Skills.DUAL4_FLYING_ASSAULTER:
				case Skills.THIEFMASTER_ASSAULTER:
				case Skills.THIEF_STEAL:
				case Skills.PRIEST_SHINING_RAY:
				case Skills.MAGE2_THUNDER_SPEAR:
				case Skills.DARKKNIGHT_MONSTER_MAGNET:
				case Skills.HERO_MONSTER_MAGNET:
				case Skills.KNIGHT_CHARGE_BLOW:
				case Skills.CRUSADER_SHOUT:
				case Skills.NIGHTLORD_NINJA_STORM:
				case Skills.WILDHUNTER_NERVEGAS:
				case Skills.MECHANIC_OPEN_GATE:
				case Skills.NIGHTWALKER_DISORDER:
				case Skills.ROGUE_DISORDER:
				case Skills.CRUSADER_AXE_PANIC:
				case Skills.CRUSADER_SWORD_PANIC:
				case Skills.SOULMASTER_PANIC_SWORD:
				case Skills.DUAL5_MONSTER_BOMB:
				case Skills.FLAMEWIZARD_SEAL:
				case Skills.MAGE1_SEAL:
				case Skills.MAGE2_SEAL:
				case Skills.NIGHTLORD_SHOWDOWN:
				case Skills.SHADOWER_SHOWDOWN:
				case Skills.ARAN_COMBO_TEMPEST:
				case Skills.VALKYRIE_COOLING_EFFECT: // ice splitter
				case Skills.SNIPER_ICE_SHOT: // blizzard
				case Skills.MAGE2_MAGIC_COMPOSITION: // element composition
				case Skills.WIZARD2_COLD_BEAM:
				case Skills.MAGE2_ICE_STRIKE:
				case Skills.ARCHMAGE2_BLIZZARD:
				case Skills.CAPTAIN_MIND_CONTROL:
					return true;
				default:
					return false;
			}
		}

		// the below are bad odin craps
		//public static int getStatDice(int stat)
		//{
		//	switch (stat)
		//	{
		//		case 2:
		//			return 30;
		//		case 3:
		//			return 20;
		//		case 4:
		//			return 15;
		//		case 5:
		//			return 20;
		//		case 6:
		//			return 30;
		//	}
		//	return 0;
		//}

		//public static int getDiceStat(int buffid, int stat)
		//{
		//	if (buffid == stat || buffid % 10 == stat || buffid / 10 == stat)
		//	{
		//		return getStatDice(stat);
		//	}
		//	else if (buffid == (stat * 100))
		//	{
		//		return getStatDice(stat) + 10;
		//	}
		//	return 0;
		//}

		public static int[] GetHiddenLinkSkills(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.WILDHUNTER_MINE:
					return new int[]
					{
						(int)Skills.WILDHUNTER_MINE_DUMMY_SUMMONED
					};
				case Skills.WILDHUNTER_SWALLOW:
					return new int[]
					{
						(int)Skills.WILDHUNTER_SWALLOW_DUMMY_ATTACK,
						(int)Skills.WILDHUNTER_SWALLOW_DUMMY_BUFF
					};
				case Skills.MECHANIC_WEAPONMASTERY:
					return new int[]
					{
						(int)Skills.MECHANIC_GATLING_UP,
						(int)Skills.MECHANIC_FLAMETHROWER_UP
					};
				case Skills.MECHANIC_ROBOROBO:
					return new int[]
					{
						(int)Skills.MECHANIC_ROBOROBO_DUMMY
					};
				case Skills.BMAGE_SUPER_BODY:
					return new int[]
					{
						(int)Skills.BMAGE_SUPER_BODY_BLUE,
						(int)Skills.BMAGE_SUPER_BODY_DARK,
						(int)Skills.BMAGE_SUPER_BODY_YELLOW
					};
				case Skills.BMAGE_FINISH_ATTACK:
					return new int[]
					{
						(int)Skills.BMAGE_FINISH_ATTACK1,
						(int)Skills.BMAGE_FINISH_ATTACK2,
						(int)Skills.BMAGE_FINISH_ATTACK3,
						(int)Skills.BMAGE_FINISH_ATTACK4,
						(int)Skills.BMAGE_FINISH_ATTACK5,
					};
				case Skills.MECHANIC_SATELITE:
					return new int[]
					{
						(int)Skills.MECHANIC_SATELITE2,
						(int)Skills.MECHANIC_SATELITE3,
					};
			}
			return new int[0];
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsPartyBuff(int nSkillID)
		{
			switch ((Skills)nSkillID)
			{
				case Skills.EVAN_MAGIC_SHIELD:
				case Skills.EVAN_MAGIC_RESISTANCE:
				case Skills.FIGHTER_FURY:
				case Skills.FLAMEWIZARD_MEDITATION:
				case Skills.WIZARD1_MEDITATION:
				case Skills.WIZARD2_MEDITATION:
				case Skills.ADMIN_HOLY_SYMBOL:
				case Skills.PRIEST_HOLY_SYMBOL:
				case Skills.ADMIN_DISPEL:
				case Skills.PRIEST_DISPEL:
				case Skills.CLERIC_HEAL:
				case Skills.ADMIN_BLESS:
				case Skills.CLERIC_BLESS:

				case Skills.ADMIN_RESURRECTION:
				case Skills.BISHOP_RESURRECTION:

				case Skills.KNIGHT_COMBAT_ORDERS:

				case Skills.ARAN_MAPLE_HERO:
				case Skills.ARCHMAGE1_MAPLE_HERO:
				case Skills.ARCHMAGE2_MAPLE_HERO:
				case Skills.BISHOP_MAPLE_HERO:
				case Skills.BMAGE_MAPLE_HERO:
				case Skills.BOWMASTER_MAPLE_HERO:
				case Skills.CAPTAIN_MAPLE_HERO:
				case Skills.CROSSBOWMASTER_MAPLE_HERO:
				case Skills.DARKKNIGHT_MAPLE_HERO:
				case Skills.DUAL5_MAPLE_HERO:
				case Skills.EVAN_MAPLE_HERO:
				case Skills.HERO_MAPLE_HERO:
				case Skills.MECHANIC_MAPLE_HERO:
				case Skills.NIGHTLORD_MAPLE_HERO:
				case Skills.PALADIN_MAPLE_HERO:
				case Skills.SHADOWER_MAPLE_HERO:
				case Skills.VIPER_MAPLE_HERO:
				case Skills.WILDHUNTER_MAPLE_HERO:

				case Skills.ADMIN_HYPER_BODY:
				case Skills.SPEARMAN_HYPER_BODY:

				case Skills.BOWMASTER_SHARP_EYES:
				case Skills.CROSSBOWMASTER_SHARP_EYES:
				case Skills.WILDHUNTER_SHARP_EYES:
				case Skills.DUAL5_THORNS_EFFECT:
				case Skills.HERMIT_MESO_UP:

				case Skills.ARAN_COMBO_BARRIER:
				case Skills.SOULMASTER_FURY:
				case Skills.VIPER_WIND_BOOSTER:
				case Skills.STRIKER_WIND_BOOSTER:

				case Skills.ASSASSIN_HASTE:
				case Skills.NIGHTWALKER_HASTE:
				case Skills.THIEF_HASTE:

				case Skills.BISHOP_HOLY_SHIELD:
					return true;
			}

			return false;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool IsSummonSkill(int nSkillID)
		{
			switch ((Skills)nSkillID) // todo enum
			{
				case Skills.SOULMASTER_SOUL:
				case Skills.FLAMEWIZARD_FLAME:
				case Skills.WINDBREAKER_STORM:
				case Skills.NIGHTWALKER_DARKNESS:
				case Skills.STRIKER_LIGHTNING:
				case Skills.DARKKNIGHT_BEHOLDER:
				case Skills.RANGER_SILVER_HAWK:
				case Skills.WILDHUNTER_SILVER_HAWK:
				case Skills.ARCHMAGE2_ELQUINES:
				case Skills.ARCHMAGE1_IFRIT:
				case Skills.FLAMEWIZARD_IFRIT:
				case Skills.PRIEST_SUMMON_DRAGON:
				case Skills.BOWMASTER_PHOENIX:
				case Skills.BISHOP_BAHAMUT:
				case Skills.CROSSBOWMASTER_FREEZER: // frost prey lmao
				case Skills.VALKYRIE_GABIOTA: // gaviota
				case Skills.DUAL5_DUMMY_EFFECT:
				case Skills.CAPTAIN_SUPPORT_OCTOPUS:
				case Skills.VALKYRIE_OCTOPUS:
				case Skills.WILDHUNTER_MINE_DUMMY_SUMMONED:
				case Skills.WILDHUNTER_TRAP:
				case Skills.MECHANIC_TESLA_COIL:
				case Skills.MECHANIC_VELOCITY_CONTROLER:
				case Skills.MECHANIC_HEALING_ROBOT_H_LX:
				case Skills.MECHANIC_SG88:
				case Skills.MECHANIC_AR_01:
				case Skills.MECHANIC_ROBOROBO_DUMMY:
				case Skills.MECHANIC_SATELITE:
				case Skills.MECHANIC_SATELITE2:
				case Skills.MECHANIC_SATELITE3:
				case Skills.MECHANIC_ROBOROBO:
				case Skills.SNIPER_GOLDEN_EAGLE:
				case Skills.THIEFMASTER_SHADOW_MIRROR:
				case Skills.HERMIT_SHADOW_MIRROR:
				case Skills.RANGER_PUPPET:
				case Skills.SNIPER_PUPPET:
				case Skills.WINDBREAKER_PUPPET:
					return true;
			}

			return false;
		}

		public static bool is_teleport_mastery_skill(int nSkillID)
		{
			bool v1; // zf

			if (nSkillID > 2311007)
			{
				v1 = nSkillID == 32111010;
			}
			else
			{
				switch (nSkillID)
				{
					case 2311007:
					case 2111007:
						return true;
				}
				v1 = nSkillID == 2211007;
			}
			return v1;
		}

		public static int get_novice_skill_as_race(NoviceSkillID nSkillID, int nJob)
		{
			int result; // eax

			if (nJob / 100 != 22 && nJob != 2001)
				result = (int)nSkillID + 10000000 * (nJob / 1000);
			else
				result = (int)nSkillID + 20010000;
			return result;
		}

		public enum NoviceSkillID : int
		{
			INVALID = 0,
			BlessingOfTheFairy = 12,
			FollowTheLead1 = 18,
			MonsterRiding = 1004,
			StatChangeAdmin = 1005,
			DamageMeter = 1006,
			MakerSkill = 1007,
			BambooRain = 1009,
			Invincibility = 1010,
			MeteoShower = 1011, // Power Explosion / Meteo Shower depending on job
			Mount_WitchsBroomstick1 = 1019, // adventurer, evan, resistance
			RageOfPharaoh = 1020,
			// Mount_YetiMount = 1022, // only koc and aran
			Mount_WitchsBroomstick2 = 1023, // aran, koc
			FollowTheLead2 = 1024,
			Mount_WoodenPony = 1025,
			Soaring = 1026, // Flying
			Mount_Croco = 1027,
			Mount_BlackScooter = 1028,
			Mount_PinkScooter = 1029,
			Mount_NimbusCloud = 1030,
			Mount_Balrog = 1031,
			Mount_ZDTiger = 1034,
			Mount_RaceKart = 1033, // only evan and resistance
			Mount_MistBalrod = 1035,
			Mount_Lion = 1036,
			Mount_Unicorn = 1037,
			Mount_LowRider = 1038,
			Mount_RedTruck = 1039,
			Mount_Gargoyle = 1040,
			// 1041 doesnt exist
			Mount_Shinjo = 1042,
			Mount_OrangeMushroom = 1044,
			Mount_Spaceship = 1046,
			SpaceDash = 1047,
			SpaceBeam = 1048,
			Mount_Nightmare = 1049,
			Mount_Yeti = 1050,
			Mount_Ostrich = 1051,
			Mount_PinkBearHotAirBalloon = 1052,
			Mount_TransformationRobot = 1053,
			Mount_Chicken = 1054,
			Capture = 1061, // only resistance
			CallOfTheHunter = 1062, // only resistance
			Mount_Motorcycle = 1063,
			Mount_PowerSuit = 1064,
			Mount_OS4Shuttle = 1065,
			VisitorMeleeAttack = 1066,
			VisitorRangeAttack = 1067,
			MechanicDash = 1068, // only resistance
			Mount_Owl = 1069,
			Mount_Mothership = 1070,
			Mount_OS3AMachine = 1071,
			DecentHaste = 8000,
			DecentMysticDoor = 8001,
			DecentSharpEyes = 8002,
			DecentHyperBody = 8003,
			PigsWeakness = 9000
		}

		public static int GetVehicleId(int nSkillID) // TODO make this a dictionary
		{
			switch (nSkillID)
			{
				case 5221006:
					return 1932000;
				case 33001001:
					return 1932015;
				case 35001002:
				case 35120000:
					return 1932016;
			}
			return 0;
		}

		public static NoviceSkillID GetSkillIdFromVehicleId(int nVehicleID) => (NoviceSkillID)get_skill_id_from_vehicle_id(nVehicleID);

		public static bool is_correct_job_for_skill_root(int nJob, int nSkillRoot)
		{
			if (nSkillRoot % 100 == 0) // if ( !(nSkillRoot % 100) )
			{
				return nSkillRoot / 100 == nJob / 100;
			}
			return nSkillRoot / 10 == nJob / 10 && nJob % 10 >= nSkillRoot % 10;
		}

		public static int get_skill_degree_from_skill_root(int nSkillRoot)
		{
			if (nSkillRoot == 0)
				return 0;
			if (nSkillRoot % 100 > 0)
				return nSkillRoot % 10 != 0 ? 0 : 2;
			return 1;
		}

		public static int get_skill_id_from_vehicle_id(int nVehicleID) // TODO make this a dictionary
		{
			int result; // eax

			switch (nVehicleID)
			{
				case 1932004:
					return 1050;
				case 1932006:
					return 1025;
				case 1932007:
					return 1027;
				case 1932008:
					return 1028;
				case 1932009:
					return 1029;
				case 1932010:
					result = 1031;
					break;
				case 1932011:
					result = 1030;
					break;
				case 1932012:
					result = 1035;
					break;
				case 1932013:
					result = 1033;
					break;
				case 1932014:
					result = 1034;
					break;
				case 1932017:
					result = 1036;
					break;
				case 1932018:
					result = 1037;
					break;
				case 1932019:
					result = 1038;
					break;
				case 1932020:
					result = 1039;
					break;
				case 1932021:
					result = 1040;
					break;
				case 1932022:
					result = 1042;
					break;
				case 1932023:
					result = 1044;
					break;
				case 1932025:
					result = 1049;
					break;
				case 1932026:
					result = 1051;
					break;
				case 1932027:
					result = 1052;
					break;
				case 1932028:
					result = 1053;
					break;
				case 1932029:
					result = 1054;
					break;
				case 1932034:
					result = 1063;
					break;
				case 1932035:
					result = 1064;
					break;
				case 1932037:
					result = 1065;
					break;
				case 1932038:
					result = 1069;
					break;
				case 1932039:
					result = 1070;
					break;
				case 1932040:
					result = 1071;
					break;
				default:
					result = 0;
					break;
			}
			return result;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_shoot_skill_not_consuming_bullet(int nSkillID)
		{
			if (is_shoot_skill_not_using_shooting_weapon(nSkillID))
				return true;
			if (nSkillID > 35001001)
			{
				if (nSkillID > 35111015)
				{
					if (nSkillID == 35121005 || nSkillID > 35121011 && nSkillID <= 35121013)
						return true;
					return false;
				}
				if (nSkillID != 35111015)
				{
					if (nSkillID <= 35101010)
					{
						if (nSkillID >= 35101009 || nSkillID == 35001004)
							return true;
						return false;
					}
					if (nSkillID != 35111004)
						return false;
				}
				return true;
			}
			if (nSkillID == 35001001)
				return true;
			if (nSkillID <= 13101005)
			{
				if (nSkillID == 13101005 || nSkillID == 3101003 || nSkillID == 3201003 || nSkillID == 4111004)
					return true;
				return false;
			}
			if (nSkillID == 14101006 || nSkillID == 33101002)
				return true;
			return false;
		}

		public static bool is_shoot_skill_not_using_shooting_weapon(int nSkillID)
		{
			bool v1; // zf

			if (nSkillID > 15111007)
			{
				if (nSkillID > 21120006)
				{
					v1 = nSkillID == 33101007;
				}
				else
				{
					if (nSkillID == 21120006 || nSkillID == 21100004)
						return true;
					v1 = nSkillID == 21110004;
				}
			}
			else
			{
				if (nSkillID >= 15111006)
					return false;
				if (nSkillID > 5121002)
				{
					v1 = nSkillID == 11101004;
				}
				else
				{
					if (nSkillID == 5121002 || nSkillID == 4121003)
						return true;
					v1 = nSkillID == 4221003;
				}
			}
			return v1;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_keydown_skill(int nSkillID)
		{
			bool v1; // zf

			if (nSkillID > 13111002)
			{
				if (nSkillID > 33101005)
				{
					if (nSkillID == 33121009 || nSkillID == 35001001)
						return true;
					v1 = nSkillID == 35101009;
				}
				else
				{
					if (nSkillID == 33101005)
						return true;
					if (nSkillID > 22121000)
					{
						v1 = nSkillID == 22151001;
					}
					else
					{
						if (nSkillID == 22121000 || nSkillID == 14111006)
							return true;
						v1 = nSkillID == 15101003;
					}
				}
				return v1;
			}
			if (nSkillID == 13111002)
				return true;
			if (nSkillID <= 3221001)
			{
				if (nSkillID == 3221001)
					return true;
				if (nSkillID > 2321001)
				{
					v1 = nSkillID == 3121004;
				}
				else
				{
					if (nSkillID == 2321001 || nSkillID == 2121001)
						return true;
					v1 = nSkillID == 2221001;
				}
				return v1;
			}
			if (nSkillID > 5201002)
			{
				v1 = nSkillID == 5221004;
				return v1;
			}
			if (nSkillID != 5201002)
			{
				if (nSkillID < 4341002)
					return false;
				if (nSkillID > 4341003)
				{
					v1 = nSkillID == 5101004;
					return v1;
				}
			}
			return true;
		}

		/// <summary>
		/// This is skill delay between attacks.
		/// </summary>
		/// <param name="nSkillID"></param>
		/// <returns></returns>
		public static int get_cool_time(int nSkillID)
		{
			bool v2; // zf

			if (nSkillID > 5211005)
			{
				if (nSkillID > 30001068)
				{
					if (nSkillID <= 33121005)
					{
						if (nSkillID == 33121005)
							return 1000;
						if (nSkillID == 32101001 || nSkillID == 32111011)
							return 500;
						if (nSkillID != 33101002)
							return 0;
						return 400;
					}
					if (nSkillID == 35001001)
						return 1000;
					v2 = nSkillID == 35101009;
				}
				else
				{
					if (nSkillID == 30001068)
						return 1000;
					if (nSkillID <= 15111004)
					{
						if (nSkillID == 15111004)
							return 200;
						if (nSkillID == 11101005)
							return 1500;
						if (nSkillID != 13101005)
						{
							if (nSkillID == 14111006)
								return 1500;
							return 0;
						}
						return 400;
					}
					if (nSkillID == 21001001)
						return 1000;
					v2 = nSkillID == 22141001;
				}
				return v2 ? 1000 : 0;
			}
			if (nSkillID >= 5211004)
				return 450;
			if (nSkillID > 4221007)
			{
				if (nSkillID <= 5121007)
				{
					if (nSkillID != 5121007)
					{
						if (nSkillID != 4321002)
						{
							if (nSkillID == 4341003)
								return 2000;
							if (nSkillID != 5121005)
								return 0;
							return 500;
						}
						return 1000;
					}
					return 200;
				}
				if (nSkillID == 5201002)
					return 1000;
				v2 = nSkillID == 5201006;
				return v2 ? 1000 : 0;
			}
			if (nSkillID == 4221007)
				return 200;
			if (nSkillID <= 3120010)
			{
				if (nSkillID != 3120010)
				{
					if (nSkillID != 1121001 && nSkillID != 1321001)
					{
						if (nSkillID != 3101003)
							return 0;
						return 400;
					}
					return 500;
				}
				return 1500;
			}
			if (nSkillID == 3201003)
				return 400;
			if (nSkillID != 4001003)
			{
				if (nSkillID == 4121008)
					return 500;
				return 0;
			}
			return 1000;
		}

		public static bool IsBeginnerSkill(int nSkillId) => (nSkillId / 10000 % 1000 == 0) || (nSkillId / 10000 == 2001);
		public static bool is_skill_static_data(int skillId) => skillId / 1000 == 1 || skillId / 9000000 == 1;

		public static bool is_bmage_aura_skill(int nSkillID)
		{
			if (nSkillID > 32110000)
			{
				if (nSkillID < 32120000 || nSkillID > 32120001)
					return false;
			}
			else if (nSkillID != 32110000 && nSkillID != 32001003 && (nSkillID <= 32101001 || nSkillID > 32101003))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Be careful when using this, this has already been assigned to each skill template. Use that if possible.
		/// </summary>
		public static int get_stance_skill_id(int nJob)
		{
			if (nJob <= 2112)
			{
				switch (nJob)
				{
					case 2112:
						return 21121003;
					case 112:
						return 1121002;
					case 122:
						return 1221002;
					case 132:
						return 1321002;
				}
				return 0;
			}
			if (nJob != 3212)
			{
				if (nJob > 3309 && nJob <= 3312)
					return 33101006;
				return 0;
			}
			return 32121005;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_heros_will_skill(int nSkillID)
		{
			bool v1; // zf

			if (nSkillID > 4221008)
			{
				if (nSkillID > 22171004)
				{
					if (nSkillID == 32121008 || nSkillID == 33121008)
						return true;
					v1 = nSkillID == 35121008;
				}
				else
				{
					if (nSkillID == 22171004)
						return true;
					if (nSkillID > 5221010)
					{
						v1 = nSkillID == 21121008;
					}
					else
					{
						if (nSkillID == 5221010 || nSkillID == 4341008)
							return true;
						v1 = nSkillID == 5121008;
					}
				}
			}
			else
			{
				if (nSkillID == 4221008)
					return true;
				if (nSkillID > 2221008)
				{
					if (nSkillID > 3221008)
					{
						v1 = nSkillID == 4121009;
					}
					else
					{
						if (nSkillID == 3221008 || nSkillID == 2321009)
							return true;
						v1 = nSkillID == 3121009;
					}
				}
				else
				{
					if (nSkillID == 2221008)
						return true;
					if (nSkillID > 1321010)
					{
						v1 = nSkillID == 2121008;
					}
					else
					{
						if (nSkillID == 1321010 || nSkillID == 1121011)
							return true;
						v1 = nSkillID == 1221012;
					}
				}
			}
			return v1;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_skill_need_master_level(int nSkillID)
		{
			int v2; // ecx
			uint v3; // edx
			uint v4; // esi
			int v5; // eax

			if (is_ignore_master_level_for_common(nSkillID))
				return false;

			v2 = nSkillID / 10000;
			v3 = (uint)((ulong)(1374389535 * (nSkillID / 10000)) >> 32) >> 5;
			v4 = v3 + (v3 >> 31);
			if (v4 == 22 || v2 == 2001)
			{
				v5 = JobLogic.get_job_level(nSkillID / 10000);
				if (v5 != 9 && v5 != 10 && nSkillID != 22111001 && nSkillID != 22141002 && nSkillID != 22140000)
					return false;
				return true;
			}
			if (v2 / 10 == 43)
			{
				if (JobLogic.get_job_level(nSkillID / 10000) == 4
				  || nSkillID == 4311003
				  || nSkillID == 4321000
				  || nSkillID == 4331002
				  || nSkillID == 4331005)
				{
					return true;
				}
				return false;
			}
			if (v2 == 100 * v4)
				return false;
			return v2 % 10 == 2;
		}

		public static bool is_ignore_master_level_for_common(int nSkillID)
		{
			bool v1; // zf

			if (nSkillID > 3220010)
			{
				if (nSkillID <= 5220012)
				{
					if (nSkillID == 5220012 || nSkillID == 4120010 || nSkillID == 4220009)
						return true;
					v1 = nSkillID == 5120011;
					return v1;
				}
				if (nSkillID != 32120009)
				{
					v1 = nSkillID == 33120010;
					return v1;
				}
				return true;
			}
			if (nSkillID >= 3220009)
				return true;
			if (nSkillID > 2120009)
			{
				if (nSkillID > 2320010)
				{
					if (nSkillID < 3120010 || nSkillID > 3120011)
						return false;
				}
				else if (nSkillID != 2320010)
				{
					v1 = nSkillID == 2220009;
					return v1;
				}
				return true;
			}
			if (nSkillID == 2120009 || nSkillID == 1120012 || nSkillID == 1220013)
				return true;

			return nSkillID == 1320011;
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_event_vehicle_skill(int skillId)
		{
			switch (skillId % 10000)
			{
				case 1025:
				case 1027:
				case 1028:
				case 1029:
				case 1030:
				case 1031:
				case 1033:
				case 1034:
				case 1035:
				case 1036:
				case 1037:
				case 1038:
				case 1039:
				case 1040:
				case 1042:
				case 1044:
				case 1049:
				case 1050:
				case 1051:
				case 1052:
				case 1053:
				case 1054:
				case 1063:
				case 1064:
				case 1065:
				case 1069:
				case 1070:
				case 1071:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Do not use this, this has already been assigned to each skill template. Use that.
		/// </summary>
		public static bool is_antirepeat_buff_skill(int SkillID)
		{
			if (SkillID > 4111001)
			{
				if (SkillID <= 11101004)
				{
					if (SkillID == 11101004) return true;
					if (SkillID > 5111007)
					{
						return SkillID > 5211007 && (SkillID == 5221000 || SkillID == 11001001);
					}
					else return SkillID == 5211007 || SkillID == 5121000 || SkillID == 5121007;
				}
				else
				{
					if (SkillID == 5111007) return true;
					else return SkillID > 4311001 ? SkillID == 4341000 || (SkillID - 4341000 == 7) : SkillID == 5111007 || SkillID == 4121000 || SkillID == 4201003;
				}
			}
			else if (SkillID == 2121000) return true;
			else if (SkillID <= 1221000)
			{
				if (SkillID == 1221000) return true;
				else if (SkillID > 1121000)
				{
					return SkillID == 1201006 || (SkillID > 1211008 && SkillID <= 1211010);
				}
				else return SkillID == 1121000 || SkillID == 1001003 || SkillID == 1101006 || SkillID == 1111007;
			}
			else if (SkillID > 1321000)
			{
				return SkillID == 2101001 || SkillID == 2101003;
			}
			else if (SkillID == 1321000) return true;
			else if (SkillID >= 1301006) return SkillID <= 1301007 || SkillID == 1311007;
			else return false;
		}

		public static int get_max_gauge_time(int nSkillID)
		{
			if (is_keydown_skill(nSkillID)) return 0;

			if (nSkillID <= 5101004)
			{
				if (nSkillID == 5101004) return 1000;

				if (nSkillID <= 3221001)
				{
					if (nSkillID == 3221001) return 900;

					if (nSkillID != 2121001 && nSkillID != 2221001 && nSkillID != 2321001)
						return 2000;

					return 1000;
				}
				switch (nSkillID)
				{
					case 4341002:
						return 600;
					case 4341003:
						return 1200;
					default:
						return 2000;
				}
			}
			if (nSkillID > 22121000)
			{
				if (nSkillID != 22151001)
				{
					return nSkillID == 33101005 ? 900 : 2000;
				}
			}
			else if (nSkillID != 22121000)
			{
				if (nSkillID == 5201002 || nSkillID == 14111006 || nSkillID == 15101003)
					return 1000;
				return 2000;
			}

			return 500;
		}
	}
}
