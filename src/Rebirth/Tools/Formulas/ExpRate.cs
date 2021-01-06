using System;

namespace Rebirth.Tools.Formulas
{
    public class ExpRate
    {
        public static double CalculatePartyBonusMultiplier(int leechableMembers, int bishopsInParty, int attackingMembers)
        {
            if (leechableMembers <= 1)
                return 1.0;

            var rate = 1.15 + (leechableMembers * 0.05);

            if (leechableMembers == bishopsInParty)
                bishopsInParty--;

            rate += Math.Min(bishopsInParty * 0.2, 0.6);
            rate += Math.Min(attackingMembers * 0.1, 0.2);
            return rate;
        }

        public static double CalculateLevelDiffModifier(int mobLevel, int charLevel)
        {
            const double CatDeterminant = 0.25;

            var levelDifference = Math.Abs(mobLevel - charLevel);
            var rateCategory = (int)Math.Ceiling(levelDifference * CatDeterminant);

			rateCategory = Math.Max(0, rateCategory);

            if (rateCategory <= 5)
            {
                float[] categoryRates =
                   {
                    1.15f, // 0
                    1.15f, // 1
                    1.0f, // 2
                    0.9f, // 3
                    0.8f, // 4
                    0.7f // 5
                };

                return categoryRates[rateCategory];
            }
            else
            {
                const double BaseReductionAt20 = 0.6;
                const double ReductionPerLvlAfter20 = 0.06;
                const double MinRate = 0.1;

                return Math.Max(BaseReductionAt20 - (ReductionPerLvlAfter20 * (levelDifference - 20)), MinRate); // diff > 19
            }
        }
    }
}
