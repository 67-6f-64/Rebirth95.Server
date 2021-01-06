using Rebirth.Common.Types;

namespace Rebirth.Entities.Item
{
    public class TempItem
    {
        public GW_ItemSlotBase Item { get; }
        public int Price { get; }
        public short ItemsInBundle { get; }
        public short NumberOfBundles { get; set; }

        public short TargetSlot { get; set; }
        public short OriginalSlot { get; }

        public InventoryType Type { get; }

        public TempItem(GW_ItemSlotBase item, int price, short itemsinbundle, short numberofbundles)
        {
            Item = item;
            Price = price;
            ItemsInBundle = itemsinbundle;
            NumberOfBundles = numberofbundles;
            TargetSlot = -1;
            OriginalSlot = -1;
        }

        public TempItem(GW_ItemSlotBase pItem, short nCurInvSlot, short nShopSlot = -1)
        {
            Item = pItem;
            TargetSlot = nShopSlot;
            OriginalSlot = nCurInvSlot;
            Type = pItem.InvType;
        }
    }
}
