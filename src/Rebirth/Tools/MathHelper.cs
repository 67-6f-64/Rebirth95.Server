using System;

namespace Rebirth.Tools
{
    public static class MathHelper
    {
        // https://en.wikipedia.org/wiki/Linear_regression
        // https://en.wikipedia.org/wiki/Ordinary_least_squares
        public static double GetSlope(double[] xArray, double[] yArray)
        {
            if (xArray == null)
                throw new ArgumentNullException("xArray");
            if (yArray == null)
                throw new ArgumentNullException("yArray");
            if (xArray.Length != yArray.Length)
                throw new ArgumentException("Array Length Mismatch");
            if (xArray.Length < 2)
                throw new ArgumentException("Arrays too short.");

            double n = xArray.Length;
            double sumxy = 0, sumx = 0, sumy = 0, sumx2 = 0;
            for (int i = 0; i < xArray.Length; i++)
            {
                sumxy += xArray[i] * yArray[i];
                sumx += xArray[i];
                sumy += yArray[i];
                sumx2 += xArray[i] * xArray[i];
            }
            return ((sumxy - sumx * sumy / n) / (sumx2 - sumx * sumx / n));
        }

        public static double SlopeDistance(int x1, int y1, int x2, int y2)
            => Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }
}
