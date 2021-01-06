using System;
using System.Reflection.Metadata.Ecma335;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.Template.Npc;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class NpcProvider : AbstractProvider<NpcTemplate>
	{
		protected override string ProviderName => "Npc";
		protected override bool LoadFromJSON => false;

		public NpcProvider(WzFileSystem baseImgDir)
			: base(baseImgDir) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var npcNode in imgDir.Files)
			{
				var npcData = npcNode.Object as WzFileProperty;
				var pEntry = new NpcTemplate(Convert.ToInt32(npcNode.Name.Substring(0, 7)));

				AssignProviderAttributes(pEntry, npcData);
				InsertItem(pEntry);
			}
		}

		protected override void ProcessAdditionalData()
		{
			foreach (var item in Values)
			{
				if (item.TrunkPut > 0)
				{
					item.Script = "trunk";
				}

				if (item.TemplateId == 9200000)
				{
					item.Script = "cody";
				}
			}
		}
	}
}
