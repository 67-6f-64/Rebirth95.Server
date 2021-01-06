namespace Rebirth.Scripts.Npc
{
    public sealed class NpcScriptException : ScriptException
    {
        public bool EndChat { get; } = true;

        public NpcScriptException() { }
    }
}
