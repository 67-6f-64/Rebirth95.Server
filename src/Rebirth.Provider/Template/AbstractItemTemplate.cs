using System;
using Rebirth.Common.Tools;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.Exception;

namespace Rebirth.Provider.Template
{
	public abstract class AbstractItemTemplate : AbstractTemplate
	{
		private int _slotMax;
		[ProviderProperty("info/slotMax")]
		public virtual int SlotMax
		{
			get => _slotMax;
			set => _slotMax = !Locked ? value : throw new TemplateAccessException();
		}

		[ProviderProperty("info/cash")]
		public virtual bool Cash { get; set; }

		[ProviderProperty("info/tradeBlock")]
		public virtual bool TradeBlock { get; set; }

		[ProviderProperty("info/notSale")]
		public virtual bool NotSale { get; set; }

		[ProviderProperty("info/quest")]
		public virtual bool Quest { get; set; }

		[ProviderProperty("info/only")]
		public virtual bool Only { get; set; }

		[ProviderProperty("info/price")]
		public virtual int Price { get; set; }

		[ProviderProperty("info/expireOnLogout")]
		public bool ExpireOnLogout { get; set; }

		[ProviderProperty("info/timeLimited")]
		public bool TimeLimited { get; set; }

		[ProviderProperty("info/max")]
		public int Max { get; set; }

		[ProviderProperty("info/time", "spec/time")]
		public int Time { get; set; }

		[ProviderProperty("info/mcType")]
		public int MCType { get; set; }

		public string PickupMessage { get; set; } // custom attribute not in wz

		protected AbstractItemTemplate(int templateId)
			: base(templateId)
		{
			PickupMessage = "";
		}
	}

	public sealed class BridleItemTemplate : AbstractItemTemplate
	{
		/*
		 * 00000000 CItemInfo::BRIDLEITEM struc ; (sizeof=0x3C, align=0x4, copyof_5431)
		   00000000 dwTargetMobID dd ?
		   00000004 nItemID dd ?
		   00000008 rc  tagRECT ?
		   00000018 nCreateItemID dd ?
		   0000001C nCreateItemPeriod dd ?
		   00000020 nCatchPercentageHP dd ?
		   00000024 nBridleMsgType dd ?
		   00000028 fBridleProb dd ?
		   0000002C fBridleProbAdj dd ?
		   00000030 tUseDelay dd ?
		   00000034 sDeleyMsg ZXString<char> ?
		   00000038 sNoMobMsg ZXString<char> ?
		   0000003C CItemInfo::BRIDLEITEM ends
		 */

		public int TargetMobID { get; set; }
		public TagRect Rect { get; set; }
		public int CreateItemID { get; set; }
		public int CreateItemPeriod { get; set; }
		public int CatchPercentageHP { get; set; }
		public int BridleMsgType { get; set; }
		public int BridleProb { get; set; }
		public int BridleProbAdj { get; set; }
		public int UseDelay { get; set; }
		public string DelayMsg { get; set; }
		public string NoMobMsg { get; set; }

		public BridleItemTemplate(int templateId) : base(templateId)
		{

		}
	}

	public sealed class LotteryItemTemplate : AbstractItemTemplate
	{
		/*
		 * 00000000 CItemInfo::LOTTERYITEM struc ; (sizeof=0x8, align=0x4, copyof_5727)
		   00000000 aEntity ZArray<CItemInfo::LOTTERY_ENTITY> ?
		   00000004 nTotalProb dd ?
		   00000008 CItemInfo::LOTTERYITEM ends
		 */

		public LotteryEntity[] Entity { get; set; }
		public int TotalProb { get; set; }

		public LotteryItemTemplate(int templateId) : base(templateId)
		{

		}

		public sealed class LotteryEntity
		{
			/*
			 * 00000000 CItemInfo::LOTTERY_ENTITY struc ; (sizeof=0x1C, align=0x4, copyof_5725)
			   00000000 nItemID dd ?
			   00000004 nProb dd ?
			   00000008 nQuantity dd ?
			   0000000C sEffect ZXString<char> ?
			   00000010 sWorldMsg ZXString<char> ?
			   00000014 nPeriod dd ?
			   00000018 sDateExpire ZXString<char> ?
			   0000001C CItemInfo::LOTTERY_ENTITY ends
			 */

			public int Prob { get; set; }
			public int Quantity { get; set; }
			public string Effect { get; set; }
			public string WorldMsg { get; set; }
			public int Period { get; set; }
			public DateTime DateExpire { get; set; }
		}
	}
}