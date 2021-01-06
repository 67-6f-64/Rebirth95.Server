using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Autofac;
using Rebirth.Characters.Actions;
using Rebirth.Characters.Combat;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Quest;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.ActiveSkill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Client;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Entities;
using Rebirth.Entities.Item;
using Rebirth.Entities.Shop;
using Rebirth.Field;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.FieldPools;
using Rebirth.Field.FieldTypes;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Provider.Template.Item.Cash;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Provider.Template.Item.Etc;
using Rebirth.Provider.Template.Item.Install;
using Rebirth.Redis;
using Rebirth.Scripts;
using Rebirth.Server.Center;
using Rebirth.Tools;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Server.Game
{
	public class WvsGame : WvsServerBase<WvsGameClient>
	{
		//-----------------------------------------------------------------------------

		public byte ChannelId { get; }
		public CFieldMan CFieldMan { get; }
		public CTimer MapRecycler { get; }
		public AreaBossMan AreaBossManager { get; }

		//-----------------------------------------------------------------------------

		public WvsGame(WvsCenter parent, byte channel) : base($"WvsGame{channel}", Constants.GamePort + channel, parent)
		{
			ChannelId = channel;

			CFieldMan = new CFieldMan(this);
			AreaBossManager = new AreaBossMan();

			MapRecycler = CreateTimer();
			MapRecycler.Elapsed = CFieldMan.Update;
			MapRecycler.Interval = CFieldMan.UpdateIntervalMillis;
			MapRecycler.Start();

			PacketHandler.Add((short)RecvOps.CP_MigrateIn, Handle_MigrateIn, false);

			PacketHandler.Add((short)RecvOps.CP_UserTransferFieldRequest, MigrationAction.Field);
			PacketHandler.Add((short)RecvOps.CP_UserTransferChannelRequest, MigrationAction.Channel);
			PacketHandler.Add((short)RecvOps.CP_UserMigrateToCashShopRequest, MigrationAction.CashShop);
			PacketHandler.Add((short)RecvOps.CP_UserMove, Handle_UserMove);
			PacketHandler.Add((short)RecvOps.CP_UserSitRequest, Handle_UserSitRequest);
			PacketHandler.Add((short)RecvOps.CP_UserPortableChairSitRequest, Handle_UserPortableChairSitRequest);
			PacketHandler.Add((short)RecvOps.CP_UserMeleeAttack, CharacterCombat.Handle_MeleeAttack);
			PacketHandler.Add((short)RecvOps.CP_UserShootAttack, CharacterCombat.Handle_ShootAttack);
			PacketHandler.Add((short)RecvOps.CP_UserMagicAttack, CharacterCombat.Handle_MagicAttack);
			PacketHandler.Add((short)RecvOps.CP_UserBodyAttack, CharacterCombat.Handle_BodyAttack);
			PacketHandler.Add((short)RecvOps.CP_UserMovingShootAttackPrepare, CharacterCombat.Handle_MovingShootAttackPrepare);
			PacketHandler.Add((short)RecvOps.CP_UserHit, CharacterCombat.Handle_UserHit);
			PacketHandler.Add((short)RecvOps.CP_UserAttackUser, CharacterCombat.Handle_UserAttackUser);
			PacketHandler.Add((short)RecvOps.CP_UserChat, ChatAction.UserChat);
			PacketHandler.Add((short)RecvOps.CP_UserADBoardClose, Handle_UserADBoardClose);
			PacketHandler.Add((short)RecvOps.CP_UserEmotion, Handle_UserEmotion);
			PacketHandler.Add((short)RecvOps.CP_UserActivateEffectItem, Handle_UserActivateEffectItem);
			PacketHandler.Add((short)RecvOps.CP_UserUpgradeTombEffect, Handle_UserUpgradeTombEffect);
			//PacketHandler.Add((short)RecvOps.CP_UserHP, Handle_UserHP);
			//PacketHandler.Add((short)RecvOps.CP_Premium, Handle_Premium);
			PacketHandler.Add((short)RecvOps.CP_UserBanMapByMob, Handle_UserBanMapByMob);
			//PacketHandler.Add((short)RecvOps.CP_UserMonsterBookSetCover, Handle_UserMonsterBookSetCover);
			PacketHandler.Add((short)RecvOps.CP_UserSelectNpc, Handle_UserSelectNpc);
			PacketHandler.Add((short)RecvOps.CP_UserRemoteShopOpenRequest, Handle_UserRemoteShopOpenRequest);
			PacketHandler.Add((short)RecvOps.CP_UserScriptMessageAnswer, Handle_UserScriptMessageAnswer);
			PacketHandler.Add((short)RecvOps.CP_UserShopRequest, CShop.Handle_UserShopRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserTrunkRequest, Handle_UserTrunkRequest);
			PacketHandler.Add((short)RecvOps.CP_UserEntrustedShopRequest, CMiniRoomPool.EntrustedShopRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserStoreBankRequest, Handle_UserStoreBankRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserParcelRequest, Handle_UserParcelRequest);
			PacketHandler.Add((short)RecvOps.CP_UserEffectLocal, Handle_UserEffectLocal);
			//PacketHandler.Add((short)RecvOps.CP_ShopScannerRequest, Handle_ShopScannerRequest);
			//PacketHandler.Add((short)RecvOps.CP_ShopLinkRequest, Handle_ShopLinkRequest);
			//PacketHandler.Add((short)RecvOps.CP_AdminShopRequest, Handle_AdminShopRequest);
			PacketHandler.Add((short)RecvOps.CP_UserGatherItemRequest, Handle_UserGatherItemRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSortItemRequest, Handle_UserSortItemRequest);
			PacketHandler.Add((short)RecvOps.CP_UserChangeSlotPositionRequest, Handle_UserChangeSlotPositionRequest);
			PacketHandler.Add((short)RecvOps.CP_UserStatChangeItemUseRequest, Handle_UserStatChangeItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserStatChangeItemCancelRequest, Handle_UserStatChangeItemCancelRequest);
			PacketHandler.Add((short)RecvOps.CP_UserStatChangeByPortableChairRequest, Handle_UserStatChangeByPortableChairRequest);
			PacketHandler.Add((short)RecvOps.CP_UserMobSummonItemUseRequest, Handle_UserMobSummonItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserPetFoodItemUseRequest, Handle_UserPetFoodItemUseRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserTamingMobFoodItemUseRequest, Handle_UserTamingMobFoodItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserScriptItemUseRequest, Handle_UserScriptItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserConsumeCashItemUseRequest, Handle_UserConsumeCashItemUseRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserDestroyPetItemRequest, Handle_UserDestroyPetItemRequest);
			PacketHandler.Add((short)RecvOps.CP_UserBridleItemUseRequest, Handle_UserBridleItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSkillLearnItemUseRequest, CharacterSkills.Handle_UserSkillLearnItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSkillResetItemUseRequest, Handle_UserSkillResetItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserShopScannerItemUseRequest, Handle_UserShopScannerItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserMapTransferItemUseRequest, Handle_UserMapTransferItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserPortalScrollUseRequest, Handle_UserPortalScrollUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserUpgradeItemUseRequest, Handle_UserUpgradeItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserHyperUpgradeItemUseRequest, Handle_UserHyperUpgradeItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserItemOptionUpgradeItemUseRequest, Handle_UserItemOptionUpgradeItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserUIOpenItemUseRequest, Handle_UserUIOpenItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserItemReleaseRequest, Handle_UserItemReleaseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserAbilityUpRequest, Handle_UserAbilityUpRequest);
			PacketHandler.Add((short)RecvOps.CP_UserAbilityMassUpRequest, Handle_UserAbilityMassUpRequest);
			PacketHandler.Add((short)RecvOps.CP_UserChangeStatRequest, Handle_UserChangeStatRequest);
			PacketHandler.Add((short)RecvOps.CP_UserChangeStatRequestByItemOption, Handle_UserChangeStatRequestByItemOption);
			PacketHandler.Add((short)RecvOps.CP_UserSkillUpRequest, CharacterSkills.Handle_SkillUpRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSkillUseRequest, CharacterSkills.Handle_SkillUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSkillCancelRequest, Handle_SkillCancelRequest);
			PacketHandler.Add((short)RecvOps.CP_UserSkillPrepareRequest, ActiveSkill_Prepare.Handle);
			PacketHandler.Add((short)RecvOps.CP_UserDropMoneyRequest, Handle_UserDropMoneyRequest);
			PacketHandler.Add((short)RecvOps.CP_UserGivePopularityRequest, GiveFame.OnPacket);
			//PacketHandler.Add((short)RecvOps.CP_UserPartyRequest, Handle_UserPartyRequest);
			PacketHandler.Add((short)RecvOps.CP_UserCharacterInfoRequest, Handle_UserCharacterInfoRequest);
			PacketHandler.Add((short)RecvOps.CP_UserActivatePetRequest, Handle_UserActivatePetRequest);
			PacketHandler.Add((short)RecvOps.CP_UserTemporaryStatUpdateRequest, Handle_UserTemporaryStatUpdateRequest);
			PacketHandler.Add((short)RecvOps.CP_UserPortalScriptRequest, Handle_UserPortalScriptRequest);
			PacketHandler.Add((short)RecvOps.CP_UserPortalTeleportRequest, Handle_UserPortalTeleportRequest);
			PacketHandler.Add((short)RecvOps.CP_UserMapTransferRequest, Handle_UserMapTransferRequest);
			PacketHandler.Add((short)RecvOps.CP_UserAntiMacroItemUseRequest, Handle_UserAntiMacroItemUseRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserAntiMacroSkillUseRequest, Handle_UserAntiMacroSkillUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserAntiMacroQuestionResult, Handle_UserAntiMacroQuestionResult);
			PacketHandler.Add((short)RecvOps.CP_UserClaimRequest, Handle_UserClaimRequest);
			PacketHandler.Add((short)RecvOps.CP_UserQuestRequest, CharacterQuests.OnQuestRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserCalcDamageStatSetRequest, Handle_UserCalcDamageStatSetRequest);
			PacketHandler.Add((short)RecvOps.CP_UserThrowGrenade, CGrenade.Handle_UserThrowGrenade);
			PacketHandler.Add((short)RecvOps.CP_UserMacroSysDataModified, Handle_UserMacroSysDataModified);
			//PacketHandler.Add((short)RecvOps.CP_UserSelectNpcItemUseRequest, Handle_UserSelectNpcItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserLotteryItemUseRequest, Handle_UserLotteryItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserItemMakeRequest, ItemMaker.OnPacket);
			//PacketHandler.Add((short)RecvOps.CP_UserSueCharacterRequest, Handle_UserSueCharacterRequest);
			PacketHandler.Add((short)RecvOps.CP_UserUseGachaponBoxRequest, Handle_UserUseGachaponBoxRequest);
			PacketHandler.Add((short)RecvOps.CP_UserUseGachaponRemoteRequest, Handle_UserUseGachaponRemoteRequest);
			PacketHandler.Add((short)RecvOps.CP_UserUseWaterOfLife, Handle_UserUseWaterOfLife);
			//PacketHandler.Add((short)RecvOps.CP_UserRepairDurabilityAll, Handle_UserRepairDurabilityAll);
			//PacketHandler.Add((short)RecvOps.CP_UserRepairDurability, Handle_UserRepairDurability);
			//PacketHandler.Add((short)RecvOps.CP_UserQuestRecordSetState, Handle_UserQuestRecordSetState);
			//PacketHandler.Add((short)RecvOps.CP_UserClientTimerEndRequest, Handle_UserClientTimerEndRequest);
			PacketHandler.Add((short)RecvOps.CP_UserFollowCharacterRequest, Handle_UserFollowCharacterRequest);
			PacketHandler.Add((short)RecvOps.CP_UserFollowCharacterWithdraw, Handle_UserFollowCharacterWithdraw);
			//PacketHandler.Add((short)RecvOps.CP_UserSelectPQReward, Handle_UserSelectPQReward);
			//PacketHandler.Add((short)RecvOps.CP_UserRequestPQReward, Handle_UserRequestPQReward);
			PacketHandler.Add((short)RecvOps.CP_SetPassengerResult, Handle_SetPassengerResult);
			PacketHandler.Add((short)RecvOps.CP_BroadcastMsg, ChatAction.Handle_BroadcastMsg);
			PacketHandler.Add((short)RecvOps.CP_GroupMessage, ChatAction.Handle_GroupMessage);
			PacketHandler.Add((short)RecvOps.CP_Whisper, ChatAction.Handle_Whisper);
			PacketHandler.Add((short)RecvOps.CP_CoupleMessage, ChatAction.Handle_CoupleMessage);
			PacketHandler.Add((short)RecvOps.CP_Messenger, MasterManager.MessengerManager.OnPacket);
			PacketHandler.Add((short)RecvOps.CP_MiniRoom, CMiniRoomPool.OnPacket);
			PacketHandler.Add((short)RecvOps.CP_PartyRequest, MasterManager.PartyPool.OnRequestPacket);
			PacketHandler.Add((short)RecvOps.CP_PartyResult, MasterManager.PartyPool.OnResultPacket);
			PacketHandler.Add((short)RecvOps.CP_ExpeditionRequest, Handle_ExpeditionRequest);
			PacketHandler.Add((short)RecvOps.CP_PartyAdverRequest, Handle_PartyAdverRequest);
			PacketHandler.Add((short)RecvOps.CP_GuildRequest, MasterManager.GuildManager.OnRequestPacket);
			PacketHandler.Add((short)RecvOps.CP_GuildResult, MasterManager.GuildManager.OnResultPacket);
			PacketHandler.Add((short)RecvOps.CP_Admin, Handle_Admin);
			PacketHandler.Add((short)RecvOps.CP_Log, Handle_Log);
			PacketHandler.Add((short)RecvOps.CP_FriendRequest, Handle_FriendRequest);
			//PacketHandler.Add((short)RecvOps.CP_MemoRequest, Handle_MemoRequest);
			//PacketHandler.Add((short)RecvOps.CP_MemoFlagRequest, Handle_MemoFlagRequest);
			PacketHandler.Add((short)RecvOps.CP_EnterTownPortalRequest, Handle_EnterTownPortalRequest);
			PacketHandler.Add((short)RecvOps.CP_EnterOpenGateRequest, Handle_EnterOpenGateRequest);
			//PacketHandler.Add((short)RecvOps.CP_SlideRequest, Handle_SlideRequest);
			PacketHandler.Add((short)RecvOps.CP_FuncKeyMappedModified, Handle_FuncKeyMappedModified);
			//PacketHandler.Add((short)RecvOps.CP_RPSGame, Handle_RPSGame);
			PacketHandler.Add((short)RecvOps.CP_MarriageRequest, Handle_MarriageRequest);
			//PacketHandler.Add((short)RecvOps.CP_WeddingWishListRequest, Handle_WeddingWishListRequest);
			//PacketHandler.Add((short)RecvOps.CP_WeddingProgress, Handle_WeddingProgress);
			//PacketHandler.Add((short)RecvOps.CP_GuestBless, Handle_GuestBless);
			PacketHandler.Add((short)RecvOps.CP_BoobyTrapAlert, Handle_BoobyTrapAlert);
			PacketHandler.Add((short)RecvOps.CP_StalkBegin, Handle_StalkBegin);
			PacketHandler.Add((short)RecvOps.CP_AllianceRequest, Handle_AllianceRequest);
			//PacketHandler.Add((short)RecvOps.CP_AllianceResult, Handle_AllianceResult);
			PacketHandler.Add((short)RecvOps.CP_FamilyChartRequest, Handle_FamilyChartRequest);
			PacketHandler.Add((short)RecvOps.CP_FamilyInfoRequest, Handle_FamilyInfoRequest);
			PacketHandler.Add((short)RecvOps.CP_FamilyRegisterJunior, Handle_FamilyRegisterJunior);
			//PacketHandler.Add((short)RecvOps.CP_FamilyUnregisterJunior, Handle_FamilyUnregisterJunior);
			//PacketHandler.Add((short)RecvOps.CP_FamilyUnregisterParent, Handle_FamilyUnregisterParent);
			//PacketHandler.Add((short)RecvOps.CP_FamilyJoinResult, Handle_FamilyJoinResult);
			//PacketHandler.Add((short)RecvOps.CP_FamilyUsePrivilege, Handle_FamilyUsePrivilege);
			PacketHandler.Add((short)RecvOps.CP_FamilySetPrecept, Handle_FamilySetPrecept);
			//PacketHandler.Add((short)RecvOps.CP_FamilySummonResult, Handle_FamilySummonResult);
			//PacketHandler.Add((short)RecvOps.CP_ChatBlockUserReq, Handle_ChatBlockUserReq);
			PacketHandler.Add((short)RecvOps.CP_GuildBBS, Handle_GuildBBS);
			PacketHandler.Add((short)RecvOps.CP_UserMigrateToITCRequest, MigrationAction.ITC);
			PacketHandler.Add((short)RecvOps.CP_UserExpUpItemUseRequest, Handle_UserExpUpItemUseRequest);
			PacketHandler.Add((short)RecvOps.CP_UserTempExpUseRequest, Handle_UserTempExpUseRequest);
			PacketHandler.Add((short)RecvOps.CP_NewYearCardRequest, Handle_NewYearCardRequest);
			PacketHandler.Add((short)RecvOps.CP_RandomMorphRequest, Handle_RandomMorphRequest);
			//PacketHandler.Add((short)RecvOps.CP_CashItemGachaponRequest, Handle_CashItemGachaponRequest);
			//PacketHandler.Add((short)RecvOps.CP_CashGachaponOpenRequest, Handle_CashGachaponOpenRequest);
			//PacketHandler.Add((short)RecvOps.CP_ChangeMaplePointRequest, Handle_ChangeMaplePointRequest);
			PacketHandler.Add((short)RecvOps.CP_TalkToTutor, Handle_TalkToTutor);
			PacketHandler.Add((short)RecvOps.CP_RequestIncCombo, CharacterCombat.Handle_RequestIncCombo);
			PacketHandler.Add((short)RecvOps.CP_MobCrcKeyChangedReply, Handle_MobCrcKeyChangedReply);
			PacketHandler.Add((short)RecvOps.CP_RequestSessionValue, Handle_RequestSessionValue);
			PacketHandler.Add((short)RecvOps.CP_UpdateGMBoard, Handle_UpdateGMBoard);
			PacketHandler.Add((short)RecvOps.CP_AccountMoreInfo, Handle_AccountMoreInfo);
			PacketHandler.Add((short)RecvOps.CP_FindFriend, Handle_FindFriend);
			PacketHandler.Add((short)RecvOps.CP_AcceptAPSPEvent, Handle_AcceptAPSPEvent);
			PacketHandler.Add((short)RecvOps.CP_UserDragonBallBoxRequest, Handle_UserDragonBallBoxRequest);
			PacketHandler.Add((short)RecvOps.CP_UserDragonBallSummonRequest, Handle_UserDragonBallSummonRequest);

			//TODO: Character.Pets.OnPacket
			PacketHandler.Add((short)RecvOps.CP_PetMove, Handle_PetMove);
			PacketHandler.Add((short)RecvOps.CP_PetActionCommand, Handle_PetAction);
			PacketHandler.Add((short)RecvOps.CP_PetAction, Handle_PetAction);
			//PacketHandler.Add((short)RecvOps.CP_PetInteractionRequest, Handle_PetInteractionRequest);
			PacketHandler.Add((short)RecvOps.CP_PetDropPickUpRequest, Handle_PetDropPickUpRequest);
			PacketHandler.Add((short)RecvOps.CP_PetStatChangeItemUseRequest, Handle_PetStatChangeItemUseRequest);
			//PacketHandler.Add((short)RecvOps.CP_PetUpdateExceptionListRequest, Handle_PetUpdateExceptionListRequest);

			PacketHandler.Add((short)RecvOps.CP_SummonedMove, CSummonedPool.Handle_SummonedMove);
			PacketHandler.Add((short)RecvOps.CP_SummonedAttack, CSummonedPool.Handle_SummonedAttack);
			PacketHandler.Add((short)RecvOps.CP_SummonedHit, CSummonedPool.Handle_SummonedHit);
			PacketHandler.Add((short)RecvOps.CP_SummonedSkill, CSummonedPool.Handle_SummonedSkill);
			PacketHandler.Add((short)RecvOps.CP_Remove, CSummonedPool.Handle_Remove);
			PacketHandler.Add((short)RecvOps.CP_DragonMove, Handle_DragonMove);
			PacketHandler.Add((short)RecvOps.CP_QuickslotKeyMappedModified, Handle_QuickslotKeyMappedModified);
			PacketHandler.Add((short)RecvOps.CP_PassiveskillInfoUpdate, CharacterBuffs.Handle_PassiveskillInfoUpdate);
			PacketHandler.Add((short)RecvOps.CP_UpdateScreenSetting, Handle_UpdateScreenSetting);
			PacketHandler.Add((short)RecvOps.CP_UserAttackUser_Specific, CharacterCombat.Handle_UserAttackUser_Specific);
			//PacketHandler.Add((short)RecvOps.CP_UserPamsSongUseRequest, Handle_UserPamsSongUseRequest);
			//PacketHandler.Add((short)RecvOps.CP_QuestGuideRequest, Handle_QuestGuideRequest);
			//PacketHandler.Add((short)RecvOps.CP_UserRepeatEffectRemove, Handle_UserRepeatEffectRemove);
			PacketHandler.Add((short)RecvOps.CP_MobMove, Handle_MobMove);
			PacketHandler.Add((short)RecvOps.CP_MobApplyCtrl, Handle_MobApplyCtrl); // CMob::OnCtrlAck
			PacketHandler.Add((short)RecvOps.CP_MobDropPickUpRequest, Handle_MobDropPickUpRequest);
			PacketHandler.Add((short)RecvOps.CP_MobHitByObstacle, Handle_MobHitByObstacle);
			PacketHandler.Add((short)RecvOps.CP_MobHitByMob, Handle_MobHitByMob);
			PacketHandler.Add((short)RecvOps.CP_MobSelfDestruct, Handle_MobSelfDestruct);
			PacketHandler.Add((short)RecvOps.CP_MobAttackMob, CharacterCombat.Handle_MobAttackMob);
			PacketHandler.Add((short)RecvOps.CP_MobSkillDelayEnd, Handle_MobSkillDelayEnd);
			PacketHandler.Add((short)RecvOps.CP_MobTimeBombEnd, Handle_MobTimeBombEnd);
			PacketHandler.Add((short)RecvOps.CP_MobEscortCollision, Handle_MobEscortCollision);
			PacketHandler.Add((short)RecvOps.CP_MobRequestEscortInfo, Handle_MobRequestEscortInfo);
			PacketHandler.Add((short)RecvOps.CP_MobEscortStopEndRequest, Handle_MobEscortStopEndRequest);
			PacketHandler.Add((short)RecvOps.CP_NpcMove, Handle_NpcMove);
			PacketHandler.Add((short)RecvOps.CP_NpcSpecialAction, Handle_NpcSpecialAction);
			PacketHandler.Add((short)RecvOps.CP_DropPickUpRequest, Handle_DropPickUpRequest);
			PacketHandler.Add((short)RecvOps.CP_ReactorHit, Handle_ReactorHit);
			PacketHandler.Add((short)RecvOps.CP_ReactorTouch, Handle_ReactorTouch);
			PacketHandler.Add((short)RecvOps.CP_RequireFieldObstacleStatus, Handle_RequireFieldObstacleStatus);
			PacketHandler.Add((short)RecvOps.CP_EventStart, Handle_EventStart);
			PacketHandler.Add((short)RecvOps.CP_SnowBallHit, Handle_SnowBallHit);
			PacketHandler.Add((short)RecvOps.CP_SnowBallTouch, Handle_SnowBallTouch);
			//PacketHandler.Add((short)RecvOps.CP_CoconutHit, Handle_CoconutHit);
			//PacketHandler.Add((short)RecvOps.CP_TournamentMatchTable, Handle_TournamentMatchTable);
			PacketHandler.Add((short)RecvOps.CP_PulleyHit, Handle_PulleyHit);
			//PacketHandler.Add((short)RecvOps.CP_MCarnivalRequest, Handle_MCarnivalRequest);
			//PacketHandler.Add((short)RecvOps.CP_CONTISTATE, Handle_CONTISTATE);
			PacketHandler.Add((short)RecvOps.CP_INVITE_PARTY_MATCH, Handle_INVITE_PARTY_MATCH);
			PacketHandler.Add((short)RecvOps.CP_CANCEL_INVITE_PARTY_MATCH, Handle_CANCEL_INVITE_PARTY_MATCH);
			PacketHandler.Add((short)RecvOps.CP_RequestFootHoldInfo, Handle_RequestFootHoldInfo);
			PacketHandler.Add((short)RecvOps.CP_FootHoldInfo, Handle_FootHoldInfo);
			//PacketHandler.Add((short)RecvOps.CP_CashShopGiftMateInfoRequest, Handle_CashShopGiftMateInfoRequest);
			//PacketHandler.Add((short)RecvOps.CP_CheckSSN2OnCreateNewCharacter, Handle_CheckSSN2OnCreateNewCharacter);
			//PacketHandler.Add((short)RecvOps.CP_CheckSPWOnCreateNewCharacter, Handle_CheckSPWOnCreateNewCharacter);
			//PacketHandler.Add((short)RecvOps.CP_FirstSSNOnCreateNewCharacter, Handle_FirstSSNOnCreateNewCharacter);
			PacketHandler.Add((short)RecvOps.CP_RaiseRefesh, Handle_RaiseRefesh);
			PacketHandler.Add((short)RecvOps.CP_RaiseUIState, Handle_RaiseUIState);
			PacketHandler.Add((short)RecvOps.CP_RaiseIncExp, Handle_RaiseIncExp);
			PacketHandler.Add((short)RecvOps.CP_RaiseAddPiece, Handle_RaiseAddPiece);
			//PacketHandler.Add((short)RecvOps.CP_SendMateMail, Handle_SendMateMail);
			//PacketHandler.Add((short)RecvOps.CP_RequestGuildBoardAuthKey, Handle_RequestGuildBoardAuthKey);
			//PacketHandler.Add((short)RecvOps.CP_RequestConsultAuthKey, Handle_RequestConsultAuthKey);
			PacketHandler.Add((short)RecvOps.CP_RequestClassCompetitionAuthKey, Handle_RequestClassCompetitionAuthKey);
			//PacketHandler.Add((short)RecvOps.CP_RequestWebBoardAuthKey, Handle_RequestWebBoardAuthKey);
			//PacketHandler.Add((short)RecvOps.CP_GoldHammerRequest, Handle_GoldHammerRequest);
			//PacketHandler.Add((short)RecvOps.CP_GoldHammerComplete, Handle_GoldHammerComplete);
			PacketHandler.Add((short)RecvOps.CP_ItemUpgradeComplete, Handle_ItemUpgradeComplete);
			//PacketHandler.Add((short)RecvOps.CP_BATTLERECORD_ONOFF_REQUEST, Handle_BATTLERECORD_ONOFF_REQUEST);
			PacketHandler.Add((short)RecvOps.CP_MapleTVSendMessageRequest, Handle_MapleTVSendMessageRequest);
			PacketHandler.Add((short)RecvOps.CP_MapleTVUpdateViewCount, Handle_MapleTVUpdateViewCount);
			//PacketHandler.Add((short)RecvOps.CP_ITCChargeParamRequest, Handle_ITCChargeParamRequest);
			//PacketHandler.Add((short)RecvOps.CP_ITCQueryCashRequest, Handle_ITCQueryCashRequest);
			//PacketHandler.Add((short)RecvOps.CP_ITCItemRequest, Handle_ITCItemRequest);
			//PacketHandler.Add((short)RecvOps.CP_CheckDuplicatedIDInCS, Handle_CheckDuplicatedIDInCS);
			PacketHandler.Add((short)RecvOps.CP_LogoutGiftSelect, Handle_LogoutGiftSelect);
		}

		//-----------------------------------------------------------------------------

		protected override WvsGameClient CreateClient(Socket socket)
		{
			return new WvsGameClient(this, socket)
			{
				ChannelId = ChannelId
			};
		}

		protected override void HandleDisconnect(WvsGameClient client)
		{
			base.HandleDisconnect(client);

			client.MigratedIn = false;

			if (!client.LoggedIn) return;

			var pChar = client.Character;

			pChar.CurMiniRoom?.HandlePlayerExit(client.Character, MR_LeaveResult.UserRequest);

			var storage = ServerApp.Container.Resolve<CenterStorage>();

			// migratory functions handle saving (cc, cs, etc)
			// we only want to save on dc/exit
			try
			{
				pChar.StatisticsTracker.nSecondsOnline += pChar.Stats.tLastLogin.SecondsSinceStart();
				pChar.Save();

				if (!storage.IsCharacterMigrate(pChar.dwId))
				{
					MasterManager.MessengerManager.GetByCharId(pChar.dwId)?.Leave(pChar.dwId);

					storage.RemoveAccountOnline(pChar.Account.ID);
					storage.RemoveCharacterOnline(pChar.dwId);

					pChar.NotifySocialChannels(SocialNotiflag.LogOut);

					pChar.Field.OnUserLeave(client.Character, true);

					// removing object from pool is the last thing we do to ensure no null pointer errors
					var removed = MasterManager.CharacterPool.Remove(pChar);
				}
				else
				{
					client.Character.Buffs.SaveForMigrate();
					client.Character.Cooldowns.SaveForMigrate();

					// cant modify this directly, need to use Field.OnUserLeave()
					// pChar.Field.Users.Remove(client.Character);

					pChar.Field.OnUserLeave(client.Character, true);
				}
			}
			catch (Exception ex)
			{
				Log.Info(ex.ToString());
				pChar.Field?.OnUserLeave(client.Character, true);
				storage.RemoveAccountOnline(pChar.Account.ID);
				storage.RemoveCharacterOnline(pChar.dwId);
				MasterManager.CharacterPool.Remove(pChar);
			}
		}


		//-----------------------------------------------------------------------------

		private void Handle_MigrateIn(WvsGameClient pClient, CInPacket p)
		{
			//[Custom Packet]
			var characterId = p.Decode4();
			var sessionId = p.Decode8(); //SessionId
			var bAdminClient = p.Decode1();

			//TODO: A state for sesssion id
			//TODO: Logic where people CC, connect to another channel, login
			//And the old session is still active... Two loaded players no thanks...

			//Fetch character accountId or encode it inside SessionId

			var storage = ServerApp.Container.Resolve<CenterStorage>();

			var charOnline = storage.IsCharacterOnline(characterId);
			var charMigrate = storage.IsCharacterMigrate(characterId);
			var charCSITC = storage.IsCharacterCSITC(characterId);

			if (!charMigrate || charCSITC) // char is removed from CS storage when it leaves CS
			{
				pClient.Disconnect();
				return;
			}

			storage.RemoveCharacterMigrate(characterId);

			// check if player is already online
			if (MasterManager.CharacterPool.Get(characterId)?.Field != null)
			{
				pClient.Disconnect();
				return;
			}

			pClient.Load(characterId);
			var pChar = pClient.Character;

			MasterManager.SummonStorage.Retrieve(pClient.Character.dwId); // delete stored summons

			if (!storage.IsAccountOnline(pChar.Account.ID))
			{
				pClient.Disconnect();
				return;
			}

			if (pChar.Stats.nHP <= 0)
			{
				pChar.Stats.nHP = 1;
				pChar.Stats.nMP = 1;
				var forcedReturnMap = MasterManager.MapTemplates[pChar.Stats.dwPosMap]?.ReturnMap ?? 100000000;
				pChar.Stats.dwPosMap = forcedReturnMap;
			}
			else
			{
				var map = pChar.Stats.dwPosMap;

				var curMap = MasterManager.MapTemplates[map];

				if (curMap != null && curMap.ForcedReturn > 0 && curMap.ForcedReturn != 999999999)
				{
					pChar.Stats.dwPosMap = curMap.ForcedReturn;
				}
			}

			// remove expired/removed pets - happens if pet is put back into the cash locker
			// has to be before SetField
			for (byte i = 0; i < pChar.Stats.aliPetLockerSN.Length; i++)
			{
				var petSN = pChar.Stats.aliPetLockerSN[i];
				if (petSN == 0) continue;

				if (pChar.InventoryCash.FindItemSlotByCashSN(petSN) != 0) continue;

				pChar.Stats.aliPetLockerSN[i] = 0;

				// no need to iterate CharacterPets cuz its all null on login
			}

			pChar.Action.SetField(pChar.Stats.dwPosMap);

			// has to be after SetField else char will DC
			for (var i = 0; i < 3; i++)
			{
				var petSN = pChar.Stats.aliPetLockerSN[i];

				if (petSN == 0) continue;

				var petSlot = pChar.InventoryCash.FindItemSlotByCashSN(petSN);

				if (petSlot == 0) continue;

				pChar.Pets.ActivateSinglePet(petSlot);
			}

			// TODO Move channel out of character and to socket probably
			pChar.Stats.Channel = pChar.Socket.ChannelId;
			pChar.Stats.tLastLogin = DateTime.Now;

			if (!charOnline)
			{
				storage.AddCharacterOnline(characterId);

				var szMessage = $"[Server] {pChar.Stats.sCharacterName} has logged in.";
				MasterManager.CharacterPool.Broadcast(CPacket.SystemMessage(szMessage));

				Discord.PostMessage(szMessage);
			}

			pChar.Friends.SendLoad();
			pChar.Macros.SendLoad();

			// has to be after pets spawn to map
			pChar.FuncKeys.SendLoad();

			//LP_HourChanged 
			//LP_ForcedStatReset
			//LP_FamilyPrivilegeList
			pClient.SendPacket(CPacket.ClaimSvrStatusChanged(true));
			pClient.SendPacket(CPacket.SetGender(pClient.Character.Stats.nGender));

			//pClient.SendPacket(CPacket.BroadcastServerMsg(Constants.ServerMessage));
			pClient.SendPacket(CPacket.BroadcastPinkNotice(Constants.ServerMessage));

			if (pChar.TamingMob.TamingMobLevel > 0)
			{
				pChar.SendPacket(pChar.TamingMob.SetTamingMobInfo(false));
			}

			if (pChar.nLinkedCharacterLevel > 0)
			{
				pChar.Modify.Skills(mod => mod.AddEntry(SkillLogic.get_novice_skill_as_race(SkillLogic.NoviceSkillID.BlessingOfTheFairy, pChar.Stats.nJob),
					entry => entry.nSLV = (byte)Math.Floor(pChar.nLinkedCharacterLevel * 0.1)));
			}

			pChar.SendPacket(pChar.WildHunterInfo.WildHunterInfo()); // yes i know this should be encoded in setfield

			foreach (var npc in NpcConstants.ScriptedNPCs)
			{
				pChar.SendPacket(CPacket.SetNpcScript(npc, "Script"));
			}

			if (!charOnline)
			{
				pChar.NotifySocialChannels(SocialNotiflag.LogIn);
			}
			else
			{
				pChar.NotifySocialChannels(SocialNotiflag.ChangeChannel);
			}

			pChar.RemoveExpiredItems(true);

			pChar.ValidateStat(true);
		}

		//-----------------------------------------------------------------------------

		private void Handle_UserMove(WvsGameClient c, CInPacket p)
		{
			if (c.Character.nActivePortableChairID > 0)
			{
				c.Character.nActivePortableChairID = 0;
				c.Character.Field.Broadcast(CPacket.UserSetActivePortableChair(c.Character.dwId, 0), c); //CUser::SendPortableChairEffect
			}

			var v1 = p.Decode8();
			var portalCount = p.Decode1(); //CField::GetFieldKey(v20);
			var v2 = p.Decode8();
			var mapCrc = p.Decode4();
			var dwKey = p.Decode4();
			var dwKeyCrc = p.Decode4();

			c.Character.Move(p); // much cleaner :)
		}

		private void Handle_UserSitRequest(WvsGameClient c, CInPacket p)
		{
			var nSeatId = p.Decode2();

			if (c.Character.Field is null) return;

			if (nSeatId >= 0) //Sitting on a chair in map
			{
				//if (CField::OnSitRequest(v2->m_pField, v2->m_dwCharacterID, v3))
				//{
				c.Character.SendPacket(CPacket.UserSitResult(nSeatId));
				//}
			}
			else if (c.Character.nActivePortableChairID > 0)
			{
				c.Character.nActivePortableChairID = 0;
				c.Character.Field.Broadcast(CPacket.UserSetActivePortableChair(c.Character.dwId, 0), c);
			}
			else
			{
				c.Character.SendPacket(CPacket.UserSitResult(-1));
			}

			c.SendPacket(CPacket.UserSitResult(nSeatId)); //c.SendPacket(CPacket.EnableActions());
		}

		private void Handle_UserPortableChairSitRequest(WvsGameClient c, CInPacket p)
		{
			var dwTemplateId = p.Decode4();

			if (dwTemplateId / 10000 == 301)
			{
				if (InventoryManipulator.ContainsItem(c.Character, dwTemplateId))
				{
					c.Character.nActivePortableChairID = dwTemplateId;
					c.Character.Field.Broadcast(CPacket.UserSetActivePortableChair(c.Character.dwId, dwTemplateId), c);
				}
			}

			c.Character.Action.Enable();
		}

		private void Handle_UserStatChangeByPortableChairRequest(WvsGameClient c, CInPacket p)
		{
			if (c.Character.nActivePortableChairID == 0) return;
			if (c.Character.tPortableChairSittingTime.SecondsSinceStart() < 10) return;

			if (InventoryManipulator.GetAnyItem(c.Character, InventoryType.Install, c.Character.nActivePortableChairID).Item2 is GW_ItemSlotBundle pItem)
			{
				if (pItem.Template is InstallItemTemplate template)
				{
					c.Character.Modify.Heal(template.RecoveryHP, template.RecoveryMP);
				}
			}
		}

		private void Handle_UserADBoardClose(WvsGameClient c, CInPacket p)
		{
			c.Character.Action.SetADBoard(null);
		}

		private void Handle_UserEmotion(WvsGameClient c, CInPacket p)
		{
			var nEmotion = p.Decode4();
			var nDuration = p.Decode4();
			var bByItemOption = p.Decode1();

			//if (emote > 7)
			//{
			//    int emoteid = 5159992 + emote;
			//    //TODO: As if i care check if the emote is in CS inventory, if not return
			//}

			c.Character.Field.Broadcast(CPacket.UserEmoticon(c.Character.dwId, nEmotion, nDuration, bByItemOption), c);
		}

		private void Handle_UserBanMapByMob(WvsGameClient c, CInPacket p)
		{
			var user = c.Character;

			int dwMobTemplateID = p.Decode4();

			//6090001.img
			//-> ban
			//
			//-->banMap
			//--->0
			//---->field | 211000000
			//---->portal | sp
			//
			//-->banMsg | "You have been relocated due to the curse of the Witch."


			user.Action.SetField(100000000, 0, 0);
			user.Action.SystemMessage("UserBanMapByMob: You were warped to default map");

			//v2 = this;
			//v3 = CMobTemplate::GetMobTemplate(dwMobTemplateID);
			//v4 = v3;
			//if ( v3 )
			//{
			//  v5 = v3->aBanMap.a;
			//  if ( v5 && (nMapNo = (int)v5[-1].sPortalName._m_pStr, nMapNo > 0) )
			//  {
			//    v12 = 0;
			//    nMapNoa = CRand32::Random(&g_rand) % nMapNo;
			//    v8 = ZArray<MobBanMap>::operator[](&v4->aBanMap, nMapNoa);
			//    v11._m_pStr = v9;
			//    v13 = &v11;
			//    ZXString<char>::ZXString<char>(&v11, &v8->sPortalName);
			//    v14 = -1;
			//    v10 = ZArray<MobBanMap>::operator[](&v4->aBanMap, nMapNoa);
			//    CUser::PostTransferField(v2, v10->dwFieldID, v11, v12);
			//    CUser::SendSystemMessage(v2, v4->sBanMsg._m_pStr);
			//  }
			//  else
			//  {
			//    v6 = v2->m_pField;
			//    if ( v6 )
			//      v7 = v6->m_dwField;
			//    else
			//      v7 = 0;
			//    CVerboseObj::LogError((CVerboseObj *)&v2->vfptr, aIncorrectMobte, v4->dwTemplateID, v7, v2->m_dwCharacterID);
			//  }
			//}
		}

		private void Handle_UserSelectNpc(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;

			var dwNpcId = p.Decode4();
			var nPosX = p.Decode2();
			var nPosY = p.Decode2();

			if (c.NpcScript != null)
			{
				//Log.Warn("UserSelectNpc: Context already set");
				c.Character.Action.SystemMessage("Seems like the server thinks you're already talking to an npc. Use @dispose to fix.");
				c.Character.Action.Enable();
				return;
			}

			var npc = c.Character.Field.Npcs[dwNpcId];

			if (npc is null)
			{
#if DEBUG
				Log.WarnFormat("UserSelectNpc: Cannot find {0}", dwNpcId);
#endif
				c.Character.Action.Enable();
				return;
			}

			var bHasShop = MasterManager.ShopManager.HasShop(npc.TemplateId);

			if (bHasShop)
			{
				MasterManager.ShopManager.InitUserShop(c.Character, npc.TemplateId);
			}
			else
			{
				var provider = ServerApp.Container.Resolve<ScriptManager>();

				var script = provider.GetNpcScript(npc.TemplateId, c);

				if (script is null)
				{
					Log.WarnFormat("Unable to find script for npc {0}", npc.TemplateId);
					c.Character.Action.Enable();
				}
				else
				{
					c.ChangeScript(script);
					c.NpcScript.Execute();
				}
			}
		}

		private void Handle_Admin(WvsGameClient c, CInPacket p)
		{
			var pUser = c.Character;

			if (pUser == null) //m_nGradeCode & 1 | Admin Check
			{
				return;
			}

			byte nSubOpcode = p.Decode1();

			switch (nSubOpcode)
			{

			}
		}

		private void Handle_Log(WvsGameClient c, CInPacket p)
		{
			var pUser = c.Character;

			if (pUser == null) //m_nGradeCode & 1 | Admin Check
			{
				return;
			}

			var sText = p.DecodeString();

			Log.InfoFormat("[AdminLog] {0} : {1}", pUser.Stats.sCharacterName, sText);
		}

		private void Handle_UserActivateEffectItem(WvsGameClient c, CInPacket p)
		{
			var nItemID = p.Decode4();

			if (c.Character.nActiveEffectItemID == nItemID) return;

			if (nItemID == 0)
			{
				c.Character.Field.Broadcast(CPacket.SetActiveEffectItem(c.dwCharId, 0), c);
			}
			else
			{
				if (nItemID / 10000 != 501) return;

				if (InventoryManipulator.GetAnyItem(c.Character, InventoryType.Cash, nItemID).Item2 is GW_ItemSlotBase)
				{
					c.Character.Field.Broadcast(CPacket.SetActiveEffectItem(c.dwCharId, nItemID), c);

					if (nItemID / 10000 != 5011)
					{
						c.Character.nActiveEffectItemID = nItemID;
					}
				}
			}
		}

		private void Handle_UserRemoteShopOpenRequest(WvsGameClient c, CInPacket p)
		{
			//CWvsContext::SendCashSlotItemUseRequest calls this get_cashslot_item_type 35
			var nPos = p.Decode2();
			c.Character.Action.FeatureNotAddedMessage("UserRemoteShopOpenRequest");
		}

		private void Handle_UserScriptMessageAnswer(WvsGameClient c, CInPacket p)
		{
			var pScript = c.NpcScript;

			if (pScript == null)
			{
				Log.Warn("UserScriptMessageAnswer: No context");
				c.Character.Action.Enable();
				return;
			}

			var nType = (ScriptMsgType)p.Decode1();
			var nTypeExpect = pScript.LastMsgType;

			if (nType != nTypeExpect)
			{
				Log.WarnFormat("UserScriptMessageAnswer: Received {0} | Expected {1}", nType, nTypeExpect);
				c.Character.Action.Enable();
				return;
			}

			var nResp = p.Decode1();

#if DEBUG
			Log.InfoFormat("UserScriptMessageAnswer: Type {0} Answer {1}", nType, nResp);
#endif

			switch (nType)
			{
				case ScriptMsgType.Say:
					{
						if (nResp == 0) //Prev
							pScript.ProceedBack();
						else if (nResp == 1) //Next                
							pScript.ProceedNext(nResp);
						else
							pScript.EndChat();

						break;
					}
				case ScriptMsgType.AskYesNo:
					{
						if (nResp == 1 || nResp == 0)
							pScript.PushResult(nResp);
						else
							pScript.EndChat();

						break;
					}
				case ScriptMsgType.AskText:
				case ScriptMsgType.AskBoxText:
					{
						if (nResp != 0)
							pScript.PushResult(p.DecodeString());
						else
							pScript.EndChat();

						break;
					}
				case ScriptMsgType.AskNumber:
				case ScriptMsgType.AskMenu:
				case ScriptMsgType.AskSlideMenu:
					{
						if (nResp == 1)
							pScript.PushResult(p.Decode4());
						else
							pScript.EndChat();

						break;
					}
				case ScriptMsgType.AskAvatar:
				case ScriptMsgType.AskMembershopAvatar:
					{
						if (nResp == 1)
							pScript.PushResult(p.Decode1());
						else
							pScript.EndChat();

						break;
					}
			}
		}

		private void Handle_UserEffectLocal(WvsGameClient c, CInPacket p)
		{
			var nSkillID = p.Decode4();
			var nSLV = p.Decode1();
			var bSendLocal = p.Decode1();

			// increment incoming skill id by 1000, check if exists, broadcast packet to field if it does

			if (c.Character.Skills.Get(nSkillID + 1000, true) is SkillEntry se && se.nSLV == nSLV)
			{
				if (nSkillID + 1000 == (int)Skills.MECHANIC_SIEGE2 && !c.Character.Buffs.Remove((int)Skills.MECHANIC_SIEGE2))
				{
					return; // cancel buff
				}

				var effect = new UserEffectPacket(UserEffect.SkillUse)
				{
					nSkillID = nSkillID,
					nSLV = nSLV
				};

				effect.BroadcastEffect(c.Character, bSendLocal > 0);
			}
			else
			{
				c.Character.SendMessage($"LocalEffect error occurred. {nSkillID}-{nSLV}-{bSendLocal}");
			}

			//if (v8 == 35100004)
			//{
			//    v8 = 35101004;
			//}
			//else if (v8 == 35110004)
			//{
			//    v8 = 35111004;
			//}
			//else if (v8 == 35120005)
			//{
			//    v8 = 35121005;
			//}
			//else if (v8 == 35120013)
			//{
			//    v8 = 35121013;
			//}
			//else if (v8 == 35000001)
			//{
			//    v8 = 35001001;
			//}
			//else if (v8 == 35100009)
			//{
			//    v8 = 35101009;
			//}

			//if (v8 == 33101005)
			//{
			//    if ((v2->vfptr[5].Update)(v2) && CAvatar::GetOneTimeAction(&v2->vfptr) > -1)
			//    {
			//        CAvatar::ResetOneTimeAction(&v2->vfptr);
			//        v2->vfptr->PrepareActionLayer(&v2->vfptr, 6, 100, 0);
			//    }
			//    CUser::LoadSwallowingEffect(v2);
			//    return;
			//}
			//if (v8 == 33101006)
			//{
			//    CUser::RemoveSwallowingEffect(v2);
			//    return;
			//}
			//if (v8 == 35101004)
			//{
			//    if (sPath._m_pStr == 35100004)
			//        CUser::ShowEffectRocketBoosterAttack(v2, sName._m_pStr, sItemName._m_pStr);
			//    else
			//        CUser::ShowEffectRocketBooster(v2, sName._m_pStr, sItemName._m_pStr);
			//    return;
			//}
			//if (v8 == 35111004)
			//{
			//    v211 = 0;
			//    if (sPath._m_pStr == 35110004)
			//        CUser::ShowEffectSiegeEnd(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    else
			//        CUser::ShowEffectSiegeStart(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    return;
			//}
			//if (v8 == 35121005)
			//{
			//    v211 = 0;
			//    if (sPath._m_pStr == 35120005)
			//        CUser::ShowEffectSiegeEnd(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    else
			//        CUser::ShowEffectSiegeStart(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    return;
			//}
			//if (v8 == 35121013)
			//{
			//    v211 = 1;
			//    if (sPath._m_pStr == 35120013)
			//        CUser::ShowEffectSiegeEnd(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    else
			//        CUser::ShowEffectSiegeStart(v2, sName._m_pStr, sItemName._m_pStr, v211);
			//    return;
			//}
			//if (v8 == 35001001)
			//{
			//    if (sPath._m_pStr == 35000001)
			//        CUser::ShowEffectFlameThrowerEnd(v2, sName._m_pStr, sItemName._m_pStr);
			//    return;
			//}
			//if (v8 == 35101009)
			//{
			//    if (sPath._m_pStr == 35100009)
			//        CUser::ShowEffectFlameThrowerEnd(v2, sName._m_pStr, sItemName._m_pStr);
			//    return;
			//}
		}

		private void Handle_UserGatherItemRequest(WvsGameClient c, CInPacket p)
		{
			var dwTickCount = p.Decode4();
			var nType = p.Decode1(); // inventory

			if (nType > 5 || nType < 1) return; // PE

			InventoryManipulator.GatherInventory(c.Character, (InventoryType)nType);

			c.SendPacket(CPacket.GatherItemResult(nType));
		}

		private void Handle_UserSortItemRequest(WvsGameClient c, CInPacket p)
		{
			var dwTickCount = p.Decode4();
			var nType = p.Decode1(); // inventory

			if (nType > 5 || nType < 1) return; // PE

			InventoryManipulator.SortInventory(c.Character, (InventoryType)nType);

			c.SendPacket(CPacket.SortItemResult(nType));
		}

		private void Handle_UserChangeSlotPositionRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // dwTickCount
			var nTI = (InventoryType)p.Decode1(); // inventory
			var src = p.Decode2();
			var dst = p.Decode2();
			var quantity = p.Decode2();

			// Recv [CP_UserChangeSlotPositionRequest] [4D 00] [93 A5 CC 31] [01] [06 00] [80 FF] [FF FF]

#if DEBUG
			c.Character.SendMessage($"{nTI} : {src} => {dst} | {quantity}");
#endif

			if (nTI == InventoryType.Equip && src < 0)
			{
				if (dst == 0 && c.Character.Field.Template.HasDropLimit())
				{
					c.Character.SendMessage("Not allowed in this map.");
				}
				else
				{
					InventoryManipulator.UnEquip(c.Character, src, dst); //check
				}
			}
			else if (nTI == InventoryType.Equip && dst < 0)
			{
				InventoryManipulator.Equip(c.Character, src, dst); //check
			}
			else if (dst == 0)
			{
				if (c.Character.Field.Template.HasDropLimit())
				{
					c.Character.SendMessage("Not allowed in this map.");
				}
				else
				{
					InventoryManipulator.Drop(c.Character, nTI, src, quantity); // check 
				}
			}
			else if (src > 0 && dst > 0)
			{
				InventoryManipulator.Move(c.Character, nTI, src, dst); //check
			}
			// else PE
			c.Character.Action.Enable(); // incase something goes wrong somewhere
		}

		private void Handle_UserStatChangeItemUseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // dwTickCount
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			c.Character.Action.ConsumeItem.UsePotion(nItemID, nPOS);
			c.Character.Action.Enable();
		}

		private void Handle_UserStatChangeItemCancelRequest(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Field.Template.HasStatChangeItemLimit())
			{
				c.Character.SendMessage("Not allowed in this map.");
				return;
			}

			var itemId = p.Decode4();
			c.Character.Buffs.UserTryCancelBuff(itemId);
			c.Character.Action.Enable();
		}

		// CWvsContext::SendConsumeCashItemUseRequest
		private void Handle_UserConsumeCashItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var dwTickCount = p.Decode4();
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			var nConsumeCashItemType = ItemConstants.get_consume_cash_item_type(nItemID);
			var pChar = c.Character;

			if (nConsumeCashItemType == CashItemType.NONE)
			{
				pChar.SendMessage($"Tried to process invalid cash item type in UserConsumeCashItemUseRequest. CIT: {nameof(nConsumeCashItemType)} | CharID: {c.Character.dwId} | nPOS: {nPOS} | nItemID: {nItemID}");
				pChar.Action.Enable();
				return;
			}

			var pField = c.Character.Field;

			var pCashItemUsed = InventoryManipulator.GetItem(pChar, InventoryType.Cash, nPOS) as GW_ItemSlotBundle;

			if (pCashItemUsed is null)
			{
				pChar.SendMessage($"Unable to find item {nItemID} in slot {nPOS}.");
				pChar.Action.Enable();
				return;
			}

			// WIP -- copying nexon's structure from v95 PDB
			switch (nConsumeCashItemType)
			{
				case CashItemType.SPEAKERCHANNEL:
				case CashItemType.SPEAKERWORLD:
				case CashItemType.SKULLSPEAKER:
					{
						var sMsg = $"{pChar.Stats.sCharacterName} : {p.DecodeString()}";
						var bWhisperIcon = p.Available > 0 && p.Decode1() > 0;

						MasterManager.CharacterPool
							.Broadcast(MegaphoneAction.MegaphonePacket(nConsumeCashItemType, bWhisperIcon, pChar.ChannelNumber, sMsg));
					}
					break;
				case CashItemType.ITEMSPEAKER: // CItemSpeakerDlg::_SendConsumeCashItemUseRequest
					{
						var sMsg = p.DecodeString();
						var bCheckBoxWhisper = p.Decode1() > 0;
						var bItem = p.Decode1() > 0;

						if (bItem)
						{
							var nTargetTI = (InventoryType)p.Decode4();
							var nTargetPOS = p.Decode4();

							if (InventoryManipulator.GetItem(pChar, nTargetTI, (short)nTargetPOS) is GW_ItemSlotBase pTargetItem)
							{
								MasterManager.CharacterPool
									.Broadcast(MegaphoneAction.ItemMegaphonePacket($"{pChar.Stats.sCharacterName} : {sMsg}",
									pChar.ChannelNumber, bCheckBoxWhisper, pTargetItem));
								break;
							}
						}

						MasterManager.CharacterPool
								.Broadcast(MegaphoneAction.ItemMegaphonePacket($"{pChar.Stats.sCharacterName} : {sMsg}",
								pChar.ChannelNumber, bCheckBoxWhisper, null));
					}
					break;
				case CashItemType.WEATHER:
					{
						if (c.Character.Field.Template.HasCashWeatherConsumeLimit())
						{
							c.Character.SendMessage("Not allowed in this map.");
							c.Character.Action.Enable();
							return;
						}
						else if (!c.Character.Field.TryAddWeatherEffect(nItemID, p.DecodeString()))
						{
							c.Character.SendMessage("Please wait until the current weather effect has ended.");
							c.Character.Action.Enable();
							return;
						}
					}
					break;
				case CashItemType.SETPETNAME:
					{
						c.Character.Pets.PetChangeNameRequest(nItemID, p.DecodeString());
					}
					break;
				case CashItemType.MESSAGEBOX:
					{
						var bannerMessage = p.DecodeString();
					}
					break;
				case CashItemType.MONEYPOCKET: // meso bag
					{
						var template = pCashItemUsed.Template as CashItemTemplate;
						var nMeso = template.Meso;

						if (template.MesoStDev <= 0) // regular meso bag
						{
							pChar.Modify.GainMeso(nMeso);
						}
						else // random meso bag
						{
							var result = new GaussianRandom().GaussianDistributionVariation(template.MesoMax - template.MesoMin, true);

							pChar.Modify.GainMeso(result);

							// TODO maybe meso logging?

							pChar.SendPacket(CPacket.RandomMesoBagSucceeded(1, result));
						}
					}
					break;
				case CashItemType.JUKEBOX: // AKA most useless item ever
					{
						c.Character.Field.Broadcast(CPacket.PlayJukeBox(c.Character.Stats.sCharacterName));
					}
					break;
				case CashItemType.SENDMEMO: // note
					{
						var noteCharTo = p.DecodeString();
						var noteMessage = p.DecodeString();
					}
					break;
				case CashItemType.MAPTRANSFER:
					{
						if (!pChar.Teleports.OnUseRequest(nItemID, p))
						{
							pChar.Action.Enable();
							return;
						}
					}
					break;
				case CashItemType.STATCHANGE: // ap reset
					{
						var apspTo = p.Decode4();
						var apspFrom = p.Decode4();

						// TODO
					}
					break;
				case CashItemType.SKILLCHANGE: // sp reset
					{
						var apspTo = p.Decode4();
						var apspFrom = p.Decode4();

						// TODO
					}
					break;
				case CashItemType.NAMING: // item tag
					{
						var nEquipPOS = p.Decode2();

						// [55 00] [81 42 66 0D] [01 00] [A0 35 4D 00] 00 00

						if (InventoryManipulator.GetItem(pChar, InventoryType.Equip, nEquipPOS) is GW_ItemSlotEquip pEquipItem)
						{
							pEquipItem.sTitle = $"{pChar.Stats.sCharacterName}";
							pChar.Modify.Inventory(ctx =>
							{
								ctx.UpdateEquipInformation(pEquipItem, nPOS);
							});
						}
						else // if u double click the item without selecting an equip it sends a packet lol
						{
							pChar.Action.Enable();
							return;
						}
					}
					break;
				case CashItemType.PROTECTING:
				case CashItemType.EXPIREDPROTECTING:
					{
						var nItemTI = (InventoryType)p.Decode4();
						var nSlotPosition = (short)p.Decode4();

						// this only works on equips right?
						if (InventoryManipulator.GetItem(pChar, nItemTI, nSlotPosition) is GW_ItemSlotEquip pEquipItem)
						{
							var template = pCashItemUsed.Template as CashItemTemplate;

							var nDuration = template.ProtectTime == 0 ? 365 : template.ProtectTime;

							pEquipItem.nAttribute |= ItemAttributeFlags.Lock;
							pEquipItem.tSealingLock.AddDays(nDuration);

							pChar.Modify.Inventory(ctx =>
							{
								ctx.UpdateEquipInformation(pEquipItem, nPOS);
							});
						}
					}
					break;
				case CashItemType.INCUBATOR:
					{
						var incubatorInv = (InventoryType)p.Decode4();
						var incubatorSlot = p.Decode2();

						// TODO fix this properly
					}
					break;
				case CashItemType.PETSKILL:
					{

					}
					break;
				case CashItemType.SHOPSCANNER:
					{

					}
					break;
				case CashItemType.PETFOOD:
					{
						if (pCashItemUsed.Template is PetFoodItemTemplate template && template.Repleteness > 0)
						{
							c.Character.Pets.EatFood(template.Repleteness, true);
						}
					}
					break;
				case CashItemType.QUICKDELIVERY:
					{

					}
					break;
				case CashItemType.ADBOARD:
					{
						pChar.Action.SetADBoard(p.DecodeString());
					}
					return; // we dont want to remove the ADBoard item
				case CashItemType.CONSUMEEFFECTITEM:
				case CashItemType.REWARD:
				case CashItemType.MASTERYBOOK:
					{

					}
					break;
				case CashItemType.CONSUMEAREABUFFITEM:
					{
						// create AffectedArea
						// TODO force all chars in affected area to have one of the randomly selected emotes from the wz files
						if (pCashItemUsed.Template is AreaBuffItemTemplate template)
						{
							var area = new CAffectedArea(nItemID, c.Character.dwId, c.Character.Position.CurrentXY, template.RB, template.LT, AffectedAreaType.Buff)
							{
								Duration = template.Time * 1000,
								nSLV = 1
							};

							c.Character.Field.AffectedAreas.Add(area);
						}
					}
					break;
				case CashItemType.COLORLENS:
					{

					}
					break;
				case CashItemType.SELECTNPC: // miu miu
					{
						MasterManager.ShopManager.InitUserShop(c.Character, 9900000);
					}
					break;
				case CashItemType.MORPH:
					{
						BuffConsume.FireAndForget(pChar, nItemID);
					}
					break;
				case CashItemType.AVATARMEGAPHONE:
					{
						if (c.Character.Stats.nLevel < Constants.MEGAPHONE_LEVEL_LIMIT)
						{
							c.Character.SendPacket(CPacket.AvatarMegaphoneRes(AvatarMegaphoneResCode.LevelLimit));
							return;
						}

						var strings = new string[4];

						for (int i = 0; i < strings.Length; i++)
						{
							if (p.Available < 2)
							{
								strings[i] = $"";
							}
							else
							{
								strings[i] = $"{p.DecodeString()}";
							}
						}

						var mega = new AvatarMegaphone
						{
							nItemID = nItemID,
							bWhisper = p.Decode1() > 0,
							alSender = c.Character.GetLook(),
							nChannelNumber = c.Character.ChannelNumber,
							nCharId = c.Character.dwId,
							sMsgs = strings,
							sName = c.Character.Stats.sCharacterName,
						};

						var nRes = MasterManager.AvatarMan.TryAddToQueue(mega);

						if (nRes == AvatarMegaphoneResCode.QueueFull)
						{
							c.Character.SendPacket(CPacket.AvatarMegaphoneRes(nRes));
							c.Character.Action.Enable();
							return;
						}

						if (nRes == AvatarMegaphoneResCode.Success_Later)
						{
							c.Character.SendPacket(CPacket.AvatarMegaphoneRes($"Your requests has successfully been placed in the queue. You are in position {mega.nInitialQueuePosition}."));
						}
					}
					break;
				case CashItemType.MAPLETV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.MAPLESOLETV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.MAPLELOVETV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.MEGATV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.MEGASOLETV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.MEGALOVETV:
					{
						//MasterManager.MapleTVMan.OnPacket(c.Character, (byte)(nItemID % 10), p);
					}
					break;
				case CashItemType.CHANGECHARACTERNAME:
					{

					}
					break;
				case CashItemType.TRANSFERWORLDCOUPON:
					{
						// TODO send unavailable popup
					}
					break;
				case CashItemType.ARTSPEAKERWORLD:
					{
						var strings = new string[p.Decode1()];

						for (int i = 0; i < strings.Length && i < 3; i++)
						{
							strings[i] = pChar.Stats.sCharacterName + " : " + p.DecodeString(); // TODO add the equipped medal info here too
						}

						var bWhisperIcon = p.Decode1() > 0;

						MasterManager.CharacterPool
							.Broadcast(MegaphoneAction.MegaphonePacket(nConsumeCashItemType, bWhisperIcon, pChar.ChannelNumber, strings));
					}
					break;
				case CashItemType.EXTENDEXPIREDATE:
					{

					}
					break;
				case CashItemType.KARMASCISSORS: // CUIKarmaDlg::_SendConsumeCashItemUseRequest
					{
						var nTargetTI = (InventoryType)p.Decode4();
						var nTargetPOS = (short)p.Decode4();

						if (InventoryManipulator.GetItem(pChar, nTargetTI, nTargetPOS) is GW_ItemSlotEquip pEquipItem)
						{
							if ((pEquipItem.nAttribute & ItemAttributeFlags.Untradeable) > 0)
							{

							}


						}
					}
					break;
				case CashItemType.CHARACTERSALE:
					{

					}
					break;
				case CashItemType.ITEMUPGRADE:
					{
						var nItemTI = (InventoryType)p.Decode4();
						var nSlotPosition = p.Decode2();

						//Log.Debug($"[{nameof(CashItemType.ITEMUPGRADE)}]: {nItemTI} - {nSlotPosition}");

						if (InventoryManipulator.GetItem(pChar, nItemTI, nSlotPosition) is GW_ItemSlotEquip pEquipItem)
						{
							if (pEquipItem.HammerUpgradeCount < 2)
							{
								pEquipItem.RemainingUpgradeCount += 1;
								pEquipItem.HammerUpgradeCount += 1;

								c.Character.SendPacket(pEquipItem.ItemUpgradeResult(
									GoldHammer.ReturnResult_ItemUpgradeDone, GoldHammer.Result_Success));
								c.Character.SendPacket(pEquipItem.ItemUpgradeResult(
									GoldHammer.ReturnResult_ItemUpgradeSuccess, GoldHammer.Result_Done));

								pChar.Modify.Inventory(ctx =>
								{
									ctx.Remove(pEquipItem.InvType, nPOS);
									ctx.Add(pEquipItem.InvType, nPOS, pEquipItem);
								});
							}
							else
							{
								c.Character.SendPacket(
									pEquipItem.ItemUpgradeResult(GoldHammer.ReturnResult_ItemUpgradeErr,
										GoldHammer.Result_Fail));
								return; // dont want to remove the hammer incase of client error
							}
						}
						else
						{
							c.Character.SendPacket( // TODO unhack this
								new GW_ItemSlotEquip(0).ItemUpgradeResult(GoldHammer.ReturnResult_ItemUpgradeErr,
									GoldHammer.Result_Fail));
							return; // dont want to remove the hammer incase of client error
						}
					}
					break;
				case CashItemType.VEGA:
					{

					}
					break;
				case CashItemType.ITEM_UNRELEASE:
					{
						var nEquipPOS = p.Decode2();

						var pEquipItem = InventoryManipulator.GetItem(pChar, InventoryType.Equip, nEquipPOS) as GW_ItemSlotEquip;

						if (pEquipItem is null || !pEquipItem.HasVisiblePotential) return;

						var bSuccess = InventoryManipulator.InsertInto(pChar, MasterManager.CreateItem(ItemConstants.MiracleCubeFragment)) > 0;

						if (bSuccess)
						{
							pEquipItem.nGrade -= 4;
							pChar.Modify.Inventory(ctx =>
							{
								ctx.UpdateEquipInformation(pEquipItem, nEquipPOS);
							});
						}

						pChar.SendPacket(pEquipItem.ShowItemUnreleaseEffect(pChar.dwId, bSuccess));
					}
					break;
				case CashItemType.SKILLRESET:
					{

					}
					break;
				case CashItemType.QUESTDELIVERY:
					{

					}
					break;
				default:
					{
						pChar.SendMessage($"Tried to process invalid cash item type in UserConsumeCashItemUseRequest. CIT: {nameof(nConsumeCashItemType)} | CharID: {c.Character.dwId} | nPOS: {nPOS} | nItemID: {nItemID}");
						return;
					}
			}

			InventoryManipulator.RemoveFrom(pChar, pCashItemUsed.InvType, nPOS);
			c.Character.Action.Enable();
		}

		private void Handle_UserSkillResetItemUseRequest(WvsGameClient c, CInPacket p)
		{
			int dwTickCount = p.Decode4();
			short nPOS = p.Decode2();
			int nItemID = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"UserSkillResetItemUseRequest {nItemID}");
		}

		private void Handle_UserShopScannerItemUseRequest(WvsGameClient c, CInPacket p)
		{
			short nPOS = p.Decode2();
			int nItemID = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"UserShopScannerItemUseRequest {nItemID}");
		}

		private void Handle_UserMapTransferItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			var (slot, item) = InventoryManipulator.GetAnyItem(c.Character, InventoryType.Consume, nPOS);

			if (item.nItemID != 2320000) return; // TODO const value for this

			if (c.Character.Teleports.OnUseRequest(nItemID, p))
			{
				InventoryManipulator.RemoveFrom(c.Character, item.InvType, nPOS);
			}
		}

		private void Handle_UserUIOpenItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var user = c.Character;

			var dwTickCount = p.Decode4();
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"UserUIOpenItemUseRequest {nItemID}");
		}

		private void Handle_UserAbilityUpRequest(WvsGameClient c, CInPacket p)
		{
			p.Skip(4);
			var stat = (ModifyStatFlags)p.Decode4();

			c.Character.Modify.Stats(mod =>
			{
				if (mod.AP <= 0) return;

				mod.AddStat(stat, 1);
				mod.AP -= 1;
			});
		}

		private void Handle_UserMobSummonItemUseRequest(WvsGameClient c, CInPacket p)
		{
			// [51 00] [28 C4 B4 38] [04 00] [4C 2E 20 00]
			var tick = p.Decode4();
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			if (InventoryManipulator.GetItem(c.Character, ItemConstants.GetInventoryType(nItemID), nPOS) is
				GW_ItemSlotBundle isb)
			{
				c.Character.Field.Mobs.OnMobSummonItemUseRequest(nItemID, c.Character.Position.X,
					c.Character.Position.Y);

				InventoryManipulator.RemoveFrom(c.Character, isb.InvType, nPOS, 1);
			}
		}

		private void Handle_UserBridleItemUseRequest(WvsGameClient c, CInPacket p)
		{
			int dwTickCount = p.Decode4();
			short nPOS = p.Decode2();
			int nItemID = p.Decode4();
			int dwMobID = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"UserBridleItemUseRequest {nItemID}");
		}

		private void Handle_UserAbilityMassUpRequest(WvsGameClient c, CInPacket p)
		{
			p.Skip(4);
			var loop_count = p.Decode4();

			c.Character.Modify.Stats(mod =>
			{
				for (int i = 0; i < loop_count; i++)
				{
					var stat = (ModifyStatFlags)p.Decode4();
					var count = p.Decode4();
					if (count > mod.AP) return;

					mod.AddStat(stat, count);
					mod.AP -= (short)count;
				}
			});
		}

		private void Handle_UserChangeStatRequest(WvsGameClient c, CInPacket p) // hp/mp recovery packet
		{
			if (c.Character.Stats.nHP <= 0) return;

			var nTickCount = p.Decode4();
			var dwFlag = p.Decode4();

			var nHPDelta = 0;
			var nMPDelta = 0;

			if ((dwFlag & 0x400) > 0)
				nHPDelta = p.Decode2();

			if ((dwFlag & 0x1000) > 0)
				nMPDelta = p.Decode2();

			//var nOption = p.Decode1(); 

			//var flag = (ModifyStatFlags)dwFlags;

			c.Character.Modify.Heal(nHPDelta, nMPDelta);

			//if (nHPDelta > 0)
			//{
			//	c.Character.Modify.Heal
			//}
			//	c.ch.HP = Math.Min(mod.MaxHP, mod.HP + nHPDelta);

			//if (nMPDelta > 0)
			//	mod.MP = Math.Min(mod.MaxMP, mod.MP + nMPDelta);

			//return;
			//c.Character.Modify.Stats(mod =>
			//{


			//	//BMS Copypasta
			//	//if (dwFlaga & 2)
			//	//{
			//	//    dRecoveryRate = dRecoveryRate * 1.5;
			//	//    pc = CItemInfo::GetPortableChairItem(TSingleton < CItemInfo >::ms_pInstance, v2->m_nActivePortableChairID);
			//	//}
			//	//if (dwFlaga & 1)
			//	//    v6 = get_endure_duration(&v2->m_character);
			//});
		}

		private void Handle_UserChangeStatRequestByItemOption(WvsGameClient c, CInPacket p)
			=> Handle_UserChangeStatRequest(c, p); // same thing

		/**
         * Teleport scroll handler
         * Location in PDB: CWvsContext::SendPortalScrollUseRequest
         * Example packet: Nearest town return scroll
         * [CP_UserPortalScrollUseRequest] [5C 00] [BF CD 28 00] [01 00] [B0 F9 1E 00]
         */
		private void Handle_UserPortalScrollUseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // tick count

			var nPos = p.Decode2();
			var nItemID = p.Decode4();

			c.Character.Action.ConsumeItem.UsePortalScroll(nItemID, nPos);
			c.Character.Action.Enable();
		}

		/**
         * Item upgrade scroll handler
         * Location in PDB: CWvsContext::SendPortalScrollUseRequest
         * Related location: CQWUInventory::UpgradeEquip(CQWUInventory *this, int nUPOS, int nEPOS, int nWhiteScroll, int bEnchantSkill, int tRequestTime)
         * Example packet: Upgrade scroll
         * [CP_UserUpgradeItemUseRequest] [5D 00] [1D C7 2B 00] [0E 00] [FB FF] [01 00] [00]
         */
		private void Handle_UserUpgradeItemUseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // tRequestTime

			var nUPos = p.Decode2();
			var nEPos = p.Decode2();
			var whiteScroll = p.Decode2() > 1; // nWhiteScroll no = 1, yes = 2 ??
			p.Decode1(); // bEnchantSkill

			c.Character.Action.ConsumeItem.UseUpgradeScroll(nUPos, nEPos, whiteScroll);
			c.Character.Action.Enable();
		}

		/**
         * Equip enhancement/star scroll handler
         * [CP_UserHyperUpgradeItemUseRequest] [5E 00] [B5 5D 1B 06] [04 00] [FC FF] [00]
         */
		private void Handle_UserHyperUpgradeItemUseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // tRequestTime

			var nUPos = p.Decode2();
			var nEPos = p.Decode2();
			p.Decode1(); // bEnchantSkill

			var pScroll = InventoryManipulator.GetItem(c.Character, InventoryType.Consume, nUPos) as GW_ItemSlotBundle;
			var pEquip = InventoryManipulator.GetItem(c.Character, InventoryType.Equip, nEPos) as GW_ItemSlotEquip;

			if (pScroll is null || pEquip is null) return;
			if (!ItemConstants.is_hyper_upgrade_item(pScroll.nItemID)) return;
			if (pEquip.CashItem || pEquip.StarUpgradeCount >= 10) return;


			var odds = pScroll.nItemID % 2 == 0 ? 1.0 : 0.8;
			odds = Math.Min(1.0, odds - (pEquip.StarUpgradeCount * 0.1));

			var bSuccess = Constants.Rand.NextDouble() < odds;

			if (bSuccess)
			{
				var rand = Constants.Rand;
				var gX = new GaussianRandom();

				pEquip.StarUpgradeCount += 1;

				if (pEquip.niSTR > 0 || rand.Next(100) == 1) // ODDS: 1%
					pEquip.niSTR += (short)gX.GaussianDistributionVariation(3, true);
				if (pEquip.niLUK > 0 || rand.Next(100) == 1) // ODDS: 1%
					pEquip.niLUK += (short)gX.GaussianDistributionVariation(3, true);
				if (pEquip.niINT > 0 || rand.Next(100) == 1) // ODDS: 1%
					pEquip.niINT += (short)gX.GaussianDistributionVariation(3, true);
				if (pEquip.niDEX > 0 || rand.Next(100) == 1) // ODDS: 1%
					pEquip.niDEX += (short)gX.GaussianDistributionVariation(3, true);
				// hp/mp
				if (pEquip.niMaxHP > 0)
					pEquip.niMaxHP += (short)gX.GaussianDistributionVariation(10, true);
				if (pEquip.niMaxMP > 0)
					pEquip.niMaxMP += (short)gX.GaussianDistributionVariation(10, true);
				// damages
				if (pEquip.niPAD > 0)
					pEquip.niPAD += (short)gX.GaussianDistributionVariation(3, true);
				if (pEquip.niMAD > 0)
					pEquip.niMAD += (short)gX.GaussianDistributionVariation(3, true);
				// defenses
				if (pEquip.niPDD > 0)
					pEquip.niPDD += (short)gX.GaussianDistributionVariation(5, true);
				if (pEquip.niMDD > 0)
					pEquip.niMDD += (short)gX.GaussianDistributionVariation(5, true);
				// accuracy/avoidability
				if (pEquip.niACC > 0)
					pEquip.niACC += (short)gX.GaussianDistributionVariation(5, true);
				if (pEquip.niEVA > 0)
					pEquip.niEVA += (short)gX.GaussianDistributionVariation(5, true);
				// movement
				if (pEquip.niSpeed > 0)
					pEquip.niSpeed += (short)gX.GaussianDistributionVariation(2, true);
				if (pEquip.niJump > 0)
					pEquip.niJump += (short)gX.GaussianDistributionVariation(2, true);

				c.Character.Modify.Inventory(ctx =>
				{
					ctx.UpdateEquipInformation(pEquip, nEPos);
				});
			}
			else
			{
				InventoryManipulator.RemoveFrom(c.Character, InventoryType.Equip, nEPos); // delete item
			}

			InventoryManipulator.RemoveFrom(c.Character, InventoryType.Consume, nUPos);

			c.Character.StatisticsTracker.IncrementScrollUse(pScroll.nItemID, bSuccess, !bSuccess);

			c.Character.Field.Broadcast(pEquip.ShowItemHyperUpgradeEffect(c.Character, bSuccess, false));
			c.Character.Action.Enable();
		}

		/**
         * Potential scroll handler.
         * Location in PDB: CWvsContext::SendItemOptionUpgradeItemUseRequest
         */
		private void Handle_UserItemOptionUpgradeItemUseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // tRequestTime

			var nUPos = p.Decode2();
			var nEPos = p.Decode2();
			p.Decode1(); // bEnchantSkill

			var pScroll = InventoryManipulator.GetItem(c.Character, InventoryType.Consume, nUPos) as GW_ItemSlotBundle;
			var pEquip = InventoryManipulator.GetItem(c.Character, InventoryType.Equip, nEPos) as GW_ItemSlotEquip;

			if (pEquip is null || pScroll is null) return;
			if (!ItemConstants.is_itemoption_upgrade_item(pScroll.nItemID)) return;
			if (pEquip.CashItem || pEquip.nGrade != PotentialGradeCode.Normal) return;

			var bSuccess = false;
			var odds = pScroll.nItemID % 2 == 0 ? 0.9f : 0.7f;

			if (Constants.Rand.NextDouble() < odds)
			{
				bSuccess = true;
				pEquip.nGrade = PotentialGradeCode.Hidden_Rare;
				c.Character.Modify.Inventory(ctx =>
				{
					ctx.UpdateEquipInformation(pEquip, nEPos);
				});
			}
			else
			{
				InventoryManipulator.RemoveFrom(c.Character, InventoryType.Equip, nEPos); // delete item
			}

			InventoryManipulator.RemoveFrom(c.Character, InventoryType.Consume, nUPos);

			c.Character.StatisticsTracker.IncrementScrollUse(pScroll.nItemID, bSuccess, !bSuccess);

			c.Character.Field.Broadcast(pEquip.ShowItemOptionUpgradeEffect(c.Character, bSuccess, false));
			c.Character.Action.Enable();
		}

		/**
         * Magnifying glass handler.
         * CP_UserItemReleaseRequest
         * [INFO] Recv [CP_UserItemReleaseRequest] [61 00] [D8 BD A7 00] [01 00] [01 00]
         */
		private void Handle_UserItemReleaseRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // m_tUpdateTime
			var nUPOS = p.Decode2();
			var nEPOS = p.Decode2();

			c.Character.Action.ConsumeItem.UseMagnifyingGlass(nUPOS, nEPOS);
			c.Character.Action.Enable();
		}

		private void Handle_UserDropMoneyRequest(WvsGameClient c, CInPacket p)
		{
			p.Decode4(); // nTickCount
			var nMeso = p.Decode4();

			if (nMeso <= 0 || nMeso > c.Character.Stats.nMoney) return; // PE

			if (c.Character.Field.Template.HasDropLimit())
			{
				c.Character.SendMessage("Not allowed in this map.");
				return;
			}

			CDropFactory.CreateDropMeso(c.Character.Field, c.Character.Position.CurrentXY, 0, nMeso);

			c.Character.Modify.GainMeso(-nMeso, true);
			c.Character.Action.Enable();
		}

		private void Handle_UserTemporaryStatUpdateRequest(WvsGameClient c, CInPacket p)
		{
			//v4 = timeGetTime();
			//v5 = SecondaryStat::ResetByTime(&v2->m_secondaryStat, v4);
			//if (v5)
			//{
			//    CUser::ValidateStat(v2, 0);
			//    CUser::SendTemporaryStatReset(v2, v5);
			//}

			//c.Character.Action.FeatureNotAddedMessage($"UserTemporaryStatUpdateRequest");
		}

		private void Handle_UserPetFoodItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var dwDickCount = p.Decode4();
			var nPOS = p.Decode2();
			var nItemID = p.Decode4();

			var item = InventoryManipulator.GetItem(c.Character, InventoryType.Consume, nPOS) as GW_ItemSlotBundle;

			if (item?.Template is ConsumeItemTemplate template && template.PetfoodInc > 0)
			{
				c.Character.Pets.EatFood(template.PetfoodInc, false);

				InventoryManipulator.RemoveFrom(c.Character, InventoryType.Consume, nPOS, 1);
			}

			c.Character.Action.Enable();
		}

		public void Handle_UserCharacterInfoRequest(WvsGameClient c, CInPacket p) =>
			c.Character.Field.Users.UserInfoRequest(c.Character, p);

		private void Handle_UserActivatePetRequest(WvsGameClient c, CInPacket p)
		{
			var dwTickCount = p.Decode4();
			var nPos = p.Decode2();
			var bLead = p.Decode1() > 0;

			c.Character.Pets.ActivateSinglePet(nPos);
			c.Character.Action.Enable();
		}

		/**
         * FM warp:
         * [INFO] Recv [CP_UserPortalScriptRequest] [70 00] [01] [08 00] [6D 61 72 6B 65 74 30 30] [39 03] [9A 00]
         */
		private void Handle_UserPortalScriptRequest(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;

			var m_bCurFieldKey = p.Decode1();
			var sPortal = p.DecodeString();
			var nX = p.Decode2();
			var nY = p.Decode2();

			c.Character.Field.OnUserEnterScriptedPortal(c.Character, sPortal);
		}

		private void Handle_UserPortalTeleportRequest(WvsGameClient c, CInPacket p)
		{
			//This is using a portal to another portal inside same map
			//Eg the henesys orbs
			//Only thing to do here is update the XY acordingly
			var m_bCurFieldKey = p.Decode1();
			var sPortal = p.DecodeString();
			var nX = p.Decode2();
			var nY = p.Decode2();
		}

		private void Handle_UserMapTransferRequest(WvsGameClient c, CInPacket p)
			=> c.Character.Teleports.OnUpdateRequest(p);

		private void Handle_UserAntiMacroItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var character = c.Character;
			var sSnith = character.Stats.sCharacterName;

			var sCharacterName = p.DecodeString();
			var nSlot = p.Decode2();
			var nItemID = p.Decode4();

			var aUseInv = c.Character.InventoryConsume;

			//Sanitary check on 
			//FieldLimit
			//Event Instance
			//Lie detector already in progress
			//Lie detector already failed on this guy

			//Prevent people from changing maps if lie detector in progrtess

			if (aUseInv.Get((byte)nSlot)?.nItemID == nItemID)
			{
				var remote = MasterManager.CharacterPool.Get(sCharacterName);

				//Check remote online, semd bad responses etc etc etc

				remote.SendPacket(CPacket.AntiMacroResult(6, AntiMacroType.Admin, sSnith));
			}
			else
			{
				c.Character.Action.Enable();
				//Hack
			}
		}

		private void Handle_UserAntiMacroQuestionResult(WvsGameClient c, CInPacket p)
		{
			var sResponse = p.DecodeString();
			c.Character.Action.FeatureNotAddedMessage("UserAntiMacroQuestionResult");
		}

		private void Handle_UserClaimRequest(WvsGameClient c, CInPacket p)
		{
			var bChatClaim = p.Decode1();
			var sCharacterName = p.DecodeString();
			var nType = p.Decode1();
			var sContext = p.DecodeString();

			if (bChatClaim > 0)
			{
				var sChatLog = p.DecodeString();
			}

			c.Character.Action.FeatureNotAddedMessage("UserClaimRequest");
		}

		private void Handle_UserMacroSysDataModified(WvsGameClient c, CInPacket p)
			=> c.Character.Macros.Decode(p);


		private void Handle_UserLotteryItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var nPOS = p.Decode2();
			var nItemId = p.Decode4();

			c.Character.Action.ConsumeItem.UseLotteryItem(nPOS, nItemId);
			c.Character.Action.Enable();
		}

		private void Handle_UserUseGachaponBoxRequest(WvsGameClient c, CInPacket p)
		{
			var user = c.Character;

			var nPOS = p.Decode2();
			var nItemId = p.Decode4();


			// response packet structure (same for remote):
			// p.encode1(opcode); // use CashItemType enum (0xC0 && 0xC1)
			// if (opcode == 0xC0)
			// {
			// p.encode8(CashItemSN)
			// p.encode4(nNumber) // item count prolly
			//                          // CInPacket::DecodeBuffer(iPacket, v11, 55u);
			// p.encode4(nSelectedItemID)
			// p.encode1(nSelectedItemCount)
			// p.encode1(bJackPot)
			// }
			// else {} // fail op, no more encodes

			//TODO: Consumption
			c.Character.Action.FeatureNotAddedMessage($"UserUseGachaponBoxRequest {nItemId}");
		}

		private void Handle_UserUseGachaponRemoteRequest(WvsGameClient c, CInPacket p)
		{
			// * Remote gachapon item request 
			// * Packet: [CP_UserUseGachaponRemoteRequest] [80 00] [F8 2C 53 00] [05 00 00 00]

			p.Decode4(); // timestamp
			var selection = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"Handle_UserUseGachaponRemoteRequest {selection}");
		}

		private void Handle_UserFollowCharacterRequest(WvsGameClient c, CInPacket p)
		{
			var dwDriverID = p.Decode4();
			var bAutoReq = p.Decode1();
			var bKeyInput = p.Decode1();

			var user = c.Character.Field.Users[dwDriverID];

			user?.SendPacket(CPacket.SetPassengerRequest(c.Character.dwId));
			//This gets sent for cancel?
		}

		private void Handle_UserFollowCharacterWithdraw(WvsGameClient c, CInPacket p)
		{
			c.Character.Action.FeatureNotAddedMessage($"UserFollowCharacterWithdraw");
		}

		private void Handle_SetPassengerResult(WvsGameClient c, CInPacket p)
		{
			var dwFollowRequesterID = p.Decode4();
			var bApply = p.Decode1();

			if (bApply == 0)
			{

			}
			else
			{
				c.Character.Field.Broadcast(CPacket.UserFollowCharacter(dwFollowRequesterID, c.Character.dwId, 0, 0, 0));

			}

			//if (bApply == 0)
			//    var unk = p.Decode4(); //5

			c.Character.Action.FeatureNotAddedMessage($"SetPassengerResult");
		}

		private void Handle_FriendRequest(WvsGameClient c, CInPacket p)
			=> c.Character.Friends.OnPacket(p);

		private void Handle_UserUseWaterOfLife(WvsGameClient c, CInPacket p)
		{
			//No Data | CWvsContext::SendWaterOfLife
			c.Character.Action.FeatureNotAddedMessage($"UserUseWaterOfLife");
		}

		private void Handle_EnterTownPortalRequest(WvsGameClient c, CInPacket p)
		{
			//Priest Mystic Door Skill Enter
			c.Character.Field.TownPortals.OnPacket(c, p);
		}

		private void Handle_EnterOpenGateRequest(WvsGameClient c, CInPacket p)
			=> c.Character.Action.OpenGate(p);

		private void Handle_FuncKeyMappedModified(WvsGameClient c, CInPacket p)
			=> c.Character.FuncKeys.OnFuncKeyMappedModified(p);

		private void Handle_MarriageRequest(WvsGameClient c, CInPacket p)
		{
			byte nSubOpcode = p.Decode1();

			if (nSubOpcode != 0x03) //CWvsContext::SendRingDropRequest
				return;

			switch (nSubOpcode) //CWvsContext::SendEngagementRequest
			{
				case 0x00:
					{
						var sName = p.DecodeString();
						int nItemID = p.Decode4();
						break;
					}

				case 0x03:
					{
						int nItemID = p.Decode4();
						break;
					}

				case 0x06:
					{
						int nPOS = p.Decode4();
						int nItemID = p.Decode4();
						break;
					}
			}

			c.Character.Action.FeatureNotAddedMessage($"MarriageRequest");
		}

		private void Handle_BoobyTrapAlert(WvsGameClient c, CInPacket p)
		{
			int nTrapType = p.Decode4();

			//if (nTrapType <= 0 || nTrapType >= 3)
			//    Log.WarnFormat("[FireBomberLog Alert] Illegal Reason Range!!!");

			//Log.WarnFormat("[FireBomberLog(%d)] Illegal Client Cheat Inspected!!! [FieldID:%d]");

			c.Character.Action.FeatureNotAddedMessage($"BoobyTrapAlert {nTrapType}");
		}

		private void Handle_StalkBegin(WvsGameClient c, CInPacket p)
		{
			var character = c.Character;
			character.Field.SendStalkees(character);
		}

		private void Handle_FamilyChartRequest(WvsGameClient c, CInPacket p)
		{
			var sName = p.DecodeString();
			c.Character.Action.FeatureNotAddedMessage($"FamilyChartRequest");
		}

		private void Handle_FamilyInfoRequest(WvsGameClient c, CInPacket p)
		{
			//Gotta send them family info
			c.Character.Action.FeatureNotAddedMessage($"FamilyInfoRequest");
		}

		private void Handle_FamilyRegisterJunior(WvsGameClient c, CInPacket p)
		{
			var sCharacterName = p.DecodeString();
			c.Character.Action.FeatureNotAddedMessage($"FamilyRegisterJunior");
		}

		private void Handle_FamilySetPrecept(WvsGameClient c, CInPacket p)
		{
			var strPrecept = p.DecodeString();
			c.Character.Action.FeatureNotAddedMessage($"FamilySetPrecept");
		}

		private void Handle_UserExpUpItemUseRequest(WvsGameClient c, CInPacket p)
		{
			var user = c.Character;

			var dwTickCount = p.Decode4();
			var nPOS = p.Decode2();
			var nItemId = p.Decode4();

			//TODO: Consumption
			c.Character.Action.FeatureNotAddedMessage($"UserExpUpItemUseRequest {nItemId}");
		}

		private void Handle_UserTempExpUseRequest(WvsGameClient c, CInPacket p)
		{
			var user = c.Character;

			var dwTickCount = p.Decode4();

			//TODO: Consumption    
			c.Character.Action.FeatureNotAddedMessage($"UserTempExpUseRequest");

		}

		private void Handle_TalkToTutor(WvsGameClient c, CInPacket p)
		{
			//Supposed to open a NPC chat ( unsure about Arans )
			c.Character.Action.FeatureNotAddedMessage($"TalkToTutor");
		}

		private void Handle_MobCrcKeyChangedReply(WvsGameClient c, CInPacket p)
		{
			//No Data
			//We now set the value we sent to the client 
			//to our internal dwMobCrcKey variable
		}

		private void Handle_RequestSessionValue(WvsGameClient c, CInPacket p)
		{
			//CField_EscortResult::Init askes for "escort_result"
			//CWvsContext::SetImpactNextBySessionValue

			var sKey = p.DecodeString();
			var bReset = p.Decode1();

			c.Character.Action.FeatureNotAddedMessage($"RequestSessionValue {sKey}");
		}

		private void Handle_AccountMoreInfo(WvsGameClient c, CInPacket p)
		{
			byte nSubOp = p.Decode1();

			if (nSubOp == 0x01) //LoadAccountMoreInfoRequest
			{

			}

			else if (nSubOp == 0x03) //SaveAccountMoreInfoRequest
			{
				// C1 00 03 06 0A 00 00 08 B9 30 01 14 00 00 00 01 00 00 00
			}

			c.Character.Action.FeatureNotAddedMessage($"AccountMoreInfo");
		}

		private void Handle_FindFriend(WvsGameClient c, CInPacket p)
		{
			byte nSubOp = p.Decode1(); //Not sure 

			if (nSubOp == 0x07) //Find More Friends
			{

			}

			c.Character.Action.FeatureNotAddedMessage($"FindFriend");
		}

		private void Handle_NewYearCardRequest(WvsGameClient c, CInPacket p)
		{
			//CUINewYearCardSenderDlg::_SendNewYearCard
			var v1 = p.Decode1();

			if (v1 == 0)
			{
				var nInvenPOS = p.Decode2();
				var nItemID = p.Decode4();
				var sTarget = p.DecodeString();
				var sMemo = p.DecodeString();

				//TODO: Logic
			}

			c.Character.Action.FeatureNotAddedMessage($"NewYearCardRequest");
		}

		private void Handle_RandomMorphRequest(WvsGameClient c, CInPacket p)
		{
			var dwTickCount = p.Decode4();
			var nInvenPOS = p.Decode2();
			var nItemID = p.Decode4();
			var sTarget = p.DecodeString();

			//TODO: Consume + Morph
			c.Character.Action.FeatureNotAddedMessage($"RandomMorphRequest");
		}

		private void Handle_UpdateGMBoard(WvsGameClient c, CInPacket p)
		{
			//CWvsContext::CheckOpBoardHasNew
			var nOpBoardIndex = p.Decode4(); //CConfig::CONFIG_GAMEOPT::nGameOpt_OpBoardIndex

			c.SendPacket(CPacket.UpdateGMBoard(nOpBoardIndex, "http:://google.com"));
		}

		private void Handle_NpcSpecialAction(WvsGameClient c, CInPacket p)
		{
			var dwNpcId = p.Decode4();
			var sAction = p.DecodeString();

			//9000021 (Gaga) requests "act2"
			c.Character.Action.FeatureNotAddedMessage($"NpcSpecialAction");
		}

		private void Handle_PulleyHit(WvsGameClient c, CInPacket p)
		{
			//No Data | CField_GuildBoss::BasicActionAttack
			c.Character.Action.FeatureNotAddedMessage($"PulleyHit");
		}

		private void Handle_RequestFootHoldInfo(WvsGameClient c, CInPacket p)
		{
			//No Data | Sent when field type is 29 ( in CStage::OnSetField )
			//TODO: Send the client all the footholds
			c.Character.Action.FeatureNotAddedMessage($"RequestFootHoldInfo");
		}

		private void Handle_FootHoldInfo(WvsGameClient c, CInPacket p)
		{
			//Client sends the server all its footholds
			c.Character.Action.FeatureNotAddedMessage($"FootHoldInfo");
		}

		private void Handle_LogoutGiftSelect(WvsGameClient c, CInPacket p)
		{
			// Recv [CP_LogoutGiftSelect] [39 01] 02 00 00 00

			var nIndex = p.Decode4();

			if (nIndex >= 0 && nIndex <= 2)
			{

			}

			c.SendPacket(LogoutGiftConfig.OnLogoutGift());
		}

		private void Handle_PetMove(WvsGameClient c, CInPacket p)
		{
			c.Character.Pets.OnPetPacket(RecvOps.CP_PetMove, p);
		}

		private void Handle_DragonMove(WvsGameClient c, CInPacket p)
			=> c.Character.Dragon?.Move(p);

		private void Handle_QuickslotKeyMappedModified(WvsGameClient c, CInPacket p)
			=> c.Character.FuncKeys.OnQuickslotKeyMapped(p);

		private void Handle_MobMove(WvsGameClient c, CInPacket p)
			=> c.Character.Field.Mobs.Move(c.Character, p);


		private void Handle_MobDropPickUpRequest(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			int dwDropID = p.Decode4();
			//if (CDropPool::OnPickUpRequest(&v3->m_pField->m_dropPool, v3, v4))
			//{
			//    v5 = ZArray < Reward >::InsertBefore(&v3->m_aRewardPicked, -1);
			//    Reward::operator= (v5, &reward);
			//}

			c.Character.Action.FeatureNotAddedMessage($"MobDropPickUpRequest");
		}

		private void Handle_MobHitByObstacle(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			int nDamage = p.Decode4();

			var pMob = c.Character.Field.Mobs[dwMobID];

			//if (pMob != null && pMob.OnMobHitByObstacleOrMob(nDamage))
			//    c.Character.Field.Mobs.Remove(pMob);

			c.Character.Action.FeatureNotAddedMessage($"MobHitByObstacle");
		}

		private void Handle_MobHitByMob(WvsGameClient c, CInPacket p)
		{
			int dwMobSrcID = p.Decode4(); //src mob
			int dwCharID = p.Decode4();
			int dwMobVictimID = p.Decode4(); //dst mob

			c.Character.Action.FeatureNotAddedMessage($"MobHitByMob");
		}

		private void Handle_MobSelfDestruct(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			//v5 = pMob->m_pTemplate;
			//if (v5)
			//{
			//    if (v5->selfDestructionInfo.nActionType & 2)
			//    {
			//        CMob::DoSelfDestruct(pMob);
			//        CLifePool::RemoveMob(v3, pMob);
			//    }
			//    else
			//    {
			//        LogError(aIllegalSelfdes);
			//    }
			//}
			c.Character.Action.FeatureNotAddedMessage($"MobSelfDestruct");
		}

		private void Handle_MobTimeBombEnd(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();

			var nX = 0;
			var nY = 0;

			//if (_ZtlSecureFuse<int>(v1->m_pTemplate->_ZtlSecureTear_bBoss, v1->m_pTemplate->_ZtlSecureTear_bBoss_CS))
			if (false)
			{
				nX = p.Decode4(); //v7 + rcBody.left) / 2
				nY = p.Decode4(); //v8 + rcBody.top) / 2
			}

			//GetPos or GetPosPrev ( I think )
			nX = p.Decode4();
			nY = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"MobTimeBombEnd");
		}

		private void Handle_MobEscortCollision(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			int nDest = p.Decode4();

			c.Character.Action.FeatureNotAddedMessage($"MobEscortCollision");
		}

		private void Handle_MobRequestEscortInfo(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			c.Character.Action.FeatureNotAddedMessage($"MobRequestEscortInfo");
		}

		private void Handle_MobEscortStopEndRequest(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			c.Character.Action.FeatureNotAddedMessage($"MobEscortStopEndRequest");
		}

		private void Handle_NpcMove(WvsGameClient c, CInPacket p)
		{
			// is null if client sends a movement packet after the player 
			//		enters the map but before player is put in object pool
			if (c.Character.Field is null) return;

			c.Character.Field.Npcs.Move(c.Character, p);
		}

		private void Handle_DropPickUpRequest(WvsGameClient c, CInPacket p)
		{
			var bFieldKey = p.Decode1();
			var nTickCount = p.Decode4();
			var x = p.Decode2();
			var y = p.Decode2();
			var dwDropId = p.Decode4();
			var dwCliCrc = p.Decode4();

			c.Character.Field.Drops.PickUp(c.Character, dwDropId, x, y);
		}

		private void Handle_ReactorHit(WvsGameClient c, CInPacket p)
			=> c.Character.Field.Reactors.OnPacket(RecvOps.CP_ReactorHit, p, c.Character);

		private void Handle_ReactorTouch(WvsGameClient c, CInPacket p)
			=> c.Character.Field.Reactors.OnPacket(RecvOps.CP_ReactorTouch, p, c.Character);

		private void Handle_RequireFieldObstacleStatus(WvsGameClient c, CInPacket p)
		{
			//TODO: Maps with obstacles
		}

		private void Handle_INVITE_PARTY_MATCH(WvsGameClient c, CInPacket p)
		{
			//CWvsContext::SendPartyWanted

			int dwMinLv = p.Decode4();
			int dwMaxLv = p.Decode4();
			int dwCount = p.Decode4();
			int dwJobFlag = p.Decode4();
		}

		private void Handle_CANCEL_INVITE_PARTY_MATCH(WvsGameClient c, CInPacket p)
		{
			//Stop Search / Pause Search
		}

		private void Handle_MapleTVSendMessageRequest(WvsGameClient c, CInPacket p)
		{
			c.Character.Action.FeatureNotAddedMessage($"MapleTVSendMessageRequest");
		}

		private void Handle_AcceptAPSPEvent(WvsGameClient c, CInPacket p)
		{
			var dwAccountId = p.Decode4(); //Idk why they use this but we shouldnt rely on it lol
			var nResult = p.Decode4(); //Dialog result ( should be 6 always )

			if (nResult == 6)
			{
				//Click the 'OK' button and the AP/SP reset item will be sent to your character's cash locker
			}

			c.Character.Action.FeatureNotAddedMessage($"AcceptAPSPEvent");
		}

		private void Handle_UserDragonBallBoxRequest(WvsGameClient c, CInPacket p)
		{
			//No Data | CWvsContext::SendDragonBallBoxRequest
			c.Character.Action.FeatureNotAddedMessage($"UserDragonBallBoxRequest");
		}

		private void Handle_UserDragonBallSummonRequest(WvsGameClient c, CInPacket p)
		{
			//No Data | CWvsContext::SendDragonBallSummonRequest
			c.Character.Action.FeatureNotAddedMessage($"UserDragonBallSummonRequest");
		}

		private void Handle_UpdateScreenSetting(WvsGameClient c, CInPacket p)
		{
			//CONFIG_SYSOPT
			var bSysOpt_LargeScreen = p.Decode1();
			var bSysOpt_WindowedMode = p.Decode1();

			//Change the Map Split size accordingly
		}

		// CMob::OnApplyCtrl
		private void Handle_MobApplyCtrl(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;

			var dwMobID = p.Decode4();
			var nCtrlPriority = p.Decode2(); // TODO do something with this

			var pMob = c.Character.Field.Mobs[dwMobID];

			pMob?.SetController(c.Character, MobCtrlType.Active_Req);

			//c.Character.Field.ChangeMobController(c.Character, pMob, true);
		}

		private void Handle_MobSkillDelayEnd(WvsGameClient c, CInPacket p)
		{
			int dwMobID = p.Decode4();
			int nSkillID = p.Decode4();
			int nSLV = p.Decode4();
			int nOption = p.Decode4();

			var item = c.Character.Field.Mobs[dwMobID];

			item?.DoSkill(nSkillID, nSLV, nOption);
		}

		private void Handle_MapleTVUpdateViewCount(WvsGameClient c, CInPacket p)
		{
			var sFlashName = p.DecodeString();
			var unk1 = p.Decode4();
			var unk2 = p.Decode4();
		}

		private void Handle_UserScriptItemUseRequest(WvsGameClient c, CInPacket p)
		{
			// *Miracle cube fragment:
			// *Recv[CP_UserScriptItemUseRequest][54 00] 52 16 E4 00[06 00][A0 14 25 00]
			if (c.Character.Stats.nHP <= 0) return;

			if (p.Available < 10) return;

			var timestamp = p.Decode4(); // timestamp?
			var itemSlot = p.Decode2();
			var itemId = p.Decode4();

			if (InventoryManipulator.GetAnyItem(c.Character, itemId).Item2 is GW_ItemSlotBase isb
				&& isb.Template is ConsumeItemTemplate template)
			{
				if (template.ItemScript.Length <= 0) return;

				ServerApp.Container
					.Resolve<ScriptManager>()
					.GetItemScript(template.ItemScript, itemId, itemSlot, c)
					.Execute();
			}

			c.Character.Action.Enable();
		}

		private void Handle_ItemUpgradeComplete(WvsGameClient c, CInPacket p)
		{
			//*This packet is received when a gold hammer upgrade is complete
			//* Recv[CP_ItemUpgradeComplete][28 01][00][00 00 00 00] 00 00 00

			// validate packet length
			if (p.Available != 8)
				return;

			var outp = new COutPacket(SendOps.LP_ItemUpgradeResult);
			outp.Encode1(2); // gold hammer done
			outp.Encode4(p.Decode4());
			//outp.Encode4(p.Decode4());

			c.SendPacket(outp);
			c.Character.Action.Enable();
		}

		private void Handle_GuildBBS(WvsGameClient c, CInPacket p)
		{
			var operation = p.Decode1();

			switch (operation)
			{
				// TODO
			}

			c.Character.Action.FeatureNotAddedMessage($"GuildBBS");
		}

		public void Handle_RequestClassCompetitionAuthKey(WvsGameClient c, CInPacket p)
		{
			c.Character.Action.FeatureNotAddedMessage($"RequestClassCompetitionAuthKey");

		}

		/**
         * Alliance packet handler. Sends one of these after
         * a guild is created as well o.o
         */
		private void Handle_AllianceRequest(WvsGameClient c, CInPacket p)
		{
			var opCode = p.Decode1();

			switch ((AllianceOps)opCode)
			{
				case AllianceOps.AllianceReq_Load:
					//c.Character.PlayerGuild.AllianceReq_Load(c.Character);
					break;
			}
		}

		private void Handle_SkillCancelRequest(WvsGameClient c, CInPacket p)
		{
			var nSkillID = p.Decode4();

			c.Character.Buffs.UserTryCancelBuff(nSkillID);
		}

		// wheel of destiny
		private void Handle_UserUpgradeTombEffect(WvsGameClient c, CInPacket p)
		{
			var itemid = p.Decode4();

			var itempos = c.Character.InventoryCash.FindItemSlot(itemid); // COutPacket::Encode4(&oPacket, 5510000u);

			if (itempos > 0 && ItemConstants.is_fieldtype_upgradetomb_usable(c.Character.Field.Template.FieldType, c.Character.Field.MapId))
			{
				InventoryManipulator.RemoveFrom(c.Character, InventoryType.Cash, itempos);

				var x = p.Decode4();
				var y = p.Decode4();

				c.Character.Modify.Heal(c.Character.BasicStats.nMHP, c.Character.BasicStats.nMMP);

				// TODO put this packet somewhere else

				var oPacket = new COutPacket(SendOps.LP_UserShowUpgradeTombEffect);

				oPacket.Encode4(c.Character.dwId);
				oPacket.Encode4(itemid);
				oPacket.Encode4(x);
				oPacket.Encode4(y);

				c.Character.Field.Broadcast(oPacket, c);
			}

			c.Character.Action.Enable();
		}

		// v95 - CPet::SendDropPickUpRequest
		private void Handle_PetDropPickUpRequest(WvsGameClient c, CInPacket p)
		{
			var liPetSN = p.Decode8();

			if (Constants.MULTIPET_ACTIVATED)
			{
				throw new NotImplementedException();
			}
			else
			{
				if (c.Character.Pets.Pets[0]?.liPetLockerSN != liPetSN) return;
			}

			// TODO more validation here

			var bUnk = p.Decode1(); // n[0] = v7[15].vfptr;
			var get_update_time = p.Decode4();

			var ptX = p.Decode2(); // maybe drop pos? -> prolly pet pos
			var ptY = p.Decode2();

			var dwID = p.Decode4(); // drop id

			var dwCliCRC = p.Decode4();

			var bPickupOthers = p.Decode1();
			var bSweepForDrop = p.Decode1();
			var bLongRange = p.Decode1();

			// TODO more validation

			if (dwID % 13 == 0) // TODO validate this
			{
				var petPosX = p.Decode2();
				var petPosY = p.Decode2();
				var dwPosCRC = p.Decode4();
				var dwRectCRC = p.Decode4();
			}

			c.Character.Field.Drops.PickUp(c.Character, dwID, ptX, ptY, true);
		}

		private void Handle_PetAction(WvsGameClient c, CInPacket p)
		{
			c.Character.Pets.OnPetPacket(RecvOps.CP_PetAction, p);
		}

		private void Handle_PetStatChangeItemUseRequest(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;

			// TODO check if active pet
			// TODO check if character has correct pet item equipped

			var liPetSN = p.Decode8();

			if (c.Character.Stats.aliPetLockerSN.Contains(liPetSN))
			{
				var bBuffSkill = p.Decode1();
				var tick = p.Decode4();
				var nPOS = p.Decode2();
				var nItemID = p.Decode4();

				c.Character.Action.ConsumeItem.UsePotion(nItemID, nPOS);
			}
		}

		private void Handle_ExpeditionRequest(WvsGameClient c, CInPacket p)
		{
			// [93 00] 31 D1 07 00 00

			if (c.Character.Stats.nHP <= 0) return;
			if (c.Character.Field.Template.HasMigrateLimit()) return;

			var op = p.Decode1();

			if (op == 0x31 || op == 0x53)
			{
				var questID = p.Decode1();//p.Decode4();

				switch (questID)
				{
					case 0xD2: // normal zakum
						c.Character.Action.SetField(211042300);
						break;
					case 0xD3: // normal horntail
						c.Character.Action.SetField(240050000); // TODO split chaos and normal horntail
						break;
					case 0xD4: // pink bean
						c.Character.Action.SetField(270050000);
						break;
					case 0xD5: // chaos zakum
						c.Character.Action.SetField(211042301);
						break;
					case 0xD6: // chaor horntail
						c.Character.Action.SetField(240050000);
						break;
					case 0xD0: // easy balrog
					case 0xD1: // normal balrog
					default:
						c.Character.SendMessage("This option has not been configured yet.");
						break;
				}
			}
		}

		private void Handle_PartyAdverRequest(WvsGameClient c, CInPacket p)
		{
			return;
			var nSubOpcode = p.Decode1();


			if (nSubOpcode == (byte)PartyOps.AdverReq_Add)
			{
				var v1 = p.Decode4(); //TabPartyAdver::GetSelectQuestID(v1);
									  //var v2 = p.DecodeString(); //This was in a conditional so IDK
			}
			switch (nSubOpcode)
			{
				case (byte)PartyOps.AdverReq_Remove:
					break;
				case (byte)PartyOps.AdverReq_GetAll:
					{
						var v1 = p.Decode4(); //TabPartyAdver::GetSelectQuestID(v1);
						break;
					}
				case (byte)PartyOps.AdverReq_RemoveUserFromNotiList:
					break;
				case (byte)PartyOps.AdverReq_Apply:
					{
						var nPartyID = p.Decode4();
						break;
					}
				case (byte)PartyOps.AdverReq_ResultApply:
					{
						var nResult = p.Decode4();
						var v1 = p.Decode4(); //Comes from a packet we send to client
						break;
					}
					//No more data
			}

			c.Character.Action.FeatureNotAddedMessage($"PartyAdverRequest");
		}

		// BMS: CField_SnowBall::OnSnowBallTouch(CUser *pUser, CInPacket *iPacket)
		private void Handle_SnowBallTouch(WvsGameClient c, CInPacket p)
		{
			c.SendPacket(new COutPacket(SendOps.LP_SnowBallTouch)); // bounce off snowball
		}

		// BMS: CField_SnowBall::OnSnowBallHit(CField_SnowBall *this, CUser *pUser, CInPacket *iPacket)
		private void Handle_SnowBallHit(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Field is CField_Snowball field)
			{
				var nSide = p.Decode1();
				var nDamage = p.Decode2(); // TODO validate damage
				var tDelay = p.Decode2();

				if (nSide == 0 || nSide == 1)
				{
					if (nDamage != 10 && nDamage != 0) return;

					if (field.tSnowBallWaitTime[nSide].MillisSinceStart() > CField_Snowball.SNOWMAN_WAIT_DURATION_MILLIS)
					{
						if (field.bSnowBallStopped[nSide])
						{
							field.bSnowBallStopped[nSide] = false;

							var snowBallMovePacket = new COutPacket(SendOps.LP_SnowBallMsg);
							snowBallMovePacket.Encode1(nSide);
							snowBallMovePacket.Encode1(5);
							field.Broadcast(snowBallMovePacket);
						}
					}
					else
					{
						nDamage = 0;
					}
				}
				else
				{
					if (nDamage != 0 && !field.nDamageSnowMan.Contains(nDamage)) return;
				}

				var team = nSide % 2;
#if RELEASE
				if (!field.aaCharacterID[team].Contains(c.dwCharId)) return; // hax
#endif

				switch (nSide)
				{
					case 0:
						field.SnowBallHit(team, -nDamage);
						break;
					case 1:
						field.SnowBallHit(team, -nDamage);
						break;
					case 2:
						field.SnowManHit(team, nDamage);
						break;
					case 3:
#if DEBUG
						field.SnowManHit(team, 1000);
#else
						field.SnowManHit(team, nDamage);
#endif
						break;
					default:
						return;
				}

				var snowballResponsePacket = new COutPacket(SendOps.LP_SnowBallHit);
				snowballResponsePacket.Encode1(nSide);
				snowballResponsePacket.Encode2(nDamage);
				snowballResponsePacket.Encode2(tDelay);
				field.Broadcast(snowballResponsePacket);
			}
		}

		private void Handle_EventStart(WvsGameClient c, CInPacket p)
		{
			// this is an abstracted packet handler
			// it behaves differently based on which field type it's in

			// the below code is executed in the base class
			//if (pUser && pUser->m_nGradeCode & 1 && this->m_dwField / 1000000 % 100 == 9)
			//{
			//	v3 = this->m_pParentFieldSet;
			//	if (v3)
			//		CFieldSet::StartEvent(v3, pUser);
			//}
		}

		private void Handle_RaiseRefesh(WvsGameClient c, CInPacket p)
		{
			// CUIRaiseWndBase::OnCreate

			// Recv [CP_RaiseRefesh] [1B 01] [A5 6E]

			var nQuestID = p.Decode2();

			if (c.Character.Quests.Contains(nQuestID))
			{
				c.Character.Quests.SendUpdateQuestRecordMessage(nQuestID);
			}
		}

		private void Handle_RaiseUIState(WvsGameClient c, CInPacket p)
		{
			// CUIRaiseWndBase::~CUIRaiseWndBase
			// CUIRaiseWndBase::CUIRaiseWndBase

			// Recv [CP_RaiseUIState] [1C 01] [F9 64 40 00] [01]

			var nItemID = p.Decode4(); // item that client is trying to use
			var bOpen = p.Decode1() > 0; //

			if (bOpen)
			{
				if (InventoryManipulator.GetAnyItem(c.Character, nItemID).Item2 is GW_ItemSlotBundle isb)
				{
					if (isb.Template is EtcItemTemplate template)
					{
						if (!template.Quest) return;

						var questId = template.QuestID;

						if (!c.Character.Quests.Contains(questId))
						{
							c.Character.Quests.SendUpdateQuestRecordMessage((short)questId, "0");
						}
					}
				}
			}
		}

		private void Handle_RaiseIncExp(WvsGameClient c, CInPacket p)
		{
			// CUIRaiseWnd::SendPutItem

			// Recv [CP_RaiseIncExp] 1D 01 04 05 00 F9 64 40 00
			// Recv [CP_RaiseIncExp] [1D 01] [04] [05 00] [F9 64 40 00]

			var nItemTI = p.Decode1();
			var nSlotPosition = p.Decode2();
			var nItemID = p.Decode4();

			if (InventoryManipulator.GetItem(c.Character, (InventoryType)nItemTI, nSlotPosition)
				is GW_ItemSlotBundle incExpItem && incExpItem.Template is EtcItemTemplate incExpTemplate)
			{
				var expToAdd = incExpTemplate.Exp;

				if (InventoryManipulator.GetAnyItem(c.Character, nItemID).Item2
					is GW_ItemSlotBundle raiseItem && raiseItem.Template is EtcItemTemplate raiseTemplate)
				{
					if (!raiseTemplate.Quest) return;

					// this gets set when they open the raise UI
					if (!c.Character.Quests.Contains(raiseTemplate.QuestID)) return;

					// existing exp value
					var qrVal = Convert.ToInt32(c.Character.Quests[raiseTemplate.QuestID].sQRValue);

					// get required item index
					var currentLevel = ItemConstants.CUIRaiseWnd__GetLevel(
						qrVal,
						raiseTemplate.Exp,
						raiseTemplate.Grade);

					// max level achieved
					if (currentLevel == raiseTemplate.Grade) return;

					// some of them only have one item that is required for each level
					var maxIndex = Math.Min(raiseTemplate.ConsumeItem.Length - 1, currentLevel);

					// item that can be used based on the current exp
					var requiredItem = raiseTemplate.ConsumeItem[maxIndex];

					// validate item trying to be used in the correct item
					if (requiredItem != incExpItem.nItemID) return;

					if (expToAdd <= 0 && raiseTemplate.ConsumeItemExpGain.Length > currentLevel)
					{
						expToAdd = raiseTemplate.ConsumeItemExpGain[currentLevel];
					}

					if (expToAdd <= 0) return;

					c.Character.Quests.SendUpdateQuestRecordMessage((short)raiseTemplate.QuestID, (qrVal + expToAdd).ToString());

					InventoryManipulator.RemoveQuantity(c.Character, requiredItem, 1);
				}
			}

			// just incase we screwed something up
			c.Character.Action.Enable();
		}

		private void Handle_RaiseAddPiece(WvsGameClient c, CInPacket p)
		{
			// CUIRaisePieceWnd::SendPutItem
			var nItemTI = p.Decode4();
			var nSlotPosition = p.Decode2();
			var nItemID = p.Decode4();
		}
	}
}
