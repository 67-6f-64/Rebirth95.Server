using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rebirth.Characters.Combat
{
	public enum DamageTrackerNotificationLevel
	{
		Nothing = 0,
		OnEnd = 1,
		Everything = 2,
	}

	public class DamageTracker
	{
		public Character Parent { get; private set; }
		public Stopwatch Timer { get; private set; }
		public bool bActive => Timer.IsRunning;

		public int nAttackCount { get; set; }
		public long nDamageRecorded { get; set; }

		public DamageTrackerNotificationLevel NotificationLevel { get; private set; }

		public DamageTracker(Character c)
		{
			Parent = c;
			Timer = new Stopwatch();
			NotificationLevel = DamageTrackerNotificationLevel.Everything;
			// TODO maybe add a timestamp for last recorded attack and then stop timer if no attacks in X minutes (hook to user update func)
		}

		public void Dispose()
		{
			Parent = null;
			Timer.Stop();
			Timer = null;
		}

		public void ApplyAttack(int nAttacks, int nDamageAmount)
		{
			if (!bActive) return;

			if (nAttackCount + 1 /*nAttacks*/ >= int.MaxValue)
			{
				nAttackCount = int.MaxValue;
				Stop("Attack count exceeded recordable amount. Stopping tracker.");
			}
			else
			{
				nAttackCount += 1; // nAttacks;
			}

			if (nDamageRecorded + nDamageAmount >= long.MaxValue)
			{
				nDamageRecorded = long.MaxValue;
				Stop("Damage exceeded recordable amount. Stopping tracker.");
			}
			else
			{
				nDamageRecorded += nDamageAmount;
			}

			if (NotificationLevel == DamageTrackerNotificationLevel.Everything)
			{
				Notify(ToString());
			}
		}

		public void Reset()
		{
			if (bActive)
			{
				Stop();
			}
			Notify("Tracker reset.");
			nAttackCount = 0;
			nDamageRecorded = 0;
			Timer.Reset();
		}

		public void Stop(string sReason = "Tracker stopped.")
		{
			if (bActive)
			{
				Notify(sReason);
				Notify(ToString());
			}
			else
			{
				Notify("Timer is not running.");
			}
			Timer.Stop();
		}

		public void Start()
		{
			if (!bActive)
			{
				Notify($"Starting timer. {ToString()}");
			}
			else
			{
				Notify("Timer is already running.");
			}
			Timer.Start();
		}

		public void Notify(string sMsg)
		{
			Parent.SendMessage(sMsg);
		}

		public override string ToString() => $"Seconds elapsed: {Timer.ElapsedMilliseconds / 1000}. Damage recorded: {nDamageRecorded}. Average damage: {(nAttackCount == 0 ? 0 : nDamageRecorded / nAttackCount)}. Attacks recorded: {nAttackCount}";
	}
}
