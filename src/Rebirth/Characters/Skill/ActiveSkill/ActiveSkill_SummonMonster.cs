using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_SummonMonster
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            // Recv [CP_UserSkillUseRequest] [67 00] [D0 B3 75 10] [A6 C7 C9 01] [01] [C0 F7 8D 00] [39 FE] [DF FF] [00]

            //COutPacket::COutPacket(&oPacket, 103);
            //v50 = 3;
            //v36 = get_update_time();
            //COutPacket::Encode4(&oPacket, v36);
            //COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //COutPacket::Encode1(&oPacket, nSLV);
            //COutPacket::Encode4(&oPacket, (unsigned int)cd);
            //COutPacket::Encode2(&oPacket, pt.x);
            //COutPacket::Encode2(&oPacket, pt.y);
            //COutPacket::Encode1(&oPacket, bLeft);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var dwMobID = p.Decode4();
            var ptX = p.Decode2();
            var ptY = p.Decode2();
            var bLeft = p.Decode1() > 0;

            if (c.Skills.Cast(nSkillID, bLeft))
            {
				new UserEffectPacket(UserEffect.SkillUse)
				{
					nSkillID = nSkillID,
					nSLV = nSLV,
					bLeft = bLeft,
					ptX = ptX,
					ptY = ptY,
				}.BroadcastEffect(c, false);

				// TODO spawn monster lol
            }
        }
    }
}
