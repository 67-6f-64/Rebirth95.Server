using System;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Rebirth.Server.Game;
using Rebirth.Server.Login;
using Rebirth.Server.Shop;
using Rebirth.Tools;

namespace Rebirth.Server.Center
{
	public class WvsCenter : IDisposable
	{
		public static readonly ILog Log = LogManager.GetLogger(typeof(WvsCenter));
		public static IConfiguration Config { get; private set; } //TODO: Get rid of this
																  //-----------------------------------------------------------------------------
		public WvsLogin WvsLogin { get; }
		public WvsShop WvsShop { get; }
		public WvsGame[] WvsGames { get; }
		//-----------------------------------------------------------------------------
		private CTimer TimerUpdate { get; set; }
		private CTimer TimerPing { get; set; }
		//-----------------------------------------------------------------------------
		public WvsCenter(IConfiguration config)
		{
			Config = config;

			var channels = config.GetValue<int>("channels");

			WvsLogin = new WvsLogin(this);
			WvsShop = new WvsShop(this);

			WvsGames = new WvsGame[channels];

			for (int i = 0; i < WvsGames.Length; i++)
			{
				var channel = (byte)i;
				WvsGames[i] = new WvsGame(this, channel);
			}
		}

		//-----------------------------------------------------------------------------

		public void Start()
		{
			if (StartupHealthCheck())
			{
				MasterManager.Load(0);
				//EmojiManager.Instance.LoadAll();

				WvsLogin.Start();
				WvsGames.ToList().ForEach(x => x.Start());

				MasterManager.SetEventManager(WvsGames[0]);

				WvsShop.Start();

				TimerUpdate = WvsLogin.CreateTimer(); //Temp
				TimerUpdate.Interval = Constants.GlobalUpdateDelay;
				TimerUpdate.Elapsed = GlobalUpdater;
				TimerUpdate.Start();

				TimerPing = WvsLogin.CreateTimer(); //Temp
				TimerPing.Interval = Constants.KeepAliveDelay;
				TimerPing.Elapsed = PingUpdater;
				TimerPing.Start();

				//FetchLastDailyQuestRecordReset();

				Log.Info("[WvsCenter] Started");

				if (Constants.AllowAccountLoginOverride)
				{
					Log.Info("[WARNING] Be advised, account login override is enabled.");
				}
			}
			else
			{
				Log.Error("[WvsCenter] Failed startup health check.");
			}
		}

		public void Stop()
		{
			TimerUpdate.Stop();
			TimerPing.Stop();

			WvsLogin.Stop();
			WvsShop.Stop();

			for (int i = 0; i < WvsGames.Length; i++)
			{
				WvsGames[i].Stop();
			}

			Log.Info("[WvsCenter] Stopped");
		}

		//-----------------------------------------------------------------------------

		/// <summary>
		/// Performs a pre-flight check to verify integrity of server components.
		/// </summary>
		/// <returns></returns>
		private bool StartupHealthCheck()
		{
			foreach (var item in Constants.DATA_FILES)
			{
				var path = string.Concat(Constants.GameDataNXPath, item, Constants.DATA_FILE_EXTENSION);

				if (!File.Exists(path))
				{
					Log.Error($"Missing data file. Name: {item}{Constants.DATA_FILE_EXTENSION}");
					Log.Error($"Expected location: {Constants.GameDataNXPath}");
					Log.Error($"Full expected path: {path}");
					return false;
				}
			}

			var cons = new string[][]
			{
				new string[] { Constants.DB_Global_ConString, Constants.DB_Global_Database },
				new string[] { Constants.DB_World0_ConString, Constants.DB_World0_Database }
			};

			foreach(var item in cons)
			{
				var constring = item[0];
				var db_name = item[1];

				try
				{
					using (var conn = new NpgsqlConnection(constring))
					{
						try
						{
							// if this exception happens here: SocketException: A non-blocking socket operation could not be completed immediately
							// uncheck the box for the compiler to break when the exception is thrown
							// happens because of npgsql limitations (https://github.com/npgsql/npgsql/issues/1183)
							conn.Open();
						}
						catch 
						{
							Log.Error($"Unable to open DB using connection string: {constring}");
							return false;
						}

						using (var cmd = new NpgsqlCommand($"SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower('{db_name}');", conn))
						{
							if (cmd.ExecuteScalar() is null)
							{
								Log.Error($"Unable to return any info from database with name: {db_name}");
								Log.Error($"Full connection string: {constring}");
								return false;
							}
						}
					}
				}
				catch
				{
					Log.Error($"Unknown error when trying to access DB with name: {db_name}");
					Log.Error($"Full connection string: {constring}");
					return false;
				}
			}

			Log.Info("Pre-startup checks complete.");
			return true;
		}

		private void PingUpdater()
		{
			WvsLogin.OnKeepAlive();
			WvsShop.OnKeepAlive();

			for (int i = 0; i < WvsGames.Length; i++)
			{
				WvsGames[i].OnKeepAlive();
			}
		}

		private void GlobalUpdater()
		{
			MasterManager.PartyPool.Update();
			MasterManager.AvatarMan.Update();
			MasterManager.GuildManager.Update();
			MasterManager.EventManager.TryDoEvent(false);

			//ResetDailyQuestRecords(); // TODO hook this up to db
		}

		private DateTime tLastDailyReset;

		private void ResetDailyQuestRecords()
		{
			if (!tLastDailyReset.AddedHoursExpired(24)) return;

			var dbQuery = new StringBuilder();

			foreach (var item in Constants.DailyQuestRecordIDs)
			{
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.quest_record WHERE quest_id = {item};");
			}

			dbQuery.AppendLine($"UPDATE {Constants.DB_All_World_Schema_Name}.server_info SET lastdailyreset = @dt;");

			using (var dbCon = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				dbCon.Open();
				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), dbCon))
				{
					var x = new NpgsqlParameter($"dt", NpgsqlTypes.NpgsqlDbType.Timestamp);
					x.Value = tLastDailyReset;
					cmd.Parameters.Add(x);

					cmd.ExecuteNonQuery();
				}
			}
		}

		private void FetchLastDailyQuestRecordReset()
		{
			var dbQuery = new StringBuilder();

			dbQuery.AppendLine($"SELECT lastdailyreset FROM {Constants.DB_All_World_Schema_Name}.server_info;");

			using (var dbCon = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				dbCon.Open();
				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), dbCon))
				{
					using (var r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							tLastDailyReset = (DateTime)r["lastdailyreset"];
						}
					}
				}
			}
		}

		//-----------------------------------------------------------------------------

		public void Dispose()
		{
			//Db?.Dispose();
			Log.Info("[WvsCenter] Dispose");
		}
	}
}