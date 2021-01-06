using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public sealed class MorphItemTemplate : CashItemTemplate
	{
		[ProviderProperty("info/hp")]
		public int HP { get; set; }

		[ProviderProperty("info/morph")]
		public int Morph { get; set; }

		public MorphItemTemplate(int templateId)
			: base(templateId) { }
	}
}