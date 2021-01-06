using Rebirth.Characters.Skill;
using System.Collections.Generic;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Tools;

namespace Rebirth.Server.Center.MigrationStorage
{
    public class BuffStorage : NumericKeyedCollection<CharacterBuffs>
    {
        /// <summary>
        /// Gets all buffs associated with a given character ID. Removes the entry after retrieval.
        /// </summary>
        /// <param name="dwCharId"></param>
        /// <returns></returns>
        public List<AbstractBuff> GetBuffsByCharId(int dwCharId)
        {
            var retVal = new List<AbstractBuff>();

            if (!Contains(dwCharId))
                return retVal;

            retVal.AddRange(this[dwCharId]);

            Remove(dwCharId);

            return retVal;
        }
        protected override int GetKeyForItem(CharacterBuffs item) => item.dwParentID;
    }
}
