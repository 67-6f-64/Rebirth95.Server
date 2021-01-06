using Autofac;
using Rebirth.Scripts;

namespace Rebirth.Commands.Player
{
    public sealed class JobAdvCommand : Command
    {
        public override string Name => "job";
        public override string Parameters => string.Empty;

        public override void Execute(CommandCtx ctx)
        {
			if (ctx.Character.Stats.nHP <= 0) return;
			int npc = 1072004; // TODO fix this up

            // only first adventurer jobs will use this command, all others are auto adv
            //switch (ctx.Character.Stats.nJob)
            //{
            //    case 100:
            //        npc += 0;
            //        break;
            //    case 200:
            //        npc += 1;
            //        break;
            //    case 300:
            //        npc += 2;
            //        break;
            //    case 400:
            //        npc += 3;
            //        break;
            //    case 500:
            //        npc += 4;
            //        break;
            //    default:
            //        npc = 0;
            //        break;
            //}

            if (npc == 0 || ctx.Character.Stats.nLevel < 30 || ctx.Character.Stats.nSubJob > 0)
            {
                ctx.Character.SendMessage("You are not eligable for a Job Advancement.");
                return;
            }

            var provider = ServerApp.Container.Resolve<ScriptManager>();

            ctx.Character.Socket.ChangeScript(provider.GetNpcScript(npc, ctx.Character.Socket));
            ctx.Character.Socket.NpcScript.Execute();
        }
    }
}
