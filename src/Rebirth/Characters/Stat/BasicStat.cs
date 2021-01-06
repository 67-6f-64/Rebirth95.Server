using System.Linq;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.Types;
using Rebirth.Game;

namespace Rebirth.Characters.Stat
{
	public class BasicStat
	{
		public int ExpRate { get; private set; }
		public int MesoRate { get; private set; }
		public int DropRate { get; private set; }

		public int nSTR { get; private set; }
		public int nDEX { get; private set; }
		public int nINT { get; private set; }
		public int nLUK { get; private set; }

		public int nMHP { get; private set; }
		public int nMMP { get; private set; }


		//BasicStat::SetFrom(...)
		public void SetFrom(Character pChar)
		{
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();

			var c = pChar.Stats;
			var buffs = pChar.Buffs;

			nSTR = c.nSTR;
			nDEX = c.nDEX;
			nINT = c.nINT;
			nLUK = c.nLUK;
			nMHP = c.nMHP;
			nMMP = c.nMMP;

			var nSTRr = 100f;
			var nDEXr = 100f;
			var nINTr = 100f;
			var nLUKr = 100f;

			var nMHPr = 100f;
			var nMMPr = 100f;

			process_equips(pChar.EquippedInventoryNormal);
			process_equips(pChar.aDragonEquipped);
			process_equips(pChar.aMechanicEquipped);

			void process_equips(CharacterInventoryEquips inv)
			{
				foreach (var item in inv)
				{
					try
					{
						nSTR += item.Value.niSTR;
						nDEX += item.Value.niDEX;
						nINT += item.Value.niINT;
						nLUK += item.Value.niLUK;
						nMHP += item.Value.niMaxHP;
						nMMP += item.Value.niMaxMP;

						if (!item.Value.HasVisiblePotential) continue;

						foreach (var option in item.Value.nOptionData)
						{
							if (option is null) continue; // when < 3 potential lines

							nSTR += option.iSTR;
							nDEX += option.iDEX;
							nINT += option.iINT;
							nLUK += option.iLUK;
							nMHP += option.iMaxHP;
							nMMP += option.iMaxMP;

							nSTRr += option.iSTRr;
							nDEXr += option.iDEXr;
							nINTr += option.iINTr;
							nLUKr += option.iLUKr;
							nMHPr += option.iMaxHPr;
							nMMPr += option.iMaxMPr;
						}
					}
					catch
					{
						pChar.SendMessage("Error ocurred when calculating local stats for equip " + item.Value.nItemID);
					}
				}
			}

			// set rates to zero
			//ExpRate; = 0;
			//MesoRate = 0;
			//DropRate = 0;

			// get highest rate modifier from each type
			//foreach (var item in buffs) // TODO phase this into SecondaryStatValues
			//{
			//	if (item is BuffConsume)
			//	{
			//		if (item.RateModifierAmount <= 0) continue;

			//		if (item.ExpRateModifier && ExpRate < item.RateModifierAmount)
			//		{
			//			ExpRate = item.RateModifierAmount;
			//		}
			//		if (item.ItemRateModifier && DropRate < item.RateModifierAmount)
			//		{
			//			DropRate = item.RateModifierAmount;
			//		}
			//		if (item.MesoRateModifier && MesoRate < item.RateModifierAmount)
			//		{
			//			MesoRate = item.RateModifierAmount;
			//		}
			//	}
			//}

			// add the base rate (100%)
			ExpRate = 100;//+= 100;
			DropRate = 100;//+= 100;
			MesoRate = 100;//+= 100;

			ExpRate += pChar.Stats.PassiveSkillData.nEXPr;
			MesoRate += pChar.Stats.PassiveSkillData.nMESOr;

			//// shadower meso mastery -- Skills.SHADOWER_GRID -- passive skill
			//if (skills.GetOrDefault((int)Skills.SHADOWER_GRID) is SkillEntry mesomastery)
			//{
			//	MesoRate += mesomastery.Template.MesoR(mesomastery.nSLV); // additive
			//}

			//if (buffs.GetByType(SecondaryStatFlag.Dice) is BuffSkill dice)
			//{
			//	if (dice.Stat.nDiceStat == 6)
			//	{
			//		ExpRate += 30; // additive -- constant value
			//	}
			//	else if (dice.Stat.nDiceStat == 3)
			//	{
			//		nMHPr += 20; // additive -- constant value
			//	}
			//}

			//if (buffs.GetByType(SecondaryStatFlag.ExpBuffRate) is BuffSkill expbuffrate)
			//{
			//	ExpRate += (int)expbuffrate.Template.X(expbuffrate.nSLV);
			//}

			MesoRate += pChar.Stats.SecondaryStats.nPickPocket;
			ExpRate += pChar.Stats.SecondaryStats.nHolySymbol;
			MesoRate += pChar.Stats.SecondaryStats.nMesoUp;

			//MesoRate += buffs.nOptionData(SecondaryStatFlag.PickPocket);
			//ExpRate += buffs.nOptionData(SecondaryStatFlag.HolySymbol);
			//MesoRate += buffs.nOptionData(SecondaryStatFlag.MesoUp);

			// add cash rate modifiers (its either none or 2x for simplicity) -- multiplicative
			//if (cashItems.Any(item => ItemConstants.IsExpCoupon(item.Value.nItemID)))
			//{
			//	ExpRate *= 2;
			//}

			//if (cashItems.Any(item => ItemConstants.IsDropCoupon(item.Value.nItemID)))
			//{
			//	DropRate *= 2;
			//	MesoRate *= 2;
			//}

			if (pChar.Stats.SecondaryStats.nExpBuffRate > 0)
			{
				ExpRate = ExpRate * pChar.Stats.SecondaryStats.nExpBuffRate / 100;
			}

			if (pChar.InventoryCash.ExpCouponRate > 0)
			{
				ExpRate *= pChar.InventoryCash.ExpCouponRate;
			}

			if (pChar.InventoryCash.DropCouponRate > 0)
			{
				DropRate *= pChar.InventoryCash.DropCouponRate;
				MesoRate *= pChar.InventoryCash.DropCouponRate;
			}

			if (buffs[(int)MobSkill.MobSkillID.CURSE] != null)
			{
				ExpRate /= 2;
			}

			// nPdsMHPr, nPdsMMPr -- Passive Skill Data
			//foreach (var item in skills)
			//{
			//	if (item.Template.PsdSkill > 0 || item.nSLV <= 0) continue;

			//	nMHPr += item.Template.MHPR(item.nSLV);
			//	nMMPr += item.Template.MMPR(item.nSLV);
			//}

			if (buffs[(int)Skills.WILDHUNTER_JAGUAR_RIDING] is BuffSkill jaguarRiding)
			{
				// cant use SecondaryStatValues class cuz this value isnt stored in nOption
				nMHPr += (int)jaguarRiding.Template.W(jaguarRiding.nSLV);
			}

			nMHPr += pChar.Stats.SecondaryStats.nMaxHP;
			nMMPr += pChar.Stats.SecondaryStats.nMaxMP;
			nMHP += pChar.Stats.SecondaryStats.nMaxHP;
			nMHPr += pChar.Stats.SecondaryStats.nConversion;
			nMHP += pChar.Stats.SecondaryStats.nEMHP;
			nMMP += pChar.Stats.SecondaryStats.nEMMP;

			//foreach (var buff in buffs)
			//{
			//	if (buff.nBuffID == (int)Skills.WILDHUNTER_JAGUAR_RIDING)
			//	{
			//		nMHPr += 10 + 2 * buff.nSLV; // Template.W
			//	}
			//	else
			//	{
			//		if (buff.Stat.ContainsKey(SecondaryStatFlag.BasicStatUp))
			//			nBasicStatInc = buff.Stat[SecondaryStatFlag.BasicStatUp].nValue;

			//		if (buff.Stat.ContainsKey(SecondaryStatFlag.MaxHP))
			//			nMHPr += buff.Stat[SecondaryStatFlag.MaxHP].nValue;

			//		if (buff.Stat.ContainsKey(SecondaryStatFlag.MaxMP))
			//			nMMPr += buff.Stat[SecondaryStatFlag.MaxMP].nValue;

			//		if (buff.Stat.ContainsKey(SecondaryStatFlag.MoreWild))
			//			nMHP += buff.Stat[SecondaryStatFlag.MorewildMaxHP].nValue;

			//		if (buff.Stat.ContainsKey(SecondaryStatFlag.Conversion))
			//			nMHPr += buff.Stat[SecondaryStatFlag.Conversion].nValue;

			//		if (buff.Stat.TryGetValue(SecondaryStatFlag.EMHP, out var emhp))
			//			nMHP += emhp.nValue;

			//		if (buff.Stat.TryGetValue(SecondaryStatFlag.EMMP, out var emmp))
			//			nMMP += emmp.nValue;
			//	}
			//}

			// add all the basic stats together
			// multiply basic stats by nBasicStatInc (maple hero buff or whatever)
			// multiply the cumulative rate modifiers by the base values and add them together

			var nBasicStatInc = pChar.Stats.SecondaryStats.nBasicStatUp;

			if (nBasicStatInc > 0)
			{
				nSTR = (int)(nSTR * nBasicStatInc * 0.01f);
				nDEX = (int)(nDEX * nBasicStatInc * 0.01f);
				nINT = (int)(nINT * nBasicStatInc * 0.01f);
				nLUK = (int)(nLUK * nBasicStatInc * 0.01f);
			}

			if (nSTRr > 100)
			{
				nSTR = (int)(nSTR * nSTRr * 0.01);
			}
			if (nDEXr > 100)
			{
				nDEX = (int)(nDEX * nDEXr * 0.01);
			}
			if (nINTr > 100)
			{
				nINT = (int)(nINT * nINTr * 0.01);
			}
			if (nLUKr > 100)
			{
				nLUK = (int)(nLUK * nLUKr * 0.01);
			}

			if (nMHPr > 100)
			{
				nMHP = (int)(nMHP * nMHPr * 0.01);
			}
			if (nMMPr > 100)
			{
				nMMP = (int)(nMMP * nMMPr * 0.01);
			}

			if (nMHP > 99999)
			{
				nMHP = 99999;
			}
			if (nMMP > 99999)
			{
				nMMP = 99999;
			}

			stopwatch.Stop();
#if DEBUG
			var ts = stopwatch.Elapsed;
			pChar.SendMessage($"Stat recalc time: {pChar.dwId} | {ts.TotalMilliseconds}");
#endif
		}

		public int CalcBasePACC() =>
			(int)(nLUK + nDEX * 1.2);

		public int CalcBaseMACC() => // not a nexon function
			(int)(nINT + nLUK * 1.2);

		public int CalcBaseMDD() =>
			(int)(nSTR * 0.4 +
				  nDEX * 0.5 +
				  nLUK * 0.5 +
				  nINT * 1.2);

		public int CalcBasePDD() =>
			(int)(nINT * 0.4 +
				  nLUK * 0.5 +
				  nDEX * 0.5 +
				  nSTR * 1.2);
	}
}
