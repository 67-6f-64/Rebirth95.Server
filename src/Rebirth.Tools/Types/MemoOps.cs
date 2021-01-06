namespace Rebirth.Common.Types
{
    public enum MemoOps
    {
        MemoReq_Send = 0x0,
        MemoReq_Delete = 0x1,
        MemoReq_Load = 0x2,
        MemoRes_Load = 0x3,
        MemoRes_Send_Succeed = 0x4,
        MemoRes_Send_Warning = 0x5,
        MemoRes_Send_ConfirmOnline = 0x6,
        MemoNotify_Receive = 0x7,
    }
}
