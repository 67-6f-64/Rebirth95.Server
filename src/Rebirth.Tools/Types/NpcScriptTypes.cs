namespace Rebirth.Common.Types
{
	//I wanna prefix all these types with Npc but maybe not

	[System.Flags]
	public enum ScriptFlagType : byte
	{
		NoESC = 0x1,
		NPCReplacedByUser = 0x2,
		NPCReplacedByNPC = 0x4,
		FlipImage = 0x8
	}

	public enum ScriptMsgType
	{
		Say,
		SayImage,
		AskYesNo,
		AskText,
		AskNumber,
		AskMenu,
		AskQuiz,
		AskSpeedQuiz,
		AskAvatar,
		AskMembershopAvatar,
		AskPet,
		AskPetAll,
		// there is no 12
		AskYesNoQuest = 13, //Accept Decline ?
		AskBoxText = 14,
		AskSlideMenu = 15
	}

	public enum InitialQuizRes
	{
		Request = 0x0,
		Fail = 0x1,
	}

	// 0:      CScriptMan::OnSay(v2, v5, v6, v3, (char)iPacket);
	// 1:      CScriptMan::OnSayImage(v2, v5, v6, v3, (char)iPacket);
	// 2:      CScriptMan::OnAskYesNo(v2, v5, v6, v3, (char)iPacket, 0, 0);
	// 3:      CScriptMan::OnAskText(v2, v5, v6, v3, (char)iPacket);
	// 4:      CScriptMan::OnAskNumber(v2, v5, v6, v3, (char)iPacket);
	// 5:      CScriptMan::OnAskMenu(v2, v5, v6, v3, (char)iPacket);
	// 6:      CScriptMan::OnAskQuiz(v5, v6, v3);
	// 7:      CScriptMan::OnAskSpeedQuiz(v5, v6, v3);
	// 8:      CScriptMan::OnAskAvatar(v2, v5, v6, v3);
	// 9:      CScriptMan::OnAskMembershopAvatar(v2, v5, v6, v3);
	// 10:      CScriptMan::OnAskPet(v2, (ZRef<GW_ItemSlotPet> *)v2, v5, v6, v3);
	// 11:      CScriptMan::OnAskPetAll(v2, (ZRef<GW_ItemSlotPet> *)v2, v5, v6, v3);
	// 13:      CScriptMan::OnAskYesNo(v2, v5, v6, v3, (char)iPacket, 0, 1);
	// 14:      CScriptMan::OnAskBoxText(v2, v5, v6, v3, (char)iPacket);
	// 15:      CScriptMan::OnAskSlideMenu(v2, v3);
}