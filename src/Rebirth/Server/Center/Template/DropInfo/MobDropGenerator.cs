using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using log4net;
using Npgsql;

namespace Rebirth.Server.Center.Template.DropInfo
{
	public class MobDropGenerator : KeyedCollection<int, MobDropStruct>
	{
		public static ILog Log = LogManager.GetLogger(typeof(MobDropGenerator));

		public void Load(int nWorldID)
		{
			if (nWorldID != 0)
				throw new Exception("Invalid world ID supplied. Unable to fetch drops for world.");

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.drop_data;", conn))
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						var mobId = (int)r["mobid"];
						var drop = new DropStruct
						{
							ItemID = (int)r["itemid"],
							Chance = (int)r["chance"],
							MaximumQuantity = (int)r["maximum_quantity"],
							MinimumQuantity = (int)r["minimum_quantity"],
							QuestID = (int)r["questid"],
						};


						if (drop.QuestID != 0) continue;

						if (!Contains(mobId))
						{
							Add(new MobDropStruct
							{
								MobID = mobId,
								Drops = new List<DropStruct> { drop }
							});
						}
						else
						{
							this[mobId].Drops.Add(drop);
						}
					}
				}
			}

			if (Count <= 0) throw new Exception($"No drops found for world {nWorldID}.");
		}

		/// <summary>
		/// Removes items that aren't in the cached data.
		/// Note: Must be called after all game data has been cached.
		/// </summary>
		public void PurgeInvalidItems()
		{
			var entriesToRemove = new List<int>();
			foreach (var mob in this)
			{
				if (MasterManager.MobTemplates[mob.MobID] is null)
				{
					entriesToRemove.Add(mob.MobID);
				}
				else
				{
					var dropsToKeep = new List<DropStruct>();
					foreach (var drop in mob.Drops)
					{
						if (ItemConstants.GetInventoryType(drop.ItemID) == Common.Types.InventoryType.Equip)
						{
							if (MasterManager.EquipTemplates[drop.ItemID] is null) continue;
						}
						else
						{
							if (MasterManager.ItemTemplates[drop.ItemID] is null) continue;
						}

						dropsToKeep.Add(drop);
					}
					mob.Drops = dropsToKeep;
				}
			}

			foreach (var entry in entriesToRemove)
			{
				Remove(entry);
			}

			Log.Info("Drops: " + Count);
		}

		protected override int GetKeyForItem(MobDropStruct item)
		{
			return item.MobID;
		}
	}
}