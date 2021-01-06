using Rebirth.Server.Center;
using Rebirth.Server.Center.GameData.DropInfo;
using System.Linq;

namespace Rebirth.Commands.Impl
{
    public sealed class DropCommand : Command
    {
        public override string Name => "drop";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;
        public override bool IsDisabled => true;

        public override void Execute(CommandCtx ctx)
        {
            // doesn't work as of 6/26/2020 when the drop refactoring occurred
            //var pField = ctx.Character.Field;

            //try
            //{
            //    var parsedMobId = int.Parse(ctx.NextString());
            //    var parsedDropId = int.Parse(ctx.NextString());
            //    var dropRate = int.Parse(ctx.NextString()) / 100;

            //    var mob = MasterManager.MobDropData(parsedMobId);

            //    if (mob == null)
            //        mob = new MobDropStruct { MobID = parsedMobId };

            //    // TODO add item and mob validation here
            //    var drops = mob.Drops.ToList();
            //    drops.Add(new DropStruct { ItemID = parsedDropId, Chance = dropRate });
            //    mob.Drops = drops;

            //    MasterManager.MobDropGenerator.Remove(mob.MobID);
            //    MasterManager.MobDropGenerator.Add(mob);

            //    ctx.Character.SendMessage($"Added drop ID: {parsedDropId} to mob ID: {mob.MobID}");
            //}
            //catch
            //{
            //    foreach (var mob in pField.Mobs)
            //    {
            //        var entry = MobDropGenerator.mobs.FirstOrDefault(m => m.ID == mob.nMobTemplateId);
            //        ctx.Character.SendMessage($"Mob ID: {entry.ID} <> Mob Name: {entry.Name}");
            //    }
            //}
        }
    }
}