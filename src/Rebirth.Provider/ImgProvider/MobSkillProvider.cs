using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Rebirth.Common.Tools;
using Rebirth.Provider.Template.Skill;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public class MobSkillProvider : AbstractProvider<MobSkillTemplate>
	{
		protected override string ProviderName => "Skill.MobSkill";

		public MobSkillProvider(WzFileSystem baseFileSystem) : base(baseFileSystem)
		{ }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var blob = imgDir.Files
					.FirstOrDefault(item => item.Name.ToLowerInvariant().Equals("mobskill.img"))
					?.Object as WzFileProperty;

			if (blob is null) throw new ArgumentException("Unable to find MobSkill.img in directory.");

			foreach (var skillBlob in blob)
			{
				var entry = new MobSkillTemplate(int.Parse(skillBlob.Key));

				var levelBlob = skillBlob.Value as WzProperty;

				foreach (var item in levelBlob["level"] as WzProperty)
				{
					levelBlob = item.Value as WzProperty;

					var levelEntry = new MobSkillLevelData
					{
						nSLV = int.Parse(item.Key),
						nSkillID = entry.TemplateId,
						X = levelBlob.GetInt32("x"),
						Y = levelBlob.GetInt32("y"),
						MpCon = levelBlob.GetInt32("mpCon"),
						Interval = levelBlob.GetInt32("interval"),
						Time = levelBlob.GetInt32("time"),
						HP = levelBlob.GetInt32("hp"),
						Prop = levelBlob.GetInt32("prop"),
						Limit = levelBlob.GetInt32("limit"),
						RandomTarget = levelBlob.GetInt64("randomTarget") != default,
						SummonEffect = levelBlob.GetInt32("summonEffect"),
						Count = levelBlob.GetInt32("count")
					};

					if (levelBlob["lt"] is WzVector2D lt)
					{
						levelEntry.LT = new Point(lt.X, lt.Y);
					}

					if (levelBlob["rb"] is WzVector2D rb)
					{
						levelEntry.RB = new Point(rb.X, rb.Y);
					}

					levelEntry.SummonIDs = levelBlob
						.GetAllChildren()
						.Where(kvp => int.TryParse(kvp.Key, out _))
						.Select(kvp => kvp.Value)
						.Cast<int>()
						.ToArray();

					entry.InsertLevelData(levelEntry);
				}

				InsertItem(entry);
			}
		}
	}
}
