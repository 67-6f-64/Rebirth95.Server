using Autofac;
using log4net;
using WzTools.FileSystem;
using Rebirth.Entities.Item;
using Rebirth.Provider;
using Rebirth.Redis;
using Rebirth.Server.Center.GameData.DropInfo;
using Rebirth.Server.Center.MigrationStorage;
using Rebirth.Server.Center.Template;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Provider.ImgProvider;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Item;
using Rebirth.Provider.Template.String;
using Rebirth.Server.Center.Template.DropInfo;
using Rebirth.Tools;

namespace Rebirth.Server.Center
{
	public sealed class MasterManager
	{
		public static ILog Log = LogManager.GetLogger(typeof(MasterManager));
		private static bool bInitialized { get; set; }

		public static MobDropGenerator MobDropGenerator { get; } = new MobDropGenerator();
		public static CAvatarMegaphoneMan AvatarMan { get; } = new CAvatarMegaphoneMan();
		public static TempInvManager TempInvManager { get; } = new TempInvManager();
		public static CharacterPool CharacterPool { get; } = new CharacterPool();
		public static CPartyMan PartyPool { get; } = new CPartyMan();
		public static CooldownStorage CooldownStorage { get; } = new CooldownStorage();
		public static SummonStorage SummonStorage { get; } = new SummonStorage();
		public static BuffStorage BuffStorage { get; } = new BuffStorage();
		public static ShopManager ShopManager { get; } = new ShopManager();
		public static MessengerMan MessengerManager { get; } = new MessengerMan();

		public static CEventManager EventManager { get; private set; }
		public static GuildManager GuildManager { get; } = new GuildManager();

		public static ReactorProvider ReactorTemplates { get; private set; }
		public static MapProvider MapTemplates { get; private set; }
		public static NpcProvider NpcTemplates { get; private set; }
		public static MobProvider MobTemplates { get; private set; }
		public static ItemMakeProvider ItemMakeTemplates { get; private set; }
		public static CashCommodityProvider CommodityProvider { get; private set; }
		public static CashPackageProvider PackageProvider { get; private set; }
		public static SkillProvider SkillTemplates { get; private set; }
		public static MobSkillProvider MobSkillTemplates { get; private set; }
		public static QuestProvider QuestTemplates { get; private set; }
		public static ItemProvider ItemTemplates { get; private set; }
		public static ItemOptionProvider ItemOptionTemplates { get; private set; }
		public static EquipProvider EquipTemplates { get; private set; }

		public static StringProvider StringData { get; set; }

		public static void SetEventManager(WvsGame parent)
			=> EventManager = new CEventManager(parent);

		private static void InitGameData()
		{
			using (var fs = new WzFileSystem { BaseDir = Constants.GameDataPath })
			{
				fs.Init(Constants.GameDataPath + @"img\Data");

				var sw = new Stopwatch();
				sw.Start();

				Task.WaitAll(Task.Run(() => { StringData = new StringProvider(fs); }),
					Task.Run(() => { EquipTemplates = new EquipProvider(fs); }),
					Task.Run(() => { ItemOptionTemplates = new ItemOptionProvider(fs); }),
					Task.Run(() => { ItemTemplates = new ItemProvider(fs); }),
					Task.Run(() => { QuestTemplates = new QuestProvider(fs); }),
					Task.Run(() => { MobSkillTemplates = new MobSkillProvider(fs); }),
					Task.Run(() => { SkillTemplates = new SkillProvider(fs); }),
					Task.Run(() => { ItemMakeTemplates = new ItemMakeProvider(fs); }),
					Task.Run(() => { CommodityProvider = new CashCommodityProvider(fs); }),
					Task.Run(() => { PackageProvider = new CashPackageProvider(fs); }),
					Task.Run(() => { NpcTemplates = new NpcProvider(fs); }),
					Task.Run(() => { ReactorTemplates = new ReactorProvider(fs); }),
					Task.Run(() => { MapTemplates = new MapProvider(fs); }),
					Task.Run(() => { MobTemplates = new MobProvider(fs); })
					);

				sw.Stop();

				Log.Info("Startup seconds elapsed: " + sw.ElapsedMilliseconds / 1000);
			}
		}

		static void TestNxRates(int nMobID)
		{
			var result = 0;

			var mob = MobTemplates[nMobID];
			var mobName = StringData[StringDataType.Mob][nMobID].Name;

			var iterations = mob.Boss ? 10 : 1000;

			//if (mob.Boss)
			{
				for (var i = 0; i < 1000; i++)
				{
					result += run();
				}

				result /= 1000;
			}
			//else
			//{
			//	result = run();
			//}

			Log.Info($"Testing Mob: {mobName,-17} (avg from killing 10 mobs) => {result,-5} Mob Level: {mob.Level,-3} Boss: {mob.Boss,-5} MaxHP: {mob.MaxHP}");

			int run()
			{
				if (mob is null) throw new Exception("invalid mob passed");

				var retval = 0;

				for (var i = 0; i < 10; i++)
				{
					retval += RateLogic.NxGainFromMob(Constants.Rand, mob.Level, mob.MaxHP, mob.Boss);
				}

				return retval;
			}
		}

		public static void Load(int nWorldID)
		{
			if (bInitialized) return;
			bInitialized = true;

			// will load from img unless json files are present
			InitGameData();

			//#if DEBUG
			//			foreach (var mob in new[] { 4130100, 5130101, 5220003, 5110301, 8220003, 7120109, 7130001, 7160000, 8800003, 8800103 })
			//			{
			//				TestNxRates(mob);
			//			}
			//#endif

			// has to be after game data is initialized
			CashShopConstants.ApplyModifiedCommodities();

			GuildManager.Init();

			MobDropGenerator.Load(nWorldID);
			GlobalDropGenerator.Load();

			var shopCount = ShopManager.Load();
			Log.Info($"[Shop Manager] -> {shopCount,-5} total shops cached from DB.");

			MobDropGenerator.PurgeInvalidItems();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Log.Info("[Master Manager] Finished loading.");

			// determine what happens when donor messages

			var storage = ServerApp.Container.Resolve<CenterStorage>();
			var sub = storage.Multiplexer().GetSubscriber();

			sub.Subscribe("donate", (channel, message) =>
			{
				// message -> accountId/amount
				var m = message.ToString().Split('/');
				var accId = int.Parse(m[0]);
				var amount = int.Parse(m[1]);

				var pChar = CharacterPool.GetByAccountID(accId);
				if (pChar != null)
				{
					pChar.SendMessage($"{amount} Rebirth points have been added to your account!"); // placeholder until we get some sort of proper popup going
				}
				else
				{
					// TODO
					// queue message to be seen on login
					// maybe use account_data class??
				}
			});

			sub.Subscribe("vote", (channel, message) =>
			{
				// TODO 
			});
		}

		// TODO figure out somewhere else for the below functions to live

		public static AbstractItemTemplate ItemTemplate(int nItemID)
		{
			switch (ItemConstants.GetInventoryType(nItemID))
			{
				case InventoryType.Equip:
					return EquipTemplates[nItemID];

				case InventoryType.Cash:
				case InventoryType.Consume:
				case InventoryType.Etc:
				case InventoryType.Install:
				case InventoryType.Special:
					return ItemTemplates[nItemID];

				default:
					throw new NullReferenceException("Unable to find a template for item with item ID: " + nItemID);
			}
		}

		public static GW_ItemSlotBase CreateItem(int nItemID, bool bRandStats = true)
		{
			if (nItemID <= 0) return null;

			// TODO test the below
			if (ItemConstants.GetInventoryType(nItemID) != InventoryType.Equip)
			{
				GW_ItemSlotBase item;
				if (ItemConstants.IsPet(nItemID))
				{
					item = new GW_ItemSlotPet(nItemID);
					((GW_ItemSlotPet)item).nRemainLife = 7776000;
				}
				else
				{
					item = new GW_ItemSlotBundle(nItemID);
				}

				if (item.Template is null) throw new ArgumentOutOfRangeException(nameof(nItemID), "Doesn't exist in data files.");

				if (ItemConstants.IsRechargeableItem(item.nItemID))
				{
					item.nNumber = (short)item.Template.SlotMax;
				}
				else
				{
					item.nNumber = 1;
				}

				item.tDateExpire = DateTime.MaxValue;

				return item;
			}

			if (bRandStats) return CreateVariableStatEquip(nItemID);

			return CreateNormalStatEquip(nItemID);
		}

		public static GW_ItemSlotEquip CreateNormalStatEquip(int nItemID, long liCashItemSN = 0)
		{
			if (ItemConstants.GetInventoryType(nItemID) != InventoryType.Equip)
				return null;

			var pItem = new GW_ItemSlotEquip(nItemID);

			pItem.niSTR = (short)pItem.EquipTemplate.incSTR;
			pItem.niDEX = (short)pItem.EquipTemplate.incDEX;
			pItem.niINT = (short)pItem.EquipTemplate.incINT;
			pItem.niLUK = (short)pItem.EquipTemplate.incLUK;
			pItem.niMaxHP = (short)pItem.EquipTemplate.incMHP;
			pItem.niMaxMP = (short)pItem.EquipTemplate.incMMP;
			pItem.niPAD = (short)pItem.EquipTemplate.incPAD;
			pItem.niMAD = (short)pItem.EquipTemplate.incMAD;
			pItem.niPDD = (short)pItem.EquipTemplate.incPDD;
			pItem.niMDD = (short)pItem.EquipTemplate.incMDD;
			pItem.niACC = (short)pItem.EquipTemplate.incACC;
			pItem.niEVA = (short)pItem.EquipTemplate.incEVA;
			pItem.niSpeed = (short)pItem.EquipTemplate.incSpeed;
			pItem.niJump = (short)pItem.EquipTemplate.incJump;

			FinishEquip(pItem, liCashItemSN);

			return pItem;
		}

		public static GW_ItemSlotEquip CreateVariableStatEquip(int nItemID, long liCashItemSN = 0)
		{
			var pItem = new GW_ItemSlotEquip(nItemID);

			if (pItem.Template is null) throw new NullReferenceException($"Unable to find item with ID {nItemID} and cash ID {liCashItemSN}.");

			if (!pItem.Template.Cash)
			{
				pItem.niSTR = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incSTR));
				pItem.niDEX = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incDEX));
				pItem.niINT = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incINT));
				pItem.niLUK = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incLUK));

				pItem.niMaxHP = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incMHP));
				pItem.niMaxMP = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incMMP));

				pItem.niPAD = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incPAD));
				pItem.niMAD = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incMAD));

				pItem.niPDD = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incPDD));
				pItem.niMDD = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incMDD));

				pItem.niACC = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incACC));
				pItem.niEVA = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incEVA));

				pItem.niSpeed = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incSpeed));
				pItem.niJump = Math.Max((short)0, GaussianDistributionVariation(pItem.EquipTemplate.incJump));

				if (Constants.Rand.NextDouble() < RateConstants.ItemPotential_EquipDropHiddenPotentialOdds)
				{
					pItem.nGrade = PotentialGradeCode.Hidden_Rare;
				}
			}

			FinishEquip(pItem, liCashItemSN);

			return pItem;
		}

		private static void FinishEquip(GW_ItemSlotEquip pItem, long liCashItemSN)
		{
			pItem.liCashItemSN = liCashItemSN;
			pItem.tDateExpire = pItem.EquipTemplate.Cash ? DateTime.Now.AddDays(90) : DateTime.MaxValue;
			pItem.RemainingUpgradeCount = (byte)pItem.EquipTemplate.TUC;
			pItem.liSN = liCashItemSN != 0 ? DateTime.Now.Ticks : 0;
			pItem.niCraft = (byte)pItem.EquipTemplate.incCraft;
		}

		private static short GaussianDistributionVariation(int nDefaultValue)
		{
			if (nDefaultValue == 0) return 0;

			var nMaxRange = nDefaultValue * 1.75;

			if (nDefaultValue <= 0)
			{
				return Constants.Rand.Next(100) == 1
					? (short)Math.Ceiling(nMaxRange * 0.1)
					: (short)0;
			}

			var iMaxRange = Math.Min(Math.Ceiling(nDefaultValue * 0.2), nMaxRange);

			var mean = nDefaultValue < 20 ? iMaxRange / 2 : iMaxRange;

			return (short)(Math.Floor(new GaussianRandom().NextGaussian() * mean) + nDefaultValue);
		}

		public static List<CashItemInfo> CreateCashPackageItems(long liCashPackageSN)
		{
			var package = PackageProvider[(int)liCashPackageSN];

			var retVal = new List<CashItemInfo>();

			if (package is null) return retVal;

			retVal.AddRange(package.SNList.Select(CreateCashCommodityItem));

			return retVal;
		}

		public static CashItemInfo CreateCashCommodityItem(long liCashItemSN)
		{
			var commodity = CommodityProvider[(int)liCashItemSN];

			if (commodity is null) return null;

			var item = CreateItem(commodity.ItemID, false);

			if (!item.IsEquip) item.nNumber = (short)commodity.Count;
			else item.nNumber = 1;

			item.liCashItemSN = commodity.CashItemSN;
			item.liSN = DateTime.Now.Ticks;

			return new CashItemInfo(item, commodity.Period, commodity.Price, commodity.CommodityID);
		}
	}
}