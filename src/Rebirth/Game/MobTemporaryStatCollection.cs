using System;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;

namespace Rebirth.Game
{
	public class MobTemporaryStatCollection : NumericKeyedCollection<MobStatEntry>
	{
		public CMob Parent { get; set; }

		public MobTemporaryStatCollection(CMob parent)
		{
			Parent = parent;
		}

		public MobStatEntry this[MobStatType nType]
			=> Contains((int)nType)
				? this[(int)nType]
				: null;

		public void RegisterMobStats(int dwCharID, int nDelaySec, params MobStatEntry[] toAdd)
		{
			if (toAdd.Length <= 0) return;

			if (nDelaySec > short.MaxValue) throw new OverflowException();

			toAdd.ForEach(entry =>
			{
				Remove(entry.MobStatTypeID);
				entry.CharIdFrom = dwCharID;
				entry.nDelay = nDelaySec;
				Add(entry);
			});

			Parent.Field.Broadcast(StatSet(nDelaySec, toAdd));

			Parent.Stats.SetFrom(Parent);

			if (toAdd.Any(item => item.MobStatTypeID == (int) MobStatType.DamagedElemAttr))
			{
				Parent.Stats.AdjustDamagedElemAttr(Parent.Stats.rDamagedElemAttr, Parent.Template.DamagedElemAttr);
			}
		}

		public void RemoveMobStats(params MobStatEntry[] toRemove)
		{
			if (toRemove.Length <= 0) return;

			toRemove.ForEach(entry => Remove(entry));

			Parent.Field.Broadcast(StatReset(toRemove));

			Parent.Stats.SetFrom(Parent);

			if (toRemove.Any(item => item.MobStatTypeID == (int)MobStatType.DamagedElemAttr))
			{
				Parent.Stats.AdjustDamagedElemAttr(Parent.Stats.rDamagedElemAttr, Parent.Template.DamagedElemAttr);
			}
		}

		public void EncodeCollection(COutPacket p)
		{
			EncodeMask(p, this.ToArray());
			EncodeTemporaryStat(Parent.Stats, p, this.ToArray());
		}

		private COutPacket StatSet(int tDelay, params MobStatEntry[] toAdd)
		{
			var p = new COutPacket(SendOps.LP_MobStatSet);
			p.Encode4(Parent.dwId);

			var movementAffectingStat = EncodeMask(p, toAdd);

			EncodeTemporaryStat(Parent.Stats, p, toAdd);

			p.Encode2((short)tDelay);
			p.Encode1(0); // m_nCalcDamageStatIndex

			if (movementAffectingStat)
			{
				p.Encode1(false);
			}

			return p;
		}

		private COutPacket StatReset(params MobStatEntry[] toRemove)
		{
			var p = new COutPacket(SendOps.LP_MobStatReset);
			p.Encode4(Parent.dwId);

			var movementAffectingStat = EncodeMask(p, toRemove);

			// order matters
			var sorted = toRemove
				.ToList()
				.OrderBy(item => item.MobStatTypeID);

			foreach (var entry in sorted)
			{
				if (entry.MobStatTypeID == (int)MobStatType.Burned)
				{
					p.Encode4(0); //size
								  // for each size
								  // p.Encode4(charId[i])
								  // p.Encode4(skillId[i])
				}
			}

			p.Encode1(0); // m_nCalcDamageStatIndex

			if (movementAffectingStat)
			{
				p.Encode1(false);
			}

			return p;
		}

		private static bool EncodeMask(COutPacket p, params MobStatEntry[] toAdd)
		{
			var mask = new int[4];
			var movementAffectingStat = false;
			toAdd.ForEach(entry =>
			{
				movementAffectingStat = movementAffectingStat || entry.MovementAffectingStat;
				mask[entry.Set] |= entry.Mask;

			});

			for (var i = 3; i >= 0; i--)
			{
				p.Encode4(mask[i]);
			}

			return movementAffectingStat;
		}

		private static void EncodeTemporaryStat(MobStat ms, COutPacket p, params MobStatEntry[] toAdd)
		{
			var pCounter = -1;
			var mCounter = -1;

			toAdd.OrderBy(item => item.MobStatTypeID) // order matters
				.ForEach(entry =>
				{
					entry.Encode(p);

					switch (entry.MobStatTypeID)
					{
						case (int)MobStatType.PCounter:
							pCounter = entry.nOption;
							break;
						case (int)MobStatType.MCounter:
							mCounter = entry.nOption;
							break;
					}
				});

			if (pCounter != -1)
			{
				p.Encode4(pCounter);
				ms.wPCounter = pCounter;
			}

			if (mCounter != -1)
			{
				p.Encode4(mCounter);
				ms.wMCounter = mCounter;
			}

			if (pCounter != -1 || mCounter != -1)
			{
				p.Encode4(ms.nCounterProb);
			}
		}

		protected override void InsertItem(int index, MobStatEntry item)
		{
			if (item is null) return;

			item.tStartTime = DateTime.Now.AddMilliseconds(item.nDelay);
			base.InsertItem(index, item);
		}

		protected override int GetKeyForItem(MobStatEntry item) => item.MobStatTypeID;
	}
}
