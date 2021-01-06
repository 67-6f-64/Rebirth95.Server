using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.Template.Etc;

namespace Rebirth.Provider.ImgProvider
{
	/// <summary>
	/// Cash commodities are indexed by the cash item serial number, not the commodity ID.
	/// </summary>
	public class CashCommodityProvider : AbstractProvider<CashCommodityTemplate>
	{
		protected override string ProviderName => "Etc.Commodity";

		public CashCommodityProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var file = imgDir.GetChild(ProviderName.Split('.')[1] + ".img");

			foreach (var item in file as WzFileProperty)
			{
				var commodityBlob = item.Value as WzProperty;
				var templateId = commodityBlob.GetInt32("SN");

				InsertItem(new CashCommodityTemplate(templateId)
				{
					CommodityID = Convert.ToInt32(item.Key),
					Bonus = commodityBlob.GetInt32("Bonus"),
					Gender = commodityBlob.GetInt32("Gender"),
					Count = commodityBlob.GetInt32("Count"),
					Period = commodityBlob.GetInt32("Period"),
					Priority = commodityBlob.GetInt32("Priority"),
					//ReqPOP = commodityBlob.GetInt32(""),
					//ReqLVL = commodityBlob.GetInt32(""),
					//PbCash = commodityBlob.GetInt32(""),
					//PbGift = commodityBlob.GetInt32(""),
					//PbPoint = commodityBlob.GetInt32(""),
					CashItemSN = commodityBlob.GetInt32("SN"),
					ItemID = commodityBlob.GetInt32("ItemId"),
					Price = commodityBlob.GetInt32("Price"),
					OnSale = commodityBlob.GetInt32("OnSale") > 0,
					Classification = commodityBlob.GetInt32("Class"),
				});
			}
		}

		public CashCommodityTemplate Random()
		{
			return null; // TODO
		}
	}
}
