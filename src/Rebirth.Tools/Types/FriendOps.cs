namespace Rebirth.Common.Types
{
    public enum FriendOps
    {
		// internal code (i think)
        FriendReq_LoadFriend = 0x0,

		// these are sent from client
        FriendReq_SetFriend = 0x1,
        FriendReq_AcceptFriend = 0x2,
        FriendReq_DeleteFriend = 0x3,

		// these are internal codes -- never sent from client
        FriendReq_NotifyLogin = 0x4,
        FriendReq_NotifyLogout = 0x5,
        FriendReq_IncMaxCount = 0x6,

        FriendRes_LoadFriend_Done = 0x7, // CWvsContext::CFriend::Reset
        FriendRes_NotifyChange_FriendInfo = 0x8, // CWvsContext::CFriend::UpdateFriend
        FriendRes_Invite = 0x9,
        FriendRes_SetFriend_Done = 0xA, // CWvsContext::CFriend::Reset

		// all same handling
        FriendRes_SetFriend_FullMe = 0xB,
        FriendRes_SetFriend_FullOther = 0xC,
        FriendRes_SetFriend_AlreadySet = 0xD,
        FriendRes_SetFriend_Master = 0xE,
        FriendRes_SetFriend_UnknownUser = 0xF,

		// all same handling
        FriendRes_SetFriend_Unknown = 0x10,
        FriendRes_AcceptFriend_Unknown = 0x11,
		FriendRes_DeleteFriend_Unknown = 0x13,
		FriendRes_IncMaxCount_Unknown = 0x16,

		FriendRes_DeleteFriend_Done = 0x12, // CWvsContext::CFriend::Reset
        FriendRes_Notify = 0x14,
        FriendRes_IncMaxCount_Done = 0x15,
        FriendRes_PleaseWait = 0x17,
    }
}
