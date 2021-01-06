using log4net;
using Npgsql;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Skill.ActiveSkill;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Characters.Stat;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Tools;

namespace Rebirth.Characters.Skill
{
	public class CharacterSkills : NumericKeyedCollection<SkillEntry>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CharacterSkills));

		// ------------------------------------

		public static void Handle_SkillUpRequest(WvsGameClient c, CInPacket p)
		{
			// TODO validate that skill exists in player job

			var dwTickCount = p.Decode4();
			var nSkillId = p.Decode4();

			var template = MasterManager.SkillTemplates[nSkillId];

			if (template is null)
			{
				c.Character.SendMessage($"Skill template null for input value {nSkillId}");
				return;
			}

			if (template.IsHiddenSkill)
			{
				c.Character.SendMessage("Unable to add points to hidden skills.");
				return;
			}

			foreach (var item in template.Req)
			{
				if (c.Character.Skills.Get(item.Key)?.nSLV < item.Value)
				{
					c.Character.SendMessage($"Missing pre-required skills. {item.Key} - {item.Value}");
					return;
				}
			}

			var skill = c.Character.Skills[nSkillId];

			if (skill != null && skill.nSLV >= skill.MaxLevel)
			{
				c.Character.SendMessage($"Skill level higher than allowed max level. SLV: {skill.nSLV}. MaxLevel: {skill.MaxLevel}.");
				return;
			}

			if (template.is_skill_need_master_level && template.MasterLevel != 0 &&
				(skill is null || skill.nSLV >= skill.CurMastery))
			{
				c.Character.SendMessage($"Needs Master Level: {template.is_skill_need_master_level}. MaxLevel: {skill.MaxLevel}. SLV: {skill.nSLV}. CurMastery: {skill.CurMastery}.");
				return;
			}

			if (!SkillLogic.IsBeginnerSkill(nSkillId))
			{
				var sp = c.Character.Stats.GetSpBySkillID(nSkillId);
				if (sp <= 0)
				{
					c.Character.SendMessage($"Not enough skill points. ({sp}).");
					return;
				}

				c.Character.Modify.Stats(ctx =>
				{
					ctx.ReduceSpBySkillID(nSkillId);
				});
			}

			c.Character.Modify.Skills(mod =>
				mod.AddEntry(nSkillId, entry =>
					entry.nSLV += 1));

			c.Character.Action.Enable();
		}

		public static void Handle_SkillUseRequest(WvsGameClient c, CInPacket p)
		{
			// Recv [CP_UserSkillUseRequest] [67 00] [86 1E 31 2F] [9B BA 3E 00] [14] 02 12 27 00 00 23 27 00 00 C2 01
			var dwTickCount = p.Decode4();
			var nSkillID = p.Decode4();
			var nSLV = p.Decode1();

			var pSkill = c.Character.Skills.Get(nSkillID, true);

			// do validation here so we dont need to validate in called functions
			if (pSkill is null || nSLV > pSkill.nSLV) return;

			if (SkillLogic.IsMobCaptureSkill(nSkillID))
			{
				ActiveSkill_MobCapture.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsSummonCapturedMobSkill(nSkillID))
			{
				ActiveSkill_SummonMonster.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsHookAndHitSkill(nSkillID))
			{
				ActiveSkill_HookAndHit.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsSmokeShellSkill(nSkillID))
			{
				ActiveSkill_SmokeShell.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsDamageMeterSkill(nSkillID, c.Character.Stats.nJob))
			{
				ActiveSkill_DamageMeter.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsFlyingSkill(nSkillID, c.Character.Stats.nJob))
			{
				ActiveSkill_Flying.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsClericHealSkill(nSkillID))
			{
				ActiveSkill_Heal.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsOpenGateSkill(nSkillID))
			{
				ActiveSkill_OpenGate.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsRecoveryAura(nSkillID))
			{
				ActiveSkill_RecoveryAura.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsMysticDoorSkill(nSkillID, c.Character.Stats.nJob) || nSkillID == (int)Skills.PRIEST_MYSTIC_DOOR)
			{
				ActiveSkill_TownPortal.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (SkillLogic.IsStatChangeAdminSkill(nSkillID, c.Character.Stats.nJob))
			{
				ActiveSkill_StatChangeAdmin.Handle(nSkillID, nSLV, c.Character, p);
			}
			else if (pSkill.Template.IsSummonSkill && nSkillID != (int)Skills.BMAGE_REVIVE)
			{
				ActiveSkill_Summon.Handle(nSkillID, nSLV, c.Character, p);
			}
			else
			{
				var bLeft = false;

				if (pSkill.Template.is_antirepeat_buff_skill)
				{
					var x = p.Decode2();
					var y = p.Decode2();

					bLeft = x < c.Character.Position.X;
				}

				//  Recv [CP_UserSkillUseRequest] [67 00] [4B 5E 91 0B] [CD 14 F9 01] 14

				var nSpiritJavelinItemID = 0;
				if (nSkillID == (int)Skills.NIGHTLORD_SPIRIT_JAVELIN) //Spirit Claw
					nSpiritJavelinItemID = p.Decode4(); //nSpiritJavelinItemID

				if (pSkill.Template.is_event_vehicle_skill)
				{
					p.Skip(1); // dwAffectedMemberBitmap

					if (nSkillID == 2311001)
						p.Skip(2); // tDelay
				}

				var nRemaining = p.Available - 2;

				if (nRemaining > 0)
					p.Skip(nRemaining);

				//if (nRemaining > 0)
				//{
				//    var nMobCount = p.Decode1(); // nMobCount
				//    for (int i = 0; i < nMobCount; i++)
				//    {
				//        p.Decode4(); // adwMobID
				//    }
				//}

				var tDelay = p.Available >= 2 ? p.Decode2() : (short)0; // tDelay

				if (nSkillID == 0 || c.Character.Skills.Cast(nSkillID, bLeft, false, nSpiritJavelinItemID))
				{
					new UserEffectPacket(UserEffect.SkillUse)
					{
						nSkillID = nSkillID,
						nSLV = nSLV
					}.BroadcastEffect(c.Character, false);
				}
			}

			c.Character.Action.Enable();
		}

		public static void Handle_UserSkillLearnItemUseRequest(WvsGameClient c, CInPacket p)
		{
			int dwTickCount = p.Decode4();
			short nPOS = p.Decode2();
			int nItemID = p.Decode4();

			c.Character.Action.Enable();

			if (c.Character.Stats.nHP <= 0) return;

			if (InventoryManipulator.GetItem(c.Character, ItemConstants.GetInventoryType(nItemID), nPOS) is GW_ItemSlotBundle item
				&& item.Template is ConsumeItemTemplate template)
			{
				var jobroot = Math.Floor(c.Character.Stats.nJob * 0.01); // 3500 -> 35

				var bUsed = false;
				var bSuccess = false;
				var bIsMasterBook = item.nItemID / 10000 == 229;

				foreach (var skillId in template.SkillData)
				{
					var skillJob = (int)Math.Floor(skillId / 10000f); // 35111010 -> 3511
					var skillRoot = (int)Math.Floor(skillJob / 100f); // 3511 -> 35

					if (skillRoot == jobroot) // this can only be true once
					{
						if (skillJob > c.Character.Stats.nJob) break;

						var skill = c.Character.Skills.FirstOrDefault(s => s.nSkillID == skillId);

						if (bIsMasterBook)
						{
							if (skill is null || skill.nSLV < template.ReqSkillLevel || skill.CurMastery >= template.MasterLevel) return;
						}
						else
						{
							if (skill != null && skill.CurMastery > 0) break;
						}

						bUsed = true;

						if (template.SuccessRate != 100)
						{
							if (Constants.Rand.Next() % 100 > template.SuccessRate) break;
						}

						c.Character.Modify.Skills(mod => mod.AddEntry(skillId, s => s.CurMastery = (byte)template.MasterLevel));
						bSuccess = true;
						break;
					}
				}

				if (bUsed)
				{
					InventoryManipulator.RemoveFrom(c.Character, item.InvType, nPOS); // always remove
				}

				c.Character.Field.Broadcast(CPacket.SkillLearnItemResult(c.dwCharId, bIsMasterBook, bUsed, bSuccess));
			}
		}

		// ------------------------------------
		public Character Parent { get; private set; } //=> MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID => Parent.dwId;

		public int nDragonFury { get; set; } // evan 2216 -> TODO move this to Parent.Stats.SecondaryStat
		public bool bDarkForce { get; set; } // dark knight 132 -> TODO move this to Parent.Stats.SecondaryStat

		public CharacterSkills(Character parent)
		{
			Parent = parent;
		}

		public void Dispose()
		{
			Parent = null;
			Clear();
		}

		/// <summary>
		/// Returns a SkillEntry object if it exists in the
		///     character's skill inventory and satisfies
		///     all conditions (not null, above level 0, not 
		///     hidden unless returnHidden is true).
		/// This method should only be called from outside this class.
		///		Retrieving skill objects inside the CharacterSkills
		///     class should be done like: this[nSkillID]
		/// </summary>
		/// <param name="nSkillID">ID of skill to return</param>
		/// <param name="returnHidden">If it's acceptable to return a hidden skill</param>
		/// <returns>SkillEntry if conditions are met, otherwise null.</returns>
		public SkillEntry Get(int nSkillID, bool returnHidden = false)
		{
			var retVal = this[nSkillID];

			if (retVal is null)
				return null;

			if (retVal.nSLV <= 0)
				return null;

			if (retVal.Template.IsHiddenSkill && !returnHidden)
				return null;

			return retVal;
		}

		public SkillEntry Get(Skills nSkillID, bool returnHidden = false)
			=> Get((int)nSkillID, returnHidden); // yes im this lazy

		/// <summary>
		/// Returns the first SkillEntry from the given skill args, if one exists
		/// </summary>
		/// <param name="returnHidden"></param>
		/// <param name="skills"></param>
		/// <returns></returns>
		public SkillEntry Get(bool returnHidden, params int[] skills)
		{
			foreach (var skill in skills)
			{
				if (Get(skill, returnHidden) is SkillEntry se) return se;
			}

			return null;
		}

		public void MaxSkillLevel(params int[] skillIds)
		{
			Parent.Modify.Skills(mod =>
			{
				foreach (var nSkillId in skillIds)
				{
					mod.AddEntry(nSkillId, entry =>
					{
						entry.CurMastery = entry.MaxLevel;
						entry.nSLV = entry.MaxLevel;
					});
				}
			});
		}

		public void SetMasterLevels()
		{
			Parent.Modify.Skills(mod =>
			{
				foreach (var skill in MasterManager.SkillTemplates.GetJobSkills(Parent.Stats.nJob))
				{
					var masterlevel = MasterManager.SkillTemplates[skill].MasterLevel;
					var invis = MasterManager.SkillTemplates[skill].Invisible;

					if (masterlevel <= 0)
					{
						if (!invis) continue;
					}

					mod.AddEntry(skill, s =>
					{
						s.CurMastery = (byte)masterlevel;
					});
				}
			});
		}

		/// <summary>
		/// Handles skill casting. Will return false if skill is unable to cast for any reason, such as not enough resources or invalid skill.
		/// </summary>
		/// <param name="nSkillID"></param>
		/// <param name="bLeft">If the skill is happening to the left of the character</param>
		/// <param name="bOutsideHandling">When this is true, the function will only process the resource/cooldown portion.</param>
		/// <returns></returns>
		public bool Cast(int nSkillID, bool bLeft, bool bOutsideHandling = false, int nSpiritJavelinItemID = 0)
		{
			if (Parent.Stats.nHP <= 0) return false; // REEEEEEEEE

#if DEBUG
			Parent.SendMessage("Casting spell " + nSkillID);
#endif

			var skill = Get(nSkillID, true);

			if (skill == null)
			{
				Parent.SendMessage("Unable to find skill.");
				return false;
			}

			var template = skill.Template;
			var nSLV = skill.nSLV;

			if (Parent.Cooldowns.OnCooldown(nSkillID))
			{
				Parent.SendMessage("Trying to cast skill while skill on cooldown.");
				return false;
			}

			double costMp = skill.MPCost; // done
			var costMeso = template.MesoR(nSLV); // may not be done..
			var itemConsumeId = template.ItemConsume; // done
			if (Parent.Buffs.Contains((int)Skills.BISHOP_INFINITY)) // party buff
			{
				costMp = 0;
			}
			else
			{
				switch (Parent.Stats.nJob / 10)
				{
					case 21:
						if (Parent.Buffs.Contains((int)Skills.ARCHMAGE1_INFINITY))
						{
							costMp = 0;
							break;
						}
						costMp *= (Parent.Skills.Get((int)Skills.MAGE1_ELEMENT_AMPLIFICATION)?.X_Effect ?? 100.0) * 0.01;
						goto default;
					case 22:
						if (Parent.Buffs.Contains((int)Skills.ARCHMAGE2_INFINITY))
						{
							costMp = 0;
							break;
						}
						costMp *= (Parent.Skills.Get((int)Skills.MAGE2_ELEMENT_AMPLIFICATION)?.X_Effect ?? 100.0) * 0.01;
						goto default;
					case 121: // koc mage
						costMp *= (Parent.Skills.Get((int)Skills.FLAMEWIZARD_ELEMENT_AMPLIFICATION)?.X_Effect ?? 100.0) * 0.01;
						goto default;
					case 221: // evan
					case 222: // evan rounded up (2215-2218)
						costMp *= (Parent.Skills.Get((int)Skills.EVAN_ELEMENT_AMPLIFICATION)?.X_Effect ?? 100.0) * 0.01;
						goto default;
					case 31:
						if (Parent.Buffs[(int)Skills.BOWMASTER_CONCENTRATION] is BuffSkill pBuff)
						{
							costMp *= 1 - (pBuff.Template.X(pBuff.nSLV) * 0.01);
						}
						goto default;
					default:
						if (Parent.Stats.nMP < Math.Floor(costMp))
						{
							Log.Debug($"Not enough mp to perform skill. Current MP: {Parent.Stats.nMP}. MP required: {costMp}. SkillID: {nSkillID}. CharID: {Parent.dwId}. CharName: {Parent.Stats.sCharacterName}.");
							return false;
						}
						break;
				}
			}
			// calculate HP cost
			int costHp;
			switch ((Skills)nSkillID)
			{
				case Skills.DUAL5_FINAL_CUT:
				case Skills.DRAGONKNIGHT_DRAGON_ROAR:
				case Skills.DRAGONKNIGHT_SACRIFICE:
				case Skills.INFIGHTER_MP_RECOVERY:
					costHp = (int)(Parent.BasicStats.nMHP * template.X(skill.nSLV) * 0.01);
					break;
				default:
					costHp = 0;
					break;
			}

			if (Parent.Stats.nHP <= costHp)
			{
				Log.Debug($"Not enough hp to perform skill. Hp required: {costHp}");
				return false;
			}

			if (Parent.Stats.nMoney < costMeso)
			{
				Log.Debug($"Not enough meso to perform skill. Meso required: {costMeso}");
				return false;
			}

			var itemConsumeAmount = template.ItemConsumeAmount;

			if (nSkillID == (int)Skills.NIGHTLORD_SPIRIT_JAVELIN)
			{
				if (!ItemConstants.IsThrowingStar(nSpiritJavelinItemID)) return false;

				itemConsumeId = nSpiritJavelinItemID;
				itemConsumeAmount = 200; // we hardcodin' bois
			}

			if (itemConsumeId > 0 && itemConsumeAmount > 0)
			{
				var valid = false;
				var itemType = ItemConstants.GetInventoryType(itemConsumeId);
				foreach (var item in itemType == InventoryType.Etc ? Parent.InventoryEtc : Parent.InventoryConsume) // lol ternary operator in a loop
				{
					if (item.Value.nItemID == itemConsumeId && item.Value.nNumber >= itemConsumeAmount)
					{
						valid = true;
						InventoryManipulator.RemoveFrom(Parent, itemType, item.Key, (short)itemConsumeAmount);
						break;
					}
				}

				if (!valid) return false;
			}

			// energy skills
			switch ((Skills)nSkillID)
			{
				case Skills.BUCCANEER_ENERGY_BURSTER:
				case Skills.STRIKER_ENERGY_BURSTER:
				case Skills.BUCCANEER_ENERGY_DRAIN:
				case Skills.STRIKER_ENERGY_DRAIN:
				case Skills.VIPER_ENERGY_ORB:
					if (Parent.Combat.nEnergy != SkillLogic.EnergyMax)
					{
						Parent.SendMessage($"Insufficient energy. ({Parent.Combat.nEnergy} != {SkillLogic.EnergyMax}).");
						return false;
					}
					break;
			}

			Parent.Modify.Stats(ctx =>
			{
				if (costHp > 0)
					ctx.HP -= costHp;

				if (costMp > 0)
					ctx.MP -= (int)Math.Floor(costMp);

				if (costMeso > 0)
					ctx.Money -= costMeso;
			});

			if (Parent.nPreparedSkill == 0)
			{
				var nCdSkillId = nSkillID;
				var nCdTime = template.Cooltime(skill.nSLV);

				switch ((Skills)nSkillID)
				{
					case Skills.MECHANIC_SAFETY:
						{
							if (!Parent.Field.Summons.Any(s => s.dwParentID == dwParentID
							&& (s.nSkillID == (int)Skills.MECHANIC_SATELITE || s.nSkillID == (int)Skills.MECHANIC_SATELITE2 || s.nSkillID == (int)Skills.MECHANIC_SATELITE3)))
							{
								return false; // satellites must be active to cast this skill
							}
						}
						break;
					case Skills.WILDHUNTER_SWALLOW:
					case Skills.MECHANIC_TESLA_COIL:
						nCdTime = 0;
						break;
					case Skills.WILDHUNTER_SWALLOW_DUMMY_ATTACK:
					case Skills.WILDHUNTER_SWALLOW_DUMMY_BUFF:
						var pTempTemplate = Parent.Skills.Get((int)Skills.WILDHUNTER_SWALLOW);
						nCdSkillId = pTempTemplate.nSkillID;
						nCdTime = (short)pTempTemplate.CoolTimeSeconds;
						break;
				}

				if (nCdTime > 0)
				{
					Parent.Cooldowns.UpdateOrInsert(nCdSkillId, (short)nCdTime);
				}
			}

			if (template.is_heros_will_skill)
			{
				Parent.Buffs.CancelAllDebuffs();
				return true;
			}

			if (bOutsideHandling || template.IsNotBuff)
			{
				return true;
			}

			if (SkillLogic.is_teleport_mastery_skill(nSkillID))
			{
				HandleTeleportMastery(nSkillID, nSLV);
				return true;
			}

			switch ((Skills)nSkillID)
			{
				case Skills.HERO_ENRAGE:
					{
						Parent.Buffs.Remove((int)Skills.CRUSADER_COMBO_ATTACK);
						Parent.Combat.ComboCounter = 0;
					}
					break;
				case Skills.CRUSADER_MAGIC_CRASH:
				case Skills.DRAGONKNIGHT_MAGIC_CRASH:
				case Skills.KNIGHT_MAGIC_CRASH:
					CastAOEMobStat(nSkillID);
					return true;
				case Skills.NIGHTLORD_SPIRIT_JAVELIN:
					{
						Parent.Buffs.Remove(nSkillID);
						var buff = new BuffSkill(nSkillID, nSLV);
						buff.GenerateSpiritJavelin(nSpiritJavelinItemID);
						Parent.Buffs.Add(buff);
					}
					return true;
				case Skills.BMAGE_SUPER_BODY:
					DoSuperBody(nSLV);
					return true;
				case Skills.BMAGE_AURA_BLUE:
					DoAuraSkill(SecondaryStatFlag.BlueAura, (int)Skills.BMAGE_AURA_BLUE_ADVANCED, nSkillID, nSLV);
					return true;
				case Skills.BMAGE_AURA_YELLOW:
					DoAuraSkill(SecondaryStatFlag.YellowAura, (int)Skills.BMAGE_AURA_YELLOW_ADVANCED, nSkillID, nSLV);
					return true;
				case Skills.BMAGE_AURA_DARK:
					DoAuraSkill(SecondaryStatFlag.DarkAura, (int)Skills.BMAGE_AURA_DARK_ADVANCED, nSkillID, nSLV);
					return true;
				case Skills.SHADOWER_SMOKE_SHELL:
				case Skills.EVAN_RECOVERY_AURA:
				case Skills.MAGE1_POISON_MIST:
				case Skills.FLAMEWIZARD_FLAME_GEAR:
				case Skills.BMAGE_SHELTER:
					CastAffectedAreaSkill
						(nSkillID, skill.nSLV, (short)skill.BuffTime, Parent.Position.CurrentXY, template.LT, template.RB);
					return true;
				case Skills.THIEFMASTER_CHAKRA:
					if (Parent.Stats.nHP < Parent.BasicStats.nMHP * 0.5)
					{
						// BMS
						// dwFlaga = sd->nY + 100;
						// tCur = CRand32::Random(&g_rand) % 100 + 100;
						// CQWUser::IncHP(v5, ((tCur * v5->m_basicStat.nLUK * 0.033 + v5->m_basicStat.nDEX) * dwFlaga * 0.002), 0);
						var nMultiplier = skill.Y_Effect + 100;
						var nRand = Constants.Rand.Next() % 100 + 100;
						Parent.Modify.Heal((int)((nRand * Parent.BasicStats.nLUK * 0.033 + Parent.BasicStats.nDEX) * nMultiplier * 0.002));
						return true;
					}
					return false;
				case Skills.KNIGHT_RESTORATION:
					Parent.Modify.Heal((int)(Parent.BasicStats.nMHP * skill.X_Effect));
					return true;
				case Skills.FLAMEWIZARD_SLOW:
				case Skills.WIZARD1_SLOW:
				case Skills.WIZARD2_SLOW:
				case Skills.NIGHTLORD_NINJA_AMBUSH:
				case Skills.SHADOWER_NINJA_AMBUSH:
				case Skills.HERMIT_SHADOW_WEB:
				case Skills.NIGHTWALKER_SHADOW_WEB:
				case Skills.DUAL3_FLASH_BANG:
				case Skills.DUAL4_UPPER_STAB:
				case Skills.HUNTER_ARROW_BOMB:
				case Skills.BOWMASTER_VENGEANCE:
				case Skills.PAGE_THREATEN:
					CastAOEMobStat(nSkillID);
					return true;
				case Skills.DUAL4_OWL_DEATH:
					if (skill.DoProp())
					{
						Parent.Buffs.AddSkillBuff(nSkillID, nSLV, Parent.Buffs.BuffTimeModifier());
						Parent.Combat.OwlSpiritCount = 10;
					}
					return true;
				case Skills.ADMIN_HOLY_SYMBOL:
					foreach (var pChar in Parent.Field.Users)
					{
						pChar.Buffs.AddSkillBuff(nSkillID, nSLV, Parent.Buffs.BuffTimeModifier());
					}
					return true;
				case Skills.WILDHUNTER_SWALLOW:
					SwallowMob(Parent.m_dwSwallowMobID, nSLV);
					return true;
				case Skills.VALKYRIE_DICE:
				case Skills.MECHANIC_DICE:
				case Skills.BUCCANEER_DICE:
					DoDice(nSkillID, nSLV);
					return true;
				case Skills.INFIGHTER_MP_RECOVERY:
					Parent.Modify.Heal(0, (int)(Parent.BasicStats.nMHP * (0.5 + (0.1 * nSLV)))); // hard coding ftw
					return true;
				case Skills.CLERIC_HEAL:
					DoPartyHeal(nSkillID, template.HP(nSLV));
					return true;
				case Skills.BISHOP_RESURRECTION:
					if (Parent.Party?.Count > 1)
					{
						Parent.Party.ApplyBuffToParty(Parent, Parent.Field.dwUniqueId, nSkillID, nSLV);
					}
					return true;
				case Skills.PRIEST_DISPEL:
					if (Parent.Party?.Count > 1)
					{
						Parent.Party.ApplyBuffToParty(Parent, Parent.Field.dwUniqueId, nSkillID, nSLV);
					}
					else
					{
						Parent.Buffs.CancelAllDebuffs();
					}
					CastAOEMobStat(nSkillID); // lazy impl
					return true;
				case Skills.KNIGHT_COMBAT_ORDERS:
					DoCombatOrders(nSkillID, nSLV);
					return true;
				case Skills.BISHOP_INFINITY: // only a party buff for clerics (custom)
					if (Parent.Party?.Count > 1)
					{
						Parent.Party.ApplyBuffToParty(Parent, Parent.Field.dwUniqueId, nSkillID, nSLV, 2);
						return true;
					}
					break;
				case Skills.VIPER_TIME_LEAP:
					DoTimeleap(nSkillID);
					return true;
				case Skills.MECHANIC_HN07:
					{
						if (Parent.Skills.Get((int)Skills.MECHANIC_HN07_UPGRADE, false) is SkillEntry se)
						{
							Parent.Buffs.AddSkillBuff(se.nSkillID, se.nSLV);
							return true;
						}
					}
					break;
			}

			if (template.Time(nSLV) != 0)
			{
				var buffTimeModifier = Parent.Buffs.BuffTimeModifier();

				if (Parent.Party?.Count > 1 && template.IsPartyBuff)
				{
					Parent.Party.ApplyBuffToParty
						(Parent, Parent.Field.dwUniqueId, nSkillID, nSLV, buffTimeModifier);
				}
				else
				{
					Parent.Buffs.AddSkillBuff(nSkillID, nSLV, buffTimeModifier);
				}
			}

			return true;
		}

		private void DoCombatOrders(int nSkillID, byte nSLV)
		{
			if (Parent.Party?.Count > 1)
			{
				Parent.Party.ApplyBuffToParty(Parent, Parent.Field.dwUniqueId, nSkillID, nSLV);
			}
			else
			{
				Parent.Buffs.DoCombatOrders(nSkillID, nSLV);
			}
		}

		private void DoTimeleap(int nSkillID)
		{
			if (Parent.Party?.Count > 1)
			{
				Parent.Party.ForEachMemberInField(Parent.Field.dwUniqueId, c => execute(c));
			}
			else
			{
				execute(Parent);
			}

			void execute(Character c)
			{
				var effect = new UserEffectPacket(UserEffect.SkillAffected)
				{
					nSkillID = nSkillID,
				};
				effect.BroadcastEffect(c);

				foreach (var cd in new List<Cooldown>(c.Cooldowns))
				{
					if (cd.nSkillID != (int)Skills.VIPER_TIME_LEAP)
					{
						Parent.Cooldowns.Remove(cd);
					}
				}
			}
		}

		private void DoPartyHeal(int nSkillID, int nHP) //  // BMS: 0x005D3295
		{
			var nIntFactor = Parent.BasicStats.nINT * 0.8;
			var nIntModifier = nIntFactor + Constants.Rand.Next() % (Parent.BasicStats.nINT - nIntFactor);

			// TODO determine if we want to even bother with adding the MAD factor or just increase the int/luk factors
			//var nMad = sender.Buffs.Sum(buff => buff.Stat[SecondaryStatFlags.MAD] != null ? buff.Stat[SecondaryStatFlags.MAD].nValue : 0);
			//var nMadFactor = nMad + nMad; // addition faster than multiplication :kek:
			var nHeal = (nIntModifier * 1.5 + Parent.BasicStats.nLUK);// * 0.01; // nMadFactor * 0.01;
			var nMembersInMap = Parent.Party != null ? Parent.Party.MembersInMap(Parent.Field.dwUniqueId) : 1;
			var nHealAmount = (int)(nHeal * (nMembersInMap * 0.3 + 1.0) * nHP * 0.01 / nMembersInMap);

			if (Parent.Party?.Count > 1)
			{
				var effect = new UserEffectPacket(UserEffect.SkillAffected)
				{
					nSkillID = nSkillID,
				};

				var nExp = 0;

				Parent.Party.ForEachMemberInField(Parent.Field.dwUniqueId, member =>
				{
					if (member.Stats.nHP > 0)
					{
						var nPrevHP = member.Stats.nHP;
						member.Modify.Heal(nHealAmount);

						if (member.dwId != Parent.dwId)
						{
							nExp += 20 * (member.Stats.nHP - nPrevHP) / (8 * member.Stats.nLevel + 190);
							effect.BroadcastEffect(member);
						}
					}
				});

				if (nExp > 0)
				{
					Parent.Modify.GainExp(nExp);
				}
			}
			else
			{
				Parent.Modify.Heal(nHealAmount);
			}
		}

		private void HandleTeleportMastery(int nSkillID, byte nSLV)
		{
			if (!Parent.Buffs.Contains(nSkillID))
			{
				var newBuff = new BuffSkill(nSkillID, nSLV);
				newBuff.Generate(0);
				Parent.Buffs.Add(newBuff);
			}
			else
			{
				Parent.Buffs.Remove(nSkillID);
			}
		}

		private void DoDice(int nSkillID, byte nSLV)
		{
			Parent.Buffs.Remove(nSkillID);

			var newBuff = new BuffSkill(nSkillID, nSLV);
			newBuff.State = (byte)(Constants.Rand.Next(6) + 1);

			var effect = new UserEffectPacket(UserEffect.SkillAffectedSelect)
			{
				nSelect = (byte)newBuff.State,
				nSkillID = nSkillID,
				nSLV = nSLV
			};

			effect.BroadcastEffect(Parent);

			if (newBuff.State <= 1) return;

			newBuff.GenerateDice();

			Parent.Buffs.Add(newBuff);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nAuraType">Type of aura. This is never just Aura or SuperBody</param>
		/// <param name="nAdvancedAuraID">This is the advanced aura ID (the passive skill)</param>
		/// <param name="nRealSkillID">This is the actual skill ID being requested by the client</param>
		/// <param name="nSLV">Level of nRealSkillID</param>
		private void DoAuraSkill(SecondaryStatFlag nAuraType, int nAdvancedAuraID, int nRealSkillID, byte nSLV)
		{
			Parent.Buffs.RemoveIf(buff
					=> buff.dwCharFromId == dwParentID
					&& (buff.StatType == SecondaryStatFlag.BlueAura
						|| buff.StatType == SecondaryStatFlag.DarkAura
						|| buff.StatType == SecondaryStatFlag.YellowAura));

			// replace nRealSkillID with pAdvancedAura skill if the skill isnt a super body skill and the advanced skill exists in collection
			if (TryGetValue(nAdvancedAuraID, out SkillEntry pAdvancedSkill))
			{
				nRealSkillID = pAdvancedSkill.nSkillID;
				nSLV = pAdvancedSkill.nSLV;
			}

			if (Parent.Party is null)
			{
				Parent.Buffs.AddAura(nAuraType, dwParentID, nRealSkillID, nSLV);
			}
			else
			{
				Parent.Field.Users
					.ForEachPartyMember(Parent.Party.PartyID,
					user => user.Buffs.AddAura(nAuraType, dwParentID, nRealSkillID, nSLV));
			}
		}

		private void DoSuperBody(byte nSLV)
		{
			var tDurationMillis = 60000;
			int superBodySkillID;

			// set only if-statements instead of else-if statements if we enable stackable auras
			if (Parent.Buffs.Contains((int)Skills.BMAGE_AURA_BLUE) || Parent.Buffs.Contains((int)Skills.BMAGE_AURA_BLUE_ADVANCED))
			{
				tDurationMillis = 10000;
				superBodySkillID = (int)Skills.BMAGE_SUPER_BODY_BLUE;
			}
			else if (Parent.Buffs.Contains((int)Skills.BMAGE_AURA_DARK) || Parent.Buffs.Contains((int)Skills.BMAGE_AURA_DARK_ADVANCED))
			{
				superBodySkillID = (int)Skills.BMAGE_SUPER_BODY_DARK;
			}
			else if (Parent.Buffs.Contains((int)Skills.BMAGE_AURA_YELLOW) || Parent.Buffs.Contains((int)Skills.BMAGE_AURA_YELLOW_ADVANCED))
			{
				superBodySkillID = (int)Skills.BMAGE_SUPER_BODY_YELLOW;
			}
			else
			{
				throw new InvalidOperationException("Trying to cast super body when no auras are active.");
			}

			var auraBoost = new BuffSkill(superBodySkillID, nSLV);
			auraBoost.GenerateAuraSkill(SecondaryStatFlag.SuperBody, tDurationMillis);
			Parent.Buffs.Add(auraBoost);
		}

		private void CastAOEMobStat(int nSkillID)
		{
			var skill = Get(nSkillID, true);

			var nAffectedMobs = skill.Template.MobCount(skill.nSLV, 15);

			var rect = skill.Template.Rect;

			rect.OffsetRect(Parent.Position.CurrentXY, (Parent.Position.MoveAction & 1) > 0);

			foreach (var item in Parent.Field.Mobs)
			{
				if (rect.PointInRect(item.Position.CurrentXY))
				{
					item.TryApplySkillDamageStatus(Parent, nSkillID, skill.nSLV, 0);
					nAffectedMobs -= 1;
				}

				if (nAffectedMobs <= 0) break;
			}
		}

		public void CastAffectedAreaSkill(int nSkillID, int skillLevel, short buffTime, TagPoint offsetPos, Point lt, Point rb)
		{
			var area = new CAffectedArea(nSkillID, dwParentID, offsetPos, lt, rb,
				(Parent.Position.MoveAction & 1) != 0)
			{
				Duration = buffTime * 1000,
				nSLV = (byte)skillLevel,
				tDelay = 5
			};

			Parent.Field.AffectedAreas.Add(area);
		}

		private void SwallowMob(int dwSwallowMobId, byte nSLV)
		{
			if (Parent.Field.Mobs.Remove(dwSwallowMobId))
			{
				var effect = new UserEffectPacket(UserEffect.SkillUse)
				{
					nSkillID = (int)Skills.WILDHUNTER_SWALLOW,
					nSLV = nSLV,
				};

				effect.BroadcastEffect(Parent);

				Parent.Cooldowns.UpdateOrInsert((int)Skills.WILDHUNTER_SWALLOW, 30);
			}
		}

		public void CheckDragonFury(int maxMp) // CWvsContext::CheckDragonFury
		{
			if (Parent.Skills.Get((int)Skills.EVAN_DRAGON_FURY) is SkillEntry pSkill)
			{
				var minAmount = pSkill.X_Effect * maxMp / 100; // x is low end of range
				var maxAmount = pSkill.Y_Effect * maxMp / 100; // y is high end of range

				var effect = new UserEffectPacket(UserEffect.SkillAffected)
				{
					nSkillID = pSkill.nSkillID,
				};

				if (Parent.Stats.nMP > minAmount && Parent.Stats.nMP < maxAmount) // in range
				{
					if (nDragonFury == 0)
					{
						effect.bDragonFuryActive = true;
						effect.BroadcastEffect(Parent);
						nDragonFury = pSkill.Template.Damage(pSkill.nSLV);
					}
				}
				else // not in range
				{
					if (nDragonFury != 0)
					{
						effect.bDragonFuryActive = false;
						effect.BroadcastEffect(Parent);
					}
				}
			}
		}

		public void ProcessDarkKnightDarkForce(int maxHp)
		{
			var pSkill = Parent.Skills.Get((int)Skills.DARKKNIGHT_DARK_FORCE);

			if (pSkill != null)
			{
				var minAmount = pSkill.X_Effect * 0.01 * maxHp; // x is low end of range
				var maxAmount = pSkill.Y_Effect * 0.01 * maxHp; // y is high end of range

				//Parent.SendMessage($"Min: {minAmount} | Max: {maxAmount}");

				var effect = new UserEffectPacket(UserEffect.SkillAffected)
				{
					nSkillID = pSkill.nSkillID,
				};

				if (Parent.Stats.nHP > minAmount && Parent.Stats.nHP < maxAmount) // in range
				{
					if (!bDarkForce)
					{
						effect.bDarkForceActive = true;
						effect.BroadcastEffect(Parent);
					}
				}
				else // not in range
				{
					if (bDarkForce)
					{
						effect.bDarkForceActive = true;
						effect.BroadcastEffect(Parent);
					}
				}
			}
		}

		public void ModifyOptionSkills(GW_ItemSlotEquip pEquip, bool bActive)
		{
			Parent.Modify.Skills(mod =>
			{
				foreach (var option in pEquip.nOptionData)
				{
					if (option is null) continue;
					if (option.iSkill == 0) continue;

					var nOptionSkillID = SkillLogic.get_novice_skill_as_race((SkillLogic.NoviceSkillID)(option.iSkill - 23001), Parent.Stats.nJob);

					mod.AddEntry(nOptionSkillID, entry => entry.nSLV = (byte)(bActive ? 1 : 0));

					if (!bActive)
					{
						Parent.Buffs.Remove(nOptionSkillID);
					}
				}
			});
		}

		public void EncodeForSetField(COutPacket p)
		{
			var aSkillsToEncode = this
				.Where(s =>
				{
					if (s.Template.IsHiddenSkill) return false;

					return MasterManager.SkillTemplates[s.nSkillID].Invisible || s.nSLV > 0 || s.CurMastery > 0;
				})
				.ToList();

			p.Encode2((short)aSkillsToEncode.Count);

			foreach (var skill in aSkillsToEncode)
			{
				skill.Encode(p, true);
			}
		}

		public void SaveToDB()
		{
			if (Parent is null || Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.skill_records WHERE character_id = {dwParentID};");

				foreach (var skill in this)
				{
					dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.skill_records (character_id, skill_id, skill_level, skill_mastery) VALUES ({dwParentID}, {skill.nSkillID}, {skill.nSLV}, {skill.CurMastery});");
				}

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public async Task LoadFromDB()
		{
			if (Parent is null) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.skill_records WHERE character_id = {dwParentID}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						Add(new SkillEntry(Convert.ToInt32(r["skill_id"]))
						{
							nSLV = Convert.ToByte(r["skill_level"]),
							CurMastery = Convert.ToByte(r["skill_mastery"]),
						});
					}
				}
			}
		}

		protected override int GetKeyForItem(SkillEntry item) => item.nSkillID;
	}
}