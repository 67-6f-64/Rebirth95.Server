using System;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Rebirth.Characters.Stat;
using Rebirth.Network;

namespace Rebirth.Entities.PlayerData
{
	public class CharacterEntry
	{
		public CharacterStat Stats { get; set; }
		public AvatarLook Look { get; set; }

		public CharacterEntry() { }

		public void Encode(COutPacket p)
		{
			const bool ranking = false;

			Stats.Encode(p);
			Look.Encode(p);

			p.Encode1(0); //VAC
			p.Encode1(ranking); //ranking

			if (ranking)
			{
				p.Skip(16);
				//v21->nWorldRank = 0;
				//v21->nWorldRankGap = 0;
				//v21->nJobRank = 0;
				//v21->nJobRankGap = 0;
			}
		}

		public void Load(int charId)
		{
			Stats = new CharacterStat(charId);
			Task.WaitAll(Stats.LoadFromDB());
			Look = new AvatarLook();
			Look.CopyStats(Stats);

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();
				using (var cmd = new NpgsqlCommand($"SELECT item_id, inventory_slot FROM {Constants.DB_All_World_Schema_Name}.char_inventory_equips "
									   + $"WHERE character_id = {charId} AND inventory_slot < 0 AND inventory_slot > -200"
									   + $"ORDER BY inventory_slot ASC", conn))
				using (var r = cmd.ExecuteReader())
					while (r.Read())
					{
						var slot = Math.Abs(r.GetInt16(1));

						if (slot >= 100) slot -= 100;

						if (Look.aEquip[slot] == 0)
						{
							Look.aEquip[slot] = r.GetInt32(0);
						}
						else
						{
							Look.aUnseenEquip[slot] = r.GetInt32(0);
						}
					}
			}
		}

		public void Insert(int accountId)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sb = new StringBuilder();

				sb.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.characters");
				sb.AppendLine($"(account_id, name, gender, skin, face, hair, pet_locker, level, job, str, dex, int,");
				sb.AppendLine($"luk, hp, m_hp, mp, m_mp, ap, pos_map, portal, sub_job, extend_sp, char_wish_list, char_last_fame)");

				sb.AppendLine($"VALUES ({accountId}, @name, {Stats.nGender}, {Stats.nSkin}, {Stats.nFace}, {Stats.nHair},");
				sb.AppendLine($"@locker, {Stats.nLevel}, {Stats.nJob}, {Stats.nSTR}, {Stats.nDEX}, {Stats.nINT},");
				sb.AppendLine($"{Stats.nLUK}, {Stats.nHP}, {Stats.nMHP}, {Stats.nMP}, {Stats.nMMP}, {Stats.nAP},");
				sb.AppendLine($"{Stats.dwPosMap}, {Stats.nPortal}, {Stats.nSubJob}, @extendsp, @wishlist, @lastfame);");

				// add character
				using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
				{
					cmd.Parameters.AddWithValue("name", Stats.sCharacterName);
					cmd.Parameters.AddWithValue("locker", new int[3]);
					cmd.Parameters.AddWithValue("extendsp", new int[10]);
					cmd.Parameters.AddWithValue("wishlist", new int[10]);
					cmd.Parameters.AddWithValue("lastfame", DateTime.Now.AddDays(-1));
					cmd.ExecuteNonQuery();
				}

				// get the char ID to return to the calling method
				using (var cmd = new NpgsqlCommand($"SELECT id FROM {Constants.DB_All_World_Schema_Name}.characters WHERE name = @name", conn))
				{
					cmd.Parameters.AddWithValue("name", Stats.sCharacterName);
					using (var r = cmd.ExecuteReader())
						while (r.Read())
							Stats.dwCharacterID = r.GetInt32(0);
				}
			}
		}
	}
}
