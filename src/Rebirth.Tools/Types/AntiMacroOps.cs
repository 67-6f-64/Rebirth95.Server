namespace Rebirth.Common.Types
{
    public enum AntiMacroOps
    {
        Fail_InvalidCharacterName = 0x0,
        Fail_NotAttack = 0x1,
        Fail_NotAvailableTime = 0x2,
        Fail_SolvingQuestion = 0x3,
        Pended = 0x4,
        Success = 0x5,
        AntiMacroRes = 0x6,
        AntiMacroRes_Fail = 0x7,
        AntiMacroRes_TargetFail = 0x8,
        AntiMacroRes_Success = 0x9,
        AntiMacroRes_TargetSuccess = 0xA,
        AntiMacroRes_Reward = 0xB,
    }
}
