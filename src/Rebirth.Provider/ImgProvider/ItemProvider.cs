using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using Newtonsoft.Json;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Item;
using Rebirth.Provider.Template.Item.Cash;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Provider.Template.Item.Etc;
using Rebirth.Provider.Template.Item.Install;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class ItemProvider : AbstractProvider<AbstractItemTemplate>
	{
		[JsonIgnore]
		protected override string ProviderName => "Item";

		[JsonIgnore]
		protected override bool LoadFromJSON => false;

		[JsonIgnore]
		private readonly Dictionary<int, HashSet<int>> _masteryBooks = new Dictionary<int, HashSet<int>>();

		public ItemProvider(WzFileSystem baseFileSystem) : base(baseFileSystem)
		{ }

		public HashSet<int> MasteryBooksByJob(int nJobID)
			=> _masteryBooks.ContainsKey(nJobID)
				? _masteryBooks[nJobID]
				: new HashSet<int>();

		public Dictionary<int, HashSet<int>>.ValueCollection MasteryBooks()
			=> _masteryBooks.Values;

		protected override void ProcessAdditionalData()
		{
			foreach (var item in this)
			{
				if (item.Value is ConsumeItemTemplate itemTemplate)
				{
					foreach (var skill in itemTemplate.SkillData)
					{
						var job = (int)Math.Floor(skill / 10000f);

						job -= job % 10;

						if (job > 1000)
						{
							job -= job % 100;
						}

						if (!_masteryBooks.ContainsKey(job))
						{
							_masteryBooks.Add(job, new HashSet<int>());
						}

						_masteryBooks[job].Add(item.Key);
					}

					if (itemTemplate.MasterLevel > 0)
					{
						itemTemplate.SlotMax = 25;
					}
					else if (itemTemplate.SlotMax <= 0)
					{
						itemTemplate.SlotMax = 200;
					}
				}
				else if (item.Value is EtcItemTemplate)
				{
					if (item.Value.SlotMax <= 0)
					{
						item.Value.SlotMax = 1000;
					}
				}
				else if (item.Value.SlotMax == 0)
				{
					item.Value.SlotMax = 1;
				}
			}

			// mastery book quest stuff
			this[4001028].SlotMax = 100; // scroll of secrets
			this[4031049].PickupMessage = "A dark presence has forced itself upon me. I must take this item to someone in Perion.";
			this[4001028].PickupMessage = "A dark presence has forced itself upon me. I must take this item to someone in Perion.";
			this[4031019].PickupMessage = "A dark presence has forced itself upon me. I must take this item to someone in Perion.";
		}

		// CItemInfo::IterateItemInfo
		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			IterateCashBundleItem(imgDir.SubDirectories.FirstOrDefault(
				dir => dir.Name.Equals("Cash")));

			LoadPets(imgDir.SubDirectories.FirstOrDefault(
				dir => dir.Name.Equals("Pet")));

			LoadInstall(imgDir.SubDirectories.FirstOrDefault(
				dir => dir.Name.Equals("Install")));

			LoadEtc(imgDir.SubDirectories.FirstOrDefault(
				dir => dir.Name.Equals("Etc")));

			LoadConsume(imgDir.SubDirectories.FirstOrDefault(
				dir => dir.Name.Equals("Consume")));
		}

		private void FinishTemplate(AbstractItemTemplate itemTemplate, WzProperty infoProp)
		{
			itemTemplate.TradeBlock = infoProp.GetInt32("tradeBlock") > 0;
			itemTemplate.Cash = infoProp.GetInt32("cash") > 0;
			itemTemplate.NotSale = infoProp.GetInt32("notSale") > 0;
			itemTemplate.Quest = infoProp.GetInt32("quest") > 0;
			itemTemplate.Price = infoProp.GetInt32("price");
			itemTemplate.Only = infoProp.GetInt32("only") > 0;
			itemTemplate.Max = infoProp.GetInt32("max");
			itemTemplate.ExpireOnLogout = infoProp.GetInt32("expireOnLogout") > 0;
			itemTemplate.TimeLimited = infoProp.GetInt32("timeLimited") > 0;
			itemTemplate.MCType = infoProp.GetInt32("mcType");
			itemTemplate.SlotMax = infoProp.GetInt32("slotMax");

			if (!(infoProp["time"] is WzProperty))
			{
				itemTemplate.Time = (infoProp.Parent["spec"] as WzProperty)?.GetInt32("time") ?? 0;
			}
		}

		private void LoadPets(NameSpaceDirectory nameSpaceDir)
		{
			if (nameSpaceDir is null) throw new ArgumentNullException(nameof(nameSpaceDir));
			if (nameSpaceDir.Files.Count <= 0) throw new ArgumentException("No files in directory.", nameof(nameSpaceDir));

			foreach (var itemBlob in nameSpaceDir.Files)
			{
				var infoProp = ((WzFileProperty)itemBlob.Object)["info"] as WzProperty;

				var itemTemplate = new PetItemTemplate(Convert.ToInt32(itemBlob.Name.Split('.')[0]))
				{
					Hungry = infoProp.GetInt32("hungry"),
					Life = infoProp.GetInt32("life"),
					NameTag = infoProp.GetInt32("nameTag"),
					ChatBalloon = infoProp.GetInt32("chatBalloon"),
					EvolReqItemID = infoProp.GetInt32("evolReqItemID"),
					EvolNo = infoProp.GetInt32("evolNo"),
					EvolReqPetLvl = infoProp.GetInt32("evolReqPetLvl"),
					LimitedLife = infoProp.GetInt32("limitedLife"),
					Permanent = infoProp.GetInt32("permanent") > 0,
					AutoReact = infoProp.GetInt32("autoReact") > 0,
					NoRevive = infoProp.GetInt32("noRevive") > 0,
					NoMoveToLocker = infoProp.GetInt32("noMoveToLocker") > 0,
					InteractByUserAction = infoProp.GetInt32("interactByUserAction") > 0,
				};

				itemTemplate.Evol = new int[itemTemplate.EvolNo];
				itemTemplate.EvolProb = new int[itemTemplate.EvolNo];

				for (var i = 0; i < itemTemplate.Evol.Length; i++)
				{
					var num = i + 1;
					itemTemplate.Evol[i] = infoProp.GetInt32("evol" + num);
					itemTemplate.EvolProb[i] = infoProp.GetInt32("evolProb" + num);
				}

				FinishTemplate(itemTemplate, infoProp);
				InsertItem(itemTemplate);
			}
		}

		private void LoadInstall(NameSpaceDirectory nameSpaceDir)
		{
			if (nameSpaceDir is null) throw new ArgumentNullException(nameof(nameSpaceDir));
			if (nameSpaceDir.Files.Count <= 0) throw new ArgumentException("No files in directory.", nameof(nameSpaceDir));

			foreach (var itemTypeBlob in nameSpaceDir.Files)
			{
				foreach (var itemBlob in (WzFileProperty)itemTypeBlob.Object)
				{
					var infoProp = ((WzProperty)itemBlob.Value)["info"] as WzProperty;

					var itemTemplate = new InstallItemTemplate(Convert.ToInt32(itemBlob.Key))
					{
						RecoveryHP = infoProp.GetInt32("recoveryMP"),
						RecoveryMP = infoProp.GetInt32("recoveryHP"),
						ReqLevel = infoProp.GetInt32("reqLevel"),
						TamingMob = infoProp.GetInt32("tamingMob"),
					};

					FinishTemplate(itemTemplate, infoProp);
					InsertItem(itemTemplate);
				}
			}
		}

		private void LoadEtc(NameSpaceDirectory nameSpaceDir)
		{
			if (nameSpaceDir is null) throw new ArgumentNullException(nameof(nameSpaceDir));
			if (nameSpaceDir.Files.Count <= 0) throw new ArgumentException("No files in directory.", nameof(nameSpaceDir));

			foreach (var itemTypeBlob in nameSpaceDir.Files)
			{
				foreach (var itemBlob in (WzFileProperty)itemTypeBlob.Object)
				{
					var infoProp = ((WzProperty)itemBlob.Value)["info"] as WzProperty;

					var templateId = Convert.ToInt32(itemBlob.Key);

					AbstractItemTemplate itemTemplate;
					switch (templateId / 10_000)
					{
						case 425:
							itemTemplate = new GemEffectTemplate(templateId)
							{
								RandOption = infoProp.GetInt8("randOption"),
								RandStat = infoProp.GetInt8("randStat"),

								incReqLevel = infoProp.GetInt8("incReqLevel"),
								incDEX = infoProp.GetInt8("incDEX"),
								incSTR = infoProp.GetInt8("incSTR"),
								incLUK = infoProp.GetInt8("incLUK"),
								incINT = infoProp.GetInt8("incINT"),
								incPDD = infoProp.GetInt8("incPDD"),
								incMDD = infoProp.GetInt8("incMDD"),
								incPAD = infoProp.GetInt8("incPAD"),
								incMAD = infoProp.GetInt8("incMAD"),
								incEVA = infoProp.GetInt8("incEVA"),
								incACC = infoProp.GetInt8("incACC"),
								incMaxHP = infoProp.GetInt8("incMaxHP"),
								incMaxMP = infoProp.GetInt8("incMaxMP"),
								incSpeed = infoProp.GetInt8("incSpeed"),
								incJump = infoProp.GetInt8("incJump"),
							};
							break;
						case 426:
							itemTemplate = new MonsterCrystalLevelTemplate(templateId)
							{
								lvMax = infoProp.GetInt32("lvMax"),
								lvMin = infoProp.GetInt32("lvMin"),
							};
							break;
						default:
							{
								var consumeItems = new List<int>();
								var consumeCounts = new List<int>();

								var consumeItemNode = infoProp["consumeItem"] as WzProperty;

								if (consumeItemNode != null)
								{
									foreach (var node in consumeItemNode)
									{
										if (node.Value is WzProperty nodeValues)
										{
											consumeItems.Add(nodeValues.GetInt32("0"));
											consumeCounts.Add(nodeValues.GetInt32("1"));
										}
										else if (node.Key.Length <= 2) // pretty safe bet its a consume item
										{
											consumeItems.Add((int)node.Value);
										}
									}
								}

								itemTemplate = new EtcItemTemplate(templateId)
								{
									lv = infoProp.GetInt32("lv"),
									Exp = infoProp.GetInt32("exp"),
									Grade = infoProp.GetInt32("grade"),
									QuestID = infoProp.GetInt32("questId"),
									PickupBlock = infoProp.GetInt32("pickUpBlock") > 0,
									PQuest = infoProp.GetInt32("pquest") > 0,
									Hybrid = infoProp.GetInt32("hybrid") > 0,
									ConsumeItem = consumeItems.ToArray(),
									ConsumeItemExpGain = consumeCounts.ToArray(),
									ConsumeCount = consumeItemNode?.GetInt32("consumeCount") ?? 0,
									ConsumeMessage = consumeItemNode?.GetString("consumeCountMessage") ?? "",
									ShopCoin = infoProp.GetInt32("shopCoin") > 0,
									BigSize = infoProp.GetInt32("bigSize") > 0,
								};
							}
							break;
					}

					FinishTemplate(itemTemplate, infoProp);
					InsertItem(itemTemplate);
				}
			}
		}

		private void LoadConsume(NameSpaceDirectory nameSpaceDir)
		{
			if (nameSpaceDir is null) throw new ArgumentNullException(nameof(nameSpaceDir));
			if (nameSpaceDir.Files.Count <= 0) throw new ArgumentException("No files in directory.", nameof(nameSpaceDir));

			foreach (var itemTypeBlob in nameSpaceDir.Files)
			{
				foreach (var itemBlob in (WzFileProperty)itemTypeBlob.Object)
				{
					var itemBlobValue = (WzProperty)itemBlob.Value;

					var infoProp = itemBlobValue["info"] as WzProperty;
					var specProp = itemBlobValue["spec"] as WzProperty;

					var templateId = Convert.ToInt32(itemBlob.Key);

					var itemTemplate = new ConsumeItemTemplate(templateId);

					var inc = infoProp.GetInt32("inc");

					itemTemplate.PetfoodInc = inc == 0 ? 100 : inc;
					itemTemplate.UnitPrice = Convert.ToDouble(infoProp.Get("unitPrice"));

					if (infoProp["skill"] is WzProperty skillProp)
					{
						itemTemplate.SkillData = skillProp
							.GetAllChildren()
							.Values
							.Cast<int>()
							.ToArray();
					}

					itemTemplate.InfoType = infoProp.GetInt32("type");

					if (itemBlobValue["mob"] is WzProperty mobProp)
					{
						itemTemplate.SummoningSackIDs = mobProp
							.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Select(item
								=> item.GetInt32("id"))
							.ToArray();

						itemTemplate.SummoningSackProbs = mobProp
							.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Select(item
								=> item.GetInt32("prob"))
							.ToArray();
					}

					itemTemplate.MasterLevel = infoProp.GetInt32("masterLevel");
					itemTemplate.ReqSkillLevel = infoProp.GetInt32("reqSkillLevel");
					itemTemplate.SuccessRate = infoProp.GetInt32("success");
					itemTemplate.MonsterBook = infoProp.GetInt32("monsterBook") > 0;

					itemTemplate.CursedRate = infoProp.GetInt32("cursed");
					itemTemplate.SuccessRate = infoProp.GetInt32("success");
					itemTemplate.IncMHP = infoProp.GetInt32("incMHP");
					itemTemplate.IncMMP = infoProp.GetInt32("incMMP");
					itemTemplate.IncPAD = infoProp.GetInt32("incPAD");
					itemTemplate.IncMAD = infoProp.GetInt32("incMAD");
					itemTemplate.IncPDD = infoProp.GetInt32("incPDD");
					itemTemplate.IncMDD = infoProp.GetInt32("incMDD");
					itemTemplate.IncACC = infoProp.GetInt32("incACC");
					itemTemplate.IncEVA = infoProp.GetInt32("incEVA");
					itemTemplate.IncINT = infoProp.GetInt32("incINT");
					itemTemplate.IncDEX = infoProp.GetInt32("incDEX");
					itemTemplate.IncSTR = infoProp.GetInt32("incSTR");
					itemTemplate.IncLUK = infoProp.GetInt32("incLUK");
					itemTemplate.IncSpeed = infoProp.GetInt32("incSpeed");
					itemTemplate.IncJump = infoProp.GetInt32("incJump");
					itemTemplate.PreventSlip = infoProp.GetInt32("preventslip") > 0;
					itemTemplate.WarmSupport = infoProp.GetInt32("warmsupport") > 0;
					itemTemplate.IncCraft = infoProp.GetInt32("incCraft");
					itemTemplate.Recover = infoProp.GetInt32("recover");
					itemTemplate.RandStat = infoProp.GetInt32("randstat") > 0;
					itemTemplate.IncRandVol = infoProp.GetInt32("incRandVol");

					if (specProp != null)
					{
						itemTemplate.HP = specProp.GetInt32("hp");
						itemTemplate.MP = specProp.GetInt32("mp");
						itemTemplate.HPR = specProp.GetInt32("hpR");
						itemTemplate.MPR = specProp.GetInt32("mpR");
						itemTemplate.EXP = specProp.GetInt32("exp");
						itemTemplate.MHPR = specProp.GetInt32("mhpR");
						itemTemplate.MMPR = specProp.GetInt32("mmpR");
						itemTemplate.PAD = specProp.GetInt32("pad");
						itemTemplate.MAD = specProp.GetInt32("mad");
						itemTemplate.PDD = specProp.GetInt32("pdd");
						itemTemplate.MDD = specProp.GetInt32("mdd");
						itemTemplate.PADRate = specProp.GetInt32("padRate");
						itemTemplate.MADRate = specProp.GetInt32("madRate");
						itemTemplate.PDDRate = specProp.GetInt32("pddRate");
						itemTemplate.MDDRate = specProp.GetInt32("mddRate");
						itemTemplate.ACC = specProp.GetInt32("acc");
						itemTemplate.EVA = specProp.GetInt32("eva");
						itemTemplate.ACCRate = specProp.GetInt32("accR");
						itemTemplate.EVARate = specProp.GetInt32("evaR");
						itemTemplate.Speed = specProp.GetInt32("speed");
						itemTemplate.Jump = specProp.GetInt32("jump");
						itemTemplate.SpeedRate = specProp.GetInt32("speedRate");
						itemTemplate.JumpRate = specProp.GetInt32("jumpRate");
						itemTemplate.MoveTo = specProp.GetInt32("moveTo");
						itemTemplate.IgnoreContinent = specProp.GetInt32("ignoreContinent") > 0;
						itemTemplate.Prob = specProp.GetInt32("prob");
						itemTemplate.CP = specProp.GetInt32("cp");
						itemTemplate.CPSkill = specProp.GetInt32("nuffSkill");

						itemTemplate.Cure_Seal = specProp.GetInt32("seal") > 0;
						itemTemplate.Cure_Curse = specProp.GetInt32("curse") > 0;
						itemTemplate.Cure_Poison = specProp.GetInt32("poison") > 0;
						itemTemplate.Cure_Weakness = specProp.GetInt32("weakness") > 0;
						itemTemplate.Cure_Darkness = specProp.GetInt32("darkness") > 0;

						itemTemplate.ConsumeOnPickup = specProp.GetInt32("consumeOnPickup") > 0;

						// important to keep this check here to keep default value if attribute doesn't exist
						if (specProp.HasChild("BFSkill"))
						{
							itemTemplate.BFSkill = specProp.GetInt32("BFSkill");
						}
						else
						{
							itemTemplate.BFSkill = -1;
						}

						itemTemplate.DojangShield = specProp.GetInt32("dojangshield");

						itemTemplate.ExpInc = specProp.GetInt32("expinc");
						itemTemplate.Morph = specProp.GetInt32("morph");

						itemTemplate.ExpUpByItem = specProp.GetInt32("expBuff") > 0;
						itemTemplate.MesoUpByItem = specProp.GetInt32("mesoupbyitem") > 0;
						itemTemplate.ItemUpByItem = specProp.GetInt32("itemupbyitem") > 0;

						itemTemplate.ExpBuffRate = specProp.GetInt32("expBuff");
						itemTemplate.ItemScript = specProp.GetString("script");
						itemTemplate.Time = specProp.GetInt32("time"); // may collide with FinishTemplate??
					}

					FinishTemplate(itemTemplate, infoProp);
					InsertItem(itemTemplate);
				}
			}
		}

		private void IterateCashBundleItem(NameSpaceDirectory nameSpaceDir)
		{
			if (nameSpaceDir is null) throw new ArgumentNullException(nameof(nameSpaceDir));
			if (nameSpaceDir.Files.Count <= 0) throw new ArgumentException("No files in directory.", nameof(nameSpaceDir));

			foreach (var itemTypeBlob in nameSpaceDir.Files)
			{
				foreach (var item in (WzFileProperty)itemTypeBlob.Object)
				{
					var templateId = Convert.ToInt32(item.Key);

					var itemBlob = item.Value as WzProperty;
					var infoProp = itemBlob["info"] as WzProperty;
					var specProp = itemBlob["spec"] as WzProperty;

					CashItemTemplate itemTemplate;
					switch (templateId / 10_000)
					{
						case 524:
							itemTemplate = RegisterPetFoodItem(templateId, itemBlob);
							break;
						case 566:
							itemTemplate = new QuestDeliveryItemTemplate(templateId); // TODO
							break;
						case 530:
							itemTemplate = new MorphItemTemplate(templateId)
							{
								Price = infoProp.GetInt32("price"),
								HP = infoProp.GetInt32("hp"),
								Morph = infoProp.GetInt32("morph"),
								Time = infoProp.GetInt32("time"),
							};
							break;
						default:
							{
								if (templateId / 1_000 == 5281)
								{
									itemTemplate = new AreaBuffItemTemplate(templateId)
									{
										RB = infoProp.Get("rb") as WzVector2D,
										LT = infoProp.Get("lt") as WzVector2D,
										Time = infoProp.GetInt32("time"),
										// TODO rest of prop stuff 
									};
								}
								else
								{
									itemTemplate = new CashItemTemplate(templateId);
								}
							}
							break;
					}

					itemTemplate.ProtectTime = infoProp.GetInt32("protectTime");
					itemTemplate.WeatherType = infoProp.GetInt32("type");
					itemTemplate.StateChangeItem = infoProp.GetInt32("stateChangeItem");
					itemTemplate.RecoveryRate = infoProp.GetInt32("recoveryRate");
					itemTemplate.Life = infoProp.GetInt32("life");
					itemTemplate.Meso = infoProp.GetInt32("meso");
					itemTemplate.MesoMin = infoProp.GetInt32("mesomin");
					itemTemplate.MesoMax = infoProp.GetInt32("mesomax");
					itemTemplate.MesoStDev = infoProp.GetInt32("mesostdev");
					itemTemplate.MaplePoint = infoProp.GetInt32("maplepoint");
					itemTemplate.Rate = infoProp.GetInt32("rate");

					FinishTemplate(itemTemplate, infoProp);
					InsertItem(itemTemplate);
				}
			}
		}

		private PetFoodItemTemplate RegisterPetFoodItem(int templateId, WzProperty prop)
		{
			var specBlob = prop["spec"] as WzProperty;
			var infoBlob = prop["info"] as WzProperty;

			var entry = new PetFoodItemTemplate(templateId)
			{
				Repleteness = specBlob.GetInt32("inc")
			};

			var acceptedPets = new List<int>();

			foreach (var specNode in specBlob)
			{
				if (int.TryParse(specNode.Key, out var i))
				{
					acceptedPets.Add(Convert.ToInt32(specNode.Value));
				}
			}

			entry.Pet = acceptedPets.ToArray();

			return entry;
		}
	}
}
