using Rebirth.Characters.Modify;
using Rebirth.Entities.Item;
using Rebirth.Server.Center;
using System;

namespace Rebirth.Commands.Impl
{
	public sealed class ItemCommand : Command
	{
		public override string Name => "item";
		public override string Parameters => "<item_id> <amount>";
		public override bool IsRestricted => true;

		public override void Execute(CommandCtx ctx)
		{
			var itemId = ctx.NextInt();

			var sCount = !ctx.Empty ? ctx.NextString() : null;
			var nCount = 1;

			if (sCount is object)
			{
				if (!int.TryParse(sCount, out nCount))
				{
					ctx.Character.SendMessage("Unable to parse count.");
					return;
				}
			}

			var character = ctx.Character;

			var nInvType = ItemConstants.GetInventoryType(itemId);

			if (InventoryManipulator.CountFreeSlots(character, nInvType) > 0)
			{
				var item = MasterManager.CreateItem(itemId);

				if (item is null) return;

				if (item is GW_ItemSlotBundle isb)
				{
					isb.nNumber = (short)Math.Min(nCount, item.SlotMax);
				}

				InventoryManipulator.InsertInto(character, item);
				character.SendPacket(CPacket.DropPickUpMessage_Item(itemId, nCount, true));
			}
		}
	}
}
