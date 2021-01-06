using Rebirth.Server.Center;

namespace Rebirth.Commands.Admin
{
	public class GiveCommand : Command
	{
		public override string Name => "give";
		public override string Parameters => "<charname> <sp/ap/nx> <amount>";
		public override bool IsRestricted => true;

		public override void Execute(CommandCtx ctx)
		{
			if (ctx.Empty)
			{
				ctx.Character.SendMessage(Syntax);
				return;
			}

			var name = ctx.NextString().ToLower();

			var targetChar = MasterManager.CharacterPool.Get(name);

			if (ctx.Empty || targetChar is null)
			{
				ctx.Character.SendMessage(Syntax);
				return;
			}

			var operation = ctx.NextString().ToLower();

			if (ctx.Empty)
			{
				ctx.Character.SendMessage(Syntax);
				return;
			}

			var amount = ctx.NextInt();

			targetChar.Modify.Stats(c =>
			{
				switch (operation)
				{
					case "sp":
						c.SP += (short)amount;
						break;
					case "ap":
						c.AP += (short)amount;
						break;
					case "nx":
						targetChar.Modify.GainNX(amount);
						break;
					default:
						ctx.Character.SendMessage(Syntax);
						return;
				}
			});
		}
	}
}
