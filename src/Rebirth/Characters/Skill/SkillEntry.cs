using Rebirth.Network;
using Rebirth.Provider.Template.Skill;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;

namespace Rebirth.Characters.Skill
{
	public class SkillEntry //GW_SkillRecord ?
	{
		public int nSkillID { get; set; }

		public SkillTemplate Template => MasterManager.SkillTemplates[nSkillID];
		public byte MaxLevel => (byte)Template.MaxLevel;
		public double X_Effect => Template.X(nSLV);
		public double Y_Effect => Template.Y(nSLV);
		public int MPCost => Template.MpCon(nSLV);
		public int BuffTime => Template.Time(nSLV);
		public int BuffTimeMillis => Template.Time(nSLV) * 1000;
		public int CoolTimeSeconds => Template.Cooltime(nSLV);
		public bool DoProp()
		{
			if (Template.Prop(nslv) <= 0 || Template.Prop(nSLV) >= 100) return true;
			return Constants.Rand.Next(100) < Template.Prop(nSLV);
		}

		public int CombatOrders { get; set; }

		private int nslv;
		public byte nSLV
		{
			get => (byte)(nslv + CombatOrders);
			set => nslv = value;
		}

		public byte CurMastery { get; set; }
		public long Expiry { get; set; } = Constants.PERMANENT; //Confirm this is really expiry lol

		public SkillEntry(int nSkillID)
		{
			this.nSkillID = nSkillID;
		}

		public void Encode(COutPacket p, bool bForSetField = false)
		{
			p.Encode4(nSkillID);
			p.Encode4(nslv); // we dont send combat orders, just the raw level

			if (!bForSetField)
				p.Encode4(CurMastery);

			p.Encode8(Expiry);

			if (bForSetField && Template.is_skill_need_master_level)
				p.Encode4(CurMastery);
		}
	}
}