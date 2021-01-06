using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Character
{
	public sealed class EquipTemplate : AbstractItemTemplate
	{
		[ProviderIgnore]
		public override int SlotMax => 1;
		/// <summary>
		/// Total upgrade count
		/// </summary>
		[ProviderProperty("info/tuc")]
		public int TUC { get; set; }

		[ProviderProperty("info/reqLevel")]
		public int ReqLevel { get; set; }

		[ProviderProperty("info/incSTR")]
		public int incSTR { get; set; }

		[ProviderProperty("info/incDEX")]
		public int incDEX { get; set; }

		[ProviderProperty("info/incINT")]
		public int incINT { get; set; }

		[ProviderProperty("info/incLUK")]
		public int incLUK { get; set; }

		[ProviderProperty("info/incMHP")]
		public int incMHP { get; set; }

		[ProviderProperty("info/incMMP")]
		public int incMMP { get; set; }

		[ProviderProperty("info/incPAD")]
		public int incPAD { get; set; }

		[ProviderProperty("info/incMAD")]
		public int incMAD { get; set; }

		[ProviderProperty("info/incPDD")]
		public int incPDD { get; set; }

		[ProviderProperty("info/incMDD")]
		public int incMDD { get; set; }

		[ProviderProperty("info/incACC")]
		public int incACC { get; set; }

		[ProviderProperty("info/incEVA")]
		public int incEVA { get; set; }

		[ProviderProperty("info/incCraft")]
		public int incCraft { get; set; }

		[ProviderProperty("info/incSpeed")]
		public int incSpeed { get; set; }

		[ProviderProperty("info/incJump")]
		public int incJump { get; set; }

		[ProviderProperty("info/equipTradeBlock")]
		public bool EquipTradeBlock { get; set; }

		[ProviderProperty("info/notExtend")]
		public bool NotExtend { get; set; }

		public EquipTemplate(int templateId)
			: base(templateId) { }
	}
}
