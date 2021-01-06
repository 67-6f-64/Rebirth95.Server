using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_DamageMeter
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 103);
            //LOBYTE(v17) = 2;
            //v10 = get_update_time();
            //COutPacket::Encode4(&oPacket, v10);
            //COutPacket::Encode4(&oPacket, nSkillID);
            //COutPacket::Encode1(&oPacket, 1);
            //COutPacket::Encode4(&oPacket, v7->m_nInputNo_Result);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var nInputNo_Result = p.Decode4();
        }
    }
}
