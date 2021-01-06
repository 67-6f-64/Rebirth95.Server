using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Skill
{
	public sealed class SkillTemplate : AbstractTemplate
	{
		/// <summary>
		/// 0-index skill level data accessor. Takes raw index as input.
		/// </summary>
		[JsonProperty]
		private SkillLevelData[] _data;

		[JsonIgnore]
		public int DataLength => _data.Length;

		/// <summary>
		/// 1-index skill level data accessor. Takes skill level as input.
		/// </summary>
		/// <param name="nSLV"></param>
		/// <returns></returns>
		public SkillLevelData this[int nSLV]
		{
			get => _data[nSLV - 1];
			set => _data[nSLV - 1] = !Locked ? value : throw new TemplateAccessException();
		}

		/// <summary>
		/// Skills and skill levels required before character is allowed to assign sp to this skill
		/// </summary>
		public Dictionary<int, int> Req { get; set; }
		public int[] AdditionPsd { get; set; }

		public SkillTemplate(int nSkillID) : base(nSkillID)
		{
			AdditionPsd = new int[0];
			Req = new Dictionary<int, int>();
		}

		public void InitSLD(int size)
			=> _data = !Locked
				? new SkillLevelData[size]
				: throw new TemplateAccessException();

		public short ParameterA(int level) => (short)X(level);
		public short ParameterB(int level) => (short)Y(level);

		/// <summary>
		/// Will return a copy of the existing Rect so the values won't be overriden.
		/// </summary>
		[JsonIgnore]
		public TagRect Rect => new TagRect(LT.X, LT.Y, RB.X, RB.Y);

		public Point LT { get; set; }
		public Point RB { get; set; }

		public bool HasAffected { get; set; }
		public bool HasSpecial { get; set; }
		public ElemAttrType ElemAttr { get; set; }
		public int CombatOrders { get; set; }
		public int MaxLevel { get; set; }
		public bool Invisible { get; set; }
		public int Morph { get; set; }
		public int MasterLevel { get; set; }
		public int SkillType { get; set; }
		/// <summary>
		/// Required equipped weapon type
		/// </summary>
		public int Weapon { get; set; }

		/// <summary>
		/// Passive skill data
		/// </summary>
		public int PsdSkill { get; set; }

		public bool CanFinalAttack { get; set; }

		/// <summary>
		/// Determines if a skill is permanently disabled from user view
		/// </summary>
		public bool IsHiddenSkill { get; set; }
		public bool IsWeaponBoosterSkill { get; set; }
		public bool IsPartyBuff { get; set; }
		public bool IsPrepareAttackSkill { get; set; }
		public bool IsWeaponChargeSkill { get; set; }
		public bool IsSharpEyesSkill { get; set; }
		public bool IsStanceSkill { get; set; }
		public bool IsHyperBodySkill { get; set; }
		public bool IsDashSkill { get; set; }
		public bool IsMapleWarriorSkill { get; set; }
		/// <summary>
		/// This is only true if the skill has a bufftime but is not an actual buff
		/// </summary>
		public bool IsNotBuff { get; set; }

		public bool is_skill_need_master_level { get; set; }
		public bool is_heros_will_skill { get; set; }
		public bool is_shoot_skill_not_consuming_bullet { get; set; }
		public bool is_keydown_skill { get; set; }
		public bool is_event_vehicle_skill { get; set; }
		public bool is_antirepeat_buff_skill { get; set; }
		public short get_required_combo_count { get; set; }

		public bool IsSummonSkill { get; set; }
		public bool IsAttackOnDieSummonSkill { get; set; }
		public bool IsAttackSummonSkill { get; set; }
		public int SummonSkillAttackAttackAfter { get; set; }
		public int SummonSkillAttackMobCount { get; set; }
		public int SummonSkillDieAttackAfter { get; set; }
		public int SummonSkillDieMobCount { get; set; }
		public bool IsFlySummonSkill { get; set; }

		public int ItemConsumeAmount { get; set; }
		public int BulletCount { get; set; }
		public int BulletConsume { get; set; }
		public int ItemConsume { get; set; }
		public int OptionalItemCost { get; set; }

		public int FixDamage(int nLevel) => nLevel > MaxLevel + CombatOrders ? 0 : this[nLevel].FixDamage;

		public int AttackCount(int nLevel) => nLevel > MaxLevel + CombatOrders ? 0 : this[nLevel].AttackCount;
		public int MobCount(int level, int nDefaultCount = 1)
		{
			if (level > MaxLevel + CombatOrders) return 0;

			var count = this[level].MobCount;

			return count < nDefaultCount ? nDefaultCount : count;
		}

		public int Time(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Time;
		public int SubTime(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].SubTime;
		public int MpCon(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MpCon;
		public int CostHP(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].HpCon;
		public int Damage(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Damage;
		public int Mastery(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Mastery;
		public int DamR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].DamR;
		public int Dot(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Dot;
		public int DotTime(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].DotTime;
		public int MesoR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MESOr;
		public int Speed(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Speed;
		public int Jump(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Jump;
		public int PAD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].PAD;
		public int PDD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].PDD;
		public int MAD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MAD;
		public int MDD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MDD;
		public int ACC(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].ACC;
		public int EVA(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EVA;
		public int HP(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].HP;
		public int MHPR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MHPr;
		public int MP(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MP;
		public int MMPR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].MMPr;
		public int Prop(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Prop;
		public int SubProp(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].SubProp;
		public int Cooltime(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Cooltime;
		public int ASRR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].ASRr;
		public int TERR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].TERr;

		public int EMDD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EMDD;
		public int EMHP(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EMHP;
		public int EMMP(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EMMP;
		public int EPAD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EPAD;
		public int EPDD(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].EPDD;

		public int CR(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Cr;
		public int CriticalDamageMin(int nLevel)=> nLevel > MaxLevel + CombatOrders ? 0 : this[nLevel].CDMin;
		public int CriticalDamageMax(int nLevel) => nLevel > MaxLevel + CombatOrders ? 0 : this[nLevel].CDMax;

		public double U(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].U;
		public double V(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].V;
		public double W(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].W;
		public double X(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].X;
		public double Y(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Y;
		public double Z(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].Z;
		public double T(int level) => level > MaxLevel + CombatOrders ? 0 : this[level].T;
	}
}
