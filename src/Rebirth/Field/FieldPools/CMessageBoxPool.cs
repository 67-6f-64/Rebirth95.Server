using Rebirth.Field.FieldObjects;
using System;
using System.Collections.Generic;

namespace Rebirth.Field.FieldPools
{
	public class CMessageBoxPool : CObjectPool<CMessageBox>
	{
		public CMessageBoxPool(CField parent)
			: base(parent) { }

		public void Update()
		{
			var toRemove = new List<CMessageBox>();

			foreach (var item in this)
			{
				var diff = DateTime.Now - item.StartTime;

				if (diff.TotalMilliseconds >= item.Duration)
				{
					toRemove.Add(item);
				}
			}

			foreach (var item in toRemove)
			{
				Remove(item);
			}
		}

		protected override void InsertItem(int index, CMessageBox item)
		{
			item.StartTime = DateTime.Now;

			base.InsertItem(index, item);

			Field.Broadcast(item.MakeEnterFieldPacket());
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			if (item != null)
			{
				Field.Broadcast(item.MakeLeaveFieldPacket());
			}

			base.RemoveItem(index);
		}
	}
}
