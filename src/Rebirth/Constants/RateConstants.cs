using System;

namespace Rebirth
{
    public static class RateConstants
    {
        public static readonly int[] GlobalDrops = 
            { };
		
		// rate multipliers
        public const byte ExpRate = 15;
        public const byte MesoRate = 5;
		public const int DropRate = 1;

		public const int RandomNX_Odds = 25; // percent
        public const int RandomNX_Mean = 36; // decimal
        public const int RandomNX_Variance = 12; // decimal
        
        public const  int BaseMinMesoAmount = 6; // per level
        public const  int BaseMaxMesoAmount = 9; // per level

        public const int DojoNXGainPerStage = 4;

		/// <summary>
		/// Odds to get 3 lines on first reveal
		/// </summary>
		public const float ItemPotential_ThirdLineFirstReveal = 0.05f;
		/// <summary>
		/// Odds to get third line after first reveal
		/// </summary>
		public const float ItemPotential_ThirdLineOdds = 0.1f;

		public const float ItemPotential_EpicOdds = 0.15f;
		public const float ItemPotential_UniqueOdds = 0.5f;

		public const float ItemPotential_EquipDropHiddenPotentialOdds = 0.15f;
    }
}
