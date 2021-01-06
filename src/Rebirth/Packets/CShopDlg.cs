using Rebirth.Entities.Shop;
using Rebirth.Network;
using System;
using System.Collections.Generic;
using Rebirth.Common.Types;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CShopDlg
		{
			public static COutPacket OpenShopDlg(int dwNpcTemplateID, List<CShopItem> aItems)
			{
				var p = new COutPacket(SendOps.LP_OpenShopDlg);
				p.Encode4(dwNpcTemplateID);

				p.Encode2((short)aItems.Count); //nCount

				foreach (var item in aItems)
				{
					item.Encode(p);
				}

				return p;
			}

			public static COutPacket ShopResult(ShopRes nResult)
			{
				var p = new COutPacket(SendOps.LP_ShopResult);
				p.Encode1((byte)nResult);

				switch (nResult)
				{
					case ShopRes.LimitLevel_Less: //You must be over lv.%d to purchase this item
					case ShopRes.LimitLevel_More: //You must be under lv.%d to purchase this item
												  //Encode 4

					case ShopRes.ServerMsg:
						//if Encode1
						//  EncodeString message
						//else
						//  Due to an error, the trade did not happen.

						throw new InvalidOperationException();
				}

				return p;
			}
		}
	}
}
