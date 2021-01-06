using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Map
{
	public sealed class MapPortalTemplate : AbstractTemplate
	{
		// this is what the client requests
		private string _targetname;
		public string sTargetName
		{
			get => _targetname;
			set
			{
				if (!Locked) _targetname = value;
				else throw new TemplateAccessException();
			}
		}

		private int _portaltype;
		public int nPortalType
		{
			get => _portaltype;
			set
			{
				if (!Locked) _portaltype = value;
				else throw new TemplateAccessException();
			}
		}

		private int _targetmap;
		public int nTargetMap
		{
			get => _targetmap;
			set
			{
				if (!Locked) _targetmap = value;
				else throw new TemplateAccessException();
			}
		}

		private string _portalname;
		public string sPortalName
		{
			get => _portalname;
			set
			{
				if (!Locked) _portalname = value;
				else throw new TemplateAccessException();
			}
		}

		private short _x;
		public short nX
		{
			get => _x;
			set
			{
				if (!Locked) _x = value;
				else throw new TemplateAccessException();
			}
		}

		private short _y;
		public short nY
		{
			get => _y;
			set
			{
				if (!Locked) _y = value;
				else throw new TemplateAccessException();
			}
		}

		// this is what we name our portal scripts
		private string _script;
		public string Script
		{
			get => _script;
			set
			{
				if (!Locked) _script = value;
				else throw new TemplateAccessException();
			}
		}

		private int _index;
		public int nIndex
		{
			get => _index;
			set
			{
				if (!Locked) _index = value;
				else throw new TemplateAccessException();
			}
		}

		public MapPortalTemplate(int nTemplateID)
			: base(nTemplateID) { }

		public override string ToString()
		{
			return $"{TemplateId} @ ( {nX}, {nY} ) -> {nTargetMap}";
		}
	}
}