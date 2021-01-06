using System;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Stat
{
	public class SecondaryStatValues
	{
		// When adding new entries to this list, make sure to prefix correctly.
		// nStat = nOption (Data)
		// rStat = rOption (SkillID)
		// tStat = tOption (Duration)
		// sStat = State (not in gms, used internally for some skills)

		// Also remember to add handling in the set/remove functions below...

		// In Nexon's SecondaryStat there are "duplicates" -> in reality one is for
		//	item and one is for skill, the only difference being that the skill stat
		//	ends in an underscore and the item stat doesn't. Example: nACC, and nACC_

		public SecondaryStatValuesEx ItemPotential { get; private set; }

		public int nItemPAD { get; private set; }
		public int nItemPADr { get; private set; }
		public int nPAD { get; private set; }
		public int rPAD { get; private set; }

		public int nItemPDD { get; private set; }
		public int nItemPDDr { get; private set; }
		public int nPDD { get; private set; }
		public int rPDD { get; private set; }

		public int nItemMAD { get; private set; }
		public int nItemMADr { get; private set; }
		public int nMAD { get; private set; }
		public int rMAD { get; private set; }

		public int nItemMDD { get; private set; }
		public int nItemMDDr { get; private set; }
		public int nMDD { get; private set; }
		public int rMDD { get; private set; }

		public int nItemACC { get; private set; } // _ZtlSecureTear_nItemACCR
		public int nItemACCr { get; private set; } // _ZtlSecureTear_nACC
		public int nACC { get; private set; } // _ZtlSecureTear_nACC_
		public int rACC { get; private set; } // _ZtlSecureTear_rACC_

		public int nItemEVA { get; private set; }
		public int nItemEVAr { get; private set; }
		public int nEVA { get; private set; }
		public int rEVA { get; private set; }

		public int nItemCraft { get; private set; } // _ZtlSecureTear_nCraft
		public int nCraft { get; private set; } // _ZtlSecureTear_nCraft_
		public int rCraft { get; private set; } // _ZtlSecureTear_rCraft_

		// also has speed from items but idc 
		public int nSpeed { get; private set; }
		public int rSpeed { get; private set; }

		// also has jump from items but idc 
		public int nJump { get; private set; }
		public int rJump { get; private set; }

		public int nEMHP { get; private set; }
		public int rEMHP { get; private set; }

		public int nEMMP { get; private set; }
		public int rEMMP { get; private set; }

		public int nEPAD { get; private set; }
		public int rEPAD { get; private set; }

		public int nEPDD { get; private set; }
		public int rEPDD { get; private set; }

		public int nEMDD { get; private set; }
		public int rEMDD { get; private set; }

		public int nPickPocket { get; private set; }
		public int rPickPocket { get; private set; }

		public int nHolySymbol { get; private set; }
		public int rHolySymbol { get; private set; }

		public int nMesoUp { get; private set; }
		public int rMesoUp { get; private set; }

		public int nMagicGuard { get; private set; }
		public int rMagicGuard { get; private set; }

		public int nPowerGuard { get; private set; }
		public int rPowerGuard { get; private set; }

		public int nMesoGuard { get; private set; }
		public int rMesoGuard { get; private set; }

		public int nGuard { get; private set; }
		public int rGuard { get; private set; }

		public int nInfinity { get; private set; }
		public int rInfinity { get; private set; }

		public int nManaReflection { get; private set; }
		public int rManaReflection { get; private set; }

		public int nShadowPartner { get; private set; }
		public int rShadowPartner { get; private set; }

		public int nHolyShield { get; private set; }
		public int rHolyShield { get; private set; }

		public int rBeholder { get; private set; } // don't need nOption for this one

		/// <summary>
		/// Multiplicative (rate)
		/// </summary>
		public int nExpBuffRate { get; private set; }
		public int rExpBuffRate { get; private set; }

		/// <summary>
		/// Additive (up)
		/// </summary>
		public int nItemUpByItem { get; private set; }
		public int rItemUpByItem { get; private set; }
		/// <summary>
		/// Additive (up)
		/// </summary>
		public int nMesoUpByItem { get; private set; }
		public int rMesoUpByItem { get; private set; }

		public int nDice { get; private set; }
		public int rDice { get; private set; }

		public int nBasicStatUp { get; private set; }
		public int rBasicStatUp { get; private set; }

		public int nMaxHP { get; private set; }
		public int rMaxHP { get; private set; }

		public int nMaxMP { get; private set; }
		public int rMaxMP { get; private set; }

		public int nMoreWild { get; private set; }
		public int rMoreWild { get; private set; }

		public int nConversion { get; private set; }
		public int rConversion { get; private set; }

		//public int nAura { get; private set; } // TODO this shit
		//public int rAura { get; private set; }
		//public int nAuraSLV{ get; private set; } // custom cuz auras are weird


		public int nWeaponCharge { get; private set; }
		public int rWeaponCharge { get; private set; }

		public int nEnergyCharged { get; private set; }
		public int rEnergyCharged { get; private set; }

		public int nComboAbilityBuff { get; private set; }
		public int rComboAbilityBuff { get; private set; }

		public int nMaxLevelBuff { get; private set; }
		public int rMaxLevelBuff { get; private set; }

		public int nDarkAura { get; private set; }
		public int rDarkAura { get; private set; }

		public int nBlueAura { get; private set; }
		public int rBlueAura { get; private set; }

		public int nSwallowAttackDamage { get; private set; }
		public int rSwallowAttackDamage { get; private set; }

		public int nSwallowDefense { get; private set; }
		public int rSwallowDefense { get; private set; }

		public int nBlessingArmor { get; private set; }
		public int rBlessingArmor { get; private set; }

		public int nSharpEyes { get; private set; }
		public int rSharpEyes { get; private set; }

		public int nThornsEffect { get; private set; }
		public int rThornsEffect { get; private set; }

		public int nRespectMImmune { get; private set; }
		public int rRespectMImmune { get; private set; }

		public int nRespectPImmune { get; private set; }
		public int rRespectPImmune { get; private set; }

		public int nSeal { get; private set; }
		public int rSeal { get; private set; }

		public int nElementalReset { get; private set; }
		public int rElementalReset { get; private set; }

		public int nDojangBerserk { get; private set; }
		public int rDojangBerserk { get; private set; }

		public int nDamR { get; private set; }
		public int rDamR { get; private set; }

		public int nRideVehicle { get; private set; }
		public int rRideVehicle { get; private set; }

		public int nDarkSight { get; private set; }
		public int rDarkSight { get; private set; }

		// TODO maybe fill out the rest of these eventually

		public SecondaryStatValues()
		{
			ItemPotential = new SecondaryStatValuesEx();
		}

		public void SetFromInventory(Character cd)
		{
			var job = cd.Stats.nJob;
			var level = cd.Stats.nLevel;
			ItemPotential = new SecondaryStatValuesEx();

			nItemPAD = 0;
			nItemPADr = 0;

			nItemPDD = 0;
			nItemPDDr = 0;

			nItemMAD = 0;
			nItemMADr = 0;

			nItemMDD = 0;
			nItemMDDr = 0;

			nItemACC = 0;
			nItemACCr = 0;

			nItemEVA = 0;
			nItemEVAr = 0;

			nItemCraft = cd.BasicStats.nLUK + cd.BasicStats.nDEX + cd.BasicStats.nINT;

			// TODO calculate item sets

			set(cd.EquippedInventoryNormal);
			set(cd.EquippedInventoryCash); // TODO iterate only slots that can have stats

			var weapon = cd.EquippedInventoryCash.Get((int)BodyPart.BP_WEAPON);

			int nWeaponItemId;

			if (weapon is null && job % 1000 / 100 == 5)
			{
				nItemPAD += (int)(level > 30 ? 31 : level * 0.7 + 10);
				nWeaponItemId = 0;
			}
			else
			{
				nWeaponItemId = weapon?.nItemID ?? 0;
			}

			set(cd.aMechanicEquipped);
			set(cd.aDragonEquipped);

			// would make more sense to put this in an if-else tree but the client doesnt so i wont

			if (cd.Skills.Get(Skills.ROGUE_NIMBLE_BODY) is SkillEntry rnb)
			{
				nItemACC += (int)rnb.X_Effect;
				nItemEVA += (int)rnb.Y_Effect;
			}

			if (cd.Skills.Get(Skills.NIGHTWALKER_NIMBLE_BODY) is SkillEntry nnb)
			{
				nItemACC += (int)nnb.X_Effect;
				nItemEVA += (int)nnb.Y_Effect;
			}

			if (cd.Skills.Get(Skills.PIRATE_QUICKMOTION) is SkillEntry pqm)
			{
				nItemACC += (int)pqm.X_Effect;
				nItemEVA += (int)pqm.Y_Effect;
			}

			if (cd.Skills.Get(Skills.STRIKER_QUICKMOTION) is SkillEntry sqm)
			{
				nItemACC += (int)sqm.X_Effect;
				nItemEVA += (int)sqm.Y_Effect;
			}

			if (cd.Skills.Get(Skills.EVAN_DRAGON_SOUL) is SkillEntry eds)
			{
				nItemMAD += eds.Template.MAD(eds.nSLV);
			}

			var blessOfNymph = SkillLogic
				.get_novice_skill_as_race
					(SkillLogic.NoviceSkillID.BlessingOfTheFairy, job);

			if (cd.Skills.Get(blessOfNymph) is SkillEntry bon)
			{
				nItemPAD += (int)bon.X_Effect;
				nItemMAD += (int)bon.Y_Effect;
				nItemACC += (int)bon.Template.Z(bon.nSLV);
				nItemEVA += (int)bon.Template.Z(bon.nSLV);
			}

			if (cd.Buffs[Skills.WILDHUNTER_JAGUAR_RIDING] is BuffSkill whjr)
			{
				nItemEVA += (int)whjr.Template.Y(whjr.nSLV);
			}

			var nAttackType = 0;

			if (job % 1000 / 100 == 5 || job / 10 == 41 || job / 10 == 52 || job / 100 == 13 || job / 100 == 14)
			{
				nAttackType = 1;
			}

			if (SkillStatLogic.get_weapon_mastery(cd, nWeaponItemId, nAttackType, 0,
				out int pncAccInc,
				out int pnPadInc) != 0)
			{
				nItemACC += pncAccInc;
				nItemPAD += pnPadInc;
			}

			if (SkillStatLogic.get_magic_mastery(cd, out int nMADInc) != 0)
			{
				nItemMAD += nMADInc;
			}

			// TODO forced stat calculation here



			void set(CharacterInventoryEquips inv)
			{
				foreach (var item in inv)
				{
					nItemPAD += item.Value.niPAD;
					nItemMAD += item.Value.niMAD;

					nItemPDD += item.Value.niPDD;
					nItemMDD += item.Value.niMDD;

					nItemACC += item.Value.niACC;
					nItemEVA += item.Value.niEVA;

					nItemCraft += item.Value.niCraft;

					// TODO eventually speed and jump here too if we need it

					if (!item.Value.HasVisiblePotential) continue;

					foreach (var option in item.Value.nOptionData)
					{
						if (option is null) continue;

						nItemACC += option.iACC;
						nItemEVA += option.iEVA;
						nItemPAD += option.iPAD;
						nItemMAD += option.iMAD;
						nItemPDD += option.iPDD;
						nItemMDD += option.iMDD;
						nItemACCr += option.iACCr;
						nItemEVAr += option.iEVAr;
						nItemPADr += option.iPADr;
						nItemMADr += option.iMADr;
						nItemPDDr += option.iPDDr;
						nItemMDDr += option.iMDDr;
						ItemPotential.nCr += option.iCr;
						ItemPotential.nCDr += option.iCDr;
						ItemPotential.nRecoveryHP += option.RecoveryHP;
						ItemPotential.nRecoveryMP += option.RecoveryMP;
						ItemPotential.nIgnoreTargetDEF += option.IgnoreTargetDEF;
						ItemPotential.nIncAllSkill += option.iAllSkill;
						ItemPotential.nMesoProp += option.iMesoProb;
						ItemPotential.nRewardProp += option.iRewardProb;
						ItemPotential.nReduceMpConR += option.MPConReduce;
						ItemPotential.nRecoveryUP += option.RecoveryUP;

						if (option.Boss > 0)
						{
							ItemPotential.nBossDamR += option.iDAMr;
						}
						else
						{
							ItemPotential.nDamR += option.iDAMr;
						}

						if (option.AttackType > 0)
						{
							ItemPotential.nAttackType = option.AttackType;
							ItemPotential.nAttackTypeLevel = option.Level;
							ItemPotential.nAttackTypeProp = option.Prop;
						}
						else if (option.DAMReflect > 0)
						{
							ItemPotential.nReflectDamR += option.DAMReflect;
							ItemPotential.nReflectDamRProb += option.Prop;
						}
						else if (option.IgnoreDAMr > 0)
						{
							ItemPotential.nIgnoreDamR += option.IgnoreDAMr;
							ItemPotential.nIgnoreDamRProp += option.Prop;
						}

						switch (option.nOptionType)
						{
							case 201: // change to hp on attack
								ItemPotential.nHpOnHit += option.iHP;
								ItemPotential.nHpOnHitProp += option.Prop;
								break;
							case 206: // change to mp on attack
								ItemPotential.nMpOnHit += option.iMP;
								ItemPotential.nMpOnHitProp += option.Prop;
								break;
							case 396:
								ItemPotential.nReduceAbnormalStatus += option.Time;
								break;
							case 401:
								ItemPotential.nHpOnKill += option.iHP;
								ItemPotential.nHpOnKillProp += option.Prop;
								break;
							case 406:
								ItemPotential.nMpOnKill += option.iMP;
								ItemPotential.nMpOnKillProp += option.Prop;
								break;
							case 511:
								// TODO determine what this is -> mpRestore is the name of the attribute??
								break;
							case 701:
							case 702:
								ItemPotential.nStealProp += option.Prop;
								break;
						}
					}
				}
			}
		}

		public void SetFromSecondaryStat(Character cd, SecondaryStat stats)
		{
			foreach (var stat in stats)
			{
				int rOption;
				int rOptionPrev = stat.Value.rValue;

				switch (stat.Key)
				{
					case SecondaryStatFlag.PAD:
						nPAD = stat.Value.nValue;
						rOption = rPAD;
						rPAD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.PDD:
						nPDD = stat.Value.nValue;
						rOption = rPDD;
						rPDD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MAD:
						nMAD = stat.Value.nValue;
						rOption = rMAD;
						rMAD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MDD:
						nMDD = stat.Value.nValue;
						rOption = rMDD;
						rMDD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ACC:
						nACC = stat.Value.nValue;
						rOption = rACC;
						rACC = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EVA:
						nEVA = stat.Value.nValue;
						rOption = rEVA;
						rEVA = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Craft:
						nCraft = stat.Value.nValue;
						rOption = rCraft;
						rCraft = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Speed:
						nSpeed = stat.Value.nValue;
						rOption = rSpeed;
						rSpeed = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Jump:
						nJump = stat.Value.nValue;
						rOption = rJump;
						rJump = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EMHP:
						nEMHP = stat.Value.nValue;
						rOption = rEMHP;
						rEMHP = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EMMP:
						nEMMP = stat.Value.nValue;
						rOption = rEMMP;
						rEMMP = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EPAD:
						nEPAD = stat.Value.nValue;
						rOption = rEPAD;
						rEPAD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EPDD:
						nEPDD = stat.Value.nValue;
						rOption = rEPDD;
						rEPDD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EMDD:
						nEMDD = stat.Value.nValue;
						rOption = rEMDD;
						rEMDD = stat.Value.rValue;
						break;
					case SecondaryStatFlag.PickPocket:
						nPickPocket = stat.Value.nValue;
						rOption = rPickPocket;
						rPickPocket = stat.Value.rValue;
						break;
					case SecondaryStatFlag.HolySymbol:
						nHolySymbol = stat.Value.nValue;
						rOption = rHolySymbol;
						rHolyShield = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MesoUp:
						nMesoUp = stat.Value.nValue;
						rOption = rMesoUp;
						rMesoUp = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MagicGuard:
						nMagicGuard = stat.Value.nValue;
						rOption = rMagicGuard;
						rMagicGuard = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MesoGuard:
						nMesoGuard = stat.Value.nValue;
						rOption = rMesoGuard;
						rMesoGuard = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Guard:
						nGuard = stat.Value.nValue;
						rOption = rGuard;
						rGuard = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Infinity:
						nInfinity = stat.Value.nValue;
						rOption = rInfinity;
						rInfinity = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ManaReflection:
						nManaReflection = stat.Value.nValue;
						rOption = rManaReflection;
						rManaReflection = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ShadowPartner:
						nShadowPartner = stat.Value.nValue;
						rOption = rShadowPartner;
						rShadowPartner = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Holyshield:
						nHolyShield = stat.Value.nValue;
						rOption = rHolyShield;
						rHolyShield = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Beholder:
						rOption = rBeholder;
						rBeholder = stat.Value.rValue; // SkillID
						break;
					case SecondaryStatFlag.ExpBuffRate:
						nExpBuffRate = stat.Value.nValue;
						rOption = rExpBuffRate;
						rExpBuffRate = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Dice:
						nDice = stat.Value.nValue;
						rOption = rDice;
						rDice = stat.Value.rValue;
						break;
					case SecondaryStatFlag.BasicStatUp:
						nBasicStatUp = stat.Value.nValue;
						rOption = rBasicStatUp;
						rBasicStatUp = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MaxHP:
						nMaxHP = stat.Value.nValue;
						rOption = rMaxHP;
						rMaxHP = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MaxMP:
						nMaxMP = stat.Value.nValue;
						rOption = rMaxMP;
						rMaxMP = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MoreWild:
						nMoreWild = stat.Value.nValue;
						rOption = rMoreWild;
						rMoreWild = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Conversion:
						nConversion = stat.Value.nValue;
						rOption = rConversion;
						rConversion = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ItemUpByItem:
						nItemUpByItem = stat.Value.nValue;
						rOption = rItemUpByItem;
						rItemUpByItem = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MesoUpByItem:
						nMesoUpByItem = stat.Value.nValue;
						rOption = rMesoUpByItem;
						rMesoUpByItem = stat.Value.rValue;
						break;
					case SecondaryStatFlag.PowerGuard:
						nPowerGuard = stat.Value.nValue;
						rOption = rPowerGuard;
						rPowerGuard = stat.Value.rValue;
						break;
					case SecondaryStatFlag.WeaponCharge:
						nWeaponCharge = stat.Value.nValue;
						rOption = rWeaponCharge;
						rWeaponCharge = stat.Value.rValue;
						break;
					case SecondaryStatFlag.EnergyCharged:
						nEnergyCharged = stat.Value.nValue;
						rOption = rEnergyCharged;
						rEnergyCharged = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ComboAbilityBuff:
						nComboAbilityBuff = stat.Value.nValue;
						rOption = rComboAbilityBuff;
						rComboAbilityBuff = stat.Value.rValue;
						break;
					case SecondaryStatFlag.MaxLevelBuff:
						nMaxLevelBuff = stat.Value.nValue;
						rOption = rMaxLevelBuff;
						rMaxLevelBuff = stat.Value.rValue;
						break;
					case SecondaryStatFlag.DarkAura:
						nDarkAura = stat.Value.nValue;
						rOption = rDarkAura;
						rDarkAura = stat.Value.rValue;
						break;
					case SecondaryStatFlag.SwallowAttackDamage:
						nSwallowAttackDamage = stat.Value.nValue;
						rOption = rSwallowAttackDamage;
						rSwallowAttackDamage = stat.Value.rValue;
						break;
					case SecondaryStatFlag.BlueAura:
						nBlueAura = stat.Value.nValue;
						rOption = rBlueAura;
						rBlueAura = stat.Value.rValue;
						break;
					case SecondaryStatFlag.BlessingArmor:
						nBlessingArmor = stat.Value.nValue;
						rOption = nBlessingArmor;
						rBlessingArmor = stat.Value.rValue;
						break;
					case SecondaryStatFlag.SharpEyes:
						nSharpEyes = stat.Value.nValue;
						rOption = rSharpEyes;
						rSharpEyes = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ThornsEffect:
						nThornsEffect = stat.Value.nValue;
						rOption = rThornsEffect;
						rThornsEffect = stat.Value.rValue;
						break;
					case SecondaryStatFlag.RespectMImmune:
						nRespectMImmune = stat.Value.nValue;
						rOption = rRespectMImmune;
						rRespectMImmune = stat.Value.rValue;
						break;
					case SecondaryStatFlag.RespectPImmune:
						nRespectPImmune = stat.Value.nValue;
						rOption = rRespectPImmune;
						rRespectPImmune = stat.Value.rValue;
						break;
					case SecondaryStatFlag.Seal:
						nSeal = stat.Value.nValue;
						rOption = rSeal;
						rSeal = stat.Value.rValue;
						break;
					case SecondaryStatFlag.ElementalReset:
						nElementalReset = stat.Value.nValue;
						rOption = rElementalReset;
						rElementalReset = stat.Value.rValue;
						break;
					case SecondaryStatFlag.DojangBerserk:
						nDojangBerserk = stat.Value.nValue;
						rOption = rDojangBerserk;
						rDojangBerserk = stat.Value.rValue;
						break;
					case SecondaryStatFlag.DamR:
						nDamR = stat.Value.nValue;
						rOption = rDamR;
						rDamR = stat.Value.rValue;
						break;
					case SecondaryStatFlag.SwallowDefence:
						nSwallowDefense = stat.Value.nValue;
						rOption = rSwallowDefense;
						rSwallowDefense = stat.Value.rValue;
						break;
					case SecondaryStatFlag.RideVehicle:
						nRideVehicle = stat.Value.nValue;
						rOption = rRideVehicle;
						rRideVehicle = stat.Value.rValue;
						break;
					case SecondaryStatFlag.DarkSight:
						nDarkSight = stat.Value.nValue;
						rOption = rDarkSight;
						rDarkSight = stat.Value.rValue;
						break;
					default:
						continue;
				}

				if (rOption == 0 || rOptionPrev == rOption) continue;

				// remove existing 
				if (cd.Buffs[rOption] is AbstractBuff ab)
				{
					// if it gets to this point it means the client values will be overriden
					// so we dont need to send a notification or call RemoveFromSecondaryStat
					ab.Stat.IsRemovedFromAddingNewStat = true;
					cd.Buffs.Remove(ab);
				}
			}
		}

		public void RemoveFromSecondaryStat(SecondaryStat stats)
		{
			// removing both nValue and rValue is critical
			foreach (var stat in stats)
			{
				switch (stat.Key)
				{
					case SecondaryStatFlag.PAD:
						nPAD = 0;
						rPAD = 0;
						break;
					case SecondaryStatFlag.PDD:
						nPDD = 0;
						rPDD = 0;
						break;
					case SecondaryStatFlag.MAD:
						nMAD = 0;
						rMAD = 0;
						break;
					case SecondaryStatFlag.MDD:
						nMDD = 0;
						rMDD = 0;
						break;
					case SecondaryStatFlag.ACC:
						nACC = 0;
						rACC = 0;
						break;
					case SecondaryStatFlag.EVA:
						nEVA = 0;
						rEVA = 0;
						break;
					case SecondaryStatFlag.Craft:
						nCraft = 0;
						rCraft = 0;
						break;
					case SecondaryStatFlag.Speed:
						nSpeed = 0;
						rSpeed = 0;
						break;
					case SecondaryStatFlag.Jump:
						nJump = 0;
						rJump = 0;
						break;
					case SecondaryStatFlag.EMHP:
						nEMHP = 0;
						rEMHP = 0;
						break;
					case SecondaryStatFlag.EMMP:
						nEMMP = 0;
						rEMMP = 0;
						break;
					case SecondaryStatFlag.EPAD:
						nEPAD = 0;
						rEPAD = 0;
						break;
					case SecondaryStatFlag.EPDD:
						nEPDD = 0;
						rEPDD = 0;
						break;
					case SecondaryStatFlag.EMDD:
						nEMDD = 0;
						rEMDD = 0;
						break;
					case SecondaryStatFlag.PickPocket:
						nPickPocket = 0;
						rPickPocket = 0;
						break;
					case SecondaryStatFlag.HolySymbol:
						nHolySymbol = 0;
						rHolySymbol = 0;
						break;
					case SecondaryStatFlag.MesoUp:
						nMesoUp = 0;
						rMesoUp = 0;
						break;
					case SecondaryStatFlag.MagicGuard:
						nMagicGuard = 0;
						rMagicGuard = 0;
						break;
					case SecondaryStatFlag.MesoGuard:
						nMesoGuard = 0;
						rMesoGuard = 0;
						break;
					case SecondaryStatFlag.Guard:
						nGuard = 0;
						rGuard = 0;
						break;
					case SecondaryStatFlag.Infinity:
						nInfinity = 0;
						rInfinity = 0;
						break;
					case SecondaryStatFlag.ManaReflection:
						nManaReflection = 0;
						rManaReflection = 0;
						break;
					case SecondaryStatFlag.ShadowPartner:
						nShadowPartner = 0;
						rShadowPartner = 0;
						break;
					case SecondaryStatFlag.Holyshield:
						nHolyShield = 0;
						rHolyShield = 0;
						break;
					case SecondaryStatFlag.Beholder:
						// no nValue
						rBeholder = 0; // SkillID
						break;
					case SecondaryStatFlag.ExpBuffRate:
						nExpBuffRate = 0;
						rExpBuffRate = 0;
						break;
					case SecondaryStatFlag.Dice:
						nDice = 0;
						rDice = 0;
						break;
					case SecondaryStatFlag.BasicStatUp:
						nBasicStatUp = 0;
						rBasicStatUp = 0;
						break;
					case SecondaryStatFlag.MaxHP:
						nMaxHP = 0;
						rMaxHP = 0;
						break;
					case SecondaryStatFlag.MaxMP:
						nMaxMP = 0;
						rMaxMP = 0;
						break;
					case SecondaryStatFlag.MoreWild:
						nMoreWild = 0;
						rMoreWild = 0;
						break;
					case SecondaryStatFlag.Conversion:
						nConversion = 0;
						rConversion = 0;
						break;
					case SecondaryStatFlag.ItemUpByItem:
						nItemUpByItem = 0;
						rItemUpByItem = 0;
						break;
					case SecondaryStatFlag.MesoUpByItem:
						nMesoUpByItem = 0;
						rMesoUpByItem = 0;
						break;
					case SecondaryStatFlag.PowerGuard:
						nPowerGuard = 0;
						rPowerGuard = 0;
						break;
					case SecondaryStatFlag.WeaponCharge:
						nWeaponCharge = 0;
						rWeaponCharge = 0;
						break;
					case SecondaryStatFlag.EnergyCharged:
						nEnergyCharged = 0;
						rEnergyCharged = 0;
						break;
					case SecondaryStatFlag.ComboAbilityBuff:
						nComboAbilityBuff = 0;
						rComboAbilityBuff = 0;
						break;
					case SecondaryStatFlag.MaxLevelBuff:
						nMaxLevelBuff = 0;
						rMaxLevelBuff = 0;
						break;
					case SecondaryStatFlag.DarkAura:
						nDarkAura = 0;
						rDarkAura = 0;
						break;
					case SecondaryStatFlag.SwallowAttackDamage:
						nSwallowAttackDamage = 0;
						rSwallowAttackDamage = 0;
						break;
					case SecondaryStatFlag.BlueAura:
						nBlueAura = 0;
						rBlueAura = 0;
						break;
					case SecondaryStatFlag.BlessingArmor:
						nBlessingArmor = 0;
						rBlessingArmor = 0;
						break;
					case SecondaryStatFlag.SharpEyes:
						nSharpEyes = 0;
						rSharpEyes = 0;
						break;
					case SecondaryStatFlag.ThornsEffect:
						nThornsEffect = 0;
						rThornsEffect = 0;
						break;
					case SecondaryStatFlag.RespectMImmune:
						nRespectMImmune = 0;
						rRespectMImmune = 0;
						break;
					case SecondaryStatFlag.RespectPImmune:
						nRespectPImmune = 0;
						rRespectPImmune = 0;
						break;
					case SecondaryStatFlag.Seal:
						nSeal = 0;
						rSeal = 0;
						break;
					case SecondaryStatFlag.ElementalReset:
						nElementalReset = 0;
						rElementalReset = 0;
						break;
					case SecondaryStatFlag.DojangBerserk:
						nDojangBerserk = 0;
						rDojangBerserk = 0;
						break;
					case SecondaryStatFlag.DamR:
						nDamR = 0;
						rDamR = 0;
						break;
					case SecondaryStatFlag.SwallowDefence:
						nSwallowDefense = 0;
						rSwallowDefense = 0;
						break;
					case SecondaryStatFlag.RideVehicle:
						nRideVehicle = 0;
						rRideVehicle = 0;
						break;
					case SecondaryStatFlag.DarkSight:
						nDarkSight = 0;
						rDarkSight = 0;
						break;
				}
			}
		}

		public int GetACC(Character cd, int nPsdACCr, int nBaseACC, out int pIncinOrg)
		{
			var nACC = nBaseACC + nItemACC + GetIncACC(cd);
			var nACCr = nPsdACCr + nItemACCr;

			pIncinOrg = nACC * (nPsdACCr + nItemACCr / 100);

			if (nACCr > 0) nACC += nACC * nACCr / 100;

			if (nACC <= 0) return 0;
			if (nACC >= 9999) return 9999;

			return nACC;
		}

		public int GetIncACC(Character cd)
		{
			var nECValue = 0;

			if (rEnergyCharged == 0) return nACC;

			if (cd.Skills.Get(JobLogic.IsKOC(cd.Stats.nJob)
				? Skills.STRIKER_ENERGY_CHARGE
				: Skills.BUCCANEER_ENERGY_CHARGE) is SkillEntry se)
			{
				nECValue = se.Template.ACC(se.nSLV);
			}

			return Math.Max(nACC, nECValue);
		}

		public int GetEVA(Character cd, int nPsdEVAr, int nBaseEVA, out int pIncinOrg)
		{
			var nEVA = nBaseEVA + nItemEVA + GetIncEVA(cd);
			var nTotalEVAr = nPsdEVAr + nItemEVAr;

			pIncinOrg = nEVA * (nPsdEVAr + nItemEVAr / 100);

			if (nTotalEVAr > 0) nEVA += nEVA * nTotalEVAr / 100;

			if (nEVA <= 0) return 0;
			if (nEVA >= 9999) return 9999;

			return nEVA;
		}

		public int GetIncEVA(Character cd)
		{
			var nECValue = 0;

			if (rEnergyCharged == 0) return nEVA;

			if (cd.Skills.Get(JobLogic.IsKOC(cd.Stats.nJob)
				? Skills.STRIKER_ENERGY_CHARGE
				: Skills.BUCCANEER_ENERGY_CHARGE) is SkillEntry se)
			{
				nECValue = se.Template.EVA(se.nSLV);
			}

			return Math.Max(nEVA, nECValue);
		}

		public int GetPDD(Character cd, int nBasePDD, int nPsdPDDr, bool bShieldEquipped, int nDarkForcePDDr, out int pIncinOrg)
		{
			var nTotalPDD = nBasePDD + nItemPDD + nPDD + nEPDD;

			if (nComboAbilityBuff != 0)
			{
				var nComboSkillID = cd.Stats.nJob != 2000
					? Skills.ARAN_COMBO_ABILITY
					: Skills.LEGEND_COMBO_ABILITY;

				if (cd.Skills.Get(nComboSkillID) is SkillEntry se)
				{
					var nComboPDD = se.Template.X(se.nSLV);

					if (nComboAbilityBuff / 10 < nComboPDD)
					{
						nComboPDD = nComboAbilityBuff;
					}

					nTotalPDD += (int)(nComboPDD * se.Template.Z(se.nSLV));
				}
			}

			var nMastery = 0;
			if (nBlueAura != 0)
			{
				if (cd.Skills.Get(Skills.BMAGE_AURA_BLUE_ADVANCED) is SkillEntry se)
				{
					nMastery = (int)se.Template.Z(se.nSLV);
				}
			}

			if (bShieldEquipped) nMastery += SkillStatLogic.get_shield_mastery(cd);

			var nPDDr = nDarkForcePDDr + nPsdPDDr + nItemPDDr + nMastery;

			pIncinOrg = nTotalPDD * (nPsdPDDr + nItemPDDr) / 100;

			if (nPDDr != 0) nTotalPDD += nTotalPDD * nPDDr / 100;

			if (nTotalPDD <= 0) return 0;
			if (nTotalPDD >= short.MaxValue) return short.MaxValue;

			return 0;
		}

		public int GetPAD(Character cd, int nBulletItemID, int nPsdPADx, int nPsdPADr, int nIncPAD)
		{
			nIncPAD += GetIncPAD(cd) + GetIncEPAD(cd);
			var nTotalPAD = nItemPAD + nIncPAD + nPsdPADx;

			if (!JobLogic.is_mechanic_job(cd.Stats.nJob) && nBulletItemID != 0)
			{
				nTotalPAD += (MasterManager.ItemTemplates[nBulletItemID] as ConsumeItemTemplate)?.IncPAD ?? 0;
			}

			if (nComboAbilityBuff != 0)
			{
				var nComboSkillID = cd.Stats.nJob != 2000 // legend job
					? Skills.ARAN_COMBO_ABILITY
					: Skills.LEGEND_COMBO_ABILITY;

				if (cd.Skills.Get(nComboSkillID) is SkillEntry se)
				{
					var nComboPAD = se.Template.X(se.nSLV);

					if (nComboAbilityBuff / 10 < nComboPAD)
					{
						nComboPAD = nComboAbilityBuff;
					}

					nTotalPAD += (int)(nComboPAD * se.Template.Y(se.nSLV));
				}
			}

			var nPADr = nMoreWild
						+ nSwallowAttackDamage
						+ nDarkAura
						+ nMaxLevelBuff
						+ nPsdPADr
						+ nItemPADr;

			if (nPADr > 0) nTotalPAD += nTotalPAD * nPADr / 100;

			if (nTotalPAD >= 30_000) return 30_000;
			if (nTotalPAD <= 0) return 0;

			return nTotalPAD;
		}

		public int GetIncPAD(Character cd)
		{
			var nECValue = 0;

			if (cd.Stats.SecondaryStats.rEnergyCharged == 0) return nPAD;

			if (cd.Skills.Get(JobLogic.IsKOC(cd.Stats.nJob)
				? Skills.STRIKER_ENERGY_CHARGE
				: Skills.BUCCANEER_ENERGY_CHARGE) is SkillEntry se)
			{
				nPAD = se.Template.PAD(se.nSLV);
			}

			return Math.Max(nECValue, nPAD);
		}

		public int GetIncEPAD(Character cd)
		{
			var nECValue = 0;

			if (cd.Stats.SecondaryStats.rEnergyCharged == 0) return nEPAD;

			if (cd.Skills.Get(JobLogic.IsKOC(cd.Stats.nJob)
				? Skills.STRIKER_ENERGY_CHARGE
				: Skills.BUCCANEER_ENERGY_CHARGE) is SkillEntry se)
			{
				nEPAD = se.Template.EPAD(se.nSLV);
			}

			return Math.Max(nECValue, nEPAD);
		}

		public int GetMDD(Character cd, int nBaseMDD, int nPsdMDDr, bool bShieldEquipped, out int pIncinOrg)
		{
			var nTotalMDD = nBaseMDD + nItemMDD + nMDD + nEMDD;

			if (nComboAbilityBuff != 0)
			{
				var nComboSkillID = cd.Stats.nJob != 2000 // legend job
					? Skills.ARAN_COMBO_ABILITY
					: Skills.LEGEND_COMBO_ABILITY;

				if (cd.Skills.Get(nComboSkillID) is SkillEntry se)
				{
					var nComboMDD = se.Template.X(se.nSLV);

					if (nComboAbilityBuff / 10 < nComboMDD)
					{
						nComboMDD = nComboAbilityBuff;
					}

					nTotalMDD += (int)(nComboMDD * se.Template.Z(se.nSLV));
				}
			}

			var nMastery = 0;

			if (nBlueAura != 0)
			{
				if (cd.Skills.Get(Skills.BMAGE_AURA_BLUE_ADVANCED) is SkillEntry se)
				{
					nMastery = (int)se.Template.Z(se.nSLV);
				}
			}

			if (bShieldEquipped) nMastery += SkillStatLogic.get_shield_mastery(cd);

			var nMDDr = nPsdMDDr + nItemMDDr + nMastery;

			pIncinOrg = nTotalMDD * (nPsdMDDr + nItemMDDr) / 100;

			if (nMDDr != 0) nTotalMDD += nTotalMDD * nMDDr / 100;

			if (nTotalMDD <= 0) return 0;
			if (nTotalMDD >= 30_000) return 30_000;

			return 0;
		}

		public int GetMAD(int nPsdMADx, int nPsdMADr, int nDragonFury)
		{
			var nTotalMAD = nItemMAD + nPsdMADx + nMAD;

			var nMADr = nDragonFury
						+ nSwallowAttackDamage
						+ nDarkAura
						+ nMaxLevelBuff
						+ nPsdMADr
						+ nItemMADr;

			if (nMADr > 0) nTotalMAD += nTotalMAD * nMADr / 100;

			if (nTotalMAD >= 30_000) return 30_000;
			if (nTotalMAD <= 0) return 0;

			return nTotalMAD;
		}
	}
}
