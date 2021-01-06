using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public sealed class QuestDeliveryItemTemplate : CashItemTemplate
	{
		[ProviderProperty("info/type")]
		public int Type { get; set; }

		[ProviderProperty("info/effect")]
		public string Effect { get; set; }

		public int[] DisallowComplete { get; set; }
		public int[] DisallowAccept { get; set; }

		public QuestDeliveryItemTemplate(int templateId)
			: base(templateId) { }
	}
}