using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Rebirth.Characters
{
	public class MobKillTracker
	{
		public int nMobID { get; set; }
		public long nCount { get; set; }
		public DateTime tLastKillTime { get; set; }
	}

	public class ScrollUseTracker
	{
		public int nScrollID { get; set; }
		public int nFailedCount { get; set; }
		public int nSuccessCount { get; set; }
		public int nDestroyCount { get; set; }
		public DateTime tLastUseTime { get; set; }
	}

	public class CharacterStatisticsTracker
	{
		public Character Parent { get; }
		public int dwParentID => Parent.dwId;

		// ========================================== \\

		public long nDamageDealt { get; set; }
		public long nDamageTaken { get; set; }
		public long nRecoveredPotionHP { get; set; }
		public long nRecoveredPotionMP { get; set; }
		public long nMobsKilled { get; set; }
		public long nEquipsDestroyed { get; set; }
		public int nDeaths { get; set; }
		public long nExpLostOnDeath { get; set; }
		public long nSecondsOnline { get; set; }

		// TODO add below to db
		public int nCubesUsed { get; set; }
		public long nMesoFromMobs { get; set; }
		public long nMesoFromItemSale { get; set; }
		public int nGachaTicketsUsed { get; set; }
		public int nFameGiven { get; set; }

		// ========================================== \\

		private readonly Dictionary<int, MobKillTracker> aMobKills;
		private readonly Dictionary<int, ScrollUseTracker> aScrollUses;
		public IEnumerable<KeyValuePair<int, MobKillTracker>> GetMobKillsOrdered() => aMobKills.OrderByDescending(entry => entry.Value.nCount);
		public IEnumerable<KeyValuePair<int, ScrollUseTracker>> GetScrollUseOrdered() => aScrollUses.OrderByDescending(entry => entry.Value.nSuccessCount + entry.Value.nFailedCount);

		public CharacterStatisticsTracker(Character parent)
		{
			Parent = parent;
			aMobKills = new Dictionary<int, MobKillTracker>();
			aScrollUses = new Dictionary<int, ScrollUseTracker>();
		}

		public void IncrementMob(int nMobID)
		{
			nMobsKilled += 1;

			if (aMobKills.ContainsKey(nMobID))
			{
				aMobKills[nMobID].nCount += 1;
			}
			else
			{
				aMobKills.Add(nMobID, new MobKillTracker { nMobID = nMobID, nCount = 1, tLastKillTime = DateTime.Now });
			}
		}

		public void IncrementScrollUse(int nScrollID, bool bSuccess, bool bDestroyed = false)
		{
			if (!aScrollUses.ContainsKey(nScrollID))
			{
				aScrollUses.Add(nScrollID, new ScrollUseTracker { nScrollID = nScrollID });
			}

			aScrollUses[nScrollID].tLastUseTime = DateTime.Now;

			if (bSuccess)
			{
				aScrollUses[nScrollID].nSuccessCount += 1;
			}
			else
			{
				aScrollUses[nScrollID].nFailedCount += 1;

				if (bDestroyed)
				{
					aScrollUses[nScrollID].nDestroyCount += 1;
					nEquipsDestroyed += 1;
				}
			}
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.char_mob_tracker WHERE char_id = {dwParentID};", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						aMobKills.Add(Convert.ToInt32(r["mob_id"]),
							new MobKillTracker
							{
								nMobID = Convert.ToInt32(r["mob_id"]),
								nCount = Convert.ToInt64(r["mob_amount"]),
								tLastKillTime = Convert.ToDateTime(r["mob_last_killed"])
							});
					}
				}

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.char_scroll_tracker WHERE char_id = {dwParentID};", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						aScrollUses.Add(Convert.ToInt32(r["scroll_id"]),
							new ScrollUseTracker
							{
								nScrollID = Convert.ToInt32(r["scroll_id"]),
								nSuccessCount = Convert.ToInt32(r["scroll_success_count"]),
								nFailedCount = Convert.ToInt32(r["scroll_fail_count"]),
								nDestroyCount = Convert.ToInt32(r["scroll_destroy_count"]),
								tLastUseTime = Convert.ToDateTime(r["scroll_last_use"])
							});
					}
				}

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.char_aggregate_stats WHERE char_id = {dwParentID};", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					if (r.Read())
					{
						nDamageDealt = Convert.ToInt64(r["stats_damage_dealt"]);
						nDamageTaken = Convert.ToInt64(r["stats_damage_taken"]);
						nRecoveredPotionHP = Convert.ToInt64(r["stats_recovered_potion_hp"]);
						nRecoveredPotionMP = Convert.ToInt64(r["stats_recovered_potion_mp"]);
						nMobsKilled = Convert.ToInt64(r["stats_mobs_killed"]);
						nEquipsDestroyed = Convert.ToInt64(r["stats_equips_destroyed"]);
						nDeaths = Convert.ToInt32(r["stats_deaths"]);
						nExpLostOnDeath = Convert.ToInt64(r["stats_exp_lost_death"]);
						nSecondsOnline = Convert.ToInt64(r["stats_seconds_online"]);
					}
				}
			}
		}

		public void SaveToDB()
		{
			if (aMobKills.Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sb = new StringBuilder();
				sb.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_mob_tracker WHERE char_id = {dwParentID};");
				sb.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_scroll_tracker WHERE char_id = {dwParentID};");
				sb.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_aggregate_stats WHERE char_id = {dwParentID};");

				foreach (var mob in aMobKills)
				{
					sb.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_mob_tracker (char_id, mob_id, mob_amount, mob_last_killed) VALUES");
					sb.AppendLine($"({dwParentID}, {mob.Value.nMobID}, {mob.Value.nCount}, '{mob.Value.tLastKillTime.ToSqlTimeStamp()}');");
				}

				foreach (var scroll in aScrollUses)
				{
					sb.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_scroll_tracker (char_id, scroll_id, scroll_success_count, scroll_fail_count, scroll_destroy_count, scroll_last_use) VALUES");
					sb.AppendLine($"({dwParentID}, {scroll.Value.nScrollID}, {scroll.Value.nSuccessCount}, {scroll.Value.nFailedCount}, {scroll.Value.nDestroyCount}, '{scroll.Value.tLastUseTime.ToSqlTimeStamp()}');");
				}

				sb.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_aggregate_stats (char_id, stats_damage_dealt, stats_damage_taken, stats_recovered_potion_hp, ");
				sb.AppendLine($"stats_recovered_potion_mp, stats_mobs_killed, stats_equips_destroyed, stats_deaths, stats_exp_lost_death, stats_seconds_online) VALUES");
				sb.AppendLine($"({dwParentID}, {nDamageDealt}, {nDamageTaken}, {nRecoveredPotionHP}, {nRecoveredPotionMP}, {nMobsKilled}, {nEquipsDestroyed}, {nDeaths}, {nExpLostOnDeath}, {nSecondsOnline});");

				using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
					cmd.ExecuteNonQuery();
			}
		}
	}
}
