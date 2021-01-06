using System;

namespace Rebirth.Common.Types
{
	[Flags]
    public enum PetAbilityFlag
    {
        Pickup_Meso = 0x1,
        Pickup_Item = 0x2,
        Pickup_Others = 0x4,
        Pickup_LongRange = 0x8,
        Pickup_SweepForDrop = 0x10,
        ConsumeHP = 0x20,
        ConsumeMP = 0x40,
        IgnoreItems = 0x80
    }
}
