namespace Rebirth.Common.Types
{
    public enum TrunkOps
    {
        TrunkReq_Load = 0x0,
        TrunkReq_Save = 0x1,
        TrunkReq_Close = 0x2,
        TrunkReq_CheckSSN2 = 0x3,
        TrunkReq_GetItem = 0x4,
        TrunkReq_PutItem = 0x5,
        TrunkReq_SortItem = 0x6,
        TrunkReq_Money = 0x7,
        TrunkReq_CloseDialog = 0x8,
        TrunkRes_GetSuccess = 0x9,
        TrunkRes_GetUnknown = 0xA,
        TrunkRes_GetNoMoney = 0xB,
        TruncRes_GetHavingOnlyItem = 0xC,
        TrunkRes_PutSuccess = 0xD,
        TrunkRes_PutIncorrectRequest = 0xE,
        TrunkRes_SortItem = 0xF,
        TrunkRes_PutNoMoney = 0x10,
        TrunkRes_PutNoSpace = 0x11,
        TrunkRes_PutUnknown = 0x12,
        TrunkRes_MoneySuccess = 0x13,
        TrunkRes_MoneyUnknown = 0x14,
        TrunkRes_TrunkCheckSSN2 = 0x15,
        TrunkRes_OpenTrunkDlg = 0x16,
        TrunkRes_TradeBlocked = 0x17,
        TrunkRes_ServerMsg = 0x18,
    }
}
