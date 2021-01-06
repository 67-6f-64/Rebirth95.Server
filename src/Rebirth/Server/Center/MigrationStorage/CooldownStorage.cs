using Rebirth.Characters.Skill;
using System.Collections.Generic;
using Rebirth.Tools;

namespace Rebirth.Server.Center.MigrationStorage
{
    public class CooldownStorage : NumericKeyedCollection<CharacterCooldowns>
    {
        /// <summary>
        /// Gets all cooldowns associated with a given character ID. Removes the entry after retrieval.
        /// </summary>
        /// <param name="dwCharId"></param>
        /// <returns></returns>
        public List<Cooldown> GetCooldownsByCharId(int dwCharId)
        {
            var retVal = new List<Cooldown>();

            if (!Contains(dwCharId))
                return retVal;

            retVal.AddRange(this[dwCharId]);

            Remove(dwCharId);

            return retVal;
        }

        protected override int GetKeyForItem(CharacterCooldowns item) => item.dwParentID;
    }
}
