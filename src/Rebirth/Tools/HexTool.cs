using System;

namespace Rebirth.Tools
{
	public static class HexTool
	{


		//If you want maximum performance when doing conversion from hex to decimal number, you can use the approach with pre-populated table of hex-to-decimal values.
		//Here is the code that illustrates that idea. My performance tests showed that it can be 20%-40% faster than Convert.ToInt32(...):

		//class TableConvert
		//        {
		//            static sbyte[] unhex_table =
		//            { -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       , 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1
		//       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
		//      };
		//            public static int Convert(string hexNumber)
		//            {
		//                int decValue = unhex_table[(byte)hexNumber[0]];
		//                for (int i = 1; i < hexNumber.Length; i++)
		//                {
		//                    decValue *= 16;
		//                    decValue += unhex_table[(byte)hexNumber[i]];
		//                }
		//                return decValue;
		//            }
		//        }


		private static Random sRandom = new Random();

		private static readonly char[] sHexChars = new char[]
		{
			'A','B','C','D','E','F',
			'0','1','2','3','4','5','6','7','8','9'
		};

		public static char RandomHexChar()
		{
			return sHexChars[sRandom.Next(0, 15)];
		}

		public static char GetHexValue(int value)
		{
			return value < 10 ? (char)(value + 48) : (char)(value - 10 + 65);
		}

		public static bool IsHexChar(char hex)
		{
			return hex >= '0' && hex <= '9' || hex >= 'A' && hex <= 'F' || hex >= 'a' && hex <= 'f';
		}

		public static string ToString(byte[] value)
		{
			int arrayLen = value.Length * 3;
			int arrayPos = 0;
			char[] array = new char[arrayLen];

			for (int i = 0; i < arrayLen; i += 3)
			{
				byte b = value[arrayPos++];
				array[i] = GetHexValue(b / 16);
				array[i + 1] = GetHexValue(b % 16);
				array[i + 2] = ' ';
			}

			return new string(array, 0, array.Length - 1);
		}
		public static string ToStringAscii(byte[] value)
		{
			char[] array = new char[value.Length];

			for (int i = 0; i < value.Length; i++)
			{
				char nib = (char)value[i];
				array[i] = nib >= ' ' && nib <= '~' ? nib : '.';
			}

			return new string(array);
		}
		//public static byte[] ToBytes(string packet)
		//{
		//    StringBuilder sr = new StringBuilder();

		//    foreach (char c in packet)
		//    {
		//        if (IsHexChar(c))
		//        {
		//            sr.Append(c);
		//        }
		//        else if (c == '*')
		//        {
		//            sr.Append(RandomHexChar());
		//        }
		//    }

		//    if (sr.Length < 4)
		//        throw new FormatException("Packet length is less than 2 bytes");

		//    if (sr.Length % 2 != 0)
		//        throw new FormatException("Packet length is not a multiple of 2");

		//    return FromBinHexString(sr.ToString());
		//}

		//internal static byte[] FromBinHexString(string value)
		//{
		//    char[] chars = value.ToCharArray();
		//    byte[] buffer = new byte[chars.Length / 2];
		//    int charLength = chars.Length;

		//    int bufIndex = 0;
		//    for (int i = 0; i < charLength - 1; i += 2)
		//    {
		//        buffer[bufIndex] = FromHex(chars[i], value);
		//        buffer[bufIndex] <<= 4;
		//        buffer[bufIndex] += FromHex(chars[i + 1], value);
		//        bufIndex++;
		//    }
		//    return buffer;
		//}

		//static byte FromHex(char hexDigit, string value)
		//{
		//        return byte.Parse(hexDigit.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
		//}

		public static void NextBytes(byte[] array)
		{
			sRandom.NextBytes(array);
		}
		public static int Next(int min, int max)
		{
			return sRandom.Next(min, max);
		}
	}
}
