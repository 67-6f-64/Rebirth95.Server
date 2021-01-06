using Autofac;
using Rebirth.Scripts;

namespace Rebirth.Commands.Admin
{
    public sealed class ReloadScriptsCommand : Command
    {
        public override string Name => "reloadscripts";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var provider = ServerApp.Container.Resolve<ScriptManager>();
            provider.ClearScriptCache();

            ctx.Character.Action.SystemMessage("Script cache has been cleared.");
        }
    }
}
