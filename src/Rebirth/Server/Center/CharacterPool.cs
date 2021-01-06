using Autofac;
using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Server.Center
{
    public class CharacterPool
    {
        private readonly Dictionary<int, Character> m_pool;

        public CharacterPool()
        {
			m_pool = new Dictionary<int, Character>(256);
        }

        /// <summary>
        /// TODO: Refactor + Redo
        /// </summary>
        /// <param name="dwCharId"></param>
        /// <returns></returns>
        public bool CharInCashShop(int dwCharId)
        {
            return ServerApp.Container.Resolve<CenterStorage>().IsCharacterCSITC(dwCharId);
        }
        
        public void Add(Character item)
        {
            m_pool.Add(item.dwId, item);
        }

        public bool Remove(Character item)
        {
			var ret = m_pool.Remove(item.dwId);

			item?.Dispose();

            return ret;
        }

        public bool Contains(int dwCharId)
        {
            return m_pool.ContainsKey(dwCharId);
        }

        public Character Get(int dwCharId, bool bReturnIfInCS = true)
        {
            if (!bReturnIfInCS && CharInCashShop(dwCharId))
                return null;

			return m_pool.GetValueOrDefault(dwCharId);
        }

        public Character Get(string sCharacterName, bool bReturnIfInCS = true)
        {
            var retVal = m_pool.FirstOrDefault(c
	            => c.Value?.Stats.sCharacterName.ToLowerInvariant().Equals(sCharacterName.ToLowerInvariant()) ?? false)
	            .Value;

            if (!bReturnIfInCS && retVal != null && CharInCashShop(retVal.dwId))
                return null;

            return retVal;
        }

        public Character GetByAccountID(int dwAccountId)
        {
            return m_pool.FirstOrDefault(c => c.Value?.Account.ID == dwAccountId).Value;
        }

        public void ForEach(Action<Character> action)
        {
            foreach(var character in m_pool)
            {
                action(character.Value);
            }
        }
                
        public Character[] ToArray() => m_pool.Values.ToArray();

        /// <summary>
        /// TODO: Confirm needed - Be careful if using
        /// </summary>
        public void Clear()
        {
            ForEach(pChar => pChar.Save());
            m_pool.Clear();
        }
        
        /// <summary>
        /// Set channel to -1 in order to broadcast to all channels
        /// </summary>
        /// <param name="nChannel"></param>
        public void Broadcast(COutPacket p, int nChannel = -1)
        {
            using (p)
            {
                foreach (var item in m_pool)
                {
                    if (nChannel > -1 && item.Value.ChannelID != nChannel)
                        continue;

                    item.Value.SendPacket(p);
                }
            }
        }       
    }
}