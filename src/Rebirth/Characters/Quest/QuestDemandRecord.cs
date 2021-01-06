namespace Rebirth.Characters.Quest
{
    public class QuestDemandRecord
    {
        public QuestDemandType nType { get; set; }
        public int nKey { get; set; } // usually id
        public int nValue { get; set; } // usually count
    }
}
