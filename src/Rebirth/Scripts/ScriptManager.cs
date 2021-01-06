using Microsoft.Extensions.Configuration;
using Rebirth.Characters.Quest;
using Rebirth.Client;
using Rebirth.Field;
using Rebirth.Field.FieldObjects;
using Rebirth.Scripts.Item;
using Rebirth.Scripts.Map;
using Rebirth.Scripts.Npc;
using Rebirth.Scripts.Portal;
using Rebirth.Scripts.Quest;
using Rebirth.Scripts.Reactor;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.IO;

namespace Rebirth.Scripts
{
	public enum ScriptType
	{
		Event, // todo (do we even want this tho??) // no we dont
		Item, // done
		Field, // done
		Npc, // done
		Portal, // done
		Quest, // done
		Reactor // done
	}

	public class ScriptManager
	{
		private Dictionary<string, string> m_aScripts;

		public ScriptManager(IConfiguration config)
		{
			m_aScripts = new Dictionary<string, string>(64);
		}

		public void ClearScriptCache()
		{
			m_aScripts.Clear();
		}

		private string GetScriptContents(ScriptType type, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return null;

			string path;

			if (name == "default")
			{
				path = Path.Combine(Constants.ScriptsFolderPath, $"{name}.py");
			}
			else
			{
				path = Path.Combine(Constants.ScriptsFolderPath, $"{type}", $"{name}.py");
			}

			if (!File.Exists(path))
				return null;

			if (!m_aScripts.TryGetValue(path, out var szContents))
			{
				szContents = File.ReadAllText(path);
#if RELEASE
				m_aScripts.Add(path, szContents); //Comment this out for hot reloading scripts
#endif
			}

			return szContents;
		}

		private string GetScript(ScriptType type, string name, WvsGameClient c)
		{
			var szContents = GetScriptContents(type, name);

			if (szContents == null)
			{
				// no need to tell players the script doesnt work, theyll know cuz it doesnt do anything
#if DEBUG
				c.Character.SendMessage("Unable to find script with name: " + name);
#endif

				if (type == ScriptType.Npc)
				{
					szContents = GetScriptContents(type, "default");
				}
				else
				{
					return "";
				}
			}
			else
			{
#if DEBUG
				c.Character.SendMessage("Opening script with name: " + name);
#endif
			}

			return szContents;
		}

		public NpcScript GetNpcScript(int nNpcID, WvsGameClient c)
		{
			var sScriptName = MasterManager.NpcTemplates[nNpcID]?.Script;

			if (sScriptName is null || sScriptName.Length <= 0) sScriptName = nNpcID.ToString();

			string subname;
			if (sScriptName.ToLower().StartsWith("zakum"))
			{
				subname = "zakum";
			}
			else if (sScriptName.ToLower().StartsWith("info"))
			{
				subname = "info";
			}
			else if (sScriptName.StartsWith("gachapon"))
			{
				subname = "gachapon";
			}
			else if (sScriptName.StartsWith("Proof"))
			{
				subname = "nana";
			}
			else if (sScriptName.StartsWith("hair_") || sScriptName.StartsWith("NLC_Hair"))
			{
				subname = "hair";
			}
			else if (sScriptName.StartsWith("skin_") || sScriptName.StartsWith("NLC_Skin"))
			{
				subname = "skin";
			}
			else if (sScriptName.StartsWith("guild_"))
			{
				subname = "guild";
			}
			else if (sScriptName.StartsWith("florina"))
			{
				subname = "florina";
			}
			else if (sScriptName.StartsWith("contimove"))
			{
				subname = "contimove";
			}
			else
			{
				subname = sScriptName;
			}

			var script = GetScript(ScriptType.Npc, subname, c);

			return new NpcScript(nNpcID, sScriptName, script, c);
		}

		public PortalScript GetPortalScript(string sPortalScriptName, WvsGameClient c)
		{
			string subname;
			if (sPortalScriptName.StartsWith("market")) // todo maybe clean this up or move the handling somewhere else
			{
				subname = "market";
			}
			else if (sPortalScriptName.StartsWith("zakum"))
			{
				subname = "zakum";
			}
			else if (sPortalScriptName.StartsWith("rien") || sPortalScriptName.Equals("enterport") || sPortalScriptName.Equals("entermcave"))
			{
				subname = "rien";
			}
			else
			{
				subname = sPortalScriptName;
			}

			var script = GetScript(ScriptType.Portal, subname, c);

			return new PortalScript(sPortalScriptName, script, c);
		}

		// called for normal map scripts (not FUE)
		public MapScript GetMapScript(string sMapScriptName, WvsGameClient c)
		{
			var script = GetScript(ScriptType.Field, sMapScriptName, c);

			return new MapScript(sMapScriptName, script, c);
		}

		// called for first user enter scripts
		public MapScript GetMapScript(string sMapScriptName, CField oField, WvsGameClient c)
		{
			var script = GetScript(ScriptType.Field, sMapScriptName, c);

			return new MapScript(sMapScriptName, script, oField, c);
		}

		public ItemScript GetItemScript(string sItemScriptName, int nItemID, short nItemPOS, WvsGameClient c)
		{
			var script = GetScript(ScriptType.Item, sItemScriptName, c);

			return new ItemScript(sItemScriptName, script, nItemID, nItemPOS, c);
		}

		public ReactorScript GetReactorScript(string sReactorScriptName, CReactor oReactor, WvsGameClient c, bool bHit)
		{
			var script = GetScript(ScriptType.Reactor, sReactorScriptName, c);

			return new ReactorScript(sReactorScriptName, script, oReactor, c, bHit);
		}

		public QuestScript GetQuestScript(int nQuestID, int nAct, QuestEntry pQuest, WvsGameClient c)
		{
			var sScriptName = $"{nQuestID}";

			var script = GetScript(ScriptType.Quest, sScriptName, c);

			if (script.Length <= 0) return null;

			return new QuestScript(sScriptName, nAct, script, pQuest, c);
		}
	}
}
