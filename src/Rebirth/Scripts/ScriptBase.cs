using log4net;
using Python.Runtime;
using Rebirth.Client;
using Rebirth.Python;
using System;
using System.Threading.Tasks;

namespace Rebirth.Scripts
{
    public abstract class ScriptBase : IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(ScriptBase));

        public bool Init { get; private set; }

        public WvsGameClient Parent { get; }
        public PyEngine Engine { get; }
        public Task Task { get; private set; }

        public string ScriptName { get; }

        protected ScriptBase(string sScriptName, string sScriptContents, WvsGameClient client)
        {
            Parent = client;
            Engine = new PyEngine(sScriptContents);
            ScriptName = sScriptName;
        }

        public abstract void InitLocals();

        public void Execute()
        {
            //TODO: Find new place for this hack
            if (!Init)
            {
                Init = true;
                InitLocals();
            }

            //TODO: Check if task is already created/running
            Task = Task.Run(new Action(ExecuteProc));
        }

        private void ExecuteProc()
        {
            var tStart = Environment.TickCount;
            try
            {
	            EngineExecPattern();
            }
            catch (PythonException pex)
            {
                if (!pex.Message.Contains("NpcScriptException"))
                {
	                Log.Error("Exception caught with script name: " + ScriptName);
                    Log.FatalFormat("ScriptBase PythonException: {0}", pex);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception caught with script name: " + ScriptName);
                Log.FatalFormat("[ScriptBase] UnknownException: {0}", ex);
            }
            finally
            {
                var tEnd = Environment.TickCount;
                var tSpan = tEnd - tStart;
                Log.InfoFormat("ScriptBase Ended {0} ms", tSpan);

                Dispose();  //Confirm this
            }
        }

        protected virtual void EngineExecPattern()
        {
	        Engine.Run();
        }

        public virtual void Dispose()
        {
            Engine?.Dispose(); //TODO: Review this
        }
    }
}
