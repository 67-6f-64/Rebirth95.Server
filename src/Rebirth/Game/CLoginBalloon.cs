using Rebirth.Network;

namespace Rebirth.Game
{
    public class CLoginBalloon
    {
        public short nX { get; set; }
        public short nY { get; set; }
        public string sMessage { get; set; }

        public void Encode(COutPacket p)
        {
            p.Encode2(nX);
            p.Encode2(nY);
            p.EncodeString(sMessage);
        }
    }
}
