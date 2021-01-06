using Rebirth.Network;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Server.Center;

namespace Rebirth.Entities.Shop
{
	public class CShopItem
	{
		public AbstractItemTemplate Template { get; }
		public int nItemID { get; private set; }
		public int nPrice { get; set; }
		public byte nDiscountRate { get; set; }
		public int nTokenItemID { get; set; }
		public int nTokenPrice { get; set; }
		public int nItemPeriod { get; set; }
		public int nLevelLimited { get; set; }
		public double dUnitPrice { get; set; }
		public short nMaxPerSlot { get; set; }
		public short nQuantity { get; set; }

		public CShopItem(int nItemId)
		{
			Template = MasterManager.ItemTemplate(nItemId);
			nItemID = nItemId;
			nMaxPerSlot = (short)Template.SlotMax;

			if (ItemConstants.IsArrow(nItemId)
			    || ItemConstants.IsBullet(nItemId) || ItemConstants.IsThrowingStar(nItemId))
			{
				nQuantity = nMaxPerSlot;
			}
			else
			{
				nQuantity = 1;
			}

			if (Template is ConsumeItemTemplate consumeTemplate)
			{
				dUnitPrice = consumeTemplate.UnitPrice;
			}
		}

		public CShopItem(int nItemId, int nPrice, short nQuantity = 1)
		{
			Template = MasterManager.ItemTemplate(nItemId);
			nItemID = nItemId;
			nMaxPerSlot = (short)Template.SlotMax;

			this.nPrice = nPrice;
			this.nQuantity = ItemConstants.IsArrow(nItemId)
					|| ItemConstants.IsBullet(nItemId)
					|| ItemConstants.IsThrowingStar(nItemId)
									? nMaxPerSlot
									: nQuantity;

			if (Template is ConsumeItemTemplate consumeTemplate)
			{
				dUnitPrice = consumeTemplate.UnitPrice;
			}
		}

		public void Encode(COutPacket p)
		{
			p.Encode4(nItemID);
			p.Encode4(nPrice);
			p.Encode1(nDiscountRate);
			p.Encode4(nTokenItemID);
			p.Encode4(nTokenPrice);
			p.Encode4(nItemPeriod);
			p.Encode4(nLevelLimited);

			if (ItemConstants.IsRechargeableItem(nItemID))
			{
				p.Encode8(dUnitPrice);
			}
			else
			{
				p.Encode2(nQuantity);
			}
			p.Encode2(nMaxPerSlot);
		}
	}
}
