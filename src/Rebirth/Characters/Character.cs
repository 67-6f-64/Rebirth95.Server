using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Npgsql;
using Rebirth.Characters.Combat;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Quest;
using Rebirth.Characters.Skill;
using Rebirth.Characters.Stat;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Entities;
using Rebirth.Entities.PlayerData;
using Rebirth.Field;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.MiniRoom;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Provider.Template.Skill;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth.Characters
{
	public sealed class Character : CFieldObj
	{
		public static ILog Log = LogManager.GetLogger(typeof(Character));

		public bool Initialized { get; private set; }

		public WvsGameClient Socket { get; private set; }
		public Account Account => Socket.Account;

		public CRand32 m_RndActionMan { get; private set; }
		public CalcDamage CalcDamage { get; private set; }

		public override int dwId => Stats.dwCharacterID;

		public CharacterStat Stats { get; private set; }
		public BasicStat BasicStats { get; private set; }

		public DamageTracker DamageTracker { get; private set; }

		public CharacterQuests Quests { get; private set; }
		public CharacterStatisticsTracker StatisticsTracker { get; private set; }

		public CharacterInventoryEquips aDragonEquipped { get; private set; }
		public CharacterInventoryEquips aMechanicEquipped { get; private set; }

		public CharacterInventoryEquips EquippedInventoryNormal { get; private set; }
		public CharacterInventoryEquips EquippedInventoryCash { get; private set; }
		public CharacterInventoryEquips InventoryEquip { get; private set; }
		public CharacterInventoryItems InventoryConsume { get; private set; }
		public CharacterInventoryItems InventoryInstall { get; private set; }
		public CharacterInventoryItems InventoryEtc { get; private set; }
		public CharacterInventoryItems InventoryCash { get; private set; }

		public CharacterRingInfo RingInfo { get; private set; }

		public CharacterCooldowns Cooldowns { get; private set; }
		public CharacterKeyMap FuncKeys { get; private set; }
		public CharacterSkills Skills { get; private set; }
		public CharacterBuffs Buffs { get; private set; }
		public CharacterMacros Macros { get; private set; }
		public CharacterMapTransfers Teleports { get; set; }

		public CharacterMonsterBook MonsterBook { get; private set; }
		public CharacterFriends Friends { get; private set; }
		public CharacterPets Pets { get; private set; }
		public CharacterTamingMob TamingMob { get; private set; }

		public CharacterActions Action { get; private set; }
		public CharacterCombat Combat { get; private set; }

		public CharacterModifiers Modify { get; private set; }

		public ForcedStats ForcedStats { get; private set; }

		public CDragon Dragon { get; set; }

		//Custom Begin | TODO: Custom class to hold all custom stuff
		public int nDamageSkin { get; set; }
		//Custom End

		public int nActivePortableChairID { get; set; }
		public int nActiveEffectItemID { get; set; }
		public int nCompletedSetItemID { get; set; }

		public string sADBoard { get; set; } = "";
		public Party Party => MasterManager.PartyPool.GetParty(this); // TODO store party ID here instead
		public int dwPartyID { get; set; }
		public int nCombatOrders { get; set; }
		public string sLinkedCharacter { get; set; } = string.Empty;
		public byte nLinkedCharacterLevel { get; set; }
		public GW_WildhunterInfo WildHunterInfo { get; private set; }
		public int nMonsterBookCoverID { get; set; }
		public int nMonsterCardNormal { get; set; }
		public int nMonsterCardSpecial { get; set; }
		public CMiniRoomBase CurMiniRoom { get; set; } // used to track which hired merchant a player is in
		public int m_dwSwallowMobID { get; set; }
		public int[] Wishlist => Stats.WishList;
		public byte ChannelID => Stats.Channel;
		public byte ChannelNumber => (byte)(Stats.Channel + 1);

		//public int nRegenHP = 0; // beginner regen skill, i do this in CharacterBuffs.Update()
		public int nRegenMP = 0; // warrior mp recovery passive skill

		public long nTotalSecondsOnline => StatisticsTracker.nSecondsOnline + Stats.tLastLogin.SecondsSinceStart();

		public DateTime tLastRegen { get; set; }
		public DateTime tPortableChairSittingTime { get; set; }
		public DateTime tLastCharacterHPInc { get; set; }

		private DateTime tLastItemExpirationCheck;

		public int nPreparedSkill { get; set; }
		public bool m_bNextAttackCritical { get; set; }

		public int DojoPoints
		{
			get => Convert.ToInt32(Quests.QuestRecordOrDefault(QuestConstants.DOJO_POINTS_QID, "0"));
			set => Quests.UpdateQuestRecordInternal(QuestConstants.DOJO_POINTS_QID, value.ToString());
		}

		public int BossPoints
		{
			get => Convert.ToInt32(Quests.QuestRecordOrDefault(QuestConstants.BOSS_POINTS_QID, "0"));
			set => Quests.UpdateQuestRecordInternal(QuestConstants.BOSS_POINTS_QID, value.ToString());
		}

		public int EventPoints
		{
			get => Convert.ToInt32(Quests.QuestRecordOrDefault(QuestConstants.EVENT_POINTS_QID, "0"));
			set => Quests.UpdateQuestRecordInternal(QuestConstants.EVENT_POINTS_QID, value.ToString());
		}

		public Character(int charId)
		{
			var timer = new Stopwatch();
			timer.Start();

			DamageTracker = new DamageTracker(this);
			Stats = new CharacterStat(charId);
			BasicStats = new BasicStat();

			InventoryConsume = new CharacterInventoryItems(InventoryType.Consume);
			InventoryEtc = new CharacterInventoryItems(InventoryType.Etc);
			InventoryInstall = new CharacterInventoryItems(InventoryType.Install);
			InventoryCash = new CharacterInventoryItems(InventoryType.Cash);
			InventoryEquip = new CharacterInventoryEquips();

			EquippedInventoryNormal = new CharacterInventoryEquips();
			EquippedInventoryCash = new CharacterInventoryEquips();

			MonsterBook = new CharacterMonsterBook(dwId);
			Skills = new CharacterSkills(this);
			Friends = new CharacterFriends(this);

			FuncKeys = new CharacterKeyMap(dwId);

			aDragonEquipped = new CharacterInventoryEquips();
			aMechanicEquipped = new CharacterInventoryEquips();

			Macros = new CharacterMacros(this);
			Teleports = new CharacterMapTransfers(this);

			Pets = new CharacterPets(dwId);

			Combat = new CharacterCombat(dwId);

			Action = new CharacterActions(dwId);
			Modify = new CharacterModifiers(dwId);

			ForcedStats = new ForcedStats();
			Quests = new CharacterQuests(dwId);

			WildHunterInfo = new GW_WildhunterInfo(dwId);
			TamingMob = new CharacterTamingMob(this);

			RingInfo = new CharacterRingInfo(this);

			m_RndActionMan = new CRand32();
			CalcDamage = new CalcDamage();

			StatisticsTracker = new CharacterStatisticsTracker(this);

			Load();

			timer.Stop();
			Log.Debug("Ms to create char object: " + timer.ElapsedMilliseconds);
		}

		private void Load()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand(
					$"SELECT * FROM {Constants.DB_All_World_Schema_Name}.inventory_slot_count WHERE character_id = {dwId};",
					conn))
				using (var r = cmd.ExecuteReader())
				{
					if (r.Read()) // default values if nothing is returned from db
					{
						InventoryEquip.SlotLimit = Convert.ToByte(r["inventory_equip"]);
						InventoryConsume.SlotLimit = Convert.ToByte(r["inventory_consume"]);
						InventoryEtc.SlotLimit = Convert.ToByte(r["inventory_etc"]);
						InventoryInstall.SlotLimit = Convert.ToByte(r["inventory_install"]);
					}

					InventoryCash.SlotLimit = 96; //Convert.ToByte(r["inventory_cash"]);
				}
			}

			var inventoryTasks = new[]
				{
					Quests.LoadFromDB(),
					Stats.LoadFromDB(),
					MonsterBook.LoadFromDB(),
					Friends.LoadFromDB(),
					Skills.LoadFromDB(),
					FuncKeys.LoadFromDB(),
					Macros.LoadFromDB(),
					Teleports.LoadFromDB(),
					WildHunterInfo.LoadFromDB(),
					TamingMob.LoadFromDB(),

					InventoryConsume.LoadFromDB(dwId, InventoryType.Consume),
					InventoryEtc.LoadFromDB(dwId, InventoryType.Etc),
					InventoryInstall.LoadFromDB(dwId, InventoryType.Install),
					InventoryCash.LoadFromDB(dwId, InventoryType.Cash),

					InventoryEquip.LoadFromDB(dwId, InventoryType.Equip),
					EquippedInventoryNormal.LoadFromDB(dwId, InventoryType.Equipped),
					EquippedInventoryCash.LoadFromDB(dwId, InventoryType.Cash),
					aDragonEquipped.LoadFromDB(dwId, InventoryType.DragonEquipped),
					aMechanicEquipped.LoadFromDB(dwId, InventoryType.MechanicEquipped),
					RingInfo.LoadFromDB(),
					StatisticsTracker.LoadFromDB()
				};

			Task.WaitAll(inventoryTasks);
		}

		private bool m_bDisposed { get; set; }

		// character has a different dispose pattern than regular cfieldobjects so we dont override this function
		public new void Dispose()
		{
			if (m_bDisposed) return;
			m_bDisposed = true;

			try
			{
				InventoryConsume.Dispose();
				InventoryEtc.Dispose();
				InventoryInstall.Dispose();
				InventoryCash.Dispose();
				InventoryEquip.Dispose();
				DamageTracker.Dispose();
				EquippedInventoryNormal.Dispose();
				EquippedInventoryCash.Dispose();
				aDragonEquipped.Dispose();
				aMechanicEquipped.Dispose();

				MonsterBook.Dispose();
				Skills.Dispose();
				Quests.Clear();
				Cooldowns.Clear();
				Friends.Dispose();
				FuncKeys.Dispose();
				Buffs.Clear();
				Macros.Dispose();
				Teleports.Dispose();
				Pets.Dispose();
				TamingMob.Dispose();
				Action.Dispose();
				Combat.Dispose();
				Modify.Dispose();
				WildHunterInfo.Dispose();
				RingInfo.Dispose();
				Dragon?.Dispose();
				CalcDamage.Dispose();

				base.Dispose(); // last dispose call

				// no need to set properties to null, c# does that for us
			}
			catch (Exception ex)
			{
				Log.Error("Unable to properly dispose character.");
				Log.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Attaches the character object to the game client object.
		/// This is done outside of the constructor so that the character class can be used in the cash shop/itc
		/// Also triggers the removal of expired items
		/// </summary>
		public void Init(WvsGameClient socket)
		{
			Initialized = false;
			Socket = socket;

			// we only want this to load in channels, not cs or itc
			Buffs = new CharacterBuffs(dwId);
			Cooldowns = new CharacterCooldowns(dwId);

			if (Common.GameLogic.JobLogic.IsEvan(Stats.nJob) && Stats.nJob != 2001)
			{
				Dragon = new CDragon(socket.Character);
			}

			InitLinkedCharInfo(); // requires socket to be initialized

			tLastRegen = DateTime.Now;
			tPortableChairSittingTime = DateTime.Now;
			tLastCharacterHPInc = DateTime.Now;
			tLastItemExpirationCheck = DateTime.MinValue;

			RecalcRegen(); // skill data needs to be initialized for this to work

			Initialized = true;
		}

		public void InitLinkedCharInfo()
		{
			_ = Socket ?? throw new NullReferenceException("Unable to fetch blessing of fairy: socket is null.");

			var info = Account.HighestLevelChar(dwId);

			nLinkedCharacterLevel = (byte)info.Item1;
			sLinkedCharacter = info.Item2;
		}

		//-----------------------------------------------------------------------------

		public void RemoveExpiredItems(bool bFromMigrateIn)
		{
			// TODO determine a good interval for this.. idk if i wanna loop inventory every minute so i changed to 5 min
			if (tLastItemExpirationCheck.SecondsSinceStart() < 60 * 5) return;
			tLastItemExpirationCheck = DateTime.Now;

			var removed = InventoryCash.RemoveExpiredItems(this);
			removed += InventoryConsume.RemoveExpiredItems(this);
			removed += InventoryEquip.RemoveExpiredItems(this);
			removed += InventoryEtc.RemoveExpiredItems(this);
			removed += InventoryInstall.RemoveExpiredItems(this);
			removed += EquippedInventoryCash.RemoveExpiredItems(this);
			removed += EquippedInventoryNormal.RemoveExpiredItems(this);

			if (removed <= 0) return;

			if (!bFromMigrateIn) ValidateStat();
		}

		//-----------------------------------------------------------------------------

		public void NotifySocialChannels(SocialNotiflag nFlag)
		{
			Friends.NotifyChangeFriendInfo(nFlag);
			MasterManager.GuildManager.UpdateGuildMember(this, nFlag);

			MasterManager.PartyPool.UpdatePartyMember(this, Party?.PartyID ?? 0, nFlag);


			/*** BMS OnSocketDestroyed 
			CUser::UnregisterUser(v2);
			CPartyMan::OnLeave(TSingleton<CPartyMan>::ms_pInstance, v2, bMigrate);
			CFriendMan::OnLeave(TSingleton<CFriendMan>::ms_pInstance, v2);
			CGuildMan::OnLeave(TSingleton<CGuildMan>::ms_pInstance, v2);
			CUser::OnUserTownPortalRemove(v2, v2->m_dwTownPortalFieldID);
			// more for v95??
			*/
		}

		public AvatarLook GetLook()
		{
			var ret = new AvatarLook();
			ret.CopyStats(Stats);
			ret.CopyInventory(EquippedInventoryNormal, EquippedInventoryCash);
			ret.CopyPets(Pets);
			return ret;
		}

		/// <summary>
		/// Recalculates server-side player stats.
		/// </summary>
		/// <param name="bWithInventory">Set to true if inventory should be recalculated as well.</param>
		public void ValidateStat(bool bWithInventory = false)
		{
			var preMHP = BasicStats.nMHP;
			var preMMP = BasicStats.nMMP;

			// order is critical
			Stats.PassiveSkillData.Update(this);

			if (bWithInventory)
			{
				Stats.SecondaryStats.SetFromInventory(this);
			}

			BasicStats.SetFrom(this);

			Modify.Stats(ctx =>
			{
				var hpDiff = BasicStats.nMHP - preMHP;
				if (hpDiff > 0)
				{
					ctx.HP += hpDiff;
				}

				var mpDiff = BasicStats.nMMP - preMMP;
				if (mpDiff > 0)
				{
					ctx.MP += mpDiff;
				}

				if (ctx.HP > BasicStats.nMHP)
				{
					ctx.HP = BasicStats.nMHP;
				}

				if (ctx.MP > BasicStats.nMMP)
				{
					ctx.MP = BasicStats.nMMP;
				}
			});
		}

		public void ValidateAdditionalItemEffect()
		{
			//if (v14 && Additional::TCond < Additional::MOBCATEGORY >::CheckCondition(v14, v1, v18, v2) && v14->nCategory)
			//v1->aMobCategoryDamage[v14->nCategory] += v14->nDamage;

			//if (v15 && Additional::TCond < Additional::ELEMBOOST >::CheckCondition(v15, v1, v18, v2) && v15->nElem >= 0)
			//v1->aElemBoost[v15->nElem] += v15->nVolume;

			//if (v16 && Additional::TCond < Additional::BOSS >::CheckCondition(v16, v1, v18, v2))
			//{
			//	v1->boss.nProb += v16->nProb;
			//	v1->boss.nDamage += v16->nDamage;
			//}

			//v17 = v11->pCritical.p;
			//pCritical.p = v17;
			//if (v17)
			//	InterlockedIncrement(&v17[-1].nDamage);
			//LOBYTE(v32) = 7;
			//if (v17)
			//{
			//	if (Additional::TCond < Additional::CRITICAL >::CheckCondition(v17, v1, v18, v2))
			//	{
			//		v1->critical.nProb += v17->nProb;
			//		v1->critical.nDamage += v17->nDamage;
			//	}
			//}
		}


		public COutPacket CharacterInfo()
		{
			//Send [LP_CharacterInfo] [3D 00] [71 04 00 00] [32] [6F 00] [00 00] [00] [00 00] [00] 00 00 00 01 01 00 00 00 01 00 00 00 01 00 00 00 0A 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00

			// Send [LP_CharacterInfo] [3D 00] [71 04 00 00] [32] [6F 00] [00 00] [00] [00 00 00 00] [00] [00] 01 01 00 00 00 01 00 00 00 01 00 00 00 0A 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00

			var p = new COutPacket(SendOps.LP_CharacterInfo);

			p.Encode4(dwId); //dwCharacterId
			p.Encode1(Stats.nLevel);
			p.Encode2(Stats.nJob);
			p.Encode2(Stats.nPOP);

			p.Encode1(false); // bIsMarried

			var guildId = MasterManager.GuildManager.GetGuildID(dwId);

			if (guildId <= 0)
			{
				p.Encode4(0);
			}
			else
			{
				p.EncodeString(MasterManager.GuildManager[guildId].GuildName);
				p.EncodeString(MasterManager.GuildManager[guildId].AllianceName);
			}

			p.Encode1(0); // pMedalInfo

			Pets.EncodeMultiPetInfo(p);

			p.Encode1(Stats.SecondaryStats.rRideVehicle != 0);

			if (Stats.SecondaryStats.rRideVehicle != 0)
				TamingMob.Encode(p);

			EncodeWishList(p);

			//MedalAchievementInfo::Decode(v29, v4);
			p.Encode4(InventoryManipulator.GetItem(this, BodyPart.BP_MEDAL, false)?.nItemID ?? 0); //this->nEquipedMedalID = CInPacket::Decode4(iPacket);

			Quests.EncodeMedalQuests(p);

			var chairs =
				InventoryInstall
				.Select(x => x.Value.nItemID)
				.Where(i => i / 10000 == 301)
				.ToList();

			p.Encode4(chairs.Count); //aChairItemID
			foreach (var item in chairs)
			{
				p.Encode4(item);

			}
			return p;
		}

		//-----------------------------------------------------------------------------

		public void Update(CField field)
		{
			Buffs.Update();
			Cooldowns.Update();
			RemoveExpiredItems(false);
			CheckCashItemExpire();
			CheckPetDead();
			Pets.UpdateActivePets();
			CheckGeneralItemExpire();
			TamingMob.UpdateTamingMobInfo();
			Regen();

			UpdateCalcDamageStat();
		}

		// CUser::CheckPetDead(CUser *this, int tCur)
		private void CheckPetDead() // has to be here instead of pet class cuz we're checking non-active pets too
		{
			// TODO
		}

		// CUser::CheckCashItemExpire(CUser *this, int tCur)
		private void CheckCashItemExpire()
		{
			// TODO
		}

		// CUser::CheckGeneralItemExpire(CUser *this, int tCur)
		private void CheckGeneralItemExpire()
		{
			// TODO
		}

		private void Regen()
		{
			if (nRegenMP <= 0) return;

			if (tLastRegen.MillisSinceStart() <= 4000) return;
			tLastRegen = DateTime.Now;

			if (Stats.nHP <= 0) return;

			if (Stats.nMP < BasicStats.nMMP)
			{
				Modify.Heal(0, nRegenMP);
			}
		}

		public void RecalcRegen()
		{
			switch (Stats.nJob)
			{
				case 111:
				case 112:
					nRegenMP = (Skills.Get((int)Common.Types.Skills.CRUSADER_UPGRADE_MP_RECOVERY)?.nSLV ?? 0) * 2;
					break;
				case 1111:
					nRegenMP = (Skills.Get((int)Common.Types.Skills.SOULMASTER_UPGRADE_MP_RECOVERY)?.nSLV ?? 0) * 2;
					break;
			}
		}

		// TODO
		private void UpdateCalcDamageStat()
		{
			//v13 = tCur - v2->m_tLastUpdateCalcDamageStat;
			//v24 = 2;
			//if (v13 > 60000)
			//{
			//	v14 = v2->m_lCalcDamageStat._m_uCount;
			//	if (v14 > 10)
			//	{
			//		CVerboseObj::LogError((CVerboseObj*)&v2->vfptr, aTooMuchCalcdam, v2->m_lCalcDamageStat._m_uCount);
			//		if (v14 > 30)
			//		{
			//			v20 = -2147467259;
			//			_CxxThrowException(&v20, &_TI1_AVZException__);
			//		}
			//	}
			//	v2->m_tLastUpdateCalcDamageStat = tCur;
			//}
		}

		public void Save()
		{
			Stats.SaveToDB();

			aMechanicEquipped.SaveToDB(dwId, InventoryType.MechanicEquipped);
			aDragonEquipped.SaveToDB(dwId, InventoryType.DragonEquipped);

			EquippedInventoryCash.SaveToDB(dwId, InventoryType.Cash);
			EquippedInventoryNormal.SaveToDB(dwId, InventoryType.Equipped);

			InventoryEquip.SaveToDB(dwId, InventoryType.Equip);

			InventoryConsume.SaveToDB(dwId, InventoryType.Consume);
			InventoryEtc.SaveToDB(dwId, InventoryType.Etc);
			InventoryInstall.SaveToDB(dwId, InventoryType.Install);
			InventoryCash.SaveToDB(dwId, InventoryType.Cash);

			Friends.SaveToDB();
			Skills.SaveToDB();
			FuncKeys.SaveToDB();
			MonsterBook.SaveToDB();
			Quests.SaveToDB();
			Macros.SaveToDB();                   
			Teleports.SaveToDB();
			WildHunterInfo.SaveToDB();
			TamingMob.SaveToDB();
			RingInfo.SaveToDB();
			StatisticsTracker.SaveToDB();

			// TODO move this inlined query someplace where it makes more sense to have it
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sb = new StringBuilder();
				sb.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.inventory_slot_count WHERE character_id = {dwId};");
				sb.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.inventory_slot_count (character_id, inventory_equip, inventory_consume, inventory_etc, inventory_install, inventory_cash)");
				sb.AppendLine($"VALUES ({dwId}, {InventoryEquip.SlotLimit}, {InventoryConsume.SlotLimit}, {InventoryEtc.SlotLimit}, {InventoryInstall.SlotLimit}, {InventoryCash.SlotLimit});");

				using (var cmd = new NpgsqlCommand(sb.ToString(), conn))
					cmd.ExecuteNonQuery();
			}
		}

		//-----------------------------------------------------------------------------

		public void Encode(COutPacket p, DbCharFlags dbFlag = DbCharFlags.ALL)
		{
			p.Encode8((long)dbFlag);
			p.Encode1((byte)nCombatOrders);
			p.Encode1(0); //Some Loop.

			if ((dbFlag & DbCharFlags.CHARACTER) > 0)
			{
				EncodeCharacter(p);
			}

			if ((dbFlag & DbCharFlags.MONEY) > 0)
			{
				Stats.EncodeMoney(p);
			}

			if ((dbFlag & DbCharFlags.INVENTORYSIZE) > 0)
			{
				EncodeInventorySize(p);
			}

			if ((dbFlag & DbCharFlags.EQUIPEXT) > 0)
			{
				p.Encode8(Constants.ZERO_TIME);
			}

			if ((dbFlag & DbCharFlags.ITEMSLOTEQUIP) > 0)
			{
				EquippedInventoryNormal.Encode(p);
				EquippedInventoryCash.Encode(p);
				InventoryEquip.Encode(p);
				aDragonEquipped.Encode(p);
				aMechanicEquipped.Encode(p);
			}

			if ((dbFlag & DbCharFlags.ITEMSLOTCONSUME) > 0)
			{
				InventoryConsume.Encode(p);
			}

			if ((dbFlag & DbCharFlags.ITEMSLOTINSTALL) > 0)
			{
				InventoryInstall.Encode(p);
			}

			if ((dbFlag & DbCharFlags.ITEMSLOTETC) > 0)
			{
				InventoryEtc.Encode(p);
			}

			if ((dbFlag & DbCharFlags.ITEMSLOTCASH) > 0)
			{
				InventoryCash.Encode(p);
			}

			if ((dbFlag & DbCharFlags.SKILLRECORD) > 0)
			{
				Skills.EncodeForSetField(p);
			}

			if ((dbFlag & DbCharFlags.SKILLCOOLTIME) > 0)
			{
				EncodeSkillCoolTime(p);
			}

			if ((dbFlag & DbCharFlags.QUESTRECORD) > 0)
			{
				EncodeQuestRecord(p);
			}

			if ((dbFlag & DbCharFlags.QUESTCOMPLETE) > 0)
			{
				EncodeQuestComplete(p);
			}

			if ((dbFlag & DbCharFlags.MINIGAMERECORD) > 0)
			{
				EncodeMiniGameInfo(p);
			}

			if ((dbFlag & DbCharFlags.COUPLERECORD) > 0)
			{
				EncodeRingInfo(p);
			}

			if ((dbFlag & DbCharFlags.MAPTRANSFER) > 0)
			{
				EncodeTeleportInfo(p);
			}

			if ((dbFlag & DbCharFlags.MONSTERBOOKCOVER) > 0)
			{
				MonsterBook.EncodeCover(p);
			}

			if ((dbFlag & DbCharFlags.MONSTERBOOKCARD) > 0)
			{
				MonsterBook.EncodeCards(p);
			}

			if ((dbFlag & DbCharFlags.NEWYEARCARD) > 0)
			{
				EncodeNewYearInfo(p); // Short 
			}

			if ((dbFlag & DbCharFlags.QUESTRECORDEX) > 0)
			{
				EncodeAreaInfo(p); // Short 
			}

			if ((dbFlag & DbCharFlags.WILDHUNTERINFO) > 0)
			{
				WildHunterInfo.Encode(p);
			}

			if ((dbFlag & DbCharFlags.QUESTCOMPLETE_OLD) > 0)
			{
				EncodeQuestCompleteOld(p);
			}

			if ((dbFlag & DbCharFlags.VISITORLOG) > 0)
			{
				EncodeVisitorLog(p);
			}
		}

		private void EncodeCharacter(COutPacket p)
		{
			Stats.Encode(p);
			p.Encode1((byte)Stats.nFriendMax);

			bool linkedChar = nLinkedCharacterLevel > 0;

			p.Encode1(linkedChar);

			if (linkedChar)
			{
				p.EncodeString(sLinkedCharacter);
			}
		}

		private void EncodeInventorySize(COutPacket p)
		{
			p.Encode1(InventoryEquip.SlotLimit);
			p.Encode1(InventoryConsume.SlotLimit);
			p.Encode1(InventoryInstall.SlotLimit);
			p.Encode1(InventoryEtc.SlotLimit);
			p.Encode1(InventoryCash.SlotLimit);
		}

		private void EncodeSkillCoolTime(COutPacket p)
		{
			p.Encode2((short)Cooldowns.Count);
			foreach (var item in Cooldowns)
			{
				p.Encode4(item.nSkillID);
				p.Encode2((short)item.SecondsLeft);
			}
		}

		private void EncodeQuestRecord(COutPacket p)
		{
			var quests = Quests.Where(item => item.nState != QuestActType.QuestComplete).ToArray();

			p.Encode2((short)quests.Length);
			foreach (var quest in quests)
			{
				p.Encode2(quest.nQuestID);
				p.EncodeString(quest.sQRValue);
			}
		}

		private void EncodeQuestComplete(COutPacket p)
		{
			var quests = Quests.Where(item => item.nState == QuestActType.QuestComplete).ToArray();

			p.Encode2((short)quests.Length);
			foreach (var quest in quests)
			{
				p.Encode2(quest.nQuestID);
				p.EncodeDateTime(quest.tCompleted);
			}
		}

		private void EncodeQuestCompleteOld(COutPacket p)
		{
			p.Encode2(0);
		}

		private void EncodeMiniGameInfo(COutPacket p)
		{
			p.Encode2(0);
		}

		public void EncodeRingInfo(COutPacket p)
		{
			RingInfo.EncodeRingRecords(p);
		}

		private void EncodeTeleportInfo(COutPacket p)
		{
			Teleports.Encode(p);
		}

		private void EncodeNewYearInfo(COutPacket p)
		{
			p.Encode2(0);
		}

		private void EncodeAreaInfo(COutPacket p)
		{
			p.Encode2(0);
		}

		private void EncodeVisitorLog(COutPacket p)
		{
			p.Encode2(0); //m_mVisitorQuestLog
		}

		public void EncodeWishList(COutPacket p)
		{
			p.Encode1((byte)Wishlist.Count(i => i > 0));
			foreach (var item in Wishlist)
			{
				if (item > 0)
					p.Encode4(item);
			}
		}

		//-----------------------------------------------------------------------------

		public void SendMessage(object sMsg)
		{
			SendPacket(CPacket.SystemMessage(sMsg.ToString()));
		}

		public void SendPacket(COutPacket packet) => Socket?.SendPacket(packet);

		//-----------------------------------------------------------------------------

		public void Move(CInPacket iPacket)
		{
			// field can be null if user is force-warped while movement packet is in transit
			Field?.Broadcast(UserMove(iPacket), this);
		}

		private COutPacket UserMove(CInPacket iPacket)
		{
			var oPacket = new COutPacket(SendOps.LP_UserMove);
			oPacket.Encode4(dwId);
			Position.UpdateMovePath(oPacket, iPacket);
			return oPacket;
		}

		public override COutPacket MakeEnterFieldPacket() => CPacket.UserEnterField(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.UserLeaveField(this);

		public override string ToString() => Stats?.sCharacterName ?? $"Char-{dwId}";
	}
}
