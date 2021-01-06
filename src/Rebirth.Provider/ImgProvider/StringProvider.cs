using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using Rebirth.Common.Tools;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.String;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.WzToolsEx;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class StringProvider : AbstractProvider<GenericKeyedTemplate<StringTemplate>>
	{
		[JsonIgnore]
		protected override string ProviderName => "String";
		[JsonIgnore]
		protected override bool LoadFromJSON => false;

		[JsonIgnore]
		protected override int Count
		{
			get
			{
				var num = 0;

				foreach (var x in this)
				{
					num += x.Value.Count;
				}

				return num;
			}
		}

		public StringProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var file in imgDir.Files)
			{
				switch (file.Name.Split('.')[0])
				{
					case "Cash":
					case "Consume":
					case "Eqp":
					case "Etc":
					case "Ins":
					case "Pet":
						ProcessStringNameSpaceFile(StringDataType.Item, file);
						break;
					case "Map":
						ProcessStringNameSpaceFile(StringDataType.Map, file);
						break;
					case "Mob":
						ProcessStringNameSpaceFile(StringDataType.Mob, file);
						break;
					case "Npc":
						ProcessStringNameSpaceFile(StringDataType.Npc, file);
						break;
					case "Skill":
						ProcessStringNameSpaceFile(StringDataType.Skill, file);
						break;
				}
			}
		}

		protected override void ProcessAdditionalData()
		{
			using (var fStream = new StreamWriter("stringwz.tsv"))
			{
				foreach (var cat in this)
				{
					foreach (var stringEntry in cat.Value)
					{
						// INSERT INTO rebirth_world0.template.data.string_wz (type, template_id, name, description_clean) VALUES (
						fStream.WriteLine($"{cat.Key}	{stringEntry.Key}	{stringEntry.Value.Name}	{stringEntry.Value.DescriptionClean}"); //)");
					}
				}
			}
		}

		private void ProcessStringNameSpaceFile(StringDataType type, NameSpaceFile imgFile)
		{
			if (!Contains((int)type))
				InsertItem(new GenericKeyedTemplate<StringTemplate>((int)type));

			WzProperty dataBlob = imgFile.Object as WzFileProperty;

			if (dataBlob.Name.Equals("Eqp.img"))
			{
				foreach (var eqpTypeBlob in dataBlob["Eqp"] as WzProperty)
				{
					run(eqpTypeBlob.Value as WzProperty);
				}
			}
			else if (dataBlob.Name.Equals("Etc.img"))
			{
				run(dataBlob["Etc"] as WzProperty);
			}
			else if (dataBlob.Name.Equals("Map.img"))
			{
				foreach (var mapBlob in dataBlob)
				{
					run(mapBlob.Value as WzProperty);
				}
			}
			else
			{
				run(dataBlob);
			}

			void run(WzProperty blob)
			{
				foreach (var stringBlob in blob)
				{
					var templateId = Convert.ToInt32(stringBlob.Key);
					var prop = stringBlob.Value as WzProperty;

					if (prop.HasChild("bookName")) continue; // skill book, we only want skill names

					var pEntry = new StringTemplate(templateId);

					AssignProviderAttributes(pEntry, prop);

					this[type].Add(pEntry);

					// BEGIN PROPERTY AUTO POPULATION

					//foreach (var templateMember in pEntry.GetType().GetMembers())
					//{
					//	var memberData = templateMember.GetCustomAttributes();

					//	var providerAttribute = memberData
					//		.FirstOrDefault(md => md is ProviderAttributeAttribute);

					//	if (providerAttribute is ProviderAttributeAttribute paa)
					//	{
					//		var pi = pEntry.GetType().GetProperty(templateMember.Name);

					//		// if pi is null: throw -- should not happen

					//		var val = prop.GetImgPropVal(pi.PropertyType, paa.AttributeName);

					//		if (val.IsNullOrDefault() || val is string s && s.Length <= 0)
					//		{
					//			foreach (var attrName in paa.AltAttributeNames)
					//			{
					//				val = prop.GetImgPropVal(pi.PropertyType, attrName);

					//				if (!val.IsNullOrDefault() || val is string ss && ss.Length <= 0) break;
					//			}
					//		}

					//		pi.SetValue(pEntry, prop.GetImgPropVal(pi.PropertyType, paa.AttributeName));
					//	}
					//}

					// END PROPERTY AUTO POPULATION

					//this[type].Add(new StringTemplate(templateId)
					//{
					//	Name = prop.GetString("name"),
					//	StreetName = prop.GetString("streetName"),
					//	Description = prop.GetString("desc")
					//});

					//if (this[type][templateId].Name is null)
					//{
					//	this[type][templateId].Name = prop.GetString("mapName");
					//}

					//if (this[type][templateId].Name is null)
					//{
					//	throw new NullReferenceException($"Template name is null. ID: {templateId}");
					//}
				}
			}
		}

		public GenericKeyedTemplate<StringTemplate> this[StringDataType key] => base[(int)key];
	}
}
