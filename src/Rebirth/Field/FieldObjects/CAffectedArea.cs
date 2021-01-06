using Rebirth.Characters.Skill;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Drawing;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.Tools;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Field.FieldObjects
{
	public class CAffectedArea : CFieldObj
	{
		public int dwOwnerId { get; }

		public DateTime StartTime { get; set; }
		public int Duration { get; set; }

		public AffectedAreaType nType { get; }
		public int nSkillID { get; }
		public byte nSLV { get; set; }
		public short tDelay { get; set; }
		public TagRect rcArea { get; set; }
		public int nBuffItem { get; set; }
		public int nPhase { get; set; }

		/// <summary>
		/// Creates an affected area using the coordinates of the 
		///     lt/rb points, offset by the position
		/// </summary>
		public CAffectedArea(int nSkillID, int dwOwnerId, TagPoint position, Point lt, Point rb, bool bLeft)
		{
			this.nSkillID = nSkillID;
			this.dwOwnerId = dwOwnerId;
			rcArea = new TagRect(lt.X, lt.Y, rb.X, rb.Y);
			rcArea.OffsetRect(position, bLeft);
			nType = AffectedAreaConstants.GetAreaType(nSkillID);
		}

		public CAffectedArea(int nSkillID, int dwOwnerId, TagPoint position, Point lt, Point rb, AffectedAreaType nAreaType)
		{
			this.nSkillID = nSkillID;
			this.dwOwnerId = dwOwnerId;
			rcArea = new TagRect(lt.X, lt.Y, rb.X, rb.Y);
			rcArea.OffsetRect(position, true);
			nType = nAreaType;
		}

		public bool Update()
		{
			switch (nType)
			{
				case AffectedAreaType.UserSkill:
					foreach (var mob in Field.Mobs)
					{
						if (!rcArea.PointInRect(mob.Position.CurrentXY)) continue;

						mob.TryApplySkillDamageStatus
							(Field.Users[dwOwnerId], nSkillID, nSLV, 0);
					}
					break;
				case AffectedAreaType.BlessedMist: // TODO fix the recovery numbers, its incorrect rn
					{
						var rate = MasterManager.SkillTemplates[nSkillID].X(nSLV) * 0.01;
						foreach (var user in Field.Users)
						{
							if (!rcArea.PointInRect(user.Position.CurrentXY)) continue;

							user.Modify.Heal(0, (int)(user.BasicStats.nMMP * rate));

							var effect = new UserEffectPacket(UserEffect.SkillAffected)
							{
								nSkillID = nSkillID,
								nSLV = nSLV,
							};

							effect.BroadcastEffect(user);
						}
					}
					break;
				case AffectedAreaType.MobSkill:
					foreach (var user in Field.Users)
					{
						if (!rcArea.PointInRect(user.Position.CurrentXY)) continue;

						var toAdd = new Debuff(nSkillID, nSLV);
						toAdd.StatType = SecondaryStatFlag.Poison;
						toAdd.Generate();
						user.Buffs.Remove(toAdd.nBuffID);
						user.Buffs.Add(toAdd);
					}
					break;
			}

			return StartTime.MillisSinceStart() >= Duration;
		}

		private void EncodeInitData(COutPacket p)
		{
			p.Encode4((int)nType);
			p.Encode4(dwOwnerId);
			p.Encode4(nSkillID);
			p.Encode1(nSLV);
			p.Encode2(tDelay);
			rcArea.Encode(p);
			p.Encode4(nBuffItem);
			p.Encode4(nPhase);
		}

		private COutPacket AffectedAreaCreated(CAffectedArea area)
		{
			var p = new COutPacket(SendOps.LP_AffectedAreaCreated);
			p.Encode4(area.dwId);
			area.EncodeInitData(p);
			return p;
		}

		private COutPacket AffectedAreaRemoved(CAffectedArea area)
		{
			var p = new COutPacket(SendOps.LP_AffectedAreaRemoved);
			p.Encode4(area.dwId);
			return p;
		}

		public override COutPacket MakeEnterFieldPacket() => AffectedAreaCreated(this);
		public override COutPacket MakeLeaveFieldPacket() => AffectedAreaRemoved(this);
	}
}
