using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Rebirth.Provider.Template;
using Rebirth.Provider.Template.Quest;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class QuestProvider : AbstractProvider<QuestTemplate>
	{
		protected override string ProviderName => "Quest";

		[JsonProperty]
		private readonly Dictionary<int, HashSet<int>> QuestByMobID = new Dictionary<int, HashSet<int>>();

		public HashSet<int> GetQuestByMobID(int nMobID) => QuestByMobID.GetValueOrDefault(nMobID);

		public QuestProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var questBlob in imgDir.SubDirectories.First(item => item.Name.Equals("QuestData")).Files)
			{
				ProcessQuestData(questBlob.Object as WzFileProperty);
			}

			RegisterQuestDemand(imgDir.Files.First(item
				=> item.Name.Equals("QuestInfo.img")).Object as WzFileProperty);
		}

		private void ProcessQuestData(WzFileProperty baseNode)
		{
			if (!int.TryParse(baseNode.Name.Split('.')[0], out var questId)) return;

			var entry = new QuestTemplate(questId);

			if (baseNode["Act"] is WzProperty actInfo)
			{
				if (actInfo["0"] is WzProperty startRewards)
				{
					entry.StartAct = RegisterQuestAct(startRewards);
					entry.RepeatInterval = startRewards.GetInt32("interval");
				}

				if (actInfo["1"] is WzProperty endRewards)
				{
					entry.EndAct = RegisterQuestAct(endRewards);
				}
			}

			// verified
			if (baseNode["QuestInfo"] is WzProperty questInfo)
			{
				entry.AutoAccept = questInfo.GetInt64("autoAccept") != 0;
				entry.AutoStart = questInfo.GetInt64("autoStart") != 0;
				entry.AutoComplete = questInfo.GetInt64("autoComplete") != 0;
				entry.AutoPreComplete = questInfo.GetInt64("autoPreComplete") != 0;
				entry.ViewMedalItem = questInfo.GetInt32("viewMedalItem");
			}

			if (baseNode["Check"] is WzProperty checkInfo) // requirements
			{
				if (checkInfo["0"] is WzProperty startRequirements)
				{
					entry.StartDemand = RegisterQuestDemand(startRequirements);

					entry.StartScript = startRequirements.GetString("startscript");
					entry.StartNPC = startRequirements.GetInt32("npc");
					entry.CompletionWaitInterval = startRequirements.GetInt32("interval");

					var start = startRequirements.GetString("start");
					var end = startRequirements.GetString("end");

					if (start.Length > 0)
					{
						entry.Start = new DateTime(
							int.Parse(start.Substring(0, 4)),
							int.Parse(start.Substring(4, 2)),
							int.Parse(start.Substring(6, 2)),
							int.Parse(start.Substring(8, 2)),
							0,
							0);
					}

					if (end.Length > 0)
					{
						entry.End = new DateTime(
							int.Parse(end.Substring(0, 4)),
							int.Parse(end.Substring(4, 2)),
							int.Parse(end.Substring(6, 2)),
							int.Parse(end.Substring(8, 2)),
							0,
							0);
					}
				}

				if (checkInfo["1"] is WzProperty endRequirements)
				{
					entry.EndDemand = RegisterQuestDemand(endRequirements);

					entry.EndScript = endRequirements.GetString("endscript");
					entry.EndNPC = endRequirements.GetInt32("npc");
				}

				if (entry.EndDemand != null)
				{
					foreach (var item in entry.EndDemand.DemandMob)
					{
						if (!QuestByMobID.ContainsKey(item.MobID))
						{
							QuestByMobID.Add(item.MobID, new HashSet<int>());
						}

						if (!QuestByMobID[item.MobID].Contains(entry.TemplateId))
						{
							QuestByMobID[item.MobID].Add(entry.TemplateId);
						}
					}
				}
			}

			InsertItem(entry);
		}

		private static QuestAct RegisterQuestAct(WzProperty baseNode)
		{
			if (baseNode.GetAllChildren().Count <= 0)
				return null;

			var rewards = new QuestAct
			{
				IncExp = baseNode.GetInt32("exp"),
				IncMoney = baseNode.GetInt32("money"),
				IncPop = baseNode.GetInt32("pop"),
				IncPetTameness = baseNode.GetInt32("pettameness"),
				IncPetSpeed = baseNode.GetInt32("petspeed"),
				PetSkill = baseNode.GetInt32("petskill"),
				NextQuest = baseNode.GetInt32("nextQuest"),
				BuffItemID = baseNode.GetInt32("buffItemID"),
				LevelMin = baseNode.GetInt32("lvmin"),
				LevelMax = baseNode.GetInt32("lvmax"),
				Info = baseNode.GetString("info")
			};

			if (baseNode["item"] is WzProperty itemRewards)
			{
				rewards.Items = itemRewards.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new QuestAct.ActItem
						{
							Item = new ItemInfo
							{
								ItemID = item.GetInt32("id"),
								Count = item.GetInt32("count"),
							},
							Period = item.GetInt32("period"),
							JobFlag = item.GetInt32("job"),
							Gender = item.GetInt32("gender"),
							ProbRate = item.GetInt32("prop"),
							ItemVariation = item.GetInt32("var"),
						})
					.ToArray();
			}

			if (baseNode["skill"] is WzProperty skillRewards)
			{
				rewards.Skills = skillRewards.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new QuestAct.ActSkill
						{
							SkillID = item.GetInt32("id"),
							MasterLevel = item.GetInt32("masterLevel"),
							SkillLevel = item.GetInt32("skillLevel") != 0
								? item.GetInt32("skillLevel")
								: item.GetInt32("acquire"),
							Job = (item["job"] as WzProperty)?.GetAllChildren().Values.Cast<int>()
							.ToArray() ?? new int[0]
						})
					.ToArray();
			}

			return rewards;
		}

		private static QuestDemand RegisterQuestDemand(WzProperty baseNode)
		{
			if (baseNode.GetAllChildren().Count <= 0)
				return null;

			var demands = new QuestDemand
			{
				LevelMax = baseNode.GetInt32("lvmax"),
				LevelMin = baseNode.GetInt32("lvmin"),
				TamingMobLevelMax = baseNode.GetInt32("pettamenessmax"),
				TamingMobLevelMin = baseNode.GetInt32("pettamenessmin"),
				PetTamenessMax = baseNode.GetInt32("tamingmoblevelmin"),
				PetTamenessMin = baseNode.GetInt32("tamingmoblevelmax"),
				Pop = baseNode.GetInt32("pop"),
				RepeatByDay = baseNode.GetInt32("dayByDay") != 0,
				SubJobFlags = baseNode.GetInt32("subJobFlags"),
				NormalAutoStart = baseNode.GetInt32("normalAutoStart") > 0,

			};

			if (baseNode["mob"] is WzProperty mobNode)
			{
				demands.DemandMob = mobNode.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new MobInfo
						{
							MobID = item.GetInt32("id"),
							Count = item.GetInt32("count")
						})
					.ToArray();
			}

			if (baseNode["quest"] is WzProperty questNode)
			{
				demands.DemandQuest = questNode.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new QuestDemand.QuestRecord
						{
							QuestID = item.GetInt16("id"),
							State = item.GetInt32("state")
						})
					.ToArray();
			}

			if (baseNode["item"] is WzProperty itemNode)
			{
				demands.DemandItem = itemNode.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new ItemInfo
						{
							ItemID = item.GetInt32("id"),
							Count = item.GetInt32("count")
						})
					.ToArray();
			}

			if (baseNode["skill"] is WzProperty skillNode)
			{
				demands.DemandSkill = skillNode.GetAllChildren()
					.Values
					.Cast<WzProperty>()
					.Select(item =>
						new SkillInfo
						{
							SkillID = item.GetInt32("id"),
							Acquire = item.GetInt32("acquire")
						})
					.ToArray();
			}

			if (baseNode["equipSelectNeed"] is WzProperty equipSelectNode)
			{
				demands.EquipSelectNeed = equipSelectNode.GetAllChildren().Values.Cast<int>().ToArray();
			}

			if (baseNode["equipAllNeed"] is WzProperty equipNode)
			{
				demands.EquipAllNeed = equipNode.GetAllChildren().Values.Cast<int>().ToArray();
			}

			if (baseNode["job"] is WzProperty jobNode)
			{
				demands.Job = jobNode.GetAllChildren().Values.Cast<int>().ToArray();
			}

			if (baseNode["fieldEnter"] is WzProperty fieldNode)
			{
				demands.FieldEnter = fieldNode.GetAllChildren().Values.Cast<int>().ToArray();
			}

			return demands;
		}
	}
}
