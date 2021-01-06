using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_RecoveryAura
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            //COutPacket::COutPacket(&oPacket, 103);
            //v33 = 1;
            //v15 = get_update_time();
            //COutPacket::Encode4(&oPacket, v15);
            //COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //v16 = nSLV;
            //COutPacket::Encode1(&oPacket, nSLV);
            //v17 = (unsigned __int16 *)((int(__thiscall *)(IVecCtrlOwnerVtbl * *, ZRef < CharacterData > *))v3->vfptr->GetPos)(
            //                               &v3->vfptr,
            //                               &result);
            //COutPacket::Encode2(&oPacket, *v17);
            //v18 = ((int(__thiscall *)(IVecCtrlOwnerVtbl * *, char *))v3->vfptr->GetPos)(&v3->vfptr, &v31);
            //COutPacket::Encode2(&oPacket, *(_WORD*)(v18 + 4));
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            // position of aura?
            var x = p.Decode2();
            var y = p.Decode2();

            var bLeft = x < c.Position.X;

            c.Skills.Cast(nSkillID, bLeft);
        }
    }
}
