using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using System;
using Rebirth.Common.Types;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CMobPool
		{
			public static COutPacket MobAttackedByMob(int dwMobId, byte nAttackIdx, int nDamage, int dwMobTemplateID, byte bLeft)
			{
				var p = new COutPacket(SendOps.LP_MobAttackedByMob);
				p.Encode4(dwMobId);
				p.Encode1(nAttackIdx);
				p.Encode4(nDamage);
				p.Encode4(dwMobTemplateID);
				p.Encode1(bLeft);
				return p;
			}

			public static COutPacket MobMove(int dwMobID, bool bNextAttackPossible, byte nActionAndDir, int dwData)
			{
				var oPacket = new COutPacket(SendOps.LP_MobMove);
				oPacket.Encode4(dwMobID);
				oPacket.Encode1(false); // bNotForceLanding
				oPacket.Encode1(false); // bNotChangeAction
				oPacket.Encode1(bNextAttackPossible);
				oPacket.Encode1(nActionAndDir);

				oPacket.Encode4(dwData); // skill information

				oPacket.Encode4(0); // aMultiTargetForBall LOOP
				oPacket.Encode4(0); // aRandTimeforAreaAttack LOOP

				// make sure to encode movement information afterwards

				return oPacket;
			}

			public static COutPacket MobMoveAck(int dwMobID, short nMobCtrlSN, bool bNextAttackPossible, int nMP, SkillCommand pCommand)
			{
				var p = new COutPacket(SendOps.LP_MobCtrlAck);
				p.Encode4(dwMobID);
				p.Encode2(nMobCtrlSN);
				p.Encode1(bNextAttackPossible);
				p.Encode2((short)Math.Min(short.MaxValue, nMP));
				p.Encode1((byte)pCommand.nSkillID);
				p.Encode1((byte)pCommand.nSLV);
				return p;
			}

			public static COutPacket MobSuspendReset(int dwMobID, bool suspendReset)
			{
				var p = new COutPacket(SendOps.LP_MobSuspendReset);
				p.Encode4(dwMobID);
				p.Encode1(suspendReset);
				return p;
			}

			public static COutPacket MobAffected(int dwMobID, int nSkillID, int tDelay)
			{
				var p = new COutPacket(SendOps.LP_MobAffected);
				p.Encode4(dwMobID);
				p.Encode4(nSkillID);
				p.Encode2((short)(tDelay > short.MaxValue ? short.MaxValue : tDelay));
				return p;
			}
			public static COutPacket MobEnterField(CMob mob)
			{
				var p = new COutPacket(SendOps.LP_MobEnterField);

				p.Encode4(mob.dwId);
				p.Encode1(5); //  nCalcDamageIndex | Controller
				p.Encode4(mob.nMobTemplateId);

				//CMob::Init
				mob.EncodeInitData(p);

				return p;
			}

			public static COutPacket MobLeaveField(CMob mob)
			{
				var p = new COutPacket(SendOps.LP_MobLeaveField);
				p.Encode4(mob.dwId);
				p.Encode1((byte)mob.LeaveFieldType); // 0 = dissapear, 1 = fade out, 2+ = special

				if (mob.LeaveFieldType == MobLeaveFieldType.MOBLEAVEFIELD_SWALLOW)
					p.Encode4(mob.m_dwSwallowCharacterID);

				return p;
			}

			public static COutPacket MobChangeController(CMob mob, byte nLevel)
			{
				var p = new COutPacket(SendOps.LP_MobChangeController);
				p.Encode1(nLevel); // 0 = None | 1 = Control | 2 = Aggro
				p.Encode4(mob.dwId);

				if (nLevel > 0)
				{
					p.Encode1(5); //  nCalcDamageIndex | Controller
					p.Encode4(mob.nMobTemplateId);
					mob.EncodeInitData(p);
				}

				return p;
			}

			public static COutPacket MobHPIndicator(int dwMobId, byte nHPPercentage)
			{
				var p = new COutPacket(SendOps.LP_MobHPIndicator);
				p.Encode4(dwMobId);
				p.Encode1(nHPPercentage);
				return p;
			}

			public static COutPacket MobSpecialEffectBySkill(int dwMobId, int nSkillID, int casterId, short tDelay)
			{
				var p = new COutPacket(SendOps.LP_MobSpecialEffectBySkill);
				p.Encode4(dwMobId);
				p.Encode4(nSkillID);
				p.Encode4(casterId);
				p.Encode2(tDelay);
				return p;
			}

			public static COutPacket MobSkillDelay(int dwMobId, int tSkillDelayTime, int nSkillID, int nSLV, int nOption)
			{
				var p = new COutPacket(SendOps.LP_MobSkillDelay);
				p.Encode4(dwMobId);
				p.Encode4(tSkillDelayTime);
				p.Encode4(nSkillID);
				p.Encode4(nSLV);
				p.Encode4(nOption);
				return p;
			}

			public static COutPacket MobCtrlAck(int dwMobId, short nMobCtrlSN, bool bNextAttackPossible, short nMP, byte nSkillCommand, byte nSLV)
			{
				var p = new COutPacket(SendOps.LP_MobCtrlAck);
				p.Encode4(dwMobId);
				p.Encode2(nMobCtrlSN);
				p.Encode1(bNextAttackPossible);
				p.Encode2(nMP); //CMob->nMP lol
				p.Encode1(nSkillCommand);
				p.Encode1(nSLV);
				return p;
			}

			public static COutPacket MobDamaged(int dwMobID, byte nType, int decHP)
			{
				var p = new COutPacket(SendOps.LP_MobDamaged);
				p.Encode4(dwMobID);
				p.Encode1(nType);
				p.Encode4(decHP);
				p.Encode8(0);// TODO
							 //if (template.isDamagedByMob()) // replace with enc8
							 //{
							 //	packet.encodeInt(getHP());
							 //	packet.encodeInt(template.getMaxHP());
							 //}
				return p;
			}
		}


		//if (nType == 0x131 )
		//  {
		//    CMob::OnEscortStopEndPermmision(v5);
		//  }
		//  else
		//  {
		//    switch (nType )
		//    {
		//      case 0x11F:
		//        CMob::OnMove(v5, iPacket);
		//        break;
		//      case 0x120:
		//        CMob::OnCtrlAck(v5, iPacket);
		//        break;
		//      case 0x122:
		//        CMob::OnStatSet(v5, iPacket);
		//        break;
		//      case 0x123:
		//        CMob::OnStatReset(v5, iPacket);
		//        break;
		//      case 0x124:
		//        CMob::OnSuspendReset(v5, iPacket);
		//        break;
		//      case 0x125:
		//        CMob::OnAffected(v5, iPacket);
		//        break;
		//      case 0x126:
		//        CMob::OnDamaged(v5, iPacket);
		//        break;
		//      case 0x127:
		//        CMob::OnSpecialEffectBySkill(v5, iPacket);
		//        break;
		//      case 0x12A:
		//        CMob::OnHPIndicator(v5, iPacket);
		//        break;
		//      case 0x12B:
		//        CMob::OnCatchEffect(v5, iPacket);
		//        break;
		//      case 0x12C:
		//        CMob::OnEffectByItem(v5, iPacket);
		//        break;
		//      case 0x12D:
		//        CMob::OnMobSpeaking(v5, iPacket);
		//        break;
		//      case 0x12E:
		//        CMob::OnIncMobChargeCount(v5, iPacket);
		//        break;
		//      case 0x12F:
		//        CMob::OnMobSkillDelay(v5, iPacket);
		//        break;
		//      case 0x130:
		//        CMob::OnEscortFullPath(v5, iPacket);
		//        break;
		//      case 0x132:
		//        CMob::OnEscortStopSay(v5, iPacket);
		//        break;
		//      case 0x133:
		//        CMob::OnEscortReturnBefore(v5, iPacket);
		//        break;
		//      case 0x134:
		//        CMob::OnNextAttack(v5, iPacket);
		//        break;
		//      case 0x135:
		//        CMob::OnMobAttackedByMob(v5, iPacket);
		//        break;
		//      default:
		//        return;
	}
}
