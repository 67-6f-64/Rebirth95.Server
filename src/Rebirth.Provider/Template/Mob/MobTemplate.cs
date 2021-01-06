using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Common.Types;

namespace Rebirth.Provider.Template.Mob
{
	public class MobTemplate : AbstractTemplate
	{
		public int Level { get; set; }
		public int MaxHP { get; set; }
		public int MaxMP { get; set; }
		public int HpRecovery { get; set; }
		public int MpRecovery { get; set; }
		public int PAD { get; set; }
		public int PDD { get; set; }
		public int MAD { get; set; }
		public int MDD { get; set; }
		public int PDR { get; set; }
		public int MDR { get; set; }
		public int ACC { get; set; }
		public int EVA { get; set; }
		public int Exp { get; set; }

		public bool Boss { get; set; }
		public int SpawnType { get; set; }
		public int[] Revive { get; set; }
		public MobDataSkillTemplate[] Skill { get; set; }
		/// <summary>
		/// Buff given to players on mob death
		/// </summary>
		public int DeadBuff { get; set; }
		public int RemoveAfter { get; set; }
		public int HpTagColor { get; set; }
		public int HpTagBgColor { get; set; }
		public bool HPGaugeHide { get; set; }
		public bool Invincible { get; set; }
		public int FixedDamage { get; set; }

		public int FlySpeed { get; set; }
		public int Speed { get; set; }
		public int ChaseSpeed { get; set; }

		public MobMoveType MoveAbility { get; set; }

		public int tRegenInterval { get; set; }

		public bool DoNotRemove { get; set; }

		public int SelfDestructActionType { get; set; }
		public int SelfDestructRemoveAfter { get; set; }
		public bool CannotEvade { get; set; }

		public int[] DamagedElemAttr { get; set; }

		public MobTemplate(int templateId)
			: base(templateId)
		{
			Skill = new MobDataSkillTemplate[0];
			Revive = new int[0];
		}

		public sealed class MobDataSkillTemplate
		{
			public int Action { get; set; }
			public int EffectAfter { get; set; }
			public int Level { get; set; }
			public int Skill { get; set; }
		}
	}
}
