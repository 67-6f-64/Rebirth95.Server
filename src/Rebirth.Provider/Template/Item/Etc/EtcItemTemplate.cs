using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Etc
{
	public sealed class EtcItemTemplate : AbstractItemTemplate
	{
		[ProviderProperty("info/lv")]
		public int lv { get; set; }

		[ProviderProperty("info/exp")]
		public int Exp { get; set; }

		[ProviderProperty("info/questId")]
		public int QuestID { get; set; }

		[ProviderProperty("info/grade")]
		public int Grade { get; set; }

		[ProviderProperty("info/consumeItem/consumeCount")]
		public int ConsumeCount { get; set; }

		[ProviderProperty("info/consumeItem/consumeCountMessage")]
		public string ConsumeMessage { get; set; }

		[ProviderProperty("info/pquest")]
		public bool PQuest { get; set; }

		[ProviderProperty("info/pickUpBlock")]
		public bool PickupBlock { get; set; }

		[ProviderProperty("info/hybrid")]
		public bool Hybrid { get; set; }

		[ProviderProperty("info/shopCoin")]
		public bool ShopCoin { get; set; }

		[ProviderProperty("info/bigSize")]
		public bool BigSize { get; set; }

		public int[] ConsumeItem { get; set; }
		public int[] ConsumeItemExpGain { get; set; }

		public EtcItemTemplate(int templateId) : base(templateId)
		{
			ConsumeItem = new int[0];
			ConsumeItemExpGain = new int[0];
			ConsumeMessage = "";
		}
	}
}