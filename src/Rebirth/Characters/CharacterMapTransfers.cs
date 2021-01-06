using System.Collections.Generic;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Rebirth.Common.Types;

namespace Rebirth.Characters
{
	public class CharacterMapTransfers
	{
		const int RegLength = 5;
		const int VipLength = 10;

		public Character Parent { get; private set; }

		public int[] adwMapTransfer { get; private set; }
		public int[] adwMapTransferEx { get; private set; }

		public CharacterMapTransfers(Character c)
		{
			Parent = c;
		}

		public void Dispose()
		{
			Parent = null;
			adwMapTransfer = null;
			adwMapTransferEx = null;
		}

		public bool OnUseRequest(int nItemID, CInPacket p)
		{
			var bWarpToPlayer = p.Decode1();
			var nMapID = 0;

			var bExt = false; //Set by nItemID

			//TODO: ERROR LOGIC
			//SENDING THE CORRECT RESP

			if (bWarpToPlayer != 0)
			{
				var sPlayer = p.DecodeString();

				//cant find remote player ? DifficultToLocate

				var remote = MasterManager.CharacterPool.Get(sPlayer, false);

				if (remote?.ChannelID != Parent.ChannelID
					|| remote.Field.IsInstanced
					|| Parent.Field.IsInstanced)
				{
					Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.TargetNotExist, bExt));
					return false;
				}

				if (remote.Stats.nHP <= 0 || Parent.Stats.nHP <= 0)
				{
					Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.TargetDied, bExt));
					return false;
				}

				nMapID = remote.Field.MapId;
			}
			else
			{
				nMapID = p.Decode4();
			}

			if (Parent.Field.MapId == nMapID)
			{
				Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.AlreadyInMap, bExt));
				return false;
			}

			if (Parent.Field.Template.HasTeleportItemLimit()
				|| (Parent.Field.ParentInstance.CFieldMan.GetField(nMapID)?.Template.HasTeleportItemLimit() ?? true))
			{
				Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.TargetNotExist, bExt));
				return false;
			}

			if (!adwMapTransfer.Contains(nMapID))
			{
				Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.TargetNotExist, bExt));
				return false;
			}

			Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.Use, bExt));
			Parent.Action.SetField(nMapID, 0, 0);
			return true;
		}

		public void OnUpdateRequest(CInPacket p)
		{
			var nReq = (MapTransferReq)p.Decode1();
			var bExt = p.Decode1() != 0;

			var pTransfer = bExt ? adwMapTransferEx : adwMapTransfer;

			switch (nReq)
			{
				case MapTransferReq.RegisterList:
					{
						var nFieldID = Parent.Field.MapId;
						if (Parent.Field.Template.HasTeleportItemLimit() || !AddLocation(pTransfer, nFieldID))
						{
							Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.RegisterFail, bExt));
						}
						else
						{
							Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.RegisterList, bExt, pTransfer));
						}
						break;
					}
				case MapTransferReq.DeleteList:
					{
						int nFieldID = p.Decode4();
						RemoveLocation(pTransfer, nFieldID);

						Parent.SendPacket(CPacket.MapTransferResult(MapTransferRes.DeleteList, bExt, pTransfer));
						break;
					}
			}
		}

		private static bool AddLocation(int[] adwMapTransfers, int dwFieldID)
		{
			for (int i = 0; i < adwMapTransfers.Length; i++)
			{
				if (adwMapTransfers[i] == Constants.InvalidMap)
				{
					adwMapTransfers[i] = dwFieldID;
					return true;
				}
			}
			return false;
		}

		private static void RemoveLocation(int[] adwMapTransfers, int dwFieldID)
		{
			for (int i = 0; i < adwMapTransfers.Length; i++)
			{
				if (adwMapTransfers[i] == dwFieldID)
				{
					adwMapTransfers[i] = Constants.InvalidMap;
					return;
				}
			}
		}

		public void Encode(COutPacket p)
		{
			adwMapTransfer.ForEach(p.Encode4);
			adwMapTransferEx.ForEach(p.Encode4);
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM rebirth.map_transfer WHERE character_id = {Parent.dwId}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					if (r.Read())
					{
						adwMapTransfer = r["map_ids"] as int[];
						adwMapTransferEx = r["map_ids_ex"] as int[];
					}
					else
					{
						adwMapTransfer = Enumerable.Repeat(Constants.InvalidMap, RegLength).ToArray();
						adwMapTransferEx = Enumerable.Repeat(Constants.InvalidMap, VipLength).ToArray();
					}
				}
			}
		}

		public void SaveToDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sb = new StringBuilder();

				sb.AppendLine($"DELETE FROM rebirth.map_transfer WHERE character_id = {Parent.dwId};");
				sb.AppendLine($"INSERT INTO rebirth.map_transfer (character_id, map_ids, map_ids_ex) VALUES ({Parent.dwId}, @maps, @maps_ex)");

				using (var cmd =
					new NpgsqlCommand(sb.ToString(), conn))
				{
					cmd.Parameters.AddWithValue("maps", adwMapTransfer);
					cmd.Parameters.AddWithValue("maps_ex", adwMapTransferEx);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
