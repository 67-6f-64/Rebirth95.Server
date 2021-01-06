using System;
using System.Drawing;
using Newtonsoft.Json;
using Rebirth.Common.Tools;

namespace Rebirth.Provider.Template.Skill
{
	public sealed class MobSkillLevelData
	{
		public MobSkillLevelData()
		{
			SummonIDs = new int[0];
		}

		public int nSkillID { get; set; }
		public int nSLV { get; set; }
		/// <summary>
		/// Skill cooldown (interval between casts) in seconds.
		/// </summary>
		public int Interval { get; set; }
		public int MpCon { get; set; }
		/// <summary>
		/// Duration of the (de)buff in seconds.
		/// </summary>
		public int Time { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		/// <summary>
		/// Minimum mob HP for this skill to be cast.
		/// </summary>
		public int HP { get; set; }
		public int Prop { get; set; }
		/// <summary>
		/// Rolls the prop dice. Will return true if prop is outside the exclusive range of (100, 0)
		/// </summary>
		/// <param name="rand">Random seed</param>
		/// <returns>true if prop is successful, else false</returns>
		public bool DoProp(Random rand)
			=> Prop >= 100 || Prop <= 0
						   || Prop > rand.Next(100);

		public Point LT { get; set; }
		public Point RB { get; set; }

		[JsonIgnore]
		public TagRect Rect => new TagRect(LT.X, LT.Y, RB.X, RB.Y);

		public int[] SummonIDs { get; set; }
		public int SummonEffect { get; set; }
		public int Limit { get; set; }
		public int Count { get; set; }
		public bool RandomTarget { get; set; }
	}
}