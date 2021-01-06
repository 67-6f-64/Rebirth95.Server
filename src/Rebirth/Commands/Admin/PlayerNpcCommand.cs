using Rebirth.Entities;
using Rebirth.Entities.PlayerData;
using Rebirth.Field.FieldObjects;

namespace Rebirth.Commands.Impl
{
	public sealed class PlayerNpcCommand : Command
    {
        public override string Name => "playernpc";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var c = ctx.Character;
            
            var x = new CNpcImitate(9901000)
            {
                Foothold = c.Position.Foothold,
                X = c.Position.X,
                Y = c.Position.Y,
                Cy = c.Position.Y,
                F = false,
                Rx0 = -100, 
                Rx1 = 100
            };

            x.Look = new AvatarLook();
            x.Look.CopyStats(c.Stats);
            x.Look.CopyInventory(c.EquippedInventoryNormal, c.EquippedInventoryCash);

            x.Name = "Rebirth";

            c.Field.Npcs.Add(x);

            c.Action.SystemMessage("PlayerNpc has been placed @ {0}", c.Position);
        }
    }
}
