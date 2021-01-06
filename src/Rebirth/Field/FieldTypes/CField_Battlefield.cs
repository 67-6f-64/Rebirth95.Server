using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Characters;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldTypes.Custom;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Game;
using System.Linq;
using Rebirth.Characters.Modify;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Server.Center;

namespace Rebirth.Field.FieldTypes
{
	public class CField_Battlefield : CField
	{
		private Dictionary<int, BattlefieldData.BattlefieldTeams> aTeams;
		private Dictionary<int, int> aSheepPoints;

		private int WolfScore;
		private int SheepScore;

		private const int POINTS_PER_SHEEP = 3;
		private const int DURATION_SEC = 300;

		private DateTime tGameOverTime { get; set; } = DateTime.MinValue;
		private bool bWolfWin { get; set; }

		public CField_Battlefield(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{ }

		public BattlefieldData.BattlefieldTeams GetPlayerTeam(int dwCharId)
			=> aTeams.GetValueOrDefault(dwCharId, BattlefieldData.BattlefieldTeams.None);

		protected override void Init()
		{
			aTeams = new Dictionary<int, BattlefieldData.BattlefieldTeams>();
			aSheepPoints = new Dictionary<int, int>();
			WolfScore = 0;
			SheepScore = 0;
			Mobs.Reset();
			Reactors.Reset();

			foreach (var reactor in Reactors)
			{
				if (reactor.tReactorTime == 0)
				{
					reactor.tReactorTime = 10; // TODO figure out how reactor events work with this
				}
			}

			tGameOverTime = DateTime.MinValue;

			CreateFieldClock(DURATION_SEC);
		}

		protected override void OnUserEnter(Character pUser)
		{
			// no base call in this func cuz the order of operations need to be different
			if (Users.Count <= 0) Init();

			// TODO add GM check here and call base func if GM Hide is active

			if (aTeams.Count < 2) aTeams.Add(pUser.dwId, BattlefieldData.BattlefieldTeams.Wolves);

			// order is important
			else if (aTeams.Count < 5) aTeams.Add(pUser.dwId, BattlefieldData.BattlefieldTeams.Sheep);
			else if (aTeams.Count < 7) aTeams.Add(pUser.dwId, BattlefieldData.BattlefieldTeams.Wolves);
			else if (aTeams.Count < 10) aTeams.Add(pUser.dwId, BattlefieldData.BattlefieldTeams.Sheep);
			else if (pUser.Account.AccountData.Admin <= 0) // if over capacity and not a GM
			{
				OnClockEnd();
				return; // critical error: should never be more than 10 players in the map
			}

			// has to be after user is added to a team
			Users.Add(pUser);

			// tell user what team they are on -- TODO does this still need to be done since we encode it in the setfield packet??
			var p = new COutPacket(SendOps.LP_FieldSpecificData);
			p.Encode1(aTeams[pUser.dwId]);
			pUser.SendPacket(p);

			if (aTeams[pUser.dwId] == BattlefieldData.BattlefieldTeams.Wolves)
			{
				WolfScore += 1;
			}
			else
			{
				aSheepPoints.Add(pUser.dwId, POINTS_PER_SHEEP);
				SheepScore += POINTS_PER_SHEEP;
			}

			Broadcast(ScoreUpdate(WolfScore, SheepScore));
			pUser.SendMessage("Damage the enemy team with the items from the NPCs! Pick up their drops to score points and win!");
		}

		public override void Update()
		{
			if (tGameOverTime != DateTime.MinValue && tGameOverTime.SecondsSinceStart() > 3)
			{
				if (bWolfWin)
				{
					WarpTeamToMap(BattlefieldData.BattlefieldTeams.Wolves, BattlefieldData.WolfVictoryMap);
					WarpTeamToMap(BattlefieldData.BattlefieldTeams.Sheep, BattlefieldData.SheepLoseMap);
				}
				else
				{
					WarpTeamToMap(BattlefieldData.BattlefieldTeams.Wolves, BattlefieldData.WolfLoseMap);
					WarpTeamToMap(BattlefieldData.BattlefieldTeams.Sheep, BattlefieldData.SheepVictoryMap);
				}
			}

			base.Update();
		}

		public override void OnUserLeave(Character pUser, bool bOnMigrateOut = false)
		{
			if (tFieldTimerExpiration.SecondsSinceStart() < DURATION_SEC)
			{
				if (aTeams.TryGetValue(pUser.dwId, out var value) && aTeams.Remove(pUser.dwId))
				{
					if (value == BattlefieldData.BattlefieldTeams.Wolves)
					{
						UpdateScore(value, 0);
					}
					else
					{
						aSheepPoints.TryGetValue(pUser.dwId, out var nSheepPoints);
						UpdateScore(BattlefieldData.BattlefieldTeams.Sheep, nSheepPoints);
					}

					aSheepPoints.Remove(pUser.dwId);
				}
			}

			foreach (var item in pUser.InventoryConsume.ToArray())
			{
				// TODO add a wz attribute that indicates an item should be removed on warp and call that here instead
				var template = item.Value.Template as ConsumeItemTemplate;

				pUser.Modify.Inventory(ctx =>
				{
					if (template.ConsumeOnPickup) // no need for null check cuz all items in InventoryConsume are of this type
					{
						pUser.InventoryConsume.Remove(item.Key);
						ctx.Remove(InventoryType.Consume, item.Key);
					}
					else if (item.Value.nItemID == (int)
							BattlefieldData.BattleItems.PlantRoseThorn ||
							item.Value.nItemID == (int)
							BattlefieldData.BattleItems.StealSheepWool)
					{
						pUser.InventoryConsume.Remove(item.Key);
						ctx.Remove(InventoryType.Consume, item.Key);
					}
				});
			}

			base.OnUserLeave(pUser, bOnMigrateOut);
		}

		public override bool TryDropPickup(Character pUser, CDrop pDrop)
		{
			if (!aTeams.ContainsKey(pUser.dwId)) return false;

			var bUserIsWolf = aTeams[pUser.dwId] == BattlefieldData.BattlefieldTeams.Wolves;

			var bKeepItem = true;

			switch ((BattlefieldData.BattleItems)pDrop.Item.nItemID)
			{
				// let sheep pick these up
				case BattlefieldData.BattleItems.CryOfLamb:
				case BattlefieldData.BattleItems.LambSurpriseAttack:
				case BattlefieldData.BattleItems.SoundOfSheepBells:
					{
						bKeepItem = !bUserIsWolf;
					}
					break;
				case BattlefieldData.BattleItems.GreatConfusion:
				case BattlefieldData.BattleItems.WoundOfWolfBells:
				case BattlefieldData.BattleItems.WolfThreat:
					{
						bKeepItem = bUserIsWolf;
					}
					break;
				case BattlefieldData.BattleItems.FineWool:
					{
						if (!bUserIsWolf)
						{
							bKeepItem = false;

							var user = Convert.ToInt32(pDrop.QR);

							if (aTeams.ContainsKey(user))
							{
								ChangePlayerTeam(user, BattlefieldData.BattlefieldTeams.Sheep);

								aSheepPoints[user] += 1;

								if (aSheepPoints[user] > POINTS_PER_SHEEP) aSheepPoints[user] = POINTS_PER_SHEEP;
							}
						}
						else
						{
							// player keeps item, score is reduced if player is still in 
							if (aTeams.ContainsKey(Convert.ToInt32(pDrop.QR)))
							{
								UpdateScore(BattlefieldData.BattlefieldTeams.Sheep, -1);
							}
						}
					}
					break;
				case BattlefieldData.BattleItems.ShepherdBoysLunch:
					{
						if (bUserIsWolf) bKeepItem = false;
						else
						{
							// else player keeps item, score is reduced
							UpdateScore(BattlefieldData.BattlefieldTeams.Wolves, -1);
						}
					}
					break;
			}

			if (bKeepItem)
			{
				if (InventoryManipulator.InsertInto(pUser, pDrop.Item) <= 0) return false;
			}
			else
			{
				Drops.Remove(pDrop);
				return false;
			}

			return true;
		}

		public override void OnUserDamaged(Character pUser, CMob pMob, int nDamage)
		{
			// TODO override drop expiration timer with template dropExpire attribute (10 sec on this map)

			if (!aTeams.ContainsKey(pUser.dwId) && pUser.Account.AccountData.Admin <= 0)
			{
				pUser.Action.SetField(ReturnMapId);
				return;
			}

			var nItemID = (int)(aTeams[pUser.dwId] == BattlefieldData.BattlefieldTeams.Wolves
				? BattlefieldData.BattleItems.ShepherdBoysLunch
				: BattlefieldData.BattleItems.FineWool);

			if (nItemID == (int)BattlefieldData.BattleItems.ShepherdBoysLunch && Constants.Rand.Next() % 100 > 10)
			{
				return;
			}

			if (nItemID == (int)BattlefieldData.BattleItems.FineWool)
			{
				if (aSheepPoints[pUser.dwId] > 0)
				{
					if (--aSheepPoints[pUser.dwId] == 0)
					{
						ChangePlayerTeam(pUser.dwId, BattlefieldData.BattlefieldTeams.SheepNaked);
						BroadcastNotice($"{pUser.Stats.sCharacterName} has lost all their wool!");
					}
				}
				else
				{
					return; // cant drop wool if u already dropped all of it
				}
			}

			var item = MasterManager.CreateItem(nItemID);

			var drop = new CDrop(pUser.Position.CurrentXY, 0)
			{
				Item = item,
				ItemId = item.nItemID,
				QR = pUser.dwId.ToString()
			};

			drop.Position.X = drop.StartPosX;
			drop.CalculateY(this, drop.StartPosY);

			Drops.Add(drop);
		}

		private void ChangePlayerTeam(int dwUserId, BattlefieldData.BattlefieldTeams nTeam)
		{
			if (!aTeams.ContainsKey(dwUserId))
			{
				aTeams.Add(dwUserId, nTeam);
			}
			else if (aTeams.ContainsKey(dwUserId) && aTeams[dwUserId] != nTeam)
			{
				aTeams[dwUserId] = nTeam;
			}
			else return; // already in the team

			Broadcast(TeamChanged(dwUserId, nTeam));
		}

		private void ChangeSheepPoints(int dwUserId, int nAmount)
		{
			// TODO
		}

		private void OnTeamWin(bool bWolf)
		{
			const string sEffectWin = "event/coconut/victory";
			const string sEffectLose = "event/coconut/lose";
			const string sSoundWin = "Coconut/Victory";
			const string sSoundLose = "Coconut/Failed";

			tGameOverTime = DateTime.Now;
			bWolfWin = bWolf;

			foreach (var user in Users)
			{
				if (aTeams.TryGetValue(user.dwId, out var nTeam))
				{
					var effect = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Screen);
					var sound = new FieldEffectPacket(FieldEffectTypes.FieldEffect.Sound);

					if (nTeam == BattlefieldData.BattlefieldTeams.Wolves && bWolf)
					{
						effect.sName = sEffectWin;
						sound.sName = sSoundWin;
					}
					else
					{
						effect.sName = sEffectLose;
						sound.sName = sSoundLose;
					}

					effect.Broadcast(user, true);
					sound.Broadcast(user, true);
				}
			}
		}

		protected override void OnClockEnd()
		{
#if DEBUG
			return;
#endif
			OnTeamWin(false);
			base.OnClockEnd();
		}

		private void WarpTeamToMap(BattlefieldData.BattlefieldTeams nTeam, int nMapId)
		{
			foreach (var player in aTeams)
			{
				if (player.Value != nTeam || Users[player.Key] is null) continue;

				var user = Users[player.Key];

				user.Action.SetField(nMapId);
			}
		}

		/// <summary>
		/// Updates score of the given team by the given amount and returns if the score has reached zero.
		/// </summary>
		/// <param name="nTeam">Team to modify score of</param>
		/// <param name="nAmount">Amount to modify score by. Score is modified additively. Can be a positive or negative value.</param>
		/// <returns>True if a score has reached zero and the game should end, else false.</returns>
		private void UpdateScore(BattlefieldData.BattlefieldTeams nTeam, int nAmount)
		{
			if (nAmount != 0)
			{
				if (nTeam == BattlefieldData.BattlefieldTeams.Wolves)
				{
					WolfScore += nAmount;
				}
				else
				{
					SheepScore += nAmount;
				}

				Broadcast(ScoreUpdate(WolfScore, SheepScore));
			}

			if (SheepScore <= 0 || WolfScore <= 0)
			{
				OnTeamWin(SheepScore <= 0);
				BroadcastNotice($"Congratualations {(SheepScore <= 0 ? "wolves" : "sheep")} on your victory!");
			}
		}

		public override void EncodeFieldSpecificData(COutPacket p, Character c)
		{
			p.Encode1(aTeams[c.dwId]);
		}

		/// <summary>
		/// LP_BattlefieldScore - 0x164
		/// </summary>
		/// <returns></returns>
		private COutPacket ScoreUpdate(int nWolfScore, int nSheepScore)
		{
			var p = new COutPacket(SendOps.LP_BattlefieldScore);
			p.Encode1((byte)nWolfScore);
			p.Encode1((byte)nSheepScore);
			return p;
		}

		/// <summary>
		/// LP_BattlefieldTeamChanged - 0x165
		/// </summary>
		/// <returns></returns>
		private COutPacket TeamChanged(int dwCharId, BattlefieldData.BattlefieldTeams nTeam)
		{
			var p = new COutPacket(SendOps.LP_BattlefieldTeamChanged);
			p.Encode4(dwCharId);
			p.Encode1(nTeam);
			return p;
		}
	}
}
