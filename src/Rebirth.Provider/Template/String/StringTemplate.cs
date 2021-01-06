using Newtonsoft.Json;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template.String
{
	public sealed class StringTemplate : AbstractTemplate
	{
		private string _name;
		[ProviderProperty("name", "mapName")]
		public string Name
		{
			get => _name;
			set => _name = !Locked ? value ?? "" : throw new TemplateAccessException();
		}

		private string _description;
		[ProviderProperty("desc")]
		public string Description
		{
			get => _description;
			set => _description = !Locked ? value ?? "" : throw new TemplateAccessException();
		}

		private string _streetName;
		[ProviderProperty("streetName")]
		public string StreetName
		{
			get => _streetName;
			set => _streetName = !Locked ? value ?? "" : throw new TemplateAccessException();
		}

		[JsonIgnore]
		public string DescriptionClean => Description.Replace("\\n", " ").Replace("\t", " ");

		public StringTemplate(int templateId)
			: base(templateId)
		{
			Name = "";
			Description = "";
			StreetName = "";
		}
	}
}
