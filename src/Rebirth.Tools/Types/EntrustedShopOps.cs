namespace Rebirth.Common.Types
{
    public enum EntrustedShopOps
    {
        EntrustedShopReq_CheckOpenPossible = 0x0, // try to open shop
        EntrustedShopReq_Create = 0x1,
        EntrustedShopReq_Save = 0x2,
        EntrustedShopReq_CloseProcess = 0x3,
        EntrustedShopReq_FindShopByEmployerName = 0x4,
        EntrustedShopReq_CheckIfClosed = 0x5,
        EntrustedShopReq_GetPos = 0x6,

        EntrustedShopRes_OpenPossible = 0x7,
        EntrustedShopRes_OpenImpossible_Using = 0x8,
        EntrustedShopRes_OpenImpossible_Stored = 0x9,
        EntrustedShopRes_OpenImpossible_AnotherCharacter = 0xA,
        EntrustedShopRes_OpenImpossible_Block = 0xB,
        EntrustedShopRes_Create_Failed = 0xC,

        EntrustedShopReq_SetMiniMapColor = 0xD,
        EntrustedShopReq_RenameResult = 0xE,

        EntrustedShopRes_ItemExistInStoreBank = 0xF,
        EntrustedShopRes_GetPosResult = 0x10,
        EntrustedShopRes_Enter = 0x11,
        EntrustedShopRes_ServerMsg = 0x12,
    }
}
