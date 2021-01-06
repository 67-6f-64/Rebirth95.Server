using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Provider.Attribute
{
	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class ProviderPropertyAttribute : System.Attribute
	{
		public string AttributePath { get; } // TODO maybe better name for this??
		public string[] AltPaths { get; }

		public ProviderPropertyAttribute(string path, params string[] altPaths)
		{
			AttributePath = path;
			AltPaths = altPaths ?? new string[0];
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ProviderClassAttribute : System.Attribute
	{
		public string AttributePath { get; set; }

		public ProviderClassAttribute()
		{ }

		public ProviderClassAttribute(string path)
		{
			AttributePath = path;
		}
	}

	/// <summary>
	/// Assumes that the property is a generic List of type T
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ProviderListAttribute : System.Attribute
	{

	}
}
