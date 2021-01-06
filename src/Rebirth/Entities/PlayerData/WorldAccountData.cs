using Npgsql;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Entities.PlayerData
{
	/// <summary>
	/// Data storage class for account data that is shared between chars in a world but not accross multiple worlds
	/// </summary>
	public class WorldAccountData
	{
		public const int BaseCharSlots = 3;
		public int AccountID { get; set; }
		public int WorldID { get; set; }
		public int CharacterSlots { get; set; }
		public int Admin { get; set; }
		public int NX_Credit { get; set; }
		public int NX_Maplepoint { get; set; }
		public int NX_Prepaid { get; set; }

		public CashLocker Locker { get; set; }

		public WorldAccountData(int accountId, int worldId)
		{
			Locker = new CashLocker(96);
			AccountID = accountId;
			WorldID = worldId;
			LoadFromDB();
		}

		public void SaveToDB()
		{
			if (WorldID != 0) throw new Exception("Unhandled world ID in WorldAccountData.");

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.account_data WHERE account_id = {AccountID};");
				dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.account_data (account_id, character_slots, admin, nx_prepaid, nx_credit, nx_maplepoint)");
				dbQuery.AppendLine($"VALUES ({AccountID}, {CharacterSlots}, {Admin}, {NX_Prepaid}, {NX_Credit}, {NX_Maplepoint});");

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.account_locker_items WHERE account_id = {AccountID};");

				foreach (var item in Locker)
				{
					dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.account_locker_items");
					dbQuery.AppendLine($"(item_serial_number, account_id, item_cash_serial_number, item_quantity, item_date_expire, purchase_character, purchase_date, purchase_name)");
					dbQuery.AppendLine($"VALUES ({item.Item.liSN}, {AccountID}, {item.SN}, {item.Count}, '{item.Item.tDateExpire.ToSqlTimeStamp()}', {item.dwCharacterID}, '{DateTime.Now.ToSqlTimeStamp()}', '{item.BuyerCharName}');"); // TODO purchase time
				}

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		private void LoadFromDB()
		{
			if (WorldID != 0) throw new Exception("Unhandled world ID in WorldAccountData.");

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(
					$"SELECT * FROM {Constants.DB_All_World_Schema_Name}.account_locker_items WHERE account_id = {AccountID};",
					conn))
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						var cs_sn = Convert.ToInt64(r["item_cash_serial_number"]);
						var commodity = MasterManager.CreateCashCommodityItem(cs_sn);
						commodity.dwAccountID = AccountID;
						commodity.dwCharacterID = Convert.ToInt32(r["purchase_character"]);
						commodity.Item.tDateExpire = (DateTime)r["item_date_expire"];
						commodity.Item.nNumber = Convert.ToInt16(r["item_quantity"]);
						commodity.Item.liSN = Convert.ToInt64(r["item_serial_number"]);
						commodity.BuyerCharName = r["purchase_name"] as string ?? string.Empty;
						Locker.Add(commodity);
					}
				}

				using (var cmd = new NpgsqlCommand(
					$"SELECT * FROM {Constants.DB_All_World_Schema_Name}.account_data WHERE account_id = {AccountID};",
					conn))
				using (var r = cmd.ExecuteReader())
				{
					if (r.Read())
					{
						CharacterSlots = (int)r["character_slots"];
						Admin = (int)r["admin"];
						NX_Credit = (int)r["nx_credit"];
						NX_Maplepoint = (int)r["nx_maplepoint"];
						NX_Prepaid = (int)r["nx_prepaid"];
					}
					else
					{
#if DEBUG
						CharacterSlots = 12;
						Admin = 1;
						NX_Credit = 50000;
						NX_Maplepoint = 50000;
						NX_Prepaid = 50000;
#else
						CharacterSlots = BaseCharSlots;
						Admin = 0;
						NX_Credit = 0;
						NX_Maplepoint = 0;
						NX_Prepaid = 0;
#endif
					}
				}
			}
		}
	}
}
