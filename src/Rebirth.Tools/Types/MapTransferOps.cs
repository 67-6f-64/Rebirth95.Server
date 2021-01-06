namespace Rebirth.Common.Types
{
    public enum MapTransferReq
    {
        DeleteList = 0x0,
        RegisterList = 0x1,
    }

    public enum MapTransferRes
    {
        DeleteList = 0x2,
        RegisterList = 0x3,
        Use = 0x4,
        Unknown = 0x5,
        TargetNotExist = 0x6,
        TargetDied = 0x7,
        NotAllowed = 0x8,
        AlreadyInMap = 0x9,
        RegisterFail = 0xA,
        LevelLimit = 0xB,

        //5 | You cannot go to that place.
        //6 | %s is currently difficult to locate, so\r\nthe teleport will not take place.
        //7 | %s is currently difficult to locate, so\r\nthe teleport will not take place.
        //8 | You cannot go to that place.
        //9 | It's the map you're currently on.
        //A | This map is not available to enter for the list.
        //B | Users below level 7 are not allowed \r\nto go out from Maple Island.
    }
}
