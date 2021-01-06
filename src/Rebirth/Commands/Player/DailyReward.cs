using System;

namespace Rebirth.Commands.Impl
{
    public partial class PlayerCommand
    {
        public class DailyReward : Command
        {
            public override string Name => "daily";
            public override string Parameters => string.Empty;
            public override bool IsDisabled => true; // because its not complete

            public override void Execute(CommandCtx ctx)
            {
                throw new NotImplementedException();
            }
        }
    }
}
