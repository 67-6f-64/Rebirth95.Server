using System;
using Rebirth.Network;

namespace Rebirth
{
	public static class CPacketExt
    {
        public static void EncodeDateTime(this COutPacket packet, DateTime dt)
        {
            packet.Encode8(dt.ToFileTime());
        }

        public static void EncodeStringFixed(this COutPacket packet, string value, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (i < value.Length)
                {
                    packet.Encode1((byte)value[i]);
                }
                else
                {
                    packet.Encode1(0);
                }
            }
        }

        public static void EncodePos(this COutPacket packet, short posX, short posY)
        {
            packet.Encode2(posX);
            packet.Encode2(posY);
        }

        public static int[] DecodeIntArray(this CInPacket packet, int nCount)
        {
            var retVal = new int[nCount];

            for (var i = 0; i < nCount; i++)
            {
                retVal[i] = packet.Decode4();
            }

            return retVal;
        }
    }
}
