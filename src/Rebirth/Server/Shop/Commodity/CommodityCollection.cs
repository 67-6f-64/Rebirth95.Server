using Rebirth.Network;
using System;
using System.Collections.Generic;

namespace Rebirth.Server.Shop.Commodity
{
    public class CommodityCollection
    {
        private readonly List<CommodityData> _entries 
            = new List<CommodityData>();

        public void Add(CommodityData item) => _entries.Add(item);

        public void Encode(COutPacket p)
        {
            p.Encode2((short)_entries.Count);

            foreach (var item in _entries)
                item.Encode(p);
        }

        public static void Create(Action<CommodityCollection> action)
        {
            var ctx = new CommodityCollection();

            action?.Invoke(ctx);
        }
    }
}
