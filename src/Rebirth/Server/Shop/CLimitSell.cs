using Rebirth.Network;
using System.Collections.Generic;

namespace Rebirth.Server.Shop
{
	public class CLimitSell
	{
		private static readonly List<LimitGoods> aLimitGoods;

		// CLimitSell *__cdecl TSingleton<CLimitSell>::GetInstance()
		static CLimitSell()
		{
			aLimitGoods = new List<LimitGoods>();

			aLimitGoods.Add(new LimitGoods()
			{
				nItemID = 1040077,
				nState = -1,
				nOriginCount = 100,
				nRemainCount = 100,
				dwConditionFlag = 1,
				nDateStart = 0,
				nDateEnd = 0,
				nHourStart = 0,
				nHourEnd = 0,
			});

			aLimitGoods[0].nSN[0] = 20400012;

			for (int i = 0; i < 7; i++)
			{
				aLimitGoods[0].abWeek[i] = 1;
			}
		}

		public static void Encode(COutPacket p)
		{
			aLimitGoods.Add(new LimitGoods()
			{
				nItemID = 1040077,
				nState = -1,
				nOriginCount = 100,
				nRemainCount = 100,
				dwConditionFlag = 3,
				nDateStart = 10,
				nDateEnd = 25,
				nHourStart = 0,
				nHourEnd = 0,
			});

			aLimitGoods[1].nSN[0] = 20400012;

			//for (int i = 0; i < 7; i++)
			//{
			//	aLimitGoods[0].abWeek[i] = 1;
			//}

			var stockItems = new List<KeyValuePair<int, int>>();

			foreach (var item in aLimitGoods)
			{
				foreach (var itemSerialNumber in item.nSN)
				{
					if (itemSerialNumber != 0)
						stockItems.Add(new KeyValuePair<int, int>(itemSerialNumber, -1));
				}
			}

			//p.Encode2((short)stockItems.Count);

			//foreach (var item in stockItems)
			//{
			//	p.Encode4(item.Key);
			//	p.Encode4(item.Value);
			//}

			p.Encode2(1);
			p.Encode4(20400012);
			p.Encode4(-1);

			p.Encode2((short)aLimitGoods.Count);

			foreach (var item in aLimitGoods)
			{
				item.Encode(p);
			}
		}

		public class LimitGoods
		{
			/* struct CS_LIMITGOODS || BMS
				{
					int nItemID;
					int nSN[10];
					int nState;
					int nOriginCount;
					int nRemainCount;
					unsigned int dwConditionFlag;
					int nDateStart;
					int nDateEnd;
					int nHourStart;
					int nHourEnd;
					int abWeek[7];
				}; */

			public int nItemID { get; set; }
			public int[] nSN { get; } = new int[10]; // 10 ints even tho its SN
			public int nState { get; set; }

			//CS_LimitGoodsState_NoGoods = 1
			//CS_LimitGoodsState_NotAvailableTime = 2
			//CS_LimitGoodsState_SearchErr = 3
			//CS_LimitGoodsState_NotLimitGoods = 0FFFFFFFEh (-2)
			//CS_LimitGoodsState_Enough  = 0FFFFFFFFh (-1)

			public int nOriginCount { get; set; }
			public int nRemainCount { get; set; }
			public short dwConditionFlag { get; set; }

			//CS_LimitGoodsState_CountLimited  = 1
			//CS_LimitGoodsState_DateLimited  = 2
			//CS_LimitGoodsState_WeekLimited  = 4
			//CS_LimitGoodsState_HourLimited  = 8

			public int nDateStart { get; set; }
			public int nDateEnd { get; set; }
			public int nHourStart { get; set; }
			public int nHourEnd { get; set; }
			public int[] abWeek { get; } = new int[7];

			public void Encode(COutPacket p)
			{
				//if (nLimitGoodsCount > 0) || BMS
				//{
				//	v6 = 104 * nLimitGoodsCount; 
				//	v7 = ZArray < CS_LIMITGOODS >::operator CS_LIMITGOODS * (&aLimitGoods);
				//	COutPacket::EncodeBuffer(oPacket, v7, v6);
				//}

				// CInPacket::DecodeBuffer(v2, v6->a, 104 * v4); // v95

				p.Encode4(nItemID); // 4
				foreach (var sn in nSN) // 44
				{
					p.Encode4(sn);
				}
				p.Encode4(nState); // 48
				p.Encode4(nOriginCount); // 52
				p.Encode4(nRemainCount); // 56
				p.Encode4(dwConditionFlag); // 60
				p.Encode4(nDateStart); // 64
				p.Encode4(nDateEnd); // 68
				p.Encode4(nHourStart); // 72
				p.Encode4(nHourEnd); // 76
				foreach (var day in abWeek) // 104
				{
					p.Encode4(day);
				}
			}
		}
	}
}
