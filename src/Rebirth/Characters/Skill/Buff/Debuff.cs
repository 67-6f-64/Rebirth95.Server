using System;
using Rebirth.Characters.Stat;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Skill.Buff
{
	public class Debuff : AbstractBuff
	{
		public Debuff(int nSkillID, int nSLV)
			: base(nSkillID, (byte)nSLV) { }

		public int nOption { get; set; }

		public override void Generate(double tDurationModifier = 1.0)
		{
			StartTime = DateTime.Now;

			if (StatType == SecondaryStatFlag.Undead)
			{
				Stat.Add(StatType, new TwoStateSecondaryStatEntry(true));
			}
			else
			{
				var entry = new SecondaryStatEntry()
				{
					nValue = nOption,
					rValue = nBuffID | (nSLV << 16),
					tValue = tDuration,
				};

				Stat.Add(StatType, entry);
			}
		}
	}
}
