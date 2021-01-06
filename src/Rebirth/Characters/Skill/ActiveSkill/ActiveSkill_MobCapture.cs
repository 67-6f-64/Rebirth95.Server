using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth.Characters.Skill.ActiveSkill
{
    public class ActiveSkill_MobCapture
    {
        public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
        {
            // Recv [CP_UserSkillUseRequest] 67 00 56 3B 36 10 A5 C7 C9 01 01 2A 4E 00 00

            //COutPacket::COutPacket(&oPacket, 103);
            //v55 = 8;
            //v41 = get_update_time();
            //COutPacket::Encode4(&oPacket, v41);
            //COutPacket::Encode4(&oPacket, pSkill->nSkillID);
            //COutPacket::Encode1(&oPacket, nSLV);
            //v42 = CMob::GetMobID(v24);
            //COutPacket::Encode4(&oPacket, v42);
            //CClientSocket::SendPacket(TSingleton < CClientSocket >::ms_pInstance, &oPacket);

            var dwMobId = p.Decode4();

            var pMob = c.Field.Mobs[dwMobId];

            if (pMob is null
                || GameConstants.is_not_capturable_mob(pMob.nMobTemplateId)
                || pMob.MaxHp * 0.5 > pMob.Stats.HP
                || !JobLogic.IsWildhunterJob(c.Stats.nJob))
                return;

            var bLeft = pMob.Position.X < c.Position.X;

            var bSuccess = c.Skills.Cast(nSkillID, bLeft);

            if (bSuccess)
            {
				new UserEffectPacket(UserEffect.SkillUse)
				{
					nSkillID = nSkillID,
					nSLV = nSLV
				}.BroadcastEffect(c, false);

				//c.SendPacket(c.WildHunterInfo.CaptureResultLocalEffect(WildHunterCaptureResult.Success));
				//c.Field.Broadcast(c.WildHunterInfo.CaptureSuccessRemoteEffect(), c);
				c.Field.Mobs.Remove(pMob);

                c.WildHunterInfo.UpdateJaguarInfo(pMob.nMobTemplateId);
            }
        }
    }
}
