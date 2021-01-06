using Rebirth.Network;

namespace Rebirth.Game
{
	public class Macro
	{
		const int SkillLength = 3;

		public int nUID { get; set; } // DB key

		public int[] aSkill { get; set; }
		public string sName { get; set; }
		public bool bMute { get; set; }

		public Macro()
		{
			aSkill = new int[SkillLength];
		}

		public void Encode(COutPacket p)
		{
			p.EncodeString(sName);
			p.Encode1(bMute);

			for (int i = 0; i < SkillLength; i++)
				p.Encode4(aSkill[i]);
		}

		public void Decode(CInPacket p)
		{
			var name = p.DecodeString();



			if (name.Length != 0 && (name.Length < 4 || name.Length > 13)) name = "Invalid";

			sName = name;

			bMute = p.Decode1() != 0;

			for (int i = 0; i < SkillLength; i++)
			{
				aSkill[i] = p.Decode4();
			}
		}
	}
}
