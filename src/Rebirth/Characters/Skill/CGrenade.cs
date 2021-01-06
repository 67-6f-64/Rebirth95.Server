using Rebirth.Characters.Combat;
using Rebirth.Client;
using Rebirth.Common.Tools;
using Rebirth.Network;
using Rebirth.Tools;

namespace Rebirth.Characters.Skill
{
    public class CGrenade
    {
        /// <summary>
        /// Packet handler for throw animation
        /// </summary>
        /// <param name="c"></param>
        /// <param name="p"></param>
        public static void Handle_UserThrowGrenade(WvsGameClient c, CInPacket p)
        {
            var nX = p.Decode4();
            var nY = p.Decode4();
            var nY2 = p.Decode4();
            var tKeyDown = p.Decode4();
            var nSkillID = p.Decode4();
            var nSLV = p.Decode4();

            var pSkill = c.Character.Skills.Get(nSkillID);

            if (pSkill == null)
            {
                c.Character.Action.Enable();
                return;
            }

            c.Character.Field.Broadcast(UserThrowGrenadeRemote(c.Character.dwId, nX, nY, tKeyDown, nSkillID, nSLV), c);
        }

        /// <summary>
        /// Internal handler for creating the affected area
        /// </summary>
        public static void CastGrenadeSkill(Character c, MapleAttack atkInfo)
        {
            // no validation because it has already occurred
            var skill = c.Skills.Get(atkInfo.nSkillID);
            var offset = new TagPoint(atkInfo.nGrenadePtX, atkInfo.nGrenadePtY);

            c.Skills.CastAffectedAreaSkill
                (atkInfo.nSkillID, skill.nSLV, (short)skill.BuffTime, offset, skill.Template.LT, skill.Template.RB);
        }

        private static COutPacket UserThrowGrenadeRemote(int dwCharId, int pX, int pY, int tKeyDown, int nSkillID, int nSLV)
        {
            var p = new COutPacket(SendOps.LP_UserThrowGrenade);
            p.Encode4(dwCharId);
            p.Encode4(pX);
            p.Encode4(pY);
            p.Encode4(tKeyDown);
            p.Encode4(nSkillID);
            p.Encode4(nSLV);
            return p;
        }
    }
}
