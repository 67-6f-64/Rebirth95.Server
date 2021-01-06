using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using System;
using System.Collections.Generic;

namespace Rebirth.Field.FieldPools
{
	public class COpenGatePool : CObjectPool<COpenGate>
	{
		public bool bFirst { get; }

		public COpenGatePool(CField parent, bool first) : base(parent)
		{
			bFirst = first;
		}

		public void Update()
		{
			var toRemove = new List<COpenGate>();

			foreach (var item in this)
			{
				var diff = DateTime.Now - item.StartTime;
				if (diff.TotalMilliseconds >= 300000) // 5 min
				{
					toRemove.Add(item);
				}
			}

			foreach (var item in toRemove)
			{
				Remove(item);
			}
		}

		public void OnOpenGate(Character c, int dwOwnerID)
		{
			if (!Contains(dwOwnerID)) return;

			var cGate = this[dwOwnerID];

			if (c.dwId != dwOwnerID && cGate.nPartyID != c.Party?.PartyID) return;

			c.SendPacket(OpenGate(cGate.Position.X, cGate.Position.Y));
		}

		protected override void InsertItem(int index, COpenGate item)
		{
			item.bFirst = bFirst;

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

		private COutPacket OpenGate(short ptX, short ptY)
		{
			var p = new COutPacket(SendOps.LP_OpenGate);
			p.Encode2(ptX);
			p.Encode2(ptY);
			return p;
		}

		//Must set dwCharacterID before inserting!!
		protected override int GetUniqueId(COpenGate item) => item.dwCharacterID;
		protected override int GetKeyForItem(COpenGate item) => item.dwCharacterID;
	}
}
