using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_StatChangeAdmin
    {
        public static void Handle(int nSkillID, byte nSLV, Character character, CInPacket p)
        {
            //          COutPacket::COutPacket(&oPacket, 103);
            //          LOBYTE(v37) = 2;
            //          v23 = get_update_time();
            //          COutPacket::Encode4(&oPacket, v23);
            //          COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //          COutPacket::Encode1(&oPacket, nSLV);
            //          v24 = adwUserID.a;
            //          if (adwUserID.a)
            //              v25 = *(adwUserID.a - 1);
            //          else
            //              LOBYTE(v25) = 0;
            //          COutPacket::Encode1(&oPacket, v25);
            //          v26 = 0;
            //          if (ZArray < unsigned long>::GetCount(&adwUserID) )
            //{
            //              do
            //                  COutPacket::Encode4(&oPacket, v24[v26++]);
            //              while (v26 < ZArray < unsigned long>::GetCount(&adwUserID) );
            //          }
            //          COutPacket::Encode2(&oPacket, (unsigned __int16)cd);
            //          CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var v25 = p.Decode1();

            var ZArray = p.DecodeIntArray(v25);

            var cd = p.Decode2();
        }
    }
}
