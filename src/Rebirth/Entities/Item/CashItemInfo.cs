using Rebirth.Characters;
using Rebirth.Network;
using System;

namespace Rebirth.Entities.Item
{
	public class CashItemInfo
	{
		public int dwAccountID { get; set; }
		public int dwCharacterID { get; set; }
		public int nPaybackRate { get; set; }
		public int nDiscountRate { get; set; }
		public int CommodityID { get; set; }
		public int Period { get; set; }
		public int NXCost { get; set; }

		/// <summary>
		/// Nexon's cash item ID. Used to index items in the WZ files.
		/// </summary>
		public long SN => Item.liCashItemSN;
		public int nItemID => Item.nItemID;
		public short Count => Item.nNumber;

		// gift stuff
		public string BuyerCharName { get; set; }
		public string GiftMessage { get; set; }
		public GW_ItemSlotBase Item { get; set; }

		public CashItemInfo(GW_ItemSlotBase item, int period, int price, int commodityId)
		{
			BuyerCharName = string.Empty;
			Item = item;
			Period = period;
			NXCost = price;
			CommodityID = commodityId;

			if (Period <= 0) Period = 90;

			Item.tDateExpire = DateTime.Now.AddDays(Period);
		}

		public void Encode(COutPacket p)
		{
			p.Encode8(SN);
			p.Encode4(dwAccountID);
			p.Encode4(dwCharacterID); // dwCharacterID
			p.Encode4(nItemID);
			p.Encode4(CommodityID);
			p.Encode2(Item.nNumber);
			p.EncodeStringFixed(BuyerCharName, 13); // sender
			p.EncodeDateTime(Item.tDateExpire);
			p.Encode4(nPaybackRate);
			p.Encode4(nDiscountRate);
		}
	}
}
