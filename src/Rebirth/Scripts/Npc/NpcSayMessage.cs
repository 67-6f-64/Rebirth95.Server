using Rebirth.Network;

namespace Rebirth.Scripts.Npc
{
    public struct NpcSayMessage
    {
        public string sText { get; set; }
        public bool bPrev { get; set; }
        public bool bNext { get; set; }

        public void Encode(COutPacket p)
        {
            p.EncodeString(sText);
            p.Encode1(bPrev);
            p.Encode1(bNext);
        }
    }
}
