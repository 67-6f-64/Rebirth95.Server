using Rebirth.Server.Center;

namespace Rebirth.Commands.Impl
{
    public sealed class WarnCommand : Command
    {
        public override string Name => "warn";
        public override string Parameters => "<name> <message>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var victim = ctx.NextString();
            var msg = ctx.Remaining();

            var remote = MasterManager.CharacterPool.Get(victim);

            if (remote != null)
                remote.Action.BoxMessage(msg);
        }
    }
}
