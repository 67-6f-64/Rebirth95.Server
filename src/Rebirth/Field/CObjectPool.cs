using System.Collections.Generic;
using System.Threading;
using Rebirth.Common.Tools;
using Rebirth.Tools;

namespace Rebirth.Field
{
	public abstract class CObjectPool<TValue> : NumericKeyedCollection<TValue> where TValue : CFieldObj
	{
		private static int PoolCount = 0;

		private int m_uidBase = 10000 * Interlocked.Increment(ref PoolCount);

		protected CField Field { get; private set; }

		protected CObjectPool(CField parent)
		{
			Field = parent;
		}

		public virtual void Load(int dwFieldID) { }

		public List<CFieldObj> FindObjectsInRect(TagRect rect)
		{
			var retVal = new List<CFieldObj>();
			foreach (var item in this)
			{
				var position = item.Position.Clone();
				var x = position.X;
				var y = position.Y;

				if (x >= rect.Left && y >= rect.Top
					&& x <= rect.Right && y <= rect.Bottom)
				{
					retVal.Add(item);
				}
			}
			return retVal;
		}

		protected virtual int GetUniqueId(TValue item)
		{
			return Interlocked.Increment(ref m_uidBase);
		}

		protected override void InsertItem(int index, TValue item)
		{
			if (item is null) return;

			item.dwId = GetUniqueId(item);
			item.Field = this.Field;
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			if (item is null) return;

			base.RemoveItem(index);
			item.Dispose();
			item.dwId = -1;
			item.Field = null;
		}

		public virtual void Dispose()
		{
			Clear();
			Field = null;
		}

		protected override int GetKeyForItem(TValue item) => item.dwId;
	}
}
