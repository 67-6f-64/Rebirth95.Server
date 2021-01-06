namespace Rebirth.Entities
{
    public sealed class GW_CoupleRecord : AbstractRingRecord
    {
        public int dwPairCharacterID { get; set; }
        public string sPairCharacterName { get; set; }
        public long liSN { get; set; }
        public long liPairSN { get; set; }
    }
}
