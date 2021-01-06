using Rebirth.Common.Types;

namespace Rebirth
{
    public static class AffectedAreaConstants
    {
        public static AffectedAreaType GetAreaType(int nSkillID)
        {
            switch ((Skills)nSkillID)
            {
                case Skills.SHADOWER_SMOKE_SHELL:
                case Skills.BMAGE_SHELTER:
                    return AffectedAreaType.Smoke;

                case Skills.NIGHTWALKER_POISON_BOMB:
                case Skills.MAGE1_POISON_MIST:
                case Skills.FLAMEWIZARD_FLAME_GEAR:
                    return AffectedAreaType.UserSkill;

                case Skills.EVAN_RECOVERY_AURA:
                    return AffectedAreaType.BlessedMist;
            }
            return AffectedAreaType.MobSkill;
        }
    }
}
