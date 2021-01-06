using Rebirth.Network;

namespace Rebirth.Common.Tools
{
    public sealed class TagPoint
    {
        public short X { get; set; }
        public short Y { get; set; }
        
        public TagPoint() { }

        public TagPoint(short x, short y)
        {
            X = x;
            Y = y;
        }

        public void Encode(COutPacket p)
        {
            p.Encode2(X);
            p.Encode2(Y);
        }

        public void Decode(CInPacket p)
        {
            X = p.Decode2();
            Y = p.Decode2();
        }

        public TagPoint Clone() => new TagPoint(X, Y);
        public override string ToString() => $"[X {X}] [Y {Y}]";
    }
}
