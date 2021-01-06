using System;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth
{
	public static class Extensions
	{
		public static readonly Random Rand = new Random();

		public static bool NotNull(this int val)
		{
			return val != 0;
		}

		public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (T current in collection)
			{
				action(current);
			}
		}
		public static void ForEach<T>(this T[] collection, Action<T> action)
		{
			foreach (T current in collection)
			{
				action(current);
			}
		}

		public static T Random<T>(this IEnumerable<T> collection)
		{
			var enumerable = collection as T[] ?? collection.ToArray();

			if (enumerable.Length <= 0) return default;

			var rand = Rand.Next(enumerable.Length);
			return enumerable.ElementAt(rand);
		}

		public static T Random<T>(this T[] collection)
		{
			if (collection.Length <= 0) return default;

			var rand = Rand.Next(collection.Length);
			return collection[rand];
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			var buffer = source.ToArray();

			for (int i = 0; i < buffer.Length; i++)
			{
				int j = Rand.Next(i, buffer.Length);

				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}

		public static bool EqualsIgnoreCase(this string input, string comperand)
		{
			return string.Compare(input, comperand, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static bool InRange(this int value, int minimum, int maximum, bool inclusiveMax = true)
		{
			return value >= minimum && (inclusiveMax ? value <= maximum : value < maximum);
		}

		public static int GetStableHashCode(this string str)
		{
			var hash1 = 352654597;
			var hash2 = hash1;
			var index = 0;

			while (index < str.Length)
			{
				hash1 = (hash1 << 5) + hash1 ^ str[index];

				if (index != str.Length - 1)
				{
					hash2 = (hash2 << 5) + hash2 ^ str[index + 1];
					index += 2;
				}
				else
				{
					break;
				}
			}

			return hash1 + hash2 * 1566083941;
		}
	}
}
