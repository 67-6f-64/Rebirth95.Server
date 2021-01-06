using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Characters;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Server.Game;

namespace Rebirth.Field.FieldTypes
{
	// FIELDTYPE_HONTAIL = 0x3F5
	public class CField_Hontail : CField
	{
		public override int ReturnMapId => 240050500;
		private bool bChaos;
		private CMob CurSink;

		private bool bStarted;

		public CField_Hontail(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{ }

		protected override void Init()
		{
			Mobs.Clear();
			Drops.Clear();
			bStarted = false;
			bChaos = MapId % 2 == 1;
			int nTimeLeftSec;
			switch (MapId)
			{
				case 240060000: // normal
					nTimeLeftSec = 60 * 60;// 1 hour
					break;
				case 240060001: // chaos
					nTimeLeftSec = 120 * 60; // 2 hours
					break;
				default:
					nTimeLeftSec = (int)ParentInstance.CFieldMan
						.GetField(MapId - 100, nInstanceID)
						.tFieldTimerExpiration
						.SecondsUntilEnd() + 2;
					if (nTimeLeftSec < 0) nTimeLeftSec = 0;

					break;
			}
			CreateFieldClock(nTimeLeftSec);

			base.Init(); // doesnt currently do anything but might in the future
		}

		public override void OnUserLeave(Character pUser, bool bOnMigrateOut = false)
		{
			if (pUser != null)
			{
				if (pUser.Party != null)
				{
					if (pUser.Party.dwOwnerId == pUser.dwId && pUser.Party.Count > 1)
					{
						pUser.Party.ChangePartyBoss(pUser.dwId, false);
					}

					if (bOnMigrateOut)
					{
						pUser.Party.PlayerLeave(pUser.dwId, pUser.Stats.sCharacterName, true);
					}
				}

				base.OnUserLeave(pUser);
			}

			if (Users.Count <= 0) Reset();
		}

		public override bool OnMobDamaged(CMob mob, int nDamage)
		{
			if (MapId != 240060200 && MapId != 240060201) return true;

			if (CurSink is null)
			{
				OnClockEnd(); // bad :(
			}
			else
			{
				if (mob.nMobTemplateId != CurSink.nMobTemplateId)
				{
					CurSink.Damage(mob.Controller, nDamage, 0);
				}
				else
				{
					CurSink.BroadcastHP();
				}
			}

			return false;
		}

		public override void OnMobDie(CMob removedMob)
		{
			base.OnMobDie(removedMob); // has to come first so the next sink has been spawned (revived)

			int nextSinkID;

			switch (removedMob.nMobTemplateId)
			{
				case 8810026: // first dummy mob -> normal
					nextSinkID = 8810018;
					break;
				case 8810130: // first dummy mob -> chaos
					nextSinkID = 8810118;
					break;
				case 8810118: // first chaos sink
				case 8810119: // second chaos sink
				case 8810120: // third chaos sink
				case 8810121: // fourth chaos sink (fifth is last one)
					nextSinkID = removedMob.nMobTemplateId + 1;
					break;
				default:
					nextSinkID = 0;
					CurSink = null;
					break;
			}

			if (nextSinkID <= 0) return;

			CurSink = Mobs.FirstOrDefault(mob => mob.nMobTemplateId == nextSinkID);

			if (CurSink != null) return;

			BroadcastNotice($"An error has occurred. (Null sink {nextSinkID})");
			OnClockEnd(); // error
		}

		public override void Update()
		{
			base.Update();

			if (bStarted || Users.Count <= 0) return;

			switch (MapId)
			{
				case 240060200: // boss maps
				case 240060201:
					Mobs.CreateMob(bChaos ? 8810026 : 8810130, null, 130, 250, 0, 0xFF, 0, 0, MobType.Normal, null);
					//Mobs.CreateMob(bChaos ? 8810026 : 8810130, -1, 130, 250, 0); // temp haxed for now
					break;
			}
			bStarted = true;
		}

		protected override void OnClockEnd()
		{
			base.OnClockEnd();

			WarpMap(ReturnMapId, 0, 1, 0);

			Reset();
		}

		private void WarpMap(int nFieldId, int nInstanceId, byte nPortalId, byte nFH)
		{
			foreach (var user in Users.ToList())
			{
				user.Action.SetFieldInstance(nFieldId, nInstanceId, nPortalId, nFH);
			}
		}

		protected override void Reset(bool bFromDispose = false)
		{
			bStarted = false;
			CurSink = null;
			base.Reset(bFromDispose);
		}
	}
}
