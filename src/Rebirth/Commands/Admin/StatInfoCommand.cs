namespace Rebirth.Commands.Impl
{
    public class StatInfoCommand : Command
    {
        public override string Name => "statinfo";
        public override string Parameters => "<statname>";
        public override bool IsRestricted => true;
        public override bool IsDisabled => true;

        public override void Execute(CommandCtx ctx)
        {
            var p = ctx.Character;

            string output;
            switch (ctx.NextString().ToLower())
            {
                case "job":
                    output = "Job ID: " + p.Stats.nJob;
                    break;
                case "subjob":
                    output = "Sub Job ID: " + p.Stats.nSubJob;
                    break;
                default:
                    output = "Unhandled or invalid stat identifier.";
                    break;
            }
            p.Action.SystemMessage(output);
        }
    }
}
