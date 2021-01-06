using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Common.GameLogic
{
	public static class JobLogic
	{
		public enum JobType
		{
			Beginner,
			Warrior,
			Magician,
			Thief, // includes dualblade
			Bowman,
			Pirate,
			Evan,
			Aran,
			All, // for drop data

			Admin,
			Resistance
		}

        public static short JobCodeFromJobID(short jobId) => (short)(jobId / 100);

        public static JobType JobTypeFromID(short jobId)
        {
            if (jobId >= 2200 && jobId <= 2220)
                return JobType.Evan;

            switch (jobId)
            {
                case 100: // warrior
                case 110: // fighter
                case 111: // crusader
                case 112: // hero
                case 120: // page
                case 121: // white knight
                case 122: // paladin
                case 130: // spearman
                case 131: // dragon knight
                case 132: // dark knight
                case 1100: // dawn warrior
                case 1110:
                case 1111:
                    return JobType.Warrior;
                case 200: // magician
                case 210: // fp wizard
                case 211: // fp mage
                case 212: // fp archmage
                case 220: // il wizard
                case 221: // il mage
                case 222: // il archmage
                case 230: // cleric
                case 231: // priest
                case 232: // bishop
                case 1200: // blaze wizard
                case 1210:
                case 1211:
                    return JobType.Magician;
                case 300: // bowman
                case 310:
                case 311:
                case 312:
                case 320:
                case 321:
                case 322:
                case 1300: // wind archer
                case 1310:
                case 1311:
                    return JobType.Bowman;
                case 400:
                case 410:
                case 411:
                case 412:
                case 420:
                case 421:
                case 422:
                case 430: // dualblade - lvl 20
                case 431: // lvl 30
                case 432: // lvl 55
                case 433: // lvl 70
                case 434: // lvl 120
                case 1400: // night walker
                case 1410:
                case 1411:
                    return JobType.Thief;
                case 500:
                case 510:
                case 511:
                case 512:
                case 520:
                case 521:
                case 522:
                case 1500: // thunder breaker
                case 1510:
                case 1511:
                    return JobType.Pirate;
                case 800:
                case 900:
                case 910:
                    return JobType.Admin;
                case 2100:
                case 2110:
                case 2111:
                case 2112:
                    return JobType.Aran;
                case 3200:
                case 3210:
                case 3211: // todo 
                case 3212:
                    return JobType.Resistance;
                case 3300:
                case 3310:
                case 3311: // todo
                case 3312:
                    return JobType.Resistance;
                case 3500:
                case 3510:
                case 3511: // todo
                case 3512:
                    return JobType.Resistance;
                case 0:
                case 1000:
                case 2000:
                case 2001:
                case 3000:
                default:
                    return JobType.Beginner;
            }
        }

        public static bool is_jobchange_level_in_evan(int level)
        {
            switch (level)
            {
                case 10: // 2200
                case 20: // 2210
                case 30: // 2211
                case 40: // 2212
                case 50: // etc
                case 60:
                case 80:
                case 100:
                case 120:
                case 160:
                    return true;
                default:
                    return false;
            }
        }

        public static int get_job_level(int nJob)
        {
            int v1; // esi
            int v2; // esi
            int result; // eax

            if (nJob % 100 == 0 || nJob == 2001)
                return 1;
            if (nJob / 10 == 43)
                v1 = (nJob - 430) / 2;
            else
                v1 = nJob % 10;
            v2 = v1 + 2;
            if (v2 >= 2 && (v2 <= 4 || v2 <= 10 && IsEvan(nJob)))
                result = v2;
            else
                result = 0;
            return result;
        }

        public static bool IsMaxJob(int id)
        {
            if (id == 434) return true; // db
            if (id == 2218) return true; // evan
            if (id == 2212 || id == 432) return false; // db & evan
            if (id > 1000 && id < 2000 && id % 100 == 11) return true; // koc

            return id % 10 == 2;
        }

        public static bool is_beginner_job(int a1) => (a1 % 1000 == 0) || a1 == 2001;

        public static bool is_mage_job(short nJobID)
        {
	        var nJobCode = JobCodeFromJobID(nJobID);

	        switch (nJobCode)
	        {
		        case 2:
		        case 12:
		        case 22:
		        case 32:
			        return true;
		        default:
			        return false;
            }
        }

        public static bool IsKOC(int job) => job >= 1000 && job < 2000;

        public static bool IsEvan(int job) => job == 2001 || (job >= 2200 && job <= 2218);

        public static bool isAran(int job) => job >= 2000 && job <= 2112 && job != 2001;

        public static bool isResist(int job) => job >= 3000 && job <= 3512;

        public static bool isAdventurer(int job) => job < 1000;

        public static bool IsExtendedSPJob(int nJobId) => nJobId / 1000 >= 3 || nJobId / 100 == 22 || nJobId == 2001;

        public static bool is_mechanic_job(int a1) => a1 >= 3500 && a1 <= 3512;

        public static bool IsWildhunterJob(int a1) => a1 >= 3300 && a1 <= 3312;

        public static bool is_bmage_job(int a1) => a1 >= 3200 && a1 <= 3212;

        public static bool is_matched_itemid_job(int itemid, short jobid)
        {
            if (jobid / 100 != 22 && jobid != 2001)
            {
                if (itemid - 5050005 <= 4)
                    return false;
            }
            else if (itemid - 5050001 <= 3)
            {
                return false;
            }
            return true;
        }

        public static int GetExtendedSPIndexBySkill(int nSkillID)
            => GetExtendedSPIndexByJob(nSkillID / 10000);

        public static int GetExtendedSPIndexByJob(int nJobId)
        {
            if (nJobId < 2200 || nJobId == 3000)
                return 0;

            if (nJobId % 100 == 0)
                return 1;

            return (nJobId % 10) + 2;
        }

        public enum BasicStat
        {
            Str = 1,
            Int = 2,
            Dex = 3,
            Luk = 4
        }

        public static BasicStat GetBasicStatByJob(int nJobID)
        {
            var v1 = nJobID / 100 % 10;

            switch (v1)
            {
                case 5:
                    return BasicStat.Dex;
                default:
                    return (BasicStat)v1;
            }
        }
    }
}
