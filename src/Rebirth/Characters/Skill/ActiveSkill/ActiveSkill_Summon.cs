using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Skill.ActiveSkill
{
	public class ActiveSkill_Summon
	{
		public static void Handle(int nSkillID, byte nSLV, Character c, CInPacket p)
		{
			var ldwTeslaCoilSummonedID = new int[2];

			if (nSkillID == (int)Skills.MECHANIC_TESLA_COIL)
			{
				var nDistance = p.Decode1();

				if (nDistance == 2)
				{
					ldwTeslaCoilSummonedID[0] = p.Decode4();
					ldwTeslaCoilSummonedID[1] = p.Decode4();
				}
			}

			var x = p.Decode2();
			var y = p.Decode2();

			var bLeft = p.Decode1() > 0;

			var unk = p.Decode1(); // FH?

			if (nSkillID == (int)Skills.WILDHUNTER_MINE_DUMMY_SUMMONED && !c.Buffs.Contains((int)Skills.WILDHUNTER_MINE))
			{
				c.SendMessage("Unable to spawn mine when buff isn't activated.");
				return;
			}

			if (!c.Skills.Cast(nSkillID, bLeft, true)) return;

			var robotMastery = c.Skills.Get((int)Skills.MECHANIC_MASTERY);
			var durationMultiplier = (100 + (robotMastery?.X_Effect ?? 0)) * 0.01;

			var pSkill = c.Skills.Get(nSkillID, true);

			if (ldwTeslaCoilSummonedID[0] > 0)
			{
				if (!c.Field.Summons.Handle_TeslaCoil(c, nSLV, x, y, ldwTeslaCoilSummonedID)) return;
			}
			else
			{
				var summon = c.Field.Summons.CreateSummon(c, nSkillID, nSLV, x, y);

				switch (summon.nMoveAbility)
				{
					case SummonMoveAbility.CircleFollow:
					case SummonMoveAbility.Follow:
						c.Buffs.AddSkillBuff(nSkillID, pSkill.nSLV, durationMultiplier);
						break;
				}
			}

			var effect = new UserEffectPacket(UserEffect.SkillUse)
			{
				nSkillID = nSkillID,
				nSLV = pSkill.nSLV,
			};

			effect.BroadcastEffect(c, false);

			switch ((Skills)nSkillID)
			{
				case Skills.MECHANIC_VELOCITY_CONTROLER:
					{
						foreach (var item in c.Field.Mobs)
						{
							item.TryApplySkillDamageStatus(c, nSkillID, pSkill.nSLV, 0);
						}

						c.Field.nVelocityControllerdwId = c.dwId;
						break;
					}
				case Skills.MECHANIC_AR_01:
					c.Skills.CastAffectedAreaSkill(nSkillID, nSLV, (short)pSkill.BuffTime,
						c.Position.CurrentXY, pSkill.Template.LT, pSkill.Template.RB);
					//if (c.Party?.Count > 1)
					//{
					// c.Party.ApplyBuffToParty(c, c.Field.dwUniqueId, nSkillID, nSLV);
					//}
					//else
					//{
					// c.Buffs.AddSkillBuff(nSkillID, pSkill.nSLV);
					//}
					break;
			}
		}
	}
}
