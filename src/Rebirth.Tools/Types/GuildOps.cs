namespace Rebirth.Common.Types
{
    public enum GuildOps : byte
    {
        GuildReq_LoadGuild = 0x0,
        GuildReq_InputGuildName = 0x1,
        GuildReq_CheckGuildName = 0x2,
        GuildReq_CreateGuildAgree = 0x3,
        GuildReq_CreateNewGuild = 0x4,
        GuildReq_InviteGuild = 0x5,
        GuildReq_JoinGuild = 0x6,
        GuildReq_WithdrawGuild = 0x7,
        GuildReq_KickGuild = 0x8,
        GuildReq_RemoveGuild = 0x9,
        GuildReq_IncMaxMemberNum = 0xA,
        GuildReq_ChangeLevel = 0xB,
        GuildReq_ChangeJob = 0xC,
        GuildReq_SetGradeName = 0xD,
        GuildReq_SetMemberGrade = 0xE,
        GuildReq_SetMark = 0xF,
        GuildReq_SetNotice = 0x10,
        GuildReq_InputMark = 0x11,
        GuildReq_CheckQuestWaiting = 0x12,
        GuildReq_CheckQuestWaiting2 = 0x13,
        GuildReq_InsertQuestWaiting = 0x14,
        GuildReq_CancelQuestWaiting = 0x15,
        GuildReq_RemoveQuestCompleteGuild = 0x16,
        GuildReq_IncPoint = 0x17,
        GuildReq_IncCommitment = 0x18,
        GuildReq_SetQuestTime = 0x19,
        GuildReq_ShowGuildRanking = 0x1A,
        GuildReq_SetSkill = 0x1B,

        // CWvsContext::OnGuildResult
        GuildRes_LoadGuild_Done = 0x1C,

        GuildRes_CheckGuildName_Available = 0x1D,
        GuildRes_CheckGuildName_Unknown = 0x1F,

        GuildRes_CreateGuildAgree_Reply = 0x20,
        GuildRes_CreateNewGuild_Done = 0x22,
        GuildRes_JoinGuild_Done = 0x29,
        GuildRes_WithdrawGuild_Done = 0x2E,
        GuildRes_KickGuild_Done = 0x31,
        GuildRes_RemoveGuild_Done = 0x34,
        GuildRes_InviteGuild_Rejected = 0x39,
        GuildRes_IncMaxMemberNum_Done = 0x3C,
        GuildRes_ChangeLevelOrJob = 0x3E,
        GuildRes_NotifyLoginOrLogout = 0x3F,
        GuildRes_SetGradeName_Done = 0x40,
        GuildRes_SetMemberGrade_Done = 0x42,
        GuildRes_SetMemberCommitment_Done = 0x44,
        GuildRes_SetMark_Done = 0x45,
        GuildRes_SetNotice_Done = 0x47,

        GuildRes_InsertQuest = 0x48,
        GuildRes_NoticeQuestWaitingOrder = 0x49,
        GuildRes_SetGuildCanEnterQuest = 0x4A,

        GuildRes_IncPoint_Done = 0x4B,
        GuildRes_ShowGuildRanking = 0x4C,

        GuildRes_GuildQuest_NotEnoughUser = 0x4D,
        GuildRes_GuildQuest_RegisterDisconnected = 0x4E,
        GuildRes_GuildQuest_NoticeOrder = 0x4F,

        GuildRes_Authkey_Update = 0x50,
        GuildRes_SetSkill_Done = 0x51,
        GuildRes_ServerMsg = 0x52,

		// Errors
        GuildRes_CheckGuildName_AlreadyUsed = 0x1E, // empty packet
        GuildRes_CreateGuildAgree_Unknown = 0x21, // empty packet
        GuildRes_CreateNewGuild_AlreayJoined = 0x23, // empty packet
        GuildRes_CreateNewGuild_GuildNameAlreadyExist = 0x24,
        GuildRes_CreateNewGuild_Beginner = 0x25, // empty packet
        GuildRes_CreateNewGuild_Disagree = 0x26, // empty packet
        GuildRes_CreateNewGuild_NotFullParty = 0x27,
        GuildRes_CreateNewGuild_Unknown = 0x28, // empty packet

        GuildRes_JoinGuild_AlreadyJoined = 0x2A, // empty packet
        GuildRes_JoinGuild_AlreadyFull = 0x2B, // empty packet
        GuildRes_JoinGuild_UnknownUser = 0x2C, // empty packet
        GuildRes_JoinGuild_Unknown = 0x2D, // empty packet

        GuildRes_WithdrawGuild_NotJoined = 0x2F, // empty packet
        GuildRes_WithdrawGuild_Unknown = 0x30,

        GuildRes_KickGuild_NotJoined = 0x32, // empty packet
        GuildRes_KickGuild_Unknown = 0x33,

		GuildRes_RemoveGuild_NotExist = 0x35,
		GuildRes_RemoveGuild_Unknown = 0x36, // empty packet

		GuildRes_InviteGuild_BlockedUser = 0x37, // decode str name??
		GuildRes_InviteGuild_AlreadyInvited = 0x38, // empty packet

        GuildRes_AdminCannotCreate = 0x3A, // empty packet
        GuildRes_AdminCannotInvite = 0x3B,

        GuildRes_IncMaxMemberNum_Unknown = 0x3D, // empty packet

        GuildRes_SetGradeName_Unknown = 0x41,

        GuildRes_SetMemberGrade_Unknown = 0x43,

        GuildRes_SetMark_Unknown = 0x46,
    }
}
