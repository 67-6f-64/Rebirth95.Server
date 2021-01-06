using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_Prepare
    {
        // note: uses another header (0x69) than regular skill req (0x67)
        public static void Handle(WvsGameClient c, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 105);
            //v71 = 10;
            //COutPacket::Encode4(&oPacket, v5);
            //COutPacket::Encode1(&oPacket, nSLV);
            //COutPacket::Encode2(&oPacket, v4->m_nOneTimeAction & 0x7FFF | ((unsigned __int16)v4->m_nMoveAction << 15));
            //COutPacket::Encode1(&oPacket, v57);
            //if (v5 == 33101005)
            //    COutPacket::Encode4(&oPacket, v4->m_dwSwallowMobID);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var nSkillID = p.Decode4();
            var nSLV = p.Decode1();
            var nMoveAction = p.Decode2();
            var nActionSpeed = p.Decode1();

            var pSkill = c.Character.Skills.Get(nSkillID, true);

            if (pSkill is null)
                return;

            if (nSkillID == (int)Skills.WILDHUNTER_SWALLOW)
            {
                var dwSwallowMobId = p.Decode4();

                var pMob = c.Character.Field.Mobs[dwSwallowMobId];

                if (pMob is object && !GameConstants.is_not_swallowable_mob(pMob.nMobTemplateId))
                {
                    if (!pMob.TrySwallowMob(c.Character))
                        return;
                }
            }

            c.Character.Field.Broadcast(UserSkillPrepare(c.Character.dwId, nSkillID, pSkill.nSLV, nMoveAction, nActionSpeed), c);
        }

        private static COutPacket UserSkillPrepare(int dwCharId, int nSkillID, byte nSLV, short nMoveAction, byte nActionSpeed)
        {
            var p = new COutPacket(SendOps.LP_UserSkillPrepare);
            p.Encode4(dwCharId);
            p.Encode4(nSkillID);
            p.Encode1(nSLV);
            p.Encode2(nMoveAction);
            p.Encode1(nActionSpeed);
            return p;
        }
    }
}
