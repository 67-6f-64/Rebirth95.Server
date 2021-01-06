using Npgsql;
using Rebirth.Characters;
using Rebirth.Entities.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Rebirth.Characters.Skill;
using Rebirth.Common.Types;

namespace Rebirth.Server.Center
{
	public class ShopManager
	{
		private readonly Dictionary<int, int> m_aNpcToShop;
		private readonly Dictionary<int, CShop> m_aShops;

		private bool bLoaded;

		public ShopManager()
		{
			m_aNpcToShop = new Dictionary<int, int>();
			m_aShops = new Dictionary<int, CShop>();
		}

		public int Load()
		{
			if (bLoaded) return 0;
			bLoaded = true;

			LoadShops();
			LoadItems();

			return m_aShops.Count;
		}

		private void LoadShops()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT shopid, npcid FROM {Constants.DB_All_World_Schema_Name}.shops", conn))
				using (var x = cmd.ExecuteReader())
				{
					while (x.Read())
					{
						var nShopId = x.GetInt32(0);
						var nNpcId = x.GetInt32(1);

						if (m_aNpcToShop.ContainsKey(nNpcId))
						{
							//Duplicate ID
							continue;
						}

						m_aNpcToShop.Add(nNpcId, nShopId);
					}
				}
			}

			var aShopIds = m_aNpcToShop.Values.Distinct();

			foreach (var nShopId in aShopIds)
			{
				var pShop = new CShop(nShopId);
				m_aShops.Add(nShopId, pShop);
			}
		}

		private void LoadItems()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT shopitem_id, shopid, itemid, price, discountrate, tokenitemid, tokenprice, levellimited FROM {Constants.DB_All_World_Schema_Name}.shopitems", conn))
				using (var x = cmd.ExecuteReader())
				{
					while (x.Read())
					{
						var nShopItemId = x.GetInt32(0);
						var nShopId = x.GetInt32(1);

						//TODO: Set position field in pItem and Sort()
						var pItem = new CShopItem(x.GetInt32(2), x.GetInt32(3))
						{
							nDiscountRate = (byte)x.GetInt32(4),
							nTokenItemID = x.GetInt32(5),
							nTokenPrice = x.GetInt32(6),
							nLevelLimited = x.GetInt32(7)
						};


						if (!m_aShops.ContainsKey(nShopId))
						{
							//Missing ID
							continue;
						}

						m_aShops[nShopId].Items.Add(pItem);
					}
				}
			}

			foreach (var item in m_aShops.Values)
			{
				item.AddDefaultItems();
			}
		}

		public bool HasShop(int dwNpcTemplateId) => m_aNpcToShop.ContainsKey(dwNpcTemplateId);
		public CShop GetShop(int dwNpcTemplateId)
		{
			if (!m_aNpcToShop.ContainsKey(dwNpcTemplateId)) return null;
			var key = m_aNpcToShop[dwNpcTemplateId];

			if (!m_aShops.ContainsKey(key)) return null;

			return m_aShops[key];
		}

		public void InitUserShop(Character pChar, int dwNpcTemplateId, CShop pShop = null)
		{
			//if (pChar.Socket.ActiveShop != null)
			//pChar.SendMessage("Shop is already open");

			if (pShop is null)
				pShop = GetShop(dwNpcTemplateId);

			if (pShop is null)
				pChar.SendMessage("Input shop doesnt exist");

			pChar.Socket.ActiveShop = pShop;

			if (pChar.Skills.Get(false,
				(int)Skills.NIGHTWALKER_JAVELIN_MASTERY,
				(int)Skills.ASSASSIN_JAVELIN_MASTERY) is SkillEntry se)
			{
				foreach (var item in pShop.Items)
				{
					if (item.dUnitPrice > 0)
					{
						item.nMaxPerSlot += (short)se.Y_Effect;
						item.nQuantity = item.nMaxPerSlot;
					}
				}
			}

			pChar.SendPacket(CPacket.CShopDlg.OpenShopDlg(dwNpcTemplateId, pShop.Items));
		}
	}
}
