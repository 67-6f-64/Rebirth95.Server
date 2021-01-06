using Rebirth.Characters;
using Rebirth.Field;
using Rebirth.Field.FieldTypes;
using Rebirth.Field.FieldTypes.Custom;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using Rebirth.Common.Types;

namespace Rebirth.Server.Game
{
	public class CFieldEntry : IDisposable
	{
		public WvsGame Parent { get; private set; }
		public int TemplateID { get; private set; }

		private readonly Dictionary<int, CFieldItem> m_aItems;

		public CFieldEntry(WvsGame parent, int nFieldID)
		{
			Parent = parent;
			TemplateID = nFieldID;

			m_aItems = new Dictionary<int, CFieldItem>();
		}

		public void Update()
		{
			var toRemove = new List<int>();

			foreach (var (key, item) in m_aItems)
			{
				var field = item.Field;

				var span = DateTime.Now - item.LastUpdate;

				if (span.TotalMilliseconds >= Constants.FieldDestroyInterval)
				{
					toRemove.Add(key);
				}
				else
				{
					if (field.CanBeDestroyed()) continue;

					field.Update();
					item.LastUpdate = DateTime.Now;
				}
			}

			foreach (var key in toRemove)
			{
				var pItem = m_aItems[key];
				pItem.Dispose();

				m_aItems.Remove(key);
			}
		}

		public CField GetField(int nInstance = 0)
		{
			if (m_aItems.TryGetValue(nInstance, out _)) return m_aItems[nInstance].Field;

			return CreateField(nInstance);
		}

		public bool ContainsInstance(int nInstance)
		{
			return m_aItems.ContainsKey(nInstance);
		}

		private CField CreateField(int nInstance = 0)
		{
			var pField = LoadNew(nInstance);
			var pOutFieldItem = new CFieldItem(pField);

			m_aItems.Add(nInstance, pOutFieldItem);

			return m_aItems[nInstance].Field;
		}

		private CField LoadNew(int nInstanceId)
		{
			var template = MasterManager.MapTemplates[TemplateID];

			CField entry;

			switch (template.FieldType)
			{
				case FieldType.WAITINGROOM:
					entry = new CField_WaitingRoom(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.CUSTOM_AREA_BOSS:
					entry =  new CField_AreaBoss(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.SNOWBALL:
					if (nInstanceId == 0) goto default;

					entry = new CField_Snowball(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.CUSTOM_PINKBEAN:
					if (nInstanceId == 0) goto default;

					entry = new CField_PinkBean(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.HONTAIL:
					if (nInstanceId == 0) goto default;

					entry = new CField_Hontail(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.ZAKUM:
				case FieldType.CHAOSZAKUM:
					if (nInstanceId == 0) goto default;

					entry = new CField_Zakum(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.DOJANG:
					if (nInstanceId == 0) goto default;

					entry = new CField_Dojang(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.CUSTOM_DUNGEONRAID_GOLEMTEMPLE:
					if (nInstanceId == 0) goto default;

					entry = new CField_DungeonRaid_GolemTemple(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.CUSTOM_COMMUNITYEVENT_RUSSIANROULETTE:
					if (nInstanceId == 0) goto default;

					entry = new CField_RussianRoulette(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.PERSONALTIMELIMIT:
					if (nInstanceId == 0) goto default;

					entry = new CField_PersonalTimeLimit(Parent, TemplateID, nInstanceId);
					break;
				case FieldType.BATTLEFIELD:
					if (nInstanceId == 0) goto default;
					
					entry = new CField_Battlefield(Parent, TemplateID, nInstanceId);
					break;
				default:
					entry = new CField(Parent, TemplateID, nInstanceId);
					break;
			}

			entry.Portals.Load(template);
			entry.Footholds.Load(template);

			entry.Mobs.Load(TemplateID);
			entry.Npcs.Load(TemplateID);
			entry.Reactors.Load(TemplateID);

			return entry;
		}

		public void Dispose()
		{
			foreach (var kvp in m_aItems)
				kvp.Value.Dispose();

			m_aItems.Clear();
		}

		public Character GetCharacter(int dwCharID)
		{
			foreach (var item in m_aItems.Values)
			{
				if (item.Field.Users[dwCharID] is Character c)
				{
					return c;
				}
			}
			return null;
		}

		public Character GetCharacter(string sCharName)
		{
			foreach (var item in m_aItems.Values)
			{
				foreach (var user in item.Field.Users)
				{
					if (user.Stats.sCharacterName.EqualsIgnoreCase(sCharName))
					{
						return user;
					}
				}
			}
			return null;
		}
	}
}
