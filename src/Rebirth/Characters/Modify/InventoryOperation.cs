using Rebirth.Entities.Item;
using Rebirth.Network;
using System;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Modify
{
	/// <summary>
	/// CWvsContext::OnInventoryOperation
	/// Based on work from Edelstein https://github.com/Kaioru/Edelstein/
	/// </summary>
	public abstract class AbstractInventoryOperation
	{
		public readonly InventoryOperationType nOperation;
		public readonly InventoryType nTI;
		public readonly short nPOS;

		public AbstractInventoryOperation(InventoryOperationType type, InventoryType inventory, short slot)
		{
			nOperation = type;
			nTI = inventory;
			nPOS = slot;
		}

		public virtual void Encode(COutPacket p)
		{
			p.Encode1((byte)nOperation);
			p.Encode1((byte)nTI);
			p.Encode2(nPOS);
		}
	}

	/// <summary>
	/// Operation ID: 0
	/// </summary>
	public class InventoryOperationAdd : AbstractInventoryOperation
	{
		private readonly GW_ItemSlotBase m_item;

		public InventoryOperationAdd(InventoryType inventory, short slot, GW_ItemSlotBase item)
			: base(InventoryOperationType.Add, inventory, slot)
		{
			m_item = item;
		}

		public override void Encode(COutPacket p)
		{
			base.Encode(p);

			switch (m_item)
			{
				case GW_ItemSlotEquip equip:
					equip.RawEncode(p);
					break;
				case GW_ItemSlotBundle bundle: // pet is derived from this
					bundle.RawEncode(p);
					break;
				default:
					throw new Exception("Dude what the fuck man? Encoding unsupported item type?!~");
			}
		}
	}

	/// <summary>
	/// Operation ID: 1
	/// </summary>
	public class InventoryOperationUpdateQuantity : AbstractInventoryOperation
	{
		private readonly short m_quantity;

		public InventoryOperationUpdateQuantity(InventoryType inventory, short slot, short quantity)
			: base(InventoryOperationType.UpdateQuantity, inventory, slot)
		{
			m_quantity = quantity;
		}

		public override void Encode(COutPacket p)
		{
			base.Encode(p);

			p.Encode2(m_quantity);
		}
	}

	/// <summary>
	/// Operation ID: 2
	/// </summary>
	public class InventoryOperationMove : AbstractInventoryOperation
	{
		public readonly short _newSlot;

		public InventoryOperationMove(InventoryType inventory, short slot, short newSlot)
			: base(InventoryOperationType.Move, inventory, slot)
		{
			_newSlot = newSlot;
		}

		public override void Encode(COutPacket p)
		{
			base.Encode(p);

			p.Encode2(_newSlot);
		}
	}

	/// <summary>
	/// Operation ID: 3
	/// </summary>
	public class InventoryOperationRemove : AbstractInventoryOperation
	{
		public InventoryOperationRemove(InventoryType inventory, short slot)
			: base(InventoryOperationType.Remove, inventory, slot) { }

		public override void Encode(COutPacket p)
		{
			base.Encode(p);
		}
	}

	/// <summary>
	/// Operation ID: 4
	/// </summary>
	public class InventoryOperationUpdateEXP : AbstractInventoryOperation
	{
		private readonly int _exp;

		public InventoryOperationUpdateEXP(InventoryType inventory, short slot, int EXP)
			: base(InventoryOperationType.UpdateEXP, inventory, slot)
		{
			_exp = EXP;
		}

		public override void Encode(COutPacket packet)
		{
			base.Encode(packet);

			packet.Encode4(_exp);
		}
	}
}
