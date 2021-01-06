using System;
using Rebirth.Characters.Stat;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Skill.Buff
{
	public enum BuffStoreType
	{
		Normal,
		InsertOnceAndUpdateTime,
		FireAndForget
	}

	public abstract class AbstractBuff
	{
		public int dwCharFromId { get; set; }

		/// <summary>
		/// For consumable buffs the buff ID is negative.
		/// </summary>
		public int nSkillID => Math.Abs(nBuffID);
		public int nBuffID { get; private set; }
		/// <summary>
		/// Used for twostate homing beacon (guided bullet)
		/// </summary>
		public int dwMobId { get; set; }

		public SecondaryStat Stat { get; } = new SecondaryStat();

		public SecondaryStatFlag StatType { get; set; }
		public BuffStoreType BuffStoreType { get; set; } = BuffStoreType.Normal;

		public int State { get; set; } = 1; // used for certain skills that need to track event states
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Buff duration in milliseconds.
		/// </summary>
		public int tDuration { get; set; }
		public byte nSLV { get; set; }

		public bool IsRateModifier { get; set; }

		//public bool ExpRateModifier { get; set; }
		//public bool ItemRateModifier { get; set; }
		//public bool MesoRateModifier { get; set; }
		//public int RateModifierAmount { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nBuffID">Negative value means it's a consumable.</param>
		/// <param name="nSLV">Leave blank (1) for consumable items.</param>
		protected AbstractBuff(int nBuffID, byte nSLV = 1)
		{
			StatType = SecondaryStatFlag.None_DONT_USE;
			this.nBuffID = nBuffID;
			this.nSLV = nSLV;
		}

		public abstract void Generate(double dBufftimeModifier = 1.0);

		protected virtual void AddSecondaryStat(SecondaryStatFlag type, int nValue, int tValue)
		{
			var entry = new SecondaryStatEntry()
			{
				nValue = nValue,
				rValue = nBuffID,
				tValue = tValue,
			};

			Stat.Add(type, entry);
		}

		protected void AddSecondaryStat(SecondaryStatFlag type, int nValue)
		{
			AddSecondaryStat(type, nValue, tDuration);
		}

		protected bool AddSecondaryStat(SecondaryStatFlag type, int nValue, Predicate<int> predicate, bool assignStatType = false)
		{
			if (predicate.Invoke(nValue)) // man i love these things so much
			{
				AddSecondaryStat(type, nValue);

				if (assignStatType)
				{
					StatType = type;
				}
				return true;
			}
			return false;
		}
	}
}
