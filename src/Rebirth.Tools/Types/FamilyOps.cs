namespace Rebirth.Common.Types
{
    public enum FamilyOps
    {
        FamilyRes_Success = 0x0,
        FamilyRes_Success_Unregister = 0x1,
        FamilyRes_Fail_CanNotRegister = 0x40,
        FamilyRes_Fail_WrongName = 0x41,
        FamilyRes_Fail_SameFamily = 0x42,
        FamilyRes_Fail_NotSameFamily = 0x43,
        FamilyRes_Fail_DifferentFamily = 0x44,
        FamilyRes_Fail_NotSameMap = 0x45,
        FamilyRes_Fail_AleadyHasParent = 0x46,
        FamilyRes_Fail_OverLevel = 0x47,
        FamilyRes_Fail_UnderLevel = 0x48,
        FamilyRes_Fail_AlreadyJoining = 0x49,
        FamilyRes_Fail_AlreadySummon = 0x4A,
        FamilyRes_Fail_Summon = 0x4B,
        FamilyRes_Fail_MaxDepth = 0x4C,
        FamilyRes_Fail_MinLevel = 0x4D,
        FamilyRes_Fail_ChildReqWorldTransferUser = 0x4E,
        FamilyRes_Fail_SelfReqWorldTransferUser = 0x4F,
        FamilyRes_Fail_NotEnoughMoney = 0x50,
        FamilyRes_Fail_NotEnoughParentMoney = 0x51,
        FamilyRes_Fail_CanNotTeleport_ByLevellimited = 0x52,
    }
}
