namespace Rebirth.Common.Types
{
    public enum InventoryType : byte
    {
        Equip = 0x1,
        Consume = 0x2,
        Install = 0x3,
        Etc = 0x4,
        Cash = 0x5,
        Equipped = 0x6, // This isnt used by the game. WE use it.
        Special = 0x9,

		DragonEquipped = 0x10,
		MechanicEquipped = 0x11,
    }
}
