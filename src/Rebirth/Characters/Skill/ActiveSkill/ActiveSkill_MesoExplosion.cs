using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_MesoExplosion
    {
        // header is UserMeleeAttack (0x2F)
        public static void Handle(int nSkillID, byte nSLV, Character character, CInPacket p)
        {
    //        COutPacket::COutPacket(&oPacket, 47);
    //        LOBYTE(v240) = 6;
    //        v64 = get_field();
    //        v65 = CField::GetFieldKey(v64);
    //        COutPacket::Encode1(&oPacket, v65);
    //        COutPacket::Encode4(&oPacket, ~pDrInfo.dr0);
    //        COutPacket::Encode4(&oPacket, ~pDrInfo.dr1);
    //        COutPacket::Encode1(&oPacket, nMaxAttackCount & 0xF | 16 * nMobCount);
    //        COutPacket::Encode4(&oPacket, ~pDrInfo.dr2);
    //        COutPacket::Encode4(&oPacket, ~pDrInfo.dr3);
    //        COutPacket::Encode4(&oPacket, nSkillID);
    //        COutPacket::Encode1(&oPacket, cd->nCombatOrders);
    //        n = get_rand(pDrInfo.dr0, 0);
    //        COutPacket::Encode4(&oPacket, n);
    //        v111 = 0;
    //        v66 = CCrc32::GetCrc32(pData, 4u, n, 0, 0);
    //        COutPacket::Encode4(&oPacket, v66);
    //        if (pSkill)
    //        {
    //            v67 = SKILLENTRY::GetLevelData(pSkill, nSLV);
    //            SKILLLEVELDATA::SKILLLEVELDATA(&v142, v67);
    //            v118 = v68;
    //            v117 = v68;
    //            LOBYTE(v240) = 7;
    //            v133 |= 2u;
    //            v116 = SKILLLEVELDATA::GetCrc(v68);
    //        }
    //        else
    //        {
    //            v116 = 0;
    //        }
    //        v219 = v116;
    //        v240 = 6;
    //        if (v133 & 2)
    //        {
    //            v133 &= 0xFFFFFFFD;
    //            SKILLLEVELDATA::~SKILLLEVELDATA(&v142);
    //        }
    //        if (pSkill)
    //        {
    //            v69 = SKILLENTRY::GetLevelData(pSkill, nSLV);
    //            SKILLLEVELDATA::SKILLLEVELDATA(&v141, v69);
    //            v115 = v70;
    //            v114 = v70;
    //            LOBYTE(v240) = 8;
    //            v133 |= 4u;
    //            v113 = SKILLLEVELDATA::GetCrc(v70);
    //        }
    //        else
    //        {
    //            v113 = 0;
    //        }
    //        v210 = v113;
    //        v240 = 6;
    //        if (v133 & 4)
    //        {
    //            v133 &= 0xFFFFFFFB;
    //            SKILLLEVELDATA::~SKILLLEVELDATA(&v141);
    //        }
    //        COutPacket::Encode4(&oPacket, v219);
    //        COutPacket::Encode4(&oPacket, v210);
    //        COutPacket::Encode1(&oPacket, 8 * bShadowPartner);
    //        COutPacket::Encode2(&oPacket, nAttackAction & 0x7FFF | ((_WORD)v226 << 15));
    //        COutPacket::Encode4(&oPacket, v215);
    //        COutPacket::Encode1(&oPacket, nAttackActionType);
    //        COutPacket::Encode1(&oPacket, nAttackSpeed);
    //        COutPacket::Encode4(&oPacket, tAttackTime);
    //        COutPacket::Encode4(&oPacket, 0);
    //        v111 = (ZRef<CharacterData>*)&v191;
    //        ((void(__thiscall *)(IVecCtrlOwnerVtbl * *, int *))v132->vfptr->GetPos)(&v132->vfptr, &v191);
    //        nTargetID = 0;
    //        v193 = 0x7FFFFFFF;
    //        for (i = 0; i < nMobCount; ++i)
    //        {
    //            v162 = &v182[152 * i];
    //            v111 = (ZRef<CharacterData>*)&v160;
    //            (*(void(__thiscall * *)(int, int *))(*(_DWORD*)(*(_DWORD*)&v182[152 * i] + 4) + 16))(
    //              *(_DWORD*)&v182[152 * i] + 4,
    //              &v160);
    //            v186 = v160 - v191;
    //            v235 = v161 - v192;
    //            v159 = (v161 - v192) * (v161 - v192) + (v160 - v191) * (v160 - v191);
    //            if (v159 < v193)
    //            {
    //                v193 = v159;
    //                nTargetID = CMob::GetMobID(*(CMob**)v162);
    //            }
    //            v71 = CMob::GetMobID(*(CMob**)v162);
    //            COutPacket::Encode4(&oPacket, v71);
    //            COutPacket::Encode1(&oPacket, v162[4]);
    //            v72 = CMob::IsLeft(*(CMob**)v162);
    //            COutPacket::Encode1(&oPacket, v162[8] & 0x7F | (v72 << 7));
    //            COutPacket::Encode1(&oPacket, v162[12]);
    //            v112 = CMob::GetCurTemplate(*(CMob**)v162)
    //                && (v73 = CMob::GetTemplate(*(CMob**)v162), v73 != CMob::GetCurTemplate(*(CMob**)v162));
    //            v74 = (_BYTE)v112 << 7;
    //            v75 = CMob::GetCalcDamageStatIndex(*(CMob**)v162);
    //            COutPacket::Encode1(&oPacket, v75 & 0x7F | v74);
    //            v111 = (ZRef<CharacterData>*)&v140;
    //            v76 = (unsigned __int16 *)(*(int(__thiscall * *)(int, char *))(*(_DWORD*)(*(_DWORD*)v162 + 4) + 16))(
    //                                        *(_DWORD*)v162 + 4,
    //                                        &v140);
    //        COutPacket::Encode2(&oPacket, *v76);
    //        v111 = (ZRef<CharacterData>*)&v139;
    //        v77 = (*(int(__thiscall * *)(int, char *))(*(_DWORD*)(*(_DWORD*)v162 + 4) + 16))(
    //                *(_DWORD*)v162 + 4,
    //                &v139);
    //        COutPacket::Encode2(&oPacket, *(_WORD*)(v77 + 4));
    //        v111 = (ZRef<CharacterData>*)&v138;
    //        v78 = (unsigned __int16 *)(*(int(__thiscall * *)(int, char *))(*(_DWORD*)(*(_DWORD*)v162 + 4) + 20))(
    //                                    *(_DWORD*)v162 + 4,
    //                                    &v138);
    //        COutPacket::Encode2(&oPacket, *v78);
    //        v111 = (ZRef<CharacterData>*)&v137;
    //        v79 = (*(int(__thiscall * *)(int, char *))(*(_DWORD*)(*(_DWORD*)v162 + 4) + 20))(
    //                *(_DWORD*)v162 + 4,
    //                &v137);
    //        COutPacket::Encode2(&oPacket, *(_WORD*)(v79 + 4));
    //        COutPacket::Encode1(&oPacket, v162[20]);
    //        for (k = 0; k < *((_DWORD*)v162 + 5); ++k)
    //        {
    //            COutPacket::Encode4(&oPacket, *(_DWORD*)&v162[4 * k + 24]);
    //            if (TSingleton < CDamageMeter >::IsInstantiated())
    //            {
    //                v111 = *(ZRef<CharacterData>**)&v162[4 * k + 24];
    //                v80 = TSingleton < CDamageMeter >::GetInstance();
    //                CDamageMeter::AddDamageInfo(v80, (int)v111);
    //            }
    //            if (TSingleton < CBattleRecordMan >::IsInstantiated())
    //            {
    //                v111 = 0;
    //                v110 = *(_DWORD*)&v162[4 * k + 84];
    //                v109 = *(_DWORD*)&v162[4 * k + 24];
    //                v81 = TSingleton < CBattleRecordMan >::GetInstance();
    //                CBattleRecordMan::SetBattleDamageInfo(v81, v109, v110, (int)v111);
    //            }
    //        }
    //        v82 = CMob::GetCrc(*(CMob**)v162);
    //        COutPacket::Encode4(&oPacket, v82);
    //    }
    //    CUser::SetTargetID(v132, nTargetID);
    //      v111 = (ZRef<CharacterData>*)&v136;
    //      v83 = (unsigned __int16 *)((int (__thiscall*) (IVecCtrlOwnerVtbl**, char*))v132->vfptr->GetPos)(
    //                                  &v132->vfptr,
    //                                  &v136);
    //      COutPacket::Encode2(&oPacket, *v83);
    //      v111 = (ZRef<CharacterData>*)&v135;
    //      v84 = ((int (__thiscall*) (IVecCtrlOwnerVtbl**, char*))v132->vfptr->GetPos)(&v132->vfptr, &v135);
    //      COutPacket::Encode2(&oPacket, *(_WORD*) (v84 + 4));
    //      v85 = ZArray<DROP*>::GetCount(&aDrop);
    //      COutPacket::Encode1(&oPacket, v85);
    //      for (i = 0; ; ++i )
    //      {
    //        v86 = ZArray<DROP*>::GetCount(&aDrop);
    //        if (i >= v86 )
    //          break;
    //        v87 = ZArray<DROP*>::operator[]<long>(&aDrop, i);
    //        COutPacket::Encode4(&oPacket, (* v87)->dwId);
    //        COutPacket::Encode1(&oPacket, v238[4 * i]);
    //      }
    //COutPacket::Encode2(&oPacket, v214);
    //      SendPacket(&oPacket);
    //CUserLocal::SetPetsAngry((CUserLocal*) v132);
    //      ActionRandMan::PreventRollback(&v196);
    //      v134 = 1;
    //      LOBYTE(v240) = 4;
    //      COutPacket::~COutPacket(&oPacket);
        }
    }
}
