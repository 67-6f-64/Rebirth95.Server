using Autofac;
using Rebirth.Scripts;

namespace Rebirth.Commands.Impl
{
    public class AioNpcCommand : Command
    {
        public override string Name => "aionpc";
        public override string Parameters => string.Empty;
        public override bool IsDisabled => true;

        public override void Execute(CommandCtx ctx)
        {
			if (ctx.Character.Stats.nHP <= 0) return;

            var provider = ServerApp.Container.Resolve<ScriptManager>();

            ctx.Character.Socket.ChangeScript(provider.GetNpcScript(9200000, ctx.Character.Socket)); // Cody: 9200000
            ctx.Character.Socket.NpcScript.Execute();
        }
    }
}