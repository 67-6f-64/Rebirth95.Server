namespace Rebirth.Commands.Admin
{
    public sealed class KillAllCommand : Command
    {
        public override string Name => "killall";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx) => ctx.Character.Field.Mobs.RemoveAll();        
    }
}
