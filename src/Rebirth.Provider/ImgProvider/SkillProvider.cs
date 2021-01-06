using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NCalc;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Skill;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public class SkillProvider : AbstractProvider<SkillTemplate>
	{
		protected override string ProviderName => "Skill";

		private readonly Dictionary<int, List<int>> _skillsByJob = new Dictionary<int, List<int>>();

		public List<int> GetJobSkills(int nJobID)
			=> !_skillsByJob.ContainsKey(nJobID)
				? new List<int>()
				: _skillsByJob[nJobID];

		public SkillProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem)
		{ }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var jobNode in imgDir.Files)
			{
				if (!int.TryParse(jobNode.Name.Split('.')[0], out _)) continue;

				var blob = jobNode.Object as WzFileProperty;

				var skillBlob = blob.GetChild("skill") as WzProperty;

				foreach (var rawSkillBlob in skillBlob)
				{
					var nSkillID = int.Parse(rawSkillBlob.Key);

					var skillData = rawSkillBlob.Value as WzProperty;

					var entry = new SkillTemplate(nSkillID)
					{
						IsWeaponBoosterSkill = SkillLogic.IsWeaponBoosterSkill(nSkillID),
						IsPartyBuff = SkillLogic.IsPartyBuff(nSkillID),
						IsWeaponChargeSkill = SkillLogic.IsWeaponChargeSkill(nSkillID),
						IsSharpEyesSkill = SkillLogic.IsSharpEyesSkill(nSkillID),
						IsStanceSkill = SkillLogic.get_stance_skill_id(nSkillID / 10000) == nSkillID,
						IsHyperBodySkill = SkillLogic.IsHyperBodySkill(nSkillID),
						IsDashSkill = SkillLogic.IsDashSkill(nSkillID),
						IsMapleWarriorSkill = SkillLogic.IsMapleWarriorSkill(nSkillID),
						IsNotBuff = SkillLogic.IsNotBuff(nSkillID),
						is_skill_need_master_level = SkillLogic.is_skill_need_master_level(nSkillID),
						is_heros_will_skill = SkillLogic.is_heros_will_skill(nSkillID),
						is_shoot_skill_not_consuming_bullet = SkillLogic.is_shoot_skill_not_consuming_bullet(nSkillID),
						is_keydown_skill = SkillLogic.is_keydown_skill(nSkillID),
						is_event_vehicle_skill = SkillLogic.is_event_vehicle_skill(nSkillID),
						is_antirepeat_buff_skill = SkillLogic.is_antirepeat_buff_skill(nSkillID),
						get_required_combo_count = SkillLogic.get_required_combo_count(nSkillID),

						HasAffected = skillData.HasChild("affected"),
						HasSpecial = skillData.HasChild("special"),
						Invisible = skillData.HasChild("invisible"),
						MasterLevel = skillData.GetInt32("masterLevel"),
						SkillType = skillData.GetInt32("skillType"),
						Weapon = skillData.GetInt32("weapon"),
						PsdSkill = skillData.GetInt32("psd"),
						IsPrepareAttackSkill = skillData.HasChild("prepare"),
						CanFinalAttack = skillData.HasChild("finalAttack")
					};

					if (entry.TemplateId != (int)Skills.KNIGHT_COMBAT_ORDERS && !SkillLogic.IsBeginnerSkill(nSkillID))
					{
						entry.CombatOrders = 2;
					}

					// verified
					if (skillData["summon"] is WzProperty summonProp)
					{
						entry.IsSummonSkill = true;

						if (summonProp["fly"] is WzProperty)
						{
							entry.IsFlySummonSkill = true;
						}

						if (summonProp["die"] is WzProperty summonDieProp)
						{
							if (summonDieProp["info"] is WzProperty summonDieInfoProp)
							{
								entry.SummonSkillDieAttackAfter = summonDieInfoProp.GetInt32("attackAfter");
								entry.SummonSkillDieMobCount = summonDieInfoProp.GetInt32("mobCount");
								entry.IsAttackOnDieSummonSkill = entry.SummonSkillDieMobCount != default;
							}
						}

						// in v95 there is one skill that has attack2 but it has the same values as attack1 so im not gonna bother parsing it
						if (summonProp["attack1"] is WzProperty summonAttackProp)
						{
							if (summonAttackProp["info"] is WzProperty summonAttackInfoProp)
							{
								entry.SummonSkillAttackAttackAfter = summonAttackInfoProp.GetInt32("attackAfter");
								entry.SummonSkillAttackMobCount = summonAttackInfoProp.GetInt32("mobCount");
							}
						}
					}

					// verified
					if (skillData["psdSkill"] is WzProperty psdSkills)
					{
						entry.AdditionPsd = psdSkills.GetAllChildren()
							.Values
							.Cast<WzProperty>()
							.Select(item => Convert.ToInt32(item.Name))
							.ToArray();
					}

					// verified
					if (skillData.HasChild("req"))
					{
						foreach (var skillId in skillData["req"] as WzProperty)
						{
							int nslv;
							if (skillId.Value is string) // some are strings some are int64 smh
							{
								nslv = int.Parse(skillId.Value.ToString());
							}
							else
							{
								nslv = (int)skillId.Value;
							}
							entry.Req.Add(int.Parse(skillId.Key), nslv); // skill id, skill level
						}
					}

					// verified
					switch ((Skills)entry.TemplateId)
					{
						case Skills.WILDHUNTER_MINE_DUMMY_SUMMONED:
						case Skills.WILDHUNTER_SWALLOW_DUMMY_ATTACK:
						case Skills.WILDHUNTER_SWALLOW_DUMMY_BUFF:
						case Skills.MECHANIC_GATLING_UP:
						case Skills.MECHANIC_FLAMETHROWER_UP:
						case Skills.MECHANIC_ROBOROBO_DUMMY:
							entry.IsHiddenSkill = true;
							break;
						default:
							entry.IsHiddenSkill = skillData.GetString("info").TrimStart().StartsWith("(hid");
							break;
					}

					// verified
					switch (skillData.GetString("elemAttr").ToLowerInvariant())
					{
						case "f":
							entry.ElemAttr = ElemAttrType.Fire;
							break;
						case "i":
							entry.ElemAttr = ElemAttrType.Ice;
							break;
						case "s":
							entry.ElemAttr = ElemAttrType.Poison;
							break;
						case "h":
							entry.ElemAttr = ElemAttrType.Holy;
							break;
						case "l":
							entry.ElemAttr = ElemAttrType.Light;
							break;
						case "p":
							entry.ElemAttr = ElemAttrType.Physical;
							break;
						case "d":
							entry.ElemAttr = ElemAttrType.Dark;
							break;
					}

					if (skillData.HasChild("level"))
					{
						// verified
						var levelData = skillData["level"] as WzProperty;
						entry.MaxLevel = levelData.GetAllChildren().Count;

						ProcessSkillData(skillData["level"] as WzProperty, entry);
					}
					else
					{
						// verified
						var commonData = skillData["common"] as WzProperty;
						entry.MaxLevel = commonData.GetInt32("maxLevel");

						ProcessSkillData(commonData, entry);
					}

					InsertItem(entry);
				}
			}
		}

		private void ProcessSkillData(WzProperty baseNode, SkillTemplate entry)
		{
			// level data
			entry.ItemConsumeAmount = baseNode.GetInt32("itemConNo");
			entry.BulletCount = baseNode.GetInt32("bulletCount");
			entry.BulletConsume = baseNode.GetInt32("bulletCon");
			entry.ItemConsume = baseNode.GetInt32("itemCon");
			entry.OptionalItemCost = baseNode.GetInt32("itemConsume");
			entry.Morph = baseNode.GetInt32("morph");

			// verified
			if (baseNode.Get("lt") is WzVector2D lt)
			{
				entry.LT = new Point(lt.X, lt.Y);
			}

			// verified
			if (baseNode["rb"] is WzVector2D rb)
			{
				entry.RB = new Point(rb.X, rb.Y);
			}

			entry.InitSLD(entry.MaxLevel + entry.CombatOrders);

			var isLevelNode = baseNode.Name.Equals("level");

			var parentNode = baseNode;

			// verified
			for (var i = 0; i < entry.DataLength; i++)
			{
				var level = i + 1;

				entry[level] = new SkillLevelData();

				if (isLevelNode)
				{
					baseNode = parentNode[level.ToString()] as WzProperty ?? parentNode;
				}

				entry[level].FixDamage = GetEvalInt(baseNode.GetString("fixdamage"), level);
				entry[level].AttackCount = GetEvalInt(baseNode.GetString("attackCount"), level);
				entry[level].MobCount = GetEvalInt(baseNode.GetString("mobCount"), level);
				entry[level].Time = GetEvalInt(baseNode.GetString("time"), level);
				entry[level].SubTime = GetEvalInt(baseNode.GetString("subTime"), level);
				entry[level].MpCon = GetEvalInt(baseNode.GetString("mpCon"), level);
				entry[level].HpCon = GetEvalInt(baseNode.GetString("hpCon"), level);
				entry[level].Damage = GetEvalInt(baseNode.GetString("damage"), level);
				entry[level].Mastery = GetEvalInt(baseNode.GetString("mastery"), level);
				entry[level].DamR = GetEvalInt(baseNode.GetString("damR"), level);
				entry[level].Dot = GetEvalInt(baseNode.GetString("dot"), level);
				entry[level].DotTime = GetEvalInt(baseNode.GetString("dotTime"), level);
				entry[level].MESOr = GetEvalInt(baseNode.GetString("mesoR"), level);
				entry[level].Speed = GetEvalInt(baseNode.GetString("speed"), level);
				entry[level].Jump = GetEvalInt(baseNode.GetString("jump"), level);
				entry[level].PAD = GetEvalInt(baseNode.GetString("pad"), level);
				entry[level].MAD = GetEvalInt(baseNode.GetString("mad"), level);
				entry[level].PDD = GetEvalInt(baseNode.GetString("pdd"), level);
				entry[level].MDD = GetEvalInt(baseNode.GetString("mdd"), level);
				entry[level].EVA = GetEvalInt(baseNode.GetString("eva"), level);
				entry[level].ACC = GetEvalInt(baseNode.GetString("acc"), level);
				entry[level].HP = GetEvalInt(baseNode.GetString("hp"), level);
				entry[level].MHPr = GetEvalInt(baseNode.GetString("mhpR"), level);
				entry[level].MP = GetEvalInt(baseNode.GetString("mp"), level);
				entry[level].MMPr = GetEvalInt(baseNode.GetString("mmpR"), level);
				entry[level].Prop = GetEvalInt(baseNode.GetString("prop"), level);
				entry[level].SubProp = GetEvalInt(baseNode.GetString("subProp"), level);
				entry[level].Cooltime = GetEvalInt(baseNode.GetString("cooltime"), level);
				entry[level].ASRr = GetEvalInt(baseNode.GetString("asrR"), level);
				entry[level].TERr = GetEvalInt(baseNode.GetString("terR"), level);
				entry[level].EMDD = GetEvalInt(baseNode.GetString("emdd"), level);
				entry[level].EMHP = GetEvalInt(baseNode.GetString("emhp"), level);
				entry[level].EMMP = GetEvalInt(baseNode.GetString("emmp"), level);
				entry[level].EPAD = GetEvalInt(baseNode.GetString("epad"), level);
				entry[level].EPDD = GetEvalInt(baseNode.GetString("epdd"), level);
				entry[level].Cr = GetEvalInt(baseNode.GetString("cr"), level);
				entry[level].T = GetEvalDouble(baseNode.GetString("t"), level);
				entry[level].U = GetEvalDouble(baseNode.GetString("u"), level);
				entry[level].V = GetEvalDouble(baseNode.GetString("v"), level);
				entry[level].W = GetEvalDouble(baseNode.GetString("w"), level);
				entry[level].X = GetEvalDouble(baseNode.GetString("x"), level);
				entry[level].Y = GetEvalDouble(baseNode.GetString("y"), level);
				entry[level].Z = GetEvalDouble(baseNode.GetString("z"), level);
				entry[level].PADr = GetEvalInt(baseNode.GetString("padR"), level);
				entry[level].PADx = GetEvalInt(baseNode.GetString("padX"), level);
				entry[level].MADr = GetEvalInt(baseNode.GetString("madR"), level);
				entry[level].MADx = GetEvalInt(baseNode.GetString("madX"), level);
				entry[level].PDDr = GetEvalInt(baseNode.GetString("pddR"), level);
				entry[level].MDDr = GetEvalInt(baseNode.GetString("mddR"), level);
				entry[level].EVAr = GetEvalInt(baseNode.GetString("evaR"), level);
				entry[level].ACCr = GetEvalInt(baseNode.GetString("accR"), level);
				entry[level].IMPr = GetEvalInt(baseNode.GetString("ignoreMobpdpR"), level);
				entry[level].IMDr = GetEvalInt(baseNode.GetString("ignoreMobDamR"), level);
				entry[level].CDMin = GetEvalInt(baseNode.GetString("criticaldamageMin"), level);
				entry[level].CDMax = GetEvalInt(baseNode.GetString("criticaldamageMax"), level);
				entry[level].EXPr = GetEvalInt(baseNode.GetString("expR"), level);
				entry[level].Er = GetEvalInt(baseNode.GetString("er"), level);
				entry[level].Ar = GetEvalInt(baseNode.GetString("ar"), level);
				entry[level].OCr = GetEvalInt(baseNode.GetString("overChargeR"), level);
				entry[level].DCr = GetEvalInt(baseNode.GetString("disCountR"), level);
				entry[level].PDamr = GetEvalInt(baseNode.GetString("pdR"), level);
				entry[level].MDamr = GetEvalInt(baseNode.GetString("mdR"), level);
				entry[level].PsdJump = GetEvalInt(baseNode.GetString("psdJump"), level);
				entry[level].PsdSpeed = GetEvalInt(baseNode.GetString("psdSpeed"), level);

				// skill bufftime modification
				switch ((Skills)entry.TemplateId)
				{
					case Skills.MECHANIC_FLAMETHROWER:
					case Skills.MECHANIC_FLAMETHROWER_UP:
						entry[level].Time = 1;
						break;
					case Skills.MECHANIC_HN07: // mount
					case Skills.WILDHUNTER_JAGUAR_RIDING: // mount
					case Skills.MECHANIC_SAFETY: // permanent
					case Skills.MECHANIC_PERFECT_ARMOR: // permanent
					case Skills.MECHANIC_SIEGE2: // semi-permanent (uses hp per sec)
						entry[level].Time = -1;
						break;
					case Skills.MECHANIC_SG88:
					case Skills.MECHANIC_SATELITE:
					case Skills.MECHANIC_SATELITE2:
					case Skills.MECHANIC_SATELITE3:
						entry[level].Time = 2100000;
						break;
					case Skills.CROSSBOWMAN_FINAL_ATTACK_CROSSBOW:
					case Skills.FIGHTER_FINAL_ATTACK:
					case Skills.HUNTER_FINAL_ATTACK_BOW:
					case Skills.PAGE_FINAL_ATTACK:
					case Skills.SOULMASTER_FINAL_ATTACK_SWORD:
					case Skills.SPEARMAN_FINAL_ATTACK:
					case Skills.WILDHUNTER_FINAL_ATTACK:
					case Skills.WINDBREAKER_FINAL_ATTACK_BOW:
						entry[level].MobCount = 3;
						break;
				}

				// bullet/attack/mob count modification
				switch ((Skills)entry.TemplateId)
				{
					case Skills.MECHANIC_SIEGE1:
					case Skills.MECHANIC_SIEGE2:
					case Skills.MECHANIC_SIEGE2_SPECIAL:
						entry[level].AttackCount = 6;
						entry.BulletCount = 6;
						break;
				}
			}
		}

		private static int GetEvalInt(string toEval, int level, string _default = "0")
		{
			var expr = new Expression(ProcessStringForEval(toEval, level));
			return int.Parse(expr.Evaluate()?.ToString() ?? "0");
		}

		private static double GetEvalDouble(string toEval, int level, string _default = "0")
		{
			var expr = new Expression(ProcessStringForEval(toEval, level));
			return double.Parse(expr.Evaluate()?.ToString() ?? "0");
		}

		private static string ProcessStringForEval(string input, int level)
		{
			input = input
				.Replace("d", "Floor")
				.Replace("u", "Ceiling")
				.Replace("x", level.ToString())
				.Trim();

			if (input.Length <= 0) input = "0";

			return input;
		}

		protected override void ProcessAdditionalData()
		{
			foreach (var templateId in Keys)
			{
				var jobId = templateId / 10000;

				if (!_skillsByJob.ContainsKey(jobId))
				{
					_skillsByJob.Add(jobId, new List<int>());
				}

				_skillsByJob[jobId].Add(templateId);
			}
		}
	}
}
