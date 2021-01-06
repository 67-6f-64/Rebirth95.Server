namespace Rebirth.Commands.Impl
{
    public sealed class SonicCommand : Command
    {
        public override string Name => "sonic";
        public override string Parameters => "<true/false>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var flag = ctx.NextBool();

            if (flag)
            {
                ctx.Character.Modify.ForcedStat(f =>
                {
                    f.Jump = 255;
                    f.Speed = 255;
                    f.SpeedMax = 255;
                });
            }
            else
            {
                ctx.Character.Modify.ResetForcedStat();
            }
        }
    }
}
