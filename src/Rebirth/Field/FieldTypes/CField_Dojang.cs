using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Characters;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Server.Center;
using Rebirth.Server.Game;
using Rebirth.Tools;

namespace Rebirth.Field.FieldTypes
{
	// FieldType.DOJANG
	public class CField_Dojang : CField
	{
		private const int BASE_BOSS_ID = 9300183;
		private const int BASE_BOSS_ITEM_SPAWNER_ID = 2100070;
		private const int BASE_MAP_ID = 925020000;
		private const int DEAD_MOB_CHECK = 9300216;
		private const int DOJO_WEATHER_EFFECT = 5120024;

		private readonly bool _normal;
		private readonly int _stage;
		private readonly int _bossMob;
		private readonly int _summonItem;

		private DateTime _spawnTime;

		public CField_Dojang(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			_normal = (MapId / 10000) - 92500 == 2;
			_stage = (MapId - BASE_MAP_ID) / 100;
			var stageIncrement = (int)Math.Floor(_stage / 6f);
			_bossMob = BASE_BOSS_ID + _stage - stageIncrement;
			_summonItem = BASE_BOSS_ITEM_SPAWNER_ID + _stage;
		}

		protected override void Init()
		{
			SetClock();
			_spawnTime = DateTime.Now.AddSeconds(1); // so the spawn animation is shown to all party members

			base.Init();
		}

		public override void Update()
		{
			if (_spawnTime.MillisUntilEnd() < 0 && !Mobs.Any())
			{
				SpawnStageBoss();
			}

			base.Update();
		}

		private void SetClock()
		{
			switch (_stage)
			{
				case 1:
					CreateFieldClock((5 * 60) + 2);
					break;
				case 7:
					CreateFieldClock((6 * 60) + 2);
					break;
				case 13:
					CreateFieldClock((7 * 60) + 2);
					break;
				case 19:
					CreateFieldClock((8 * 60) + 2);
					break;
				case 25:
					CreateFieldClock((9 * 60) + 2);
					break;
				case 31:
					CreateFieldClock((10 * 60) + 2);
					break;
				case 37:
					CreateFieldClock((15 * 60) + 2);
					break;
				default:
					var previousTime = ParentInstance.CFieldMan.GetField(MapId - 100, nInstanceID).tFieldTimerExpiration;
					CreateFieldClock((int)previousTime.SecondsUntilEnd() + 3);
					break;
			}
		}

		private void SpawnStageBoss()
		{
			Mobs.CreateMob(_bossMob, null, 146, 7, 0, 15, 0, 1, MobType.Normal, Users.FirstOrDefault());
		}

		private void TryDropDojoItem(TagPoint pos)
		{
			// 50% chance to drop a dojo powerup
			if (Constants.Rand.NextDouble() < 0.50)
			{
				var randBuffItem = Constants.Rand.Next(2022359, 2022422);

				var item = MasterManager.CreateItem(randBuffItem);

				if (item is null) return;

				CDropFactory.CreateDropItem(this, pos, 0, item);
			}

			// 50% chance to drop a dojo hp/mp pot
			if (Constants.Rand.NextDouble() < 0.50)
			{
				var randPotItem = Constants.Rand.Next(2022430, 2022433);

				var item = MasterManager.CreateItem(randPotItem);

				if (item is null) return;

				CDropFactory.CreateDropItem(this, pos, 0, item);
			}
		}

		public override bool TryDropPickup(Character pUser, CDrop pItem)
		{
			if (pItem.Item.Template is ConsumeItemTemplate template && template.ConsumeOnPickup)
			{
				foreach (var item in Users)
				{
					// todo maybe clean this up

					// add consumable effects
					var newBuff = new BuffConsume(template.TemplateId);
					newBuff.GenerateConsumeable(item);

					// check required cuz calling function adds item buffs already
					if (item.dwId == pUser.dwId) continue;

					// add real buffs
					item.Buffs.AddItemBuff(pItem.Item.nItemID);
				}
			}

			return true;
		}

		protected override void OnClockEnd()
		{
			Mobs.RemoveAll(false);

			FieldEffectPacket.BroadcastDojoEffect(this, FieldEffectTypes.DojoEffect.TimeOver);

			Reset();
			base.OnClockEnd();
		}

		public override void OnMobDie(CMob removedMob)
		{
			// copy pos for the dojo item drop
			var pos = removedMob.Position.CurrentXY.Clone();

			// no drops/mesos
			removedMob.bRewardsDistributed = true;

			// need to call this to override the above statement for exp
			removedMob.DistributeEXP();

			base.OnMobDie(removedMob);

			if (_bossMob != removedMob.nMobTemplateId) return;

			TryDropDojoItem(pos);

			UpdatePoints();

			FieldEffectPacket.BroadcastDojoEffect(this, FieldEffectTypes.DojoEffect.Clear);
		}

		public override void OnUserEnterScriptedPortal(Character user, string sPortal)
		{
			if (tFieldTimerExpiration.SecondsUntilEnd() < 0) // timer expired -> warp out
			{
				if (user.Party is null)
				{
					user.Action.SetField(ReturnMapId);
				}
				else
				{
					user.Party.WarpParty(dwUniqueId, ReturnMapId, false);
					// user.Action.SetField(MapId);
				}

				Reset();
			}
			else if (Mobs.Any(mob => mob.nMobTemplateId == DEAD_MOB_CHECK))
			{
				if (user.Party is null)
				{
					user.Action.SetFieldInstance(MapId + 100, nInstanceID);
				}
				else
				{
					user.Party.WarpParty(dwUniqueId, MapId + 100, true);
					// user.Action.SetFieldInstance(MapId, nInstanceID);
				}

				Reset();
			}
			else
			{
				user.SendMessage("Kill the boss to proceed to the next stage..");
			}

			user.Action.Enable();
		}

		private void UpdatePoints()
		{
			// base points
			int stagePoints = Users.Count > 1 ? 1 : 2;

			// additional stage points
			stagePoints += (int)Math.Floor(_stage / 6f);

			Broadcast(CPacket.ScriptProgressMessage($"Gained {stagePoints} Dojo Points and {_stage * RateConstants.DojoNXGainPerStage} NX!"));

			foreach (var user in Users)
			{
				var totalPoints = stagePoints;

				if (user.Quests.Contains(QuestConstants.DOJO_POINTS_QID))
				{
					var amount = user.Quests[QuestConstants.DOJO_POINTS_QID].sQRValue;

					int.TryParse(amount, out var nAmountResult);

					totalPoints = nAmountResult + stagePoints;
				}

				user.Quests.UpdateQuestRecordInternal(QuestConstants.DOJO_POINTS_QID, totalPoints);

				user.SendMessage($"Total dojo points: {totalPoints}.");

				user.Modify.GainNX(stagePoints * RateConstants.DojoNXGainPerStage);
			}
		}
	}
}
