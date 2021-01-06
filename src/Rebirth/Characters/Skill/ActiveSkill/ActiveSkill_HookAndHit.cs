using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
	public class ActiveSkill_HookAndHit
    {
        // actually in CUserLocal::TryDoingMonsterMagnet
        // but that function is only called by the active which has the name of this class
        // 
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            //            COutPacket::COutPacket(&oPacket, 103);
            //            LOBYTE(v115) = 2;
            //            v81 = get_update_time();
            //            COutPacket::Encode4(&oPacket, v81);
            //            COutPacket::Encode4(&oPacket, v76->nSkillID);
            //            COutPacket::Encode1(&oPacket, nSLV);
            //            v82 = nRange;
            //            COutPacket::Encode4(&oPacket, nRange);
            //            for (k = 0; k < v82; ++k)
            //            {
            //                if (apMob.a[k].p)
            //                {
            //                    v84 = apMob.a[k].p;
            //                    v94 = (int*)v84->_ZtlSecureTear_m_dwMobID_CS;
            //                    v85 = _ZtlSecureFuse < unsigned long> (v84->_ZtlSecureTear_m_dwMobID, (unsigned int)v94);
            //        }
            //    else
            //    {
            //      v85 = 0;
            //    }
            //    COutPacket::Encode4(&oPacket, v85);
            //    v86 = anMobMove[k];
            //    v87 = v86 == 3 || v86 == 4;
            //    COutPacket::Encode1(&oPacket, v87);
            //  }
            //COutPacket::Encode1(&oPacket, v71->m_nMoveAction & 1);
            //  CClientSocket::SendPacket(TSingleton<CClientSocket>::ms_pInstance, &oPacket);

            var nCount = p.Decode4();
            var apMob = new int[nCount];

            for (var i = 0; i > nCount; i++)
            {
                apMob[i] = p.Decode4();
                var anMobMove = p.Decode1(); // i dont think we need this???
            }

            var bLeft = p.Decode1() > 0;

            if (c.Skills.Cast(nSkillID, bLeft))
            {
				new UserEffectPacket(UserEffect.SkillUse)
				{
					nSkillID = nSkillID,
					nSLV = nSLV,
					bLeft = bLeft,
					dwMobId = nCount > 0 ? apMob[0] : 0, // idk
				}.BroadcastEffect(c, false);

				foreach (var dwMobId in apMob)
				{
					if (c.Field.Mobs.TryGetValue(dwMobId, out CMob cMob))
					{
						cMob.TryApplySkillDamageStatus(c, nSkillID, nSLV, 0); // todo configure this
					}
				}
			}
        }
    }
}
