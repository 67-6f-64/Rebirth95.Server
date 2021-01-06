using Rebirth.Entities;
using Rebirth.Network;

namespace Rebirth.Game
{
	public class AvatarMegaphone
	{
		public int nCharId { get; set; } // for internal purposes
		public int nInitialQueuePosition { get; set; } // for internal purposes

		public int nItemID { get; set; }
		public string sName { get; set; }
		public string[] sMsgs { get; set; }
		public int nChannelNumber { get; set; }
		public bool bWhisper { get; set; }

		public AvatarLook alSender { get; set; }

		public void Encode(COutPacket p)
		{
			p.Encode4(nItemID);

			p.EncodeString(sName);

			for (int i = 0; i < 4; i++) // 4 strings always ?
			{
				if (sMsgs.Length <= i)
				{
					p.EncodeString("");
				}
				else
				{
					p.EncodeString(sMsgs[i]);
				}
			}

			p.Encode4(nChannelNumber);
			p.Encode1(bWhisper);

			alSender.Encode(p);
		}
	}
}
