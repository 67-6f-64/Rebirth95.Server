using log4net;
using Npgsql;
using Rebirth.Characters;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;
using System;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Character;
using Rebirth.Provider.Template.Item.ItemOption;
using Rebirth.Tools;

namespace Rebirth.Entities.Item
{
	public class GW_ItemSlotEquip : GW_ItemSlotBase
	{
		public EquipTemplate EquipTemplate => Template as EquipTemplate;

		public static ILog Log = LogManager.GetLogger(typeof(GW_ItemSlotEquip));
		//private readonly bool logpotential = false;

		public byte CurrentUpgradeCount { get; set; }
		public byte RemainingUpgradeCount { get; set; }
		public short niSTR { get; set; }
		public short niDEX { get; set; }
		public short niINT { get; set; }
		public short niLUK { get; set; }
		public short niMaxHP { get; set; }
		public short niMaxMP { get; set; }
		public short niPAD { get; set; }
		public short niMAD { get; set; }
		public short niPDD { get; set; }
		public short niMDD { get; set; }
		public short niACC { get; set; }
		public short niEVA { get; set; }
		public short niCraft { get; set; }
		public short niSpeed { get; set; }
		public short niJump { get; set; }
		public ItemAttributeFlags nAttribute { get; set; }
		public byte nLevelUpType { get; set; }
		public byte nLevel { get; set; }
		public int nEXP { get; set; }
		public byte HammerUpgradeCount { get; set; } // nIUC
		public PotentialGradeCode nGrade { get; set; }
		public byte StarUpgradeCount { get; set; } // nCHUC

		public short nOption1 { get; set; }
		public short nOption2 { get; set; }
		public short nOption3 { get; set; }

		public ItemOptionLevelData[] nOptionData
			=> new[]
			{
				get_option_data(1),
				get_option_data(2),
				get_option_data(3),
			};

		private ItemOptionLevelData get_option_data(int nIdx)
		{
			short nOption;
			switch (nIdx)
			{
				case 1:
					nOption = nOption1;
					break;
				case 2:
					nOption = nOption1;
					break;
				case 3:
					nOption = nOption1;
					break;
				default:
					throw new ArgumentOutOfRangeException
						(nameof(nIdx), $"Unable to find option with index ({nIdx}). Proper range is 1-3.");
			}

			if (nOption > 0)
			{
				return MasterManager
					.ItemOptionTemplates[nOption]
					.LevelData
					.FirstOrDefault(i => i.nIdx == (int)Math.Floor(EquipTemplate.ReqLevel * 0.1f));
			}

			return null;
		}

		public bool HasHiddenPotential // or nGrade > 0 && nGrade < 5
			=> nGrade == PotentialGradeCode.Hidden_Rare
			|| nGrade == PotentialGradeCode.Hidden_Epic
			|| nGrade == PotentialGradeCode.Hidden_Unique;

		public bool HasVisiblePotential // or nGrade > 4
			=> nGrade == PotentialGradeCode.Visible_Rare
			|| nGrade == PotentialGradeCode.Visible_Epic
			|| nGrade == PotentialGradeCode.Visible_Unique;

		public short nSocket1 { get; set; }
		public short nSocket2 { get; set; }

		public DateTime tSealingLock { get; set; }

		public string sTitle { get; set; } = string.Empty;
		public int nDurability { get; set; } = -1;
		public long ftEquipped { get; set; } = Constants.PERMANENT; // TODO datetime ;)
		public int nPrevBonusExpRate { get; set; } = -1;

		public override string DbInsertString(int dwCharId, short nPOS)
		{
			var subKey = CreateDbSubKey(nPOS);

			return $"INSERT INTO {Constants.DB_All_World_Schema_Name}.char_inventory_equips "
				   + $"(character_id, "
				   + $"item_id, "
				   + $"item_date_expire, "
				   + $"item_upgrade_count, "
				   + $"item_remaining_upgrades, "
				   + $"item_str, "
				   + $"item_dex, "
				   + $"item_int, "
				   + $"item_luk, "
				   + $"item_mhp, "
				   + $"item_mmp, "
				   + $"item_pad, "
				   + $"item_mad, "
				   + $"item_pdd, "
				   + $"item_mdd, "
				   + $"item_acc, "
				   + $"item_eva, "
				   + $"item_craft, "
				   + $"item_speed, "
				   + $"item_jump, "
				   + $"item_attribute, "
				   + $"item_serial_number, "
				   + $"item_title, "
				   + $"item_level_type, "
				   + $"item_level, "
				   + $"item_exp, "
				   + $"item_durability, "
				   + $"item_hammer_count, "
				   + $"item_grade, "
				   + $"item_star_count, "
				   + $"item_option1, "
				   + $"item_option2, "
				   + $"item_option3, "
				   + $"item_socket1, "
				   + $"item_socket2, "
				   + $"inventory_slot, "
				   + $"item_cash_serial_number, "
				   + $"item_sealinglock) "
				   + $"VALUES ({dwCharId}, "
				   + $"{nItemID}, "
				   + $"'{tDateExpire.ToSqlTimeStamp()}', "
				   + $"{CurrentUpgradeCount}, "
				   + $"{RemainingUpgradeCount}, "
				   + $"{niSTR}, "
				   + $"{niDEX}, "
				   + $"{niINT}, "
				   + $"{niLUK}, "
				   + $"{niMaxHP}, "
				   + $"{niMaxMP}, "
				   + $"{niPAD}, "
				   + $"{niMAD}, "
				   + $"{niPDD}, "
				   + $"{niMDD}, "
				   + $"{niACC}, "
				   + $"{niEVA}, "
				   + $"{niCraft}, "
				   + $"{niSpeed}, "
				   + $"{niJump}, "
				   + $"{(short)nAttribute}, "
				   + $"{liSN}, "
				   + $"@title{subKey}, "
				   + $"{nLevelUpType}, "
				   + $"{nLevel}, "
				   + $"{nEXP}, "
				   + $"{nDurability}, "
				   + $"{HammerUpgradeCount}, "
				   + $"{(short)nGrade}, "
				   + $"{StarUpgradeCount}, "
				   + $"{nOption1}, "
				   + $"{nOption2}, "
				   + $"{nOption3}, "
				   + $"{nSocket1}, "
				   + $"{nSocket2}, "
				   + $"{nPOS}, "
				   + $"{liCashItemSN}, "
				   + $"'{tSealingLock.ToSqlTimeStamp()}');";
		}

		public GW_ItemSlotEquip(int nItemID) : base(nItemID) { nNumber = 1; }

		public override GW_ItemSlotBase DeepCopy()
		{
			return base.DeepCopy(); // todo
		}

		public override void RawEncode(COutPacket p, bool bFromCS = false)
		{
			p.Encode1(1);

			base.RawEncode(p, bFromCS);

			p.Encode1(RemainingUpgradeCount); // RUC
			p.Encode1(CurrentUpgradeCount); // CUC(K)
			p.Encode2(niSTR);
			p.Encode2(niDEX);
			p.Encode2(niINT);
			p.Encode2(niLUK);
			p.Encode2(niMaxHP);
			p.Encode2(niMaxMP);
			p.Encode2(niPAD);
			p.Encode2(niMAD);
			p.Encode2(niPDD);
			p.Encode2(niMDD);
			p.Encode2(niACC);
			p.Encode2(niEVA);
			p.Encode2(niCraft); // hands? i think
			p.Encode2(niSpeed);
			p.Encode2(niJump);
			p.EncodeString(sTitle);
			p.Encode2((short)nAttribute);

			p.Encode1(nLevelUpType);
			p.Encode1(nLevel);
			p.Encode4(nEXP);
			p.Encode4(nDurability);
			p.Encode4(HammerUpgradeCount); // IUC
			p.Encode1((byte)nGrade);
			p.Encode1(StarUpgradeCount); // CHUC
			p.Encode2(nOption1);
			p.Encode2(nOption2);
			p.Encode2(nOption3);
			p.Encode2(nSocket1);
			p.Encode2(nSocket2);

			if (liCashItemSN == 0)
				p.Encode8(liSN);

			p.Encode8(ftEquipped);
			p.Encode4(nPrevBonusExpRate);
		}

		/// <summary>
		/// ONLY CALL THIS WHEN INSERTING ITEMS FROM CHAR CREATION
		/// </summary>
		public void SaveToDB(int dwCharId, short nPOS, bool bTempInv = false)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(DbInsertString(dwCharId, nPOS), conn))
				{
					cmd.Parameters.AddWithValue($"title{CreateDbSubKey(nPOS)}", sTitle);
					cmd.ExecuteNonQuery();
				}
			}
		}

		public COutPacket ShowItemUpgradeEffect(Character player, bool success, bool cursed, bool whitescroll)
		{
			var p = new COutPacket(SendOps.LP_UserItemUpgradeEffect);
			p.Encode4(player.dwId);
			p.Encode1(success); // bSuccess
			p.Encode1(cursed); // bCursed
			p.Encode1(0); // bEnchantSkill
			p.Encode4(0); // nEnchantCategory
			p.Encode1(whitescroll); // white scroll used
			p.Encode1(0); // bRecoverable
			return p;
		}

		// hyper = equip enhance = stars
		public COutPacket ShowItemHyperUpgradeEffect(Character player, bool success, bool enchant)
		{
			var p = new COutPacket(SendOps.LP_UserItemHyperUpgradeEffect);
			p.Encode4(player.dwId);
			p.Encode1(success); // bSuccess
			p.Encode1(!success); // bCursed
			p.Encode1(0); // bEnchantSkill
			p.Encode4(0); // nEnchantCategory
			return p;
		}

		// potential
		public COutPacket ShowItemOptionUpgradeEffect(Character player, bool success, bool enchant)
		{
			var p = new COutPacket(SendOps.LP_UserItemOptionUpgradeEffect);
			p.Encode4(player.dwId);
			p.Encode1(success); // bSuccess
			p.Encode1(!success); // bCursed
			p.Encode1(0); // bEnchantSkill
			p.Encode4(0); // nEnchantCategory
			return p;
		}

		// clean slate scroll?
		public COutPacket ShowRecoverUpgradeCountEffect(Character player, bool success, bool cursed, bool enchant)
		{
			var p = new COutPacket(); // ?? need opcode from PDB zzz
			p.Encode4(player.dwId);
			// empty??
			return p;
		}

		// magnifying glass response packet
		public COutPacket ShowItemReleaseEffect(int charId, short equipSlot)
		{
			var p = new COutPacket(SendOps.LP_UserItemReleaseEffect);
			p.Encode4(charId);
			p.Encode2(equipSlot);
			return p;
		}

		/**
         * Miracle cube response packet.
         * Success false shows inventory full popup.
         */
		public COutPacket ShowItemUnreleaseEffect(int charId, bool success)
		{
			var p = new COutPacket(SendOps.LP_UserItemUnreleaseEffect);
			p.Encode4(charId);
			p.Encode1(success);
			return p;
		}

		public COutPacket ItemUpgradeResult(GoldHammer nReturnResult, GoldHammer nResult)
		{
			// CUIItemUpgrade::OnItemUpgradeResult
			var p = new COutPacket(SendOps.LP_ItemUpgradeResult);

			//v2->m_nReturnResult = CInPacket::Decode1(iPacket);
			p.Encode1((byte)nReturnResult);
			p.Encode4((int)nResult);
			if (nResult == GoldHammer.ReturnResult_ItemUpgradeSuccess)
			{
				p.Encode4(HammerUpgradeCount); // upgrades left
			}

			// (0x41 && 0x0) -> show upgrade message
			// (0x3D && 0x0) -> finishe process

			return p;
		}

	}
}
