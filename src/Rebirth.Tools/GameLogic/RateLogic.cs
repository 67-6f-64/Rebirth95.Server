using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Common.GameLogic
{
	public static class RateLogic
	{
		public static int NxGainFromMob(Random rand, int nMobLevel, int nMobHP, bool bBoss)
		{
			const int NX_BaseProc = 20;
			const double NX_LevelProcAdd = 0.125;
			const double NX_LowHpBossProcMultiplier = 1.5;
			const int NX_BossMinHP = 250_000;
			const int NX_MinMobLevel = 20;
			const double NX_BossHpDenominator = 250_000;
			const double NX_NonBossHpDenominator = 70_000;
			const double NX_VariancePerLevel = 0.2;
			const double NX_GainPerLevel = 0.45;
			const int NX_MaxGain = 14_500;

			if (nMobLevel < NX_MinMobLevel) return 0;

			var proc = (NX_LevelProcAdd * nMobLevel) + NX_BaseProc;
			var variance = nMobLevel * NX_VariancePerLevel;
			var gain = NX_GainPerLevel * nMobLevel;

			var multiplier = bBoss ? 3d : 1d;

			if (nMobHP > NX_BossHpDenominator)
			{
				multiplier += nMobHP / NX_BossHpDenominator;
			}

			if (proc < 100 && rand.Next(100) > proc) return 0;

			gain *= multiplier;

			gain -= variance * multiplier;

			if (gain < 0) return 0;

			if (gain > NX_MaxGain) gain = NX_MaxGain;

			return (int)gain;
		}
	}
}
