using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Python
{
    public class PyEngine : IDisposable
    {
        public string Script { get; }
        private Dictionary<string, object> Locals { get; }
        private PyScope Scope { get; set; }

        public PyEngine(string script)
        {
            Script = script;
            Locals = new Dictionary<string, object>();
        }

        public static void Initialize()
        {
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }
        
        public void Set<T>(string name,T value)
        {
            var pyName = name;
            var pyObj = value;

            Locals.Add(pyName, pyObj);
        }

        public void Run()
        {
            using (Py.GIL())
            using (PyScope scope = Py.CreateScope())
            {
                Scope = scope;

                foreach(var kvp in Locals)
                {
                    var pyName = kvp.Key;
                    PyObject pyObj = kvp.Value.ToPython();

                    Scope.Set(pyName, pyObj);
                }

                Scope.Exec(Script);
            }
        }

        public void RunFunction(string sFuncName, params string[] args)
        {
	        using (Py.GIL())
	        using (PyScope scope = Py.CreateScope())
	        {
		        Scope = scope;

		        foreach (var kvp in Locals)
		        {
			        var pyName = kvp.Key;
			        PyObject pyObj = kvp.Value.ToPython();

			        Scope.Set(pyName, pyObj);
		        }

		        Scope.Exec(Script);

                var functionBuilder = new StringBuilder();
                functionBuilder.Append(sFuncName);

                functionBuilder.Append("(");
		        foreach (var arg in args)
		        {
			        functionBuilder.Append(arg);
		        }
		        functionBuilder.Append(")");

                Scope.Exec(functionBuilder.ToString());
	        }
        }

        public void Dispose()
        {
            Locals?.Clear();
        }
    }
}