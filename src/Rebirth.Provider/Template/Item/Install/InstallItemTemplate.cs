using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Install
{
	public sealed class InstallItemTemplate : AbstractItemTemplate
	{
		[ProviderProperty("info/recoveryHP")]
		public int RecoveryHP { get; set; }

		[ProviderProperty("info/recoveryMP")]
		public int RecoveryMP { get; set; }

		[ProviderProperty("info/reqLevel")]
		public int ReqLevel { get; set; }

		[ProviderProperty("info/tamingMob")]
		public int TamingMob { get; set; }

		public InstallItemTemplate(int templateId)
			: base(templateId)
		{ }
	}
}