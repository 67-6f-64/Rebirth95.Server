using System;

namespace Rebirth.Network.Crypto
{
    public static class Shanda
    {
        public static void EncryptTransform(ref Span<byte> data)
        {
            for (int passes = 0; passes < 3; passes++)
            {
                byte xorKey = 0;
                byte len = (byte)(data.Length & 0xFF);

                for (int i = 0; i < data.Length; i++)
                {
                    byte cur = (byte)((RollLeft(data[i], 3) + len) ^ xorKey);
                    xorKey = cur;
                    cur = (byte)(((~RollRight(cur, len & 0xFF)) & 0xFF) + 0x48);
                    data[i] = cur;
                    len--;
                }

                xorKey = 0;
                len = (byte)(data.Length & 0xFF);

                for (int i = data.Length - 1; i >= 0; i--)
                {
                    byte cur = (byte)(xorKey ^ (len + RollLeft(data[i], 4)));
                    xorKey = cur;
                    cur = RollRight((byte)(cur ^ 0x13), 3);
                    data[i] = cur;
                    len--;
                }
            }
        }
        public static void DecryptTransform(ref Span<byte> data)
        {
            for (int passes = 0; passes < 3; passes++)
            {
                byte xorKey = 0;
                byte save = 0;
                byte len = (byte)(data.Length & 0xFF);

                for (int i = data.Length - 1; i >= 0; --i)
                {
                    byte temp = (byte)(RollLeft(data[i], 3) ^ 0x13);
                    save = temp;
                    temp = RollRight((byte)((xorKey ^ temp) - len), 4);
                    xorKey = save;
                    data[i] = temp;
                    --len;
                }

                xorKey = 0;
                len = (byte)(data.Length & 0xFF);

                for (int i = 0; i < data.Length; ++i)
                {
                    byte temp = RollLeft((byte)(~(data[i] - 0x48)), len & 0xFF);
                    save = temp;
                    temp = RollRight((byte)((xorKey ^ temp) - len), 3);
                    xorKey = save;
                    data[i] = temp;
                    --len;
                }
            }
        }

        private static byte RollLeft(byte value, int shift)
        {
            uint num = (uint)(value << (shift % 8));
            return (byte)((num & 0xff) | (num >> 8));
        }
        private static byte RollRight(byte value, int shift)
        {
            uint num = (uint)((value << 8) >> (shift % 8));
            return (byte)((num & 0xff) | (num >> 8));
        }
    }
}
