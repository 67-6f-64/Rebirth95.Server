using Rebirth.Characters;
using Rebirth.Client;

namespace Rebirth.Scripts.Item
{
	public class ItemScript : ScriptBase
	{
		public ItemScriptContext Context { get; }
		private Character Character => Context.Character;

		public ItemScript(string sScriptName, string sScriptContents, int nItemID, short nItemPOS, WvsGameClient c) : base(sScriptName, sScriptContents, c)
		{
			Context = new ItemScriptContext(this, nItemID, nItemPOS);
		}

		public override void InitLocals()
		{
			Engine.Set("ctx", Context);
		}

		public override void Dispose()
		{
			base.Dispose();
			Character.Action.Enable();
		}
	}
}
