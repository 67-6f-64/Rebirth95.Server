namespace Rebirth.Provider.Template.Skill
{
	public sealed class SkillLevelData
	{
		public int FixDamage { get; set; }
		public int AttackCount { get; set; }
		public int MobCount { get; set; }
		public int Time { get; set; }
		public int SubTime { get; set; }
		public int MpCon { get; set; }
		public int HpCon { get; set; }
		public int Damage { get; set; }
		public int Mastery { get; set; }
		/// <summary>
		/// Client calls this DIPr
		/// </summary>
		public int DamR { get; set; }
		public int Dot { get; set; }
		public int DotTime { get; set; }
		public int MESOr { get; set; }
		public int EXPr { get; set; }
		public int Speed { get; set; }
		public int Jump { get; set; }
		public int PAD { get; set; }
		public int PADr { get; set; }
		public int PADx { get; set; }
		public int MAD { get; set; }
		public int MADr { get; set; }
		public int MADx { get; set; }
		public int PDD { get; set; }
		public int PDDr { get; set; }
		public int MDD { get; set; }
		public int MDDr { get; set; }
		public int EVA { get; set; }
		public int EVAr { get; set; } // not in wz??
		public int ACC { get; set; }
		public int ACCr { get; set; } // not in wz??
		public int HP { get; set; }
		public int MHPr { get; set; }
		public int MP { get; set; }
		public int MMPr { get; set; }
		public int Prop { get; set; }
		public int SubProp { get; set; }
		public int Cooltime { get; set; }
		public int ASRr { get; set; }
		public int TERr { get; set; }
		public int EMDD { get; set; }
		public int EMHP { get; set; }
		public int EMMP { get; set; }
		public int EPAD { get; set; }
		public int EPDD { get; set; }
		public int Cr { get; set; } // critical rate
		public int Er { get; set; } // evasionR
		public int Ar { get; set; }
		public int CDMin { get; set; } // criticalDamageMin
		public int CDMax { get; set; }

		public int PDamr { get; set; }
		public int MDamr { get; set; }

		public int OCr { get; set; }
		public int DCr { get; set; }
		public int IMDr { get; set; }
		public int IMPr { get; set; }

		public int PsdJump { get; set; }
		public int PsdSpeed { get; set; }

		public double T { get; set; }
		public double U { get; set; }
		public double V { get; set; }
		public double W { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
	}

	public class AdditionPsd
	{
		public int Cr { get; set; }
		public int CDMin { get; set; }
		public int Ar { get; set; }
		/// <summary>
		/// WZ attribute name for this is damR
		/// </summary>
		public int DIPr { get; set; }
		public int PDamr { get; set; }
		public int MDamr { get; set; }
		public int IMPr { get; set; }
	}
}