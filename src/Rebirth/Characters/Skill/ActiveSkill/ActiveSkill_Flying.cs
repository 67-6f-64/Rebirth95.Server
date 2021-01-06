using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_Flying
    {
        public static void Handle(int nSkillID, byte nSLV, Character character, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 103);
            //v16 = 1;
            //v9 = get_update_time();
            //COutPacket::Encode4(&oPacket, v9);
            //v10 = nSkillID;
            //COutPacket::Encode4(&oPacket, nSkillID);
            //COutPacket::Encode1(&oPacket, 1);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            // prolly just validate and then broadcast
        }
    }
}
