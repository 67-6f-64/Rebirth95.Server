namespace Rebirth.Common.Types
{
    [System.Flags]
    public enum DbCharFlags
    {
        CHARACTER = 0x1,
        MONEY = 0x2,
        ITEMSLOTEQUIP = 0x4,
        ITEMSLOTCONSUME = 0x8,
        ITEMSLOTINSTALL = 0x10,
        ITEMSLOTETC = 0x20,
        ITEMSLOTCASH = 0x40,
        INVENTORYSIZE = 0x80,
        SKILLRECORD = 0x100,
        QUESTRECORD = 0x200,
        MINIGAMERECORD = 0x400,
        COUPLERECORD = 0x800,
        MAPTRANSFER = 0x1000,
        AVATAR = 0x2000,
        QUESTCOMPLETE = 0x4000,
        SKILLCOOLTIME = 0x8000,
        MONSTERBOOKCARD = 0x10000,
        MONSTERBOOKCOVER = 0x20000,
        NEWYEARCARD = 0x40000,
        QUESTRECORDEX = 0x80000,
        ADMINSHOPCOUNT = 0x100000,
        EQUIPEXT = 0x100000,
        WILDHUNTERINFO = 0x200000,
        QUESTCOMPLETE_OLD = 0x400000,
        VISITORLOG = 0x800000,
        VISITORLOG1 = 0x1000000,
        VISITORLOG2 = 0x2000000,
        VISITORLOG3 = 0x4000000,
        VISITORLOG4 = 0x8000000,
        ALL = -1,
        ITEMSLOT = 0x7C,
    }
}
