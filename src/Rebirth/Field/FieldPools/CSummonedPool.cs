using Rebirth.Characters;
using Rebirth.Characters.Skill;
using Rebirth.Client;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.Life;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Field.FieldPools
{
	public class CSummonedPool : CObjectPool<CSummon>
	{
		public static void Handle_SummonedMove(WvsGameClient c, CInPacket p)
		{
			var dwSummonedID = p.Decode4();
			var item = c.Character.Field.Summons[dwSummonedID];

			if (item is null) return;

			c.Character.Field.Broadcast(CPacket.CSummonedPool.SummonedMove(item, p), c.Character);
		}

		public static void Handle_SummonedAttack(WvsGameClient c, CInPacket p)
		{
			var dwSummonedID = p.Decode4();
			var item = c.Character.Field.Summons[dwSummonedID];

			if (item is null) return;

			var nSkillID = item.nSkillID;

			p.Skip(8); // crc
			p.Decode4(); // get_update_time()
			p.Skip(8); // crc

			var nActionAndDir = p.Decode1();

			var nAction = nActionAndDir & 0x7F;

			var bLeft = ((nAction >> 7) & 1) > 0;

			p.Skip(8); // crc

			var nMobCount = p.Decode1();

			if (nSkillID == (int)Skills.MECHANIC_TESLA_COIL && nAction != 0x10)
			{
				var adwTeslaFamily = p.DecodeIntArray(3);
			}

			p.Skip(8); // mob pos, summon pos
			p.Skip(4);

			var deadMobs = new List<int>();
			var aSAI = new List<SummonAttackInfo>();

			for (var i = 0; i < nMobCount; i++)
			{
				var dwMobID = p.Decode4();

				var pMob = item.Field.Mobs[dwMobID];

				var dwTemplateID = p.Decode4();
				var nHitAction = p.Decode1();
				var nForeAction = p.Decode1();
				var nFrameIdx = p.Decode1();
				var nCalcDamageStatIndex = p.Decode1();

				p.Skip(8); // position

				var tDelay = p.Decode2();

				var nDamage = p.Decode4();

				

				if (nDamage <= 0) continue;

				if (pMob is null || pMob.Dead || pMob.Template.Invincible) continue; // this happens naturally when two summons attack the same mob at the same time

				aSAI.Add(new SummonAttackInfo
				{
					dwMobID = dwMobID,
					nHitAction = nHitAction,
					nDamage = nDamage
				});

				if (pMob.Damage(c.Character, nDamage, tDelay))
				{
					deadMobs.Add(dwMobID);
				}
			}

			item.Field.Broadcast(CPacket.CSummonedPool.SummonedAttack(item, item.dwId, nActionAndDir, aSAI), c.Character);

			deadMobs.ForEach(mob => c.Character.Field.Mobs.Remove(mob));

			if (nAction == 0x10)
			{
				item.nLeaveType = SummonLeaveType.LEAVE_TYPE_SELF_DESTRUCT;
				item.Field.Summons.Remove(item);
			}
			else if (nSkillID == (int)Skills.VALKYRIE_GABIOTA)
			{
				item.nLeaveType = SummonLeaveType.LEAVE_TYPE_GABIOTA;
				item.Field.Summons.Remove(item);
			}
		}

		public static void Handle_SummonedSkill(WvsGameClient c, CInPacket p)
		{
			var dwSummonedID = p.Decode4();
			var item = c.Character.Field.Summons[dwSummonedID];

			if (item is null) return;

			var nBuffSkillID = p.Decode4();

			var pSkill = item.Parent.Skills.Get(nBuffSkillID, true);

			if (pSkill is null)
			{
				item.Parent.SendMessage($"Unable to find Skill ID {nBuffSkillID}.");
				return;
			}

			var nAction = p.Decode1();

			var nIntervalMillis = SkillLogic.get_summoned_attack_delay(pSkill.nSkillID, pSkill.nSLV, (int)pSkill.X_Effect);

#if DEBUG
			item.Parent.SendMessage("Skill ID sent from summon: " + nBuffSkillID);
#endif

			if (!item.tLastBuffTime.AddedMillisExpired(nIntervalMillis)) return;

			item.tLastBuffTime = DateTime.Now;

			switch ((Skills)nBuffSkillID)
			{
				case Skills.MECHANIC_HEALING_ROBOT_H_LX:
					{
						if (item.Parent.Party != null)
						{
							item.Parent.Party.ApplyBuffToParty(item.Parent, item.Field.dwUniqueId, nBuffSkillID, pSkill.nSLV);
						}
						else if (item.Parent.Stats.nHP > 0)
						{
							var nHealAmount = (int)(item.Parent.BasicStats.nMHP * pSkill.Template.HP(pSkill.nSLV) * 0.01);

							item.Parent.Modify.Heal(nHealAmount);
						}
					}
					break;
				case Skills.DARKKNIGHT_BEHOLDERS_HEALING:
					{
						if (item.Parent.Stats.nHP > 0)
						{
							var nHealAmount = pSkill.Template.HP(pSkill.nSLV);
							item.Parent.Modify.Heal(nHealAmount);
						}
					}
					break;
				case Skills.DARKKNIGHT_BEHOLDERS_BUFF:
					{
						if (item.Parent.Stats.nHP > 0)
						{
							var nType = p.Decode1();

							var buffid = 2022125 + Constants.Rand.Next(0, 5);

							if (!item.Parent.Buffs.Contains(-buffid))
							{
								var buff = new BuffConsume(buffid);
								buff.GenerateBeholderBuff(item.Parent);

								item.Parent.Buffs.Add(buff);
							}
						}
					}
					break;
				case Skills.MECHANIC_ROBOROBO:
					{
						item.Field.Summons.CreateSummon(item.Parent, (int)Skills.MECHANIC_ROBOROBO_DUMMY,
							item.nSLV,
							item.Position.X,
							item.Position.Y);
					}
					break;
				default:
					item.Parent.SendMessage("Unhandled summon skill ID: " + nBuffSkillID);
					return;
			}

			var effect = new UserEffectPacket(UserEffect.SkillAffected)
			{
				nSkillID = nBuffSkillID,
				nSLV = pSkill.nSLV,
			};

			effect.BroadcastEffect(item.Parent); // i think this includes parent

			item.Field.Broadcast(CPacket.CSummonedPool.SummonedSkill(item, item.nSkillID, nAction), item.Parent); // i think this excludes parent
		}

		public static void Handle_SummonedHit(WvsGameClient c, CInPacket p)
		{
			int dwSummonedID = p.Decode4();
			var item = c.Character.Field.Summons[dwSummonedID];

			if (item is null) return;

			var nAttackIdx = p.Decode1();
			var nDamage = p.Decode4();
			var dwMobID = p.Decode4();
			var bLeft = p.Decode1();

			if (item.nCurHP <= 0 || item.Field.Mobs[dwMobID] is null || item.Field.Mobs[dwMobID].Stats.HP <= 0) return;

			item.Field.Broadcast(CPacket.CSummonedPool.SummonedHit(item, nAttackIdx, nDamage, dwMobID, bLeft));

			if (nDamage >= item.nCurHP)
			{
				item.nLeaveType = SummonLeaveType.LEAVE_TYPE_SUMMONED_DEAD;
				item.Field.Summons.Remove(item);
			}
			else
			{
				item.nCurHP -= (short)nDamage;
			}
		}

		/// <summary>
		/// Client summon remove request
		/// </summary>
		/// <param name="c"></param>
		/// <param name="p"></param>
		public static void Handle_Remove(WvsGameClient c, CInPacket p)
		{
			var dwSummonedID = p.Decode4();
			var summon = c.Character.Field.Summons[dwSummonedID];

			if (summon?.dwParentID != c.Character.dwId) return;

			if (summon.Parent.Buffs.GetOrDefault((int)Skills.MECHANIC_SAFETY) is BuffSkill safety) // satellite safety
			{
				switch ((Skills)summon.nSkillID)
				{
					case Skills.MECHANIC_SATELITE3:
						summon.Parent.Cooldowns.UpdateOrInsert(safety.nSkillID, safety.Template.Cooltime(safety.nSLV));
						summon.Field.Summons.CreateSummon(summon.Parent, (int)Skills.MECHANIC_SATELITE2, summon.Parent.Skills.Get((int)Skills.MECHANIC_SATELITE2, true).nSLV, summon.Parent.Position.X, summon.Parent.Position.Y);
						summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_UPDATE;
						c.Character.Field.Summons.Remove(summon); // has to be last
						return;
					case Skills.MECHANIC_SATELITE2:
						summon.Parent.Cooldowns.UpdateOrInsert(safety.nSkillID, safety.Template.Cooltime(safety.nSLV));
						summon.Field.Summons.CreateSummon(summon.Parent, (int)Skills.MECHANIC_SATELITE, summon.Parent.Skills.Get((int)Skills.MECHANIC_SATELITE, true).nSLV, summon.Parent.Position.X, summon.Parent.Position.Y);
						summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_UPDATE;
						c.Character.Field.Summons.Remove(summon); // has to be last
						return;
					case Skills.MECHANIC_SATELITE:
						summon.Parent.Cooldowns.UpdateOrInsert(safety.nSkillID, safety.Template.Cooltime(safety.nSLV));
						summon.Parent.Buffs.Remove(safety);
						summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_UPDATE;
						c.Character.Field.Summons.Remove(summon); // has to be last
						return;
				}
			}

			summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_ON_REMOVE;

			if (summon.nMoveAbility == SummonMoveAbility.Follow || summon.nMoveAbility == SummonMoveAbility.CircleFollow)
			{
				summon.Parent.Buffs.Remove(summon.nSkillID);
			}

			c.Character.Field.Summons.Remove(summon); // has to be last
		}

		// ============================================================

		public CSummonedPool(CField parent)
			: base(parent) { }

		public void Update()
		{
			foreach (var summon in new List<CSummon>(this))
			{
				if (summon.tExpiration.MillisUntilEnd() <= 0)
				{
					Remove(summon);
				}
				else
				{
					switch (summon.nSkillID)
					{
						case (int)Skills.MECHANIC_AR_01:
							{
								foreach (var user in Field.Users)
								{
									if (user.dwId == summon.dwParentID || user.Party?.PartyID == summon.Parent.Party?.PartyID)
									{
										//user.Buffs.AddSkillBuff(summon.nSkillID, summon.nSLV);

										var rect = summon.Template.Rect;
										rect.OffsetRect(summon.Position.CurrentXY);

										if (rect.PointInRect(user.Position.CurrentXY))
										{
											user.Buffs.AddSkillBuff(summon.nSkillID, summon.nSLV);
										}
									}
								}
							}
							break;
						case (int)Skills.MECHANIC_VELOCITY_CONTROLER:
							{
								// TODO consider applying the mob buff here instead of in mob pool
							}
							break;
					}
				}
			}
		}

		public CSummon GetBySkillID(int dwCharID, int nSkillID)
			=> this.FirstOrDefault(summon => summon.dwParentID == dwCharID && summon.nSkillID == nSkillID);

		public void RemoveCharSummons(int dwCharID, SummonLeaveType nLeaveType)
		{
			foreach (var summon in new List<CSummon>(this))
			{
				if (summon.dwParentID != dwCharID) continue;

				summon.nLeaveType = nLeaveType;
				Remove(summon.dwId);
			}
		}

		public bool Handle_TeslaCoil(Character owner, byte nSLV, short ptX, short ptY, int[] ldwTeslaCoilSummonedID)
		{
			var teslaCoilList = ldwTeslaCoilSummonedID.ToList();

			foreach (var coilId in teslaCoilList)
			{
				var coil = this[coilId];
				if (coil is null) return false;

				coil.nTeslaCoilState = TeslaCoilType.Follower;
			}

			var summon = CreateSummon(owner, (int)Skills.MECHANIC_TESLA_COIL, nSLV, ptX, ptY);

			summon.nTeslaCoilState = TeslaCoilType.Leader;

			teslaCoilList.Add(summon.dwId);

			Field.Broadcast(CPacket.CSummonedPool.TeslaTriangle(owner.dwId, teslaCoilList));

			return true;
		}

		public CSummon CreateSummon(Character owner, int nSkillID, byte nSLV, short ptX, short ptY)
		{
			var toRemove = this
				.Where(s => s.nSkillID == nSkillID && s.dwParentID == owner.dwId)
				.OrderByDescending(s => s.dwId); // removes the oldest first

			if (toRemove.Any())
			{
				int maxSummonedCount;
				switch ((Skills)nSkillID)
				{
					case Skills.VALKYRIE_OCTOPUS: // stationary summon with 10 sec cd and 30 sec duration
					case Skills.CAPTAIN_SUPPORT_OCTOPUS: // TODO determine how this is handled (BMS)
					case Skills.MECHANIC_TESLA_COIL:
					case Skills.WILDHUNTER_SWALLOW_DUMMY_ATTACK:
					case Skills.BMAGE_REVIVE: // summon reaper buff
						maxSummonedCount = 3;
						break;
					case Skills.MECHANIC_ROBOROBO_DUMMY:
						maxSummonedCount = 5; // TODO verify how often these things spawn/blow up and get a more accurate number
						break;
					case Skills.VALKYRIE_GABIOTA: // TODO determine if this has special handling
					default:
						maxSummonedCount = 1;
						break;
				}

				foreach (var item in toRemove)
				{
					maxSummonedCount -= 1;

					if (maxSummonedCount <= 0)
					{
						item.nLeaveType = SummonLeaveType.LEAVE_TYPE_NOT_ABLE_MULTIPLE;
						Remove(item.dwId);
					}
				}
			}

			var summon = new CSummon(owner, nSkillID)
			{
				nSLV = nSLV,
				nEnterType = SummonEnterType.ENTER_TYPE_CREATE_SUMMONED,
				Position = new CMovePath()
				{
					X = ptX,
					Y = ptY,
					Foothold = Field.Footholds.FindBelow(ptX, ptY)?.Id ?? 0,
					MoveAction = (byte)(nSkillID == (int)Skills.MECHANIC_TESLA_COIL
							? SummonMoveAction.MA_TESLA_COIL_TRIANGLE
							: SummonMoveAction.MA_STAND)
				},
			};

			summon.tExpiration = DateTime.Now.AddSeconds(summon.Template.Time(summon.nSLV));

			Add(summon);

			return summon;
		}

		public void InsertFromStorage(Character c)
		{
			foreach (var summon in MasterManager.SummonStorage.Retrieve(c.dwId))
			{
				summon.Parent = c;
				summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_DEFAULT;
				summon.Position.ResetPosTo(c.Position);
				Add(summon);
			}
		}

		protected override void InsertItem(int index, CSummon summon)
		{
			if (summon is null) return;

			base.InsertItem(index, summon);

			// enter field packet has to be after item is inserted
			Field.Broadcast(summon.MakeEnterFieldPacket());
			summon.nEnterType = SummonEnterType.ENTER_TYPE_REREGISTER_SUMMONED;
		}

		protected override void RemoveItem(int index)
		{
			var summon = GetAtIndex(index);

			if (summon?.Field != null)
			{
				summon.Field.Broadcast(summon.MakeLeaveFieldPacket());

				if (summon.nSkillID == (int)Skills.MECHANIC_VELOCITY_CONTROLER)
				{
					foreach (var mob in summon.Field.Mobs)
					{
						var toRemove = new List<MobStatEntry>();

						foreach (var stat in mob.Stats.TempStats)
						{
							if (stat.rOption == (int)Skills.MECHANIC_VELOCITY_CONTROLER)// && stat.CharIdFrom == dwParentID)
							{
								toRemove.Add(stat);
							}
						}

						mob.Stats.TempStats.RemoveMobStats(toRemove.ToArray());
					}

					summon.Field.nVelocityControllerdwId = 0;
				}
			}

			base.RemoveItem(index); // needs to be last
		}
	}
}
