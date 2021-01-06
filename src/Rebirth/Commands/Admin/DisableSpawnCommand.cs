namespace Rebirth.Commands.Admin
{
    public class DisableSpawnCommand : Command
    {
        public override string Name => "togglemobspawn";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            ctx.Character.Field.bPauseSpawn = !ctx.Character.Field.bPauseSpawn;
        }
    }
}
