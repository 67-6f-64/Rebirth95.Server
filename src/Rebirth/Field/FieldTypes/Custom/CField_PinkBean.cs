using Npgsql;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Center.GameData.DropInfo;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebirth.Field.FieldTypes.Custom
{
	public class CField_PinkBean : CField
	{
		public override int ReturnMapId => 270050300;

		private CMob CurSink { get; set; }

		const int FIRST_DUMMY_MOB = 8820008;
		const int SECOND_DUMMY_MOB = 8820009;

		const int IMMORTAL_PB = 8820000;
		const int MORTAL_PB = 8820001;

		const int FIRST_DAMAGE_SINK = 8820010;
		const int LAST_DAMAGE_SINK = 8820014;

		const int ARIEL_BATTLE_STATUE = 8820002;
		const int SOLOMON_BATTLE_STATUE = 8820003;
		const int REX_BATTLE_STATUE = 8820004;
		const int HUGIN_BATTLE_STATUE = 8820005;
		const int MUNIN_BATTLE_STATUE = 8820006;

		private bool IsStatue(int nMobTemplateId)
			=> (nMobTemplateId >= 8820002 && nMobTemplateId <= 8820006) // statues
			|| (nMobTemplateId >= 8820015 && nMobTemplateId <= 8820018); // more statues

		private bool IsDamageSink(int nMobTemplateId)
			=> nMobTemplateId >= FIRST_DAMAGE_SINK && nMobTemplateId <= LAST_DAMAGE_SINK;

		public CField_PinkBean(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId) { }

		protected override void Init()
		{
			Npcs.Add(new CNpc(2141000)
			{
				Y = -236,
				X = -47,
				Cy = -47,
				Foothold = 35, // seems like all the life in this map is on fh 35 ._.
				Position = new CMovePath() { X = -236, Y = -47 },
			});
			CreateFieldClock(2 * 60 * 60); // 2 hours
			base.Init();
		}

		public override bool OnMobDamaged(CMob mob, int nDamage)
		{
			if (IsStatue(mob.nMobTemplateId))
			{
				CurSink.Damage(CurSink.Controller, nDamage, 0);
			}
			else if (IsDamageSink(mob.nMobTemplateId))
			{
				return true;
			}

			return mob.nMobTemplateId == MORTAL_PB;
		}

		public override void OnInsertMob(CMob mob)
		{
			if (IsStatue(mob.nMobTemplateId))
			{
				switch (mob.nMobTemplateId)
				{
					case FIRST_DUMMY_MOB:
						remove(8820020, 8820024); // solomon
						break;
					case 8820002: // ariel
						remove(8820019);
						break;
					case 8820003: // solomon
					case 8820015:
						remove(8820020, 8820024);
						break;
					case 8820004: // rex
					case 8820016:
						remove(8820021, 8820025);
						break;
					case 8820005: // hugin
					case 8820017:
						remove(8820022, 8820026);
						break;
					case 8820006: // munin
					case 8820018:
						remove(8820023, 8820027);
						break;
				}
			}
			else if (mob.nMobTemplateId >= FIRST_DAMAGE_SINK && mob.nMobTemplateId <= LAST_DAMAGE_SINK)
			{
				CurSink = mob;
			}

			void remove(params int[] mobids)
			{
				foreach (var _mob in Mobs)
				{
					if (mobids.Contains(_mob.nMobTemplateId))
					{
						//Mobs.Remove(_mob);
						_mob.Stats.HP = 0; // roundabout way of force-removing it in the next field update cycle
						Broadcast(_mob.MakeLeaveFieldPacket());
					}
				}
			}
		}

		public override void OnMobDie(CMob removedMob)
		{
			// remove statues
			switch (removedMob.nMobTemplateId)
			{
				case FIRST_DUMMY_MOB:
					break;
				case SECOND_DUMMY_MOB:
					// do nothing
					break;
				case FIRST_DAMAGE_SINK:
					remove(8820020, 8820024); // solomon
					remove(8820021, 8820025); // rex
					break;
				case 8820011: // second
					remove(8820020, 8820024); // solomon
					remove(8820021, 8820025); // rex
					remove(8820022, 8820026); // hugin
					break;
				case 8820012: // third
					remove(8820020, 8820024); // solomon
					remove(8820021, 8820025); // rex
					remove(8820022, 8820026); // hugin
					remove(8820023, 8820027); // munin
					break;
				case 8820013: // fourth
					remove(8820020, 8820024); // solomon
					remove(8820021, 8820025); // rex
					remove(8820022, 8820026); // hugin
					remove(8820023, 8820027); // munin
					remove(8820019); // ariel
					break;
				case LAST_DAMAGE_SINK:
					CurSink = null;
					remove(IMMORTAL_PB);
					// remove immortal pb
					//var pb = Mobs.FirstOrDefault(mob => mob.nMobTemplateId == IMMORTAL_PB);
					//Mobs.Remove(pb);
					break;
			}

			if (removedMob.nMobTemplateId == MORTAL_PB)
			{
				base.OnMobDie(removedMob); // give rewards
			}
			else
			{
				removedMob.DistributeEXP(); // they always give exp
			}

			void remove(params int[] mobids)
			{
				foreach (var _mob in Mobs)
				{
					if (mobids.Contains(_mob.nMobTemplateId))
					{
						//Mobs.Remove(_mob);
						_mob.Stats.HP = 0; // roundabout way of force-removing it in the next field update cycle
						//Broadcast(_mob.MakeLeaveFieldPacket());
					}
				}
			}
		}

		protected override void OnClockEnd()
		{
			base.OnClockEnd();

			foreach (var user in new List<Character>(Users))
			{
				user.Action.SetFieldInstance(ReturnMapId, nInstanceID, 0, 0); // TODO correct portal
			}

			Reset();
		}

		protected override void Reset(bool bFromDispose = false)
		{
			CurSink = null;
			base.Reset(bFromDispose);
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
	}
}
