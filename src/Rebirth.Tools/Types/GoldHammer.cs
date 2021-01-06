namespace Rebirth.Common.Types
{
    public enum GoldHammer : byte
    {
        ReturnResult_ItemUpgradeSuccess = 0x3D,
        ReturnResult_ItemUpgradeDone = 0x41,
        ReturnResult_ItemUpgradeErr = 0x42,

        Result_Success = 0,
        Result_Fail = 1,
        Result_Done = 2,
        Result_Error = 3,
    }
}
