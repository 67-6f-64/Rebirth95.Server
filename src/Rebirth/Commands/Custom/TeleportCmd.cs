using Rebirth.Commands;

namespace Rebirth.Custom.Commands
{
    public sealed class TeleportCmd : Command
    {
        public override string Name => "tele";
        public override string Parameters => "<x> <y>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var nX = ctx.NextInt();
            var nY = ctx.NextInt();

            //ctx.SendPacket(CPacket.Custom.Teleport(nX,nY));
        }
    }
}
