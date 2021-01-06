// thx swordie

namespace Rebirth.Common.Types
{
    public enum BroadcastMsgType : byte
    {
        // BM_ALL = 0
        // BM_NOTICE = 0
        // BM_CLONE = 1
        // BM_ALERT = 1
        // BM_MAP = 2
        // BM_SPEAKERCHANNEL = 2
        // BM_SPEAKERWORLD = 3
        // BM_SLIDE = 4
        // BM_EVENT = 5
        // BM_NOTICEWITHOUTPREFIX = 6
        // BM_UTILDLGEX = 7
        // BM_ITEMSPEAKER = 8
        // BM_SPEAKERBRIDGE = 9
        // BM_ARTSPEAKERWORLD = 0Ah
        // BM_BLOWWEATHER = 0Bh
        // BM_GACHAPONANNOUNCE = 0Ch
        // BM_GACHAPONANNOUNCE_OPEN = 0Dh
        // BM_GACHAPONANNOUNCE_COPY = 0Eh
        // BM_ULISTCLIP = 0Fh
        // BM_FREEMARKETCLIP = 10h
        // BM_DESTROYSHOP = 11h
        // BM_CASHSHOPAD = 12h
        // BM_HEARTSPEAKER = 13h
        // BM_SKULLSPEAKER = 14h
        Notice = 0x00, // [Notice] <message>
        PopupMessage = 0x01,
        SpeakerChannel = 0x02,
        SpeakerWorld = 0x03,
        ScrollingMessage = 0x04,
        PartyChat = 0x05, // pink text
        BlueChat_ItemInfo = 0x06, // light blue text
        AdminErrorMessage = 0x07,
        ItemMegaphone = 0x08,
        SpeakerBridge = 0x09,
        ArtSpeakerWorld = 0x0A,
        YellowChat_ItemInfo = 0x0B,
        GachaponAnnounce = 0xC,
        GachaponAnnounce_Open = 0xD,
        GachaponAnnounce_Copy = 0xE,
        UListClip = 0xF,
        FreeMarketClip = 0x10,
        DestroyShop = 0x11,
        CashShopAd = 0x12,

        HeartSpeaker = 0x13,
		SkullSpeaker = 0x14
    }
}
