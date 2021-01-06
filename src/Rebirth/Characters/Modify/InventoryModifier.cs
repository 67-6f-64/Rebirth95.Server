using Rebirth.Entities.Item;
using Rebirth.Network;
using System.Collections.Generic;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Modify
{
	/// <summary>
	/// BMS -- CInventoryManipulator::InsertChangeLog
	/// </summary>
	public class InventoryModifier
	{
		private readonly List<AbstractInventoryOperation> aChangeLog; // we dont want this directly accessed outside this class

		/// <summary>
		/// Returns true if there are more than zero inventory operations queued.
		/// </summary>
		public bool ContainsOperations => aChangeLog.Count > 0;
		public int Available { get; private set; }
		public bool Broadcast_UserAvatarModified { get; private set; }

		private Character parent;

		public InventoryModifier(Character c)
		{
			parent = c;
			aChangeLog = new List<AbstractInventoryOperation>();
		}

		public void Add(InventoryType inventory, short slot, GW_ItemSlotBase item)
		{
			aChangeLog.Add(new InventoryOperationAdd(inventory, slot, item));

			if (slot < 0 && slot > -200)
				Broadcast_UserAvatarModified = true;
		}

		public void UpdateQuantity(InventoryType inventory, short slot, short quantity)
		{
			aChangeLog.Add(new InventoryOperationUpdateQuantity(inventory, slot, quantity));
		}

		public void Move(InventoryType inventory, short slot, short newSlot)
		{
			aChangeLog.Add(new InventoryOperationMove(inventory, slot, newSlot));

			if (slot < 0 || newSlot < 0)
				Broadcast_UserAvatarModified = true;
		}

		public void Remove(InventoryType inventory, short slot)
		{
			aChangeLog.Add(new InventoryOperationRemove(inventory, slot));

			if (slot < 0)
				Broadcast_UserAvatarModified = true;
		}

		public void UpdateEXP(InventoryType inventory, short slot, int EXP)
		{
			aChangeLog.Add(new InventoryOperationUpdateEXP(inventory, slot, EXP));

			if (slot < 0)
				Broadcast_UserAvatarModified = true;
		}

		/// <summary>
		/// Notify client that one stack of items has been combined with another stack of items.
		/// </summary>
		/// <param name="inventory"></param>
		/// <param name="slotFrom"></param>
		/// <param name="slotTo"></param>
		/// <param name="nAmountTo"></param>
		public void FullMerge(InventoryType inventory, short slotFrom, short slotTo, short nAmountTo)
		{
			aChangeLog.Add(new InventoryOperationRemove(inventory, slotFrom));
			aChangeLog.Add(new InventoryOperationUpdateQuantity(inventory, slotTo, nAmountTo));
		}

		/// <summary>
		/// Notify client that one stack of items has been partially combined with another stack of items.
		/// Contains two UpdateQuantity operations.
		/// </summary>
		/// <param name="inventory"></param>
		/// <param name="slotFrom"></param>
		/// <param name="slotTo"></param>
		/// <param name="nAmountFrom"></param>
		/// <param name="nAmountTo"></param>
		public void PartialMerge(InventoryType inventory, short slotFrom, short slotTo, short nAmountFrom, short nAmountTo)
		{
			aChangeLog.Add(new InventoryOperationUpdateQuantity(inventory, slotFrom, nAmountFrom));
			aChangeLog.Add(new InventoryOperationUpdateQuantity(inventory, slotTo, nAmountTo));
		}

		/// <summary>
		/// Notifies the client that an equip has changed stats.
		/// Contains one remove operation and one add operation.
		/// </summary>
		/// <remarks>This is how Nexon does it as well. (You'd think they'd have another UpdateEquip op lol)</remarks>
		public void UpdateEquipInformation(GW_ItemSlotEquip pItemEquip, short nPOS)
		{
			aChangeLog.Add(new InventoryOperationRemove(InventoryType.Equip, nPOS));
			aChangeLog.Add(new InventoryOperationAdd(InventoryType.Equip, nPOS, pItemEquip));
		}

		public void UpdatePetItem(GW_ItemSlotPet pItemPet, short nPOS)
		{
			aChangeLog.Add(new InventoryOperationRemove(InventoryType.Cash, nPOS));
			aChangeLog.Add(new InventoryOperationAdd(InventoryType.Cash, nPOS, pItemPet));
		}

		public void Encode(COutPacket p)
		{
			p.Encode1((byte)aChangeLog.Count);

			var bAddMovementInfo = true;

			foreach (var operation in aChangeLog)
			{
				operation.Encode(p);
			}

			if (bAddMovementInfo)
			{
				p.Encode1(0); // CVecCtrlUser::AddMovementInfo(v3->m_pvc.p, &v3->m_secondaryStat, &v3->m_character, 0);
			}

			return;

			// below is proper handling but since we arent using it ima just pad the packet
			if (aChangeLog.Count > 0)
			{
				switch (aChangeLog[aChangeLog.Count - 1])
				{
					case InventoryOperationMove moveOp:
						if (moveOp.nTI == InventoryType.Equip && (moveOp.nPOS > 0 || moveOp._newSlot < 0))
						{
							p.Encode1(0);
						}
						break;
					case InventoryOperationRemove removeOp:
						if (removeOp.nTI == InventoryType.Equip && removeOp.nPOS < 0)
						{
							p.Encode1(0);
						}
						break;
				}
			}
		}
	}
}