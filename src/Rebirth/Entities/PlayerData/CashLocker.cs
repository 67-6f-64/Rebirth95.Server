using Rebirth.Entities.Item;
using Rebirth.Network;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Entities.PlayerData
{
    public sealed class CashLocker : List<CashItemInfo>
    {
        public short SlotMax { get; set; }

        /**
         * This class represents the account cash item storage.
         * This class contains a list of cash item objects.
         */
        public CashLocker(short slotmax)
        {
            SlotMax = slotmax;
        }

        /**
         * Searches for a cash item in the locker based on serial number.
         * Returns item if found, otherwise null.
         */
        public CashItemInfo GetBySN(long uid)
            => this.FirstOrDefault(x => x.SN == uid || x.Item.liSN == uid);

        /**
         * Searches for an item based on its SN, not ItemID.
         * Returns whether or not the item exists in this account locker.
         */
        public bool Contains(long cashId)
            => GetBySN(cashId) != null;

        public void EncodeItem(CashItemInfo i, COutPacket p)
        {
            p.Encode8(i.SN);
            p.Encode4(i.dwAccountID);
            p.Encode4(i.dwCharacterID);
            p.Encode4(i.nItemID);
            p.Encode4(i.CommodityID);
            p.Encode2(i.Count);
            p.EncodeStringFixed(i.BuyerCharName, 13);
            p.EncodeDateTime(i.Item.tDateExpire);
            p.Encode4(i.nPaybackRate);
            p.Encode4(i.nDiscountRate);
        }
    }
}
