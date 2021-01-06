using System;
using Rebirth.Tools;

namespace Rebirth.Entities
{
    public class Foothold : IComparable
    {
        public short Id { get; set; }
        public byte Prev { get; set; }
        public byte Next { get; set; }
        public short X1 { get; set; }
        public short Y1 { get; set; }
        public short X2 { get; set; }
        public short Y2 { get; set; }

        public bool Wall => X1 == X2;
        public bool Flat => Y1 == Y2;

        public double Slope
            => Flat ? 0
            : MathHelper.GetSlope(
                new double[] { X1, X2 },
                new double[] { Y1, Y2 }
                );

        public int Length
            => (int)Math.Floor(MathHelper.SlopeDistance(X1, Y1, X2, Y2));

        public int CompareTo(object obj)
        {
            Foothold foothold = (Foothold)obj;

            if (Y2 < foothold.Y1)
            {
                return -1;
            }
            if (Y1 > foothold.Y2)
            {
                return 1;
            }

            return 0;
        }
    }
}