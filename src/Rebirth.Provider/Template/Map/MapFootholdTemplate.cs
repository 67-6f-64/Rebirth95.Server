using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Map
{
	public sealed class MapFootholdTemplate : AbstractTemplate
	{
		private byte _next;
		public byte Next
		{
			get => _next;
			set
			{
				if (!Locked) _next = value;
				else throw new TemplateAccessException();
			}
		}
		private byte _prev;
		public byte Prev
		{
			get => _prev;
			set
			{
				if (!Locked) _prev = value;
				else throw new TemplateAccessException();
			}
		}
		private short _x1;
		public short X1
		{
			get => _x1;
			set
			{
				if (!Locked) _x1 = value;
				else throw new TemplateAccessException();
			}
		}
		private short _x2;
		public short X2
		{
			get => _x2;
			set
			{
				if (!Locked) _x2 = value;
				else throw new TemplateAccessException();
			}
		}
		private short _y1;
		public short Y1
		{
			get => _y1;
			set
			{
				if (!Locked) _y1 = value;
				else throw new TemplateAccessException();
			}
		}
		private short _y2;
		public short Y2
		{
			get => _y2;
			set
			{
				if (!Locked) _y2 = value;
				else throw new TemplateAccessException();
			}
		}

		public MapFootholdTemplate(int templateId)
			: base(templateId) { }
	}
}