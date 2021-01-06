namespace Rebirth.Commands
{
    public abstract class Command
    {
        public abstract string Name { get; }
        public abstract string Parameters { get; }
        public virtual bool IsRestricted => false;
        public virtual bool IsDisabled => false;
		public string Syntax => $"Command Syntax: { (IsRestricted ? "!" : "@") }{Name} {Parameters}";
            
        public abstract void Execute(CommandCtx ctx);
    }
}
