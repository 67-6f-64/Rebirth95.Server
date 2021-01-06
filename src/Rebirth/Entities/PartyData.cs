using Rebirth.Characters;
using Rebirth.Server.Center;
using System.Collections.Generic;
using Rebirth.Common.Tools;
using Rebirth.Tools;

namespace Rebirth.Entities
{
    /*
    struct PARTYDAMAGE
    {
        int nPartyID;
        int nDamage;
        int nMinLevel;
        int nMaxDamage;
        unsigned int dwMaxDamageCharacter;
        int nMaxDamageLevel;
        int bLast;
        unsigned int adwPartyMemberHitMob[6];
        int nMemberIdx;
    };
    */
    public class PartyTownPortal
    {
        public int dwTownID { get; set; }
        public int dwFieldID { get; set; }
        public int nSkillID { get; set; }
        public TagPoint ptFieldPortal { get; set; }
    }
    public class PartyMember
    {
        public int[] adwCharacterID { get; set; }
        public string[] asCharacterName { get; set; }
        public int[] anJob { get; set; }
        public int[] anLevel { get; set; }
        public int[] anChannelID { get; set; }
        public int dwPartyBossCharacterID { get; set; }

        public PartyMember()
        {
            adwCharacterID = new int[6];
            asCharacterName = new string[6];
            anJob = new int[6];
            anLevel = new int[6];
            anChannelID = new int[6];
        }

    }
    public class PartyData
    {
        public PartyMember Members { get; }
        public int[] adwFieldID { get; }
        public PartyTownPortal[] aTownPortal { get; }
        public int[] aPQReward { get; }
        public int[] aPQRewardType { get; }
        public int dwPQRewardMobTemplateID { get; }
        public int bPQReward { get; }

        public PartyData()
        {
            Members = new PartyMember();
            adwFieldID = new int[6];
            aTownPortal = new PartyTownPortal[6];
            aPQReward = new int[6];
            aPQRewardType = new int[6];
        }
    }
}