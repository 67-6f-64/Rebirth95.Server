using System.Collections.Generic;
using Autofac;
using Rebirth.Scripts;

namespace Rebirth.Commands.Developer
{
    public sealed class NpcCommand : Command
    {
        public override string Name => "npc";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;
        public override bool IsDisabled => true;

        public override void Execute(CommandCtx ctx)
        {
            var next = ctx.NextString();
            try
            {
                var npc = int.Parse(next);
                var provider = ServerApp.Container.Resolve<ScriptManager>();

                ctx.Character.Socket.ChangeScript(provider.GetNpcScript(npc, ctx.Character.Socket));
                ctx.Character.Socket.NpcScript.Execute();
            }
            catch
            {
                switch (next.ToLower())
                {
                    case "here":
                        List<int> names = new List<int>();
                        ctx.Character.Field.Npcs.ForEach(n => names.Add(n.TemplateId));

                        var nm = "NPCs: ";
                        names.ForEach(b => nm += " " + b);
                        ctx.Character.SendMessage(nm);
                        break;
                }
            }
        }
    }


}
