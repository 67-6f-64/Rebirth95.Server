using Npgsql;
using Rebirth.Characters;
using Rebirth.Network;

namespace Rebirth.Game
{
	public sealed class GuildMember
	{
		public Guild Guild { get; set; }

		public byte GuildRank { get; set; }
		public byte AllianceRank { get; set; } // 0 means they are not in an alliance
		public int GuildContribution { get; set; }
		public int GuildPoints { get; set; }

		public int dwParentID { get; set; }
		public string ParentName { get; set; }
		public short ParentJob { get; set; }
		public short ParentLevel { get; set; }

		public bool bOnline { get; set; }

		public GuildMember(int dwId, int nGuildRank, int nGuildPoints, int nContribution, int nAllianceRank)
		{
			dwParentID = dwId;
			GuildRank = (byte)nGuildRank;
			GuildPoints = nGuildPoints;
			GuildContribution = nContribution;
			AllianceRank = (byte)nAllianceRank;
		}

		public void Init(string name, short job, short level)
		{
			ParentName = name;
			ParentJob = job;
			ParentLevel = level;
		}

		public void EncodeGuildMember(COutPacket p)
		{
			p.EncodeStringFixed(ParentName, 13);
			p.Encode4(ParentJob);
			p.Encode4(ParentLevel);
			p.Encode4(GuildRank); // nGrade
			p.Encode4(bOnline ? 1 : 0);
			p.Encode4(GuildContribution); // nCommitment
			p.Encode4(AllianceRank); // nAllianceGrade
		}
	}
}
