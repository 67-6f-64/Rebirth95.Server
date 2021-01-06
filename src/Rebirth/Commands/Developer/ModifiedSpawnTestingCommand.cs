namespace Rebirth.Commands.Developer
{
	public sealed class ModifiedSpawnTestingCommand : Command
    {
        public override string Name => "spawntest";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;
        public override bool IsDisabled => true; // dont need anyone doing this

        public override void Execute(CommandCtx ctx)
        {
            var c = ctx.Character;
            var f = c.Field; // yes im lazy

            var fhCount = f.Footholds.Count;

            if (ctx.Empty)
            {
                ctx.Character.SendMessage("Total FH: " + fhCount);
            }
            else
            {
                var rand = ctx.NextInt();

                var spawnedMobs = 0;
                var nonwall = 0;

                foreach (var item in f.Footholds.NonWallFHs)
                {
                    if (item.Wall)
                        continue;

                    nonwall += 1;

                    if (Constants.Rand.Next(100) > rand)
                        continue;

                    //var mob = new CMob(4230503);

                    //mob.SpawnCY = (short)((item.Y1 + item.Y2) / 2);
                    //mob.SpawnX = (short)((item.X1 + item.X2) / 2);
                    //mob.SpawnFH = item.Id;
                    //f.Mobs.CreateMob(mob);

                    spawnedMobs += 1;
                }

                ctx.Character.SendMessage($"Total FH: {fhCount}. Total non-wall FH: {nonwall}. Mobs spawned: {spawnedMobs}.");
            }
        }
    }
}
