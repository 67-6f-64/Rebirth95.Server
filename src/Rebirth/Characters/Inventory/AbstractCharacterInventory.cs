using Rebirth.Characters.Modify;
using Rebirth.Entities.Item;
using Rebirth.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Inventory
{
	//COVARIANCE SO I CAN DOWNCAST THE DERIVED TYPES THANK YOU OXYSOFT I LOVE U NICK

	public abstract class AbstractCharacterInventory<TItem> : IEnumerable<KeyValuePair<short, TItem>> where TItem : GW_ItemSlotBase
	{
		private readonly Dictionary<short, TItem> Items;

		public InventoryType InventoryType { get; }

		public byte SlotLimit { get; set; }
		public int Count => Items.Count;
		public int SpaceAvailable() => SlotLimit - Count;

		protected AbstractCharacterInventory(InventoryType type)
		{
			InventoryType = type;
			Items = new Dictionary<short, TItem>();
		}

		public void Dispose()
		{
			Items.Clear();
		}

		/// <summary>
		/// Determines if the given inventory slot is within the range of 
		///		the inventory limit and does not have an item in it.
		/// </summary>
		/// <param name="nPOS">Slot to check.</param>
		/// <returns>True if the slot is free.</returns>
		public bool IsFreeSlot(short nPOS)
		{
			if (nPOS < 0) // equipped item
			{
				return !Items.ContainsKey(nPOS);
			}

			return !Items.ContainsKey(nPOS) && nPOS > 0 && nPOS <= SlotLimit;
		}

		/// <summary>
		/// Remove expired items from character inventory and notify client.
		/// </summary>
		/// <param name="pChar"></param>
		public int RemoveExpiredItems(Character pChar)
		{
			var toRemove = this
				.Where(item => item.Value.tDateExpire.MillisSinceStart() > 0)
				.ToArray();

			foreach (var slot in toRemove) // TODO verify this works
			{
				var pItem = Get(slot.Key);

				pChar.SendPacket(pItem.CashItem
					? CPacket.CashItemExpireMessage(pItem.nItemID)
					: CPacket.GeneralItemExpireMessage(pItem.nItemID));

				Remove(slot.Key);
			}

			return toRemove.Length;
		}

		public short GetFreeSlot()
		{
			for (short i = 1; i <= SlotLimit; i++)
			{
				if (!Contains(i))
				{
					return i;
				}
			}
			return 0; //Nothing can be at zero
		}

		/**
         * Finds item slot for specified item ID.
         * Returns 0 if not in inventory.
         */
		public short FindItemSlot(int itemID)
		{
			var (key, value) = Items.FirstOrDefault(i => i.Value.nItemID == itemID);

			return value == null ? (short)0 : key;
		}

		public short FindItemSlotBySN(long liSN)
		{
			var (key, value) = Items.FirstOrDefault(i => i.Value.liSN == liSN);

			return value is null ? (short)0 : key;
		}

		public short FindItemSlotByCashSN(long itemsn)
		{
			var retVal = Items
				.FirstOrDefault(i =>
					i.Value.liCashItemSN == itemsn
					|| i.Value.liSN == itemsn);

			return retVal.Value == null ? (short)0 : retVal.Key;
		}

		/**
         * Switches the items in two item slots.
         */
		public void Swap(short slotFrom, short slotTo)
		{
			// clone items
			TItem itemFrom = Items.GetValueOrDefault(slotFrom);
			TItem itemTo = Items.GetValueOrDefault(slotTo);

			// cant move nothing into something
			if (itemFrom == null)
				return;

			// clear slots
			Items.Remove(slotFrom);
			Items.Remove(slotTo);

			// add initiation item into slot
			Items.Add(slotTo, itemFrom);

			// replace initial slot with displaced item
			if (itemTo != null)
				Items.Add(slotFrom, itemTo);
		}

		/// <summary>
		/// Merges two item slots.
		/// Returns true if slot merged completely.
		/// Returns false if slot merged partially.
		/// No validation is done in this function.
		/// </summary>
		/// <param name="slotFrom"></param>
		/// <param name="slotTo"></param>
		/// <returns></returns>
		public bool Merge(short slotFrom, short slotTo)
		{
			// clone items
			TItem itemFrom = Items.GetValueOrDefault(slotFrom);
			TItem itemTo = Items.GetValueOrDefault(slotTo);

			// clear slots
			Items.Remove(slotFrom);
			Items.Remove(slotTo);

			// full merge
			if (itemFrom.nNumber <= (itemTo.SlotMax - itemTo.nNumber))
			{
				itemTo.nNumber += itemFrom.nNumber;

				Items.Add(slotTo, itemTo);

				return true;
			}

			// partial merge
			itemFrom.nNumber -= (short)(itemTo.SlotMax - itemTo.nNumber);
			itemTo.nNumber = itemTo.SlotMax;

			Items.Add(slotFrom, itemFrom);
			Items.Add(slotTo, itemTo);

			return false;
		}

		public bool CanMergeSlots(short slotFrom, short slotTo)
		{
			TItem itemFrom = Items.GetValueOrDefault(slotFrom);
			TItem itemTo = Items.GetValueOrDefault(slotTo);

			if (itemTo == null) return false; // both items need to exist

			if (itemFrom.nItemID != itemTo.nItemID) return false;
			if (itemFrom.CashItem) return false; // cant merge cash items
			if (itemFrom.IsEquip) return false; // no need to check slotTo
			if (itemFrom.IsRechargeable) return false; // no need to check slotTo

			return true;
		}

		public virtual void Add(short slot, TItem item)
		{
			Items.Add(slot, item);
		}

		public virtual bool Remove(short slot)
		{
			return Items.Remove(slot);
		}

		public TItem Get(short slot)
		{
			if (Items.ContainsKey(slot))
				return Items[slot];

			return default;
		}

		public KeyValuePair<short, TItem> GetKvp(short key)
		{
			if (Items.ContainsKey(key))
				return new KeyValuePair<short, TItem>(key, Items[key]);

			return default;
		}

		public bool Contains(short slot)
		{
			return Items.ContainsKey(slot);
		}

		public bool ContainsItem(int itemId)
		{
			return Items.FirstOrDefault(i => i.Value.nItemID == itemId).Value != null;
		}

		protected abstract void EncodeSlot(short nSlot, COutPacket p);

		public virtual void Encode(COutPacket p)
		{
			foreach (var (key, value) in this)
			{
				var nSlot = Math.Abs(key); //Confirm later

				if (nSlot >= 100 && nSlot <= 200)
				{
					nSlot -= 100; // client sends < -100 numbers but isnt expecting them back lol
				}

				EncodeSlot(nSlot, p);

				value.RawEncode(p);
			}
			EncodeSlot(0, p);
		}

		public IEnumerator<KeyValuePair<short, TItem>> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
