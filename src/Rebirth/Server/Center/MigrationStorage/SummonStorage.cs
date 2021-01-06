using Rebirth.Field.FieldObjects;
using System.Collections.Generic;

namespace Rebirth.Server.Center.MigrationStorage
{
	public class SummonStorage
	{
		private Dictionary<int, List<CSummon>> collection;

		public SummonStorage()
		{
			collection = new Dictionary<int, List<CSummon>>();
		}

		public List<CSummon> Retrieve(int dwCharID)
		{
			if (collection.ContainsKey(dwCharID))
			{
				var items = collection[dwCharID];
				collection.Remove(dwCharID);
				return items;
			}
			else
			{
				return new List<CSummon>();
			}
		}

		public void Insert(int dwCharID, List<CSummon> items)
		{
			collection.Remove(dwCharID); // TODO think about this
			collection.Add(dwCharID, items);
		}
	}
}
