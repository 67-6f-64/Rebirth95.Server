using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_Heal
    {
        public static void Handle(int nSkillID, byte nSLV, Character character, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 0x67);
            //v24 = 0;
            //v13 = get_update_time();
            //COutPacket::Encode4(&oPacket, v13);
            //COutPacket::Encode4(&oPacket, v9->nSkillID);
            //v14 = nSLV;
            //COutPacket::Encode1(&oPacket, nSLV);
            //v15 = CUserLocal::FindParty(v3, v9, v14, &nCount);
            //COutPacket::Encode1(&oPacket, v15);
            //COutPacket::Encode2(&oPacket, 0);
            //CWvsContext::SetExclRequestSent(pCtx, 1);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var nCount = p.Decode1(); // we arent really interested in this rn
            var zeroshort = p.Decode2(); // unused??

           // character.Skills.Cast(nSkillID, false);
           // two packets are sent from cleric heal: SkillUseRequest and MagicAttack
           // since we cast the spell from the attack handler we dont need to process it again here
        }
    }
}
