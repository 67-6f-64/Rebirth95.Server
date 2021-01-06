using Rebirth.Server.Center;
using System;

namespace Rebirth.Commands.Impl
{
    public sealed class WarpCommand : Command
    {
        public override string Name => "warp";
        public override string Parameters => "<mapid / character name>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
			if(ctx.Empty)
            {
				ctx.Character.SendMessage($"[WarpCmd] Current MapId: {ctx.Character.Field.MapId}");
			}
            else
            {
                var szInput = ctx.NextString();

                if (!int.TryParse(szInput, out var nMapId))
                {
                    var pUserRemote = MasterManager.CharacterPool.Get(szInput);

                    if (pUserRemote == null)
                    {
                        ctx.Character.SendMessage($"[WarpCmd] Unable to find user: {szInput}");
                        return;
                    }

                    nMapId = pUserRemote.Stats.dwPosMap;
                }

                ctx.Character.Action.SetField(nMapId, 0, 0);
            }
        }
    }
}