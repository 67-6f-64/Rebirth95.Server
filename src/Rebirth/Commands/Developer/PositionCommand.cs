namespace Rebirth.Commands.Developer
{
    public sealed class PositionCommand : Command
    {
        public override string Name => "position";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var pUser = ctx.Character;
            var szMessage = $"Map: {pUser.Stats.dwPosMap} | {pUser.Position}";
            pUser.SendMessage(szMessage);
        }
    }
}
