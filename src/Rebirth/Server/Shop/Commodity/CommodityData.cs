using Rebirth.Network;
using Rebirth.Server.Center;
using System.Collections.Generic;
using Rebirth.Common.Types;

namespace Rebirth.Server.Shop.Commodity
{
	public class CommodityData
	{
		public CommodityFlag dwModifiedFlag { get; private set; }

		public int nSN { get; }

		public CommodityData(int nCommoditySerialNumber)
			=> nSN = nCommoditySerialNumber;

		private int _nItemID;
		public int nItemID
		{
			get => _nItemID;
			set
			{
				dwModifiedFlag |= CommodityFlag.ItemID;
				_nItemID = value;
			}
		}

		private short _nCount;
		public short nCount
		{
			get => _nCount;
			set
			{
				dwModifiedFlag |= CommodityFlag.Count;
				_nCount = value;
			}
		}

		private int _nPrice;
		public int nPrice
		{
			get => _nPrice;
			set
			{
				dwModifiedFlag |= CommodityFlag.Price;
				_nPrice = value;
			}
		}

		private bool _bBonus;
		public bool bBonus
		{
			get => _bBonus;
			set
			{
				dwModifiedFlag |= CommodityFlag.Bonus;
				_bBonus = value;
			}

		}

		private byte _nPriority;
		public byte nPriority
		{
			get => _nPriority;
			set
			{
				dwModifiedFlag |= CommodityFlag.Priority;
				_nPriority = value;
			}
		}

		private short _nPeriod;
		/// <summary>
		/// Days until expire
		/// </summary>
		public short nPeriod
		{
			get => _nPeriod;
			set
			{
				dwModifiedFlag |= CommodityFlag.Period;
				_nPeriod = value;
			}
		}

		private short _nReqPOP;
		public short nReqPOP
		{
			get => _nReqPOP;
			set
			{
				dwModifiedFlag |= CommodityFlag.ReqPOP;
				_nReqPOP = value;
			}
		}

		private short _nReqLVL;
		public short nReqLVL
		{
			get => _nReqLVL;
			set
			{
				dwModifiedFlag |= CommodityFlag.ReqLVL;
				_nReqLVL = value;
			}
		}

		private int _nMaplePoint;
		public int nMaplePoint
		{
			get => _nMaplePoint;
			set
			{
				dwModifiedFlag |= CommodityFlag.MaplePoint;
				_nMaplePoint = value;
			}
		}

		private int _nMeso;
		public int nMeso
		{
			get => _nMeso;
			set
			{
				dwModifiedFlag |= CommodityFlag.Meso;
				_nMeso = value;
			}
		}

		private bool _bForPremiumUser;
		public bool bForPremiumUser
		{
			get => _bForPremiumUser;
			set
			{
				dwModifiedFlag |= CommodityFlag.ForPremiumUser;
				_bForPremiumUser = value;
			}
		}

		private SexType _nCommodityGender;
		public SexType nCommodityGender
		{
			get => _nCommodityGender;
			set
			{
				dwModifiedFlag |= CommodityFlag.CommodityGender;
				_nCommodityGender = value;
			}
		}

		private bool _bOnSale;
		public bool bOnSale
		{
			get => _bOnSale;
			set
			{
				dwModifiedFlag |= CommodityFlag.OnSale;
				_bOnSale = value;
			}
		}

		private CommodityClass _nClass;
		public CommodityClass nClass
		{
			get => _nClass;
			set
			{
				dwModifiedFlag |= CommodityFlag.Class;
				_nClass = value;
			}
		}

		private CommodityLimitFlag _nLimit;
		public CommodityLimitFlag nLimit
		{
			get => _nLimit;
			set
			{
				dwModifiedFlag |= CommodityFlag.Limit;
				_nLimit = value;
			}
		}

		private short _nPbCash;
		public short nPbCash
		{
			get => _nPbCash;
			set
			{
				dwModifiedFlag |= CommodityFlag.PbCash;
				_nPbCash = value;
			}
		}

		private short _nPbPoint;
		public short nPbPoint
		{
			get => _nPbPoint;
			set
			{
				dwModifiedFlag |= CommodityFlag.PbPoint;
				_nPbPoint = value;
			}
		}

		private short _nPbGift;
		public short nPbGift
		{
			get => _nPbGift;
			set
			{
				dwModifiedFlag |= CommodityFlag.PbGift;
				_nPbGift = value;
			}
		}

		private List<int> _aPackageSN;
		public List<int> aPackageSN
		{
			get => _aPackageSN;
			set
			{
				dwModifiedFlag |= CommodityFlag.PackageSN;
				_aPackageSN = value;
			}
		}

		/// <summary>
		/// CS_COMMODITY::DecodeModifiedData
		/// </summary>
		/// <param name="p"></param>
		public void Encode(COutPacket p)
		{
			p.Encode4(nSN);
			p.Encode4((int)dwModifiedFlag);

			//MasterManager.Log.Debug((int)dwModifiedFlag);

			if ((dwModifiedFlag & CommodityFlag.ItemID) > 0)
				p.Encode4(nItemID);
			if ((dwModifiedFlag & CommodityFlag.Count) > 0)
				p.Encode2(nCount);
			if ((dwModifiedFlag & CommodityFlag.Priority) > 0)
				p.Encode1(nPriority);
			if ((dwModifiedFlag & CommodityFlag.Price) > 0)
				p.Encode4(nPrice);
			if ((dwModifiedFlag & CommodityFlag.Bonus) > 0)
				p.Encode1(bBonus);
			if ((dwModifiedFlag & CommodityFlag.Period) > 0)
				p.Encode2(nPeriod);
			if ((dwModifiedFlag & CommodityFlag.ReqPOP) > 0)
				p.Encode2(nReqPOP);
			if ((dwModifiedFlag & CommodityFlag.ReqLVL) > 0)
				p.Encode2(nReqLVL);
			if ((dwModifiedFlag & CommodityFlag.MaplePoint) > 0)
				p.Encode4(nMaplePoint);
			if ((dwModifiedFlag & CommodityFlag.Meso) > 0)
				p.Encode4(nMeso);
			if ((dwModifiedFlag & CommodityFlag.ForPremiumUser) > 0)
				p.Encode1(bForPremiumUser);
			if ((dwModifiedFlag & CommodityFlag.CommodityGender) > 0)
				p.Encode1((byte)nCommodityGender);
			if ((dwModifiedFlag & CommodityFlag.OnSale) > 0)
				p.Encode1(bOnSale);
			if ((dwModifiedFlag & CommodityFlag.Class) > 0)
				p.Encode1((byte)nClass);
			if ((dwModifiedFlag & CommodityFlag.Limit) > 0)
				p.Encode1((byte)nLimit);
			if ((dwModifiedFlag & CommodityFlag.PbCash) > 0)
				p.Encode2(nPbCash);
			if ((dwModifiedFlag & CommodityFlag.PbPoint) > 0)
				p.Encode2(nPbPoint);
			if ((dwModifiedFlag & CommodityFlag.PbGift) > 0)
				p.Encode2(nPbGift);
			if ((dwModifiedFlag & CommodityFlag.PackageSN) > 0)
			{
				p.Encode1((byte)aPackageSN.Count);
				foreach (var item in aPackageSN)
				{
					p.Encode4(item);
				}
			}
		}
	}
}
