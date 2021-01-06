using log4net;
using Rebirth.Characters.Inventory;
using System.Collections.Generic;
using System.Linq;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Server.Center
{
    public class TempInvManager
    {
        public static ILog Log = LogManager.GetLogger(typeof(TempInvManager));

        private readonly Dictionary<int, List<TempInventory>> stack = new Dictionary<int, List<TempInventory>>(); // searchable by char id

        /// <summary>
        /// This function will return a temp inventory of defined type, if none exists, it will create it.
        /// Searchable by character ID.
        /// </summary>
        public TempInventory Get(MR_Type type, int dwCharId)
        {
            var list = stack[dwCharId] ?? new List<TempInventory>();

            return list.FirstOrDefault(ctx => ctx.Type == type) ?? new TempInventory(type, 9, dwCharId);
        }

        public void Add(int dwCharId, TempInventory toAdd)
        {
            if (!stack.ContainsKey(dwCharId))
            {
                stack.Add(dwCharId, new List<TempInventory>());
            }

            var list = stack[dwCharId];

            foreach (var item in list)
            {
                if (item.Type == toAdd.Type)
                {
                    list.Remove(item); // this may be dangerous..
                    return;
                }
            }

            list.Add(toAdd);
        }

        public void Initialize()
        {

        }

        public void SaveAll()
        {
            foreach (var list in stack.Values)
            {
                foreach (var inv in list)
                {
                    inv.SaveToDB();
                }
            }
        }
    }
}
