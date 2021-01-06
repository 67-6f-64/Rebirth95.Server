using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Commands.Admin
{
	public class ClockCommand : Command
	{
		public override string Name => "clock";
		public override string Parameters => "<seconds>";
		public override bool IsRestricted => true;

		public override void Execute(CommandCtx ctx)
		{
			if (!ctx.Empty && int.TryParse(ctx.NextString(), out int seconds) && seconds > 0)
			{
				ctx.Character.Field.CreateFieldClock(seconds);
			}
			else
			{
				ctx.Character.SendMessage($"Syntax: !{Name} {Parameters}");
			}
		}
	}
}
