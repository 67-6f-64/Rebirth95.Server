namespace Rebirth.Scripts
{
    public class ScriptException : System.Exception
    {
        public ScriptException() : base() { }
        public ScriptException(string message) : base(message) { }
    }
}