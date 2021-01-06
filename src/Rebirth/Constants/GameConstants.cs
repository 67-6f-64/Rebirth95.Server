using Rebirth.Characters;
using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Common.GameLogic;

namespace Rebirth
{
    public static class GameConstants
    {
        public static int GetStartingMap(short nJob,short nSubJob)
        {
            return 180000001;

            //var startmap = 1020000; // for adventurers

            //if (nSubJob == 1)
            //{
            //    return 2000001; // send straight to armory
            //}

            //switch (newChar.Stats.nJob)
            //{
            //    case 1000: // koc
            //        startmap = 130010220; // kiridu's hatchery
            //        break;
            //    case 2000: // aran
            //        startmap = 140090000; // actual aran start map
            //        break;
            //    case 2001: // evan
            //        startmap = 100030100; // actual evan start map
            //        break;
            //    case 3000: // resistance
            //        startmap = 310010000; // resistance headquarters
            //        break;
            //}
        }

        public static bool BlockedFromCommands(Character c)
        {
           return JobLogic.is_beginner_job(c.Stats.nJob);
        }

        public static short GetRealJobFromCreation(int job)
        {
            switch (job)
            {
                case 0:
                    return 3000; //citizen
                case 1:
                    return 0; //beginner
                case 2:
                    return 1000; //noblese
                case 3:
                    return 2000; //legend
                case 4:
                    return 2001; //evan
            }
            return 0;
        }


        //-----------------------------------------------------------------

        private static readonly int[] closeness = 
			{
				1, 3, 6, 14, 31, 60, 108, 181, 287, 434,
				632, 891, 1224, 1642, 2161, 2793, 3557, 4467, 5542,
				6801, 8263, 9950, 11882, 14084, 16578, 19391, 22547, 26074, 30000,
			};

        public static int GetPetLevel(int nTameness)
		{
			for (var i = 0; i < closeness.Length; i++)
			{
				if (closeness[i] >= nTameness)
					return i;
			}

			return 30;
		}

        //-----------------------------------------------------------------

        private static readonly int[] guildexp = { 15000, 45000, 75000, 105000, 135000, 165000, 195000, 225000, 255000, 285000, 315000, 345000, 375000, 405000, 435000, 465000, 495000, 525000, 555000, 585000, 615000, 645000, 675000, 705000 };

        public static int GuildMaxLevel => guildexp.Length;

        public static int NextLevelGuildExp(int level)
        {
            level += 2; // cuz array starts at 0 and we want next level
            if (level < 0 || level >= guildexp.Length)
                return int.MaxValue;
            return guildexp[level];
        }

        //-----------------------------------------------------------------

        private static readonly int[] mountexp = { 0, 6, 25, 50, 105, 134, 196, 254, 263, 315, 367, 430, 543, 587, 679, 725, 897, 1146, 1394, 1701, 2247, 2543, 2898, 3156, 3313, 3584, 3923, 4150, 4305, 4550 };

        public static int getMountExpNeededForLevel(int level) => mountexp[level - 1];

        //-----------------------------------------------------------------
      
        public static bool is_not_capturable_mob(int dwMobTemplateID)
        {
            bool result; // eax
            int v2; // eax

            if (dwMobTemplateID - 9304000 <= 5)
                return false;
            v2 = dwMobTemplateID / 0x186A0;
            if (dwMobTemplateID / 0x186A0 < 0x5A || v2 > 0x5F && v2 != 97)
                result = dwMobTemplateID / 0x2710 == 999;
            else
                result = true;
            return result;
        }

        public static bool is_not_swallowable_mob(int dwMobTemplateID)
        {
            int v1; // eax
            bool result; // eax

            v1 = dwMobTemplateID / 0x186A0;
            if (dwMobTemplateID / 0x186A0 < 0x5A || v1 > 0x5F && v1 != 97)
                result = dwMobTemplateID / 0x2710 == 999;
            else
                result = true;
            return result;
        }
    }
}
