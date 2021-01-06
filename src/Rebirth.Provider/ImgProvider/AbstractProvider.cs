using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using WzTools;
using WzTools.FileSystem;
using Rebirth.Provider.Template;
using Newtonsoft.Json;
using Rebirth.Common.Tools;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.WzToolsEx;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	/// <summary>
	/// Base class for all provider classes
	/// </summary>
	/// <typeparam name="TItem">Object of collection using base class AbstractTemplate</typeparam>
	public abstract class AbstractProvider<TItem> where TItem : AbstractTemplate
	{
		[JsonIgnore] protected static ILog Log = LogManager.GetLogger(typeof(AbstractProvider<TItem>));

		protected readonly Dictionary<int, TItem> _templates;

		[JsonIgnore] protected virtual int Count => _templates.Count;

		[JsonIgnore] protected virtual bool LoadFromJSON => false;

		/// <summary>
		/// Name of the base img folder.
		/// </summary>
		[JsonIgnore] protected abstract string ProviderName { get; }

		/// <summary>
		/// Calls LoadFromImg and then LockCollection. Outputs metrics to console.
		/// Requires an initialized WFileSystem object.
		/// </summary>
		/// <param name="baseFileSystem"></param>
		protected AbstractProvider(WzFileSystem baseFileSystem)
		{
			if (ProviderName is null || ProviderName.Length <= 0) throw new ArgumentNullException(nameof(ProviderName));
			if (baseFileSystem is null) throw new ArgumentNullException(nameof(baseFileSystem));
			if (baseFileSystem.SubDirectories.Count <= 0 && baseFileSystem.Files.Count <= 0)
				throw new ArgumentException("No sub directories or files exist. Verify the file system has been initialized.");

			var watch = new Stopwatch();
			watch.Start();

			_templates = new Dictionary<int, TItem>();

			var jsonFileName = baseFileSystem.BaseDir + @"json\" + ProviderName + ".json";
			var loadFromJson = LoadFromJSON && File.Exists(jsonFileName);

#if DEBUG
			if (loadFromJson)
			{
				RetrieveJsonCache(jsonFileName);
			}
			else
			{
#endif
				if (LoadFromJSON)
				{
					Log.Warn($"Unable to find {ProviderName} JSON. Fetching data from img instead.");
				}

				var dirs = ProviderName.Split('.');

				if (dirs.Length <= 0) throw new NullReferenceException("ProviderName is empty.");

				NameSpaceDirectory baseImgDir = baseFileSystem.SubDirectories
					.FirstOrDefault(item => item.Name.Equals(dirs[0]))
					?? throw new NullReferenceException($"Unable to find '{dirs[0]}' sub directory.");

				LoadFromImg(baseImgDir);
				SaveToJson(jsonFileName);

				baseImgDir.Files.ForEach(item => item.Unload());
#if DEBUG
			}
#endif
			ProcessAdditionalData();
			LockCollection(_templates.Values.ToList());

			watch.Stop();

			var time = $"{Math.Floor(watch.ElapsedMilliseconds * 0.001)}.{watch.ElapsedMilliseconds % 1000}";
			var type = loadFromJson ? "json" : "img";
			var nameSplit = ProviderName.Split('.');

			Log.Info($"{nameSplit[nameSplit.Length - 1] + "Provider",-20} -> Loaded {Count,5} items from {type,4} in {time,5} seconds.");
		}

		/// <summary>
		/// All the img loading logic goes in here. This is called from the base class constructor.
		/// </summary>
		/// <param name="imgDir"></param>
		protected abstract void LoadFromImg(NameSpaceDirectory imgDir);

		/// <summary>
		/// Can be used to populate additional collections or modify template data after the initial data is loaded.
		/// Gets called before collection templates are locked in the base class constructor.
		/// Is not required for basic functionality.
		/// </summary>
		protected virtual void ProcessAdditionalData() { }

		/// <summary>
		/// Uses the template property attributes to assign values to properties.
		/// </summary>
		/// <param name="pEntry">Target template to work on</param>
		/// <param name="prop">Base WZ property node</param>
		protected void AssignProviderAttributes(AbstractTemplate pEntry, WzProperty prop)
		{
			var pTypeInfo = pEntry.GetType();
			var pClassMembers = pTypeInfo.GetMembers();

			foreach (var templateMember in pClassMembers)
			{
				var apCustomAttributes = System.Attribute.GetCustomAttributes(templateMember);

				if (apCustomAttributes.Any(t => t is ProviderIgnoreAttribute)) continue;

				foreach (var attribute in apCustomAttributes)
				{
					if (attribute is ProviderPropertyAttribute paa)
					{
						var pi = pEntry.GetType().GetProperty(templateMember.Name);

						// if pi is null: throw -- should not happen

						SetProviderPropertyValue(pEntry, pi, prop, paa);
					}
					else if (attribute is ProviderListAttribute pla)
					{
						; // TODO

						var pi = pEntry.GetType().GetProperty(templateMember.Name);

						;
					}
				}
			}
		}

		private void SetProviderPropertyValue(AbstractTemplate pEntry, PropertyInfo pi, WzProperty dataProp, ProviderPropertyAttribute paa)
		{
			WzProperty realPropDir = dataProp;

			var propNameSplit = paa.AttributePath.Split('/');
			string attributeName;

			for (var i = 0; i < propNameSplit.Length - 1; ++i)
			{
				attributeName = propNameSplit[i];

				if (realPropDir is null) continue; // cant find path

				realPropDir = realPropDir.GetChild(attributeName) as WzProperty;
			}

			attributeName = propNameSplit[propNameSplit.Length - 1]; // should always be last of the split

			if (realPropDir is null) return; // cant find path

			var val = realPropDir.GetImgPropVal(pi.PropertyType, attributeName);

			if (val.IsNullOrDefault() || val is string s && s.Length <= 0)
			{
				foreach (var attrName in paa.AltPaths)
				{
					val = realPropDir.GetImgPropVal(pi.PropertyType, attrName);

					if (!val.IsNullOrDefault() || val is string ss && ss.Length <= 0) break;
				}
			}

			pi.SetValue(pEntry, val);
		}

		private void RetrieveJsonCache(string jsonFileName)
		{
			var json = File.ReadAllText(jsonFileName);
			var output = JsonConvert.DeserializeObject<Dictionary<int, TItem>>(json);

			foreach (var (id, template) in output)
			{
				if (_templates.ContainsKey(id)) continue;

				_templates.Add(id, template);
			}
		}

		private void SaveToJson(string jsonFileName)
		{
			var json = JsonConvert.SerializeObject(_templates, Formatting.Indented);
			File.WriteAllText(jsonFileName, json);
		}

		/// <summary>
		/// Locks all AbstractTemplates in the given collection.
		/// </summary>
		/// <param name="templates">Templates to lock</param>
		protected void LockCollection(List<TItem> templates)
		{
			if (templates is null) throw new ArgumentNullException(nameof(templates));
			if (templates.Count <= 0)
				throw new ArgumentException("Value cannot be an empty collection.", nameof(templates));

			templates.ForEach(item => item.LockTemplate());
		}

		/// <summary>
		/// Retrieves an AbstractTemplate from the collection
		/// </summary>
		/// <param name="key">Template key to retrieve. Must be positive value</param>
		/// <returns>AbstractTemplate if it exists, else null</returns>
		public virtual TItem this[int key]
		{
			get
			{
				if (key < 0) throw new ArgumentOutOfRangeException(nameof(key));
				return GetOrDefault(key);
			}
		}

		/// <summary>
		/// Retrieves an AbstractTemplate from the collection
		/// </summary>
		/// <param name="key">Object key to search for</param>
		/// <returns>AbstractTemplate if it exists, else null</returns>
		public TItem GetOrDefault(int key)
		{
			if (key < 0) throw new ArgumentOutOfRangeException(nameof(key));

			return _templates.TryGetValue(key, out var ret) ? ret : default;
		}

		/// <summary>
		/// Retrieves the AbstractTemplate at the given index
		/// </summary>
		/// <param name="index">Index to retrieve AbstractTemplate from</param>
		/// <returns>AbstractTemplate if index exists, else throws IndexException</returns>
		public TItem GetAtIndex(int index)
		{
			if (index <= 0) throw new ArgumentOutOfRangeException(nameof(index));
			return _templates[index];
		}

		public bool Contains(int key) => _templates.ContainsKey(key);
		public bool Contains(TItem item) => _templates.ContainsKey(GetKeyForItem(item));
		protected void InsertItem(TItem item) => _templates.Add(GetKeyForItem(item), item);

		protected virtual int GetKeyForItem(TItem item) => item.TemplateId;

		public Dictionary<int, TItem>.Enumerator GetEnumerator() => _templates.GetEnumerator();
		public Dictionary<int, TItem>.ValueCollection Values => _templates.Values;
		public Dictionary<int, TItem>.KeyCollection Keys => _templates.Keys;
	}
}
