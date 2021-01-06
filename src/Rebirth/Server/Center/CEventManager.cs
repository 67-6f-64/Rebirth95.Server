using System;
using Rebirth.Characters;
using Rebirth.Field.FieldTypes;
using Rebirth.Field.FieldTypes.Custom;
using Rebirth.Server.Game;

namespace Rebirth.Server.Center
{
	public class CEventManager
	{
		private const int nEventIntervalMinutes = 75; // 1:15hr
		private DateTime tLastEventStart;
		private CField_CommunityEvent CurrentEventMap;
		private WvsGame Parent;

		public CEventManager(WvsGame parent)
		{
			tLastEventStart = DateTime.Now.AddMinutes(-nEventIntervalMinutes + 5);
			Parent = parent;
			MasterManager.Log.Info("CommunityEventManager loaded.");
		}

		public void TryDoEvent(bool bForce)
		{
			if (CurrentEventMap != null) return; // cant have two events running at once

			if (tLastEventStart.SecondsSinceStart() <= nEventIntervalMinutes * 60 && !bForce) return;

			CurrentEventMap = RandomEvent();

			if (CurrentEventMap is null)
			{
				tLastEventStart = DateTime.Now;
				//throw new NullReferenceException("CurrentEventMap is null in CEventManager.");
				return; // json memes
			}

			CurrentEventMap.Setup();

			tLastEventStart = DateTime.Now;
		}

		/// <summary>
		/// Sets the CurrentEventMap field to null
		/// </summary>
		public void EndCurrentEvent()
		{
			// warpout and map reset is done in event map
			CurrentEventMap = null;
		}

		public bool EventInProgress()
		{
			if (CurrentEventMap is null) return false;

			return CurrentEventMap.QR.Equals("1");
		}

		public void TryJoinEvent(Character c)
		{
			if (c.Stats.nLevel < 30)
			{
				c.SendMessage("You must be level 30 or above in order to join an event.");
			}
			else if (CurrentEventMap is null)
			{
				c.SendMessage("There is no active event currently. Please check back again later!");
			}
			else if (EventInProgress())
			{
				c.SendMessage("The event has already started. Sorry :(");
			}
			else if (c.Field.Template.HasMigrateLimit())
			{
				c.SendMessage("Cannot join event from current map.");
			}
			else if (c.Socket.ChannelId != 0)
			{
				c.SendMessage("You must be on channel 1 in order to join the event.");
			}
			else
			{
				var curMap = c.Field.MapId;

				var qr = c.Quests[QuestConstants.EVENT_PREVMAP_QID]?.sQRValue ?? string.Empty;

				if (int.TryParse(qr, out var qrInt) && qrInt == CurrentEventMap.MapId)
				{
					c.Quests.UpdateQuestRecordInternal(QuestConstants.EVENT_PREVMAP_QID, c.Field.ReturnMapId);
				}
				else if (curMap != CurrentEventMap.MapId)
				{
					c.Quests.UpdateQuestRecordInternal(QuestConstants.EVENT_PREVMAP_QID, curMap);
				}

				c.Stats.dwPosMap = CurrentEventMap.MapId;
				c.Position.MoveAction = 2; // stand

				c.Field.OnUserLeave(c);

				var portal = CurrentEventMap.GetStartPoint(c);

				c.Position.X = portal.nX;
				c.Position.Y = portal.nY;

				CurrentEventMap.AddClient(c.Socket);
			}
		}

		private CField_CommunityEvent RandomEvent()
		{
			const int RUSSIAN_ROULLETTE_MAP = 910030000;
			const int SNOWBALL_MAP = 109060000;

			return Parent.CFieldMan.GetField(RUSSIAN_ROULLETTE_MAP, RUSSIAN_ROULLETTE_MAP) as CField_RussianRoulette;

			if (Constants.Rand.Next() % 2 == 0)
			{
				return Parent.CFieldMan.GetField(RUSSIAN_ROULLETTE_MAP, RUSSIAN_ROULLETTE_MAP) as CField_RussianRoulette;
			}
			else
			{
				return Parent.CFieldMan.GetField(SNOWBALL_MAP, SNOWBALL_MAP) as CField_Snowball;
			}
		}
	}
}
