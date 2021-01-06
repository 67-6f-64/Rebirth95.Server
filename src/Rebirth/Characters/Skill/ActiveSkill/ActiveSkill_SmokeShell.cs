using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_SmokeShell
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 103);
            //v21 = 0;
            //v11 = get_update_time();
            //COutPacket::Encode4(&oPacket, v11);
            //COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //COutPacket::Encode1(&oPacket, nSLV);
            //v12 = (unsigned __int16 *)((int(__thiscall *)(IVecCtrlOwnerVtbl * *, ZRef < CharacterData > *))v3->vfptr->GetPos)(
            //                               &v3->vfptr,
            //                               &result);
            //COutPacket::Encode2(&oPacket, *v12);
            //v13 = ((int(__thiscall *)(IVecCtrlOwnerVtbl * *, char *))v3->vfptr->GetPos)(&v3->vfptr, &v19);
            //COutPacket::Encode2(&oPacket, *(_WORD*)(v13 + 4));
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var x = p.Decode2();
            var y = p.Decode2();

            var bLeft = x < c.Position.X;

            c.Skills.Cast(nSkillID, bLeft);
        }
    }
}
