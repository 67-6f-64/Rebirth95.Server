using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Characters;
using Rebirth.Game;
using Rebirth.Server.Game;

namespace Rebirth.Field.FieldTypes
{
	public class CField_WaitingRoom : CField
	{
		private readonly int MaxPlayers;
		private readonly int WaitTimeSeconds;
		private readonly string WeatherMessage;
		private readonly int nWarpMap;
		private readonly int nTimeoutMap;
		private const int WeatherItemID = 5120033;

		public CField_WaitingRoom(WvsGame parentInstance, int nMapId, int nInstanceId)
			: base(parentInstance, nMapId, nInstanceId)
		{
			switch (MapId)
			{
				case BattlefieldData.RanchWaitingRoom: // TODO figure out how to add support for more group sizes
					MaxPlayers = 5;
//#if DEBUG
//						2;
//#else
//						5;
//#endif
					nWarpMap = BattlefieldData.BattleMap;
					nTimeoutMap = BattlefieldData.RanchEntranceMap;
					WaitTimeSeconds = 120; // two minutes
					WeatherMessage = $"When the room reaches {MaxPlayers} players, talk to me and get warped in!";
					break;
			}
		}

		protected override void Init()
		{
			if (WaitTimeSeconds > 0)
			{
				CreateFieldClock(WaitTimeSeconds);
			}

			if (WeatherMessage != null)
			{
				ForceWeatherEffect(WeatherItemID, WeatherMessage);
			}

			base.Init();
		}

		protected override void OnUserEnter(Character pUser)
		{
			if (Users.Count >= MaxPlayers)
			{
				int nInstance = 0;
				while (true)
				{
					var warpInMap = ParentInstance.CFieldMan.GetField(nWarpMap, ++nInstance);

					if (warpInMap.Users.Count < MaxPlayers) break;
				}

				pUser.Action.SetFieldInstance(MapId, nInstance);
			}
			else
			{
				base.OnUserEnter(pUser);
			}
		}

		public override void Update()
		{
			base.Update();

			if (Users.Count >= MaxPlayers)
			{
				WarpMapTo(nWarpMap);
			}
		}

		protected override void OnClockEnd()
		{
			switch (MapId)
			{
				case BattlefieldData.RanchWaitingRoom:
					if (Users.Count >= MaxPlayers)
					{
						WarpMapTo(nWarpMap);
					}
					else
					{
						Broadcast(CPacket.SystemMessage("Not enough players to start event."));
						WarpMapTo(nTimeoutMap);
					}
					break;
			}
			base.OnClockEnd();
		}
	}
}
