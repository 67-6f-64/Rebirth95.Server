using Rebirth.Field.FieldObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Field
{
	public class CAffectedAreaPool : CObjectPool<CAffectedArea>
    {
        public CAffectedAreaPool(CField parent) : base(parent) { }

        public void Update()
        {
			this.ToList().RemoveAll(area => area.Update());

			var toRemove = new List<CAffectedArea>();
			foreach (var area in this)
			{
				if (area.Update()) toRemove.Add(area);
			}
			toRemove.ForEach(area => Remove(area));
		}

        protected override void InsertItem(int index, CAffectedArea item)
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
