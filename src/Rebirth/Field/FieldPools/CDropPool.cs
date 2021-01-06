using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Skill;
using Rebirth.Field.FieldObjects;
using System;
using System.Collections.Generic;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Item.Cash;

namespace Rebirth.Field.FieldPools
{
	public class CDropPool : CObjectPool<CDrop>
	{
		public CDropPool(CField parentField)
			: base(parentField) { }

		public void Update()
		{
			var dropsToRemove = new List<CDrop>();

			foreach (var drop in this)
			{
				if (drop.SpawnTime.AddedSecondsExpired(drop.DropExpirySeconds))
				{
					dropsToRemove.Add(drop);
				}
			}

			foreach (var drop in dropsToRemove)
			{
				Remove(drop);
			}
		}

		public void PickUp(Character pChar, int dwDropId, short pX, short pY, bool bByPet = false)
		{
			var pDrop = this[dwDropId];

			if (pDrop is null) return;

			switch (pDrop.DropOwnType)
			{
				case DropOwnType.PartyOwn when pChar.Party?.PartyID != pDrop.DropOwnerID:
				case DropOwnType.UserOwn when pChar.dwId != pDrop.DropOwnerID:
					pChar.SendMessage("Trying to pick up a drop that doesn't belong to you.");
					return;
			}

			if (pDrop.Item != null)
			{
				if (InventoryManipulator.CountFreeSlots(pChar, ItemConstants.GetInventoryType(pDrop.Item.nItemID)) <= 0)
				{
					return;
				}
			}

			if (bByPet)
			{
				if (Constants.MULTIPET_ACTIVATED)
				{
					throw new NotImplementedException(); // since we arent checking multiple pet equip slots
				}

				if (pDrop.bIsMoney == 1)
				{
					if (InventoryManipulator.GetItem(pChar, BodyPart.BP_PETABIL_MESO, true) is null) return;
				}
				else
				{
					if (InventoryManipulator.GetItem(pChar, BodyPart.BP_PETABIL_ITEM, true) is null) return;
				}

				pDrop.nLeaveType = DropLeaveType.PetPickup;
			}
			else
			{
				pDrop.nLeaveType = DropLeaveType.UserPickup;
			}

			pDrop.OwnerCharId = pChar.dwId;

			if (pDrop.bIsMoney > 0)
			{
				pChar.Modify.GainMeso(pDrop.nMesoVal);
			}
			else
			{
				if (pDrop.Item.Template is CashItemTemplate itemDataTemplate)
				{
					if (itemDataTemplate.Max > 0
					    && InventoryManipulator.ContainsItem(pChar, pDrop.ItemId, (short)itemDataTemplate.Max))
					{
						pChar.SendMessage("Can't hold anymore of this item..");
						return;
					}
				}

				if (!Field.TryDropPickup(pChar, pDrop)) return;

				pChar.SendPacket(CPacket.DropPickUpMessage_Item(pDrop.ItemId, pDrop.Item.nNumber, false));
		
				if (pDrop.Item.Template.PickupMessage.Length > 0)
				{
					pChar.SendMessage(pDrop.Item.Template.PickupMessage);
				}
			}

			Remove(dwDropId);
		}

		protected override void InsertItem(int index, CDrop item)
		{
			if (item.DropExpirySeconds <= 0) item.DropExpirySeconds = 60;

			base.InsertItem(index, item);

			Field.Broadcast(item.MakeEnterFieldPacket());

			// so when new characters join the map the items wont appear to re-drop
			item.nEnterType = DropEnterType.OnFoothold;
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			if (item != null)
			{
				Field.Broadcast(item.MakeLeaveFieldPacket());
			}

			base.RemoveItem(index);
		}
	}
}
