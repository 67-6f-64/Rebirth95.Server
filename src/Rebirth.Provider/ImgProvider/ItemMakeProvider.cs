using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.Template.Etc;

namespace Rebirth.Provider.ImgProvider
{
	public class ItemMakeProvider : AbstractProvider<ItemMakeTemplate>
	{
		protected override string ProviderName => "Etc.ItemMake";

		public ItemMakeProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var file = imgDir.GetChild(ProviderName.Split('.')[1] + ".img");

			foreach (var item in file as WzFileProperty)
			{
				foreach (var dirFileEntry in item.Value as WzProperty)
				{
					var recipeProductId = Convert.ToInt32(dirFileEntry.Key);
					var recipeBlob = dirFileEntry.Value as WzProperty;

					ItemMakeTemplate pEntry = new ItemMakeTemplate(recipeProductId)
					{
						CatalystID = recipeBlob.GetInt32("catalyst"),
						ItemNum = recipeBlob.GetInt32("itemNum"),
						Meso = recipeBlob.GetInt32("meso"),
						ReqLevel = recipeBlob.GetInt32("reqLevel"),
						ReqSkillLevel = recipeBlob.GetInt32("reqSkillLevel"),
						TUC = recipeBlob.GetInt32("tuc"),
						ReqItem = recipeBlob.GetInt32("reqItem"),
					};

					if (recipeBlob.GetChild("randomReward") is WzProperty randRewardProp)
					{
						var rewards = new List<ItemMakeRandomReward>();

						foreach (WzProperty randomRewardEntry in randRewardProp.GetAllChildren().Values)
						{
							rewards.Add(new ItemMakeRandomReward
							{
								ItemID = randomRewardEntry.GetInt32("item"),
								ItemNum = randomRewardEntry.GetInt32("itemNum"),
								Prob = randomRewardEntry.GetInt32("prob")
							});	
						}

						pEntry.RandomReward = rewards.ToArray();
					}

					if (recipeBlob.GetChild("recipe") is WzProperty recipeProp)
					{
						var recipes = new List<ItemMakeRecipe>();

						foreach (WzProperty rewardEntry in recipeProp.GetAllChildren().Values)
						{
							if (rewardEntry["item"] is null) continue; // nexon forgot a value

							recipes.Add(new ItemMakeRecipe
							{
								Count = rewardEntry.GetInt32("count"),
								ItemID = rewardEntry.GetInt32("item"),
							});
						}

						pEntry.Recipe = recipes.ToArray();
					}


					InsertItem(pEntry);
				}
			}
		}
	}
}
