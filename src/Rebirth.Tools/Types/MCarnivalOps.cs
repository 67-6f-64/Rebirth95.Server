namespace Rebirth.Common.Types
{
    public enum MCarnivalOps
    {
        MCarnivalReq_SummonMob = 0x0,
        MCarnivalRes_Success = 0x0,
        MCarnivalReq_UseSkill = 0x1,
        MCarnivalRes_Fail_CPLaking = 0x1,
        MCarnivalReq_SummonGuardian = 0x2,
        MCarnivalRes_Fail_MobOverflow = 0x2,
        MCarnivalRes_Fail_GuardianOverflow = 0x3,
        MCarnivalRes_Fail_GuardianAlreadySummoned = 0x4,
        MCarnivalRes_Fail_Unknown = 0x5,
        MCarnivalOut_PartyBoss = 0x6,
        MCarnivalOut_PartyMember = 0x7,
        MCarnivalGameRes_Win = 0x8,
        MCarnivalGameRes_Lose = 0x9,
        MCarnivalGameRes_Draw = 0x0A,
        MCarnivalGameRes_Cancel = 0x0B,
    }
}
