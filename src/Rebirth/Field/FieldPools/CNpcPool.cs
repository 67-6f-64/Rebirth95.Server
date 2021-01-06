using Npgsql;
using Rebirth.Characters;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using Rebirth.Provider.Template.Map;

namespace Rebirth.Field.FieldPools
{
	public class CNpcPool : CObjectPool<CNpc>
	{
		public CNpcPool(CField parentField)
			: base(parentField) { }

		public bool RemoveFirstByID(int nNpcId)
		{
			foreach (var npc in new List<CNpc>(this))
			{
				if (npc.TemplateId == nNpcId)
				{
					return Remove(npc);
				}
			}

			return false;
		}

		protected override void InsertItem(int index, CNpc item)
		{
			base.InsertItem(index, item);

			Field.Broadcast(item.MakeEnterFieldPacket());
		}

		protected override void RemoveItem(int index)
		{
			var npc = GetAtIndex(index);

			if (npc != null)
			{
				Field.Broadcast(npc.MakeLeaveFieldPacket());
			}

			base.RemoveItem(index);
		}

		public void Move(Character pUser, CInPacket p)
		{
			var dwNpcId = p.Decode4();
			var item = this[dwNpcId];

			if (item == null) return;
			if (item.Controller != pUser) return; //TODO: Validate you require controller to move a npc

			var act1 = p.Decode1();
			var act2 = p.Decode1();

			byte[] aMovePath = null;

			if (p.Available > 0)
			{
				aMovePath = p.DecodeBuffer(p.Available);
			}

			Field.Broadcast(CPacket.NpcMove(item, act1, act2, aMovePath));
		}

		public override void Load(int mapId)
		{
			Clear();

			var life = MasterManager.MapTemplates[mapId]?.Life;

			if (life != null)
			{
				foreach (var n in life)
				{
					if (n.Type != "n") continue;

					if (MasterManager.NpcTemplates[n.TemplateId]?.MapleTV ?? false) continue;

					var entry = new CNpc(n.TemplateId)
					{
						Cy = n.CY,
						F = n.F,
						Foothold = n.Foothold,
						X = n.X,
						Y = n.Y,
						Rx0 = n.RX0,
						Rx1 = n.RX1
					};
					Add(entry);
				}
			}

			// load custom NPCs from database
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.custom_field_life WHERE field_id = {Field.MapId}", conn))
				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						if ((string)r[2] != "npc") continue;

						var nTemplateId = (int)r[3];

						var entry = new CNpc(nTemplateId)
						{
							Cy = (short)r[12],
							F = false,
							Foothold = (short)r[7],
							X = (short)r[5],
							Y = (short)r[6],
							Rx0 = (short)r[8],
							Rx1 = (short)r[9]
						};
						Add(entry);
					}
				}
			}
		}
	}
}
