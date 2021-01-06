namespace Rebirth.Scripts.Item
{
	public class ItemScriptContext : ScriptContextBase<ItemScript>
	{
		public int ScriptItemID { get; private set; }
		public short ScriptItemPOS { get; private set; }

		public ItemScriptContext(ItemScript script, int nItemID, short nItemPOS) : base(script)
		{
			ScriptItemID = nItemID;
			ScriptItemPOS = nItemPOS;
		}
	}
}
