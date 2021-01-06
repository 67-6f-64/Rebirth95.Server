namespace Rebirth.Entities
{
   public class GW_MarriageRecord : AbstractRingRecord
    {
        public int dwMarriageNo { get; set; }
        public int dwGroomID { get; set; }
        public int dwBrideID { get; set; }
        public short usStatus { get; set; }
        public int nGroomItemID { get; set; }
        public int nBrideItemID { get; set; }
        public string sGroomName { get; set; }
        public string sBrideName { get; set; }
    }
}
