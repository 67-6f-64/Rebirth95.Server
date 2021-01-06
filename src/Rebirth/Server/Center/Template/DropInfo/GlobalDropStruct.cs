using Rebirth.Common.GameLogic;

namespace Rebirth.Server.Center.Template.DropInfo
{
	/// <summary>
	/// Represents a drop that occurs for all monsters
	/// </summary>
	public struct GlobalDropStruct
	{
		public int ItemID;
		public double DropRate;
		public byte MaxLevelDiff;
		public JobLogic.JobType Job;
		public int MinMobLevel;
		public int RequiredQuestID;
	}
}
