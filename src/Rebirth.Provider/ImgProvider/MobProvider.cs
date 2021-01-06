using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.Template.Mob;
using Rebirth.Provider.Template.Npc;

namespace Rebirth.Provider.ImgProvider
{
	public class MobProvider : AbstractProvider<MobTemplate>
	{
		protected override string ProviderName => "Mob";

		public MobProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			const int mobIdLen = 7;

			foreach (var mobNode in imgDir.Files)
			{
				if (!mobNode.Name.EndsWith(".img")) continue; // two entries are not mobs

				var mobData = mobNode.Object as WzFileProperty;

				var nMobId = Convert.ToInt32(mobNode.Name.Substring(0, mobIdLen));
				var infoData = mobData.GetChild("info") as WzProperty;

				var pEntry = new MobTemplate(nMobId)
				{
					//Exp = infoData.GetInt32(""),
					Level = infoData.GetInt8("level"),
					MaxHP = infoData.GetInt32("maxHP"),
					MaxMP = infoData.GetInt32("maxMP"),
					HpRecovery = infoData.GetInt32("hpRecovery"),
					MpRecovery = infoData.GetInt32("mpRecovery"),

					PAD = infoData.GetInt32("PADamage"),
					PDD = infoData.GetInt32("PDDamage"),
					MAD = infoData.GetInt32("MADamage"),
					MDD = infoData.GetInt32("MDDamage"),
					MDR = infoData.GetInt32("MDRate"),
					PDR = infoData.GetInt32("PDRate"),
					EVA = infoData.GetInt32("eva"),
					ACC = infoData.GetInt32("acc"),

					Exp = infoData.GetInt32("exp"),
					Boss = infoData.GetInt32("boss") > 0,
					DeadBuff = infoData.GetInt32("buff"),
					HPGaugeHide = infoData.GetInt32("hpGaugeHide") > 0,
					RemoveAfter = infoData.GetInt32("removeAfter"),
					HpTagBgColor = infoData.GetInt32("hpTagBgcolor"),
					HpTagColor = infoData.GetInt32("hpTagColor"),
					Invincible = infoData.GetInt32("invincible") > 0,
					Speed = infoData.GetInt32("speed") + 100,
					FlySpeed = infoData.GetInt32("flySpeed") + 100,
					ChaseSpeed = infoData.GetInt32("chaseSpeed") + 100,
					FixedDamage = infoData.GetInt32("fixedDamage"),
					DoNotRemove = infoData.GetInt32("doNotRemove") > 0 || infoData.GetInt32("HPgaugeHide") > 0,
					SelfDestructActionType = (infoData.GetChild("selfDestruction") as WzProperty)?.GetInt32("action") ?? 0,
					SelfDestructRemoveAfter = (infoData.GetChild("selfDestruction") as WzProperty)?.GetInt32("removeAfter") ?? 0,
					CannotEvade = infoData.GetInt32("cannotEvade") > 0,
				};

				var link = infoData.GetInt32("link");

				var linkProp = mobData;

				if (link > 0) // TODO
				{
					//var linkString = $"{link.ToString().PadLeft(7, '0')}.img";
					//linkProp = imgDir.Files
					//	.FirstOrDefault(f => f.Name.Equals(linkString))
					//	?.Object as WzFileProperty;
				}

				if (linkProp.HasChild("fly"))
				{
					pEntry.MoveAbility = Common.Types.MobMoveType.Fly;
				}
				else
				{
					if (linkProp.HasChild("jump") && !linkProp.HasChild("move"))
					{
						pEntry.MoveAbility = Common.Types.MobMoveType.Jump;
					}
					else
					{
						pEntry.MoveAbility = linkProp.HasChild("move")
							? Common.Types.MobMoveType.Move
							: Common.Types.MobMoveType.Stop;
					}
				}

				if (linkProp.GetChild("regen") is WzProperty regen)
				{
					pEntry.tRegenInterval = regen.GetAllChildren()
						.Values
						.Cast<WzProperty>()
						.Sum(item => item.GetInt32("delay"));
				}

				if (infoData.GetChild("revive") is WzProperty revive)
				{
					pEntry.Revive = revive.GetAllChildren()
						.Values
						.Select(Convert.ToInt32)
						.ToArray();
				}

				if (infoData.GetChild("skill") is WzProperty skill)
				{
					pEntry.Skill = skill.GetAllChildren()
						.Values
						.Cast<WzProperty>()
						.Select(item => new MobTemplate.MobDataSkillTemplate
						{
							Action = item.GetInt32("action"),
							EffectAfter = item.GetInt32("effectAfter"),
							Skill = item.GetInt32("skill"),
							Level = item.GetInt32("level"),
						})
						.ToArray();
				}

				pEntry.DamagedElemAttr = new int[8];

				var sElemAttr = infoData.GetString("elemAttr").ToLowerInvariant();

				if (sElemAttr.Length > 0)
				{
					if (sElemAttr.Length % 2 != 0)
						throw new InvalidOperationException("Wrong elemental attribute length for skill " + pEntry.TemplateId);

					for (var i = 0; i < sElemAttr.Length; i += 2)
					{
						var aChars = sElemAttr.ToCharArray();

						var nVal = aChars[i + 1];

						switch (aChars[i])
						{
							case 'd':
								pEntry.DamagedElemAttr[6] = int.Parse(nVal.ToString());
								break;
							case 'f':
								pEntry.DamagedElemAttr[2] = int.Parse(nVal.ToString());
								break;
							case 'h':
								pEntry.DamagedElemAttr[5] = int.Parse(nVal.ToString());
								break;
							case 'i':
								pEntry.DamagedElemAttr[1] = int.Parse(nVal.ToString());
								break;
							case 'l':
								pEntry.DamagedElemAttr[3] = int.Parse(nVal.ToString());
								break;
							case 'p':
								pEntry.DamagedElemAttr[0] = int.Parse(nVal.ToString());
								break;
							case 's':
								pEntry.DamagedElemAttr[4] = int.Parse(nVal.ToString());
								break;
							case 'u':
								pEntry.DamagedElemAttr[7] = int.Parse(nVal.ToString());
								break;
						}
					}
				}

				mobNode.Unload();

				InsertItem(pEntry);
			}
		}
	}
}