namespace Rebirth.Common.Types
{
    public enum PartyOps
    {
        // ---------------------------------- incoming ops
        PartyReq_LoadParty = 0x0,
        CreateParty = 0x1,
        LeaveParty = 0x2,
        JoinParty = 0x3,
        InviteParty = 0x4,
        KickParty = 0x5,
        ChangePartyLeader = 0x6,

        // ---------------------------------- outgoing ops
        PartyRes_LoadParty_Done = 0x7,

        PartyRes_CreateNewParty_Done = 0x8,
        PartyRes_CreateNewParty_AlreayJoined = 0x9,
        PartyRes_CreateNewParty_Beginner = 0xA,
        PartyRes_CreateNewParty_Unknown = 0xB,

        PartyRes_WithdrawParty_Done = 0xC,
        PartyRes_WithdrawParty_NotJoined = 0xD,
        PartyRes_WithdrawParty_Unknown = 0xE,

        PartyRes_JoinParty_Done = 0xF,
        PartyRes_JoinParty_Done2 = 0x10,
        PartyRes_JoinParty_AlreadyJoined = 0x11,
        PartyRes_JoinParty_AlreadyFull = 0x12,
        PartyRes_JoinParty_OverDesiredSize = 0x13,
        PartyRes_JoinParty_UnknownUser = 0x14,
        PartyRes_JoinParty_Unknown = 0x15,

        PartyRes_InviteParty_Sent = 0x16,
        PartyRes_InviteParty_BlockedUser = 0x17,
        PartyRes_InviteParty_AlreadyInvited = 0x18,
        PartyRes_InviteParty_AlreadyInvitedByInviter = 0x19,

        PartyRes_InviteParty_Rejected = 0x1A,
        PartyRes_InviteParty_Accepted = 0x1B,

        PartyRes_KickParty_Done = 0x1C,
        PartyRes_KickParty_FieldLimit = 0x1D,
        PartyRes_KickParty_Unknown = 0x1E,

        PartyRes_ChangePartyBoss_Done = 0x1F,
        PartyRes_ChangePartyBoss_NotSameField = 0x20,
        PartyRes_ChangePartyBoss_NoMemberInSameField = 0x21,
        PartyRes_ChangePartyBoss_NotSameChannel = 0x22,
        PartyRes_ChangePartyBoss_Unknown = 0x23,

        PartyRes_AdminCannotCreate = 0x24,
        PartyRes_AdminCannotInvite = 0x25,

        PartyRes_UserMigration = 0x26,
        PartyRes_ChangeLevelOrJob = 0x27,
        PartyRes_SuccessToSelectPQReward = 0x28,
        PartyRes_FailToSelectPQReward = 0x29,
        PartyRes_ReceivePQReward = 0x2A,
        PartyRes_FailToRequestPQReward = 0x2B,
        PartyRes_CanNotInThisField = 0x2C,
        PartyRes_ServerMsg = 0x2D,
        PartyInfo_TownPortalChanged = 0x2E,
        PartyInfo_OpenGate = 0x2F,

        // ---------------------------------- end party

        ExpeditionReq_Load = 0x30,
        ExpeditionReq_CreateNew = 0x31,
        ExpeditionReq_Invite = 0x32,
        ExpeditionReq_ResponseInvite = 0x33,
        ExpeditionReq_Withdraw = 0x34,
        ExpeditionReq_Kick = 0x35,
        ExpeditionReq_ChangeMaster = 0x36,
        ExpeditionReq_ChangePartyBoss = 0x37,
        ExpeditionReq_RelocateMember = 0x38,
        ExpeditionNoti_Load_Done = 0x39,
        ExpeditionNoti_Load_Fail = 0x3A,
        ExpeditionNoti_CreateNew_Done = 0x3B,
        ExpeditionNoti_Join_Done = 0x3C,
        ExpeditionNoti_You_Joined = 0x3D,
        ExpeditionNoti_You_Joined2 = 0x3E,
        ExpeditionNoti_Join_Fail = 0x3F,
        ExpeditionNoti_Withdraw_Done = 0x40,
        ExpeditionNoti_You_Withdrew = 0x41,
        ExpeditionNoti_Kick_Done = 0x42,
        ExpeditionNoti_You_Kicked = 0x43,
        ExpeditionNoti_Removed = 0x44,
        ExpeditionNoti_MasterChanged = 0x45,
        ExpeditionNoti_Modified = 0x46,
        ExpeditionNoti_Modified2 = 0x47,
        ExpeditionNoti_Invite = 0x48,
        ExpeditionNoti_ResponseInvite = 0x49,
        AdverNoti_LoadDone = 0x4A,
        AdverNoti_Change = 0x4B,
        AdverNoti_Remove = 0x4C,
        AdverNoti_GetAll = 0x4D,
        AdverNoti_Apply = 0x4E,
        AdverNoti_ResultApply = 0x4F,
        AdverNoti_AddFail = 0x50,
        AdverReq_Add = 0x51,
        AdverReq_Remove = 0x52,
        AdverReq_GetAll = 0x53,
        AdverReq_RemoveUserFromNotiList = 0x54,
        AdverReq_Apply = 0x55,
        AdverReq_ResultApply = 0x56,
    }
}
