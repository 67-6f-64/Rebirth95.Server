using System;
using System.Collections.Generic;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Stat
{
	public class AdditionPsd
	{
		public int nCr { get; set; }
		public int nCDMin { get; set; }
		public int nAr { get; set; }
		/// <summary>
		/// WZ attribute name for this is damR
		/// </summary>
		public int nDIPr { get; set; }
		public int nPDamr { get; set; }
		public int nMDamr { get; set; }
		public int nIMPr { get; set; }
	}

	public class PassiveSkillData
	{
		public int nMHPr { get; set; }
		public int nMMPr { get; set; }
		public int nCr { get; set; }
		public int nCDMin { get; set; }
		public int nACCr { get; set; }
		public int nEVAr { get; set; }
		public int nAr { get; set; }
		public int nEr { get; set; }
		public int nPDDr { get; set; }
		public int nMDDr { get; set; }
		public int nPDr { get; set; }
		public int nMDr { get; set; }
		public int nDIPr { get; set; }
		public int nPDamr { get; set; }
		public int nMDamr { get; set; }
		public int nPADr { get; set; }
		public int nMADr { get; set; }
		public int nEXPr { get; set; }
		public int nIMPr { get; set; }
		public int nASRr { get; set; }
		public int nTERr { get; set; }
		public int nMESOr { get; set; }
		public int nPADx { get; set; }
		public int nMADx { get; set; }
		public int nIMDr { get; set; }
		public int nPsdJump { get; set; }
		public int nPsdSpeed { get; set; }
		public int nOCr { get; set; }
		public int nDCr { get; set; }
		public Dictionary<int, AdditionPsd> AdditionPsd { get; }

		private DateTime tLastPassiveSkillDataUpdate;

		public PassiveSkillData()
		{
			AdditionPsd = new Dictionary<int, AdditionPsd>();
			tLastPassiveSkillDataUpdate = DateTime.MinValue;
		}

		// CUserLocal::UpdatePassiveSkillData
		public void Update(Character parent)
		{
			if (tLastPassiveSkillDataUpdate.MillisSinceStart() < 10000) return;
			tLastPassiveSkillDataUpdate = DateTime.Now;

			Clear();

			// TODO guild skills here

			foreach (var skill in parent.Skills)
			{
				var template = skill.Template;

				if (template.PsdSkill <= 0) continue;
				if (template.TemplateId == (int)Skills.MECHANIC_PERFECT_ARMOR) continue;

				if (template.TemplateId == (int)Skills.MECHANIC_SIEGE2_SPECIAL)
				{
					var slv = parent.Skills.Get((int)Skills.MECHANIC_SIEGE1)?.nSLV ?? 0;
					SetPSD(skill, slv);
				}
				else if (skill.nSLV > 0)
				{
					SetPSD(skill, skill.nSLV);
				}
			}

			if (parent.Stats.nJob / 100 == 35) // mechanic
			{
				if (parent.Skills.Get((int)Skills.MECHANIC_SIEGE2) != null)
				{
					int slv = parent.Skills.Get((int)Skills.MECHANIC_SIEGE1)?.nSLV ?? 0;

					SetPSD(parent.Skills.Get((int)Skills.MECHANIC_SIEGE2_SPECIAL), slv);
				}
			}

			//if (parent.Buffs.GetByType(SecondaryStatFlag.Dice) is BuffSkill diceBuff && diceBuff.State > 1)

			if (parent.Stats.SecondaryStats.nDice > 1)
			{
				foreach (var buff in parent.Buffs)
				{
					if (buff.Stat.TryGetValue(SecondaryStatFlag.Dice, out _))
					{
						nMHPr += buff.Stat.aDiceInfo[0];
						nMMPr += buff.Stat.aDiceInfo[1];
						nCr += buff.Stat.aDiceInfo[2];
						nCDMin += buff.Stat.aDiceInfo[3];
						nEVAr += buff.Stat.aDiceInfo[5];
						nAr += buff.Stat.aDiceInfo[6];
						nEr += buff.Stat.aDiceInfo[7];
						nPDDr += buff.Stat.aDiceInfo[8];
						nMDDr += buff.Stat.aDiceInfo[9];
						nPDr += buff.Stat.aDiceInfo[10];
						nMDr += buff.Stat.aDiceInfo[11];
						nDIPr += buff.Stat.aDiceInfo[12];
						nPDamr += buff.Stat.aDiceInfo[13];
						nMDamr += buff.Stat.aDiceInfo[14];
						nPADr += buff.Stat.aDiceInfo[15];
						nMADr += buff.Stat.aDiceInfo[16];
						nEXPr += buff.Stat.aDiceInfo[17];
						nIMPr += buff.Stat.aDiceInfo[18];
						nASRr += buff.Stat.aDiceInfo[19];
						nTERr += buff.Stat.aDiceInfo[20];
						nMESOr += buff.Stat.aDiceInfo[21];
					}
				}
			}

			// validate inclusive range 0,100
			if (nMESOr < 0) nMESOr = 0;
			else if (nMESOr > 100) nMESOr = 100;

			// validate inclusive range 0,50
			if (nOCr < 0) nOCr = 0;
			else if (nOCr > 50) nOCr = 50;

			// validate inclusive range 0,50
			if (nDCr < 0) nDCr = 0;
			else if (nDCr > 50) nDCr = 50;
		}

		// CUserLocal::SetPassiveSkillData
		public void SetPSD(SkillEntry se, int nSLV)
		{
			if (se is null || nSLV <= 0) return;

			if (se.Template.DataLength < nSLV)
				throw new ArgumentOutOfRangeException(nameof(nSLV),
					$"Skill level outside skill data length. Level: {nSLV} | SkillID: {se.nSkillID}");

			var sd = se.Template[nSLV];

			if (sd.MHPr > 0) nMHPr += sd.MHPr;
			if (sd.MMPr > 0) nMMPr += sd.MMPr;
			if (sd.ACCr > 0) nACCr += sd.ACCr;
			if (sd.EVAr > 0) nEVAr += sd.EVAr;
			if (sd.Er > 0) nEr += sd.Er;
			if (sd.PDDr > 0) nPDDr += sd.PDDr;
			if (sd.MDDr > 0) nMDDr += sd.MDDr;
			if (sd.PDamr > 0) nPDamr += sd.PDamr;
			if (sd.MDamr > 0) nMDamr += sd.MDamr;
			if (sd.PADr > 0) nPADr += sd.PADr;
			if (sd.MADr > 0) nMADr += sd.MADr;
			if (sd.EXPr > 0) nEXPr += sd.EXPr;
			if (sd.IMPr > 0) nIMPr += sd.IMPr;
			if (sd.ASRr > 0) nASRr += sd.ASRr;
			if (sd.TERr > 0) nTERr += sd.TERr;
			if (sd.MESOr > 0) nMESOr += sd.MESOr;
			if (sd.PADx > 0) nPADx += sd.PADx;
			if (sd.MADx > 0) nMADx += sd.MADx;
			if (sd.IMDr > 0) nIMDr += sd.IMDr;
			if (sd.PsdJump > 0) nPsdJump += sd.PsdJump;
			if (sd.PsdSpeed > 0) nPsdSpeed += sd.PsdSpeed;
			if (sd.OCr > 0) nOCr += sd.OCr;
			if (sd.DCr > 0) nDCr += sd.DCr;

			if (se.Template.AdditionPsd.Length <= 0 || se.Template.PsdSkill == 2)
			{
				nCr += sd.Cr;
				nCDMin += sd.CDMin;
				nAr += sd.Ar;
				nDIPr += sd.DamR; // dipr == damr in wz
				nPDamr += sd.PDamr;
				nMDamr += sd.MDamr;
			}

			foreach (var psdSkill in se.Template.AdditionPsd)
			{
				if (!AdditionPsd.ContainsKey(psdSkill))
				{
					AdditionPsd.Add(psdSkill, new AdditionPsd());
				}

				AdditionPsd[psdSkill].nCr = sd.Cr;
				AdditionPsd[psdSkill].nCDMin += sd.CDMin;
				AdditionPsd[psdSkill].nAr += sd.Ar;
				AdditionPsd[psdSkill].nDIPr += sd.DamR; // DIPr == DamR in wz
				AdditionPsd[psdSkill].nPDamr += sd.PDamr;
				AdditionPsd[psdSkill].nMDamr += sd.MDamr;
				AdditionPsd[psdSkill].nIMPr += sd.IMPr;
			}
		}

		public void Clear()
		{
			nMHPr = 0;
			nMMPr = 0;
			nCr = 0;
			nCDMin = 0;
			nACCr = 0;
			nEVAr = 0;
			nAr = 0;
			nEr = 0;
			nPDDr = 0;
			nMDDr = 0;
			nPDr = 0;
			nMDr = 0;
			nDIPr = 0;
			nPDamr = 0;
			nMDamr = 0;
			nPADr = 0;
			nMADr = 0;
			nEXPr = 0;
			nIMPr = 0;
			nASRr = 0;
			nTERr = 0;
			nMESOr = 0;
			nPADx = 0;
			nMADx = 0;
			nIMDr = 0;
			nPsdJump = 0;
			nPsdSpeed = 0;
			nOCr = 0;
			nDCr = 0;
			AdditionPsd.Clear();
		}
	}
}
