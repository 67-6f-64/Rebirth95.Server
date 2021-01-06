using System.Collections.ObjectModel;

namespace Rebirth
{
	// TODO: Recode this so it mimics behaviour of KeyedCollection but doesn't inherit
	// Ran into an issue where key was long and overriding the indexer didnt work out in our favour :(
	public abstract class NumericKeyedCollection<TItem> : KeyedCollection<int, TItem>
	{
		public new TItem this[int key] => GetOrDefault(key);

		public TItem GetOrDefault(int key)
		{
			if (TryGetValue(key, out var ret))
				return ret;
			return default;
		}

		public TItem GetAtIndex(int index)
		{
			return Items[index];
		}
	}
}
