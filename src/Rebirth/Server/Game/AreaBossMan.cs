using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Server.Center;

namespace Rebirth.Server.Game
{
	public class AreaBossMan
	{
		private const int COOLDOWN_SECONDS = 3 * 60 * 60;
		private const int COOLDOWN_RANDOMNESS = 60;

		private readonly Dictionary<int, DateTime> spawns;

		public AreaBossMan()
		{
			spawns = new Dictionary<int, DateTime>();
		}

		public bool CanSpawn(int nMapId)
		{
			if (!spawns.ContainsKey(nMapId))
			{
				spawns.Add(nMapId, DateTime.Now.AddMinutes(Constants.Rand.Next(COOLDOWN_RANDOMNESS)));
				return true;
			}

			if (spawns[nMapId].SecondsSinceStart() > COOLDOWN_SECONDS)
			{
				spawns[nMapId] = DateTime.Now.AddMinutes(Constants.Rand.Next(COOLDOWN_RANDOMNESS));
				return true;
			}

			return false;
		}
	}
}
