using Rebirth.Field.FieldPools;
using Rebirth.Network;
using System;
using Rebirth.Provider.Template.Item;
using Rebirth.Provider.Template.Item.Cash;

namespace Rebirth.Entities.Item
{
	public class GW_ItemSlotPet : GW_ItemSlotBundle
	{
		public PetItemTemplate PetTemplate => Template as PetItemTemplate;
		public string sPetName { get; set; } = "";
		public byte nLevel { get; set; }
		public short nTameness { get; set; }
		public byte nRepleteness { get; set; }
		public short nPetAttribute { get; set; }
		public short usPetSkill { get; set; } //Bitflag for skills - THANKS SWORDIEMEN <3 !!
		public int nRemainLife { get; set; }
		public DateTime tDateDead { get; set; } = DateTime.MaxValue;

		public override int CreateDbSubKey(short nPOS) => nPOS << 6;

		public override string DbInsertString(int dwCharId, short nPOS)
			=> $"INSERT INTO {Constants.DB_All_World_Schema_Name}.inventory_pets "
				+ $"(item_serial_number, "
				+ $"pet_level, "
				+ $"pet_closeness, "
				+ $"pet_fullness, "
				+ $"pet_name, "
				+ $"pet_remaining_life, "
				+ $"pet_skill, "
				+ $"pet_attribute, "
				+ $"character_id, "
				+ $"pet_date_dead) "
				+ $"VALUES ({liSN}, "
				+ $"{nLevel}, "
				+ $"{nTameness}, "
				+ $"{nRepleteness}, "
				+ $"@petname{CreateDbSubKey(nPOS)}, "
				+ $"{nRemainLife}, "
				+ $"{usPetSkill}, "
				+ $"{nPetAttribute}, "
				+ $"{dwCharId}, "
				+ $"'{tDateDead.ToSqlTimeStamp()}');" +
			$"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_inventory_items " +
			$"(character_id, item_id, inventory_slot, item_quantity, item_date_expire, item_serial_number, item_cash_serial_number) VALUES " +
			$"({dwCharId}, {nItemID}, {nPOS}, 1, '{tDateExpire.ToSqlTimeStamp()}', {liSN}, {liCashItemSN});";

		public GW_ItemSlotPet(int nItemID) : base(nItemID)
		{
			nNumber = 1;
		}

		public override void RawEncode(COutPacket p, bool bFromCS = false)
		{
			p.Encode1(3);

			p.Encode4(nItemID);

			var bCashItem = liCashItemSN != 0;

			p.Encode1(bCashItem);

			if (bCashItem) // arent all pets cash items?? o__o
				p.Encode8(liCashItemSN);

			p.EncodeDateTime(tDateExpire);

			p.EncodeStringFixed(sPetName, 13);
			p.Encode1(nLevel);
			p.Encode2(nTameness);
			p.Encode1(nRepleteness);
			if (tDateDead == DateTime.MaxValue)
			{
				p.Encode8(0);
			}
			else
			{
				p.EncodeDateTime(tDateDead);
			}
			p.Encode2(nPetAttribute);
			p.Encode2(usPetSkill);
			p.Encode4(nRemainLife);
			p.Encode2(nAttribute);
		}
	}
}
