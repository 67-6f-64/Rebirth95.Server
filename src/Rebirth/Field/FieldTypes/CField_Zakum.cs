using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Game;
using System.Collections.Generic;
using System.Drawing;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Field.FieldTypes
{
	public class CField_Zakum : CField
	{
		private const int EYE_OF_FIRE = 4001017;

		private const int BATTLE_TIME_SECONDS = 45 * 60;

		private const int ZAKUM1_MOB_ID = 8800000; // auto-spawns the next ones after it dies
		private const int ZAKUMARM1_MOB_ID = 8800003; // spawn this and 7 more increments

		private const int CHAOSZAKUM1_MOB_ID = 8800100;
		private const int CHAOSZAKUMARM1_MOB_ID = 8800103;

		private const int ZAKUM_MAP_ID = 280030000;
		private const int CHAOSZAKUM_MAP_ID = 280030001;

		private readonly TagRect DropRect;
		private readonly TagPoint SpawnPoint;

		private readonly int _exitmap;

		public bool IsChaosZakum { get; }

		private int nCyclesSinceDropDetected;

		public CField_Zakum(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			SpawnPoint = new TagPoint(-11, -215);

			DropRect = new TagRect(-185, -240, 185, -200);

			IsChaosZakum = MapId == CHAOSZAKUM_MAP_ID;
			_exitmap = IsChaosZakum ? 211042301 : 211042300; // door to (chaos) zakum
		}

		protected override void Init()
		{
			CreateFieldClock(BATTLE_TIME_SECONDS);

			base.Init();
		}

		protected override void OnUserEnter(Character pUser)
		{
			if (Users.Count <= 0) Init();

			base.OnUserEnter(pUser);
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

		public override void Update()
		{
			base.Update();

			if (nCyclesSinceDropDetected < 4)
			{
				if (nCyclesSinceDropDetected == 0)
				{
					if (!RemoveEyeOfFire(false)) return;
				}
				else if (nCyclesSinceDropDetected < 3)
				{
					// just wait
				}
				else
				{
					if (!RemoveEyeOfFire(true)) return;

					new FieldEffectPacket(FieldEffectTypes.FieldEffect.ChangeBGM)
					{
						sName = "Bgm06/FinalFight"
					}.Broadcast(this);
					BroadcastNotice("Zakum is summoned by the force of Eye of Fire.");
					if (IsChaosZakum)
					{
						SpawnZakum(CHAOSZAKUM1_MOB_ID, CHAOSZAKUMARM1_MOB_ID);
					}
					else
					{
						SpawnZakum(ZAKUM1_MOB_ID, ZAKUMARM1_MOB_ID);
					}
				}

				nCyclesSinceDropDetected += 1;
			}
		}

		protected override void OnClockEnd()
		{
			base.OnClockEnd();

			foreach (var user in new List<Character>(Users))
			{
				user.Action.SetField(_exitmap, 2);
			}

			Reset();
		}

		private void SpawnZakum(int dwBodyID, int dwArmID)
		{
			Mobs.CreateMob(dwBodyID, null, SpawnPoint.X, SpawnPoint.Y, 70,
				0xFC, 0, 0, MobType.ParentMob, null);


			for (var i = 0; i < 8; i++)
			{
				Mobs.CreateMob(dwArmID + i, null, SpawnPoint.X, SpawnPoint.Y, 70,
					0, 0, 0, MobType.SubMob, null);
			}
		}

		private bool RemoveEyeOfFire(bool bRemove)
		{
			int dwDropIdToRemove = 0;

			foreach (var drop in Drops)
			{
				if (drop.Item?.nItemID == EYE_OF_FIRE && DropRect.PointInRect(drop.Position.CurrentXY))
				{
					if (bRemove && drop.OwnerCharId == 69)
					{
						dwDropIdToRemove = drop.dwId;
						break;
					}
					else
					{
						drop.nLeaveType = DropLeaveType.TimeOut;
						drop.DropOwnType = DropOwnType.UserOwn;
						drop.OwnerCharId = 69;
						return true;
					}
				}
			}
			return Drops.Remove(dwDropIdToRemove);
		}

		protected override void Reset(bool bFromDispose = false)
		{
			nCyclesSinceDropDetected = 0;
			base.Reset(bFromDispose);
		}
	}
}
