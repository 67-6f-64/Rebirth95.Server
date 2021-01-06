using Rebirth.Entities;
using Rebirth.Network;

namespace Rebirth.Game
{
    public struct MapleTVMessage
    {
		public byte nFlag { get; set; }
		public byte nMessageType { get; set; }
		public AvatarLook alSender { get; set; }

		public string sSender { get; set; }
		public string sReceiver { get; set; }
		public string[] sMsgs { get; set; }
		public int nTotalWaitTime { get; set; }

		public AvatarLook alReceiver { get; set; }

		public void Encode(COutPacket p)
        {
            p.Encode1(nFlag); // technically a bitflag but it only uses bit 1 and 2
            p.Encode1(nMessageType);

			alSender.Encode(p);

			p.EncodeString(sSender);
			p.EncodeString(sReceiver);

			for(int i = 0; i < 5; i++)
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

			if ((nFlag & 2) > 0)
			{
				alReceiver.Encode(p);
			}
        }
    }
}
