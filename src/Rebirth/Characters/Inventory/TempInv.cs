using log4net;
using Npgsql;
using Rebirth.Entities.Item;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebirth.Common.Types;
using static Rebirth.Field.MiniRoom.MiniRoomEnum;

namespace Rebirth.Characters.Inventory
{
	public class TempInventory
	{
		public static ILog Log = LogManager.GetLogger(typeof(TempInventory));

		private readonly List<TempItem> items = new List<TempItem>();

		private readonly int maxsize;
		public int dwOwnerID { get; set; }
		public int Count => items.Count;
		public bool Full => maxsize <= Count;
		public long Meso { get; set; }
		public MR_Type Type { get; set; }

		public TempInventory(MR_Type type, int size, int dwCharId)
		{
			Type = type;
			maxsize = size;
			Meso = 0;
			dwOwnerID = dwCharId;

			MasterManager.TempInvManager.Add(dwCharId, this);
			LoadFromDB();
		}

		public void Add(TempItem item)
		{
			if (item.TargetSlot < 0)
				item.TargetSlot = GetNextSlot();

			if (CanAddToSlot(item.TargetSlot))
			{
				items.Add(item);
			}
		}

		public TempItem GetItemInSlot(int nSlot) => items.Find(i => i.TargetSlot == nSlot) ?? null;

		private short GetNextSlot()
		{
			for (short i = 0; i < maxsize; i++)
			{
				if (items.FirstOrDefault(ctx => ctx.TargetSlot == i) == null)
					return i; // extremely inefficient, todo fix this
			}
			return -1;
		}

		public bool CanAddToSlot(short nSlot) => items.All(ctx => ctx.TargetSlot != nSlot) && nSlot >= 0 && nSlot < maxsize;
		public List<TempItem> GetAll() => items;

		public TempItem GetAndRemove(short nSlot)
		{
			var pOutItem = items.FirstOrDefault(ctx => ctx.TargetSlot == nSlot);

			if (pOutItem is null)
				return null;

			items.Remove(pOutItem);

			// reorder
			for (int i = nSlot; i < maxsize; i++)
			{
				var pLoopItem = items.FirstOrDefault(ctx => ctx.TargetSlot == i);

				if (pLoopItem is null)
					continue;

				pLoopItem.TargetSlot -= 1;
			}

			return pOutItem;
		}

		public void Clear()
		{
			items.Clear();
			Meso = 0;
		}

		public void SaveToDB()
		{
			if (Type != MR_Type.EntrustedShop) return; // we only save hired merchants

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_inventory_items "
							+ $"WHERE character_id = {dwOwnerID} "
							+ $"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMAX} "
							+ $"AND inventory_slot >= {Constants.DB_ITEMSTORAGE_SLOTMIN};");

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.inventory_pets "
							+ $"WHERE character_id = {dwOwnerID}");

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_inventory_equips "
							+ $"WHERE character_id = {dwOwnerID} "
							+ $"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMAX} "
							+ $"AND inventory_slot >= {Constants.DB_ITEMSTORAGE_SLOTMIN};");

				items.ForEach(item => dbQuery.AppendLine(item.Item.DbInsertString(dwOwnerID, item.OriginalSlot)));

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					foreach(var item in items)
					{
						// TODO if we add bundle item titles we need to handle that
						// TODO if we add support for pets in temp inv we need to handle their titles
						if (item.Item is GW_ItemSlotEquip isp)
						{
							var subkey = item.Item.CreateDbSubKey(item.OriginalSlot);
							cmd.Parameters.AddWithValue($"title{subkey}", isp.sTitle);
						}
					}
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT item_id, item_quantity, item_date_expire, item_serial_number, item_cash_serial_number FROM {Constants.DB_All_World_Schema_Name}.char_inventory_items "
												+ $"WHERE character_id = {dwOwnerID} "
												+ $"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMAX} "
												+ $"AND inventory_slot >= {Constants.DB_ITEMSTORAGE_SLOTMIN}", conn))
				{
					using (var r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							var i = new GW_ItemSlotBundle(Convert.ToInt16(r["item_id"]))
							{
								nNumber = Convert.ToInt16(r["item_quantity"]),
								tDateExpire = (DateTime)r["item_date_expire"],
								liSN = Convert.ToInt16(r["item_serial_number"]),
								liCashItemSN = Convert.ToInt16(r["item_cash_serial_number"]),
							};

							var tItem = new TempItem(i, 0, 0, 0);

							Add(tItem);
						}
					}
				}

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.char_inventory_equips "
									   + $"WHERE character_id = {dwOwnerID} "
									   + $"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMAX} "
									   + $"AND inventory_slot >= {Constants.DB_ITEMSTORAGE_SLOTMIN}", conn))
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						//Log.Info("Before adding item...");
						var e = new GW_ItemSlotEquip(r.GetInt32(2))
						{
							dwInvItemId = (int)r.GetInt64(0),
							// character_id =                   (1),
							//nItemID = r.GetInt32(2),
							CurrentUpgradeCount = (byte)r.GetInt16(3),
							RemainingUpgradeCount = (byte)r.GetInt16(4),
							niSTR = r.GetInt16(5),
							niDEX = r.GetInt16(6),
							niINT = r.GetInt16(7),
							niLUK = r.GetInt16(8),
							niMaxHP = r.GetInt16(9),
							niMaxMP = r.GetInt16(10),
							niPAD = r.GetInt16(11),
							niMAD = r.GetInt16(12),
							niPDD = r.GetInt16(13),
							niMDD = r.GetInt16(14),
							niACC = r.GetInt16(15),
							niEVA = r.GetInt16(16),
							niCraft = r.GetInt16(17),
							niSpeed = r.GetInt16(18),
							niJump = r.GetInt16(19),
							nAttribute = (ItemAttributeFlags)r.GetInt16(20),
							liSN = r.GetInt64(21),
							sTitle = r["item_title"] as string,
							nLevelUpType = (byte)r.GetInt16(23),
							nLevel = (byte)r.GetInt16(24),
							nEXP = r.GetInt32(25),
							nDurability = r.GetInt32(26),
							HammerUpgradeCount = (byte)r.GetInt32(27),
							nGrade = (PotentialGradeCode)r.GetInt16(28),
							StarUpgradeCount = (byte)r.GetInt16(29),
							nOption1 = r.GetInt16(30),
							nOption2 = r.GetInt16(31),
							nOption3 = r.GetInt16(32),
							nSocket1 = r.GetInt16(33),
							nSocket2 = r.GetInt16(34),
							liCashItemSN = r.GetInt64(38),
							tSealingLock = (DateTime)r["sealinglock_datetime"],
							tDateExpire = (DateTime)r["date_expire"],
						};

						//Log.Info($"Adding item ID {r.GetInt32(2)} to slot {r.GetInt16(36)} to char ID {r.GetInt32(1)}/{charId}");

						var tItem = new TempItem(e, 0, 0, 0); // 0's cuz we dont care

						Add(tItem);
					}
				}

				using (var cmd = new NpgsqlCommand($"SELECT merchant_mesos "
												 + $"FROM {Constants.DB_All_World_Schema_Name}.characters "
												 + $"WHERE id = {dwOwnerID}", conn))
				{
					using (var r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							Meso = r.GetInt64(0);
							break;
						}
					}
				}
			}
		}
	}
}
