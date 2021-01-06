namespace Rebirth.Common.Types
{
    [System.Flags]
    public enum ForcedStatFlags
    {
        AUTO = 0x0,
        STR = 0x1,
        DEX = 0x2,
        INT = 0x4,
        LUK = 0x8,
        PAD = 0x10,
        PDD = 0x20,
        MAD = 0x40,
        MDD = 0x80,
        ACC = 0x100,
        EVA = 0x200,
        SPEED = 0x400,
        JUMP = 0x800,
        SPEEDMAX = 0x1000,
        ALL = 0x1FFF,
    };
}
