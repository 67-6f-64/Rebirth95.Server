using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Commands.Admin
{
    public sealed class DiscordCommand : Command
    {
        public override string Name => "discord";
        public override string Parameters => "<message>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var szMessage = ctx.Remaining();
            Tools.Discord.PostMessage($"[AdminCommandLog] {szMessage}");
        }
    }
}
