using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using Rebirth.Common.Types;
using Rebirth.Tools;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Field.FieldTypes.Custom
{
	public abstract class CField_DungeonRaid : CField
	{
		protected const string INTRO_EFFECT = "evan/monster";
		protected const string CLEAR_EFFECT = "killing/clear";
		protected const string TIMEOUT_EFFECT = "praid/timeout";
		protected const string STAGE_EFFECT = "killing/first/stage";
		protected const string NUMBER_EFFECT = "killin/first/number/"; // add 1-5 after

		protected abstract int DungeonFieldIndex { get; }
		protected abstract FieldType FieldType { get; }
		public abstract int DungeonTimeSeconds { get; protected set; }
		protected abstract int WarpOutField { get; }
		protected abstract Dictionary<int, int> MapSuccession { get; }
		protected abstract bool TryProceed(Character c, int nPortalID);
		protected abstract void GiveRewards();
		protected abstract void InitMobs();
		protected abstract int NextPortalID();
		protected abstract int PreviousFieldID();
		protected abstract void SetDungeonTimeRemaining();
		public abstract override void OnMobDie(CMob removedMob);

		private DateTime SpawnTime;

		protected Dictionary<int, int> MobKills;
		protected int NextFieldID() => MapSuccession[MapId];
		protected bool WarpToNextField { get; set; }

		private bool bLost = false;
		private bool bStarted = false;

		protected int StageExp = 0;
		public int CumulativeExp = 0;
		protected int MobHpMultiplier = 35;
		protected int FinalBossHP = 0;
		protected int FinalBossMaxHP = 0;

		protected CField_DungeonRaid(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			MobKills = new Dictionary<int, int>();
		}

		protected override void OnUserEnter(Character pUser)
		{
			if (Users.Count <= 0)
			{
				SoftReset();
				SetDungeonTimeRemaining(); // must come before field clock creation
				CreateFieldClock(DungeonTimeSeconds + 3); // add a few seconds to offset any latency issues
			}

			base.OnUserEnter(pUser);

			new FieldEffectPacket(FieldEffect.Screen)
			{ sName = INTRO_EFFECT }
			.Broadcast(pUser, true);

			SpawnTime = DateTime.Now.AddSeconds(4); // 2 seconds from last char
		}

		protected void SoftReset()
		{
			WarpToNextField = false;
			bStarted = false;
			bLost = false;
			StageExp = 0;
			CumulativeExp = 0;
			MobKills.Clear();
			Mobs.aMobGen.Clear(); // clear default spawns so we can add our own
			Mobs.Clear();
			Npcs.Clear();
		}

		public override void Update()
		{
			base.Update();

			if (!bStarted && SpawnTime.SecondsUntilEnd() <= 0)
			{
				bStarted = true;
				InitMobs();
				BuffMobs();
			}
		}

		protected virtual void BuffMobs()
		{
			foreach (var mob in Mobs)
			{
				mob.MaxHp *= MobHpMultiplier;
				mob.Stats.HP = mob.MaxHp;
				FinalBossMaxHP += mob.MaxHp;
			}
			FinalBossHP = FinalBossMaxHP;
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

		public override void OnUserEnterPortal(Character c, string sPortalName)
		{
			var portal = Portals.FindPortal(sPortalName);

			if (TryProceed(c, portal?.nIdx ?? 0))
			{
				WarpMap(NextFieldID(), nInstanceID, (byte)NextPortalID(), 0);
			}
			else
			{
				c.Action.Enable();
			}
		}

		protected void WarpMap(int nFieldID, int nInstanceUID, byte nPortalID, byte nFH)
		{
			foreach (var user in new List<Character>(Users))
			{
				user.Action.SetFieldInstance(nFieldID, nInstanceUID, nPortalID, nFH);
			}
		}

		protected override void OnClockEnd()
		{
			base.OnClockEnd();

			if (WarpToNextField)
			{
				WarpMap(NextFieldID(), nInstanceID, (byte)NextPortalID(), 0);
			}
			else
			{
				if (!bLost) // show animation, reset timer for warpout
				{
					BroadcastWZMapEffect(TIMEOUT_EFFECT);

					tFieldTimerExpiration = DateTime.Now.AddSeconds(4); // invisible timer
					Mobs.Clear();
					bLost = true;
				}
				else // warp out
				{
					GiveRewards(); // rewards are also distributed in OnMobDie() in all cases except timer expiring
					WarpMap(WarpOutField, 0, 0, 0); // TODO portal & FH IDs
					Reset();
				}
			}
		}
	}
}
