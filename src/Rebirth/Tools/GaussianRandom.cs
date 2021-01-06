using System;

namespace Rebirth.Tools
{
	public sealed class GaussianRandom
	{
		private bool _hasDeviate;
		private double _storedDeviate;
		private Random _random => Constants.Rand;

		/// <summary>
		/// Obtains normally (Gaussian) distributed random numbers, using the Box-Muller
		/// transformation.  This transformation takes two uniformly distributed deviates
		/// within the unit circle, and transforms them into two independently
		/// distributed normal deviates.
		/// </summary>
		/// <param name="mu">The mean of the distribution.  Default is zero.</param>
		/// <param name="sigma">The standard deviation of the distribution.  Default is one.</param>
		/// <returns></returns>
		public double NextGaussian(double mu = 0, double sigma = 1)
		{
			if (sigma <= 0)
				throw new ArgumentOutOfRangeException("sigma", "Must be greater than zero.");

			if (_hasDeviate)
			{
				_hasDeviate = false;
				return _storedDeviate * sigma + mu;
			}

			double v1, v2, rSquared;
			do
			{
				// two random values between -1.0 and 1.0
				v1 = 2 * _random.NextDouble() - 1;
				v2 = 2 * _random.NextDouble() - 1;
				rSquared = v1 * v1 + v2 * v2;
				// ensure within the unit circle
			} while (rSquared >= 1 || rSquared == 0);

			// calculate polar tranformation for each deviate
			var polar = Math.Sqrt(-2 * Math.Log(rSquared) / rSquared);
			// store first deviate
			_storedDeviate = v2 * polar;
			_hasDeviate = true;

			// return second deviate
			return v1 * polar * sigma + mu;
		}

		/// <summary>
		/// 68% of the time the final number will be between -nMaxRange and nMaxRange
		/// 95% of the time the final number will be between -2*nMaxRange and 2*nMaxRange
		/// 99.7% of the time the final number will be between -3*nMaxRange and 3*nMaxRange
		/// </summary>
		/// <param name="nDefaultValue"></param>
		/// <param name="nRange"></param>
		/// <param name="bUnsigned">False if return value can be larger or smaller than the input value</param>
		/// <returns></returns>
		public int GaussianDistributionVariation(int nDefaultValue, int nRange, bool bUnsigned)
			=> GaussianDistributionVariation(nRange, bUnsigned) + nDefaultValue;

		public int GaussianDistributionVariation(int nRange, bool bUnsigned)
		{
			var randVal = new GaussianRandom().NextGaussian();
			var mult = randVal * nRange;
			var floor = Math.Floor(mult);

			if (bUnsigned)
			{
				var abs = Math.Abs(floor);
				return (short)abs;
			}
			return (short)floor;
		}
	}
}
