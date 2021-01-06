using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Provider.Template.Item.ItemOption;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class ItemOptionProvider : AbstractProvider<ItemOptionTemplate>
	{
		protected override string ProviderName => "Item.ItemOption";

		public ItemOptionProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var optionFile in imgDir.Files)
			{
				foreach (var optionNode in (WzFileProperty)optionFile.Object)
				{
					var templateId = Convert.ToInt32(optionNode.Key);

					// face expressions on hit
					if (templateId % 10_000 >= 901 && templateId % 10_000 <= 905) continue;

					switch (templateId)
					{
						case 20366: // invincible duration increase
						case 30366: // invincible duration increase
						case 30371: // invincible activate
							continue;
					}

					var optionProp = optionNode.Value as WzProperty;

					if (optionProp is null) throw new NullReferenceException("Unexpected null option node.");

					var itemOptionTemplate = new ItemOptionTemplate(templateId);

					if (optionProp["info"] is WzProperty infoProp)
					{
						itemOptionTemplate.ReqLevel = infoProp.GetInt32("reqLevel");
						itemOptionTemplate.OptionType = infoProp.GetInt32("optionType");
					}

					if (optionProp["level"] is WzProperty levelProp)
					{
						var itemOptionLevels = new List<ItemOptionLevelData>();
						foreach (var levelNode in levelProp)
						{
							levelProp = levelNode.Value as WzProperty;

							var levelEntry = new ItemOptionLevelData
							{
								nIdx = Convert.ToInt32(levelProp.Name),
								nOptionType = itemOptionTemplate.TemplateId % 10_000,
								Prop = levelProp.GetInt32("prop"),
								Time = levelProp.GetInt32("time"),
								iSTR = levelProp.GetInt32("incSTR"),
								iDEX = levelProp.GetInt32("incDEX"),
								iINT = levelProp.GetInt32("incINT"),
								iLUK = levelProp.GetInt32("incLUK"),
								iHP = levelProp.GetInt32("HP"),
								iMP = levelProp.GetInt32("MP"),
								iMaxHP = levelProp.GetInt32("incMHP"),
								iMaxMP = levelProp.GetInt32("incMMP"),
								iACC = levelProp.GetInt32("incACC"),
								iEVA = levelProp.GetInt32("incEVA"),
								iSpeed = levelProp.GetInt32("incSpeed"),
								iJump = levelProp.GetInt32("incJump"),
								iPAD = levelProp.GetInt32("incPAD"),
								iMAD = levelProp.GetInt32("incMAD"),
								iPDD = levelProp.GetInt32("incPDD"),
								iMDD = levelProp.GetInt32("incMDD"),
								iSTRr = levelProp.GetInt32("incSTRr"),
								iDEXr = levelProp.GetInt32("incDEXr"),
								iINTr = levelProp.GetInt32("incINTr"),
								iLUKr = levelProp.GetInt32("incLUKr"),
								iMaxHPr = levelProp.GetInt32("incMHPr"),
								iMaxMPr = levelProp.GetInt32("incMMPr"),
								iACCr = levelProp.GetInt32("incACCr"),
								iEVAr = levelProp.GetInt32("incEVAr"),
								iPADr = levelProp.GetInt32("incPADr"),
								iMADr = levelProp.GetInt32("incMADr"),
								iPDDr = levelProp.GetInt32("incPDDr"),
								iMDDr = levelProp.GetInt32("incMDDr"),
								iCr = levelProp.GetInt32("incCr"),
								// these dont exist
								//iCDr = levelProp.GetInt32("reduceCooltime"),
								//iMAMr = levelProp.GetInt32("incMAMr"),
								iAllSkill = levelProp.GetInt32("incAllskill"),
								RecoveryHP = levelProp.GetInt32("RecoveryHP"),
								RecoveryMP = levelProp.GetInt32("RecoveryMP"),
								RecoveryUP = levelProp.GetInt32("RecoveryUP"),
								MPConReduce = levelProp.GetInt32("mpconReduce"),
								MPConRestore = levelProp.GetInt32("mpRestore"),
								IgnoreTargetDEF = levelProp.GetInt32("ignoreTargetDEF"),
								IgnoreDAM = levelProp.GetInt32("ignoreDAM"),
								IgnoreDAMr = levelProp.GetInt32("ignoreDAMr"),
								iDAMr = levelProp.GetInt32("incDAMr"),
								DAMReflect = levelProp.GetInt32("DAMreflect"),
								AttackType = levelProp.GetInt32("attackType"),
								iMesoProb = levelProp.GetInt32("incMesoProp"),
								iRewardProb = levelProp.GetInt32("incRewardProp"),
								Level = levelProp.GetInt32("level"),
								Boss = levelProp.GetInt32("boss"),
								// removing these cuz i dont want them
								//	Emotion_angry = levelProp.GetString("face").ToLowerInvariant().Equals("angry"),
								//	Emotion_cheer = levelProp.GetString("face").ToLowerInvariant().Equals("cheer"),
								//	Emotion_love = levelProp.GetString("face").ToLowerInvariant().Equals("love"),
								//	Emotion_blaze = levelProp.GetString("face").ToLowerInvariant().Equals("blaze"),
								//	Emotion_glitter = levelProp.GetString("face").ToLowerInvariant().Equals("glitter"),
							};

							itemOptionLevels.Add(levelEntry);
						}

						itemOptionTemplate.LevelData = itemOptionLevels.ToArray();
					}

					InsertItem(itemOptionTemplate);
				}
			}
		}

		// TODO figure out a better way to feed this data to a requester
		public IEnumerable<ItemOptionTemplate> GetAll() => Values;
	}
}
