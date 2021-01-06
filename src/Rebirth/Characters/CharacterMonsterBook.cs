using Npgsql;
using Rebirth.Entities;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rebirth.Provider.Template.Item.Consume;

namespace Rebirth.Characters
{
	public class CharacterMonsterBook : List<GW_MonsterBookCard>
	{
		// TODO make this a keyed collection or something
		public GW_MonsterBookCover Cover { get; set; }

		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		public CharacterMonsterBook(int parent)
		{
			//Temp for testing
			Cover = new GW_MonsterBookCover();

			dwParentID = parent;
		}

		public void Dispose()
		{
			Cover = null;
			Clear();
		}

		public void EncodeCover(COutPacket p)
		{
			p.Encode4(Cover.nCardID);//nMonsterBookCoverID
			p.Encode1(0); //IDK
		}

		public void EncodeCards(COutPacket p)
		{
			p.Encode2((short)Count);

			foreach (var card in this)
			{
				p.Encode2(card.usCardID);
				p.Encode1(card.nCardCount);
			}
		}

		public bool SpecialCard(int cardId)
		{
			return cardId / 1000 >= 2388;
		}

		public void AddCard(int cardId)
		{
			if (MasterManager.ItemTemplate(cardId) is ConsumeItemTemplate consumeTemplate)
			{
				if (consumeTemplate.MonsterBook)
				{
					Add(new GW_MonsterBookCard
					{
						usCardID = (short)(cardId - 2380000),
						nCardCount = 1
					});
				}
			}
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.monsterbook " +
												   $"WHERE char_id= {dwParentID}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						foreach (var cardId in (int[])r["cards"])
						{
							AddCard(2380000 + cardId);
						}
					}
				}
			}
		}

		public void SaveToDB()
		{
			if (Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"DELETE FROM {Constants.DB_All_World_Schema_Name}.monsterbook " +
												   $"WHERE char_id = {dwParentID}", conn))
					cmd.ExecuteNonQuery();

				using (var cmd = new NpgsqlCommand($"INSERT INTO {Constants.DB_All_World_Schema_Name}.monsterbook (char_id, cards) " +
												   $"VALUES ({dwParentID}, @cards)", conn))
				{
					var cards = new List<int>();
					ForEach(c => cards.Add(c.usCardID));
					cmd.Parameters.AddWithValue("cards", cards.ToArray());
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}