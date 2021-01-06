using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth
{
	public partial class CPacket
	{
		// CSummonedPool::OnPacket
		public static class CSummonedPool
		{
			// CSummonedPool::OnCreated
			public static COutPacket EnterFieldPacket(CSummon summon)
			{
				var p = new COutPacket(SendOps.LP_SummonedEnterField);
				p.Encode4(summon.dwParentID);
				summon.EncodeInitData(p);
				return p;
			}

			// CSummonedPool::OnRemoved
			public static COutPacket LeaveFieldPacket(CSummon summon)
			{
				var p = new COutPacket(SendOps.LP_SummonedLeaveField);
				p.Encode4(summon.dwParentID);
				p.Encode4(summon.dwId);
				p.Encode1((byte)summon.nLeaveType);
				return p;
			}

			// CSummonedPool::OnMove
			public static COutPacket SummonedMove(CSummon summon, CInPacket iPacket)
			{
				var oPacket = new COutPacket(SendOps.LP_SummonedMove);
				oPacket.Encode4(summon.dwParentID);
				oPacket.Encode4(summon.dwId);
				summon.Position.UpdateMovePath(oPacket, iPacket);
				return oPacket;
			}

			// CSummonedPool::OnAttack
			public static COutPacket SummonedAttack(CSummon summon, int attackSkillId, byte nActionAndDir, List<SummonAttackInfo> aSAI)
			{
				var p = new COutPacket(SendOps.LP_SummonedAttack);
				p.Encode4(summon.dwParentID);
				p.Encode4(attackSkillId);
				p.Encode1(summon.nSLV);
				p.Encode1(nActionAndDir);
				p.Encode1((byte)aSAI.Count);
				foreach (var attackInfo in aSAI)
				{
					p.Encode4(attackInfo.dwMobID);
					p.Encode1(attackInfo.nHitAction);
					p.Encode4(attackInfo.nDamage);
				}
				p.Encode1(attackSkillId == (int)Skills.HERMIT_SHADOW_MIRROR || attackSkillId == (int)Skills.THIEFMASTER_SHADOW_MIRROR);
				return p;
			}

			// CSummonedPool::OnSkill
			public static COutPacket SummonedSkill(CSummon summon, int summonSkillId, byte bAttackAction)
			{
				var p = new COutPacket(SendOps.LP_SummonedSkill);
				p.Encode4(summon.dwParentID);
				p.Encode4(summonSkillId);
				p.Encode1(bAttackAction);
				return p;
			}

			// CSummonedPool::OnHit
			public static COutPacket SummonedHit(CSummon summon,  byte nAttackIdx, int nDamage, int dwMobId, byte bLeft)
			{
				var p = new COutPacket(SendOps.LP_SummonedHit);
				p.Encode4(summon.dwParentID);
				p.Encode4(summon.dwId);
				p.Encode1(nAttackIdx);
				p.Encode4(nDamage);
				p.Encode4(dwMobId);
				p.Encode1(bLeft);
				return p;
			}

			/// <summary>
			/// This packet is sent to activate the tesla coils.
			/// The client will expect three coils in the map for this to work.
			/// </summary>
			public static COutPacket TeslaTriangle(int dwParentID, List<int> teslaCoilSummonIDs)
			{
				var p = new COutPacket(SendOps.LP_UserTeslaTriangle);
				p.Encode4(dwParentID);
				teslaCoilSummonIDs.ForEach(coil => p.Encode4(coil));
				return p;
			}
		}
	}
}
