using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Actions
{
    public class GiveFame
    {
        public static void OnPacket(WvsGameClient c, CInPacket p)
        {
            var chr = c.Character;
            var targetId = p.Decode4();
            short type = p.Decode1();

            var targetChar = chr.Field.Users[targetId];

            if (targetChar is null || chr.dwId == targetId)
            {
                c.SendPacket(GivePopularityResult_Error(GivePopularityRes.InvalidCharacterID));
                return;
            }
            else if (chr.Stats.nLevel < 15)
            {
                c.SendPacket(GivePopularityResult_Error(GivePopularityRes.LevelLow));
                return;
            }

            bool famedPersonThisMonth = false; // todo

            if (c.Character.Stats.tLastFame.CompareTo(DateTime.Now.AddDays(-1)) >= 0)
            {
                c.SendPacket(GivePopularityResult_Error(GivePopularityRes.AlreadyDoneToday));
                return;
            }
            else if (famedPersonThisMonth)
            {
                c.SendPacket(GivePopularityResult_Error(GivePopularityRes.AlreadyDoneTarget));
                return;
            }

            var bFameUp = type == 1;

            targetChar.Modify.GainFame((short)(bFameUp ? 1 : -1));

            c.Character.StatisticsTracker.nFameGiven += 1;

            //if (targetChar.Stats.nPOP >= 20) // TODO add celebrity medal here
            //    InventoryManipulator.InsertInto(targetChar, MasterManager.CreateItem(0));

            targetChar.SendPacket(GivePopularityResult_Notify(chr.Stats.sCharacterName, bFameUp));
            c.SendPacket(GivePopularityResult_Success(targetChar.Stats.sCharacterName, bFameUp, targetChar.Stats.nPOP));
        }

        private static COutPacket GivePopularityResult_Success(string sName, bool bFameUp, int nPOP)
        {
            var p = new COutPacket(SendOps.LP_GivePopularityResult);
            p.Encode1((byte)GivePopularityRes.Success);
            p.EncodeString(sName);
            p.Encode1(bFameUp);
            p.Encode4(nPOP);
            return p;
        }

        private static COutPacket GivePopularityResult_Error(GivePopularityRes type)
        {
            var p = new COutPacket(SendOps.LP_GivePopularityResult);
            p.Encode1((byte)type);
            return p;
        }

        private static COutPacket GivePopularityResult_Notify(string sName, bool bFameUp)
        {
            var p = new COutPacket(SendOps.LP_GivePopularityResult);
            p.Encode1((byte)GivePopularityRes.Notify);
            p.EncodeString(sName);
            p.Encode1(bFameUp);
            return p;
        }
    }
}
