using Rebirth.Common.Tools;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth.Field
{
	public static class CDropFactory
    {
        public static void CreateDropItem(CField field, TagPoint pStartPos, int dwOwnerID, GW_ItemSlotBase pItem)
        {
            var drop = new CDrop(pStartPos, dwOwnerID)
            {
                Item = pItem,
                ItemId = pItem.nItemID
            };

            drop.Position.X = drop.StartPosX;
            drop.CalculateY(field, drop.StartPosY);

            field.Drops.Add(drop);
        }

        public static void CreateDropMeso(CField field, TagPoint pStartPos, int dwOwnerID, int nAmount)
        {
            var drop = new CDrop(pStartPos, dwOwnerID)
            {
                bIsMoney = 1,
                ItemId = nAmount
            };

            drop.Position.X = drop.StartPosX;
            drop.CalculateY(field, drop.StartPosY);

            field.Drops.Add(drop);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStartPos"></param>
        /// <param name="dwOwnerID"></param>
        /// <param name="bMeso"></param>
        /// <param name="nOption">ItemID or amount of meso, depending on drop type.</param>
        /// <returns></returns>
        public static CDrop CreateDropFromMob(CMovePath pStartPos, int dwOwnerID, bool bMeso, int nOption)
        {
            var drop = new CDrop(pStartPos, dwOwnerID) 
            {
                bIsMoney = (byte)(bMeso ? 1 : 0),

                Item = bMeso ? null : MasterManager.CreateItem(nOption),
                ItemId = nOption
            };

            return drop;
        }
    }
}
