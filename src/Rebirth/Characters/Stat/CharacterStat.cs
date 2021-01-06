using System;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Rebirth.Common.GameLogic;
using Rebirth.Network;

namespace Rebirth.Characters.Stat
{
	public sealed class CharacterStat
	{
		public PassiveSkillData PassiveSkillData { get; }
		public SecondaryStatValues SecondaryStats { get; }

		public int dwCharacterID { get; set; }
		public string sCharacterName { get; set; }
		public byte nGender { get; set; }
		public byte nSkin { get; set; }
		public int nFace { get; set; }
		public int nHair { get; set; }
		public long[] aliPetLockerSN { get; set; }
		public byte nLevel { get; set; }
		public short nJob { get; set; }
		public short nSTR { get; set; }
		public short nDEX { get; set; }
		public short nINT { get; set; }
		public short nLUK { get; set; }
		public int nHP { get; set; }
		public int nMHP { get; set; }
		public int nMP { get; set; }
		public int nMMP { get; set; }
		public short nAP { get; set; }
		public short nSP
		{
			get => (short)extendSP[JobLogic.GetExtendedSPIndexByJob(nJob)];
			set => extendSP[JobLogic.GetExtendedSPIndexByJob(nJob)] = value;
		}
		public int nEXP { get; set; }
		public short nPOP { get; set; }
		public int nMoney { get; set; }
		public int nTempEXP { get; set; }
		public int[] extendSP { get; set; } //public ExtendSP extendSP;
		public int dwPosMap { get; set; }
		public byte nPortal { get; set; }
		public int nPlaytime { get; set; }
		public short nSubJob { get; set; }
		public short nFriendMax { get; set; }
		public long MerchantMesos { get; set; }
		public byte WorldID { get; set; }
		public byte Channel { get; set; }
		public int[] WishList { get; set; }
		public DateTime tLastFame { get; set; }
		public DateTime tDateCreated { get; set; }
		public DateTime tLastLogin { get; set; }

		public CharacterStat(int charId)
		{
			dwCharacterID = charId;
			aliPetLockerSN = new long[3];
			extendSP = new int[11]; //new ExtendSP();
			PassiveSkillData = new PassiveSkillData();
			SecondaryStats = new SecondaryStatValues();
		}

		public int GetSpBySkillID(int nSkillID)
		{
			return extendSP[JobLogic.GetExtendedSPIndexBySkill(nSkillID)];
		}

		public void Encode(COutPacket p)
		{
			p.Encode4(dwCharacterID);
			p.EncodeStringFixed(sCharacterName, 13);
			p.Encode1(nGender);
			p.Encode1(nSkin);
			p.Encode4(nFace);
			p.Encode4(nHair);

			aliPetLockerSN.ForEach(p.Encode8);

			p.Encode1(nLevel);
			p.Encode2(nJob);
			p.Encode2(nSTR);
			p.Encode2(nDEX);
			p.Encode2(nINT);
			p.Encode2(nLUK);
			p.Encode4(nHP);
			p.Encode4(nMHP);
			p.Encode4(nMP);
			p.Encode4(nMMP);
			p.Encode2(nAP);

			if (!JobLogic.IsExtendedSPJob(nJob))
			{
				p.Encode2(nSP);
			}
			else
			{
				var count = (byte)extendSP.Length;
				p.Encode1(count);

				for (int i = 0; i < count; i++)
				{
					p.Encode1((byte)i);
					p.Encode1((byte)extendSP[i]);
				}

				//extendSP.Encode(p);
			}

			p.Encode4(nEXP);
			p.Encode2(nPOP);
			p.Encode4(nTempEXP); //Gachapon
			p.Encode4(dwPosMap);
			p.Encode1(nPortal);
			p.Encode4(nPlaytime);
			p.Encode2(nSubJob);
		}

		public void EncodeMoney(COutPacket p)
		{
			p.Encode4(nMoney);
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * " +
												   $"FROM {Constants.DB_All_World_Schema_Name}.characters " +
												   $"WHERE id = {dwCharacterID}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					if (r.Read())
					{
						sCharacterName = Convert.ToString(r["name"]);
						nGender = Convert.ToByte(r["gender"]);
						nSkin = Convert.ToByte(r["skin"]);
						nFace = Convert.ToInt32(r["face"]);
						nHair = Convert.ToInt32(r["hair"]);
						nLevel = Convert.ToByte(r["level"]);
						nJob = Convert.ToInt16(r["job"]);
						nSTR = Convert.ToInt16(r["str"]);
						nDEX = Convert.ToInt16(r["dex"]);
						nINT = Convert.ToInt16(r["int"]);
						nLUK = Convert.ToInt16(r["luk"]);
						nHP = Convert.ToInt32(r["hp"]);
						nMHP = Convert.ToInt32(r["m_hp"]);
						nMP = Convert.ToInt32(r["mp"]);
						nMMP = Convert.ToInt32(r["m_mp"]);
						nAP = Convert.ToInt16(r["ap"]);
						nEXP = Convert.ToInt32(r["exp"]);
						nMoney = Convert.ToInt32(r["money"]);
						nTempEXP = Convert.ToInt32(r["temp_exp"]);
						dwPosMap = Convert.ToInt32(r["pos_map"]);
						nPortal = Convert.ToByte(r["portal"]);
						nPlaytime = Convert.ToInt32(r["play_time"]);
						nSubJob = Convert.ToInt16(r["sub_job"]);
						nPOP = Convert.ToInt16(r["fame"]);
						WorldID = Convert.ToByte(r["world_id"]);
						nFriendMax = Convert.ToInt16(r["friend_list_max_size"]);
						MerchantMesos = Convert.ToInt64(r["merchant_mesos"]);
						Channel = Convert.ToByte(r["char_last_channel"]);

						tLastFame = Convert.ToDateTime(r["char_last_fame"]);
						tLastLogin = Convert.ToDateTime(r["char_last_login"]);
						tDateCreated = Convert.ToDateTime(r["char_created_on"]);

						extendSP = r["extend_sp"] as int[];
						WishList = r["char_wish_list"] as int[];
						aliPetLockerSN = r["pet_locker"] as long[];
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

				// don't forget to update the INSERT statement in CharacterEntry.cs also

				sb.AppendLine($"UPDATE {Constants.DB_All_World_Schema_Name}.characters SET");
				sb.AppendLine($"gender = {nGender},");
				sb.AppendLine($"skin = {nSkin},");
				sb.AppendLine($"face = {nFace},");
				sb.AppendLine($"hair = {nHair},");
				sb.AppendLine($"level = {nLevel},");
				sb.AppendLine($"job = {nJob},");
				sb.AppendLine($"sub_job = {nSubJob},");
				sb.AppendLine($"str = {nSTR},");
				sb.AppendLine($"dex = {nDEX},");
				sb.AppendLine($"int = {nINT},");
				sb.AppendLine($"luk = {nLUK},");
				sb.AppendLine($"hp = {nHP},");
				sb.AppendLine($"m_hp = {nMHP},");
				sb.AppendLine($"mp = {nMP},");
				sb.AppendLine($"m_mp = {nMMP},");
				sb.AppendLine($"ap = {nAP},");
				sb.AppendLine($"fame = {nPOP},");
				sb.AppendLine($"exp = {nEXP},");
				sb.AppendLine($"money = {nMoney},");
				sb.AppendLine($"temp_exp = {nTempEXP},");
				sb.AppendLine($"pos_map = {dwPosMap},");
				sb.AppendLine($"portal = {nPortal},");
				sb.AppendLine($"play_time = {nPlaytime},");
				sb.AppendLine($"friend_list_max_size = {nFriendMax},");
				sb.AppendLine($"merchant_mesos = {MerchantMesos},");
				sb.AppendLine($"char_last_channel = {Channel},");

				sb.AppendLine($"name = @name,");
				sb.AppendLine($"char_last_fame = @lastfame,");
				sb.AppendLine($"char_created_on = @createdon,");
				sb.AppendLine($"char_last_login = @lastlogin,");

				sb.AppendLine($"char_wish_list = @wishlist,");
				sb.AppendLine($"pet_locker = @petlocker,");
				sb.AppendLine($"extend_sp = @extendsp");

				sb.AppendLine($"WHERE id = {dwCharacterID};");

				using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
				{
					cmd.Parameters.AddWithValue("name", sCharacterName);

					cmd.Parameters.AddWithValue("lastfame", tLastFame);
					cmd.Parameters.AddWithValue("createdon", tDateCreated);
					cmd.Parameters.AddWithValue("lastlogin", tLastLogin);

					cmd.Parameters.AddWithValue("wishlist", WishList);
					cmd.Parameters.AddWithValue("petlocker", aliPetLockerSN);
					cmd.Parameters.AddWithValue("extendsp", extendSP);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
