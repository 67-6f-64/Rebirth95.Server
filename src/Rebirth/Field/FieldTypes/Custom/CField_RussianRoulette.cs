using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Game;

namespace Rebirth.Field.FieldTypes.Custom
{
	public class CField_RussianRoulette : CField_CommunityEvent
	{
		public override string sEventName => "Russian Roulette"; // map: 910030000
		protected override int nEntranceMap => 910030000;

		/// <summary>
		/// Number of rounds before all the remaining players win
		/// </summary>
		private const int MAX_ROUNDS = 10;
		/// <summary>
		/// Current round of the game
		/// </summary>
		private int nCurrentRound;
		private bool bEnd;

		public CField_RussianRoulette(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId) { }

		public override void Setup()
		{
			var toRemove = new List<CNpc>();
			foreach (var npc in Npcs)
			{
				if (npc.TemplateId != 9000021) // if not event npc -> remove
					toRemove.Add(npc);
			}

			toRemove.ForEach(npc => Npcs.Remove(npc));

			base.Reset();
			base.Setup();

			nCurrentRound = 0;
			bEnd = false;
		}

		public override void OnUserEnterPortal(Character user, string sPortalName)
		{
			user.Action.Enable(); // portals are disabled
		}

		public override void OnUserEnterScriptedPortal(Character user, string sPortal)
		{
			user.Action.Enable(); // portals are disabled
		}

		protected override void OnUserEnter(Character pUser)
		{
			base.OnUserEnter(pUser);
			SetWeather("Waiting for event to start! Current participants: " + Users.Count);
		}

		public override void OnUserWarpDie(Character user, bool bLoseExp = true)
		{
			user.Modify.Stats(ctx =>
			{
				ctx.HP = user.BasicStats.nMHP;
				ctx.MP = user.BasicStats.nMMP;
			});

			user.Action.SetField(user.Stats.dwPosMap);
		}

		protected override void OnClockEnd()
		{
			nCurrentRound += 1;

			if (bEnd)
			{
				EndEvent();
				return;
			}

			var remaining = 0;

			if (QR.Equals("0"))
			{
				CloseRegistration();
				remaining = Users.Count;
			}
			else // if (nCurrentRound <= MAX_ROUNDS)
			{
				KillRandomPlatform();

				foreach (var user in Users)
				{
					if (user.Stats.nHP <= 0) continue;

					remaining += 1;

					user.Modify.GainNX(nCurrentRound * 100);
				}
			}

			if (nCurrentRound > MAX_ROUNDS)
			{
				SetWeather("Congratulations all winners!");
				DistributeRewards(Users.Where(user => user.Stats.nHP > 0).ToList());
				CreateFieldClock(5); // warp out timer
				bEnd = true;
			}
			else
			{
				CreateFieldClock(20);
				var sRoundText = nCurrentRound == MAX_ROUNDS ? "Last round!" : $"Round {nCurrentRound} of {MAX_ROUNDS}.";
				SetWeather($"{sRoundText} Pick a stage! Players remaining: " + remaining);
			}
		}

		private void SetWeather(string sMsg)
		{
			ForceWeatherEffect(5120023, sMsg);
		}

		/// <summary>
		/// Kills all players that are not on one of the three platforms, then kills all players on one random platform.
		/// </summary>
		private void KillRandomPlatform()
		{
			var platform = Constants.Rand.Next(0, 3);

			var effect = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Screen)
			{ sName = sEffectLose };

			var sound = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Sound)
			{ sName = sSoundLose };

			foreach (var user in Users)
			{
				// on left/right ropes
				if (trykill(user, 620, -730)) continue;

				// left center rope
				if (trykill(user, -340, -230)) continue;

				// right center rope
				if (trykill(user, 160, 250)) continue;

				if (user.Position.Y < -100) // above platforms
				{
					kill(user);
				}
				else if (user.Position.Y > 80) //  below platforms
				{
					kill(user);
				}
				else // user is on a platform
				{
					switch (platform)
					{
						case 0: // left platform
							{
								trykill(user, -780, -250);
							}
							break;
						case 1: // center platform
							{
								trykill(user, -300, 250);
							}
							break;
						case 2: // right platform
							{
								trykill(user, 180, 800);
							}
							break;
					}
				}
			}

			bool trykill(Character user, int minX, int maxX)
			{
				if (user.Position.X < minX || user.Position.X > maxX) return false;

				kill(user);
				return true;
			}

			void kill(Character user)
			{
				user.Modify.Stats(ctx => ctx.HP = 0);
				effect.Broadcast(user, true);
				sound.Broadcast(user, true);
			}
		}

		protected override void Reset(bool bFromDispose = false)
		{
			base.OnClockEnd();
			base.Reset(bFromDispose);
		}

		public override void DistributeRewards(List<Character> charsTo)
		{
			var effect = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Screen)
			{ sName = sEffectWin };

			var sound = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Sound)
			{ sName = sSoundWin };

			foreach (var user in charsTo)
			{
				// TODO give reward
				effect.Broadcast(user, true);
				sound.Broadcast(user, true);
			}
		}
	}
}
