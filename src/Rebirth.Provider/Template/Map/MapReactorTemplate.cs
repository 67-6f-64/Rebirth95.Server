using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.Map
{
	/// <summary>
	/// This contains the data that is unique to each reactor on each map.
	/// </summary>
	public sealed class MapReactorTemplate : AbstractTemplate
	{
		private int _spawnindex;
		public int SpawnIndex
		{
			get => _spawnindex;
			set
			{
				if (!Locked) _spawnindex = value;
				else throw new TemplateAccessException();
			}
		}
		private string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (!Locked) _name = value;
				else throw new TemplateAccessException();
			}
		}
		private int _reactortime;
		public int ReactorTime
		{
			get => _reactortime;
			set
			{
				if (!Locked) _reactortime = value;
				else throw new TemplateAccessException();
			}
		}
		/// <summary>
		/// Flipped
		/// </summary>
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

		public MapReactorTemplate(int templateId)
			: base(templateId) { }
	}
}