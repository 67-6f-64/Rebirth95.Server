using System;
using Rebirth.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Rebirth.Characters.Modify;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Characters
{
	public class CharacterRingInfo
	{
		private List<GW_CoupleRecord> lCoupleRecord;
		private List<GW_FriendRecord> lFriendRecord;
		private List<GW_MarriageRecord> lMarriageRecord;

		public Character Parent { get; private set; }

		public CharacterRingInfo(Character parent)
		{
			Parent = parent;
			lCoupleRecord = new List<GW_CoupleRecord>();
			lFriendRecord = new List<GW_FriendRecord>();
			lMarriageRecord = new List<GW_MarriageRecord>();
		}

		public void Insert(AbstractRingRecord record)
		{
			switch (record)
			{
				case GW_CoupleRecord cr:
					lCoupleRecord.Add(cr);
					break;
				case GW_FriendRecord fr:
					lFriendRecord.Add(fr);
					break;
				case GW_MarriageRecord mr:
					lMarriageRecord.Add(mr);
					break;
				default: throw new Exception("Invalid type passed to character ring records.");
			}
		}

		public void Dispose()
		{
			Parent = null;

			lCoupleRecord = null;
			lFriendRecord = null;
			lMarriageRecord = null;
		}

		public void EncodeEquippedRings(COutPacket p)
		{
			#region COUPLE_RING

			p.Encode1(0); // TODO

			#endregion

			#region FRIENDSHIP_RING

			GW_FriendRecord pFriendshipRing = null;
			var nFriendshipRingID = 0;
			long nFriendShipSN = 0;

			for (var bp = BodyPart.BP_RING1; bp < BodyPart.BP_RING4; bp++)
			{
				var item = InventoryManipulator.GetItem(Parent, bp, true);

				if (item is null) continue;

				if (ItemConstants.is_friendship_equip_item(item.nItemID))
				{
					pFriendshipRing = lFriendRecord.FirstOrDefault(ring => ring.liSN == item.liSN);
					break;
				}
			}

			p.Encode1(pFriendshipRing != null);

			if (pFriendshipRing != null)
			{
				p.Encode8(pFriendshipRing.liSN);
				p.Encode8(pFriendshipRing.liPairSN);
				p.Encode4(pFriendshipRing.dwFriendItemID);
			}

			#endregion

			#region MARRIAGE_RING

			p.Encode1(0); // TODO

			#endregion
		}

		public void EncodeRingRecords(COutPacket p)
		{
			p.Encode2(0);
			p.Encode2((short)lFriendRecord.Count);
			lFriendRecord.ForEach(ring => ring.Encode(p));
			p.Encode2(0);
		}

		public void SaveToDB()
		{
			// TODO couple and marriage

			if (lFriendRecord.Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.ring_friend_record");
				dbQuery.AppendLine($"WHERE character_id = {Parent.dwId};");

				foreach (var item in lFriendRecord)
				{
					dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.ring_friend_record");
					dbQuery.AppendLine($"(character_id, pair_character_id, item_serial_number, pair_item_serial_number, item_id)");
					dbQuery.AppendLine($"VALUES ({Parent.dwId}, {item.dwPairCharacterID}, {item.liSN}, {item.liPairSN}, {item.dwFriendItemID});");
				}

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					cmd.ExecuteNonQuery();
			}
		}

		public async Task LoadFromDB()
		{
			// TODO couple and marriage

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"SELECT rr.*, c.name FROM {Constants.DB_All_World_Schema_Name}.ring_friend_record rr");
				dbQuery.AppendLine($"JOIN {Constants.DB_All_World_Schema_Name}.characters c ON c.id = rr.pair_character_id");
				dbQuery.AppendLine($"WHERE character_id = {Parent.dwId}");

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						Insert(new GW_FriendRecord
						{
							dwPairCharacterID = Convert.ToInt32(r["pair_character_id"]),
							liSN = Convert.ToInt64(r["item_serial_number"]),
							liPairSN = Convert.ToInt64(r["pair_item_serial_number"]),
							dwFriendItemID = Convert.ToInt32(r["item_id"]),
							sPairCharacterName = Convert.ToString(r["name"])
						});
					}
				}
			}
		}
	}
}
