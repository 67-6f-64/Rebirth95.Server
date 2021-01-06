namespace Rebirth.Common.Types
{
    public enum ClaimRes : byte
    {
       //Success = 0x1, | Start
       Success_Sender = 0x2,
       Success_Target = 0x3,

       //Fail = 0x40, | Start
       Fail_Unknown = 0x41,
       Fail_InvalidCharacter = 0x42,
       Fail_NotEnoughMoney = 0x43,
       Fail_NotConnected = 0x44,
       Fail_OverLimit = 0x45,
       Fail_OverWeeklyLimit = 0x46,
       Fail_UnAvailable = 0x47,
       Fail_Liar = 0x48,
    }
}
