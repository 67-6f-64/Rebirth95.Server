namespace Rebirth.Characters.Stat
{
	public class SecondaryStatValuesEx
	{
		/// <summary>
		/// Crit %
		/// </summary>
		public int nCr { get; set; }
		/// <summary>
		/// Crit damage %
		/// </summary>
		public int nCDr { get; set; }
		/// <summary>
		/// Per 4 sec
		/// </summary>
		public int nRecoveryHP { get; set; }
		/// <summary>
		/// Per 4 sec
		/// </summary>
		public int nRecoveryMP { get; set; }
		/// <summary>
		/// Potion % effectiveness increase
		/// </summary>
		public int nRecoveryUP { get; set; }
		/// <summary>
		/// Hp gained on mob kill
		/// </summary>
		public int nHpOnKill { get; set; }
		/// <summary>
		/// HpOnKill proc rate
		/// </summary>
		public int nHpOnKillProp { get; set; }
		/// <summary>
		/// Mp gained on mob kill
		/// </summary>
		public int nMpOnKill { get; set; }
		/// <summary>
		/// MpOnKill proc rate
		/// </summary>
		public int nMpOnKillProp { get; set; }
		/// <summary>
		/// Target DEF % to ignore
		/// </summary>
		public int nIgnoreTargetDEF { get; set; }
		public int nHpOnHit { get; set; }
		public int nHpOnHitProp { get; set; }
		public int nMpOnHit { get; set; }
		public int nMpOnHitProp { get; set; }

		/// <summary>
		/// Monster status effect (defined in Skill.wz/AttackType)
		/// </summary>
		public int nAttackType { get; set; }
		/// <summary>
		/// Monster status effect level (defined in Skill.wz/AttackType)
		/// </summary>
		public int nAttackTypeLevel { get; set; }
		/// <summary>
		/// Monster status proc rate
		/// </summary>
		public int nAttackTypeProp { get; set; }

		/// <summary>
		/// Invincibility proc chance
		/// </summary>
		public int nInvincibleProp { get; set; }
		/// <summary>
		/// Invincibility duration
		/// </summary>
		public int nInvincibleTime { get; set; }
		/// <summary>
		/// Invincibility duration increase on mob hit
		/// </summary>
		public int nInvincibleIncTime { get; set; }

		public int nIgnoreDam { get; set; }
		public int nIgnoreDamProp { get; set; }

		public int nIgnoreDamR { get; set; }
		public int nIgnoreDamRProp { get; set; }

		public int nReflectDamR { get; set; }
		public int nReflectDamRProb { get; set; }

		public int nDamR { get; set; }
		public int nBossDamR { get; set; }

		public int nMesoProp { get; set; }
		public int nRewardProp { get; set; }

		public int nStealProp { get; set; }

		public int nReduceAbnormalStatus { get; set; }
		public int nReduceMpConR { get; set; }

		public int nIncAllSkill { get; set; }
	}
}