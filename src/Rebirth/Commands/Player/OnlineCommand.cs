namespace Rebirth.Commands.Player
{
	public sealed class OnlineCommand : Command
	{
		public override string Name => "online";
		public override string Parameters => string.Empty;

		public override void Execute(CommandCtx ctx)
		{
			var center = ctx.Character.Socket.Server.Center;

			foreach (var game in center.WvsGames)
			{
				if (game.Clients.Count == 0)
					continue;

				ctx.Character.Action.SystemMessage("[{0}]", game.Name);

				foreach (var user in game.Clients)
				{
					if (user.LoggedIn)
					{
						ctx.Character.Action.SystemMessage("\t{0}\t{1}\t{2}", user.Character.dwId, user.Character.Stats.sCharacterName, user.Character.Stats.dwPosMap);
					}
				}
			}
		}
	}
}
