using System;
using Rebirth.Characters.Stat;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Skill;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;

namespace Rebirth.Characters.Skill.Buff
{
	public class BuffSkill : AbstractBuff
	{
		public SkillTemplate Template => MasterManager.SkillTemplates[nSkillID];

		public BuffSkill(int nSkillID, byte nSLV)
			: base(nSkillID, nSLV) { }

		/// <summary>
		/// Temp function while I figure out best way to handle this shit
		/// </summary>
		public void GenerateAuraSkill(SecondaryStatFlag nAuraType, int tDurationMillis = -1000)
		{
			StartTime = DateTime.Now;

			tDuration = tDurationMillis;

			AddSecondaryStat(nAuraType, nSLV);
			// we want to send the given stat type, but internally we only use aura and superbody
			switch (nAuraType)
			{
				case SecondaryStatFlag.Aura:
					StatType = SecondaryStatFlag.Aura;
					BuffStoreType = BuffStoreType.FireAndForget;
					break;
				case SecondaryStatFlag.BlueAura:
				case SecondaryStatFlag.YellowAura:
				case SecondaryStatFlag.DarkAura:
					StatType = nAuraType;
					BuffStoreType = BuffStoreType.Normal;
					break;
				case SecondaryStatFlag.SuperBody:
					StatType = SecondaryStatFlag.SuperBody;
					BuffStoreType = BuffStoreType.Normal;
					break;
				default:
					throw new InvalidOperationException($"Unable to generate aura buff ({nSkillID} - {nSLV}) with secondary stat type: {nameof(nAuraType)}");
			}
		}

		public void GenerateSpiritJavelin(int nItemID)
		{
			StartTime = DateTime.Now;

			tDuration = Template.Time(nSLV) * 1000;

			// v5->m_secondaryStat.nSpiritJavelin_ = v32 % 10000 + 1;
			AddSecondaryStat(SecondaryStatFlag.SpiritJavelin, nItemID % 10000 + 1);
			StatType = SecondaryStatFlag.SpiritJavelin;
		}

		public void GenerateDice()
		{
			var sd = Template[nSLV];

			StartTime = DateTime.Now;

			tDuration = Template.Time(nSLV) * 1000;

			AddSecondaryStat(SecondaryStatFlag.Dice, State);
			StatType = SecondaryStatFlag.Dice;

			//v2->m_pPassiveSkillData.p->nMHPr += v18->aDiceInfo[0];
			//v2->m_pPassiveSkillData.p->nMMPr += v18->aDiceInfo[1];
			//v2->m_pPassiveSkillData.p->nCr += v18->aDiceInfo[2];
			//v2->m_pPassiveSkillData.p->nCDMin += v18->aDiceInfo[3];
			//v2->m_pPassiveSkillData.p->nEVAr += v18->aDiceInfo[5];
			//v2->m_pPassiveSkillData.p->nAr += v18->aDiceInfo[6];
			//v2->m_pPassiveSkillData.p->nEr += v18->aDiceInfo[7];
			//v2->m_pPassiveSkillData.p->nPDDr += v18->aDiceInfo[8];
			//v2->m_pPassiveSkillData.p->nMDDr += v18->aDiceInfo[9];
			//v2->m_pPassiveSkillData.p->nPDr += v18->aDiceInfo[10];
			//v2->m_pPassiveSkillData.p->nMDr += v18->aDiceInfo[11];
			//v2->m_pPassiveSkillData.p->nDIPr += v18->aDiceInfo[12];
			//v2->m_pPassiveSkillData.p->nPDamr += v18->aDiceInfo[13];
			//v2->m_pPassiveSkillData.p->nMDamr += v18->aDiceInfo[14];
			//v2->m_pPassiveSkillData.p->nPADr += v18->aDiceInfo[15];
			//v2->m_pPassiveSkillData.p->nMADr += v18->aDiceInfo[16];
			//v2->m_pPassiveSkillData.p->nEXPr += v18->aDiceInfo[17];
			//v2->m_pPassiveSkillData.p->nIMPr += v18->aDiceInfo[18];
			//v2->m_pPassiveSkillData.p->nASRr += v18->aDiceInfo[19];
			//v2->m_pPassiveSkillData.p->nTERr += v18->aDiceInfo[20];
			//v2->m_pPassiveSkillData.p->nMESOr += v18->aDiceInfo[21];

			switch (State)
			{
				case 2: // 30% Physical Defense
					Stat.aDiceInfo[8] = sd.PDDr;
					//v2->m_pPassiveSkillData.p->nPDDr += v18->aDiceInfo[8];
					break;
				case 3: // 20% HP and MP Increase
					Stat.aDiceInfo[0] = sd.MHPr;
					Stat.aDiceInfo[1] = sd.MMPr;
					//v2->m_pPassiveSkillData.p->nMHPr += v18->aDiceInfo[0];
					//v2->m_pPassiveSkillData.p->nMMPr += v18->aDiceInfo[1];
					break;
				case 4: // 15% Critical Rate
					Stat.aDiceInfo[2] = sd.Cr;
					//v2->m_pPassiveSkillData.p->nCr += v18->aDiceInfo[2];
					break;
				case 5: // 20% Damage Increase
					Stat.aDiceInfo[12] = sd.DamR;
					//v2->m_pPassiveSkillData.p->nDIPr += v18->aDiceInfo[12]; // DIPr is DamR in wz
					break;
				case 6: // 30% Experience Boost
					Stat.aDiceInfo[17] = sd.EXPr;
					//v2->m_pPassiveSkillData.p->nEXPr += v18->aDiceInfo[17];
					break;
				default: // or throw??
					State = 1;
					break;
			}

			Stat.nDiceStat = State;
			Stat.tDelay = 1000;
		}

		public override void Generate(double dBufftimeModifier = 1.0)
		{
			StartTime = DateTime.Now;

			tDuration = Template.Time(nSLV) * 1000;

			if (dBufftimeModifier > 1)
				tDuration = (int)(tDuration * dBufftimeModifier);

			if (tDuration <= 0)
			{
				tDuration = -1000;
			}

			if (Template.PAD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.PAD, Template.PAD(nSLV));
			}

			if (Template.PDD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.PDD, Template.PDD(nSLV));
			}

			if (Template.MAD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.MAD, Template.MAD(nSLV));
			}

			if (Template.MDD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.MDD, Template.MDD(nSLV));
			}

			if (Template.ACC(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.ACC, Template.ACC(nSLV));
			}

			if (Template.EVA(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EVA, Template.ACC(nSLV));
			}

			if (Template.Speed(nSLV) > 0 && nSkillID != (int)Skills.BMAGE_AURA_YELLOW) // this debuffs mobs
			{
				AddSecondaryStat(SecondaryStatFlag.Speed, Template.Speed(nSLV));
			}

			if (Template.Jump(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.Jump, Template.Jump(nSLV));
			}

			if (Template.Morph > 0)
			{
				if ((int)Skills.INFIGHTER_OAK_CASK == nBuffID)
				{
					AddSecondaryStat(SecondaryStatFlag.Morph, Template.Morph);
				}
				else
				{
					AddSecondaryStat(SecondaryStatFlag.Morph, Template.Morph + 100 * State);
				}
				StatType = SecondaryStatFlag.Morph;
			}

			if (Template.EMDD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EMDD, Template.EMDD(nSLV));
			}

			if (Template.EMHP(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EMHP, Template.EMHP(nSLV));
			}

			if (Template.EMMP(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EMMP, Template.EMMP(nSLV));
			}

			if (Template.EPAD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EPAD, Template.EPAD(nSLV));
			}

			if (Template.EPDD(nSLV) > 0)
			{
				AddSecondaryStat(SecondaryStatFlag.EPDD, Template.EPDD(nSLV));
			}

			// All skill-specific handling should be after this point

			if (Template.IsHyperBodySkill)
			{
				var paramA = Template.ParameterA(nSLV);
				AddSecondaryStat(SecondaryStatFlag.MaxHP, paramA);
				AddSecondaryStat(SecondaryStatFlag.MaxMP, paramA);
				StatType = SecondaryStatFlag.HyperBody;
				return;
			}

			if (Template.IsDashSkill)
			{
				Stat.Add(SecondaryStatFlag.Dash_Speed,
					new TwoStateSecondaryStatEntry(true)
					{
						nValue = Template.ParameterA(nSLV),
						rValue = nBuffID,
						tValue = tDuration,
						m_usExpireTerm = (short)(tDuration / 1000)
					});
				Stat.Add(SecondaryStatFlag.Dash_Jump,
					new TwoStateSecondaryStatEntry(true)
					{
						nValue = Template.ParameterB(nSLV),
						rValue = nBuffID,
						tValue = tDuration,
						m_usExpireTerm = (short)(tDuration / 1000)
					});
				StatType = SecondaryStatFlag.Dash;
				return;
			}

			if (Template.IsStanceSkill)
			{
				AddSecondaryStat(SecondaryStatFlag.Stance, Template.Prop(nSLV));
				StatType = SecondaryStatFlag.Stance;
				return;
			}

			if (Template.IsWeaponBoosterSkill)
			{
				if ((int)Skills.ARAN_POLEARM_BOOSTER == nSkillID)
				{
					AddSecondaryStat(SecondaryStatFlag.Booster, -Template.ParameterB(nSLV));
				}
				else
				{
					AddSecondaryStat(SecondaryStatFlag.Booster, Template.ParameterA(nSLV));
				}
				StatType = SecondaryStatFlag.Booster;
				return;
			}

			if (Template.IsWeaponChargeSkill)
			{
				AddSecondaryStat(SecondaryStatFlag.WeaponCharge, Template.ParameterA(nSLV));
				StatType = SecondaryStatFlag.WeaponCharge;
				return;
			}

			if (Template.IsMapleWarriorSkill)
			{
				AddSecondaryStat(SecondaryStatFlag.BasicStatUp, Template.ParameterA(nSLV)); // todo revise
				StatType = SecondaryStatFlag.BasicStatUp;
				return;
			}

			if (Template.IsSharpEyesSkill)
			{
				AddSecondaryStat(SecondaryStatFlag.SharpEyes,
					 (Template.ParameterA(nSLV) << 8) | Template.CriticalDamageMax(nSLV)); // BMS
				StatType = SecondaryStatFlag.SharpEyes;
				return;
			}

			if (SkillLogic.IsBeginnerSkill(nBuffID))
			{
				int vehicle;
				switch (nBuffID % 10000)
				{
					case 0001004:
						{
							// TODO grab this value from the ItemID in the tamingmob slot
							vehicle = 01902000;
							break;
						}
					case 0001013: vehicle = 01932001; break;
					case 0001017: vehicle = 01932003; break;
					case 0001018: vehicle = 01932004; break;
					case 0001019: vehicle = 01932005; break;
					case 0001025: vehicle = 01932006; break;
					case 0001027: vehicle = 01932007; break;
					case 0001028: vehicle = 01932008; break;
					case 0001029: vehicle = 01932009; break;
					case 0001030: vehicle = 01932011; break;
					case 0001031: vehicle = 01932010; break;
					case 0001032: vehicle = 01932012; break;
					case 0001033: vehicle = 01932013; break;
					case 0001034: vehicle = 01932014; break;
					case 0001035: vehicle = 01932012; break;
					case 0001036: vehicle = 01932017; break;
					case 0001037: vehicle = 01932018; break;
					case 0001038: vehicle = 01932019; break;
					case 0001039: vehicle = 01932020; break;
					case 0001040: vehicle = 01932021; break;
					case 0001042: vehicle = 01932022; break;
					case 0001044: vehicle = 01932023; break;
					case 0001046: vehicle = 01932002; break;
					case 0001049: vehicle = 01932025; break;
					case 0001050: vehicle = 01932003; break;
					case 0001051: vehicle = 01932026; break;
					case 0001052: vehicle = 01932027; break;
					case 0001053: vehicle = 01932028; break;
					case 0001054: vehicle = 01932029; break;
					case 0001063: vehicle = 01932034; break;
					case 0001064: vehicle = 01932035; break;
					case 0001065: vehicle = 01932037; break;
					case 0001069: vehicle = 01932038; break;
					case 0001070: vehicle = 01932039; break;
					case 0001071: vehicle = 01932040; break;
					default: vehicle = 0; break;
				}

				if (vehicle != 0)
				{
					tDuration = -1000;

					var ts = new TwoStateSecondaryStatEntry(false)
					{
						nValue = vehicle,
						rValue = nBuffID,
						tValue = tDuration
					};

					Stat.Add(SecondaryStatFlag.RideVehicle, ts);
					StatType = SecondaryStatFlag.RideVehicle;
					return;
				}
			}

			switch ((Skills)nBuffID)
			{
				case Skills.EVANJR_REGENERATION:
				case Skills.LEGEND_REGENERATION:
				case Skills.NOBLESSE_REGENERATION:
				case Skills.NOVICE_REGENERATION:
				case Skills.KNIGHT_RESTORATION:
					AddSecondaryStat(SecondaryStatFlag.Regen, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Regen;
					break;
				case Skills.CITIZEN_BERSERK:
				case Skills.EVANJR_BERSERK:
				case Skills.LEGEND_BERSERK:
				case Skills.NOBLESSE_BERSERK:
				case Skills.NOVICE_BERSERK:
					AddSecondaryStat(SecondaryStatFlag.DojangBerserk, 1);
					StatType = SecondaryStatFlag.DojangBerserk;
					break;
				case Skills.CITIZEN_INVINCIBLE:
				case Skills.EVANJR_INVINCIBLE:
				case Skills.LEGEND_INVINCIBLE:
				case Skills.NOBLESSE_INVINCIBLE:
				case Skills.NOVICE_INVINCIBLE:
					AddSecondaryStat(SecondaryStatFlag.DojangInvincible, 1);
					StatType = SecondaryStatFlag.DojangInvincible;
					break;
				case Skills.EVAN_KILLING_WING:
				case Skills.CAPTAIN_ADVANCED_HOMING:
				case Skills.VALKYRIE_HOMING:
					Stat.Add(SecondaryStatFlag.GuidedBullet, new TwoStateSecondaryStatEntry_GuidedBullet()
					{
						nValue = Template.ParameterA(nSLV),
						rValue = nBuffID,
						tValue = tDuration,
						m_dwMobID = dwMobId
					});
					StatType = SecondaryStatFlag.GuidedBullet;
					break;
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
				case Skills.SNIPER_GOLDEN_EAGLE:
					AddSecondaryStat(SecondaryStatFlag.Beholder, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Beholder;
					break;
				case Skills.FIGHTER_POWER_GUARD:
				case Skills.PAGE_POWER_GUARD:
					AddSecondaryStat(SecondaryStatFlag.PowerGuard, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.PowerGuard;
					break;
				case Skills.DRAGONKNIGHT_DRAGON_BLOOD:
					AddSecondaryStat(SecondaryStatFlag.DragonBlood, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.DragonBlood;
					break;
				case Skills.FLAMEWIZARD_MAGIC_GUARD:
				case Skills.MAGICIAN_MAGIC_GUARD:
				case Skills.EVAN_MAGIC_GUARD:
					AddSecondaryStat(SecondaryStatFlag.MagicGuard, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.MagicGuard;
					break;
				case Skills.CLERIC_INVINCIBLE:
					AddSecondaryStat(SecondaryStatFlag.Invincible, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Invincible;
					break;
				case Skills.EVAN_ELEMENTAL_RESET:
				case Skills.FLAMEWIZARD_ELEMENTAL_RESET:
				case Skills.MAGE1_ELEMENTAL_RESET:
				case Skills.MAGE2_ELEMENTAL_RESET:
					AddSecondaryStat(SecondaryStatFlag.ElementalReset, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.ElementalReset;
					break;
				case Skills.ADMIN_HOLY_SYMBOL:
				case Skills.PRIEST_HOLY_SYMBOL:
					AddSecondaryStat(SecondaryStatFlag.HolySymbol, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.HolySymbol;
					break;
				case Skills.BISHOP_HOLY_SHIELD:
					AddSecondaryStat(SecondaryStatFlag.Holyshield, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Holyshield;
					break;
				case Skills.ARCHMAGE1_MANA_REFLECTION:
				case Skills.ARCHMAGE2_MANA_REFLECTION:
				case Skills.BISHOP_MANA_REFLECTION:
					AddSecondaryStat(SecondaryStatFlag.ManaReflection, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.ManaReflection;
					break;
				case Skills.ARCHMAGE1_INFINITY:
				case Skills.ARCHMAGE2_INFINITY:
				case Skills.BISHOP_INFINITY:
					AddSecondaryStat(SecondaryStatFlag.Infinity, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Infinity;
					break;
				case Skills.CROSSBOWMAN_SOUL_ARROW_CROSSBOW:
				case Skills.HUNTER_SOUL_ARROW_BOW:
				case Skills.WILDHUNTER_SOUL_ARROW_CROSSBOW:
				case Skills.WINDBREAKER_SOUL_ARROW_BOW:
					AddSecondaryStat(SecondaryStatFlag.SoulArrow, Template.ParameterA(nSLV)); // x = 1
					StatType = SecondaryStatFlag.SoulArrow;
					break;
				case Skills.BOWMASTER_HAMSTRING:
					AddSecondaryStat(SecondaryStatFlag.HamString, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.HamString;
					break;
				case Skills.CROSSBOWMASTER_BLIND:
				case Skills.WILDHUNTER_BLIND:
				case Skills.UNRECORDED_BLIND: // ?
					AddSecondaryStat(SecondaryStatFlag.Blind, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Blind;
					break;
				case Skills.BOWMASTER_CONCENTRATION:
					AddSecondaryStat(SecondaryStatFlag.Concentration, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Concentration;
					break;
				case Skills.ROGUE_DARK_SIGHT:
				case Skills.DUAL4_ADVANCED_DARK_SIGHT:
				case Skills.NIGHTWALKER_DARK_SIGHT:
					AddSecondaryStat(SecondaryStatFlag.DarkSight, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.DarkSight;
					break;
				case Skills.HERMIT_MESO_UP:
				case Skills.GUILD_MESOUP:
					AddSecondaryStat(SecondaryStatFlag.MesoUp, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.MesoUp;
					State = (short)(Template.ParameterA(nSLV) - 100);
					break;
				case Skills.HERMIT_SHADOW_PARTNER:
				case Skills.NIGHTWALKER_SHADOW_PARTNER:
				case Skills.THIEFMASTER_SHADOW_PARTNER:
				case Skills.DUAL4_MIRROR_IMAGING:
					AddSecondaryStat(SecondaryStatFlag.ShadowPartner, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.ShadowPartner;
					break;
				case Skills.THIEFMASTER_PICKPOCKET:
					AddSecondaryStat(SecondaryStatFlag.PickPocket, Template.ParameterA(nSLV) - 100);
					StatType = SecondaryStatFlag.PickPocket;
					break;
				case Skills.THIEFMASTER_MESO_GUARD:
					AddSecondaryStat(SecondaryStatFlag.MesoGuard, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.MesoGuard;
					break;
				case Skills.STRIKER_SPARK:
					AddSecondaryStat(SecondaryStatFlag.Spark, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Spark;
					break;
				case Skills.SOULMASTER_FINAL_ATTACK_SWORD:
					AddSecondaryStat(SecondaryStatFlag.SoulMasterFinal, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.SoulMasterFinal;
					break;
				case Skills.WINDBREAKER_FINAL_ATTACK_BOW:
					AddSecondaryStat(SecondaryStatFlag.WindBreakerFinal, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.WindBreakerFinal;
					break;
				case Skills.WINDBREAKER_WIND_WALK:
					AddSecondaryStat(SecondaryStatFlag.WindWalk, 1); // verified
					StatType = SecondaryStatFlag.WindWalk;
					break;
				case Skills.ARAN_COMBO_ABILITY:
					AddSecondaryStat(SecondaryStatFlag.ComboAbilityBuff, State);
					StatType = SecondaryStatFlag.ComboAbilityBuff;
					break;
				case Skills.ARAN_COMBO_DRAIN:
				case Skills.BMAGE_BLOOD_DRAIN: // todo figure out the actual cts for this
					AddSecondaryStat(SecondaryStatFlag.ComboDrain, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.ComboDrain;
					break;
				case Skills.ARAN_COMBO_BARRIER:
					AddSecondaryStat(SecondaryStatFlag.ComboBarrier, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.ComboBarrier;
					break;
				case Skills.ARAN_BODY_PRESSURE:
					AddSecondaryStat(SecondaryStatFlag.BodyPressure, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.BodyPressure;
					break;
				case Skills.ARAN_SMART_KNOCKBACK:
					AddSecondaryStat(SecondaryStatFlag.SmartKnockback, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.SmartKnockback;
					break;
				case Skills.EVAN_SLOW:
					AddSecondaryStat(SecondaryStatFlag.EvanSlow, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.EvanSlow;
					break;
				case Skills.EVAN_MAGIC_SHIELD:
					AddSecondaryStat(SecondaryStatFlag.MagicShield, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.MagicShield;
					break;
				case Skills.EVAN_MAGIC_RESISTANCE:
					AddSecondaryStat(SecondaryStatFlag.MagicResistance, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.MagicResistance;
					break;
				case Skills.PALADIN_DIVINE_SHIELD:
					AddSecondaryStat(SecondaryStatFlag.BlessingArmor, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.BlessingArmor;
					break;
				case Skills.KNIGHT_COMBAT_ORDERS:
					AddSecondaryStat(SecondaryStatFlag.CombatOrders, State); // setting nSLV to the amount cuz rounding issues
					StatType = SecondaryStatFlag.CombatOrders;
					break;
				case Skills.CRUSADER_COMBO_ATTACK:
				case Skills.SOULMASTER_COMBO_ATTACK:
					AddSecondaryStat(SecondaryStatFlag.ComboCounter, State); // x is current combo orbs 
					StatType = SecondaryStatFlag.ComboCounter;
					break;
				case Skills.MECHANIC_SG88:
					tDuration = -1000;
					goto case Skills.RANGER_PUPPET;
				case Skills.RANGER_PUPPET:
				case Skills.SNIPER_PUPPET:
				case Skills.WINDBREAKER_PUPPET:
				case Skills.CAPTAIN_SUPPORT_OCTOPUS:
				case Skills.VALKYRIE_OCTOPUS:
					AddSecondaryStat(SecondaryStatFlag.Beholder, 1);
					StatType = SecondaryStatFlag.Beholder;
					break;
				case Skills.DUAL4_OWL_DEATH:
					AddSecondaryStat(SecondaryStatFlag.SuddenDeath, Template.ParameterB(nSLV));
					StatType = SecondaryStatFlag.SuddenDeath;
					break;
				case Skills.DUAL5_THORNS_EFFECT:
					AddSecondaryStat(SecondaryStatFlag.ThornsEffect, (Template.ParameterA(nSLV) << 8) | Template.CriticalDamageMin(nSLV));
					StatType = SecondaryStatFlag.ThornsEffect;
					break;
				case Skills.DUAL5_FINAL_CUT:
					AddSecondaryStat(SecondaryStatFlag.DamR, Template.DamR(nSLV));
					AddSecondaryStat(SecondaryStatFlag.FinalCut, Template.ParameterB(nSLV));
					StatType = SecondaryStatFlag.FinalCut;
					break;
				case Skills.BMAGE_CONVERSION:
					AddSecondaryStat(SecondaryStatFlag.Conversion, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Conversion;
					break;
				case Skills.BMAGE_REVIVE:
					AddSecondaryStat(SecondaryStatFlag.Revive, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Revive;
					break;
				case Skills.BMAGE_TELEPORT_MASTERY:
				case Skills.MAGE1_TELEPORT_MASTERY:
				case Skills.MAGE2_TELEPORT_MASTERY:
				case Skills.PRIEST_TELEPORT_MASTERY:
					tDuration = -1000;
					AddSecondaryStat(SecondaryStatFlag.TeleportMasteryOn, nSLV);
					StatType = SecondaryStatFlag.TeleportMasteryOn;
					break;
				case Skills.BMAGE_CYCLONE:
					AddSecondaryStat(SecondaryStatFlag.Cyclone, 1);
					StatType = SecondaryStatFlag.Cyclone;
					break;
				case Skills.MECHANIC_HN07:
				case Skills.MECHANIC_HN07_UPGRADE:
				//case Skills.CITIZEN_MONSTER_RIDING:
				//case Skills.EVANJR_MONSTER_RIDING:
				//case Skills.LEGEND_MONSTER_RIDING:
				//case Skills.NOBLESSE_MONSTER_RIDING:
				//case Skills.NOVICE_MONSTER_RIDING:
				case Skills.WILDHUNTER_JAGUAR_RIDING:
				case Skills.CAPTAIN_BATTLESHIP:
					tDuration = -1000;

					var ts = new TwoStateSecondaryStatEntry(false)
					{
						nValue = SkillLogic.GetVehicleId(nBuffID),
						rValue = nBuffID,
						tValue = tDuration
					};

					Stat.Add(SecondaryStatFlag.RideVehicle, ts);
					StatType = SecondaryStatFlag.RideVehicle;
					break;
				case Skills.WILDHUNTER_MOREWILD:
					AddSecondaryStat(SecondaryStatFlag.MorewildMaxHP, Template.ParameterA(nSLV));
					AddSecondaryStat(SecondaryStatFlag.MorewildDamageUp, Template.ParameterB(nSLV));
					AddSecondaryStat(SecondaryStatFlag.Speed, (int)Template.Z(nSLV));
					StatType = SecondaryStatFlag.MoreWild;
					break;
				case Skills.WILDHUNTER_MINE:
					AddSecondaryStat(SecondaryStatFlag.Mine, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Mine;
					break;
				case Skills.WILDHUNTER_SWALLOW_DUMMY_BUFF:
					var swallowtypes = new SecondaryStatFlag[5]
					{
						SecondaryStatFlag.SwallowDefence,
						SecondaryStatFlag.SwallowMaxMP,
						SecondaryStatFlag.SwallowAttackDamage,
						SecondaryStatFlag.SwallowCritical,
						SecondaryStatFlag.SwallowEvasion,
					};
					StatType = swallowtypes.Random();

					switch (StatType)
					{
						case SecondaryStatFlag.SwallowDefence:
						case SecondaryStatFlag.SwallowMaxMP:
							AddSecondaryStat(StatType, Template.ParameterA(nSLV)); // x = 3 * nSLV
							break;
						default:
							AddSecondaryStat(StatType, Template.ParameterB(nSLV)); // y = 1 * nSLV
							break;
					}

					Stat.tSwallowBuffTime = (byte)Template.Time(nSLV); // seconds
					break;
				case Skills.MECHANIC_PERFECT_ARMOR:
					AddSecondaryStat(SecondaryStatFlag.Guard, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Guard;
					break;
				case Skills.MECHANIC_FLAMETHROWER:
				case Skills.MECHANIC_FLAMETHROWER_UP:
					tDuration = -1000;
					AddSecondaryStat(SecondaryStatFlag.Mechanic, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Mechanic;
					break;
				case Skills.MECHANIC_SIEGE1: // siege mode
				case Skills.MECHANIC_SIEGE2_SPECIAL:
					AddSecondaryStat(SecondaryStatFlag.Mechanic, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Mechanic;
					break;
				case Skills.MECHANIC_SIEGE2: // missile tank
					AddSecondaryStat(SecondaryStatFlag.Mechanic, Template.ParameterA(nSLV));
					StatType = SecondaryStatFlag.Mechanic;
					break;
				case Skills.MECHANIC_SAFETY:
					AddSecondaryStat(SecondaryStatFlag.SafetyDamage, Template.ParameterA(nSLV));
					AddSecondaryStat(SecondaryStatFlag.SafetyAbsorb, Template.ParameterB(nSLV));
					StatType = SecondaryStatFlag.SafetyDamage;
					break;
				case Skills.MECHANIC_AR_01:
					tDuration = -1000; // TODO consider if this is PAD or EPAD or DAMR
					AddSecondaryStat(SecondaryStatFlag.DamR, Template.ParameterA(nSLV));
					break;
				case Skills.CAPTAIN_COUNTER_ATTACK:
					AddSecondaryStat(SecondaryStatFlag.DamR, Template.DamR(nSLV));
					break;
				case Skills.BUCCANEER_ENERGY_CHARGE:
				case Skills.STRIKER_ENERGY_CHARGE:
					Stat.Add(SecondaryStatFlag.EnergyCharged,
						new TwoStateSecondaryStatEntry(true)
						{
							nValue = State,
							rValue = nBuffID,
							tValue = tDuration,
							m_usExpireTerm = (short)(tDuration / 1000)
						});
					break;
				case Skills.VIPER_WIND_BOOSTER:
				case Skills.STRIKER_WIND_BOOSTER:
					Stat.Add(SecondaryStatFlag.PartyBooster,
						new TwoStateSecondaryStatEntry_PartyBooster()
						{
							nValue = -2, // static attack speed increase value
							rValue = nBuffID,
							tValue = tDuration,
							//tCurrentTime = DateTime.Now.Ticks,
							m_usExpireTerm = (short)(tDuration / 1000)
						});
					StatType = SecondaryStatFlag.PartyBooster;
					break;
				case Skills.WILDHUNTER_NERVEGAS:
					AddSecondaryStat(SecondaryStatFlag.PDD, Template.ParameterA(nSLV));
					break;
				case Skills.HERO_ENRAGE:
					{
						AddSecondaryStat(SecondaryStatFlag.DamR, Template.ParameterA(nSLV));
					}
					break;
			}

			if (StatType == 0) StatType = SecondaryStatFlag.None_DONT_USE;
		}
	}
}