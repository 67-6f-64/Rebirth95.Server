using System.Collections.Generic;
using Rebirth.Common.GameLogic;
using Rebirth.Server.Center.Template.DropInfo;

namespace Rebirth.Server.Center.GameData.DropInfo
{
	public sealed class GlobalDropGenerator
	{
		public static List<GlobalDropStruct> GlobalDropData { get; set; } = new List<GlobalDropStruct>();

		private static readonly double BoxDropRate = 0.001;

		private static readonly int[] BowmanBoxes = {
			2022582,   // King Pepe Bowman Box
            2022655 }; // Dragon Rider's Bowman Box
		private static readonly int[] WarriorBoxes = {
			2022652,   // Dragon Rider's Warrior Box
            2022575,   // King Pepe Warrior Armor Box
            2022570,   // King Pepe Warrior Weapon Box
                    };
		private static readonly int[] MagicianBoxes = {
			2022581,   // King Pepe Magician Box
            2022653 }; // Dragon Rider's Magician Box
		private static readonly int[] ThiefBoxes = {
			2022583,   // King Pepe Thief Box
            2022654 }; // Dragon Rider's Thief Box
		private static readonly int[] PirateBoxes = {
			2022584,   // King Pepe Pirate Box
            2022656 }; // Dragon Rider's Pirate Box
		private static readonly int[] AranBoxes = {
			 };
		private static readonly int[] EvanBoxes = {
			 };

		private static readonly int[] OneIn50 = { };
		private static readonly int[] OneIn100 = { 5062000,     // Miracle cube
                                                   2030000      // Return Scroll - Nearest Town
                                               };
		private static readonly int[] OneIn200 = { 2022336,     // Secret box
                                                   02049301,    // Equip Enhancement Scroll
                                                   02049401,    // Potential Scroll
                                               };
		private static readonly int[] OneIn400 = { 02049300,     // Advanced Equip Enhancement Scroll
                                                   02049400,     // Advanced Potential Scroll
                                               };
		private static readonly int[] OneIn500__Level30 =
			{
			};

		private static readonly int[] OneIn5000 =
			{
			01082002, // Work Gloves
			01102053, // Old Raggedy Cape
			05200001, // Silver Sack of Mesos (5M)
			};

		private static readonly int[] OneIn10_000 =
			{
			01082145, // Yellow Work Gloves
			01082146, // Red Work Gloves
			01082147, // Blue Work Gloves
			01082148, // Purple Work Gloves
			01082149, // Brown Work Gloves
			01082150,  // Grey Work Gloves

			01102079, // Ragged Red Cape
			01102080, // Ragged Blue Cape
			01102081, // Ragged Yellow Cape
			01102082, // Ragged Black Cape
			01102083, // Ragged Green Cape

			05202000, // Rare Meso Sack
			};

		public static void Load()
		{
			CreateItem(BowmanBoxes, BoxDropRate, JobLogic.JobType.Bowman, 7, 30);
			CreateItem(WarriorBoxes, BoxDropRate, JobLogic.JobType.Warrior, 7, 30);
			CreateItem(MagicianBoxes, BoxDropRate, JobLogic.JobType.Magician, 7, 30);
			CreateItem(ThiefBoxes, BoxDropRate, JobLogic.JobType.Thief, 7, 30); // includes dualblade also
			CreateItem(PirateBoxes, BoxDropRate, JobLogic.JobType.Pirate, 7, 30);
			CreateItem(WarriorBoxes, BoxDropRate, JobLogic.JobType.Aran, 7, 30);
			CreateItem(MagicianBoxes, BoxDropRate, JobLogic.JobType.Evan, 7, 30);

			CreateItem(OneIn100, 0.01, JobLogic.JobType.All, 5, 30);
			CreateItem(OneIn200, 0.005, JobLogic.JobType.All, 5, 30);
			CreateItem(OneIn400, 0.0025, JobLogic.JobType.All, 5, 70);
			CreateItem(OneIn500__Level30, 0.002, JobLogic.JobType.All, 5, 30);
			CreateItem(OneIn5000, 0.0002, JobLogic.JobType.All, 5, 30);
			CreateItem(OneIn10_000, 0.0001, JobLogic.JobType.All, 5, 30);

			CreateItem(02460003, 0.002, JobLogic.JobType.All, 5, 120); // Magnifying Glass (Premium)

			// below used for mastery book crafting
			CreateItem(4031049, 0.0012, JobLogic.JobType.All, 30, 90); // A Piece of an Ancient Scroll
			CreateItem(4001028, 0.0003, JobLogic.JobType.All, 30, 90); // Scroll of Wisdom
			CreateItem(4031019, 0.0001, JobLogic.JobType.All, 30, 120); // Scroll of Secrets

			CreateItem(4032342, 0.001, JobLogic.JobType.All, 30, 70, 21763);
		}

		private static void CreateItem(int[] itemIds, double rate, JobLogic.JobType job, byte maxLevelDif, int minMobLevel)
			=> itemIds.ForEach(i => CreateItem(i, rate, job, maxLevelDif, minMobLevel));

		public static void CreateItem(int itemId, double rate, JobLogic.JobType job, byte maxLevelDif, int minMobLevel, int questid = 0)
			=> GlobalDropData.Add(
				new GlobalDropStruct
				{
					ItemID = itemId,
					DropRate = rate,
					MaxLevelDiff = maxLevelDif,
					Job = job,
					MinMobLevel = minMobLevel,
					RequiredQuestID = questid,
				});
	}
}
