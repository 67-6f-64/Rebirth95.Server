using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Characters;
using Rebirth.Characters.Skill;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;

namespace Rebirth.Game
{
	public static class SkillStatLogic // TODO put this is CharacterSkills?
	{
		public static int get_amplification(Character cd, int nSkillID, out int pnIncMPCon)
		{
			pnIncMPCon = 100;

			SkillEntry pSkill;

			switch (cd.Stats.nJob)
			{
				case 211:
					pSkill = cd.Skills.Get(Skills.MAGE1_ELEMENT_AMPLIFICATION);
					break;
				case 221:
					pSkill = cd.Skills.Get(Skills.MAGE2_ELEMENT_AMPLIFICATION);
					break;
				case 1211:
					pSkill = cd.Skills.Get(Skills.FLAMEWIZARD_ELEMENT_AMPLIFICATION);
					break;
				case 2215:
					pSkill = cd.Skills.Get(Skills.EVAN_ELEMENT_AMPLIFICATION);
					break;
				default:
					pSkill = null;
					break;
			}

			if (pSkill is null) return 100;

			// TODO clean this up and put in a switch statement

			if (nSkillID != 0)
			{
				if (nSkillID > 12101002)
				{
					if (nSkillID > 22121000)
					{
						if (nSkillID > 22161001)
						{
							if (nSkillID > 22181002 ||
								nSkillID < 22181001 && (nSkillID < 22171002 || nSkillID > 22171003))
							{
								return (int)pSkill.Y_Effect;
							}
						}
						else if (nSkillID != 22161001)
						{
							if (nSkillID > 22141001)
							{
								if (nSkillID < 22151001 || nSkillID > 22151002)
								{
									return (int)pSkill.Y_Effect;
								}
							}
							else if (nSkillID != 22141001 && nSkillID != 22131000)
							{
								return (int)pSkill.Y_Effect;
							}
						}
					}
					else if (nSkillID != 22121000)
					{
						if (nSkillID > 22001001)
						{
							if (nSkillID != 22101000 && nSkillID != 22111000)
							{
								return (int)pSkill.Y_Effect;
							}
						}
						else if (nSkillID != 22001001)
						{
							if (nSkillID > 12111003)
							{
								if (nSkillID < 12111005 || nSkillID > 12111006)
								{
									return (int)pSkill.Y_Effect;
								}
							}
							else if (nSkillID != 12111003 && nSkillID != 12101006)
							{
								return (int)pSkill.Y_Effect;
							}
						}
					}
				}
				else if (nSkillID != 12101002)
				{
					if (nSkillID > 2201005)
					{
						if (nSkillID > 2221003)
						{
							if (nSkillID < 2221006 || nSkillID > 2221007 && nSkillID != 12001003)
							{
								return (int)pSkill.Y_Effect;
							}
						}
						else if (nSkillID != 2221003)
						{
							if (nSkillID > 2211006)
							{
								if (nSkillID != 2221001)
								{
									return (int)pSkill.Y_Effect;
								}
							}
							else if (nSkillID != 2211006 && (nSkillID < 2211002 || nSkillID > 2211003))
							{
								return (int)pSkill.Y_Effect;
							}
						}
					}
					else if (nSkillID < 2201004)
					{
						if (nSkillID > 2111006)
						{
							switch ((Skills)nSkillID)
							{
								case Skills.ARCHMAGE1_BIGBANG:
								case Skills.ARCHMAGE1_FIRE_DEMON:
								case Skills.ARCHMAGE1_PARALYZE:
								case Skills.ARCHMAGE1_METEOR:
									pnIncMPCon = (int)pSkill.X_Effect;
									break;
								default:
									return (int)pSkill.Y_Effect;
							}

							return (int)pSkill.Y_Effect;
						}

						if (nSkillID != 2111006)
						{
							if (nSkillID > 2101005)
							{
								if (nSkillID < 2111002 || nSkillID > 2111003)
								{
									return (int)pSkill.Y_Effect;
								}
							}
							else if (nSkillID < 2101004 && (nSkillID < 2001004 || nSkillID > 2001005))
							{
								return (int)pSkill.Y_Effect;
							}
						}
					}
				}
			}

			pnIncMPCon = (int)pSkill.X_Effect;

			return (int)pSkill.Y_Effect;
		}

		public static int get_mastery_from_skill(Character cd, Skills nSkillID, out int pnInc)
		{
			var mastery = 0;
			pnInc = 0;

			if (cd.Skills.Get(nSkillID) is SkillEntry se)
			{
				pnInc = (int)se.X_Effect;
				mastery = se.Template.Mastery(se.nSLV);
			}

			return mastery;
		}

		public static int get_shield_mastery(Character cd)
		{
			if (SkillLogic.is_correct_job_for_skill_root(cd.Stats.nJob, 121))
			{
				if (cd.Skills.Get(Skills.KNIGHT_SHIELD_MASTERY) is SkillEntry se)
				{
					return (int)se.X_Effect;
				}
			}
			else if (SkillLogic.is_correct_job_for_skill_root(cd.Stats.nJob, 421))
			{
				if (cd.Skills.Get(Skills.THIEFMASTER_SHIELD_MASTERY) is SkillEntry se)
				{
					return (int)se.X_Effect;
				}
			}

			return 0;
		}

		public static int get_magic_mastery(Character cd, out int nMADInc)
		{
			int mastery = 0;
			nMADInc = 0;

			var nJobType = cd.Stats.nJob / 10;

			switch (nJobType)
			{
				case 21:
					mastery = get_mastery_from_skill(cd, Skills.WIZARD1_SPELL_MASTERY, out nMADInc);
					break;
				case 22:
					mastery = get_mastery_from_skill(cd, Skills.WIZARD2_SPELL_MASTERY, out nMADInc);
					break;
				case 23:
					mastery = get_mastery_from_skill(cd, Skills.CLERIC_SPELL_MASTERY, out nMADInc);
					break;
				case 120:
				case 121:
					mastery = get_mastery_from_skill(cd, Skills.FLAMEWIZARD_SPELL_MASTERY, out nMADInc);
					break;
				case 220:
				case 221:
					mastery = get_mastery_from_skill(cd, Skills.EVAN_MAGIC_MASTERY, out nMADInc);

					if (mastery == 0)
					{
						var subMadInc = nMADInc;
						mastery = get_mastery_from_skill(cd, Skills.EVAN_SPELL_MASTERY, out nMADInc);
						nMADInc += subMadInc;
					}
					break;
				case 320:
				case 321:
					mastery = get_mastery_from_skill(cd, Skills.BMAGE_SPELL_MASTERY, out nMADInc);
					break;
			}

			return mastery;
		}

		public static int get_weapon_mastery(Character cd, int nWeaponItemID, int nAttackType, int nSkillID,
			out int pnACCInc,
			out int pnPADInc)
		{
			pnACCInc = 0;
			pnPADInc = 0;
			var mastery = 0;

			// TODO clean this function up -> ugly af :(

			switch ((WeaponType)ItemConstants.get_weapon_type(nWeaponItemID))
			{
				case WeaponType.WT_OH_SWORD:
				case WeaponType.WT_TH_SWORD:
					if (nAttackType > 0)
					{
						if (nSkillID != (int)Skills.SOULMASTER_SOUL_BLADE) return mastery;
						if (nAttackType != 1) return mastery;
					}

					mastery = get_mastery_from_skill(cd, Skills.FIGHTER_WEAPON_MASTERY, out pnACCInc);

					if (mastery == 0)
					{
						mastery = get_mastery_from_skill(cd, Skills.PAGE_WEAPON_MASTERY, out pnACCInc);
						if (mastery != 0)
						{
							if (cd.Stats.SecondaryStats.rWeaponCharge != 0)
							{
								var tempmastery = get_mastery_from_skill(cd,
									Skills.PALADIN_ADVANCED_CHARGE,
									out var pnSubACCInc);

								if (tempmastery > 0)
								{
									pnACCInc = pnSubACCInc;
									mastery = tempmastery;
								}
							}
						}
						else
						{
							mastery = get_mastery_from_skill(cd, Skills.SOULMASTER_SWORD_MASTERY, out pnACCInc);
						}
					}

					break;
				case WeaponType.WT_OH_AXE:
				case WeaponType.WT_TH_AXE:
					if (nAttackType != 0) return 0;
					mastery = get_mastery_from_skill(cd, Skills.FIGHTER_WEAPON_MASTERY, out pnACCInc);
					break;
				case WeaponType.WT_OH_MACE:
				case WeaponType.WT_TH_MACE:
					if (nAttackType != 0) return 0;

					mastery = get_mastery_from_skill(cd, Skills.PAGE_WEAPON_MASTERY, out pnACCInc);
					if (cd.Stats.SecondaryStats.rWeaponCharge != 0)
					{
						var tempmastery = get_mastery_from_skill(cd,
							Skills.PALADIN_ADVANCED_CHARGE,
							out var pnSubACCInc);

						if (tempmastery > 0)
						{
							pnACCInc = pnSubACCInc;
							mastery = tempmastery;
						}
					}
					break;
				case WeaponType.WT_DAGGER:
					if (nAttackType != 0) return 0;

					var pSubDaggerItemSlot = cd.EquippedInventoryNormal.Get((short)BodyPart.BP_SHIELD);
					var bSubDagger = ItemConstants.get_weapon_type(pSubDaggerItemSlot.nItemID) == (int)WeaponType.WT_SUB_DAGGER;

					mastery = get_mastery_from_skill(cd,
						bSubDagger
							? Skills.DUAL1_DUAL_MASTERY
							: Skills.THIEF_DAGGER_MASTERY,
						out pnACCInc);
					break;
				case WeaponType.WT_SPEAR:
					if (nAttackType != 0) return 0;

					mastery = get_mastery_from_skill(cd, Skills.SPEARMAN_WEAPON_MASTERY, out pnACCInc);
					break;
				case WeaponType.WT_POLEARM:
					if (nAttackType != 0 && (nSkillID != (int)Skills.ARAN_COMBO_SMASH
											 && nSkillID != (int)Skills.ARAN_COMBO_FENRIR
											 && nSkillID != (int)Skills.ARAN_COMBO_TEMPEST
											 || nAttackType != 1))
					{
						return mastery;
					}

					if (JobLogic.isAran(cd.Stats.nSubJob))
					{
						mastery = get_mastery_from_skill(cd, Skills.ARAN_POLEARM_MASTERY, out pnACCInc);
						var highMastery = get_mastery_from_skill(cd, Skills.ARAN_HIGH_MASTERY, out pnPADInc);
						if (highMastery != 0)
						{
							mastery = highMastery;
						}
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.SPEARMAN_WEAPON_MASTERY, out pnACCInc);

						if (cd.Stats.SecondaryStats.rBeholder != 0)
						{
							mastery += get_mastery_from_skill(cd, Skills.DARKKNIGHT_BEHOLDER, out _);
						}
					}
					break;
				case WeaponType.WT_BOW:
					if (nAttackType != 1) return mastery;

					int bowexpert;
					if (JobLogic.IsKOC(cd.Stats.nJob))
					{
						mastery = get_mastery_from_skill(cd, Skills.WINDBREAKER_BOW_MASTERY, out pnACCInc);
						bowexpert = get_mastery_from_skill(cd, Skills.WINDBREAKER_BOW_EXPERT, out pnPADInc);
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.HUNTER_BOW_MASTERY, out pnACCInc);
						bowexpert = get_mastery_from_skill(cd, Skills.BOWMASTER_BOW_EXPERT, out pnPADInc);
					}

					if (bowexpert != 0) mastery = bowexpert;

					break;
				case WeaponType.WT_CROSSBOW:

					var bWildhunterJob = JobLogic.IsWildhunterJob(cd.Stats.nJob);
					if (nAttackType != 1 && (!bWildhunterJob || nAttackType != 0))
					{
						return mastery;
					}

					int xbowexpert;
					if (bWildhunterJob)
					{
						mastery = get_mastery_from_skill(cd, Skills.WILDHUNTER_CROSSBOW_MASTERY, out pnACCInc);
						xbowexpert = get_mastery_from_skill(cd, Skills.WILDHUNTER_CROSSBOW_EXPERT, out pnPADInc);
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.CROSSBOWMAN_CROSSBOW_MASTERY, out pnACCInc);
						xbowexpert = get_mastery_from_skill(cd, Skills.CROSSBOWMASTER_CROSSBOW_EXPERT, out pnPADInc);
					}

					if (xbowexpert != 0) mastery = xbowexpert;

					break;
				case WeaponType.WT_THROWINGGLOVE:
					if (nAttackType != 1 && (nSkillID != (int)Skills.NIGHTWALKER_POISON_BOMB || nAttackType != 0))
					{
						return mastery;
					}

					if (JobLogic.IsKOC(cd.Stats.nJob))
					{
						mastery = get_mastery_from_skill(cd, Skills.NIGHTWALKER_JAVELIN_MASTERY, out pnACCInc);
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.ASSASSIN_JAVELIN_MASTERY, out pnACCInc);
					}
					break;
				case WeaponType.WT_KNUCKLE:

					if (nAttackType != 0 && (nSkillID != (int)Skills.VIPER_ENERGY_ORB && nSkillID != (int)Skills.STRIKER_SHARK_WAVE || nAttackType != 1))
					{
						return mastery;
					}

					if (JobLogic.IsKOC(cd.Stats.nJob))
					{
						mastery = get_mastery_from_skill(cd, Skills.STRIKER_KNUCKLE_MASTERY, out pnACCInc);
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.INFIGHTER_KNUCKLE_MASTERY, out pnACCInc);
					}
					break;
				case WeaponType.WT_GUN:
					if (nAttackType != 1 && (nSkillID != (int)Skills.CAPTAIN_AIR_STRIKE || nAttackType != 0))
					{
						return mastery;
					}

					if (JobLogic.is_mechanic_job(cd.Stats.nJob))
					{
						mastery = get_mastery_from_skill(cd, Skills.MECHANIC_HN07_UPGRADE, out pnACCInc);
						if (mastery == 0)
						{
							mastery = get_mastery_from_skill(cd, Skills.MECHANIC_HN07, out pnACCInc);
						}
					}
					else
					{
						mastery = get_mastery_from_skill(cd, Skills.GUNSLINGER_GUN_MASTERY, out pnACCInc);
					}
					break;
			}
			return mastery;
		}
	}
}
