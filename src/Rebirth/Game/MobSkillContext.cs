using System;

namespace Rebirth.Field.Life
{
	public class MobSkillContext
	{
		public int nSkillID { get; }
		public int nSLV { get; }
		public DateTime tLastSkillUse { get; set; }
		public int nSummoned { get; set; }

		public MobSkillContext(int id, int level)
		{
			nSkillID = id;
			nSLV = level;

			Reset();
		}

		public void Reset()
		{
			tLastSkillUse = DateTime.Now.AddYears(-1);
			nSummoned = 0;
		}
	}
}
