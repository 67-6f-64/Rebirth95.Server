namespace Rebirth.Common.Types
{
    public enum QuestOps
    {
        QuestReq_LostItem = 0x0,
        QuestReq_AcceptQuest = 0x1,
        QuestReq_CompleteQuest = 0x2,
        QuestReq_ResignQuest = 0x3,
        QuestReq_OpeningScript = 0x4,
        QuestReq_CompleteScript = 0x5,
        QuestRes_Start_QuestTimer = 0x6,
        QuestRes_End_QuestTimer = 0x7,
        QuestRes_Start_TimeKeepQuestTimer = 0x8,
        QuestRes_End_TimeKeepQuestTimer = 0x9,
        QuestRes_Act_Success = 0xA,
        QuestRes_Act_Failed_Unknown = 0xB,
        QuestRes_Act_Failed_Inventory = 0xC,
        QuestRes_Act_Failed_Meso = 0xD,
        QuestRes_Act_Failed_Pet = 0xE,
        QuestRes_Act_Failed_Equipped = 0xF,
        QuestRes_Act_Failed_OnlyItem = 0x10,
        QuestRes_Act_Failed_TimeOver = 0x11,
        QuestRes_Act_Reset_QuestTimer = 0x12,
    }
}
