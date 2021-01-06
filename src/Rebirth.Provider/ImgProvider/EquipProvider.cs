using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebirth.Provider.Template.Character;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class EquipProvider : AbstractProvider<EquipTemplate>
	{
		protected override string ProviderName => "Character";
		protected override bool LoadFromJSON => false;

		public EquipProvider(WzFileSystem baseFileSystem) : base(baseFileSystem)
		{ }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var tasks = new List<Task>();

			foreach (var equipTypeDir in imgDir.SubDirectories)
			{
				if (equipTypeDir.Name.Equals("Afterimage")) continue;

				tasks.Add(Task.Run(() =>
				{
					foreach (var equipFile in equipTypeDir.Files)
					{
						var templateId = Convert.ToInt32(equipFile.Name.Split('.')[0]);
						var equipBlob = equipFile.Object as WzFileProperty;
						var infoProp = equipBlob["info"] as WzProperty;

						var equipTemplate = new EquipTemplate(templateId)
							{
								Cash = infoProp.GetInt32("cash") > 0,
								Price = infoProp.GetInt32("price"),
								TradeBlock = infoProp.GetInt32("tradeBlock") > 0,
								NotSale = infoProp.GetInt32("notSale") > 0,
								Only = infoProp.GetInt32("only") > 0,
								TimeLimited = infoProp.GetInt32("timeLimited") > 0,
								ExpireOnLogout = infoProp.GetInt32("expireOnLogout") > 0,
								NotExtend = infoProp.GetInt32("notExtend") > 0,
								EquipTradeBlock = infoProp.GetInt32("equipTradeBlock") > 0,

								ReqLevel = infoProp.GetInt32("reqLevel"),
								TUC = infoProp.GetInt32("tuc"),
								incSTR = infoProp.GetInt32("incSTR"),
								incDEX = infoProp.GetInt32("incDEX"),
								incINT = infoProp.GetInt32("incINT"),
								incLUK = infoProp.GetInt32("incLUK"),
								incMHP = infoProp.GetInt32("incMHP"),
								incMMP = infoProp.GetInt32("incMMP"),
								incPAD = infoProp.GetInt32("incPAD"),
								incMAD = infoProp.GetInt32("incMAD"),
								incPDD = infoProp.GetInt32("incPDD"),
								incMDD = infoProp.GetInt32("incMDD"),
								incACC = infoProp.GetInt32("incACC"),
								incEVA = infoProp.GetInt32("incEVA"),
								incCraft = infoProp.GetInt32("incCraft"),
								incSpeed = infoProp.GetInt32("incSpeed"),
								incJump = infoProp.GetInt32("incJump"),
							};

						InsertItem(equipTemplate);
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());

			//var tasks = new List<Task>();

			//foreach (var equipTypeDir in imgDir.SubDirectories)
			//{
			//	if (equipTypeDir.Name.Equals("Afterimage")) continue;

			//	//tasks.Add(Task.Run(() =>
			//	//{

			//	//}));

			//	foreach (var equipFile in equipTypeDir.Files)
			//	{
			//		var templateId = Convert.ToInt32(equipFile.Name.Split('.')[0]);
			//		var equipBlob = equipFile.Object as WzFileProperty;

			//		var equipTemplate = new EquipTemplate(templateId);

			//		AssignProviderAttributes(equipTemplate, equipBlob);

			//		if (equipTemplate.TemplateId == 01032082)
			//		{
			//			;
			//		}

			//		InsertItem(equipTemplate);

			//		equipFile.Unload();
			//	}
			//}

			//Task.WaitAll(tasks.ToArray());
		}
	}
}
