namespace Rebirth.Commands.Developer
{
    public sealed class PortalCommand : Command
    {
        public override string Name => "portals";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;
        public override bool IsDisabled => false;
               
        public override void Execute(CommandCtx ctx)
        {
            var pUser = ctx.Character;
            var pPortal = pUser.Field.Portals.GetPortalInRect(pUser.Position.X, pUser.Position.Y, 50);

            if (pPortal is null) return;

            pUser.SendMessage("Portal ID: " + pPortal.nIdx);
            pUser.SendMessage("Portal Name: " + pPortal.sName);
            pUser.SendMessage("Portal X-Pos: " + pPortal.nX);
            pUser.SendMessage("Portal Y-Pos: " + pPortal.nY);
        }
    }
}
