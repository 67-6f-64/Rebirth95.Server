using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Rebirth.Characters;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Field.Life;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Provider.Template.Mob;
using Rebirth.Provider.Template.Skill;
using Rebirth.Server.Center;
using Rebirth.Server.Center.GameData.DropInfo;
using Rebirth.Tools.Formulas;
using static Rebirth.Game.FieldEffectTypes;
using static Rebirth.Game.MobSkill;

namespace Rebirth.Field.FieldObjects
{
	public class SkillCommand
	{
		public int nSkillID;
		public int nSLV;
		public bool ShootAttack;
	}

	public class AttackerInfo // TODO get the nexon name for this
	{
		public int nAmount { get; set; }
		public DateTime tLastAttack { get; set; }
	}

	public class CMob : CFieldObj
	{
		public static ILog Log = LogManager.GetLogger(typeof(CFieldObj));

		// ---------------------
		// Dynamic Variables
		// ---------------------

		public Character Controller { get; private set; }

		// ---------------------
		// MobStat Variables
		// ---------------------
		public MobStat Stats { get; private set; }

		// ---------------------
		// Static Variables
		// ---------------------

		public MobTemplate Template => MasterManager.MobTemplates[nMobTemplateId];
		public int nMobTemplateId { get; }
		public int MaxHp { get; set; }// => Template.MaxHP;
		public int MaxMp { get; set; } // => Template.MaxMP;
		public bool DropsMeso => true; // might be used later for event/pq stuff
		public bool Dead => Stats.HP <= 0;

		/// <summary>
		/// Key: Character ID | Value: Total Damage Dealt
		/// </summary>
		public Dictionary<int, AttackerInfo> Attackers;
		//public Dictionary<int, int> Attackers { get; }
		public MobLeaveFieldType LeaveFieldType { get; set; }

		public int m_tTimeBomb { get; set; }
		public int m_dwSwallowCharacterID { get; set; }
		public int dwGuidedBulletTargetCharID { get; set; }

		public byte m_nSummonType { get; set; }
		public int m_dwSummonOption { get; set; }

		public short DeathDelay { get; private set; }

		public readonly MobSkillContext[] m_aSkillContext;
		public bool m_bNextAttackPossible { get; private set; } = true; // TODO
		public int nSkillCommand { get; private set; }

		//public bool m_bSelfDestruct { get; private set; } 

		public MobType m_nMobType { get; set; }

		public MOBGEN MobGen { get; private set; }

		public bool bRewardsDistributed { get; set; }

		public CMob(int nTemplateID)
		{
			//SpawnIndex = -1; // from previous spawn system
			nMobTemplateId = nTemplateID;

			if (Template is null)
				throw new NullReferenceException
					("Invalid template ID has been passed to CMob constructor.");

			MaxHp = Template.MaxHP;
			MaxMp = Template.MaxMP;

			//Attackers = new Dictionary<int, int>();
			Attackers = new Dictionary<int, AttackerInfo>();
			LeaveFieldType = MobLeaveFieldType.MOBLEAVEFIELD_ETC;

			Stats = new MobStat(this)
			{
				HP = MaxHp,
				MP = MaxMp
			};

			m_aSkillContext = new MobSkillContext[Template.Skill.Length];

			for (var i = 0; i < m_aSkillContext.Length; i++)
			{
				m_aSkillContext[i] = new MobSkillContext(Template.Skill[i].Skill, Template.Skill[i].Level);
			}

			// summon type set in new CreateMob func
			//m_nSummonType = 0xFE; // MobAppearType.MOBAPPEAR_REGEN = 0xFE, // -2 // this is when the mob is spawned through the life pool
			PrepareNextSkill(null);
		}

		public override void Dispose()
		{
			MobGen = null;
			Attackers.Clear();
			Stats.TempStats.Parent = null;
			Stats.TempStats.Clear();
			Stats.TempStats = null;
			Stats = null;
			Controller = null;

			base.Dispose();
		}

		public bool OnMobMove(bool bNextAttackPossible, int nAction, int dwData, SkillCommand pCommand)
		{
			Stats.tLastMove = DateTime.Now;

			var sid = dwData & 0xFF;
			var slv = dwData >> 8 & 0xFF;
			var delay = dwData >> 16;

			if (nAction >= (int)MobAction.MOBACT_MOVE)
			{
				if (nAction < (int)MobAction.MOBACT_ATTACK1
					|| nAction > (int)MobAction.MOBACT_ATTACKF)
				{
					if (nAction >= (int)MobAction.MOBACT_SKILL1
						&& nAction <= (int)MobAction.MOBACT_SKILLF
						&& !DoSkill(dwData & 0xFF, dwData >> 8 & 0xFF, dwData >> 16))
					{
						return false;
					}
				}
				else
				{
					// TODO deadly attack handling
				}

				//if (dwData > 0 && !DoSkill(dwData & 0xFF, (dwData >> 8) & 0xFF, dwData >> 16)) return false;
			}

			// TODO last attack time handling

			m_bNextAttackPossible = bNextAttackPossible;
			PrepareNextSkill(pCommand);
			return true;
		}

		public bool DoSkill(int nSkillID, int nSLV, int tDelay)
		{
			var template = MasterManager.MobSkillTemplates[nSkillID][nSLV];

			if (template is null)//nSkillCommand != nSkillID || template == null)
			{
				nSkillCommand = 0;
				Log.Error($"Mob DoSkill failed {nSkillCommand} => MobID: {nMobTemplateId} SkillInfo: {nSkillID} | {nSLV}");
				return false; // TODO return false if mob has seal debuff
			}

			var skillContext = m_aSkillContext
				.FirstOrDefault(ctx
				=> ctx.nSkillID == nSkillID
					&& ctx.nSLV == nSLV);

			if (skillContext is null)
			{
				Log.Error($"Invalid mob skill context passed to DoSkill function. MobID: {nMobTemplateId} SkillInfo: {nSkillID} | {nSLV}");
				return false;
			}

			if (skillContext.tLastSkillUse.SecondsSinceStart() < template.Interval)
			{
				Log.Error($"Mob attempting to cast skill before its expired. MobID: {nMobTemplateId} SkillInfo: {nSkillID} | {nSLV}");
				return false;
			}

			if (Stats.MP < template.MpCon)
			{
				return false;
			}

			Stats.tLastSkillUse = DateTime.Now;
			skillContext.tLastSkillUse = Stats.tLastSkillUse;

			nSkillCommand = 0;

			if (Stats[MobStatType.SealSkill] != null) return false; // TODO determine if this is correct

			if (IsStatChange(nSkillID))
			{
				DoSkill_StatChange(template, tDelay);
			}
			else if (IsUserStatChange(nSkillID))
			{
				DoSkill_UserStatChange(template, tDelay);
			}
			else if (IsPartizanStatChange(nSkillID))
			{
				DoSkill_PartizanStatChange(template, tDelay);
			}
			else if (IsPartizanOneTimeStatChange(nSkillID))
			{
				DoSkill_PartizanOneTimeStatChange(template, tDelay);
			}
			else if (IsSummon(nSkillID))
			{
				if (template.SummonIDs.Length <= 0) return true;

				skillContext.nSummoned += template.SummonIDs.Length;

				DoSkill_Summon(template, tDelay);
			}
			else if (IsAffectArea(nSkillID))
			{
				DoSkill_AffectArea(template, tDelay);
			}
			else
			{
				Log.Error($"Unhandled mob skill detected: {nSkillID} from Mob: {nMobTemplateId}");
			}

			return true;
		}

		private void DoSkill_StatChange(MobSkillLevelData template, int tDelay)
		{
			if (Stats[MobStatType.MagicCrash] != null)
			{
				return;
			}

			MobStatType nType;
			switch ((MobSkillID)template.nSkillID)
			{
				case MobSkillID.POWERUP:
				case MobSkillID.POWERUP_M:
					nType = MobStatType.PowerUp;
					break;
				case MobSkillID.MAGICUP:
				case MobSkillID.MAGICUP_M:
					nType = MobStatType.MagicUp;
					break;
				case MobSkillID.PGUARDUP:
				case MobSkillID.PGUARDUP_M:
					nType = MobStatType.PGuardUp;
					break;
				case MobSkillID.MGUARDUP:
				case MobSkillID.MGUARDUP_M:
					nType = MobStatType.MGuardUp;
					break;
				case MobSkillID.HASTE:
				case MobSkillID.HASTE_M:
				case MobSkillID.SPEED:
					nType = MobStatType.Speed;
					break;
				case MobSkillID.PHYSICAL_IMMUNE:
					nType = MobStatType.PImmune;
					break;
				case MobSkillID.MAGIC_IMMUNE:
					nType = MobStatType.MImmune;
					break;
				case MobSkillID.HARDSKIN:
					nType = MobStatType.HardSkin;
					break;
				case MobSkillID.PCOUNTER:
					nType = MobStatType.PCounter;
					break;
				case MobSkillID.MCOUNTER:
					nType = MobStatType.MCounter;
					break;
				case MobSkillID.PMCOUNTER:
					Stats.TempStats.RegisterMobStats(0, tDelay,
						new MobStatEntry(MobStatType.PCounter, template.nSkillID | (template.nSLV << 16), (short)template.X, template.Time),
						new MobStatEntry(MobStatType.MCounter, template.nSkillID | (template.nSLV << 16), (short)template.X, template.Time)
						);
					return;
				case MobSkillID.PAD:
					nType = MobStatType.PAD;
					break;
				case MobSkillID.MAD:
					nType = MobStatType.MAD;
					break;
				case MobSkillID.PDR:
					nType = MobStatType.PDR;
					break;
				case MobSkillID.MDR:
					nType = MobStatType.MDR;
					break;
				case MobSkillID.ACC:
					nType = MobStatType.ACC;
					break;
				case MobSkillID.EVA:
					nType = MobStatType.EVA;
					break;
				case MobSkillID.SEALSKILL:
					nType = MobStatType.SealSkill;
					break;
				default:
					{
						Log.Error($"Unhandled mob skill: {Enum.GetName(typeof(MobSkillID), (MobSkillID)template.nSkillID)} ({template.nSkillID})");
						Controller.SendMessage($"Unhandled mob skill: {Enum.GetName(typeof(MobSkillID), (MobSkillID)template.nSkillID)} ({template.nSkillID})");
					}
					return;
			}
			Stats.TempStats.RegisterMobStats(0, tDelay,
				new MobStatEntry(nType, template.nSkillID | (template.nSLV << 16), (short)template.X, template.Time)
				);
		}

		private void DoSkill_UserStatChange(MobSkillLevelData template, int tDelay)
		{
			var rect = template.Rect;
			if (!rect.IsEmpty)
			{
				rect.OffsetRect(Position.CurrentXY, (Position.MoveAction & 1) > 0);
			}

			var count = template.Count == 0 ? -1 : template.Count;

			foreach (var user in Field.Users)
			{
				if (count == 0) break;

				if ((rect.IsEmpty || rect.PointInRect(user.Position.CurrentXY)) && user.Stats.nHP > 0)
				{
					count -= 1;
					user.Buffs.OnStatChangeByMobSkill(template, dwId, tDelay);
				}
			}
		}

		private void DoSkill_PartizanStatChange(MobSkillLevelData template, int tDelay)
		{
			var rect = template.Rect;
			if (!rect.IsEmpty)
			{
				rect.OffsetRect(Position.CurrentXY, (Position.MoveAction & 1) > 0);
			}

			foreach (var mob in Field.Mobs)
			{
				if (mob.nMobTemplateId != 9999999 && (rect.IsEmpty || rect.PointInRect(mob.Position.CurrentXY)))
				{
					mob.DoSkill_StatChange(template, tDelay);
				}
			}
		}

		private void DoSkill_PartizanOneTimeStatChange(MobSkillLevelData template, int tDelay)
		{
			var nAmount = template.X + Constants.Rand.Next() % template.Y;

			Heal(nAmount, 0);
			Field.Broadcast(CPacket.CMobPool.MobDamaged(dwId, 1, -nAmount));
			Field.Broadcast(CPacket.CMobPool.MobAffected(dwId, template.nSkillID | template.nSLV << 16, tDelay));
		}

		private void DoSkill_Summon(MobSkillLevelData template, int tDelay)
		{
			foreach (var id in template.SummonIDs)
			{
				//var spawn = new CMob(id)
				//{
				//	//SpawnIndex = -1,
				//	Position = Position.Clone(), // BMS picks a random foothold within a 300pt rectangle of the mob XY
				//	m_nSummonType = (byte)(m_nSummonType == 0xFC ? template.SummonEffect : 0xFF), // unsure how this is exactly handled in BMS
				//	m_dwSummonOption = template.SummonEffect,
				//};

				//Field.Mobs.CreateMob(spawn);

				// TODO use template rect
				// TODO foothold validation
				// TODO get random foothold in rect to use for spawn

				Field.Mobs.CreateMob(id, null, Position.X, Position.Y - 10, // offset y so mob doesnt fall thru map
					Position.Foothold, (byte)template.SummonEffect, tDelay, 0, 0, null);
			}
		}

		private void DoSkill_AffectArea(MobSkillLevelData template, int tDelay)
		{
			var area = new CAffectedArea(template.nSkillID, dwId, Position.CurrentXY, template.LT, template.RB, (Position.MoveAction & 1) != 0)
			{
				Duration = template.Time * 1000 + tDelay,
				nSLV = (byte)template.nSLV,
				tDelay = (short)tDelay
			};

			Field.AffectedAreas.Add(area);
		}

		public void PrepareNextSkill(SkillCommand pCommand)
		{
			if (Stats.TempStats.Contains((int)MobStatType.Seal)) return;

			if (m_aSkillContext.Length == 0) return;

			if (!m_bNextAttackPossible && pCommand != null || nSkillCommand != 0) return;

			if (Stats.tLastSkillUse.SecondsSinceStart() < 3) return;

			foreach (var item in m_aSkillContext.Shuffle())
			{
				var pST = MasterManager.MobSkillTemplates[item.nSkillID][item.nSLV];

				if (pST.Interval > 0 && item.tLastSkillUse.SecondsSinceStart() < pST.Interval) continue;

				if (pST.HP > 0 && (float)Stats.HP / MaxHp * 100.0f > pST.HP) continue;

				var nOption = 0;

				switch ((MobSkillID)item.nSkillID)
				{
					case MobSkillID.HASTE:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.Speed)?.nOption ?? 0;
						goto default;
					case MobSkillID.POWERUP:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.PowerUp)?.nOption ?? 0;
						goto default;
					case MobSkillID.MAGICUP:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.MagicUp)?.nOption ?? 0;
						goto default;
					case MobSkillID.MGUARDUP:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.MGuardUp)?.nOption ?? 0;
						goto default;
					case MobSkillID.PGUARDUP:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.PGuardUp)?.nOption ?? 0;
						goto default;
					case MobSkillID.HARDSKIN:
						nOption = Stats.TempStats.GetOrDefault((int)MobStatType.HardSkin)?.nOption ?? 0;
						goto default;
					case MobSkillID.SUMMON: // verify summon count wont exceed allowed amount
						if (pST.Limit >= item.nSummoned + pST.SummonIDs.Length) continue;
						break;
					case MobSkillID.PHYSICAL_IMMUNE: // cant have both of these at the same time
					case MobSkillID.MAGIC_IMMUNE: // and cant reapply it while its still active
						{
							if (Stats.TempStats.GetOrDefault((int)MobStatType.PImmune) != null
							   || Stats.TempStats.GetOrDefault((int)MobStatType.MImmune) != null)
								continue;
						}
						break;
					default: // skip if existing option has higher stat value
						if (nOption > 0 && Math.Abs(100 - nOption) > Math.Abs(100 - pST.X)) continue;
						break;
				}

				nSkillCommand = item.nSkillID;
				pCommand.nSkillID = item.nSkillID;
				pCommand.nSLV = item.nSLV;
				break;
			}

#if DEBUG
			if (nSkillCommand > 0)
				Controller?.SendMessage($"Next skill: {Enum.GetName(typeof(MobSkillID), (MobSkillID)nSkillCommand)} ({nSkillCommand})");
#endif
		}

		//--------------------------------------------------

		/// <summary>
		/// Sets controller variables and notifies user if not null.
		/// </summary>
		/// <remarks>Mob must be in mob pool before this is called.</remarks>
		/// <param name="pUser"></param>
		public void SetController(Character pUser, MobCtrlType nLevel)
		{
			// CMob::SendChangeControllerPacket(CMob *this, CUser *pUser, int nLevel)
			if (Controller != null)
			{
				// notify old controller
				Controller.SendPacket(CPacket.CMobPool.MobChangeController(this, 0));
			}
			else
			{
				// BMS
				//COutPacket::COutPacket(&oPacket, 164, 0);
				//v6 = 0;
				//COutPacket::Encode1(&oPacket, nLevel);
				//CMob::EncodeInitData(v3, &oPacket);
				//CUser::SendPacket(pUser, &oPacket);
			}

			Controller = pUser;

			// notify new controller
			Controller?.SendPacket(CPacket.CMobPool.MobChangeController(this, (byte)nLevel));

			m_bNextAttackPossible = false;
			//nSkillCommand = 0; // TODO figure out why this is set here (breaks things atm)

			Stats.tLastMove = DateTime.Now;
			Stats.tLastAttack = DateTime.Now;
		}

		public void Init(MOBGEN pmg, int fh)
		{
			// TODO consider implementing a damage log like this
			//damageLog.vainDamage = 0;
			//damageLog.fieldID = getField().getFieldID();
			//damageLog.initHP = getHP();

			if (pmg != null && pmg.tRegenInterval != 0)
			{
				pmg.nMobCount += 1;
			}

			MobGen = pmg;
			Position.Foothold = (short)(Template.MoveAbility != MobMoveType.Fly ? fh : 0);
		}

		private bool bRemoved;
		public void SetRemoved()
		{
			if (bRemoved) return;
			bRemoved = true;

			if (MobGen != null && MobGen.tRegenInterval != 0)
			{
				MobGen.nMobCount -= 1;

				if (MobGen.nMobCount == 0)
				{
					// add one incase it rounds to 0 and gets a DBZ error
					var tInterval = 7 * MobGen.tRegenInterval / 10 + 1;

					tInterval += Constants.Rand.Next() % (13 * MobGen.tRegenInterval / 10);

					MobGen.tRegenAfter = DateTime.Now.AddMilliseconds(tInterval);
				}
			}
		}

		public bool SetMovePos(int ptX, int ptY, int nMoveAction, int nFH)
		{
			var action = nMoveAction >> 1;
			if (action < 1 || action >= 17)
			{
				return false;
			}
			else
			{
				Position.X = (short)ptX;
				Position.Y = (short)ptY;

				Position.MoveAction = (byte)nMoveAction;
				Position.Foothold = (short)(Template.MoveAbility == MobMoveType.Fly ? 0 : nFH);

				return true;
			}
		}

		//--------------------------------------------------

		public void Heal(int nHp, int nMp)
		{
			if (nMp < 0)
			{
				if (Stats.MP <= 0) return;

				Stats.MP = Math.Max(0, Stats.MP + nMp);
			}
			else if (nMp > 0)
			{
				if (Stats.MP >= MaxMp) return;

				Stats.MP = Math.Min(MaxMp, Stats.MP + nMp);
			}

			if (nHp < 0)
			{
				if (Stats.HP <= 0) return;

				Stats.HP = Math.Max(MaxHp, Stats.HP + nHp);
			}
			else if (nHp > 0)
			{
				if (Stats.HP >= MaxHp) return;

				Stats.HP = Math.Min(MaxHp, Stats.HP + nHp);
			}

			BroadcastHP();
		}

		public void BroadcastHP()
		{
			if (Template.HpTagColor > 0 && Template.HpTagBgColor > 0 && !Template.HPGaugeHide)
			{
				SendMobHpChange(Stats.HP, false);
			}

			var percentage = 100 * Stats.HP / MaxHp;

			if (percentage <= 0) percentage = 1;

			foreach (var user in Attackers.Keys)
			{
				if (Field.Users[user] is Character c)
				{
					c.SendPacket(CPacket.CMobPool.MobHPIndicator(dwId, (byte)percentage));
				}
			}
		}

		public void SendMobHPEnd()
		{
			if (Template.HpTagColor > 0 && Template.HpTagBgColor > 0)
			{
				SendMobHpChange(Template.HPGaugeHide ? -1 : 0, true);
			}
		}

		public void SendMobHpChange(int nHP, bool bForce)
		{
			if (Stats.tLastSendMobHP.MillisSinceStart() >= 500 || bForce)
			{
				Stats.tLastSendMobHP = DateTime.Now;

				new FieldEffectPacket(FieldEffect.MobHPTag)
				{
					dwMobTemplateID = nMobTemplateId,
					nHP = nHP,
					nMaxHP = MaxHp,
					nColor = (byte)Template.HpTagColor,
					nBgColor = (byte)Template.HpTagBgColor
				}.Broadcast(Field);
			}
		}

		public void GiveBuffOnDead()
		{
			if (Template.DeadBuff > 0)
			{
				foreach (var user in Field.Users)
				{
					if (user.Stats.nHP <= 0) continue;

					user.Buffs.AddItemBuff(Template.DeadBuff);
				}
			}
		}

		// CMob::IsTimeToSelfDestruct
		private bool IsTimeToSelfDestruct()
		{
			return (Template.SelfDestructActionType & 4) != 0
				   && Stats.tCreateTime.MillisSinceStart() > 1000 * Template.SelfDestructRemoveAfter;
		}

		// CMob::IsTimeToRemove
		public bool IsTimeToRemove(bool bFixedMob)
		{
			int t;
			if (bFixedMob)
			{
				t = 30000;
			}
			else
			{
				if (IsTimeToSelfDestruct())
				{
					DoSelfDestruct();
					return true;
				}

				if (Template.RemoveAfter <= 0) return false;

				t = Template.RemoveAfter * 1000;
			}

			return t < Stats.tCreateTime.MillisSinceStart();
		}

		public void DoSelfDestruct()
		{
			Stats.HP = 0;
			//m_bSelfDestruct = true; // i think this indicates the leavefield type
			LeaveFieldType = MobLeaveFieldType.MOBLEAVEFIELD_SELFDESTRUCT;
		}

		// CMob::CheckSelfDestruct
		public bool CheckSelfDestruct()
		{
			//v1 = this->m_pTemplate;
			//if (!(v1->selfDestructionInfo.nActionType & 1) || this->m_nHP > v1->selfDestructionInfo.nBearHP)
			//	return 0;
			//CMob::DoSelfDestruct(this);
			//return 1;
			return false;
		}

		public bool Damage(Character character, int amount, int tDelay)
		{
			if (Dead || m_dwSwallowCharacterID > 0) return false;

			Stats.tLastAttack = DateTime.Now;

			if (Stats[MobStatType.HealByDamage] is null)
			{
				amount = Math.Min(amount, Stats.HP);
				if (amount <= 0) return false;
			}

			if (Stats.TempStats.Contains((int)MobStatType.MCounter))
			{
				// TODO
			}

			if (Stats.TempStats.Contains((int)MobStatType.PCounter))
			{
				// TODO
			}

			if (Stats.TempStats.Contains((int)MobStatType.MImmune))
			{
				// TODO
			}

			if (Stats.TempStats.Contains((int)MobStatType.PImmune))
			{
				// TODO
			}

			if (Attackers.ContainsKey(character.dwId))
			{
				Attackers[character.dwId].nAmount += amount;
				Attackers[character.dwId].tLastAttack = DateTime.Now;
			}
			else
			{
				Attackers.Add(character.dwId, new AttackerInfo
				{
					nAmount = amount,
					tLastAttack = DateTime.Now
				});
			}

			Stats.HP -= amount;

			if (Controller?.dwId != character.dwId)
			{
				SetController(character, MobCtrlType.Active_Req);
				//Field.ChangeMobController(character, this, true); //SwitchController(character);
			}

			if (Field.OnMobDamaged(this, amount)) // communicates with field child classes
			{
				BroadcastHP();
			}

			if (Dead)
			{
				DeathDelay = (short)tDelay;

				foreach (var user in Attackers.Keys)
				{
					if (Field.Users[user] is Character c)
					{
						c.Quests.UpdateMobKills(nMobTemplateId, 1);
						c.StatisticsTracker.IncrementMob(nMobTemplateId);
					}
				}

				if (dwGuidedBulletTargetCharID > 0)
				{
					MasterManager.CharacterPool.Get(dwGuidedBulletTargetCharID)?.Buffs.Remove((int)Skills.CAPTAIN_ADVANCED_HOMING);
					MasterManager.CharacterPool.Get(dwGuidedBulletTargetCharID)?.Buffs.Remove((int)Skills.VALKYRIE_HOMING);
				}
			}

			return Dead;
		}

		//void __thiscall CMob::GiveMoney(CMob *this, CUser *pUser, ATTACKINFO *ai, int nAttackCount)
		public void GiveMoney()
		{
			//Called when you use that thief skill that drops mesos from (
		}

		//void __thiscall CMob::GiveReward(CMob *this, unsigned int dwOwnerID, unsigned int dwOwnPartyID, int nOwnType, tagPOINT ptHit, int tDelay, int nMesoUp, int nMesoUpByItem, int bSteal)
		public void GiveReward()
		{
			// TODO figure out how we should assign drop owner id
			// most damage? most recent damage?

			var aFinalDrops = new List<CDrop>();
			double mesoRateMultiplier = Controller.BasicStats.MesoRate;
			double itemRateMultiplier = Controller.BasicStats.DropRate;

			if (Stats.ShowdownRate > 0)
			{
				itemRateMultiplier += Stats.ShowdownRate;
				itemRateMultiplier += Stats.ShowdownRate;
			}

			if (Stats.PickPocketRate > 0)
			{
				itemRateMultiplier += Stats.PickPocketRate;
				itemRateMultiplier += Stats.PickPocketRate;
			}

			mesoRateMultiplier *= 0.01;
			itemRateMultiplier *= 0.01;

			if (MasterManager.MobDropGenerator.Contains(nMobTemplateId))
			{
				var dropList = MasterManager.MobDropGenerator[nMobTemplateId]?.Drops;

				if (dropList != null)
				{
					foreach (var entry in dropList)
					{
						var rand = Constants.Rand.Next(999999);

						if (rand >= entry.Chance * itemRateMultiplier * RateConstants.DropRate) continue;

						var drop = CDropFactory.CreateDropFromMob(Position.Clone(), 0, false, entry.ItemID);

						if (drop?.Item is null)
						{
							Log.Error($"Null drop in CMob.GiveReward(). MobID: {Template}. Item ID: {entry.ItemID}.");
							continue;
						}

						if (drop.Item.InvType == InventoryType.Cash)
						{
							Log.Error($"Mob {Template} attempting to drop cash item {entry.ItemID}.");
							continue;
						}

						if (drop.Item is GW_ItemSlotBundle item && item.Template.Quest)
							continue;

						if (entry.ItemID / 10000 == 413) continue; // stim item
						if (entry.ItemID / 1000 == 4002) continue; // stamps

						aFinalDrops.Add(drop);
					}
				}
			}

			// calculate mob-specific drops


			if (!Template.Boss)
			{
				// calculate global drops
				foreach (var entry in GlobalDropGenerator.GlobalDropData)
				{
					if (entry.RequiredQuestID > 0 && !Controller.Quests.Contains(entry.RequiredQuestID)) continue;

					var nextDouble = Constants.Rand.NextDouble();

					if (Rates.WillDrop(nextDouble, Template.Level, Controller.Stats.nLevel, Controller.Stats.nJob, entry, itemRateMultiplier))
					{
						var drop = CDropFactory.CreateDropFromMob(Controller.Position.Clone(), 0, false, entry.ItemID);
						aFinalDrops.Add(drop);
					}
				}

				// calculate amount of meso to drop
				if (DropsMeso)
				{
					var nMesoAmount = Rates.MakeMesoDropAmount(Template.Level, Controller.Stats.nLevel, mesoRateMultiplier);

					var drop = CDropFactory.CreateDropFromMob(Position.Clone(), 0, true, nMesoAmount);

					aFinalDrops.Add(drop);
				}
			}

			var mobPosX = Position.X;

			// calculate direction drops will fly based on mob position related to center of map
			var direction = mobPosX > Field.Footholds.CenterPointX
				? -1
				: 1;

			mobPosX += (short)(30 * direction); // shift the first drop a lil

			var nDropIdx = 0;

			// randomize drop order and calculate drop positions
			foreach (var drop in aFinalDrops.Shuffle())
			{
				if (Template.Boss) drop.DropExpirySeconds = 600; // 10 min

				drop.Position.X = (short)(mobPosX + 20 * nDropIdx * direction);
				drop.CalculateY(Field, drop.StartPosY);

				// maybe instead we directly assign the delay value to the mob instead of the skill id...
				drop.tDelay = DeathDelay;

				Field.Drops.Add(drop);

				nDropIdx += 1;
			}
		}

		//unsigned int __thiscall CMob::DistributeExp(CMob *this, int *nOwnType, unsigned int *dwOwnPartyID, unsigned int *dwLastDamageCharacterID)
		public void DistributeEXP()
		{
			// only the killer gets the NX -- TODO make party get NX
			Controller.Modify.GainNX(RandomNX.Gain(1.0));

			var partyIds = new List<int>();
			var realAttackers = new Dictionary<int, int>();
			var baseExp = Template.Exp;

			if (Stats.ShowdownRate > 0)
			{
				baseExp += (int)(baseExp * (Stats.ShowdownRate / 100.0f));
			}
			if (Stats.StinkBombRate > 0)
			{
				baseExp += (int)(baseExp * (Stats.StinkBombRate / 100.0f));
			}

			foreach (var aAttackerInfo in Attackers)
			{
				var pAttackingChar = Field.Users[aAttackerInfo.Key];

				if (aAttackerInfo.Value.tLastAttack.SecondsSinceStart() > 5 || pAttackingChar is null) continue;

				realAttackers.Add(aAttackerInfo.Key, aAttackerInfo.Value.nAmount);

				if (pAttackingChar.Party?.MembersInMap(Field.dwUniqueId) >= 2
					&& !partyIds.Contains(pAttackingChar.Party.PartyID))
				{
					partyIds.Add(pAttackingChar.Party.PartyID);
				}
				else
				{
					// dead chars dont get exp
					if (pAttackingChar.Stats.nHP <= 0) continue;

					var expShare = (float)aAttackerInfo.Value.nAmount / MaxHp;

					var formula = (int)(baseExp * RateConstants.ExpRate * expShare * pAttackingChar.BasicStats.ExpRate * 0.01
						* ExpRate.CalculateLevelDiffModifier(Template.Level, pAttackingChar.Stats.nLevel));

					pAttackingChar.Modify.GainExp(formula, 0);
				}
			}

			Attackers.Clear();

			foreach (var item in partyIds)
			{
				MasterManager.PartyPool.GetOrDefault(item)
					?.DistributeMobExp(Field.dwUniqueId, realAttackers, Template.Level, MaxHp, baseExp);
			}
		}

		void SetMobCountQuestInfo()
		{
			//Assign any players current quests that take mob count
		}

		//In BMS CMob::OnMobDead && CLifePool::OnSummonedAttack call this , not pool remove
		public void OnMobDead()
		{
			if (bRewardsDistributed) return;
			bRewardsDistributed = true;

			try
			{
				GiveBuffOnDead();
				DistributeEXP();
				GiveReward();
				//SetMobCountQuestInfo
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public void SuspendReset(bool bSuspendReset)
		{
			Field.Broadcast(CPacket.CMobPool.MobSuspendReset(dwId, bSuspendReset));

			m_nSummonType = 0xFF; // MobAppearType.Normal
			m_dwSummonOption = 0;
		}

		public bool TrySwallowMob(Character c)
		{
			if (Dead || m_dwSwallowCharacterID > 0) return false;

			// this needs to happen before mob leaves field
			LeaveFieldType = MobLeaveFieldType.MOBLEAVEFIELD_SWALLOW;
			m_dwSwallowCharacterID = c.dwId;
			c.m_dwSwallowMobID = dwId;

			return true;
		}

		//--------------------------------------------------

		public void TryApplySkillDamageStatus(Character c, int nSkillID, int nSLV, int tDelay, bool bIgnoreProp = false)
		{
			// todo add effectiveness check here

			if (c is null) return; // very important

			if (m_dwSwallowCharacterID > 0 && c.dwId != m_dwSwallowCharacterID)
				return;

			var template = MasterManager.SkillTemplates[nSkillID];

			var skill = c.Skills.Get(nSkillID, true);

			if (!bIgnoreProp)
			{
				var subprop = template.SubProp(nSLV);

				// some skills have success rates for multiple things -> subprop 
				if (subprop > 0)
				{
					if (Constants.Rand.Next(100) >= subprop)
						return;
				}
				else if (!skill.DoProp())
				{
					// threaten shouldnt use a prop cuz its not in the tooltip
					if (nSkillID != (int)Skills.PAGE_THREATEN) return;
				}
			}

			switch ((Skills)nSkillID) // skills that can affect boss mobs
			{
				case Skills.CRUSADER_MAGIC_CRASH:
				case Skills.DRAGONKNIGHT_MAGIC_CRASH:
				case Skills.KNIGHT_MAGIC_CRASH:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.MagicCrash,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					goto case Skills.ADMIN_DISPEL; // fall thru case
				case Skills.ADMIN_DISPEL:
				case Skills.PRIEST_DISPEL:
					Stats.TempStats.RemoveMobStats(Stats.TempStats.Where(entry => entry.CharIdFrom == 0).ToArray());
					return;
				case Skills.THIEF_STEAL: // TODO refactor this to mimick BMS
					{
						if (Stats.AffectedBySteal) // can only happen once
							return;

						var dropList = MasterManager.MobDropGenerator[nMobTemplateId]?.Drops;

						if (dropList != null)
						{
							var item = dropList.Where(i => ItemConstants.GetInventoryType(i.ItemID) == InventoryType.Etc).Random().ItemID;

							var drop = CDropFactory.CreateDropFromMob(Position.Clone(), Controller.dwId, false, item);
							drop.Position.X = Position.X;
							drop.CalculateY(Field, drop.StartPosY);
							Field.Drops.Add(drop);
						}

						Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
								new MobStatEntry(
								nType: MobStatType.Stun,
								nSkillID: nSkillID,
								nOption: 1,
								liDurationSeconds: skill.BuffTime
							));

						Stats.AffectedBySteal = true;
					}
					return;
				case Skills.THIEFMASTER_PICKPOCKET: // TODO refactor this to mimick BMS
					{
						// in addition to the increased meso rate, theres a chance to drop a sack of mesos

						var nMesoAmount = Rates.MakeMesoDropAmount(Template.Level, Controller.Stats.nLevel, Controller.BasicStats.MesoRate * 0.01);

						nMesoAmount = (int)(nMesoAmount * 0.1 * Stats.PickPocketRate * 0.01); // it drops one tenth the normal amount of mesos

						var drop = CDropFactory.CreateDropFromMob(Position.Clone(), 0, true, nMesoAmount);
						drop.Position.X = Position.X;
						drop.CalculateY(Field, drop.StartPosY);
						Field.Drops.Add(drop);
					}
					return;
				case Skills.FLAMEWIZARD_SEAL: // verified
				case Skills.MAGE1_SEAL: // verified
				case Skills.MAGE2_SEAL: // verified
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Seal,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					return;
				default: // skills that cant affect boss mobs
					if (Template.Boss) return;
					break;
			}

			// poison -> venom is handled outside of this function 
			if (template.DotTime(nSLV) > 0)
			{
				Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
					new MobStatEntry(
					nType: MobStatType.Poison,
					nSkillID: nSkillID,
					nOption: (short)(MaxHp / (70 - nSLV)), // BMS-like : TODO verify its still good
					liDurationSeconds: skill.BuffTime)
					);
			}

			switch ((Skills)nSkillID)
			{
				case Skills.HERMIT_SHADOW_WEB:
				case Skills.NIGHTWALKER_SHADOW_WEB:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Web,
						nSkillID: nSkillID,
						nOption: (short)(MaxHp / (50 - nSLV)), // BMS-like
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.NIGHTLORD_NINJA_AMBUSH: // BMS-like
				case Skills.SHADOWER_NINJA_AMBUSH:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Ambush,
						nSkillID: nSkillID,
						nOption: (short)((nSLV + 30) * template.Damage(nSLV) * (c.BasicStats.nSTR + c.BasicStats.nSTR) / 2000),
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.FLAMEWIZARD_SLOW:
				case Skills.WIZARD1_SLOW:
				case Skills.WIZARD2_SLOW:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Speed,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.ARAN_SNOW_CHARGE:
				case Skills.BOWMASTER_HAMSTRING:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Speed,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: (int)skill.Y_Effect
						));
					break;
				case Skills.EVAN_SLOW:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Speed,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: (int)skill.Y_Effect
						));
					break;
				case Skills.RANGER_SILVER_HAWK:
				case Skills.WILDHUNTER_SILVER_HAWK:
				case Skills.BOWMASTER_PHOENIX:
				case Skills.SNIPER_GOLDEN_EAGLE:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Stun,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: (int)skill.Y_Effect
						));
					break;
				case Skills.ARAN_ROLLING_SPIN:
				case Skills.SHADOWER_BOOMERANG_STEP:
				case Skills.CRUSADER_SWORD_COMA:
				case Skills.MECHANIC_ROCKET_PUNCH:
				case Skills.MECHANIC_TESLA_COIL: // rock n shock
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
				case Skills.BOWMASTER_VENGEANCE:
				case Skills.HUNTER_ARROW_BOMB:
				case Skills.PRIEST_SHINING_RAY:
				case Skills.MAGE1_TELEPORT_MASTERY:
				case Skills.MAGE2_TELEPORT_MASTERY:
				case Skills.BMAGE_TELEPORT_MASTERY:
				case Skills.PRIEST_TELEPORT_MASTERY:
				case Skills.MAGE2_THUNDER_SPEAR:
				case Skills.DARKKNIGHT_MONSTER_MAGNET:
				case Skills.HERO_MONSTER_MAGNET:
				case Skills.KNIGHT_CHARGE_BLOW:
				case Skills.CRUSADER_SHOUT:
				case Skills.NIGHTLORD_NINJA_STORM:
				case Skills.PALADIN_BLOCKING:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Stun,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.PAGE_THREATEN:
					var time = skill.BuffTime;
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.PAD,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: time
						),
						new MobStatEntry(
						nType: MobStatType.PDR,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: time
						),
						new MobStatEntry(
						nType: MobStatType.ACC,
						nSkillID: nSkillID,
						nOption: (short)skill.Y_Effect,
						liDurationSeconds: time / 10
						));
					break;
				case Skills.DRAGONKNIGHT_DRAGON_ROAR:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Stun,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: 1
						));
					break;
				case Skills.ARAN_BODY_PRESSURE:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.BodyPressure,
						nSkillID: nSkillID,
						nOption: (short)nSLV,
						liDurationSeconds: (int)skill.X_Effect
						));
					break;
				case Skills.NIGHTLORD_SHOWDOWN:
				case Skills.SHADOWER_SHOWDOWN:
					{
						Stats.ShowdownRate = Math.Max(Stats.ShowdownRate, (int)skill.X_Effect + 100);

						Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
							new MobStatEntry(
							nType: MobStatType.Showdown,
							nSkillID: nSkillID,
							nOption: (short)skill.X_Effect,
							liDurationSeconds: skill.BuffTime
							));
					}
					break;
				case Skills.EVAN_ICE_BREATH:
				case Skills.CROSSBOWMASTER_FREEZER: // frostprey
				case Skills.ARCHMAGE2_ELQUINES:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Freeze,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: (int)skill.X_Effect
						));
					break;
				case Skills.ARAN_COMBO_TEMPEST:
				case Skills.VALKYRIE_COOLING_EFFECT: // ice splitter
				case Skills.SNIPER_ICE_SHOT: // blizzard
				case Skills.MAGE2_MAGIC_COMPOSITION: // element composition
				case Skills.WIZARD2_COLD_BEAM:
				case Skills.MAGE2_ICE_STRIKE:
				case Skills.ARCHMAGE2_BLIZZARD:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Freeze,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.PRIEST_DOOM:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Doom,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.ARCHMAGE2_ICE_DEMON:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Freeze,
						nSkillID: nSkillID,
						nOption: 1,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.NIGHTWALKER_DISORDER:
				case Skills.ROGUE_DISORDER:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.PAD,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						),
						new MobStatEntry(
						nType: MobStatType.PDR,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.DUAL3_FLASH_BANG: // verified
				case Skills.CROSSBOWMASTER_BLIND:
				case Skills.WILDHUNTER_BLIND:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.ACC,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.DUAL5_MONSTER_BOMB: // verified
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.TimeBomb,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.ARAN_FINAL_TOSS: // verified
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.RiseByToss,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: 1
						));
					break;
				case Skills.CRUSADER_SWORD_PANIC:
				case Skills.SOULMASTER_PANIC_SWORD: // verified
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Weakness,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.WILDHUNTER_NERVEGAS:
					Stats.StinkBombRate = (int)skill.X_Effect;
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.PDR,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.MECHANIC_VELOCITY_CONTROLER:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Speed,
						nSkillID: nSkillID,
						nOption: (short)skill.X_Effect,
						liDurationSeconds: skill.BuffTime
						),
						new MobStatEntry(
						nType: MobStatType.PDR,
						nSkillID: nSkillID,
						nOption: (short)skill.Y_Effect,
						liDurationSeconds: skill.BuffTime
						));
					break;
				case Skills.CAPTAIN_MIND_CONTROL:
					Stats.TempStats.RegisterMobStats(c.dwId, tDelay,
						new MobStatEntry(
						nType: MobStatType.Dazzle,
						nSkillID: nSkillID,
						nOption: (short)c.dwId,
						liDurationSeconds: skill.BuffTime
						));
					break;
				default:
					return;
			}
		}

		//--------------------------------------------------

		public void EncodeInitData(COutPacket p)
		{
			//p.Encode4(dwMobId);
			//p.Encode1(5); //  nCalcDamageIndex | Controller
			//p.Encode4(dwTemplateId);

			//CMob::SetTemporaryStat
			Stats.TempStats.EncodeCollection(p);

			//CMob::Init

			Position.EncodePos(p); //m_ptPosPrev.x | m_ptPosPrev.y
			p.Encode1(Position.MoveAction);
			p.Encode2(Position.Foothold); //  m_nFootholdSN
			p.Encode2(Template.FlySpeed != 0 ? (short)0 : Position.Foothold); //  m_nHomeFoothold

			p.Encode1(m_nSummonType); //m_nSummonType

			if (m_nSummonType == 0xFD || m_nSummonType <= 0x7F) // MOBAPPEAR_REVIVED = 0xFD
				p.Encode4(m_dwSummonOption); // delay or parent mob id I think

			p.Encode1(0xFF); //m_nTeamForMCarnival
			p.Encode4(0); //nEffectItemID
			p.Encode4(0); //m_nPhase
		}

		public override COutPacket MakeEnterFieldPacket() => CPacket.CMobPool.MobEnterField(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.CMobPool.MobLeaveField(this);
	}
}