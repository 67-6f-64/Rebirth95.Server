using System;
using System.Collections.Generic;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;

namespace Rebirth.Game
{
	public class MobStat
	{
		public MobTemporaryStatCollection TempStats { get; set; } // TODO bump this class up into this class

		public DateTime tLastUpdateTime { get; set; }
		public DateTime tLastRecovery { get; set; }
		public DateTime tLastMove { get; set; }
		public DateTime tLastAttack { get; set; }
		public DateTime tCreateTime { get; }
		public DateTime tLastSkillUse { get; set; }
		public DateTime tLastSendMobHP { get; set; }

		public int HP { get; set; }
		public int MP { get; set; }

		public bool AffectedBySteal { get; set; }
		/// <summary>
		/// Increases meso and drop rate
		/// </summary>
		public int PickPocketRate { get; set; }
		/// <summary>
		/// increases exp, meso, and drop rate
		/// </summary>
		public int ShowdownRate { get; set; }
		/// <summary>
		/// Increases exp rate
		/// </summary>
		public int StinkBombRate { get; set; }

		public int nPAD { get; private set; }
		public int nPDD { get; private set; }
		private int _nPDR;
		public int nPDR
		{
			get => _nPDR;
			private set
			{
				_nPDR = value;
				if (_nPDR > 100) _nPDR = 100;
				else if (_nPDR < 0) _nPDR = 0;
			}
		}
		public int nMDD { get; private set; }

		private int _nMAD;
		public int nMAD
		{
			get => _nMAD;
			private set
			{
				_nMAD = value;
				if (_nMAD > 29999) _nMAD = 29999;
				else if (_nMAD < 0) _nMAD = 0;
			}
		}

		private int _nMDR;
		public int nMDR
		{
			get => _nMDR;
			private set
			{
				_nMDR = value;
				if (_nMDR > 100) _nMDR = 100;
				else if (_nMDR < 0) _nMDR = 0;
			}
		}

		private int _nACC;
		public int nACC
		{
			get => _nACC;
			private set
			{
				_nACC = value;
				if (_nACC > 9999) _nACC = 9999;
				else if (_nACC < 0) _nACC = 0;
			}
		}

		private int _nEVA;
		public int nEVA
		{
			get => _nEVA;
			private set
			{
				_nEVA = value;
				if (_nEVA > 9999) _nEVA = 9999;
				else if (_nEVA < 0) _nEVA = 0;
			}
		}

		public int nCounterProb { get; set; }
		public int nPCounter { get; set; }
		public int wPCounter { get; set; }
		public int nMCounter { get; set; }
		public int wMCounter { get; set; }

		public int rDamagedElemAttr { get; set; }
		public int[] aDamagedElemAttr { get; private set; }

		// placeholder until i move TempStats up into MobStat
		public MobStatEntry this[MobStatType nType] => TempStats[nType];

		public MobStat(CMob parent)
		{
			tLastUpdateTime = DateTime.Now;
			tLastRecovery = DateTime.Now;
			tLastMove = DateTime.Now;
			tLastAttack = DateTime.Now;
			tCreateTime = DateTime.Now;
			tLastSkillUse = DateTime.Now;
			tLastSkillUse = DateTime.Now;
			tLastSendMobHP = DateTime.Now;
			TempStats = new MobTemporaryStatCollection(parent);
			aDamagedElemAttr = new int[8];

			// has to be after tempstats has been initialized
			SetFrom(parent);
		}

		public void SetFrom(CMob mob)
		{
			nPAD = mob.Template.PAD + (TempStats[MobStatType.PAD]?.nOption ?? 0);
			nPDD = mob.Template.PDD;
			nPDR = mob.Template.PDR + (TempStats[MobStatType.PDR]?.nOption ?? 0);
			nMAD = mob.Template.MAD + (TempStats[MobStatType.MAD]?.nOption ?? 0);
			nMDD = mob.Template.MDD;
			nMDR = mob.Template.MDR + (TempStats[MobStatType.MDR]?.nOption ?? 0);
			nACC = mob.Template.ACC + (TempStats[MobStatType.ACC]?.nOption ?? 0);
			nEVA = mob.Template.EVA + (TempStats[MobStatType.EVA]?.nOption ?? 0);

			// TODO mirror client in how it determines this value (most recent one I think?)
			nCounterProb = Math.Max(TempStats[MobStatType.MCounter]?.nOption ?? 0,
									TempStats[MobStatType.PCounter]?.nOption ?? 0);

			rDamagedElemAttr = TempStats[MobStatType.DamagedElemAttr]?.rOption ?? 0;
		}

		public void AdjustDamagedElemAttr(int nSkillID, int[] aOriginalDamagedElemAttr)
		{
			int nIdx;
			switch ((Skills)nSkillID)
			{
				case Skills.ARCHMAGE1_FIRE_DEMON:
					nIdx = 2;
					break;
				case Skills.ARCHMAGE2_ICE_DEMON:
					nIdx = 1;
					break;
				default:
					return;
			}

			var nElement = aOriginalDamagedElemAttr[nIdx];
			if (nElement != 0)
			{
				nElement -= 1;
				if (nElement != 0)
				{
					if (nElement == 1)
					{
						aDamagedElemAttr[nIdx] = 0;
					}
				}
				else
				{
					aDamagedElemAttr[nIdx] = 2;
				}
			}
			else
			{
				aDamagedElemAttr[nIdx] = 3;
			}
		}

		public void ResetDamagedElemAttr(int[] aOriginalDamagedElemAttr)
		{
			for (var i = 0; i < aOriginalDamagedElemAttr.Length; i++)
			{
				aDamagedElemAttr[i] = aOriginalDamagedElemAttr[i];
			}
		}

		public void Update(CMob parent)
		{
			if (parent is null || parent.Dead) return;

			if (tLastUpdateTime.MillisSinceStart() < 1000) return;

			tLastUpdateTime = DateTime.Now;

			if (tLastRecovery.MillisSinceStart() >= 8000)
			{
				parent.Heal(parent.Template.HpRecovery, parent.Template.MpRecovery);
				tLastRecovery = DateTime.Now;
			}

			if (tLastMove.MillisSinceStart() > 5000)
			{
				parent.SetController(null, MobCtrlType.Active_Int);
				tLastAttack = DateTime.Now;
				tLastMove = DateTime.Now;
				return;
			}

			var toRemove = new List<MobStatEntry>();

			proc_dot(MobStatType.Poison);
			proc_dot(MobStatType.Venom);
			proc_dot(MobStatType.Ambush);
			proc_dot(MobStatType.Web);

			void proc_dot(MobStatType nType)
			{
				if (TempStats[(int)nType] is MobStatEntry dot)
				{
					var chr = parent.Field.Users[dot.CharIdFrom];
					if (chr is null || dot.nOption <= 0)
					{
						toRemove.Add(dot);
						TempStats.Remove(dot);
					}
					else if (HP < dot.nOption)
					{
						parent.Damage(chr, HP - 1, 0); // should not kill mob
						toRemove.Add(dot);
						TempStats.Remove(dot);
					}
					else
					{
						parent.Damage(chr, dot.nOption, 0);
					}
				}
			}

			foreach (var stat in TempStats)
			{
				if (stat.tStartTime.SecondsSinceStart() > stat.nDurationSeconds)
				{
					toRemove.Add(stat);
				}
			}

			if (toRemove.Count <= 0) return;

			TempStats.RemoveMobStats(toRemove.ToArray());
		}
	}
}
