using Rebirth.Server.Center;

namespace Rebirth.Commands.Admin
{
	public sealed class BannerCommand : Command
	{
		public override string Name => "banner";
		public override string Parameters => "<message>";
		public override bool IsRestricted => true;

		public override void Execute(CommandCtx ctx)
		{
			var szMessage = ctx.Remaining();

			MasterManager.CharacterPool.ForEach(pUser =>
			 {
				 pUser.SendPacket(CPacket.BroadcastServerMsg(szMessage));
			 });
		}
	}
}
