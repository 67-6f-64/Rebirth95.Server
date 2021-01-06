using Rebirth.Server.Center;

namespace Rebirth.Commands.Player
{
	public class GoEventCommand : Command
	{
		public override string Name => "event";
		public override string Parameters => string.Empty;

		public override void Execute(CommandCtx ctx)
		{
			MasterManager.EventManager.TryJoinEvent(ctx.Character);
		}
	}
}
