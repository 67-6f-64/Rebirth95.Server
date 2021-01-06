using Rebirth.Characters;
using Rebirth.Client;

namespace Rebirth.Scripts.Portal
{
    public class PortalScript : ScriptBase
    {
        public PortalScriptContext Context { get; }
        private Character Char { get => Context.Character; }

        public PortalScript(string sScriptName, string sScriptContents, WvsGameClient client) : base(sScriptName, sScriptContents, client)
        {
            Context = new PortalScriptContext(this);
        }

        public override void InitLocals()
        {
            Engine.Set("ctx", Context);
			Engine.Set("script", this);
        }

        public override void Dispose()
        {
            base.Dispose();
            Char.Action.Enable();
        }
    }
}
