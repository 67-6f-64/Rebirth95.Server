namespace Rebirth.Tools.Formulas
{
    public class RandomNX
    {
        public static int Gain(double amountModifier)
        {
			if (Constants.Rand.Next(100) > RateConstants.RandomNX_Odds)
				return 0;

			return new GaussianRandom().GaussianDistributionVariation(RateConstants.RandomNX_Mean, RateConstants.RandomNX_Variance, true);

            //var RandAmount = Constants.Rand.Next(RateConstants.RandomNX_LowerBound, RateConstants.RandomNX_UpperBound) * amountModifier;

            //var lbDbl = RateConstants.RandomNX_LowerBound / 100;
            //var ubDbl = RateConstants.RandomNX_UpperBound / 100;
            //var Odds = Constants.Rand.NextDouble() < Math.Abs((RandAmount / 100) - (lbDbl + ubDbl));

            //return (int)(Odds ? RandAmount : 0);
        }
    }
}