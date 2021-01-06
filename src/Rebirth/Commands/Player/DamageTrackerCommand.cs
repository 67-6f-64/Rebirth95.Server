using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Commands.Player
{
	public class DamageTrackerCommand : Command
	{
		public override string Name => "damagetracker";
		public override string Parameters => "<start/stop/reset/restart>";
		public override bool IsRestricted => false;
		public override bool IsDisabled => false;

		public override void Execute(CommandCtx ctx)
		{
			if (ctx.Empty)
			{
				ctx.Character.SendMessage(Syntax);
			}
			else
			{
				switch (ctx.NextString().ToLower())
				{
					case "start":
						ctx.Character.DamageTracker.Start();
						break;
					case "stop":
					case "end":
						ctx.Character.DamageTracker.Stop();
						break;
					case "restart":
						ctx.Character.DamageTracker.Reset();
						ctx.Character.DamageTracker.Start();
						break;
					case "reset":
						ctx.Character.DamageTracker.Reset();
						break;
					default:
						ctx.Character.SendMessage(Syntax);
						break;
				}
			}
		}
	}
}
