using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public sealed class PetFoodItemTemplate : CashItemTemplate
	{
		[ProviderProperty("spec/inc")]
		public int Repleteness { get; set; }

		public int[] Pet { get; set; }

		public PetFoodItemTemplate(int templateId)
			: base(templateId) { }
	}
}