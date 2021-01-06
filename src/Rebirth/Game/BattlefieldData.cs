using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Game
{
	public static class BattlefieldData
	{
		public enum BattlefieldTeams : byte
		{
			None = 0xFF,
			Sheep = 0,
			Wolves = 1,
			SheepNaked = 2,
			Num = 3,
		}

		public enum BattleItems
		{
			/// <summary>
			/// Summons Wolf Bomb
			/// </summary>
			StealSheepWool = 02109004,
			/// <summary>
			/// Summons Rose Thorns
			/// </summary>
			PlantRoseThorn = 02109005,
			/// <summary>
			/// Dropped by sheep. Reduces score by 1 if a wolf picks it up.
			/// Can be traded for rewards.
			/// </summary>
			FineWool = 04001263,
			/// <summary>
			/// Dropped by wolfs. Reduces score by 1 if a sheep picks it up.
			/// Can be traded for rewards.
			/// </summary>
			ShepherdBoysLunch = 04001264,
			/// <summary>
			/// Movement speed increased for 3 seconds.
			/// +40 Speed
			/// </summary>
			DangerEscape = 02022540,
			/// <summary>
			/// Protects from a Wolf's attack 1 time.
			/// Activates the bubble shield from Mu Lung Dojo (Prevents all damage)
			/// </summary>
			SelfProtection = 02022541,
			/// <summary>
			/// Wolves slow down when they hear a lamb cry.
			/// BFSkill = 0
			/// </summary>
			CryOfLamb = 02022539,
			/// <summary>
			/// Attacks a Wolf's back, temporarily immobolizing it.
			/// BFSkill = 1
			/// </summary>
			LambSurpriseAttack = 02022542,
			/// <summary>
			/// Causes the sheep to become confused and lose direction.
			/// BFSkill = 2
			/// </summary>
			GreatConfusion = 02022543,
			/// <summary>
			/// Slows wolves' movement speed.
			/// BFSkill = 3
			/// </summary>
			SoundOfSheepBells = 02022547,
			/// <summary>
			/// Sheep are temporarily unable to move.
			/// BFSkill = 4
			/// </summary>
			WoundOfWolfBells = 02022548,
			/// <summary>
			/// Intimidates the sheep, making them weaker.
			/// BFSkill = 5
			/// </summary>
			WolfThreat = 02022549
		}

		public static bool SheepConsumable(int nItemID) => nItemID > 2022543;
		public static bool WolfConsumable(int nItemID) => nItemID <= 2022543;

		/// <summary>
		/// First map that players enter when they warp to the event
		/// </summary>
		public const int RanchEntranceMap = 910040000;
		/// <summary>
		/// Map that players get warped to if they DC during the battle
		/// </summary>
		public const int RanchForcedReturn = 910040001;
		/// <summary>
		/// Waiting room for the event to start
		/// </summary>
		public const int RanchWaitingRoom = 910040002;

		public const int WolfVictoryMap = 910040003;
		public const int SheepVictoryMap = 910040004;
		public const int WolfLoseMap = 910040005;
		public const int SheepLoseMap = 910040006;

		public const int BattleMap = 910040100;

		public const int SheepBattleMap = 910040110;
		public const int WolfBattleMap = 910040120;
	}
}
