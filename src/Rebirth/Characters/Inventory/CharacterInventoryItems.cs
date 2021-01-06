using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Npgsql;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Provider.Template.Item.Cash;

namespace Rebirth.Characters.Inventory
{
	public class CharacterInventoryItems : AbstractCharacterInventory<GW_ItemSlotBundle>
	{
		//TODO: Some additional logic to restrict slot to a byte ( maybe a max limit in base class )

		public static ILog Log = LogManager.GetLogger(typeof(CharacterInventoryItems));

		public int ExpCouponRate { get; private set; } // rates only used in cash inv
		public int DropCouponRate { get; private set; }

		public CharacterInventoryItems(InventoryType type)
			: base(type)
		{
			SlotLimit = (byte)(type == InventoryType.Cash ? 96 : 24);
		}

		protected override void EncodeSlot(short nSlot, COutPacket p) => p.Encode1((byte)nSlot);

		/// <summary>
		/// Fetches the first slot that matches the given item id and isnt full
		/// </summary>
		/// <param name="nItemID"></param>
		/// <param name="nSlot"></param>
		/// <returns></returns>
		public short GetFreeSlot(int nItemID)
		{
			foreach (var item in this)
			{
				if (item.Value.nItemID == nItemID && item.Value.nNumber != item.Value.SlotMax)
				{
					return item.Key;
				}
			}

			return GetFreeSlot(); // cant find a matching item, get any open slot
		}

		public override void Add(short slot, GW_ItemSlotBundle item)
		{
			if (InventoryType == InventoryType.Cash)
			{
				if (item.Template is CashItemTemplate template)
				{
					if (ItemConstants.IsExpCoupon(item.nItemID))
					{
						ExpCouponRate = template.Rate;
					}
					else if (ItemConstants.IsDropCoupon(item.nItemID))
					{
						DropCouponRate = template.Rate;
					}
				}
			}

			base.Add(slot, item);
		}

		public override bool Remove(short slot)
		{
			if (InventoryType == InventoryType.Cash)
			{
				if (Get(slot) is GW_ItemSlotBundle isb)
				{
					if (ItemConstants.IsExpCoupon(isb.nItemID))
					{
						ExpCouponRate = 0;
					}
					else if (ItemConstants.IsDropCoupon(isb.nItemID))
					{
						DropCouponRate = 0;
					}
				}
			}

			return base.Remove(slot);
		}

		public void SaveToDB(int dwCharId, InventoryType invType)
		{
			if (Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				var nLowerBound = (int)invType * 1000000;
				var nUpperBound = nLowerBound + 1000000 - 1;

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.char_inventory_items");
				dbQuery.AppendLine($"WHERE character_id = {dwCharId}");
				dbQuery.AppendLine($"AND item_id >= {nLowerBound}");
				dbQuery.AppendLine($"AND item_id < {nUpperBound}");
				dbQuery.AppendLine($"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMIN};"); // not in temp storage

				if (invType is InventoryType.Cash)
				{
					dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.inventory_pets");
					dbQuery.AppendLine($"WHERE character_id = {dwCharId};");
				}

				this.ForEach(item => dbQuery.AppendLine(item.Value.DbInsertString(dwCharId, item.Key)));

#if DEBUG
				Log.Debug(dbQuery.ToString());
#endif

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					foreach (var item in this)
					{
						if (item.Value is GW_ItemSlotPet isp)
						{
							var subkey = isp.CreateDbSubKey(item.Key);
							cmd.Parameters.AddWithValue($"petname{subkey}", isp.sPetName);
						}
					}

					cmd.ExecuteNonQuery();
				}
			}
		}

		public async Task LoadFromDB(int dwCharId, InventoryType invType)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				if (invType == InventoryType.Cash)
				{
					dbQuery.AppendLine($"SELECT inventory_slot, item_id, item_date_expire, i.item_serial_number, item_cash_serial_number,");
					dbQuery.AppendLine($"pet_level, pet_closeness, pet_fullness, pet_name, pet_remaining_life, pet_skill, pet_attribute, pet_date_dead");
					dbQuery.AppendLine($"FROM {Constants.DB_All_World_Schema_Name}.char_inventory_items i");
					dbQuery.AppendLine($"INNER JOIN {Constants.DB_All_World_Schema_Name}.inventory_pets p ON p.item_serial_number = i.item_serial_number");
					dbQuery.AppendLine($"WHERE i.character_id = {dwCharId}");
					//dbQuery.AppendLine($"AND item_id / 10000 = 500"); // TODO use proper join operation instead of doing division

					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					{
						try
						{
							using (var r = await cmd.ExecuteReaderAsync())
							{
								while (r.Read())
								{
									var slot = Convert.ToInt16(r["inventory_slot"]);

									Add(slot, new GW_ItemSlotPet(Convert.ToInt32(r["item_id"]))
									{
										tDateExpire = (DateTime)r["item_date_expire"],
										liSN = Convert.ToInt64(r["item_serial_number"]),
										liCashItemSN = Convert.ToInt64(r["item_cash_serial_number"]),

										nLevel = Convert.ToByte(r["pet_level"]),
										nTameness = Convert.ToInt16(r["pet_closeness"]),
										nRepleteness = Convert.ToByte(r["pet_fullness"]),
										sPetName = r["pet_name"] as string,
										nRemainLife = Convert.ToInt32(r["pet_remaining_life"]),
										usPetSkill = Convert.ToInt16(r["pet_skill"]),
										nAttribute = Convert.ToInt16(r["pet_attribute"]),
										// TODO pet date dead
									});
								}
							}
						}
						catch (Exception ex)
						{
							Log.Debug(dbQuery.ToString());
							Log.Debug(ex);
						}
					}

					dbQuery.Clear();
				}

				var buggedItems = new List<GW_ItemSlotBundle>();

				var nLowerBound = (int)invType * 1000000;
				var nUpperBound = nLowerBound + 1000000 - 1;

				dbQuery.AppendLine($"SELECT inventory_slot, item_id, item_quantity, item_date_expire, item_serial_number, item_cash_serial_number");
				dbQuery.AppendLine($"FROM {Constants.DB_All_World_Schema_Name}.char_inventory_items");
				dbQuery.AppendLine($"WHERE character_id = {dwCharId}");
				dbQuery.AppendLine($"AND item_id >= {nLowerBound}");
				dbQuery.AppendLine($"AND item_id < {nUpperBound}");
				dbQuery.AppendLine($"AND inventory_slot < {Constants.DB_ITEMSTORAGE_SLOTMIN}");
				dbQuery.AppendLine($"AND NOT item_id / 10000 = 500"); // TODO remove this division and just do some type of exclusive join operation with pet table

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					try
					{
						using (var r = await cmd.ExecuteReaderAsync())
						{
							while (r.Read())
							{
								var item = new GW_ItemSlotBundle(Convert.ToInt32(r["item_id"]))
								{
									nNumber = Convert.ToInt16(r["item_quantity"]),
									tDateExpire = (DateTime)r["item_date_expire"],
									liSN = Convert.ToInt64(r["item_serial_number"]),
									liCashItemSN = Convert.ToInt64(r["item_cash_serial_number"]),
									sTitle = "" // we dont store this currently
								};

								var slot = Convert.ToInt16(r["inventory_slot"]);

								if (!Contains(slot) /*|| r.GetInt16(0) > SlotLimit*/)
								{
									Add(slot, item);
								}
								else
								{
									buggedItems.Add(item);
								}
							}

							if (Count > SlotLimit)
							{
								SlotLimit = (byte)(Count + (4 - Count % 4));
							}
						}

						if (buggedItems.Count <= 0) return;

						dbQuery.Clear();

						short i = 900; // this way theyll get scooped into the normal inventory after they reload their inv
						foreach (var item in buggedItems)
						{
							dbQuery.AppendLine(item.DbInsertString(dwCharId, i));
							i += 1;
						}

						using (var cmd2 = new NpgsqlCommand(dbQuery.ToString(), conn))
							cmd2.ExecuteNonQuery();
					}
					catch (Exception ex)
					{
						Log.Debug(dbQuery.ToString());
						Log.Debug(ex);
					}
				}
			}
		}
	}
}
