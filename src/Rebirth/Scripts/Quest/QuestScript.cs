using Python.Runtime;
using Rebirth.Characters.Quest;
using Rebirth.Client;

namespace Rebirth.Scripts.Quest
{
    public class QuestScript : ScriptBase
    {
        public QuestScriptContext Context { get; }
        public QuestEntry Quest { get; }

        private int nAct { get; }

        public QuestScript(string sScriptName, int nAct, string sScriptContents, QuestEntry quest, WvsGameClient client)
            : base(sScriptName, sScriptContents, client)
        {
            Context = new QuestScriptContext(this);
            Quest = quest;
            this.nAct = nAct;
        }

        public override void InitLocals()
        {
            Engine.Set("ctx", Context);
        }

        protected override void EngineExecPattern()
        {
	       Engine.RunFunction(nAct == 0 ? "start" : "end");
        }

        public override void Dispose()
        {
            Parent.ClearScripts(false);
            base.Dispose();
        }
    }
}
