using Rebirth.Server.Shop.Commodity;
using System.Collections.Generic;
using Rebirth.Common.Types;
using Rebirth.Server.Center;

namespace Rebirth
{
	public static class CashShopConstants
	{
		public static List<CommodityData> ModifiedCommodities { get; }
		public static int[] DisabledItems { get; }

		public static void ApplyModifiedCommodities()
		{
			foreach (var cd in ModifiedCommodities)
			{
				var item = MasterManager.CommodityProvider[cd.nSN];

				if ((cd.dwModifiedFlag & CommodityFlag.ItemID) > 0)
					item.ItemID = cd.nItemID;

				if ((cd.dwModifiedFlag & CommodityFlag.Count) > 0)
					item.Count = cd.nCount;

				if ((cd.dwModifiedFlag & CommodityFlag.Priority) > 0)
					item.Priority = cd.nPriority;

				if ((cd.dwModifiedFlag & CommodityFlag.Price) > 0)
					item.Price = cd.nPrice;

				if ((cd.dwModifiedFlag & CommodityFlag.Bonus) > 0)
					item.Bonus = cd.bBonus ? 1 : 0;

				if ((cd.dwModifiedFlag & CommodityFlag.Period) > 0)
					item.Period = cd.nPeriod;

				if ((cd.dwModifiedFlag & CommodityFlag.ReqPOP) > 0)
					item.ReqPOP = cd.nReqPOP;
				if ((cd.dwModifiedFlag & CommodityFlag.ReqLVL) > 0)
					item.ReqLVL = cd.nReqLVL;
				if ((cd.dwModifiedFlag & CommodityFlag.CommodityGender) > 0)
					item.Gender = (int)cd.nCommodityGender;
				if ((cd.dwModifiedFlag & CommodityFlag.OnSale) > 0)
					item.OnSale = cd.bOnSale;
				if ((cd.dwModifiedFlag & CommodityFlag.Class) > 0)
					item.Classification = (int)cd.nClass;
				if ((cd.dwModifiedFlag & CommodityFlag.PbCash) > 0)
					item.PbCash = cd.nPbCash;
				if ((cd.dwModifiedFlag & CommodityFlag.PbPoint) > 0)
					item.PbPoint = cd.nPbPoint;
				if ((cd.dwModifiedFlag & CommodityFlag.PbGift) > 0)
					item.PbGift = cd.nPbGift;
			}

			foreach (var item in DisabledItems)
			{
				MasterManager.CommodityProvider[item].OnSale = false;
			}
		}

		static CashShopConstants()
		{
			ModifiedCommodities = new List<CommodityData>
			{
				new CommodityData(40000002)
				{
					nItemID = 5222000,
					nPrice = 1850, // regular price for this SN is 2300
					nCount = 1,
					bOnSale = true,
					nCommodityGender = SexType.Either,
					nReqLVL = 0,
					nClass = CommodityClass.Hot,
					nPeriod = 90
				}
			};

			var nonfriendrings = new[]
			{
				20900006, 20900009, 20900010, 20900011, 20900012, 20900013, 20900014, 20900015, 20900016, 20900017,
				20900020, 20900021, 20900022, 20900023, 20900024, 20900025, 20900026, 20900028, 20900035, 20900036,
				20900037, 20900038, 20900041, 20900042, 20900043, 20900044, 20900045, 20900046, 20900047, 20900048,
				20900050, 20900052, 20900054, 20900059, 20900060, 20900061, 20900062, 20900063, 20900064, 20900065,
				20900066, 20900067, 20900069, 20900071, 20900073, 20900075, 20900077, 20900079, 20900080, 20900081,
				20900082, 20900083, 20900084, 20900085, 20900087, 20900088, 20900091, 20900101, 20900115, 20900116,
				20900117
			};

			foreach (var ring in nonfriendrings)
			{
				ModifiedCommodities.Add(new CommodityData(ring)
				{
					nClass = CommodityClass.None,
					nPriority = 0,
				});
			}

			var friendrings = new[]
			{
				20900056, 20900057, 20900058, 20900098, 20900099, 20900100
			};

			foreach (var ring in friendrings)
			{
				ModifiedCommodities.Add(new CommodityData(ring)
				{
					nClass = CommodityClass.Sale,
					nPriority = 10,
					nPrice = 4050 // 10% off
				});
			}

			var pets = new List<CommodityData>
			{
				// re-enabled pets with modified SN (price: 15700)
				new CommodityData(10001482) { nItemID = 5000034, }, // black bunny
				new CommodityData(10001483) { nItemID = 5000039, }, // porcupine
				new CommodityData(10001484) { nItemID = 5000044, }, // orange tiger
				new CommodityData(10001485) { nItemID = 5000055, }, // crystal rudolph
				new CommodityData(10001486) { nItemID = 5000038, }, // white monkey
				new CommodityData(10001487) { nItemID = 5000056, }, // toucan
				new CommodityData(10002064) { nItemID = 5000074, }, // bing monkey
				new CommodityData(10002065) { nItemID = 5000054, }, // snail
				new CommodityData(10002066) { nItemID = 5000060, }, // pink bean
				new CommodityData(10002067) { nItemID = 5000067, }, // weird alien
				new CommodityData(10002068) { nItemID = 5000045, }, // skunk
				//new CommodityData(0) { nItemID = 0, nPrice = 0, },
			};

			foreach (var item in pets)
			{
				item.nCount = 1;
				item.nClass = CommodityClass.New;
				item.nCommodityGender = SexType.Either;
				item.nPeriod = 90;
				item.bOnSale = true;
				item.bBonus = false;
				item.nReqPOP = 0;
				item.nReqLVL = 0;
				item.nLimit = CommodityLimitFlag.None;
				item.nMeso = 0;
				item.bForPremiumUser = false;
				item.aPackageSN = new List<int>(0);
			}

			ModifiedCommodities.AddRange(pets);

			pets.Clear();

			pets = new List<CommodityData>
			{
				// all pets with a classification flag
				new CommodityData(60000024),
				new CommodityData(60000033),
				new CommodityData(60000037),
				new CommodityData(60000038),
				new CommodityData(60000078),
				new CommodityData(60000104),
				new CommodityData(60000105),
				new CommodityData(60000106),
			};

			pets.ForEach(pet =>
			{
				pet.nClass = CommodityClass.None;
				pet.nPeriod = 90;
			});

			ModifiedCommodities.AddRange(pets);

			pets.Clear();

			var disabled_items = new List<int>
			{
				// evan scrolls
				40000000,
				40000001,
				40000002,
				40000003,
				40000004,
				40000005,
				// db scrolls
				40000013,
				40000014,
				40000015,
				40000016,
				40000017,
				40000018,
				// maple tvs
				30100006,
				30100007,
				30100008,
				30100009,
				30100010,
				30100011,
				30100012,
				30100013,
				30100014,
				30100015,
				30100016,
				30100017,
				// ap & sp reset
				30000000,
				30000001,
				30000003,
				30000004,
				30000005,
				30000006,
				30000015,
				// miu miu
				30000018,
				// owl of minerva
				50100007,
				50100008,
				// player shops (not entrusted servants)
				50100000,
				50100012,
				50100026,
				50100027,
				50100028,
				50100029,
				50100030,
				50100031,
				50100032,
				50100033,
				50100034,
				50100035,
				50100036,
				50100037,
				// quest delivery/completion
				50200218,
				50200231,
				50200219,
				50200232,
				// meso sacks
				50200084,
				50200085,
				50200086,
				// miracle cube
				50200211,
				// gachapon & keys
				50200136,// -- keys
				50200137,
				50200003,// -- regular
				50200004,
				50200005,
				50200096,// -- remote
				50200097,
				50200098,
				// rate modifiers
				50200070,
				50200050,
				50200059,
				50200051,
				50200060,
				50200052,
				50200061,
				50200053,
				50200062,
				50200054,
				50200063,
				50200055,
				50200064,
				50200056,
				50200065,
				50200057,
				50200066,
				50200058,
				// character& inventory storage slots
				//50200083, // -- need to get the storage slot ids too
				// wedding
				50400000,
				50400001,
				50400002,
				50400003,
				50400004,
				// more meso sacks and keys in the event category
				10102946,
				10100977,
				10102947,

				10102343,
				10102344,

				// remaining items in special tab
				10102551,
				10102559,
				10102975,
				10102976,
				10102953,
				10102563,
				10102952,

				// items in the Main->New tab
				10002791,
				10002813,
				10002814,
				10002815,
				10002826,
				10002827,
				10002828,
				10002829,
			};

			foreach (var mc in ModifiedCommodities)
			{
				// incase we overwrote one of the blocked SNs with another item
				disabled_items.Remove(mc.nSN);
			}

			DisabledItems = disabled_items.ToArray();
		}
	}
}
