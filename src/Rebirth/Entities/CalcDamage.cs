using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Rebirth.Characters;
using Rebirth.Characters.Combat;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Stat;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Provider.Template.Mob;
using Rebirth.Server.Center;
using Math = System.Math;

namespace Rebirth.Entities
{
	public class CalcDamage
	{
		private const int RND_SIZE = 7;
		private const int AT_MAGIC = 2;

		public CRand32 m_RndGenForCharacter { get; private set; }
		public CRand32 m_RndForCheckDamageMiss { get; private set; }
		public CRand32 m_RndForMortalBlow { get; private set; }
		public CRand32 m_RndForSummoned { get; private set; }
		public CRand32 m_RndForMob { get; private set; }
		public CRand32 m_RndGenForMob { get; private set; }

		private int _invalidCount;

		public int InvalidCount
		{
			get => _invalidCount;
			set
			{
				_invalidCount = value;
				if (_invalidCount < 0) _invalidCount = 0;
				if (_invalidCount > 50) _invalidCount = 0;
			}
		}

		// CalcDamage::CalcDamage
		public CalcDamage()
		{
			m_RndGenForCharacter = new CRand32();
			m_RndForCheckDamageMiss = new CRand32();
			m_RndForMortalBlow = new CRand32();
			m_RndForSummoned = new CRand32();
			m_RndForMob = new CRand32();
			m_RndGenForMob = new CRand32();
		}

		public void Dispose()
		{
			m_RndGenForCharacter = null;
			m_RndForCheckDamageMiss = null;
			m_RndForMortalBlow = null;
			m_RndForSummoned = null;
			m_RndForMob = null;
			m_RndGenForMob = null;
		}

		public void SetSeed(uint s1, uint s2, uint s3)
		{
			m_RndGenForCharacter.Seed(s1, s2, s3);
			m_RndForCheckDamageMiss.Seed(s1, s2, s3);
			m_RndForMortalBlow.Seed(s1, s2, s3);
			m_RndForSummoned.Seed(s1, s2, s3);
			m_RndForMob.Seed(s1, s2, s3);
			m_RndGenForMob.Seed(s1, s2, s3);
		}

		public bool CheckMDamageMiss(Character cd, CMob mob, uint nRandForMissCheck)
		{
			if (mob.Template.CannotEvade) return false;

			var nBaseEVA = cd.BasicStats.nINT + 2 * cd.BasicStats.nLUK;
			var nCharEVA = cd.Stats.SecondaryStats.GetEVA(cd, cd.Stats.PassiveSkillData.nEVAr, nBaseEVA, out _);

			var nMobACC = mob.Stats.nACC; // pdb combines nACC and nACC_ but we do this in MobStat

			var nRandMiss = get_rand(nRandForMissCheck, 100.0, 0.0);

			return calc_evar(nCharEVA, nMobACC, cd.Stats.nLevel, mob.Template.Level, cd.Stats.PassiveSkillData.nEr)
				   >= nRandMiss;
		}

		public bool CheckPDamageMiss(Character cd, CMob mob, uint nRandForMissCheck)
		{
			if (mob.Template.CannotEvade) return false;

			if (cd.Stats.SecondaryStats.nBlessingArmor > 0) return true;

			var nBaseEVA = cd.BasicStats.nDEX + 2 * cd.BasicStats.nLUK;
			var nCharEVA = cd.Stats.SecondaryStats.GetEVA(cd, cd.Stats.PassiveSkillData.nEVAr, nBaseEVA, out _);

			var nMobACC = mob.Stats.nACC; // pdb combines nACC and nACC_ but we do this in MobStat

			var nRandMiss = get_rand(nRandForMissCheck, 100.0, 0.0);

			return calc_evar(nCharEVA, nMobACC, cd.Stats.nLevel, mob.Template.Level, cd.Stats.PassiveSkillData.nEr)
				   >= nRandMiss;
		}

		public int GetCounterDamage(CMob ms, int nAT, int tAttackTime, int nMobCount, int nDamagePerMob, int nSkillID, int nAction)
		{
			if (ms is null || ms.Stats.nCounterProb > 0 && Constants.Rand.Next() % 100 >= ms.Stats.nCounterProb)
				return 0;

			int nCounterOption, wCounterOption;

			if (nAT == AT_MAGIC)
			{
				nCounterOption = ms.Stats.nMCounter;
				wCounterOption = ms.Stats.wMCounter;
			}
			else
			{
				nCounterOption = ms.Stats.nPCounter;
				wCounterOption = ms.Stats.wPCounter;
			}

			if (wCounterOption <= 0) return nCounterOption;

			var rand = new CRand32();
			rand.SetSeed((uint)tAttackTime, (uint)(nSkillID + tAttackTime + nMobCount), (uint)(tAttackTime + nDamagePerMob + nAction + 10));

			var result = nCounterOption + rand.Random() % wCounterOption - wCounterOption / 2;
			return result >= int.MaxValue ? int.MaxValue : (int)result;
		}

		public int GetMesoGuardReduce(Character cd, double damage)
		{
			var pSkill = cd.Skills.Get(Skills.THIEFMASTER_MESO_GUARD);

			if (pSkill is null) return 0;

			if (damage < 1.0) damage = 1.0;

			var nX = pSkill.X_Effect;
			var pGridEffect = 50;

			var pGridSkill = cd.Skills.Get(Skills.SHADOWER_GRID);

			if (pGridSkill != null)
			{
				pGridEffect = (int)(pGridSkill.Template.V(pGridSkill.nSLV) + 50);

				if (pGridEffect > 100) pGridEffect = 100;
				else if (pGridEffect < 50) pGridEffect = 50;

				var wGrid = nX - pGridSkill.Template.W(pGridSkill.nSLV);

				if (wGrid < 0) wGrid = 0;

				if (wGrid > nX) wGrid = nX;

				nX = wGrid;
			}

			var nRealDamage = damage * pGridEffect / 100;

			if (cd.Stats.nMoney >= nRealDamage * nX / 100)
			{
				return (int)nRealDamage;
			}

			return (int)(100 * cd.Stats.nMoney / nX);
		}

		public bool IsCounterAttackHit(Character cd, CMob ms, bool bInvincible, int bMagicAttack)
		{
			if (bInvincible) return false;

			var nBasePACC = cd.BasicStats.CalcBasePACC();
			var nACC = cd.Stats.SecondaryStats.GetACC(cd, cd.Stats.PassiveSkillData.nACCr, nBasePACC, out _);
			var nMobEVA = ms.Stats.nEVA; // pdb combines EVA and EVA_ but we do that in MobStat when value is generated

			return calc_accr(nACC, nMobEVA, cd.Stats.nLevel, ms.Template.Level, cd.Stats.PassiveSkillData.nAr)
				   != 0;
		}

		public void LoadStandardPDD()
		{
			// TODO determine what this does
		}

		// =======================================================

		public void MDamage(Character cd, CMob mob, SkillEntry pSkill, MapleAttack pAttackEntry, int nWeaponItemID, out int[] aDamage, out int[] abCritical)
		{
			var aRandom = new uint[RND_SIZE];

			//cd.SendMessage($"Rand Before MDamage: S1 {(int)m_RndGenForCharacter.m_s1} | S2 {(int)m_RndGenForCharacter.m_s2} | S3 {(int)m_RndGenForCharacter.m_s3}");

			for (var i = 0; i < aRandom.Length; i++)
			{
				aRandom[i] = m_RndGenForCharacter.Random();
			}

			//cd.SendMessage($"Rand After MDamage:  S1 {(int)m_RndGenForCharacter.m_s1} | S2 {(int)m_RndGenForCharacter.m_s2} | S3 {(int)m_RndGenForCharacter.m_s3}");

			var nCr = cd.Stats.PassiveSkillData.nCr;
			var nMDamr = cd.Stats.PassiveSkillData.nMDamr;
			var nMADr = cd.Stats.PassiveSkillData.nMADr;
			var nACCr = cd.Stats.PassiveSkillData.nACCr;
			var nCDMin = cd.Stats.PassiveSkillData.nCDMin;
			var nDIPr = cd.Stats.PassiveSkillData.nDIPr;
			var nAr = cd.Stats.PassiveSkillData.nAr;
			var nMADx = cd.Stats.PassiveSkillData.nMADx;
			var nIMPr = cd.Stats.PassiveSkillData.nIMPr;

			if (pSkill != null)
			{
				if (cd.Stats.PassiveSkillData.AdditionPsd.ContainsKey(pSkill.nSkillID))
				{
					var apsd = cd.Stats.PassiveSkillData.AdditionPsd[pSkill.nSkillID];
					nCDMin += apsd.nCDMin;
					nAr += apsd.nAr;
					nDIPr += apsd.nDIPr;
					nIMPr += apsd.nIMPr;
					nCr += apsd.nCr;
					nMDamr += apsd.nMDamr;
				}
			}

			var nACC = cd.Stats.SecondaryStats.GetACC(cd, nACCr, cd.BasicStats.CalcBaseMACC(), out _);
			var nWT = ItemConstants.get_weapon_type(nWeaponItemID);
			var nMAD = cd.Stats.SecondaryStats.GetMAD(nMADx, nMADr, cd.Skills.nDragonFury);
			var nMagicMastery = SkillStatLogic.get_magic_mastery(cd, out _);

			//cd.SendMessage("nACC " + nACC);
			//cd.SendMessage("nWT " + nWT);
			//cd.SendMessage("nMAD " + nMAD);

			if (nMagicMastery == 0)
			{
				nMagicMastery = pSkill?.Template.Mastery(pSkill.nSLV) ?? 0;
			}

			//cd.SendMessage("nMagicMastery " + nMagicMastery);

			var nAmp = SkillStatLogic.get_amplification(cd, 0, out _);

			//cd.SendMessage("nAmp " + nAmp);

			var nAccRcalc = calc_accr(nACC, mob.Stats.nEVA, cd.Stats.nLevel, mob.Template.Level, nAr);
			var nWTMasteryConst = GetMasteryConstByWT(nWT);
			var nWTCalcDamage = CalcDamageByWT(cd, nWT, 0, nMAD);
			var nSkillDMG = pSkill?.Template.Damage(pSkill.nSLV) ?? 0;

			var nCritProp = 5;
			var nCritParam = 0;

			if (JobLogic.IsEvan(cd.Stats.nSubJob))
			{
				if (cd.Skills.Get(Skills.EVAN_MAGIC_CRITICAL) is SkillEntry pMagicCrit)
				{
					nCritProp += pMagicCrit.Template.Prop(pMagicCrit.nSLV);
					nCritParam = pMagicCrit.Template.Damage(pMagicCrit.nSLV);
				}
			}

			nCritProp += cd.Stats.SecondaryStats.nSharpEyes >> 8; // strip off lower order bits
			nCritParam += cd.Stats.SecondaryStats.nSharpEyes & 0xF; // strip off higher order bits

			nCritProp += cd.Stats.SecondaryStats.nThornsEffect >> 8;
			nCritParam += cd.Stats.SecondaryStats.nThornsEffect & 0xF;

			// TODO -> this comes from ItemSets
			var cdcriticalnProb = 0; // cd->critical.nProb
			var cdcriticalnDamage = 0;  // cd->critical.nDamage

			nCritProp += cd.Stats.SecondaryStats.ItemPotential.nCr;

			nCritParam += cd.Stats.SecondaryStats.ItemPotential.nCDr * (cdcriticalnDamage + nCritParam) / 100 + cdcriticalnDamage;

			//cd.SendMessage("nCritParam " + nCritParam);

			nCritProp += nCr;

			//cd.SendMessage("nCritProp " + nCritProp);

			aDamage = new int[pAttackEntry.nDamagePerMob];
			abCritical = new int[pAttackEntry.nDamagePerMob];

			var nIdx = 0;

			for (var i = 0; i < pAttackEntry.nDamagePerMob; i++)
			{
				if (mob.Template.Invincible)
				{
					//cd.SendMessage("Setting invincible dmg: " + aDamage[i]);
					continue;
				}

				uint nRandInt = aRandom[nIdx++ % RND_SIZE] % 100;

				if (mob.Stats[MobStatType.MImmune] is MobStatEntry pMImmune)
				{
					if (nRandInt > cd.Stats.SecondaryStats.nRespectMImmune)
					{
						aDamage[i] = 1;
						//cd.SendMessage("Setting dmg: " + aDamage[i]);
						continue;
					}
				}

				if (cd.Stats.SecondaryStats.nSeal != 0)
				{
					//cd.SendMessage("Setting seal dmg: " + aDamage[i]);
					continue;
				}

				nRandInt = aRandom[nIdx++ % RND_SIZE];

				double nRand = get_rand(nRandInt, 100.0, 0.0);

				if (nAccRcalc < nRand)
				{
					//cd.SendMessage($"nAccRcalc < nRand ({nAccRcalc} < {nRand}). Dmg: " + aDamage[i]);
					continue;
				}

				if (pSkill != null)
				{
					if (pSkill.Template.FixDamage(pSkill.nSLV) != 0)
					{
						aDamage[i] = pSkill.Template.FixDamage(pSkill.nSLV);
						//cd.SendMessage("Setting fixed dmg: " + aDamage[i]);
						continue;
					}
				}

				nRandInt = aRandom[nIdx++ % RND_SIZE];

				var nRandomAdjustedDamage = adjust_random_damage(nWTCalcDamage, nRandInt, nWTMasteryConst, nMagicMastery);

				nRandomAdjustedDamage = (nRandomAdjustedDamage + nMDamr * nRandomAdjustedDamage / 100.0) * nAmp / 100;

				//if (pSkill)
				//	nElemBoost = cd->aElemBoost[pSkill->nAttackElemAttr];
				//else
				var nElemBoost = 0; // TODO this has something to do with elemental boost from equips, maybe potential

				var nElemBuffAdjust = cd.Stats.SecondaryStats.nElementalReset;

				nRandomAdjustedDamage = get_damage_adjusted_by_elemAttr(nRandomAdjustedDamage,
					pSkill,
					mob.Stats.aDamagedElemAttr,
					1.0 - nElemBuffAdjust / 100.0,
					nElemBoost / 100.0);

				var nTotalIMPr = nIMPr + cd.Stats.SecondaryStats.ItemPotential.nIgnoreTargetDEF;

				if (nTotalIMPr > 100) nTotalIMPr = 100;

				var nMobTotalMDD = mob.Stats.nMDR * nTotalIMPr / -100 + mob.Stats.nMDR;

				nRandomAdjustedDamage *= (100.0 - nMobTotalMDD) / 100.0;

				if (mob.Stats[MobStatType.MGuardUp] is MobStatEntry pMGuard)
				{
					nRandomAdjustedDamage *= pMGuard.nOption / 100.0;
				}

				if (nSkillDMG > 0)
				{
					nRandomAdjustedDamage = nSkillDMG / 100.0 * nRandomAdjustedDamage;
				}

				if (cd.m_bNextAttackCritical || nCritProp > 0 && get_rand(aRandom[nIdx++ % RND_SIZE], 0.0, 100.0) <= nCritProp)
				{
					cd.m_bNextAttackCritical = false;

					var nTotalCDMin = nCritParam + nCDMin + 20;
					if (nTotalCDMin > 50) nTotalCDMin = 50;

					var nCritDamage = get_rand(aRandom[nIdx++ % RND_SIZE], nTotalCDMin, 50.0);

					abCritical[i] = 1;

					nRandomAdjustedDamage = nCritDamage / 100.0 * nRandomAdjustedDamage + nRandomAdjustedDamage;
				}

				// TODO ItemEffect
				//nCriticalDamage = cd->aMobCategoryDamage[_ZtlSecureFuse<long>(
				//	                  pTemplate->_ZtlSecureTear_nCategory,
				//	                  pTemplate->_ZtlSecureTear_nCategory_CS)]
				//                  + 100;
				//v89 = pTemplate->_ZtlSecureTear_bBoss_CS;
				//*(&nAmp + 4) = nCriticalDamage / 100.0 * *(&nAmp + 4);

				if (mob.Template.Boss)
				{
					nRand = get_rand(aRandom[nIdx++ % 7], 0.0, 100.0);

					var nBossProb = 0;

					if (nBossProb > nRand) // TODO item boss dmg effect
					{
						nRandomAdjustedDamage *= (nBossProb + 100) / 100.0;
					}
				}

				if (pAttackEntry.tKeyDown > 0)
				{
					var nGaugeMod = 90 * pAttackEntry.tKeyDown / SkillLogic.get_max_gauge_time(pSkill.nSkillID) + 10;
					nRandomAdjustedDamage = nGaugeMod / 100.0 * nRandomAdjustedDamage;
				}

				var nHardSkin = mob.Stats[MobStatType.HardSkin]?.nOption ?? 0;

				if (nHardSkin != 0 && abCritical[i] == 0) continue;

				nRandomAdjustedDamage += ApplyGuidedBulletDamage(cd, mob.nMobTemplateId);

				if (cd.Stats.SecondaryStats.nDojangBerserk != 0)
				{
					nRandomAdjustedDamage *= 2;
				}

				if (mob.Stats[MobStatType.Weakness] is MobStatEntry pWeakness)
				{
					nRandomAdjustedDamage *= (pWeakness.nOption + 100) / 100.0;
				}

				if (cd.Field.AffectedAreas.TryGetValue((int)Skills.MECHANIC_AR_01, out var pMechAr01))
				{
					if (pMechAr01.dwOwnerId == cd.dwId || (cd.Party?.Contains(cd.dwId) ?? false))
					{
						nRandomAdjustedDamage *=
							MasterManager.SkillTemplates[(int)Skills.MECHANIC_AR_01].Y(pMechAr01.nSLV)
							/ 100.0;
					}
				}

				if (pSkill?.nSkillID == (int)Skills.ARCHMAGE1_PARALYZE)
				{
					if (pAttackEntry.nMobCount > 1)
					{
						nRandomAdjustedDamage *= (100 - pSkill.X_Effect * (pAttackEntry.nMobCount - 1)) / 100.0;
					}
				}

				if (!mob.Template.Boss)
				{
					var nMHP = mob.Template.MaxHP;

					if (nMHP > nRandomAdjustedDamage && i == 0)
					{
						//TODO deadlyattack
						var nDeadlyAttackProp = 0;
						//for (i = 0; ; ++i)
						//{
						//	v99 = cd->aSkill.a;
						//	if (!v99 || i >= v99[-1].nSLV)
						//		break;
						//	if (v99[i].nSkillID == UNRECORDED_DEADLYATTACK)
						//	{
						//		v100 = CSkillInfo::GetSkill(TSingleton < CSkillInfo >::ms_pInstance, UNRECORDED_DEADLYATTACK);
						//		if (v100)
						//		{
						//			v101 = SKILLENTRY::GetLevelData(v100, cd->aSkill.a[i].nSLV);
						//			v97 += _ZtlSecureFuse<long>(v101->_ZtlSecureTear_nProp, v101->_ZtlSecureTear_nProp_CS);
						//		}
						//	}
						//}

						nRand = get_rand(aRandom[nIdx++ % 7], 0.0, 100.0);
						if (nDeadlyAttackProp > nRand)
						{
							nRandomAdjustedDamage = mob.Template.MaxHP;
						}
					}
				}

				var nTotalDAMr = nDIPr
								 + cd.Stats.SecondaryStats.ItemPotential.nDamR
								 + cd.Stats.SecondaryStats.nDamR;

				if (mob.Template.Boss)
				{
					nTotalDAMr += cd.Stats.SecondaryStats.ItemPotential.nBossDamR;
				}

				if (cd.Stats.SecondaryStats.nInfinity != 0)
				{
					nRandomAdjustedDamage +=
						(cd.Stats.SecondaryStats.nInfinity - 1) * nRandomAdjustedDamage / 100.0;
				}

				nRandomAdjustedDamage += nTotalDAMr * nRandomAdjustedDamage / 100.0;

				if (nRandomAdjustedDamage < 1) nRandomAdjustedDamage = 1;
				if (nRandomAdjustedDamage > Constants.DAMAGE_CAP) nRandomAdjustedDamage = Constants.DAMAGE_CAP;

				//cd.SendMessage("Crit: " + abCritical[i]);

				if (mob.Stats[MobStatType.HealByDamage] is MobStatEntry pHealByDamage)
				{
					aDamage[i] *= pHealByDamage.nOption / -100;
				}
				else
				{
					aDamage[i] = (int)nRandomAdjustedDamage;
				}
			}
		}

		public int MDamage(Character cd, CMob ms, SkillEntry pSkill, int nDamage, int nWeaponItemID, int nDragonFury)
		{
			var aRandom = new uint[7];

			for (var i = 0; i < aRandom.Length; i++)
			{
				aRandom[i] = m_RndForSummoned.Random();
			}

			var nMDamr = cd.Stats.PassiveSkillData.nMDamr;
			var nMADr = cd.Stats.PassiveSkillData.nMADr;
			var nACCr = cd.Stats.PassiveSkillData.nACCr;
			var nMADx = cd.Stats.PassiveSkillData.nMADx;

			if (cd.Stats.PassiveSkillData.AdditionPsd.TryGetValue(pSkill.nSkillID, out var pPsd))
			{
				nMDamr += pPsd.nMDamr;
			}

			var nACC = cd.Stats.SecondaryStats.GetACC(cd, nACCr, cd.BasicStats.CalcBaseMACC(), out _);
			var nMAD = cd.Stats.SecondaryStats.GetMAD(nMADx, nMADr, nDragonFury);

			var nMobEVA = ms.Stats.nEVA;

			var nFinalACCr = calc_accr(nACC, nMobEVA, cd.Stats.nLevel, ms.Template.Level, 0);

			if (nFinalACCr < get_rand(aRandom[0], 100.0, 0.0) || ms.Template.Invincible) return 0;

			var nMImmune = ms.Stats[MobStatType.MImmune]?.nOption ?? 0;

			if (nMImmune != 0 && aRandom[1] % 100 > cd.Stats.SecondaryStats.nRespectMImmune) return 1;

			var nWT = ItemConstants.get_weapon_type(nWeaponItemID);
			var nAmp = SkillStatLogic.get_amplification(cd, 0, out _);
			var nMagicMastery = SkillStatLogic.get_magic_mastery(cd, out _);

			var dMasteryConst = GetMasteryConstByWT(nWT);
			var dDamageByWT = CalcDamageByWT(cd, nWT, 0, nMAD) * nDamage / 100.0;

			dDamageByWT = adjust_random_damage(nAmp * dDamageByWT / 100.0, aRandom[2], dMasteryConst, nMagicMastery);

			dDamageByWT = get_damage_adjusted_by_elemAttr(
				nMDamr * dDamageByWT / 100.0 + dDamageByWT,
				pSkill,
				ms.Stats.aDamagedElemAttr,
				1.0 - cd.Stats.SecondaryStats.nElementalReset / 100.0,
				0.0);

			dDamageByWT += ApplyGuidedBulletDamage(cd, ms.nMobTemplateId);

			if (cd.Skills.Get(Skills.MECHANIC_MASTERY) is SkillEntry pMechMastery)
			{
				dDamageByWT = (100 + pMechMastery.X_Effect) * dDamageByWT / 100.0;
			}

			int nHealByDamage = ms.Stats[MobStatType.HealByDamage]?.nOption ?? 0;

			if (nHealByDamage != 0)
			{
				return (int)(dDamageByWT * nHealByDamage / -100.0);
			}

			return (int)dDamageByWT;
		}

		public int MDamage(CMob msMobAttack, CMob msMobTarget)
		{
			var nRand = m_RndForMob.Random();

			return (int)(calc_mob_base_damage(msMobAttack.Stats.nMAD, nRand) * ((100.0 - msMobTarget.Stats.nMDR) / 100.0));
		}

		// 0x0072EA20
		public int MDamage(Character cd, CMob ms, uint nRandForMissCheck, bool bShieldEquiped, out int pnReduce)
		{
			pnReduce = 0;

			if (CheckMDamageMiss(cd, ms, nRandForMissCheck)) return 0;

			var nPsdMDDr = cd.Stats.PassiveSkillData.nMDDr;
			var nPsdMDr = cd.Stats.PassiveSkillData.nMDr;

			var nMobMAD = ms.Stats.nMAD;

			var nRand = m_RndGenForMob.Random();
			var nMobBaseDamage = calc_mob_base_damage(nMobMAD, nRand);

			var nBaseMDD = cd.BasicStats.CalcBaseMDD();
			var nMDD = cd.Stats.SecondaryStats.GetMDD(cd, nBaseMDD, nPsdMDDr, bShieldEquiped, out _);

			var nBaseDefense = adjust_base_defense(nMobBaseDamage, nMDD, ms.Template.Level, cd.Stats.nLevel, nPsdMDr);

			var nDamage = nBaseDefense - cd.Stats.SecondaryStats.nSwallowDefense * nBaseDefense / 100.0;

			if (cd.Stats.SecondaryStats.nMesoGuard != 0)
			{
				pnReduce = GetMesoGuardReduce(cd, nDamage);
			}

			if (ms.Stats[MobStatType.MagicUp] is MobStatEntry pMagicUp && pMagicUp.nOption != 0)
			{
				nDamage = pMagicUp.nOption * pMagicUp.nOption / 100.0;
			}

			if (nDamage < 1.0) nDamage = 1.0;
			if (nDamage > Constants.DAMAGE_CAP) nDamage = Constants.DAMAGE_CAP;

			return 0;
		}

		// TODO hook this into CSummoned::SetDamaged
		public int MDamageSummoned(Character cd, CMob ms, uint nRandForMissCheck, bool bShieldEquiped, out int pnReduce)
		{
			// TODO consider just merging these two funcs
			return MDamage(cd, ms, nRandForMissCheck, bShieldEquiped, out pnReduce);
		}

		// =======================================================

		public int PDamage(Character cd, CMob ms, MapleAttack pInfo)
		{

			return 0;
		}
		//void __thiscall CalcDamage::PDamage(CalcDamage *this, CharacterData *cd, BasicStat *bs, SecondaryStat *ss, const unsigned int dwMobID, MobStat *ms, CMobTemplate *pTemplate,
		//ZRef<PassiveSkillData> pPsd, int *bNextAttackCritical, int nAttackCount, int nDamagePerMob, int nWeaponItemID, int nBulletItemID, int nAttackType, int nAction, int bShadowPartner,
		//SKILLENTRY *pSkill, int nSLV, int *aDamage, int *abCritical, int nCriticalProb, int nCriticalDamage, int nTotalDAMr, int nBossDAMr, int nIgnoreTargetDEF, int nDragonFury,
		//int nAR01Pad, int tKeyDown, int nDarkForce, int nAdvancedChargeDamage, int bInvincible)
		public int PDamage(Character cd, CMob ms, SkillEntry pSkill, MapleAttack pAttackInfo, int nWeaponItemID, out int[] aDamage, out int[] abCritical)
		{
			aDamage = new int[pAttackInfo.nDamagePerMob];
			abCritical = new int[pAttackInfo.nDamagePerMob];

			var aRandom = new uint[RND_SIZE];

			//cd.SendMessage($"Rand Before MDamage: S1 {(int)m_RndGenForCharacter.m_s1} | S2 {(int)m_RndGenForCharacter.m_s2} | S3 {(int)m_RndGenForCharacter.m_s3}");

			for (var i = 0; i < aRandom.Length; i++)
			{
				aRandom[i] = m_RndGenForCharacter.Random();
			}

			//cd.SendMessage($"Rand After MDamage:  S1 {(int)m_RndGenForCharacter.m_s1} | S2 {(int)m_RndGenForCharacter.m_s2} | S3 {(int)m_RndGenForCharacter.m_s3}");

			return 0;
		}

		// =======================================================

		public double GetMasteryConstByWT(int nWT)
		{
			// func name: CalcDamage::GetMsateryConstByWT <- yes there's a typo

			switch ((WeaponType)nWT)
			{
				case WeaponType.WT_WAND:
				case WeaponType.WT_STAFF:
					return 0.25;
				case WeaponType.WT_BOW:
				case WeaponType.WT_CROSSBOW:
				case WeaponType.WT_THROWINGGLOVE:
				case WeaponType.WT_GUN:
					return 0.15;
				default:
					return 0.2;
			}
		}

		public double CalcDamageByWT(Character cd, int nWT, int nPAD, int nMAD)
		{
			var result = 0.0;

			if (JobLogic.is_beginner_job(cd.Stats.nJob))
			{
				result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.2);
			}
			else if (JobLogic.is_mage_job(cd.Stats.nJob))
			{
				result = calc_base_damage(cd.BasicStats.nINT, cd.BasicStats.nLUK, 0, nMAD, 1.0);
			}
			else
			{
				switch ((WeaponType)nWT)
				{
					case WeaponType.WT_OH_SWORD:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.2);
						break;
					case WeaponType.WT_OH_AXE:
					case WeaponType.WT_OH_MACE:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.2);
						break;
					case WeaponType.WT_DAGGER:
						result = calc_base_damage(cd.BasicStats.nLUK, cd.BasicStats.nDEX, cd.BasicStats.nSTR, nPAD, 1.3);
						break;
					case WeaponType.WT_BAREHAND:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, 1, 1.43);
						break;
					case WeaponType.WT_TH_SWORD:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.32);
						break;

					case WeaponType.WT_TH_AXE:
					case WeaponType.WT_TH_MACE:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.32);
						break;

					case WeaponType.WT_SPEAR:
					case WeaponType.WT_POLEARM:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.49);
						break;

					case WeaponType.WT_BOW:
						result = calc_base_damage(cd.BasicStats.nDEX, cd.BasicStats.nSTR, 0, nPAD, 1.2);
						break;
					case WeaponType.WT_CROSSBOW:
						result = calc_base_damage(cd.BasicStats.nDEX, cd.BasicStats.nSTR, 0, nPAD, 1.35);
						break;

					case WeaponType.WT_THROWINGGLOVE:
						result = calc_base_damage(cd.BasicStats.nLUK, cd.BasicStats.nDEX, 0, nPAD, 1.75);
						break;
					case WeaponType.WT_KNUCKLE:
						result = calc_base_damage(cd.BasicStats.nSTR, cd.BasicStats.nDEX, 0, nPAD, 1.7);
						break;

					case WeaponType.WT_GUN:
						result = calc_base_damage(cd.BasicStats.nDEX, cd.BasicStats.nSTR, 0, nPAD, 1.5);
						break;
				}
			}

			return result;
		}

		public static bool CalcPImmune(CMob ms, int nRand)
		{
			var nOption = ms.Stats.TempStats[MobStatType.PImmune]?.nOption ?? 0;
			return nOption > 0 && nRand > nOption;
		}

		// =======================================================

		public static double get_damage_adjusted_by_elemAttr(double damage, SkillEntry pSkill, int[] aDamagedElemAttr, double dAdjustByBuff, double dBoost)
		{
			if (pSkill is null) return damage;
			switch ((Skills)pSkill.nSkillID)
			{
				case Skills.MAGE1_MAGIC_COMPOSITION:
					return get_damage_adjusted_by_elemAttr(damage * 0.5,
							   aDamagedElemAttr[(int)ElemAttrType.Fire], 1.0, 0.0)
						   + get_damage_adjusted_by_elemAttr(damage * 0.5,
							   aDamagedElemAttr[(int)ElemAttrType.Poison], 1.0, dBoost);
				case Skills.MAGE2_MAGIC_COMPOSITION:
					return get_damage_adjusted_by_elemAttr(damage * 0.5,
							   aDamagedElemAttr[(int)ElemAttrType.Ice], 1.0, 0.0)
						   + get_damage_adjusted_by_elemAttr(damage * 0.5,
							   aDamagedElemAttr[(int)ElemAttrType.Light], 1.0, dBoost);
				case Skills.RANGER_FIRE_SHOT:
				case Skills.SNIPER_ICE_SHOT:
					return get_damage_adjusted_by_elemAttr(damage, aDamagedElemAttr[(int)pSkill.Template.ElemAttr],
						pSkill.Template.X(pSkill.nSLV) / 100.0, dBoost);
			}

			return get_damage_adjusted_by_elemAttr(damage, aDamagedElemAttr[(int)pSkill.Template.ElemAttr], dAdjustByBuff, dBoost);
		}

		public static double get_damage_adjusted_by_elemAttr(double damage, int nAttr, double dAdjust, double dBoost)
		{
			switch (nAttr)
			{
				case 1:
					return (1.0 - dAdjust) * damage;
				case 2:
					return (1.0 - (dAdjust * 0.5 + dBoost)) * damage;
				case 3:
					var result = (dAdjust * 0.5 + dBoost + 1.0) * damage;

					if (damage > result) result = damage;

					if (result > Constants.DAMAGE_CAP) result = Constants.DAMAGE_CAP;

					return result;
				default:
					return damage;
			}
		}

		// =======================================================

		// `anonymous namespace'::calc_evar
		private static int calc_evar(int nEVA, int nMobACC, int nTargetLevel, int nAttackLevel, int nEr)
		{
			var nCharEvaSqrt = Math.Sqrt(nEVA);
			var nMobAccSqrt = Math.Sqrt(nMobACC);

			var nFormulaRes = (int)(nCharEvaSqrt - nMobAccSqrt + nEr * (nCharEvaSqrt - nMobAccSqrt) / 100);

			var result = nFormulaRes <= 0 ? 0 : nFormulaRes;

			if (nAttackLevel > nTargetLevel)
			{
				var nDiff = 5 * (nAttackLevel - nTargetLevel);

				if (nDiff >= result) nDiff = result;

				result -= nDiff;
			}

			return result;
		}

		// `anonymous namespace'::calc_base_damage
		private static int calc_base_damage(int p1, int p2, int p3, int ad, double k)
		{
			return (int)((p3 + p2 + 4 * p1) / 100.0 * (ad * k) + 0.5);
		}

		private static double calc_mob_base_damage(int p1, uint nRand)
		{
			return get_rand(nRand, p1, 0.85 * p1);
		}

		private static double get_rand(uint nRand, double f0, double f1) // TODO verify this
		{
			double realF1 = f1;
			double realF0 = f0;
			if (f0 > f1)
			{
				realF1 = f0;
				realF0 = f1;
			}
			double result;
			if (realF1 != realF0)
			{
				result = realF0 + nRand % 10000000 * (realF1 - realF0) / 9999999.0;
			}
			else
			{
				result = realF0;
			}

			return result;
		}

		private static int calc_accr(int nACC, int nMobEVA, int nAttackLevel, int nTargetLevel, int nAr)
		{
			nACC = (int)Math.Sqrt(nACC);
			nMobEVA = (int)Math.Sqrt(nMobEVA);

			var nACCr = nACC - nMobEVA + 100 + nAr * (nACC - nMobEVA + 100) / 100;

			if (nACCr > 100) nACCr = 100;

			if (nTargetLevel > nAttackLevel)
			{
				var nLevelMod = 5 * (nTargetLevel - nAttackLevel);
				if (nLevelMod > nACCr) nLevelMod = nACCr;

				nACCr -= nLevelMod;
			}

			return nACCr;
		}

		// `anonymous namespace'::adjust_ramdom_damage <- yes typo
		private static double adjust_random_damage(double damage, uint nRand, double k, int nMastery)
		{
			var v4 = nMastery / 100.0 + k;

			if (v4 > 0.95) v4 = 0.95;

			return get_rand(nRand, damage, v4 * damage + 0.5);
		}

		private static double adjust_base_defense(double damage, int nADD, int nAttackLevel, int nTargetLevel,
			int nPsdDr)
		{
			var v5 = nADD * 0.25;
			var v6 = v5 + 0.5;
			var nFixedCanceling = v5 + 0.5;
			var v7 = Math.Sqrt(v5);
			var v8 = nPsdDr * v7 / 100 + v7;

			if (nTargetLevel < nAttackLevel)
			{
				var v9 = Math.Abs(nAttackLevel - nTargetLevel);
				double v10 = 4 * v9;

				if (4 * v9 >= v6) v10 = v6;

				double v11 = 2 * v9;
				nFixedCanceling = v6 - v10;

				if (v11 >= v8) v11 = v8;

				v8 -= v11;
			}

			var result = damage - nFixedCanceling;
			var v13 = damage * (100 - v8) / 100.0;

			if (v13 <= result) result = v13;
			if (result <= 1.0) result = 1.0;

			return result;
		}

		private static double ApplyGuidedBulletDamage(Character cd, int dwMobID)
		{
			// TODO

			//pAdvGuidedBullet = 0;
			//v4 = CSkillInfo::GetSkillLevel(TSingleton < CSkillInfo >::ms_pInstance, cd, CAPTAIN_ADVANCED_HOMING, &pAdvGuidedBullet);
			//if (v4 > 0)
			//{
			//	v5 = ss->aTemporaryStat[5].p;
			//	if (v5)
			//	{
			//		if ((v5->vfptr[1].__vecDelDtor)(ss->aTemporaryStat[5].p))
			//		{
			//			v6 = &v5->m_lock;
			//			ZFatalSection::Lock(&v5->m_lock);
			//			v7 = v5[1]._m_nRef;
			//			if (v6)
			//			{
			//				v8 = v6->_m_nRef-- == 1;
			//				if (v8)
			//					v6->_m_pTIB = 0;
			//			}
			//			if (v7 == dwMobID)
			//			{
			//				v9 = SKILLENTRY::GetLevelData(pAdvGuidedBullet, v4);
			//				*damage = (_ZtlSecureFuse<long>(v9->_ZtlSecureTear_nX, v9->_ZtlSecureTear_nX_CS) + 100) * *damage / 100.0;
			//			}
			//		}
			//	}
			//}

			return 0;
		}
	}
}
