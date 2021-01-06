﻿namespace Rebirth.Server.Shop
{
	public enum CashItemReq : byte
	{
		// TODO split these up
	}

	public enum CashItemOps : byte
    {
        CashItemReq_WebShopOrderGetList = 0x0,
        CashItemReq_LoadLocker = 0x1,
        CashItemReq_LoadWish = 0x2,
        CashItemReq_Buy = 0x3,
        CashItemReq_Gift = 0x4,
        CashItemReq_SetWish = 0x5,
        CashItemReq_IncSlotCount = 0x6,
        CashItemReq_IncTrunkCount = 0x7,
        CashItemReq_IncCharSlotCount = 0x8,
        CashItemReq_IncBuyCharCount = 0x9,
        CashItemReq_EnableEquipSlotExt = 0xA,
        CashItemReq_CancelPurchase = 0xB,
        CashItemReq_ConfirmPurchase = 0xC,
        CashItemReq_Destroy = 0xD,
        CashItemReq_MoveLtoS = 0xE,
        CashItemReq_MoveStoL = 0xF,
        CashItemReq_Expire = 0x10,
        CashItemReq_Use = 0x11,
        CashItemReq_StatChange = 0x12,
        CashItemReq_SkillChange = 0x13,
        CashItemReq_SkillReset = 0x14,
        CashItemReq_DestroyPetItem = 0x15,
        CashItemReq_SetPetName = 0x16,
        CashItemReq_SetPetLife = 0x17,
        CashItemReq_SetPetSkill = 0x18,
        CashItemReq_SetItemName = 0x19,
        CashItemReq_SendMemo = 0x1A,
        CashItemReq_GetMaplePoint = 0x1B,
        CashItemReq_Rebate = 0x1C,
        CashItemReq_UseCoupon = 0x1D,
        CashItemReq_GiftCoupon = 0x1E,
        CashItemReq_Couple = 0x1F,
        CashItemReq_BuyPackage = 0x20,
        CashItemReq_GiftPackage = 0x21,
        CashItemReq_BuyNormal = 0x22,
        CashItemReq_ApplyWishListEvent = 0x23,
        CashItemReq_MovePetStat = 0x24,
        CashItemReq_FriendShip = 0x25,
        CashItemReq_ShopScan = 0x26,
        CashItemReq_LoadPetExceptionList = 0x27,
        CashItemReq_UpdatePetExceptionList = 0x28,
        CashItemReq_FreeCashItem = 0x29,
        CashItemReq_LoadFreeCashItem = 0x2A,
        CashItemReq_Script = 0x2B,
        CashItemReq_PurchaseRecord = 0x2C,
        CashItemReq_TradeDone = 0x2D,
        CashItemReq_BuyDone = 0x2E,
        CashItemReq_TradeSave = 0x2F,
        CashItemReq_TradeLog = 0x30,
        CashItemReq_EvolPet = 0x31,
        CashItemReq_BuyNameChange = 0x32,
        CashItemReq_CancelChangeName = 0x33,
        CashItemRes_CancelNameChangeFail = 0x34,
        CashItemReq_BuyTransferWorld = 0x35,
        CashItemReq_CancelTransferWorld = 0x36,

        CashItemReq_CharacterSale = 0x37,
        CashItemRes_CharacterSaleSuccess = 0x38,
        CashItemRes_CharacterSaleFail = 0x39,
        CashItemRes_CharacterSaleInvalidName = 0x3A,
        CashItemRes_CharacterSaleInvalidItem = 0x3B,

        // hammer 
        CashItemReq_ItemUpgrade = 0x3C,
        CashItemRes_ItemUpgradeSuccess = 0x3D,
        CashItemReq_ItemUpgradeFail = 0x3E,
        CashItemReq_ItemUpgradeReq = 0x3F,
        CashItemReq_ItemUpgradeDone = 0x40,
        CashItemRes_ItemUpgradeDone = 0x41,
        CashItemRes_ItemUpgradeErr = 0x42,

        CashItemReq_Vega = 0x43,
        CashItemRes_VegaSuccess1 = 0x44,
        CashItemRes_VegaSuccess2 = 0x45,
        CashItemRes_VegaErr = 0x46,
        CashItemRes_VegaErr2 = 0x47,
        CashItemRes_VegaErr_InvalidItem = 0x48,
        CashItemRes_VegaFail = 0x49,

        CashItemReq_CashItemGachapon = 0x4A,
        CashItemReq_CashGachaponOpen = 0x4B,
        CashItemReq_CashGachaponCopy = 0x4C,
        CashItemReq_ChangeMaplePoint = 0x4D,

        CashItemReq_CheckFreeCashItemTable = 0x4E,
        CashItemRes_CheckFreeCashItemTable_Done = 0x4F,
        CashItemRes_CheckFreeCashItemTable_Failed = 0x50,

        CashItemReq_SetFreeCashItemTable = 0x51,
        CashItemRes_SetFreeCashItemTable_Done = 0x52,
        CashItemRes_SetFreeCashItemTable_Failed = 0x53,

        CashItemRes_LimitGoodsCount_Changed = 0x54, // CCashShop::OnCashItemResLimitGoodsCountChanged

        CashItemRes_WebShopOrderGetList_Done = 0x55,
        CashItemRes_WebShopOrderGetList_Failed = 0x56,
        CashItemRes_WebShopReceive_Done = 0x57,

        CashItemRes_LoadLocker_Done = 0x58, // CCashShop::OnCashItemResLoadLockerDone
        CashItemRes_LoadLocker_Failed = 0x59, // CCashShop::OnCashItemResLoadLockerFailed

        CashItemRes_LoadGift_Done = 0x5A, // CCashShop::OnCashItemResLoadGiftDone
        CashItemRes_LoadGift_Failed = 0x5B, // CCashShop::OnCashItemResLoadGiftFailed

        CashItemRes_LoadWish_Done = 0x5C, // CCashShop::OnCashItemResLoadWishDone
        CashItemRes_LoadWish_Failed = 0x5D, // CCashShop::OnCashItemResLoadWishFailed

        CashItemRes_MapleTV_Failed_Wrong_User_Name = 0x5E,
        CashItemRes_MapleTV_Failed_User_Not_Connected = 0x5F,

        CashItemRes_AvatarMegaphone_Queue_Full = 0x60,
        CashItemRes_AvatarMegaphone_Level_Limit = 0x61,

        CashItemRes_SetWish_Done = 0x62,
        CashItemRes_SetWish_Failed = 0x63,

        CashItemRes_Buy_Done = 0x64,
        CashItemRes_Buy_Failed = 0x65,

        CashItemRes_UseCoupon_Done = 0x66,
        CashItemRes_UseCoupon_Done_NormalItem = 0x67,
        CashItemRes_GiftCoupon_Done = 0x68,
        CashItemRes_UseCoupon_Failed = 0x69,
        CashItemRes_UseCoupon_CashItem_Failed = 0x6A,

        CashItemRes_Gift_Done = 0x6B,
        CashItemRes_Gift_Failed = 0x6C,

        CashItemRes_IncSlotCount_Done = 0x6D,
        CashItemRes_IncSlotCount_Failed = 0x6E,

        CashItemRes_IncTrunkCount_Done = 0x6F,
        CashItemRes_IncTrunkCount_Failed = 0x70,

        CashItemRes_IncCharSlotCount_Done = 0x71,
        CashItemRes_IncCharSlotCount_Failed = 0x72,

        CashItemRes_IncBuyCharCount_Done = 0x73,
        CashItemRes_IncBuyCharCount_Failed = 0x74,

        CashItemRes_EnableEquipSlotExt_Done = 0x75,
        CashItemRes_EnableEquipSlotExt_Failed = 0x76,

        CashItemRes_MoveLtoS_Done = 0x77,
        CashItemRes_MoveLtoS_Failed = 0x78,

        CashItemRes_MoveStoL_Done = 0x79,
        CashItemRes_MoveStoL_Failed = 0x7A,

        CashItemRes_Destroy_Done = 0x7B,
        CashItemRes_Destroy_Failed = 0x7C,

        CashItemRes_Expire_Done = 0x7D,
        CashItemRes_Expire_Failed = 0x7E,

        CashItemRes_Use_Done = 0x7F,
        CashItemRes_Use_Failed = 0x80,

        CashItemRes_StatChange_Done = 0x81,
        CashItemRes_StatChange_Failed = 0x82,

        CashItemRes_SkillChange_Done = 0x83,
        CashItemRes_SkillChange_Failed = 0x84,

        CashItemRes_SkillReset_Done = 0x85,
        CashItemRes_SkillReset_Failed = 0x86,

        CashItemRes_DestroyPetItem_Done = 0x87,
        CashItemRes_DestroyPetItem_Failed = 0x88,

        CashItemRes_SetPetName_Done = 0x89,
        CashItemRes_SetPetName_Failed = 0x8A,

        CashItemRes_SetPetLife_Done = 0x8B,
        CashItemRes_SetPetLife_Failed = 0x8C,

        CashItemRes_MovePetStat_Failed = 0x8D,
        CashItemRes_MovePetStat_Done = 0x8E,

        CashItemRes_SetPetSkill_Failed = 0x8F,
        CashItemRes_SetPetSkill_Done = 0x90,

        CashItemRes_SendMemo_Done = 0x91,
        CashItemRes_SendMemo_Warning = 0x92,
        CashItemRes_SendMemo_Failed = 0x93,

        CashItemRes_GetMaplePoint_Done = 0x94,
        CashItemRes_GetMaplePoint_Failed = 0x95,

        CashItemRes_Rebate_Done = 0x96,
        CashItemRes_Rebate_Failed = 0x97,

        CashItemRes_Couple_Done = 0x98,
        CashItemRes_Couple_Failed = 0x99,

        CashItemRes_BuyPackage_Done = 0x9A,
        CashItemRes_BuyPackage_Failed = 0x9B,

        CashItemRes_GiftPackage_Done = 0x9C,
        CashItemRes_GiftPackage_Failed = 0x9D,

        CashItemRes_BuyNormal_Done = 0x9E,
        CashItemRes_BuyNormal_Failed = 0x9F,

        CashItemRes_ApplyWishListEvent_Done = 0xA0,
        CashItemRes_ApplyWishListEvent_Failed = 0xA1,

        CashItemRes_Friendship_Done = 0xA2,
        CashItemRes_Friendship_Failed = 0xA3,

        CashItemRes_LoadExceptionList_Done = 0xA4,
        CashItemRes_LoadExceptionList_Failed = 0xA5,

        CashItemRes_UpdateExceptionList_Done = 0xA6,
        CashItemRes_UpdateExceptionList_Failed = 0xA7,

        CashItemRes_LoadFreeCashItem_Done = 0xA8,
        CashItemRes_LoadFreeCashItem_Failed = 0xA9,

        CashItemRes_FreeCashItem_Done = 0xAA,
        CashItemRes_FreeCashItem_Failed = 0xAB,

        CashItemRes_Script_Done = 0xAC,
        CashItemRes_Script_Failed = 0xAD,

        CashItemRes_Bridge_Failed = 0xAE,

        CashItemRes_PurchaseRecord_Done = 0xAF,
        CashItemRes_PurchaseRecord_Failed = 0xB0,

        CashItemRes_EvolPet_Failed = 0xB1,
        CashItemRes_EvolPet_Done = 0xB2,

        CashItemRes_NameChangeBuy_Done = 0xB3,
        CashItemRes_NameChangeBuy_Failed = 0xB4,

        CashItemRes_TransferWorld_Done = 0xB5,
        CashItemRes_TransferWorld_Failed = 0xB6,

        CashItemRes_CashGachaponOpen_Done = 0xB7,
        CashItemRes_CashGachaponOpen_Failed = 0xB8,

        CashItemRes_CashGachaponCopy_Done = 0xB9,
        CashItemRes_CashGachaponCopy_Failed = 0xBA,

        CashItemRes_ChangeMaplePoint_Done = 0xBB,
        CashItemRes_ChangeMaplePoint_Failed = 0xBC,

        CashItemReq_Give = 0xBD,
        CashItemRes_Give_Done = 0xBE,
        CashItemRes_Give_Failed = 0xBF,

        CashItemRes_GashItemGachapon_Failed = 0xC0,
        CashItemRes_CashItemGachapon_Done = 0xC1,
    }
}
