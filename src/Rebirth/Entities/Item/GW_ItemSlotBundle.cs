using log4net;
using Npgsql;
using Rebirth.Network;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Item;

namespace Rebirth.Entities.Item
{
	public class GW_ItemSlotBundle : GW_ItemSlotBase
	{
		public short nAttribute = 0x2;
		public string sTitle = string.Empty;

		public override string DbInsertString(int dwCharId, short nPOS)
			=> $"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_inventory_items "
				+ $"(character_id, item_id, inventory_slot, "
				+ $"item_quantity, item_date_expire, item_serial_number, "
				+ $"item_cash_serial_number) "
				+ $"VALUES ({dwCharId}, {nItemID}, {nPOS}, "
				+ $"{nNumber}, '{tDateExpire.ToSqlTimeStamp()}', {liSN}, "
				+ $"{liCashItemSN});";

		public GW_ItemSlotBundle(int nItemID) : base(nItemID) { }

		public override GW_ItemSlotBase DeepCopy()
		{
			var pItem = (GW_ItemSlotBundle)base.DeepCopy();
			pItem.sTitle = sTitle;
			pItem.nAttribute = nAttribute;
			return pItem;
		}

		public override void RawEncode(COutPacket p, bool bFromCS = false)
		{
			p.Encode1(2);

			base.RawEncode(p, bFromCS);

			p.Encode2(nNumber);
			p.EncodeString(sTitle);
			p.Encode2(nAttribute);

			if (ItemConstants.IsRechargeableItem(nItemID))
				p.Encode8(liSN);
		}

		/// <summary>
		/// ONLY CALL THIS WHEN INSERTING ITEMS FROM CHAR CREATION
		/// </summary>
		public void SaveToDB(int dwCharId, short nPOS)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(DbInsertString(dwCharId, nPOS), conn))
					cmd.ExecuteNonQuery();
			}
		}
	}
}