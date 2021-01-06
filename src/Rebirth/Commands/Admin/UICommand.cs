using Rebirth.Common.Types;

namespace Rebirth.Commands.Impl
{
	public sealed class UICommand : Command
	{
		public override string Name => "ui";
		public override string Parameters => "<id>";
		public override bool IsRestricted => true;
		public override bool IsDisabled => false;

		public override void Execute(CommandCtx ctx)
		{
			var nType = (byte)ctx.NextInt();

			if (ctx.Queue.Count > 0)
			{
				var nOpt = ctx.NextInt();
				var p = CPacket.UserOpenUIWithOption((UIWindow)nType, nOpt);
				ctx.SendPacket(p);
			}
			else
			{
				var p = CPacket.UserOpenUI((UIWindow)nType);
				ctx.SendPacket(p);
			}
		}
	}
}
