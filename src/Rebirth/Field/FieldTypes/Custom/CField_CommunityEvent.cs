using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Server.Center;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace Rebirth.Field.FieldTypes.Custom
{
	public abstract class CField_CommunityEvent : CField
	{
		protected const string sEffectWin = "event/coconut/victory";
		protected const string sEffectLose = "event/coconut/lose";
		protected const string sSoundWin = "Coconut/Victory";
		protected const string sSoundLose = "Coconut/Failed";

		public override int ReturnMapId => 109050001; // event exit map for first few GMS events
		
		protected abstract int nEntranceMap { get; }
		public abstract string sEventName { get; }
		public abstract void DistributeRewards(List<Character> charsTo);

		/// <summary>
		/// Used to track how often we are broadcasting the join event message to the world
		/// </summary>
		protected int nWarningState;
		protected DateTime tStart;

		protected CField_CommunityEvent(WvsGame parentInstance, int nMapId, int nInstanceId) 
			: base(parentInstance, nMapId, nInstanceId)
		{
			tFieldTimerExpiration = DateTime.Now.AddMinutes(5);
			MasterManager.CharacterPool.Broadcast(CPacket.SystemMessage($"[Community Event] {sEventName} is about to start! Enter @event to join."));
		}

		public virtual CPortal GetStartPoint(Character c) => Portals.GetRandStartPoint();
		public override bool CanBeDestroyed() => nWarningState <= 0 && base.CanBeDestroyed();

		/// <summary>
		/// Make sure to always call this base when extending this class
		/// </summary>
		public virtual void Setup()
		{
			nWarningState = 5;
			QR = "0";

#if DEBUG
			CreateFieldClock(15); // for testing
#else
			CreateFieldClock(300); // 5 minutes
#endif
		}

		protected void CloseRegistration()
		{
			MasterManager.CharacterPool.Broadcast(CPacket.SystemMessage($"[Community Event] {sEventName} has closed for new entrants."));
			QR = "1";
			tStart = DateTime.Now;
		}

		protected virtual void EndEvent()
		{
			foreach (var user in new List<Character>(Users))
			{
				user.Action.SetField(ReturnMapId);
			}

			base.OnClockEnd();
			MasterManager.EventManager.EndCurrentEvent();
		}

		public override void Update()
		{
			if (nWarningState > 0)
			{
				var mins = Math.Ceiling(tFieldTimerExpiration.SecondsUntilEnd() / 60.0f);
				if (nWarningState > mins && mins > 0)
				{
					nWarningState -= 1;
					MasterManager.CharacterPool.Broadcast(CPacket.SystemMessage($"[Community Event] {nWarningState} minutes until {sEventName} begins!"));
				}
			}

			base.Update();
		}
	}
}
