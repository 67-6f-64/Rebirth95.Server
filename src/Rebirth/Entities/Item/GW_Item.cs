using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using log4net;
using Rebirth.Common.Types;
using Rebirth.Provider.Template;
using Rebirth.Tools;

namespace Rebirth.Entities.Item
{
	public abstract class GW_ItemSlotBase
	{
		// keep logger in base class
		public static ILog Log = LogManager.GetLogger(typeof(GW_ItemSlotBundle));

		public AbstractItemTemplate Template => MasterManager.ItemTemplate(nItemID);
		public virtual short SlotMax => (short)Template.SlotMax; // this has to be set in order for inv to work properly
		public bool CashItem => liCashItemSN != 0;
		public bool NotSale => Template.NotSale;
		public int Price => Template.Price;
		public bool Only => Template.Only; // one of a kind, cant have multiple in inventory
		public int nItemID { get; set; }
		/// <summary>
		/// This is the unique serial number. No two items should have the same unless its a zero.
		/// </summary>
		public long liSN { get; set; }
		/// <summary>
		/// Cash item serial number. This is the indexer used for cash items in the wz files.
		/// </summary>
		public long liCashItemSN { get; set; }
		public DateTime tDateExpire { get; set; }
		public int dwInvItemId { get; set; }
		public short nNumber { get; set; }

		public InventoryType InvType { get; }
		public bool IsEquip => InvType is InventoryType.Equip;
		public bool IsRechargeable { get; }
		public bool IsArrow { get; }

		public abstract string DbInsertString(int dwCharId, short nPOS);
		public virtual int CreateDbSubKey(short nPOS) => Math.Abs(nPOS << (int)InvType);

		protected GW_ItemSlotBase(int nItemID)
		{
			this.nItemID = nItemID;
			InvType = ItemConstants.GetInventoryType(nItemID);
			IsRechargeable = ItemConstants.IsRechargeableItem(nItemID);
			IsArrow = ItemConstants.IsArrow(nItemID);
		}

		public virtual GW_ItemSlotBase DeepCopy()
		{
			var pItem = MemberwiseClone() as GW_ItemSlotBase;
			pItem.nItemID = nItemID;
			pItem.liSN = liSN > 0 ? DateTime.Now.Ticks : 0; // give it a new unique ID if we are copying it
			pItem.liCashItemSN = liCashItemSN;
			pItem.tDateExpire = tDateExpire;
			pItem.nNumber = nNumber;
			return pItem;
		}

		public virtual void RawEncode(COutPacket p, bool bFromCS = false)
		{
			p.Encode4(nItemID);

			var bCashItem = liCashItemSN != 0;

			p.Encode1(bCashItem);

			// ring equips have to have their unique ID sent instead of the cash ID
			// but not during the locker -> storage transaction in the cash shop
			// not pretty but it works ok
			if (!bFromCS && 
			    (ItemConstants.is_friendship_equip_item(nItemID)
			    || ItemConstants.is_couple_equip_item(nItemID)
			    || ItemConstants.is_wedding_ring_item(nItemID)))
			{
				p.Encode8(liSN);
			}
			else
			{
				if (bCashItem)
					p.Encode8(liCashItemSN);
			}

			p.EncodeDateTime(tDateExpire);
		}

		public virtual void RawDecode(CInPacket p)
		{
			nItemID = p.Decode4();

			var v3 = p.Decode1() != 0;

			if (v3)
				liCashItemSN = p.Decode8();

			tDateExpire = DateTime.FromFileTimeUtc(p.Decode8());
		}
	}
}