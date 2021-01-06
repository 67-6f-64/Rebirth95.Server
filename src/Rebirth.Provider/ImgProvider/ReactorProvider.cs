using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using WzTools;
using WzTools.FileSystem;
using WzTools.Objects;
using Rebirth.Provider.Template.Npc;
using Rebirth.Provider.Template.Reactor;

namespace Rebirth.Provider.ImgProvider
{
	public sealed class ReactorProvider : AbstractProvider<ReactorTemplate>
	{
		protected override string ProviderName => "Reactor";
		protected override bool LoadFromJSON => false;

		public ReactorProvider(WzFileSystem baseImgDir)
			: base(baseImgDir) { }

		protected override void LoadFromImg(NameSpaceDirectory imgDir)
		{
			foreach (var reactorNode in imgDir.Files)
			{
				var reactorData = reactorNode.Object as WzFileProperty;

				var pEntry = new ReactorTemplate(Convert.ToInt32(
					reactorNode.Name.Substring(0, 7)))
				{
					//Action = reactorData.GetChild("action") as string
				};

				if (reactorData.GetChild("info") is WzProperty infoNode)
				{
					//pEntry.Move = infoNode.GetInt32("move") != 0;
					//if (pEntry.Move)
					//{
					//	pEntry.MoveOnce = infoNode.GetInt32("moveOnce");
					//	pEntry.MoveDelay = infoNode.GetInt32("moveDelay");
					//}

					//pEntry.RequiredHitCount = infoNode.GetInt32("hitCount");
					//pEntry.RemoveInFieldSet = infoNode.GetInt32("removeInFieldSet") != 0;

					pEntry.HitDelay = GetSumDelay(reactorData);
					var link = infoNode.GetInt32("link");

					var linkProp = reactorData;

					if (link != 0)
					{
						var linkString = $"{link.ToString().PadLeft(7, '0')}.img";
						linkProp = imgDir.Files
							.FirstOrDefault(f => f.Name.Equals(linkString))
								?.Object as WzFileProperty;
					}

					for (var i = 0; ; i++)
					{
						var linkData = linkProp.GetChild(i.ToString()) as WzProperty;
						if (linkData is null)
						{
							pEntry.StateCount = i;
							break;
						}

						var stateInfo = new ReactorTemplate.StateInfo();
						stateInfo.HitDelay = GetSumDelay(linkData);

						if (linkData.GetChild("event") is WzProperty eventData)
						{
							stateInfo.TimeOut = eventData.GetInt32("timeOut");

							for (var j = 0; ; j++)
							{
								var eventDataIndex = eventData.GetChild(j.ToString()) as WzProperty;

								if (eventDataIndex is null) break;

								var eventInfo = new ReactorTemplate.StateInfo.EventInfo();

								if (eventDataIndex.HasChild("type"))
									eventInfo.EventType = eventDataIndex.GetInt32("type");
								else eventInfo.EventType = -1;

								if (eventDataIndex.HasChild("state"))
									eventInfo.NextState = eventDataIndex.GetInt32("state");
								else eventInfo.NextState = -1;

								eventInfo.HitDelay = GetSumDelay(eventDataIndex);

								if (eventDataIndex.GetChild("lt") is WzVector2D lt)
								{
									eventInfo.LT = new Point(lt.X, lt.Y);
								}

								if (eventDataIndex.GetChild("rb") is WzVector2D rb)
								{
									eventInfo.RB = new Point(rb.X, rb.Y);
								}

								stateInfo.EventInfos.Add(eventInfo);
							}
						}

						pEntry.StateInfoList.Add(stateInfo);
					}
				}

				AssignProviderAttributes(pEntry, reactorData); // do this last to overwrite any link data

				InsertItem(pEntry);
			}
		}

		private static int GetSumDelay(WzProperty wzProp)
		{
			if (wzProp is null) return -1;

			var nDelay = 0;

			for (var i = 0; ; i++)
			{
				var data = wzProp.GetChild(i.ToString()) as WzProperty;

				if (data is null) break;

				nDelay += data.GetInt32("delay");
			}

			return nDelay;
		}
	}
}
