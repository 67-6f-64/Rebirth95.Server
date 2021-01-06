using System;

namespace Rebirth.Common.Types
{
    [Flags]
    public enum ItemAttributeFlags : short
    {
        Lock = 0x01,
        Spikes = 0x02,
        Cold = 0x03,
        Untradeable = 0x04,
        Karma_Eq = 0x08,
        Karma_Use = 0x02,
        Charm_Equipped = 0x20,
        Crafted = 0x80,
        Crafted_Use = 0x10
        // are there more for v95??
    }
}
