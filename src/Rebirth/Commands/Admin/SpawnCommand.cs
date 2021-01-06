using Rebirth.Common.Types;

namespace Rebirth.Commands.Admin
{
    public sealed class SpawnMobCommand : Command
    {
        public override string Name => "spawn";
        public override string Parameters => "<id> <count>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var nMobId = ctx.NextInt();
            var nMobCount = 1;

            if (!ctx.Empty)
                nMobCount = ctx.NextInt();

            nMobCount = System.Math.Min(nMobCount, 150);

            for (int i = 0; i < nMobCount; i++)
            {
                var ptX = ctx.Character.Position.X;
                var ptY = ctx.Character.Position.Y - 10;
                var nFH = ctx.Character.Position.Foothold;

                ctx.Character.Field.Mobs.CreateMob(nMobId, null, ptX, ptY, nFH, 0xFE, 0, 0, MobType.Normal, null);
            }
        }
    }
}
