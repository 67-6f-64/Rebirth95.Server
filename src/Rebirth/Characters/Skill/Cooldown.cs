using System;

namespace Rebirth.Characters.Skill
{
    public class Cooldown
    {
        public int nSkillID { get; }
        public int nCooldownSeconds { get; private set; }
        public DateTime tStartTime { get; private set; }

        public bool CooldownExpired => (DateTime.Now - tStartTime).TotalSeconds >= nCooldownSeconds;
        public int SecondsLeft => (int)(tStartTime - DateTime.Now).TotalSeconds + nCooldownSeconds;

        public Cooldown(int nSkillID, int tCd)
        {
            tStartTime = DateTime.Now;
            nCooldownSeconds = tCd;
            this.nSkillID = nSkillID;
        }

		public void Reset(int tSeconds)
		{
			tStartTime = DateTime.Now;
			nCooldownSeconds = tSeconds;
		}
    }
}
