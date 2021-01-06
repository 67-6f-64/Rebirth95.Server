namespace Rebirth.Common.Types
{
    public enum StoreBankOps
    {
        StoreBankReq_Load = 0x13,
        StoreBankReq_Remove = 0x14,
        StoreBankReq_CheckSSN2 = 0x15,

        StoreBankRes_Load_Done = 0x16,
        StoreBankRes_Load_Failed = 0x17,
        StoreBankRes_Block = 0x18,
        StoreBankRes_Remove_Done = 0x19,

        StoreBankReq_CalculateFee = 0x1A,
        StoreBankReq_GetAll = 0x1B,
        StoreBankReq_Exit = 0x1C,

        StoreBankRes_CalculateFee = 0x1D,
        StoreBankRes_GetAll_Done = 0x1E,
        StoreBankRes_GetAll_OverPrice = 0x1F,
        StoreBankRes_GetAll_OnlyItem = 0x20,
        StoreBankRes_GetAll_NoFee = 0x21,
        StoreBankRes_GetAll_NoSlot = 0x22,
        StoreBankRes_OpenStoreBankDlg = 0x23,
        StoreBankRes_StoreBankCalculateFee = 0x24,
        StoreBankRes_StoreBankLoadFailed = 0x25,
        StoreBankRes_StoreBankBlock = 0x26,
    }
}
