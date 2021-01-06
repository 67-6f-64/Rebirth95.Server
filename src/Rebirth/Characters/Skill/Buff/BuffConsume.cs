using System;
using Rebirth.Common.Types;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Item.Cash;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Skill.Buff
{
	public class BuffConsume : AbstractBuff
	{
		public AbstractItemTemplate Template { get; }

		public BuffConsume(int nItemID)
			: base(-Math.Abs(nItemID), 1)
		{
			Template = MasterManager.ItemTemplate(nSkillID); // skill id cuz buff id is negative
		}

		public void GenerateBeholderBuff(Character c)
		{
			var skill = c.Skills.Get((int)Skills.DARKKNIGHT_BEHOLDERS_BUFF, true);
			var template = skill.Template;

			tDuration = skill.BuffTimeMillis;
			StartTime = DateTime.Now;

			switch (-nBuffID)
			{
				case 2022125: // An increase in weapon defense with a little help from a Dark Spirit.
					AddSecondaryStat(SecondaryStatFlag.EPDD, template.EPDD(skill.nSLV));
					break;
				case 2022126: // An increase in magic defense with a little help from a Dark Spirit.
					AddSecondaryStat(SecondaryStatFlag.EMDD, template.EMDD(skill.nSLV));
					break;
				case 2022127: // An increase in accuracy with a little help from a Dark Spirit.
					AddSecondaryStat(SecondaryStatFlag.ACC, template.ACC(skill.nSLV));
					break;
				case 2022128: // An increase in avoidability with a little help from a Dark Spirit.
					AddSecondaryStat(SecondaryStatFlag.EVA, template.EVA(skill.nSLV));
					break;
				case 2022129: // An increase in attack with a little help from a Dark Spirit.
					AddSecondaryStat(SecondaryStatFlag.EPAD, template.EPAD(skill.nSLV));
					break;
				default: throw new InvalidOperationException($"Invalid type passed to beholder skill. BuffID: {nBuffID}");
			}
		}

		/// <summary>
		/// Used for consumeables with or without a bufftime.
		/// Will add any buffs associated with the item id if they exist.
		/// Processes non-buff item effects as well.
		/// </summary>
		public void GenerateConsumeable(Character c)
		{
			StartTime = DateTime.Now;

			if (Template is ConsumeItemTemplate data)
			{
				if (data.Cure_Poison)
					c.Buffs.RemoveIf(b => b.StatType == SecondaryStatFlag.Poison);

				if (data.Cure_Seal)
					c.Buffs.RemoveIf(b => b.StatType == SecondaryStatFlag.Seal);

				if (data.Cure_Darkness)
					c.Buffs.RemoveIf(b => b.StatType == SecondaryStatFlag.Darkness);

				if (data.Cure_Curse)
					c.Buffs.RemoveIf(b => b.StatType == SecondaryStatFlag.Curse);

				if (data.Cure_Weakness)
					c.Buffs.RemoveIf(b => b.StatType == SecondaryStatFlag.Weakness);

				var eMod = c.Buffs.PotionEffectivenessModifier();

				var healHP = (int)(data.HP * eMod);
				var healMP = (int)(data.MP * eMod);

				c.Modify.Heal(healHP, healMP);

				c.StatisticsTracker.nRecoveredPotionHP += healHP;
				c.StatisticsTracker.nRecoveredPotionMP += healMP;

				if (data.HPR > 0 || data.MMPR > 0)
				{
					healHP = (int)(c.BasicStats.nMHP * data.HPR * 0.01);
					healMP = (int)(c.BasicStats.nMMP * data.MPR * 0.01);

					c.Modify.Heal(healHP, healMP);

					c.StatisticsTracker.nRecoveredPotionHP += healHP;
					c.StatisticsTracker.nRecoveredPotionMP += healMP;
				}

				if (data.EXP > 0)
				{
					c.Modify.GainExp(data.EXP);
				}

				if (data.ExpInc > 0)
				{
					c.Modify.GainExp(data.ExpInc);
				}

				// i think this should be last
				if (data.BuffTime > 0)
				{
					Generate();
					c.Buffs.AddItemBuff(this);
				}
			}
			else if (Template is MorphItemTemplate t)
			{
				tDuration = t.Time; // already in millis

				if (tDuration <= 0) return;

				if (AddSecondaryStat(SecondaryStatFlag.Morph, t.Morph, stat => stat > 0, true))
				{
					StatType = SecondaryStatFlag.Morph;
					c.Buffs.AddItemBuff(this);
				}
			}
		}

		/// <summary>
		/// Used for items that have bufftime.
		/// </summary>
		/// <param name="dBufftimeModifier"></param>
		public override void Generate(double dBufftimeModifier = 1.0)
		{
			StartTime = DateTime.Now;

			if (Template is ConsumeItemTemplate template)
			{
				tDuration = (int)(template.BuffTime * dBufftimeModifier);

				if (template.ExpUpByItem)
				{
					AddSecondaryStat(SecondaryStatFlag.ExpBuffRate, template.ExpBuffRate);
					//ExpRateModifier = template.ExpUpByItem;
					//RateModifierAmount = template.ExpBuff - 100; // all these are 1xx while the others are 0xx
					//StatType = SecondaryStatFlag.ExpBuffRate;
					IsRateModifier = true;
				}

				if (template.ItemUpByItem)
				{
					AddSecondaryStat(SecondaryStatFlag.ItemUpByItem, template.Prob);
					//ItemRateModifier = template.ItemUpByItem; // drop rate increase
					//RateModifierAmount = template.Prob;
					//StatType = SecondaryStatFlag.ItemUpByItem;
					IsRateModifier = true;
				}

				if (template.MesoUpByItem)
				{
					AddSecondaryStat(SecondaryStatFlag.MesoUpByItem, template.Prob);
					//MesoRateModifier = template.MesoUpByItem;
					//RateModifierAmount = template.Prob;
					//StatType = SecondaryStatFlag.MesoUpByItem;
					IsRateModifier = true;
				}

				AddSecondaryStat(SecondaryStatFlag.PAD, template.PAD, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.PDD, template.PDD, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.MAD, template.MAD, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.MDD, template.MDD, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.ACC, template.ACC, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.EVA, template.EVA, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.Speed, template.Speed, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.Jump, template.Jump, stat => stat > 0);
				AddSecondaryStat(SecondaryStatFlag.DojangShield, template.DojangShield, stat => stat > 0);

				if (AddSecondaryStat(SecondaryStatFlag.Morph, template.Morph, stat => stat > 0, true))
				{
					StatType = SecondaryStatFlag.Morph;
				}
			}
		}

		public static void FireAndForget(Character c, int nItemID)
			=> new BuffConsume(nItemID).GenerateConsumeable(c);
	}
}
