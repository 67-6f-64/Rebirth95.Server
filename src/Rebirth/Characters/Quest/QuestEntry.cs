using System;
using System.Collections.Generic;
using Rebirth.Provider.Template.Quest;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Quest
{
	public class QuestEntry
	{
		public short nQuestID { get; set; } // usQRKey
		public QuestTemplate Template => MasterManager.QuestTemplates[nQuestID];
		public string sQRValue { get; set; } = string.Empty;
		public QuestActType nState { get; set; } = QuestActType.NotStarted;
		public DateTime tCompleted { get; set; } = DateTime.MinValue;
		public bool IsComplete => tCompleted > DateTime.MinValue;
		public Dictionary<int, QuestDemandRecord> DemandRecords { get; }

		public QuestEntry(short nQuestId)
		{
			nQuestID = nQuestId;

			if (Template is null) return; // internal quest tracking

			DemandRecords = new Dictionary<int, QuestDemandRecord>();

			foreach (var req in Template.EndDemand.DemandMob)
			{
				DemandRecords.Add(req.MobID, new QuestDemandRecord
				{
					nKey = req.MobID,
					nType = QuestDemandType.Mob
				});
			}

			foreach (var req in Template.EndDemand.DemandMap)
			{
				DemandRecords.Add(req.MapID, new QuestDemandRecord
				{
					nKey = req.MapID,
					nType = QuestDemandType.Map
				});
			}
		}
	}
}