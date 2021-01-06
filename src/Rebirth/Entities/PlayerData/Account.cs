using Npgsql;
using Rebirth.Entities.PlayerData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Rebirth.Server.Center;

namespace Rebirth.Entities
{
	public enum CashType
	{
		NX_Credit = 1,
		NX_Maplepoint = 2,
		NX_Prepaid = 4
	}
	public sealed class Account
	{
		public int ID { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int Ban { get; set; }
		public int Gender { get; set; }
		public WorldAccountData AccountData { get; set; }


		private int _rp;
		public int RebirthPoints
		{
			get => _rp;
			set => RebirthPointTransaction(value);
		}

		private int _vp;
		public int VotePoints
		{
			get => _vp;
			set => VotePointTransaction(value);
		}

		private void RebirthPointTransaction(int amount)
		{
			_rp += amount;
			using (var conn = new NpgsqlConnection(Constants.DB_Global_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"UPDATE {Constants.DB_Global_Schema}.accounts SET rebirth_points = {RebirthPoints}");
				dbQuery.AppendLine($"INSERT INTO {Constants.DB_Global_Schema}.points_log (account_id, amount, source, type)");
				dbQuery.AppendLine($"VALUES ({ID}, {amount}, 'system_game', 1)");

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		private void VotePointTransaction(int amount)
		{
			_vp += amount;

			using (var conn = new NpgsqlConnection(Constants.DB_Global_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"UPDATE {Constants.DB_Global_Schema}.accounts SET vote_points = {VotePoints}");
				dbQuery.AppendLine($"INSERT INTO {Constants.DB_Global_Schema}.points_log (account_id, amount, source, type)");
				dbQuery.AppendLine($"VALUES ({ID}, {amount}, 'system_game', 0)");

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Account constructor that fetches the
		///     account ID associated with the given character ID
		/// </summary>
		/// <param name="charId"></param>
		public Account(int charId)
		{
			// TODO modify this to work with multi-worlds
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT account_id FROM {Constants.DB_All_World_Schema_Name}.characters WHERE id = {charId}", conn))
				using (var r = cmd.ExecuteReader())
				{
					if (r.Read())
					{
						ID = Convert.ToInt32(r["account_id"]);
					}
				}
			}
		}

		public Account(string username)
		{
			Username = username;
		}

		public bool HasCash(CashType type, int amount)
		{
			switch (type)
			{
				case CashType.NX_Credit:
					return AccountData.NX_Credit >= amount;
				case CashType.NX_Maplepoint:
					return AccountData.NX_Maplepoint >= amount;
				case CashType.NX_Prepaid:
					return AccountData.NX_Prepaid >= amount;
			}
			return false;
		}

		public void ModifyCash(CashType type, int amount)
		{
			switch (type)
			{
				case CashType.NX_Credit:
					AccountData.NX_Credit += amount;
					break;
				case CashType.NX_Maplepoint:
					AccountData.NX_Maplepoint += amount;
					break;
				case CashType.NX_Prepaid:
					AccountData.NX_Prepaid += amount;
					break;
			}
		}

		/// <summary>
		/// Returns a list of all character ID's associated with this account
		/// </summary>
		/// <returns></returns>
		public List<int> LoadCharIdList()
		{
			var retVal = new List<int>();

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT id FROM {Constants.DB_All_World_Schema_Name}.characters WHERE account_id = {ID}", conn))
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						retVal.Add(Convert.ToInt32(r["id"]));
					}
				}
			}

			return retVal;
		}

		/// <summary>
		/// Queries the database for the highest leveled character.
		/// </summary>
		/// <param name="nExclId">Character ID to exclude</param>
		/// <returns>Level and name of character</returns>
		public (int, string) HighestLevelChar(int nExclId = 0)
		{
			try
			{
				using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
				{
					conn.Open();

					var dbQuery = new StringBuilder();
					dbQuery.AppendLine($"SELECT level, name FROM {Constants.DB_All_World_Schema_Name}.characters");
					dbQuery.AppendLine($"WHERE account_id = {ID}");
					dbQuery.AppendLine($"AND NOT id = {nExclId}");
					dbQuery.AppendLine($"ORDER BY level DESC");

					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					using (var r = cmd.ExecuteReader())
					{
						if (r.Read())
						{
							return (Convert.ToInt32(r["level"]), r["name"] as string);
						}
					}
				}
			}
			catch (InvalidCastException)
			{
				// this means there are no other chars
			}

			return (0, string.Empty);
		}

		/// <summary>
		/// Returns true if able to find the username or account id in the database.
		/// </summary>
		/// <returns></returns>
		public bool Init()
		{
			var watch = new Stopwatch();
			watch.Start();
			using (var conn = new NpgsqlConnection(Constants.DB_Global_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"SELECT * FROM {Constants.DB_Global_Schema}.accounts");

				if (!string.IsNullOrEmpty(Username))
				{
					dbQuery.AppendLine($"WHERE UPPER(username) = '{Username.ToUpper()}'");
				}
				else
				{
					dbQuery.AppendLine($"WHERE id = {ID}");
				}

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				using (var r = cmd.ExecuteReader())
				{
					if (r.Read())
					{
						ID = Convert.ToInt32(r["id"]);
						Username = r["username"] as string;
						Password = r["password"] as string;
						Ban = Convert.ToInt32(r["ban"]);
						Gender = Convert.ToInt32(r["gender"]);
						_vp = Convert.ToInt32(r["vote_points"]);
						_rp = Convert.ToInt32(r["rebirth_points"]);
						AccountData = new WorldAccountData(ID, 0);
						watch.Stop();
#if DEBUG
						MasterManager.Log.Debug("Account.Init Time: " + watch.ElapsedMilliseconds);
#endif
						return true;
					}
				}
			}
			watch.Stop();
#if DEBUG
			MasterManager.Log.Debug("Account.Init Time: " + watch.ElapsedMilliseconds);
#endif
			return false;
		}

		/// <summary>
		/// Saves player account to the database.
		/// The columns that this makes changes to are:
		///      - ban
		///      - gender
		/// </summary>
		public void Save()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_Global_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"UPDATE {Constants.DB_Global_Schema}.accounts");
				dbQuery.AppendLine($"SET ban = {Ban}, gender = {Gender}, last_login = '{DateTime.Now.ToSqlTimeStamp()}', vote_points = {VotePoints}, rebirth_points = {RebirthPoints}");
				dbQuery.AppendLine($"WHERE id = {ID}");

				try
				{
					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					{
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					MasterManager.Log.Error(dbQuery.ToString());
					MasterManager.Log.Error(ex.ToString());
				}
			}

			AccountData.SaveToDB();
		}
	}
}