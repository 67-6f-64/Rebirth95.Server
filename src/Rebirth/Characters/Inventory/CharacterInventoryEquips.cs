using log4net;
using Npgsql;
using Rebirth.Entities.Item;
using Rebirth.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Inventory
{
	public class CharacterInventoryEquips : AbstractCharacterInventory<GW_ItemSlotEquip>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CharacterInventoryEquips));

		public CharacterInventoryEquips()
			: base(InventoryType.Equip)
		{
			SlotLimit = (int)InventoryType > 5 ? (byte)96 : (byte)24;
		}

		protected override void EncodeSlot(short nSlot, COutPacket p) => p.Encode2(nSlot);

		public void SaveToDB(int dwCharId, InventoryType nType)
		{
			if (Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_inventory_equips");
				dbQuery.AppendLine($"WHERE character_id = {dwCharId}");
				dbQuery.AppendLine($"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMIN}");

				switch (nType)
				{
					case InventoryType.Equip:
						dbQuery.AppendLine("AND inventory_slot > 0;");
						break;
					case InventoryType.Cash:
						dbQuery.AppendLine("AND inventory_slot < 0");
						dbQuery.AppendLine("AND item_cash_serial_number > 0;");
						break;
					case InventoryType.Equipped:
						dbQuery.AppendLine("AND inventory_slot < 0");
						dbQuery.AppendLine("AND inventory_slot > -1000");
						dbQuery.AppendLine("AND item_cash_serial_number = 0;");
						break;
					case InventoryType.DragonEquipped:
						dbQuery.AppendLine("AND inventory_slot <= -1000");
						dbQuery.AppendLine("AND inventory_slot > -1100");
						dbQuery.AppendLine("AND item_cash_serial_number = 0;");
						break;
					case InventoryType.MechanicEquipped:
						dbQuery.AppendLine("AND inventory_slot <= -1100");
						dbQuery.AppendLine("AND inventory_slot > -1200");
						dbQuery.AppendLine("AND item_cash_serial_number = 0;");
						break;
					default:
						throw new InvalidOperationException
							($"{nType} is not a valid inventory type.");
				}

				this.ForEach(item
					=> dbQuery.AppendLine(item.Value.DbInsertString(dwCharId, item.Key)));

				try
				{
					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					{
						foreach (var item in this)
						{
							var subKey = item.Value.CreateDbSubKey(item.Key);
							cmd.Parameters.AddWithValue($"title{subKey}", item.Value.sTitle);
						}

						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					Log.Debug(dbQuery.ToString());
					Log.Debug(ex);
				}
			}
		}

		public async Task LoadFromDB(int dwCharId, InventoryType nType)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.char_inventory_equips");
				dbQuery.AppendLine($"WHERE character_id = {dwCharId}");

				switch (nType)
				{
					case InventoryType.Equip:
						dbQuery.AppendLine($"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMIN}");
						dbQuery.AppendLine("AND inventory_slot > 0");
						break;
					case InventoryType.Equipped:
						dbQuery.AppendLine("AND inventory_slot < 0");
						dbQuery.AppendLine("AND inventory_slot > -100");
						dbQuery.AppendLine("AND item_cash_serial_number = 0");
						break;
					case InventoryType.Cash:
						dbQuery.AppendLine("AND inventory_slot <= -100");
						dbQuery.AppendLine("AND inventory_slot > -1000");
						dbQuery.AppendLine("AND item_cash_serial_number > 0");
						break;
					case InventoryType.DragonEquipped:
						dbQuery.AppendLine("AND inventory_slot <= -1000");
						dbQuery.AppendLine("AND inventory_slot > -1100");
						dbQuery.AppendLine("AND item_cash_serial_number = 0");

						break;
					case InventoryType.MechanicEquipped:
						dbQuery.AppendLine("AND inventory_slot <= -1100");
						dbQuery.AppendLine("AND inventory_slot > -1200");
						dbQuery.AppendLine("AND item_cash_serial_number = 0");

						break;
					default:
						throw new InvalidOperationException
							($"{nType} is not a valid inventory type.");
				}

				var buggedItems = new List<GW_ItemSlotEquip>();

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					try
					{
						using (var r = await cmd.ExecuteReaderAsync())
						{
							while (r.Read())
							{
								var slot = Convert.ToInt16(r["inventory_slot"]);
								var e = new GW_ItemSlotEquip(Convert.ToInt32(r["item_id"]))
								{
									CurrentUpgradeCount = Convert.ToByte(r["item_upgrade_count"]),
									RemainingUpgradeCount = Convert.ToByte(r["item_remaining_upgrades"]),
									niSTR = Convert.ToInt16(r["item_str"]),
									niDEX = Convert.ToInt16(r["item_dex"]),
									niINT = Convert.ToInt16(r["item_int"]),
									niLUK = Convert.ToInt16(r["item_luk"]),
									niMaxHP = Convert.ToInt16(r["item_mhp"]),
									niMaxMP = Convert.ToInt16(r["item_mmp"]),
									niPAD = Convert.ToInt16(r["item_pad"]),
									niMAD = Convert.ToInt16(r["item_mad"]),
									niPDD = Convert.ToInt16(r["item_pdd"]),
									niMDD = Convert.ToInt16(r["item_mdd"]),
									niACC = Convert.ToInt16(r["item_acc"]),
									niEVA = Convert.ToInt16(r["item_eva"]),
									niCraft = Convert.ToInt16(r["item_craft"]),
									niSpeed = Convert.ToInt16(r["item_speed"]),
									niJump = Convert.ToInt16(r["item_jump"]),
									nAttribute = (ItemAttributeFlags)Convert.ToInt16(r["item_attribute"]),
									liSN = Convert.ToInt64(r["item_serial_number"]),
									sTitle = r["item_title"] as string,
									nLevelUpType = Convert.ToByte(r["item_level_type"]),
									nLevel = Convert.ToByte(r["item_level"]),
									nEXP = Convert.ToInt32(r["item_exp"]),
									nDurability = Convert.ToInt32(r["item_durability"]),
									HammerUpgradeCount = Convert.ToByte(r["item_hammer_count"]),
									nGrade = (PotentialGradeCode)Convert.ToInt16(r["item_grade"]),
									StarUpgradeCount = Convert.ToByte(r["item_star_count"]),
									nOption1 = Convert.ToInt16(r["item_option1"]),
									nOption2 = Convert.ToInt16(r["item_option2"]),
									nOption3 = Convert.ToInt16(r["item_option3"]),
									nSocket1 = Convert.ToInt16(r["item_socket1"]),
									nSocket2 = Convert.ToInt16(r["item_socket2"]),
									liCashItemSN = Convert.ToInt64(r["item_cash_serial_number"]),
									tSealingLock = (DateTime)r["item_sealinglock"],
									tDateExpire = (DateTime)r["item_date_expire"],
								};

								if (!Contains(slot) || slot > SlotLimit)
								{
									Add(slot, e);
								}
								else
								{
									buggedItems.Add(e);
								}
							}
						}

						if (Count > SlotLimit)
						{
							SlotLimit = (byte)(Count + (4 - Count % 4));
						}

						if (buggedItems.Count <= 0) return;

						dbQuery.Clear();

						// this way theyll get scooped into the normal inventory after they reload their inv
						short i = 900;
						foreach (var item in buggedItems)
						{
							dbQuery.AppendLine(item.DbInsertString(dwCharId, i));
							i += 1;
						}

						using (var cmd2 = new NpgsqlCommand(dbQuery.ToString(), conn))
						{
							foreach (var item in this)
							{
								var subKey = item.Value.CreateDbSubKey(item.Key);
								cmd.Parameters.AddWithValue($"title{subKey}", item.Value.sTitle);
							}

							cmd2.ExecuteNonQuery();
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex.ToString());
					}
				}
			}
		}
	}
}
