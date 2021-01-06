using Rebirth.Characters;
using Rebirth.Field;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using Rebirth.Provider.Template.Item.Cash;

namespace Rebirth.Entities
{
    public class BlowWeather
    {
        public readonly int Duration = 30;

        public CField Field { get; private set; }

        public DateTime StartTime { get; private set; }
        public int nItemID { get; private set; }
        public int StateChangeItemID { get; private set; }
        public byte nBlowType { get; private set; } // todo
        public string sMsg { get; private set; }

        public BlowWeather(CField parent)
        {
            Field = parent;
            StartTime = DateTime.Now.AddMinutes(-3);
        }

        public void UpdateItemInfo(int itemId, string msg, int blowType)
        {
            nBlowType = (byte)blowType;
            nItemID = itemId;
            sMsg = msg;

            Field.Broadcast(StartWeather());

            if (MasterManager.ItemTemplate(nItemID) is CashItemTemplate i)
            {
                if (i.StateChangeItem > 0)
                {
                    foreach (var user in Field.Users)
                    {
                        user.Buffs.AddItemBuff(i.StateChangeItem);
                    }
                }
            }

            StartTime = DateTime.Now;
        }

        public void SendWeatherEffect(Character c)
        {
            if (MasterManager.ItemTemplate(nItemID) is CashItemTemplate i)
            {
                if (i.StateChangeItem > 0)
                {
                    c.Buffs.AddItemBuff(i.StateChangeItem);
                }
                c.SendPacket(StartWeather());
            }
        }

        public COutPacket StartWeather()
        {
            var p = new COutPacket(SendOps.LP_BlowWeather);
            p.Encode1(0); // it removes the msg if this is more than zero o__o
            p.Encode4(nItemID);
            p.EncodeString(sMsg);
            return p;
        }

        public COutPacket EndWeather()
        {
            var p = new COutPacket(SendOps.LP_BlowWeather);
            p.Encode1(0);
            p.Encode4(0);
            return p;
        }

        public void Clear()
        {
            Field.Broadcast(EndWeather());
			nItemID = 0;
		}

		public void Dispose()
		{
			Field = null;
		}
	}
}