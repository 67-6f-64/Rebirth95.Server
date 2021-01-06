using System;
using System.Linq;
using Rebirth.Characters;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Provider.Template.Skill;
using Rebirth.Server.Center;

namespace Rebirth.Field.FieldObjects
{
	public struct SummonAttackInfo
	{
		public int dwMobID { get; set; }
		public byte nHitAction { get; set; }
		public int nDamage { get; set; }

		//public int get_summon_attack_type()
		//{
		//	// nAction = nHitAction & 0x7F;
		//	// p = CSummonedBase::Action2AIType(nAction);
		//	switch (nHitAction & 0xF) // making this assumption from the above lines in CSummoned::OnAttack
		//	{

		//	}
		//}
	}

	public class CSummon : CFieldObj
	{
		public SkillTemplate Template => MasterManager.SkillTemplates[nSkillID];

		public Character Parent { get; set; }
		public int dwParentID => Parent.dwId;
		public int nSkillID { get; }
		public short nCurHP { get; set; }
		public short nMaxHP { get; }
		public byte nCharLevel => Parent.Stats.nLevel;
		public byte nSLV { get; set; } = 1;
		public TeslaCoilType nTeslaCoilState { get; set; }
		public DateTime tLastBuffTime { get; set; }
		public DateTime tExpiration { get; set; }
		public DateTime tLastSiegeMechProc { get; set; } // TODO remove this

		public SummonLeaveType nLeaveType { get; set; } = SummonLeaveType.LEAVE_TYPE_DEFAULT;
		public SummonEnterType nEnterType { get; set; }
		public SummonMoveAbility nMoveAbility => GetMoveAbility();
		public AssistType nAssistType => GetAssistType();

		/// <summary>
		/// TODO encode all of these static types on template creation
		/// </summary>

		private AssistType GetAssistType()
		{
			switch ((Skills)nSkillID)
			{
				// no need to iterate these since they are the default value
				// im keeping them in debug so i can make sure i dont add double entries
#if DEBUG
				case Skills.CAPTAIN_SUPPORT_OCTOPUS:
				case Skills.VALKYRIE_OCTOPUS:
				case Skills.DUAL4_MIRROR_IMAGING:
				case Skills.WILDHUNTER_TRAP:
				case Skills.RANGER_SILVER_HAWK:
				case Skills.SNIPER_GOLDEN_EAGLE:
				case Skills.PRIEST_SUMMON_DRAGON:
				case Skills.CROSSBOWMASTER_FREEZER:
				case Skills.BOWMASTER_PHOENIX:
				case Skills.VALKYRIE_GABIOTA:
				case Skills.ARCHMAGE2_ELQUINES:
				case Skills.ARCHMAGE1_IFRIT:
				case Skills.FLAMEWIZARD_IFRIT:
				case Skills.WINDBREAKER_STORM:
				case Skills.NIGHTWALKER_DARKNESS:
				case Skills.STRIKER_LIGHTNING:
				case Skills.BMAGE_REVIVE:
				case Skills.MECHANIC_ROBOROBO_DUMMY:
					return AssistType.ASSIST_ATTACK;
#endif
				case Skills.MECHANIC_SG88:
					return AssistType.ASSIST_ATTACK_MANUAL;

				case Skills.THIEFMASTER_SHADOW_MIRROR:
				case Skills.HERMIT_SHADOW_MIRROR:
					return AssistType.ASSIST_ATTACK_COUNTER;

				case Skills.DARKKNIGHT_BEHOLDER:
					return AssistType.ASSIST_HEAL;

				case Skills.DUAL5_DUMMY_EFFECT:
				case Skills.RANGER_PUPPET:
				case Skills.SNIPER_PUPPET:
				case Skills.WINDBREAKER_PUPPET:
				case Skills.MECHANIC_VELOCITY_CONTROLER:
				case Skills.MECHANIC_AR_01: // AF-11
				case Skills.MECHANIC_HEALING_ROBOT_H_LX:
				case Skills.WILDHUNTER_MINE_DUMMY_SUMMONED:
				case Skills.MECHANIC_TESLA_COIL:
					return AssistType.ASSIST_NONE;

				case Skills.MECHANIC_ROBOROBO:
					return AssistType.ASSIST_SUMMON;

				case Skills.MECHANIC_SATELITE:
				case Skills.MECHANIC_SATELITE2:
				case Skills.MECHANIC_SATELITE3:
					return AssistType.ASSIST_ATTACK_EX;
			}
			return AssistType.ASSIST_ATTACK;
		}

		private SummonMoveAbility GetMoveAbility()
		{
			switch ((Skills)nSkillID)
			{
				case Skills.RANGER_PUPPET:
				case Skills.SNIPER_PUPPET:
				case Skills.WINDBREAKER_PUPPET:
				case Skills.CAPTAIN_SUPPORT_OCTOPUS:
				case Skills.VALKYRIE_OCTOPUS:
				case Skills.DUAL5_DUMMY_EFFECT:
				case Skills.WILDHUNTER_MINE_DUMMY_SUMMONED:
				case Skills.WILDHUNTER_TRAP:
				case Skills.MECHANIC_TESLA_COIL:
				case Skills.MECHANIC_VELOCITY_CONTROLER:
				case Skills.MECHANIC_SG88:
				case Skills.THIEFMASTER_SHADOW_MIRROR:
				case Skills.HERMIT_SHADOW_MIRROR:
				case Skills.MECHANIC_ROBOROBO:
				case Skills.MECHANIC_AR_01:
				case Skills.MECHANIC_HEALING_ROBOT_H_LX:
					return SummonMoveAbility.NoMove;

				case Skills.RANGER_SILVER_HAWK:
				case Skills.SNIPER_GOLDEN_EAGLE:
				case Skills.PRIEST_SUMMON_DRAGON:
				case Skills.CROSSBOWMASTER_FREEZER:
				case Skills.BOWMASTER_PHOENIX:
				case Skills.VALKYRIE_GABIOTA:
					return SummonMoveAbility.CircleFollow;
#if DEBUG // these have the default value
				case Skills.DARKKNIGHT_BEHOLDER:
				case Skills.ARCHMAGE2_ELQUINES:
				case Skills.ARCHMAGE1_IFRIT:
				case Skills.FLAMEWIZARD_IFRIT:
				case Skills.WINDBREAKER_STORM:
				case Skills.NIGHTWALKER_DARKNESS:
				case Skills.STRIKER_LIGHTNING:
				case Skills.MECHANIC_SATELITE:
				case Skills.MECHANIC_SATELITE2:
				case Skills.MECHANIC_SATELITE3:
					return SummonMoveAbility.Follow;
#endif

				case Skills.BMAGE_REVIVE:
				case Skills.MECHANIC_ROBOROBO_DUMMY:
					return SummonMoveAbility.WalkRandom;
			}
			return SummonMoveAbility.Follow;
		}

		public CSummon(Character c, int nSkillID)
		{
			Parent = c;
			this.nSkillID = nSkillID;
			nMaxHP = (short)Template.X(nSLV);
			tLastBuffTime = DateTime.Now;
			tLastSiegeMechProc = DateTime.Now;
		}

		public void EncodeInitData(COutPacket p)
		{
			p.Encode4(dwId);
			p.Encode4(nSkillID);
			p.Encode1(nCharLevel);
			p.Encode1(nSLV);
			Position.EncodePos(p);
			p.Encode1(Position.MoveAction);
			p.Encode2(Position.Foothold);
			p.Encode1((byte)nMoveAbility);
			p.Encode1((byte)nAssistType);
			p.Encode1((byte)nEnterType);

			var mirrorTarget = nSkillID == (int)Skills.DUAL5_DUMMY_EFFECT;
			p.Encode1(mirrorTarget);
			if (mirrorTarget)
			{
				Parent.GetLook().Encode(p);
			}
			else if (nSkillID == (int)Skills.MECHANIC_TESLA_COIL)
			{
				p.Encode1((byte)nTeslaCoilState);

				if (nTeslaCoilState == TeslaCoilType.Leader)
				{
					foreach (var item in Field.Summons
					.Where(item => item.nSkillID == (int)Skills.MECHANIC_TESLA_COIL && dwParentID == item.dwParentID))
					{
						p.Encode2(item.Position.X);
						p.Encode2(item.Position.Y);
					}
				}
			}
		}

		public override COutPacket MakeEnterFieldPacket() => CPacket.CSummonedPool.EnterFieldPacket(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.CSummonedPool.LeaveFieldPacket(this);
	}
}