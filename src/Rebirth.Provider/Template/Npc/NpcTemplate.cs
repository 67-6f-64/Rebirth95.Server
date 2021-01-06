using Rebirth.Provider.Attribute;
using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Npc
{
	public sealed class NpcTemplate : AbstractTemplate
	{
		private bool _mapletv;
		[ProviderProperty("info/MapleTV")]
		public bool MapleTV
		{
			get => _mapletv;
			set => _mapletv = !Locked ? value : throw new TemplateAccessException();
		}

		private string _script;
		[ProviderProperty("info/script/0/script")]
		public string Script
		{
			get => _script;
			set => _script = !Locked ? value ?? "" : throw new TemplateAccessException();
		}

		private int _trunkput;
		[ProviderProperty("trunkPut")]
		public int TrunkPut
		{
			get => _trunkput;
			set => _trunkput = !Locked ? value : throw new TemplateAccessException();
		}

		public NpcTemplate(int templateId)
			: base(templateId)
		{
			Script = "";
		}
	}
}
