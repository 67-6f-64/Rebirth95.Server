using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Characters.Quest;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth
{
	public partial class CPacket
	{
		/// <summary>
		/// Nexon doesn't process packets in CQuestMan, they're scattered around CUserLocal
		///		and CWvsContext, but I put them here for organization.
		/// </summary>
		public static class CQuestMan
		{
			public static COutPacket QuestCompleted(int dwQuestId)
			{
				var p = new COutPacket(SendOps.LP_QuestClear);
				p.Encode4(dwQuestId);
				return p;
			}

			public static COutPacket UserQuestResult(QuestResultType op, QuestEntry pQuest, int nNpcTemplateId = 0, short usNextQuestID = 0)
			{
				var p = new COutPacket(SendOps.LP_UserQuestResult);
				p.Encode1(op);

				switch (op)
				{
					case QuestResultType.Failed_Equipped:
					case QuestResultType.Failed_Meso:
					case QuestResultType.Failed_OnlyItem:
					case QuestResultType.Failed_Unknown:
						return p;
					case QuestResultType.Failed_Inventory:
					case QuestResultType.Reset_QuestTimer:
					case QuestResultType.Failed_TimeOver:
						p.Encode2(pQuest.nQuestID);
						return p;
					case QuestResultType.Success:
						p.Encode2(pQuest.nQuestID);
						p.Encode4(nNpcTemplateId);
						p.Encode2(usNextQuestID); // 0 if no next
						break;
				}

				return p;
			}

			public static COutPacket UserQuestResult_Timer(short dwQuestId, int tRemain, bool bRemove)
			{
				var p = new COutPacket(SendOps.LP_UserQuestResult);

				p.Encode1(bRemove ? QuestResultType.End_QuestTimer : QuestResultType.Start_QuestTimer);
				p.Encode2(1); // count
				p.Encode2(dwQuestId);

				if (!bRemove)
					p.Encode4(tRemain);

				return p;
			}

			public static COutPacket QuestRecordMessage(QuestEntry pQuest)
			{
				var p = new COutPacket(SendOps.LP_Message);
				p.Encode1(MessageType.QuestRecordMessage);
				p.Encode2(pQuest.nQuestID);
				p.Encode1((byte)pQuest.nState);

				switch (pQuest.nState)
				{
					case QuestActType.NotStarted:
						p.Encode1(0);
						// nothing
						break;
					case QuestActType.QuestAccept:
						p.EncodeString(pQuest.sQRValue);
						// accept time, idc -> used for a few quests to determine when it can be completed
						//p.EncodeDateTime(DateTime.Now.AddMinutes(-3));
						break;
					case QuestActType.QuestComplete:
						p.EncodeDateTime(pQuest.tCompleted);
						break;
				}

				return p;
			}
		}
	}
}
