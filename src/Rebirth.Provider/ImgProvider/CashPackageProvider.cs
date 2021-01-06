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
	/// Cash packages are indexed by the item ID starting with a 9.
	/// </summary>
	public class CashPackageProvider : AbstractProvider<CashPackageTemplate>
	{
		protected override string ProviderName => "Etc.CashPackage";

		public CashPackageProvider(WzFileSystem baseFileSystem)
			: base(baseFileSystem) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			var file = imgDir.GetChild(ProviderName.Split('.')[1] + ".img");

			foreach (var item in file as WzFileProperty)
			{
				var packageBlob = item.Value as WzProperty;
				var templateId = Convert.ToInt32(item.Key);

				InsertItem(new CashPackageTemplate(templateId)
				{
					SNList = ((WzProperty)packageBlob["SN"]).GetAllChildren().Values.Select(Convert.ToInt64).ToArray()
				});
			}
		}
	}
}
