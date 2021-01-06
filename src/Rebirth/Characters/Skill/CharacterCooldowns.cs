using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using Rebirth.Tools;

namespace Rebirth.Characters.Skill
{
    public class CharacterCooldowns : NumericKeyedCollection<Cooldown>
    {
        public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
        public int dwParentID { get; set; }

        public CharacterCooldowns(int parent)
        {
            dwParentID = parent;

            BulkInsert(MasterManager.CooldownStorage.GetCooldownsByCharId(dwParentID));
        }

        public void SaveForMigrate() => MasterManager.CooldownStorage.Add(this);

        public void Update()
        {
            foreach (var entry in new List<Cooldown>(this))
            {
                if (entry.CooldownExpired)
                {
					Remove(entry);
                }
            }
        }

        public bool OnCooldown(int nSkillID)
        {
            if (!Contains(nSkillID))
                return false;

            return this[nSkillID].CooldownExpired;
        }

        private COutPacket SkillCooltimeSet(int nSkillId, short tCooltimeSec)
        {
            var p = new COutPacket(SendOps.LP_SkillCooltimeSet);
            p.Encode4(nSkillId);
            p.Encode2(tCooltimeSec);
            return p;
        }

        private void BulkInsert(List<Cooldown> toAdd)
        {
            foreach (var item in toAdd)
            {
                Add(item);
            }
        }

		public void UpdateOrInsert(int nSkillID, int tCooldownSeconds)
		{
			if (Contains(nSkillID))
			{
				this[nSkillID].Reset(tCooldownSeconds);
				Parent.SendPacket(SkillCooltimeSet(nSkillID, (short)tCooldownSeconds));
			}
			else
			{
				Add(new Cooldown(nSkillID, tCooldownSeconds));
			}
		}

        protected override void InsertItem(int index, Cooldown item)
        {
            // parent is null when we are adding items after migrating
            if (Parent?.Initialized ?? false)
            {
                Parent.SendPacket(SkillCooltimeSet(item.nSkillID, (short)item.nCooldownSeconds));
            }

			base.InsertItem(index, item);
        }

		protected override void RemoveItem(int index)
		{
			var cd = GetAtIndex(index);

			if (cd != null)
			{
				Parent.SendPacket(SkillCooltimeSet(cd.nSkillID, 0));
			}

			base.RemoveItem(index);
		}

		protected override int GetKeyForItem(Cooldown item) => item.nSkillID;
    }
}
