using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public class CashItemTemplate : AbstractItemTemplate
	{
		[ProviderIgnore]
		public override bool Cash => true;
		[ProviderIgnore]
		public override bool TradeBlock => true;
		[ProviderIgnore]
		public override bool NotSale => true;
		[ProviderIgnore]
		public override bool Quest => false;

		[ProviderProperty("info/protectTime")]
		public int ProtectTime { get; set; }

		[ProviderProperty("info/recoveryRate")]
		public int RecoveryRate { get; set; }

		[ProviderProperty("info/life")]
		public int Life { get; set; }

		[ProviderProperty("info/stateChangeItem")] // TODO verify
		public int StateChangeItem { get; set; }

		[ProviderProperty("info/type")]
		public int WeatherType { get; set; }

		[ProviderProperty("info/meso")]
		public int Meso { get; set; }

		[ProviderProperty("info/mesomin")]
		public int MesoMin { get; set; }

		[ProviderProperty("info/mesomax")]
		public int MesoMax { get; set; }

		[ProviderProperty("info/mesostdev")]
		public int MesoStDev { get; set; }

		[ProviderProperty("info/maplepoint")]
		public int MaplePoint { get; set; }

		[ProviderProperty("info/rate")]
		public int Rate { get; set; }

		public CashItemTemplate(int templateId)
			: base(templateId) { }
	}
}