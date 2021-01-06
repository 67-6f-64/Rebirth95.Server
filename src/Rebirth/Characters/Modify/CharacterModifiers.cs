using System;
using Rebirth.Characters.Quest;
using Rebirth.Characters.Stat;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Center;
using Rebirth.Tools.Formulas;

namespace Rebirth.Characters.Modify
{
	public class CharacterModifiers
	{
		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		public CharacterModifiers(int parent)
		{
			dwParentID = parent;
		}

		public void Dispose()
		{
			// maybe something someday
		}

		public void UseVotePoints(int amount)
		{
			Parent.Account.VotePoints -= amount;
		}

		public void UseRebirthPoints(int amount)
		{
			Parent.Account.RebirthPoints -= amount;
		}

		public void GainNX(int amount, bool bNotify = true)
		{
			if (amount <= 0) return;

			Parent.Account.AccountData.NX_Credit = Math.Min(int.MaxValue, Parent.Account.AccountData.NX_Credit + amount);

			//if (!bNotify) return;

			//Parent.SendPacket(CPacket.Custom.GainNX(amount));
		}

		public void GainExp(int normalExp, int partyBonus = 0)
		{
			const int energy_cartridge_qid = 23980;
			const int mechanical_heart_part_qid = 23981;

			if (Parent.Stats.nLevel >= 200) return;

			Stats(mod =>
			{
				mod.EXP += normalExp + partyBonus;
			});

			if (normalExp <= 0) return;

			Parent.SendPacket(CPacket.IncEXPMessage(normalExp, partyBonus));

			if (Parent.Quests[energy_cartridge_qid]?.nState == QuestActType.QuestAccept)
			{
				var currentExp = Convert.ToInt32(Parent.Quests[energy_cartridge_qid].sQRValue);

				Parent.Quests.SendUpdateQuestRecordMessage(energy_cartridge_qid, (currentExp + normalExp + partyBonus).ToString());
			}

			if (Parent.Quests[mechanical_heart_part_qid]?.nState == QuestActType.QuestAccept)
			{
				var currentExp = Convert.ToInt32(Parent.Quests[mechanical_heart_part_qid].sQRValue);

				Parent.Quests.SendUpdateQuestRecordMessage(mechanical_heart_part_qid, (currentExp + normalExp + partyBonus).ToString());
			}
		}

		public void GainFame(short nAmount, bool bWithNotification = true)
		{
			Stats(mod =>
			{
				mod.POP = (short)Math.Min(Parent.Stats.nPOP + nAmount, short.MaxValue);
			});

			if (bWithNotification)
			{
				Parent.SendPacket(CPacket.IncPOPMessage(nAmount));
			}
		}

		public void GainMeso(int nAmount, bool bWithNotification = true)
		{
			Stats(mod =>
			{
				mod.Money = Math.Min(Parent.Stats.nMoney + nAmount, int.MaxValue);
			});

			if (bWithNotification)
			{
				Parent.SendPacket(CPacket.DropPickUpMessage_Meso(nAmount));
			}
		}

		public void Inventory(Action<InventoryModifier> action, bool bExclRequest = true)
		{
			var ctx = new InventoryModifier(Parent);

			action?.Invoke(ctx);

			if (ctx.ContainsOperations)
			{
				Parent.SendPacket(CPacket.InventoryOperation(ctx, bExclRequest));

				if (ctx.Broadcast_UserAvatarModified)
				{
					Broadcast_UserAvatarModified();
				}
			}
		}

		private void Broadcast_UserAvatarModified()
		{
			Parent.Field.Broadcast(CPacket.UserAvatarModified(Parent), Parent);

			Parent.ValidateStat(true);

			MasterManager.MessengerManager
				.GetByCharId(Parent.dwId)?
				.UpdateAvatar(Parent);
		}

		public void Heal(int nHP, int nMP = 0)
		{
			Stats(ctx =>
			{
				if (nHP != 0)
				{
					if (nHP + ctx.HP > Parent.BasicStats.nMHP)
					{
						ctx.HP = Parent.BasicStats.nMHP;
					}
					else
					{
						ctx.HP += nHP;
					}

					if (ctx.HP < 0) ctx.HP = 0;
				}

				if (nMP != 0)
				{
					if (nMP + ctx.MP > Parent.BasicStats.nMMP)
					{
						ctx.MP = Parent.BasicStats.nMMP;
					}
					else
					{
						ctx.MP += nMP;
					}

					if (ctx.MP < 0) ctx.MP = 0;
				}
			});
		}

		/// <summary>
		/// Sorts pet locker and sends pet locket stat update packet.
		/// </summary>
		public void UpdatePetLocker()
		{
			Parent.Pets.SortPets();

			for (var i = 0; i < 3; i++)
			{
				Parent.Stats.aliPetLockerSN[i] = Parent.Pets.Pets[i]?.liPetLockerSN ?? 0;
			}

			Stats(ctx => ctx.UpdatePetLocker());
		}

		public void Stats(Action<StatModifier> action, bool bExclRequest = true)
		{
			var ctx = new StatModifier(Parent);

			action?.Invoke(ctx);

			if ((ctx.Flag & ModifyStatFlags.EXP) > 0)
			{
				if (ctx.EXP < 0) ctx.EXP = 0;

				var nExpSurplus = ctx.EXP - NextLevel.get_next_level_exp(ctx.Level - 1);

				// make sure this is before levelup func
				var levels = 0;
				var jobLevels = 0;

				int beforesp = ctx.SP;

				while (nExpSurplus >= 0) // while has more exp than required to reach next level
				{
					ctx.EXP = nExpSurplus;

					if (!ctx.try_process_levelup()) break;

					nExpSurplus = ctx.EXP - NextLevel.get_next_level_exp(ctx.Level - 1);
					levels += 1;

					Parent.NotifySocialChannels(SocialNotiflag.ChangeLevel);

					var job = ctx.Job;
					ctx.AutoJobAdvanceEvent(Parent);
					if (job != ctx.Job) jobLevels += 1;
				}

				if (levels > 0)
				{
					if (ctx.HP > 0)
					{
						ctx.HP = Parent.BasicStats.nMHP;
						ctx.MP = Parent.BasicStats.nMMP;
					}

					var effect = new UserEffectPacket(UserEffect.LevelUp);
					effect.BroadcastEffect(Parent);

					ctx.SP += (short)(jobLevels * 1);

					var aftersp = ctx.SP - beforesp;

					Parent.SendPacket(CPacket.IncSPMessage((byte)aftersp, ctx.Job)); // send sp gain message
					Parent.ValidateStat();
				}
			}

			if ((ctx.Flag & ModifyStatFlags.Money) != 0)
			{
				ctx.Money = Math.Max(0, ctx.Money);
			}

			if ((ctx.Flag & ModifyStatFlags.POP) != 0)
			{
				ctx.POP = Math.Max((short)0, ctx.POP);
			}

			if ((ctx.Flag & ModifyStatFlags.Job) != 0)
			{
				Parent.Skills.SetMasterLevels();

				var effect = new UserEffectPacket(UserEffect.JobChanged);
				effect.BroadcastEffect(Parent);

				if (JobLogic.IsEvan(ctx.Job))
				{
					if (ctx.Job == 2200 && Parent.Dragon is null)
					{
						Parent.Dragon = new CDragon(Parent);
						Parent.Quests.OnCompleteQuest(null, 22100, 0, true);
					}

					Parent.Dragon.SpawnDragonToMap(); // update visuals
				}
				else
				{
					Parent.Modify.Skills(mod =>
					{
						switch (ctx.Job)
						{
							// hacking aran skills together for now. TODO add a ForceVisible field or something to the skill class
							case 2100:
								mod.AddEntry(21000000, entry => entry.CurMastery = entry.MaxLevel);
								mod.AddEntry(21001003, entry => entry.CurMastery = entry.MaxLevel);
								break;
							case 2110:
								mod.AddEntry(21100000, entry => entry.CurMastery = entry.MaxLevel);
								mod.AddEntry(21100002, entry => entry.CurMastery = entry.MaxLevel);
								mod.AddEntry(21100004, entry => entry.CurMastery = entry.MaxLevel);
								mod.AddEntry(21100005, entry => entry.CurMastery = entry.MaxLevel);
								break;
							case 2111:
								mod.AddEntry(21110002, entry => entry.CurMastery = entry.MaxLevel);
								break;
							case 3300:
								mod.AddEntry(30001061, entry => entry.nSLV = 1);
								mod.AddEntry(30001062, entry => entry.nSLV = 1);
								break;
							case 3500:
								mod.AddEntry(30001068, entry => entry.nSLV = 1);
								break;
						}

						foreach (var skill in MasterManager.SkillTemplates.GetJobSkills(ctx.Job))
						{
							var template = MasterManager.SkillTemplates[skill];

							if (template.MasterLevel > 0)
							{
								mod.AddEntry(skill, entry => entry.CurMastery = (byte)template.MasterLevel);
							}
						}
					});
				}

				// notify correct channels
				Parent.NotifySocialChannels(SocialNotiflag.ChangeJob);
			}

			if ((ctx.Flag & ModifyStatFlags.HP) > 0 || (ctx.Flag & ModifyStatFlags.MaxHP) > 0)
			{
				if (ctx.HP <= 0) // TODO move death exp loss (and safety charm) handling here
				{
					Parent.Field.Summons.RemoveCharSummons(Parent.dwId, SummonLeaveType.LEAVE_TYPE_USER_DEAD);
				}

				if (ctx.Job == 132) // dark knight
				{
					Parent.Skills.ProcessDarkKnightDarkForce(Parent.BasicStats.nMHP);
				}
			}

			if (((ctx.Flag & ModifyStatFlags.MP) > 0 || (ctx.Flag & ModifyStatFlags.MaxMP) > 0) && ctx.Job >= 2216 && ctx.Job <= 2218) // evan
			{
				Parent.Skills.CheckDragonFury(Parent.BasicStats.nMMP);
			}

			if ((ctx.Flag & ModifyStatFlags.BasicStat) != 0)
			{
				Parent.ValidateStat();
			}

			if (ctx.Flag > 0)
			{
				Parent.SendPacket(CPacket.StatChanged(ctx, bExclRequest));
			}
		}

		/// <summary>
		/// Adding skills to this function will notify the client and insert them
		///     into the internal skill collection.
		/// No additional SkillEntry processing is required.
		/// Skill points are not deducted.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="exclRequest"></param>
		public void Skills(Action<SkillModifier> action, bool exclRequest = true)
		{
			var ctx = new SkillModifier(Parent);

			action?.Invoke(ctx);

			if (ctx.Count > 0)
			{
				Parent.SendPacket(CPacket.ChangeSkillRecord(ctx, exclRequest));

				if (ctx.NeedsStatRecalc)
				{
					Parent.ValidateStat(true);
				}

				if (ctx.NeedsRegenRecalc)
				{
					Parent.RecalcRegen();
				}
			}
		}

		public void ForcedStat(Action<ForcedStats> action)
		{
			var ctx = Parent.ForcedStats;

			action?.Invoke(ctx);

			if (ctx.Flag > 0)
				Parent.SendPacket(CPacket.ForcedStatSet(ctx));
		}

		public void ResetForcedStat()
		{
			Parent.SendPacket(CPacket.ForcedStatReset());
		}
	}
}
