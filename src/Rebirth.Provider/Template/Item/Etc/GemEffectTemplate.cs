using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Etc
{
	public sealed class GemEffectTemplate : AbstractItemTemplate
	{
		[ProviderProperty("info/randStat")]
		public byte RandStat { get; set; }


		[ProviderProperty("info/randOption")]
		public byte RandOption { get; set; }


		[ProviderProperty("info/incReqLevel")]
		public byte incReqLevel { get; set; }


		[ProviderProperty("info/incDEX")]
		public byte incDEX { get; set; }


		[ProviderProperty("info/incSTR")]
		public byte incSTR { get; set; }


		[ProviderProperty("info/incLUK")]
		public byte incLUK { get; set; }


		[ProviderProperty("info/incINT")]
		public byte incINT { get; set; }


		[ProviderProperty("info/incPDD")]
		public byte incPDD { get; set; }


		[ProviderProperty("info/incMDD")]
		public byte incMDD { get; set; }


		[ProviderProperty("info/incPAD")]
		public byte incPAD { get; set; }


		[ProviderProperty("info/incMAD")]
		public byte incMAD { get; set; }


		[ProviderProperty("info/incEVA")]
		public byte incEVA { get; set; }


		[ProviderProperty("info/incACC")]
		public byte incACC { get; set; }


		[ProviderProperty("info/incMaxHP")]
		public byte incMaxHP { get; set; }


		[ProviderProperty("info/incMaxMP")]
		public byte incMaxMP { get; set; }


		[ProviderProperty("info/incSpeed")]
		public byte incSpeed { get; set; }


		[ProviderProperty("info/incJump")]
		public byte incJump { get; set; }

		public GemEffectTemplate(int templateId) 
			: base(templateId) { }
	}
}