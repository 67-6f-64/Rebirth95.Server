using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Map
{
	public sealed class MapLifeTemplate : AbstractTemplate
	{
		private short _cy;
		public short CY
		{
			get => _cy;
			set
			{
				if (!Locked) _cy = value;
				else throw new TemplateAccessException();
			}
		}

		private bool _hide;
		public bool Hide
		{
			get => _hide;
			set
			{
				if (!Locked) _hide = value;
				else throw new TemplateAccessException();
			}
		}

		private bool _f;
		public bool F
		{
			get => _f;
			set
			{
				if (!Locked) _f = value;
				else throw new TemplateAccessException();
			}
		}

		private short _foothold;
		public short Foothold
		{
			get => _foothold;
			set
			{
				if (!Locked) _foothold = value;
				else throw new TemplateAccessException();
			}
		}

		private int _mobtime;
		public int MobTime
		{
			get => _mobtime;
			set
			{
				if (!Locked) _mobtime = value;
				else throw new TemplateAccessException();
			}
		}

		private string _type;
		public string Type
		{
			get => _type;
			set
			{
				if (!Locked) _type = value;
				else throw new TemplateAccessException();
			}
		}

		private short _rx0;
		public short RX0
		{
			get => _rx0;
			set
			{
				if (!Locked) _rx0 = value;
				else throw new TemplateAccessException();
			}
		}

		private short _rx1;
		public short RX1
		{
			get => _rx1;
			set
			{
				if (!Locked) _rx1 = value;
				else throw new TemplateAccessException();
			}
		}

		private short _x;
		public short X
		{
			get => _x;
			set
			{
				if (!Locked) _x = value;
				else throw new TemplateAccessException();
			}
		}

		private short _y;
		public short Y
		{
			get => _y;
			set
			{
				if (!Locked) _y = value;
				else throw new TemplateAccessException();
			}
		}

		public MapLifeTemplate(int templateId)
			: base(templateId) { }
	}
}