using Rebirth.Client;
using Rebirth.Field.FieldObjects;

namespace Rebirth.Scripts.Reactor
{
	public class ReactorScript : ScriptBase
    {
        public ReactorScriptContext Context { get; }

        public ReactorScript(string sScriptName, string sScriptContents, CReactor reactor, WvsGameClient client, bool bHit)
            : base(sScriptName, sScriptContents, client)
        {
            Context = new ReactorScriptContext(this, reactor, bHit);
        }

        public override void InitLocals()
        {
            Engine.Set("ctx", Context);
            Engine.Set("reactor", Context.Reactor);
        }

        public override void Dispose()
        {
            base.Dispose(); // no need to do anything here - client expects nothing
        }
    }
}
