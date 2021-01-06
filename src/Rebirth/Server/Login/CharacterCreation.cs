    using Npgsql;
using Rebirth.Client;
using Rebirth.Entities;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
    using Rebirth.Characters.Stat;
    using Rebirth.Entities.Item;
    using Rebirth.Entities.PlayerData;

    namespace Rebirth.Server.Login
{
    public class CharacterCreation
    {
        // Etc.wz
        // PremiumCharMale/PremiumCharFemale = Explorer/Cygnus - 0/1
        // OrientCharMale/OrientCharFemale = Aran - 2
        // EvanCharMale/EvanCharFemale = Evan - 3
        // 0 = Face
        // 1 = Hair
        // 2 = Hair Color
        // 3 = Skin
        // 4 = Top
        // 5 = Pants
        // 6 = Boots
        // 7 = Weapon

        private static int[] femaleHair = { 31000, 31010, 31030, 31040, 31050, 31100, 31120, 31240 },
                femaleHairColor = { 0, 2, 3, 7 },
                femaleSkin = { 0, 1, 2, 3 };

        private static int[] maleHair = { 30000, 30020, 30030, 30050, 30120, 30270, 30310, 30670 },
                maleHairColor = { 0, 2, 3, 7 },
                maleSkin = { 0, 1, 2, 3 };

        public static void CreateCharacter(WvsLoginClient c, CInPacket p)
        {
            var newChar = Default();

            // create explorer:   Recv [CP_CreateNewCharacter] 16 00 06 00 70 6F 6F 70 69 65 01 00 00 00 00 00 24 4E 00 00 [4E 75 00 00] [03 00 00 00] [03 00 00 00] [86 DE 0F 00] [2F 2D 10 00] [85 5B 10 00] [8B DE 13 00] [00]
            // create resistance: Recv [CP_CreateNewCharacter] 16 00 06 00 64 6F 6F 64 6F 6F 00 00 00 00 00 00 84 4E 00 00 [30 75 00 00] [07 00 00 00] [01 00 00 00] [47 06 10 00] [00 00 00 00] [74 5D 10 00] [8C DE 13 00] [00]

            newChar.Stats.sCharacterName = p.DecodeString();

            var job = (short)p.Decode4();
            newChar.Stats.nJob = GameConstants.GetRealJobFromCreation(job);

            bool subJob = p.Decode2() > 0; //whether dual blade = 1 or adventurer = 0
            newChar.Stats.nSubJob = (short)(subJob ? 1 : 0); // doing it this way to reduce potential packet editing fuckery
            
            newChar.Stats.nFace = p.Decode4();
            var hairColor = p.Decode4();
            newChar.Stats.nHair = p.Decode4() + hairColor;
            newChar.Stats.nSkin = (byte)p.Decode4();

            var top = p.Decode4();
            var bottom = p.Decode4();
            var shoes = p.Decode4();
            var weapon = p.Decode4();

            newChar.Stats.nGender = p.Decode1();

            if (newChar.Stats.nSubJob == 1)
            {
                newChar.Stats.nJob = 400; // thief job
            }

            newChar.Stats.dwPosMap = GameConstants.GetStartingMap(newChar.Stats.nJob, newChar.Stats.nSubJob);

            newChar.Look.CopyStats(newChar.Stats);

            // TODO validate these against wz files
            newChar.Look.aEquip[5] = top;
            newChar.Look.aEquip[6] = bottom;
            newChar.Look.aEquip[7] = shoes;
            newChar.Look.aEquip[11] = weapon;

            newChar.Insert(c.Account.ID);

            // create empty entries in mapping tables
            InitDefaultMappings(newChar.Stats.dwCharacterID);

            if (newChar.Stats.dwCharacterID < 0)
            {
                c.SendPacket(CreateCharacterPacket(false, null));
            }
            else
            {
	            // add equips  // todo figure out if overalls have their own slot ID
                MasterManager.CreateNormalStatEquip(top)
                    .SaveToDB(newChar.Stats.dwCharacterID, (short)(bottom > 0 ? -5 : -5));

                if (bottom > 0) // will be zero if its a champ that starts with an overall
                {
	                MasterManager.CreateNormalStatEquip(bottom)
                        .SaveToDB(newChar.Stats.dwCharacterID, -6);
                }

                MasterManager.CreateNormalStatEquip(shoes)
                    .SaveToDB(newChar.Stats.dwCharacterID, -7);

                MasterManager.CreateNormalStatEquip(weapon)
                    .SaveToDB(newChar.Stats.dwCharacterID, -11);

                var nWhitePot = 2000002;
                var nManaPot = 2000006;
                var nPotQuantity = (short)100;

                var item1 = MasterManager.CreateItem(nWhitePot) as GW_ItemSlotBundle;
                item1.nNumber = nPotQuantity;

                item1.SaveToDB(newChar.Stats.dwCharacterID, 1);

                var item2 = MasterManager.CreateItem(nManaPot) as GW_ItemSlotBundle;
                item2.nNumber = nPotQuantity;

                item2.SaveToDB(newChar.Stats.dwCharacterID, 2);

                c.SendPacket(CreateCharacterPacket(true, newChar));
            }
        }

        public static void DeleteCharacter(WvsLoginClient c, CInPacket p)
        {
            var szSPW = p.DecodeString();
            var dwCharID = p.Decode4();

            // TODO check for guild ownership and family membership

            byte nRetCode = 0x09; //Error
            var aCharList = c.Account.LoadCharIdList();
            
            if (aCharList.Contains(dwCharID))
            {
                using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand($"DELETE FROM {Constants.DB_All_World_Schema_Name}.characters WHERE id = {dwCharID}", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                nRetCode = 0; //Success
            }

            c.SendPacket(CPacket.CLogin.DeleteCharacter(dwCharID, nRetCode));
        }

        private static CharacterEntry Default()
        {
            var rv = new CharacterEntry
            {
                Stats = new CharacterStat(-1),
                Look = new AvatarLook()
            };

            rv.Stats.nMHP = 50;
            rv.Stats.nHP = 50;
            rv.Stats.nMMP = 50;
            rv.Stats.nMP = 50;

            rv.Stats.nSTR = 4; // starter npc will give ap to set correct values here
            rv.Stats.nDEX = 4;
            rv.Stats.nINT = 4;
            rv.Stats.nLUK = 4;

            rv.Stats.nLevel = 1;
            rv.Stats.nFriendMax = 20;

            rv.Stats.tLastFame = DateTime.Now.AddDays(-1);
            rv.Stats.nPOP = 0;
            rv.Stats.nEXP = 0;
            rv.Stats.nAP = 0;
            rv.Stats.nSP = 1;

            rv.Stats.nPortal = 0;
            rv.Stats.nPlaytime = 0;
            rv.Look.nWeaponStickerID = 0;

            return rv;
        }

        private static void InitDefaultMappings(int charId)
        {
            using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"INSERT INTO {Constants.DB_All_World_Schema_Name}.key_map VALUES ({charId}, @keys, @types, @actions);" +
                                                   $"INSERT INTO {Constants.DB_All_World_Schema_Name}.monsterbook VALUES ({charId}, @cards);", conn))
                {
	                // default key mapping
                    int[] key = { 18, 65, 2, 23, 3, 4, 5, 6, 16, 17, 19, 25, 26, 27, 31, 34, 35, 37, 38, 40, 43, 44, 45, 46, 50, 56, 59, 60, 61, 62, 63, 64, 57, 48, 29, 7, 24, 33, 41 };
                    int[] type = { 4, 6, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 4, 4, 5, 6, 6, 6, 6, 6, 6, 5, 4, 5, 4, 4, 4, 4 };
                    int[] action = { 0, 106, 10, 1, 12, 13, 18, 24, 8, 5, 4, 19, 14, 15, 2, 17, 11, 3, 20, 16, 9, 50, 51, 6, 7, 53, 100, 101, 102, 103, 104, 105, 54, 22, 52, 21, 25, 26, 23 };

                    // keymap
                    cmd.Parameters.AddWithValue("keys", key);
                    cmd.Parameters.AddWithValue("types", type);
                    cmd.Parameters.AddWithValue("actions", action);

                    // monsterbook
                    cmd.Parameters.AddWithValue("cards", new int[0]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CharacterNameInUse(WvsLoginClient c, CInPacket p)
        {
            bool retVal = false;
            var charName = p.DecodeString();

            using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.characters WHERE name = (@name)", conn))
                {
                    cmd.Parameters.AddWithValue("name", charName);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            retVal = true; // values to read means entry exists
                            break;
                        }
                    }
                }
            }

            c.SendPacket(CheckDuplicatedIDResult(charName, retVal));
        }


        private static COutPacket CreateCharacterPacket(bool worked, CharacterEntry c)
        {
            var p = new COutPacket(SendOps.LP_CreateNewCharacterResult);
            p.Encode1((byte)(worked ? 0 : 1));

            if (worked)
                c.Encode(p);

            return p;
        }

        private static COutPacket CheckDuplicatedIDResult(string name, bool nameTaken)
        {
            var p = new COutPacket(SendOps.LP_CheckDuplicatedIDResult);
            p.EncodeString(name);
            p.Encode1(nameTaken);
            return p;
        }
    }
}
