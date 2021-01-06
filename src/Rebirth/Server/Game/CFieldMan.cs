using Rebirth.Characters;
using Rebirth.Field;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;

namespace Rebirth.Server.Game
{
	/// <summary>
	/// This class serves as the field cache.
	/// Fields are recycled every x minutes if no character or miniroom exists in it.
	/// One of these exists for each channel instance.
	/// </summary>
	public class CFieldMan
	{
		public const int UpdateIntervalMillis = 1000;

		public WvsGame Parent { get; }

		private readonly Dictionary<int, CFieldEntry> m_aFields;

		public CFieldMan(WvsGame parent)
		{
			Parent = parent;
			m_aFields = new Dictionary<int, CFieldEntry>();
		}

		public static bool IsFieldValid(int nFieldID)
		{
			return MasterManager.MapTemplates[nFieldID] != null;
		}

		public void Update()
		{
			var toRemove = new List<CFieldEntry>();

			// have to copy the list cuz new field entries are created inside the update function
			// and when that happens it throws an enumeration error -> collection modified
			foreach (var kvp in new List<CFieldEntry>(m_aFields.Values))
			{
				kvp.Update();
				//TODO: Dispose of entry FieldEntrys
			}

			foreach (var item in toRemove)
			{
				item.Dispose();
				m_aFields.Remove(item.TemplateID);
			}
		}

		private CFieldEntry GetEntry(int nFieldID)
		{
			if (!m_aFields.TryGetValue(nFieldID, out var pOutField))
			{
				pOutField = new CFieldEntry(Parent, nFieldID);
				m_aFields.Add(nFieldID, pOutField);
			}

			return pOutField;
		}

		public CField GetField(int nFieldID, int nInstance = 0)
		{
			if (IsFieldValid(nFieldID))
			{
				return GetEntry(nFieldID).GetField(nInstance);
			}

			return null;
		}

		public bool ContainsField(int nFieldID, int nInstance = 0)
		{
			// calling function can take care of invalid fields
			if (!IsFieldValid(nFieldID)) return true;

			if (!m_aFields.ContainsKey(nFieldID)) return false;
			if (!m_aFields[nFieldID].ContainsInstance(nInstance)) return false;

			return true;
		}

		public Character GetCharacter(int dwCharID)
		{
			foreach (var item in m_aFields.Values)
			{
				if (item.GetCharacter(dwCharID) is Character c)
				{
					return c;
				}
			}

			return null;
		}

		public Character GetCharacter(string sCharName)
		{
			foreach (var item in m_aFields.Values)
			{
				if (item.GetCharacter(sCharName) is Character c)
				{
					return c;
				}
			}

			return null;
		}
	}
}
