using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rebirth.Characters
{
    public enum WildHunterCaptureResult
    {
        Success = 0,
        HPTooHigh = 1,
        IncorrectMobType = 2,
    }

    public class GW_WildhunterInfo
    {
        public int[] adwTempMobID { get; private set; }
        public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
        public int dwParentID { get; }

        public int Count => adwTempMobID.Count(id => id > 0);

        public GW_WildhunterInfo(int parent)
        {
            dwParentID = parent;
			adwTempMobID = new int[5];
			//for (int i = 0; i < adwTempMobID.Length; i++)
			//{
			//    adwTempMobID[i] = 9304000 + i;
			//}
		}

		public void Dispose()
		{
			adwTempMobID = null;
		}

		public void UpdateJaguarInfo(int nMobTemplateId)
        {
            for (var i = 0; i < adwTempMobID.Length; i++)
            {
                if (adwTempMobID[i] == 0)
                    adwTempMobID[i] = nMobTemplateId;

                if (adwTempMobID[i] == nMobTemplateId)
                    break;
            }

            Parent.SendPacket(WildHunterInfo());
        }

        public async Task LoadFromDB()
        {
            // todo
        }

        public void SaveToDB()
        {
            // todo
        }

        public COutPacket WildHunterInfo()
        {
            var p = new COutPacket(SendOps.LP_WildHunterInfo);
            Encode(p);
            return p;
        }

        public void Encode(COutPacket p)
        {
            p.Encode1(0x28); // unsure, took from eric v117 :teehee:
            for (int i = 0; i < adwTempMobID.Length; i++)
            {
                p.Encode4(adwTempMobID[i]);
            }
        }
    }
}
