using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Server.Shop
{
	public class CS_BEST
	{
		public int nSoldCount { get; set; }

		public int nCategory { get; set; }
		public int nGender { get; set; }
		public int nCommoditySN { get; set; }
	}

	public class BestItemManager
	{
		private Dictionary<int, CS_BEST> m_aClientBest;


		public BestItemManager()
		{
			m_aClientBest = new Dictionary<int, CS_BEST>();
		}

		public void InsertPurchase(int liCashItemSerialNumber, int nCategory)
		{
			if (m_aClientBest.ContainsKey(liCashItemSerialNumber))
			{
				m_aClientBest[liCashItemSerialNumber].nSoldCount = 1;
			}
			else
			{
				m_aClientBest.Add(liCashItemSerialNumber, new CS_BEST { nSoldCount = 1, nCategory = nCategory, nGender = 0, nCommoditySN = liCashItemSerialNumber });
			}
		}

		public void Sort()
		{
			var newDic = m_aClientBest;
			newDic.OrderByDescending(i => i.Value.nSoldCount);
			m_aClientBest = newDic;
		}
	}
}
