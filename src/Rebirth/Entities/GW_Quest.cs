namespace Rebirth.Entities
{
    public sealed class GW_QuestComplete
    {
        public int usQRKey { get; set; }
        public int tEnd { get; set; }
    }

    public sealed class GW_QuestCompleteNew
    {
        public int sQRKey { get; set; }
        public long ftFinishTime { get; set; }
    }

    public sealed class GW_QuestRecord 
    {
        public int usQRKey { get; set; }
        public string sQRValue { get; set; }
    }

    public sealed class GW_QuestRecordEx
    {
        public int usQRKey { get; set; }
        public string sQRValue { get; set; }
    }
}
