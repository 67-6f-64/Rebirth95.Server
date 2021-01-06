using System;

namespace Rebirth.Game
{
	public class MOBGEN
	{
		//00000000 MOBGEN struc; (sizeof=0x20, align=0x4, copyof_541)
		//00000000 dwTemplateID dd ?
		//00000004 x dd ?
		//00000008 y dd ?
		//0000000C fh              dd?
		//00000010 tRegenInterval dd ?
		//00000014 tRegenAfter dd ?
		//00000018 nMobCount dd ?
		//0000001C nTeamForMCarnival dd?
		//00000020 MOBGEN ends

		public int dwTemplateID { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int FH { get; set; }
		public bool F { get; set; } // flipped
		public int tRegenInterval { get; set; }
		public DateTime tRegenAfter { get; set; }
		public int nMobCount { get; set; }
		public int nTeamForMonsterCarnival { get; set; }

		public MOBGEN()
		{
			tRegenAfter = DateTime.Now;
		}

		public void Reset()
		{
			// TODO
		}
	}
}
