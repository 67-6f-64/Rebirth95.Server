using Npgsql;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebirth.Characters
{
	public class CharacterKeyMap
	{
		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		private readonly Dictionary<int, KeyValuePair<byte, int>> map = new Dictionary<int, KeyValuePair<byte, int>>();
		public int Count => map.Count;

		public int m_nPetConsumeItemID_HP { get; private set; }
		public int m_nPetConsumeItemID_MP { get; private set; }

		public CharacterKeyMap(int parent)
		{
			dwParentID = parent;
		}

		public void Dispose()
		{
			map.Clear();
		}

		public void ChangePetConsumeItemID(int nItemID)
		{
			m_nPetConsumeItemID_HP = nItemID;
			Parent.SendPacket(PetConsumeItemPacket());
		}

		public void ChangePetConsumeMPItemID(int nItemID)
		{
			m_nPetConsumeItemID_MP = nItemID;
			Parent.SendPacket(PetConsumeMPItemPacket());
		}

		public void Insert(int key, byte type, int action)
		{
			Delete(key);
			map.Add(key, new KeyValuePair<byte, int>(type, action));
		}

		public void Delete(int key)
		{
			if (map.ContainsKey(key))
				map.Remove(key);
		}

		public KeyValuePair<byte, int> Get(int key)
		{
			if (map.ContainsKey(key))
				return map[key];
			return new KeyValuePair<byte, int>(0, 0);
		}

		// BMS -> CUser::OnFuncKeyMappedModified
		public void OnFuncKeyMappedModified(CInPacket p)
		{
			var nType = p.Decode4();

			if (nType > 0)
			{
				if (nType == 1) // FuncKeyMapped_PetConsumeItemModified
				{
					ChangePetConsumeItemID(p.Decode4());
				}
				else if (nType == 2) // FuncKeyMapped_PetConsumeMPItemModified
				{
					ChangePetConsumeMPItemID(p.Decode4());
				}
			}
			else if (nType == 0) // FuncKeyMapped_KeyModified
			{
				var nCount = p.Decode4();

				if (nCount <= 0) return;

				for (var i = 0; i < nCount; i++)
				{
					var key = p.Decode4();

					if (key >= 0 && key <= 89) // BMS, assuming this is the same for v95 GMS
					{
						var type = p.Decode1();
						var action = p.Decode4(); // skill ID/consumable/key action

						if (type != 0)
							Insert(key, type, action);
						else
							Delete(key);
					}
				}
			}
		}

		public void OnQuickslotKeyMapped(CInPacket p)
		{
			for (int i = 0; i < 8; i++)
			{
				Insert(93 + 1, 8, p.Decode4());
			}
		}

		public void SendLoad()
		{
			Parent.SendPacket(FuncKeyMappedPacket());

			if (m_nPetConsumeItemID_HP > 0)
				Parent.SendPacket(PetConsumeItemPacket());

			if (m_nPetConsumeItemID_MP > 0)
				Parent.SendPacket(PetConsumeMPItemPacket());
		}

		private COutPacket FuncKeyMappedPacket()
		{
			var p = new COutPacket(SendOps.LP_FuncKeyMappedInit);
			p.Encode1(0); // 1 for default keymap

			for (int i = 0; i < 90; i++)
			{
				if (map.ContainsKey(i))
				{
					p.Encode1(map[i].Key);
					p.Encode4(map[i].Value);
				}
				else
				{
					p.Encode1(0);
					p.Encode4(0);
				}
			}

			return p;
		}

		private COutPacket PetConsumeItemPacket()
		{
			var p = new COutPacket(SendOps.LP_PetConsumeItemInit);
			p.Encode4(m_nPetConsumeItemID_HP);
			return p;
		}

		private COutPacket PetConsumeMPItemPacket()
		{
			var p = new COutPacket(SendOps.LP_PetConsumeMPItemInit);
			p.Encode4(m_nPetConsumeItemID_MP);
			return p;
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM rebirth.key_map WHERE character_id = {dwParentID}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						int[] key_array = r["key_array"] as int[],
							  type_array = r["type_array"] as int[],
							  action_array = r["action_array"] as int[];

						for (int i = 0; i < key_array.Length; i++)
						{
							Insert(key_array[i], (byte)type_array[i], action_array[i]);
						}

						m_nPetConsumeItemID_HP = r.GetInt32(4);
						m_nPetConsumeItemID_MP = r.GetInt32(5);
					}
				}
			}
		}

		public void SaveToDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"UPDATE rebirth.key_map "
												   + $"SET key_array = @key, "
												   + $"type_array = @type, "
												   + $"action_array = @action, "
												   + $"pet_consume_item_id = {m_nPetConsumeItemID_HP}, "
												   + $"pet_consume_mp_item_id = {m_nPetConsumeItemID_MP} "
												   + $"WHERE character_id = {dwParentID}", conn))
				{
					List<int> key = new List<int>(),
							  type = new List<int>(),
							  action = new List<int>();

					foreach (var func in map)
					{
						key.Add(func.Key);
						type.Add(func.Value.Key);
						action.Add(func.Value.Value);
					}

					cmd.Parameters.AddWithValue("key", key);
					cmd.Parameters.AddWithValue("type", type);
					cmd.Parameters.AddWithValue("action", action);

					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
