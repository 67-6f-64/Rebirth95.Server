using System;
using log4net;
using Rebirth.Common.GameLogic;
using Rebirth.Server.Center.GameData.DropInfo;
using Rebirth.Server.Center.Template.DropInfo;

namespace Rebirth.Tools.Formulas
{
	// https://strategywiki.org/wiki/MapleStory/Formulas
	public class Rates
	{
		public static ILog Log = LogManager.GetLogger(typeof(Rates));

		public static int MakeMesoDropAmount(int mobLevel, int charLevel, double extraRateModifier)
		{
			var nDif = charLevel - mobLevel;
			var nFinalRate = (double)RateConstants.MesoRate;

			if (nDif > 10) // higher level than monster
			{
				if (nDif <= 20)
					nFinalRate -= nDif * 0.02;
				else
					nFinalRate -= nDif * 0.05;
			}
			else if (nDif < -10) // lower level than monster
			{
				if (nDif >= -20)
					nFinalRate -= -nDif * 0.03;
				else
					nFinalRate -= -nDif * 0.05;
			}

			nFinalRate = Math.Max(0, nFinalRate);

			var baseAmount = Constants.Rand.Next(RateConstants.BaseMinMesoAmount * mobLevel, RateConstants.BaseMaxMesoAmount * mobLevel);

			return (int)(baseAmount * nFinalRate * extraRateModifier) + 1;
		}

		public static bool WillDrop(double nextDouble, int mobLevel, int charLevel, short jobId, GlobalDropStruct drop, double extraRateModifier)
		{
			var levelDiff = charLevel - mobLevel;

			if (levelDiff > drop.MaxLevelDiff)
				return false;

			if (mobLevel > drop.MinMobLevel)
				return false;

			if (drop.Job != JobLogic.JobType.All && drop.Job != JobLogic.JobTypeFromID(jobId))
				return false;

			return nextDouble < (drop.DropRate * extraRateModifier);
		}
	}
}
