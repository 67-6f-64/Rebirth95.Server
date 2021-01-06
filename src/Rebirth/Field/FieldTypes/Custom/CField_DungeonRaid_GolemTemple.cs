using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Characters;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Game;
using Rebirth.Tools;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Field.FieldTypes.Custom
{
	public sealed class CField_DungeonRaid_GolemTemple : CField_DungeonRaid
	{
		public override int ReturnMapId => WarpOutField;

		protected override int DungeonFieldIndex { get; }
		protected override FieldType FieldType => FieldType.CUSTOM_DUNGEONRAID_GOLEMTEMPLE;
		public override int DungeonTimeSeconds { get; protected set; }
		protected override int WarpOutField => 100000000; // Golem's Temple Entrance
		protected override Dictionary<int, int> MapSuccession { get; }
		protected override int NextPortalID() => 3;
		protected override int PreviousFieldID() => MapId - 100;

		private int MobExpDenominator = 100;
		private DateTime tLastBossHpSent;

		public CField_DungeonRaid_GolemTemple(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			MapSuccession = new Dictionary<int, int> // <FieldFrom, FieldTo>
			{
				{ 100040100, 100040200 }, // first to second map
				{ 100040200, 100040300 }, // second to third map
				{ 100040300, 100040400 }, // third to fourth map
				{ 100040400, 100040500 }, // fourth to fifth map
				{ 100040500, WarpOutField } // fifth to warpout map
			};

			DungeonFieldIndex = nMapId % 1000 / 100;
		}

		protected override void InitMobs()
		{
			var mobcount = 30;

			const int npc = 1012119;

			switch (DungeonFieldIndex)
			{
				case 1:
					{
						var mobs = new int[2] { 9200009, 9500123 };
						var y = new int[5] { 295, 206, 46, -55, -249 };
						var x = new int[2][]
						{
							new int[5] { -20, 275, 290, 310, 310 }, // low range
							new int[5] { 1250, 1100, 980, 740, 730 }, // high range
						};
						execute(mobs, y, x);
						makenpc(-340, 298, 21);
					}
					break;
				case 2:
					{
						var mobs = new int[2] { 9500123, 9300387 };
						var y = new int[5] { 300, 105, 11, -80, -260 };
						var x = new int[2][]
						{
							new int[5] { -115, 136, 130, 232, 243 }, // low range
							new int[5] { 1166, 830, 735, 670, 662 }, // high range
						};
						execute(mobs, y, x);
						makenpc(-340, 298, 4);
					}
					break;
				case 3:
					{
						var mobs = new int[2] { 9300387, 9500149 };
						var y = new int[5] { 178, 84, -94, -187, -367 };
						var x = new int[2][]
						{
							new int[5] { 97, 623, 659, 766, 859 }, // low range
							new int[5] { 1386, 1656, 1465, 1372, 1290 }, // high range
						};
						execute(mobs, y, x);
						makenpc(-170, -180, 21);
					}
					break;
				case 4:
					{
						var mobs = new int[2] { 9500149, 9500150 }; // 3k hp * 10
						var y = new int[9] { 175, 76, 48, 34, 0, -97, -127, -240, -368 };
						var x = new int[2][]
						{
							new int[9] { 380, 498, 1202, 93, 930, 125, 1069, 235, 762 }, // low range
							new int[9] { 1725, 920, 1590, 480, 1360, 930, 1500, 630, 1130 }, // high range
						};
						execute(mobs, y, x);
						makenpc(1260, -420, 14);
					}
					break;
				case 5:
					{
						MobHpMultiplier = 60;

						var mobs = new int[3] { 8220009, 8220009, 8220009 };
						var y = new int[3] { 1967, 1606, 1373 };
						var x = new int[2][]
						{
							new int[3]{ 0, 113, 210 },
							new int[3]{ 470, 400, 445 },
						};

						for (var i = 0; i < mobs.Length; i++)
						{
							Mobs.CreateMob(mobs[i], null, Constants.Rand.Next(x[0][i], x[1][i]), y[i], 0, 0, 0, 0, MobType.Normal, null);
							//var mob = new CMob(mobs[i])
							//{
							//	m_nSummonType = 0xFE,
							//	Position = new CMovePath
							//	{
							//		Y = (short)y[i],
							//		X = (short)Constants.Rand.Next(x[0][i], x[1][i]),
							//	},
							//};
							//Mobs.CreateMob(mob);
						}

						execute(
							new int[] { 9500106, 9500105, 9400622, 9000002, 9000301 },
							new int[] { 1967, 1793, 1652, 1619, 1480, 1379 },
							new int[][]{
								new int[]{ -374, -88, 207, -12, 94, 125 },
								new int[]{ 914, 637, 569, 391, 500, 456, }
							});
						makenpc(-345, 1800, 36);
					}
					break;
			}

			void execute(int[] mobs, int[] y, int[][] x)
			{
				while (mobcount > 0)
				{
					var level = Constants.Rand.Next(0, y.Length);
					var spawnY = y[level];
					var spawnX_L = x[0][level];
					var spawnX_R = x[1][level];

					var spawnX = Constants.Rand.Next(spawnX_L, spawnX_R);

					Mobs.CreateMob(mobs.Random(), null, spawnX, spawnY, 0, 0, 0, 0, MobType.Normal, null);

					//var mob = new CMob(mobs.Random())
					//{
					//	m_nSummonType = 0xFE,
					//	Position = new CMovePath
					//	{
					//		Y = (short)spawnY,
					//		X = (short)spawnX,
					//	},
					//};

					//Mobs.CreateMob(mob);

					mobcount -= 1;
				}
			}

			void makenpc(short x, short y, short fh)
			{
				var entry = new CNpc(npc)
				{
					Rx0 = x,
					Rx1 = y,
					X = x,
					Y = y,
					Cy = y,
					F = true,
					Foothold = fh
				};
				Npcs.Add(entry);
				Broadcast(entry.MakeEnterFieldPacket());
			}
		}

		protected override void SetDungeonTimeRemaining()
		{
			if (DungeonFieldIndex == 1)
			{
				DungeonTimeSeconds = 600; // 10 minutes
			}
			else
			{
				var previousField = ParentInstance.CFieldMan
					.GetField(PreviousFieldID(), nInstanceID) as CField_DungeonRaid;

				DungeonTimeSeconds = previousField.DungeonTimeSeconds; // remaining time
				CumulativeExp = previousField.CumulativeExp; // total exp
			}
		}

		protected override void OnUserEnter(Character pUser)
		{
			base.OnUserEnter(pUser);

			if (Users.Count == 1) // first user
			{
				MobExpDenominator = pUser.Party?.AveragePartyLevel(dwUniqueId) ?? pUser.Stats.nLevel;
			}
		}

		protected override bool TryProceed(Character user, int nPortalID)
		{
			new FieldEffectPacket(FieldEffect.Screen)
			{ sName = INTRO_EFFECT }
			.Broadcast(user, true);

			return false;
		}

		public override bool OnMobDamaged(CMob mob, int nDamage)
		{
			if (tLastBossHpSent.MillisSinceStart() <= 500) return false;

			FinalBossHP -= nDamage;

			tLastBossHpSent = DateTime.Now;

			new FieldEffectPacket(FieldEffect.MobHPTag)
			{
				dwMobTemplateID = 3220000,
				nHP = FinalBossHP,
				nMaxHP = FinalBossMaxHP,
				nColor = 3,
				nBgColor = 5,
			}.Broadcast(this);

			return false; // return false to disable default mob HP tag broadcasting
		}

		public override void OnMobDie(CMob removedMob)
		{
			var gain = (Constants.Rand.Next() % 13 + (removedMob.MaxHp / MobExpDenominator)) * DungeonFieldIndex; // TODO maybe do dungeon points instead?
			StageExp += gain;
			CumulativeExp += gain;

			ForceWeatherEffect(5120017, $"Stage Exp Earned: {StageExp}  |  Cumulative Exp Earned: {CumulativeExp}");

			if (Mobs.Count > 1) return;

			GiveRewards();

			DungeonTimeSeconds = (int)tFieldTimerExpiration.SecondsUntilEnd();

			if (DungeonTimeSeconds < 0) DungeonTimeSeconds = 0;

			WarpToNextField = true;

			if (DungeonFieldIndex == 5)
			{
				BroadcastWZMapEffect(CLEAR_EFFECT);
				CreateFieldClock(15); // enough time to grab any loot that is on the ground
			}
			else
			{
				BroadcastWZMapEffect(CLEAR_EFFECT);
				CreateFieldClock(8); // enough time to grab any loot that is on the ground
				Broadcast(CPacket.SystemMessage("You have won favor in the eyes of the gods, prepare to be sent to the next stage."));
			}
		}

		protected override void GiveRewards()
		{
			foreach (var user in Users)
			{
				user.Modify.GainExp(StageExp);
			}
		}
	}
}
