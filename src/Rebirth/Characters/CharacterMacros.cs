using Npgsql;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rebirth.Characters
{
	public class CharacterMacros : List<Macro>
	{
		public Character Parent { get; private set; }

		public CharacterMacros(Character c)
		{
			Parent = c;
		}

		public void Dispose()
		{
			Parent = null;
			Clear();
		}

		public void SendLoad()
		{
			Parent.SendPacket(CPacket.MacroSysDataInit(this));
		}

		public void Encode(COutPacket p)
		{
			p.Encode1((byte)Count);

			foreach (var macro in this)
			{
				macro.Encode(p);
			}
		}

		public void Decode(CInPacket p)
		{
			var count = p.Decode1();

			if (count > 5) return; // PE

			Clear(); // client resends all macros when one is modified

			for (int i = 0; i < count; i++)
			{
				var pMacro = new Macro();
				pMacro.nUID = i;
				pMacro.Decode(p);

				Add(pMacro);
			}
		}

		public async Task LoadFromDB()
		{
			// uid, character_id, skills[], name, mute
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.character_macros WHERE character_id = {Parent.dwId}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						Add(new Macro()
						{
							nUID = Count,
							aSkill = r["skills"] as int[],
							sName = r["name"] as string,
							bMute = (bool)r["mute"]
						});

						if (Count >= 5) break;
					}
				}
			}
		}

		public void SaveToDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.Append($"DELETE FROM {Constants.DB_All_World_Schema_Name}.character_macros WHERE character_id = {Parent.dwId};");

				foreach (var entry in this)
				{
					var key = (Parent.dwId << 1) | entry.nUID;
					dbQuery.Append($"INSERT INTO {Constants.DB_All_World_Schema_Name}.character_macros (character_id, skills, name, mute)");
					dbQuery.Append($"VALUES ({Parent.dwId}, @skills{key}, @name{key}, {entry.bMute});");
				}

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				{
					foreach (var entry in this)
					{
						var key = (Parent.dwId << 1) | entry.nUID;
						cmd.Parameters.AddWithValue($"name{key}", entry.sName);
						cmd.Parameters.AddWithValue($"skills{key}", entry.aSkill);
					}

					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
