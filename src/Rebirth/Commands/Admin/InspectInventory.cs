using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Commands.Admin
{
	public sealed class InspectInventory : Command
	{
		public override string Name => "inspectinv";
		public override string Parameters => "playername";
		public override void Execute(CommandCtx ctx)
		{
			//if (ctx.Empty)
			//{
			//ctx.Character.SendPacket();
			//}
			//else
			//{

			//}
		}
	}
}
