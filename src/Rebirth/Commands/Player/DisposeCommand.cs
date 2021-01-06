using System;

namespace Rebirth.Commands.Impl
{
    public sealed class DisposeCommand : Command
    {
        public override string Name => "dispose";
        public override string Parameters => string.Empty;

        public override void Execute(CommandCtx ctx)
        {
            ctx.Character.Action.Enable();
            ctx.Character.Modify.ResetForcedStat();

            var pScript = ctx.Character.Socket.NpcScript;

            pScript?.Dispose();

            ctx.Character.SendMessage("Dispose Complete");
        }
    }
}