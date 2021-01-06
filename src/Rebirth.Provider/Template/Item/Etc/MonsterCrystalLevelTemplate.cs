using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Etc
{
	public sealed class MonsterCrystalLevelTemplate : AbstractItemTemplate
	{
		[ProviderProperty("info/lvMax")]
		public int lvMax { get; set; }

		[ProviderProperty("info/lvMin")]
		public int lvMin { get; set; }

		public MonsterCrystalLevelTemplate(int templateId) 
			: base(templateId) { }
	}
}
