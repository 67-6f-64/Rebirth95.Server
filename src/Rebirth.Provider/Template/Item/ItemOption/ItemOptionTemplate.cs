namespace Rebirth.Provider.Template.Item.ItemOption
{
	public sealed class ItemOptionTemplate : AbstractTemplate
	{
		public int OptionType { get; set; }
		public int ReqLevel { get; set; }
		public ItemOptionLevelData[] LevelData { get; set; }

		public ItemOptionTemplate(int templateId)
			: base(templateId)
		{
			LevelData = new ItemOptionLevelData[0];
		}
	}
}
