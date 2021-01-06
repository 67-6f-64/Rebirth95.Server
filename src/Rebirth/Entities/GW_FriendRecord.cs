using Rebirth.Network;

namespace Rebirth.Entities
{
    public class GW_FriendRecord : AbstractRingRecord
    {
        public int dwPairCharacterID { get; set; }
        public string sPairCharacterName { get; set; }
        public long liSN { get; set; }
        public long liPairSN { get; set; }
        public int dwFriendItemID { get; set; }

        public void Encode(COutPacket p)
        {
            p.Encode4(dwPairCharacterID);
            p.EncodeStringFixed(sPairCharacterName, 13);
            p.Encode8(liSN);
            p.Encode8(liPairSN);
            p.Encode4(dwFriendItemID);
        }
    }
}
