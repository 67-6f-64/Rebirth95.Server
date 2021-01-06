namespace Rebirth.Common.Types
{
    public enum GivePopularityRes : byte
    {
        Success = 0x0,
        InvalidCharacterID = 0x1,
        LevelLow = 0x2,
        AlreadyDoneToday = 0x3,
        AlreadyDoneTarget = 0x4,
        Notify = 0x5,

        UnknownError = 0xFF,
    }
}