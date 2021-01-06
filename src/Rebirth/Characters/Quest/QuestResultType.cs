
namespace Rebirth.Characters.Quest
{
    public enum QuestResultType
    {
        Start_QuestTimer = 0x6,
        End_QuestTimer = 0x7,

        Start_TimeKeepQuestTimer = 0x8,
        End_TimeKeepQuestTimer = 0x9,

        Success = 0xA,

        Failed_Unknown = 0xB,
        Failed_Inventory = 0xC,
        Failed_Meso = 0xD,
        Failed_Pet = 0xE,
        Failed_Equipped = 0xF,
        Failed_OnlyItem = 0x10,
        Failed_TimeOver = 0x11,
        Reset_QuestTimer = 0x12,
    }
}
