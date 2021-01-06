using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Characters.Skill;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Item.Consume;

namespace Rebirth.Entities.Shop
{
	public sealed class CShop
	{
		/// <summary>
		/// These are sent in every enter shop packet
		/// </summary>
		private readonly int[] RechargeableItems = new int[]
		{
			2070000, 2070001, 2070002, 2070003, 2070004, 2070005, 2070006, 2070007, 2070008, 2070009,
			2070010, 2070011, 2070012, 2070013, /*2070014,*/ 2070015, 2070016, /*2070017,*/ 2070018, // 14 and 17 dont exist
            2331000, 2332000, // blaze & glaze capsules
            2330000, 2330001, 2330002, 2330003, 2330004, 2330005, 2330006,
		};

		public int dwShopId { get; }

		public List<CShopItem> Items { get; }

		public CShop(int shopId)
		{
			Items = new List<CShopItem>();
			dwShopId = shopId;
		}

		/// <summary>
		/// This will add the basic items that all shops get to the end of the shop item list.
		/// </summary>
		public void AddDefaultItems()
		{
			// magnifying glasses
			{
				var pItem = new CShopItem(2460000) // Magnifying Glass (Basic)
				{
					nPrice = 50,
				};

				Items.Add(pItem);
			}
			{
				var pItem = new CShopItem(2460001) // Magnifying Glass (Average)
				{
					nPrice = 500,
				};

				Items.Add(pItem);
			}
			{
				var pItem = new CShopItem(2460002) // Magnifying Glass (Advanced)
				{
					nPrice = 5000,
				};

				Items.Add(pItem);
			}
			{
				var pItem = new CShopItem(2460003) // Magnifying Glass (Advanced)
				{
					nPrice = 50000,
				};

				Items.Add(pItem);
			}

			// adding ability to recharge items
			foreach (var itemId in RechargeableItems)
			{
				if (ContainsItem(itemId))
					continue;

				var pItem = new CShopItem(itemId);

				Items.Add(pItem);
			};
		}

		public bool ContainsItem(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId) != null;
		public int GetItemPrice(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.nPrice ?? 0;
		public double GetItemUnitPrice(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.dUnitPrice ?? 0;
		public short GetItemQuantity(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.nQuantity ?? 0;
		public int GetTokenPrice(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.nTokenPrice ?? 0;
		public int GetTokenItemID(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.nTokenItemID ?? 0;
		public int GetItemLevelLimited(int nItemId) => Items.FirstOrDefault(ctx => ctx.nItemID == nItemId)?.nLevelLimited ?? 0;


		public static void Handle_UserShopRequest(WvsGameClient c, CInPacket p)
		{
			if (c.ActiveShop is null)
			{
				c.Character.SendMessage("Unable to find active shop. Please re-open the shop or report this to staff.");
				return;
			}

			switch ((ShopReq)p.Decode1()) // nOperation
			{
				case ShopReq.Buy:
					{
						var nPos = p.Decode2(); //Item index within the npc shop lol
						var dwTemplateID = p.Decode4();
						var nCount = p.Decode2();

						BuyItem(c.Character, nPos, dwTemplateID, nCount);
						break;
					}
				case ShopReq.Sell:
					{
						var nSlot = p.Decode2(); //Item inventory index
						var dwTemplateID = p.Decode4();
						var nCount = p.Decode2();

						SellItem(c.Character, nSlot, dwTemplateID, nCount);

						break;
					}
				case ShopReq.Recharge:
					{
						var nSlot = p.Decode2();

						RechargeItem(c.Character, nSlot);
						break;
					}
				case ShopReq.Close:
					{
						Close(c.Character);
						break;
					}
			}
		}

		private static void BuyItem(Character pChar, short nPos, int nItemId, short nCount)
		{
			var cShop = pChar.Socket.ActiveShop;

			// price is 0 for invis items
			if (!cShop.ContainsItem(nItemId)
				|| cShop.GetItemPrice(nItemId) <= 0
				|| cShop.GetItemLevelLimited(nItemId) > pChar.Stats.nLevel)
			{
				pChar.SendMessage("Trying to buy an item that doesn't exist in the shop.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.BuyUnknown));
				return;
			}

			nCount = ItemConstants.is_treat_singly(nItemId) ? (short)1 : nCount;

			var nPricePerCount = cShop.GetItemPrice(nItemId);
			var nFinalPrice = nPricePerCount * nCount;

			if (nFinalPrice > pChar.Stats.nMoney)
			{
				pChar.SendMessage("Trying to buy an item without enough money.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.BuyNoMoney));
				return;
			}

			var nInvType = ItemConstants.GetInventoryType(nItemId);
			if (InventoryManipulator.CountFreeSlots(pChar, nInvType) < 1)
			{
				pChar.SendMessage("Please make some space in your inventory.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.BuyUnknown));
				return;
			}

			var pItem = MasterManager.CreateItem(nItemId, false);

			var slotmax = pItem.SlotMax;

			if (pChar.Skills.Get(false,
				(int)Skills.NIGHTWALKER_JAVELIN_MASTERY,
				(int)Skills.ASSASSIN_JAVELIN_MASTERY) is SkillEntry se)
			{
				slotmax += (short)se.Y_Effect;
			}

			if (slotmax < nCount * cShop.GetItemQuantity(nItemId))
			{
				pChar.SendMessage("Unable to purchase more than one slot of items at a time.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.BuyUnknown));
				return;
			}

			pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.BuySuccess));

			pItem.nNumber = (short)(pItem.IsRechargeable ? slotmax : nCount * cShop.GetItemQuantity(nItemId));

			pChar.SendMessage("final price: " + nFinalPrice);

			InventoryManipulator.InsertInto(pChar, pItem);
			pChar.Modify.GainMeso(-nFinalPrice, false);
		}

		private static void SellItem(Character pChar, short nSlot, int nItemId, short nCount)
		{
			var pItem = InventoryManipulator.GetItem(pChar, ItemConstants.GetInventoryType(nItemId), nSlot);

			if (pItem is null)
			{
				pChar.SendMessage("Trying to sell null item.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.SellIncorrectRequest));
				return;
			}

			if (pItem.CashItem || pItem.NotSale)
			{
				pChar.SendMessage("Cannot trade cash items.");
				return;
			}

			var nCostPerItem = pItem.Template.Price;

			nCount = ItemConstants.is_treat_singly(nItemId) ? (short)1 : nCount;

			var nFinalCost = nCostPerItem * nCount;

			if (pItem.IsRechargeable)
			{
				var dUnitPrice = ((ConsumeItemTemplate)pItem.Template).UnitPrice;

				nFinalCost += (int)Math.Floor(dUnitPrice * pItem.nNumber);
			}

			if (pItem is GW_ItemSlotBundle && !pItem.IsRechargeable)
			{
				if (pItem.nNumber < nCount)
				{
					pChar.SendMessage("Trying to sell more than you possess.");
					pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.SellIncorrectRequest));
					return;
				}

				if (nCount <= 0)
				{
					pChar.SendMessage("Trying to sell negative amount.");
					pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.SellIncorrectRequest));
					return;
				}
			}
			else
			{
				nCount = -1;
			}

			pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.SellSuccess));

			//pChar.SendMessage("final price: " + nFinalCost);

			InventoryManipulator.RemoveFrom(pChar, pItem.InvType, nSlot, nCount);
			pChar.Modify.GainMeso(nFinalCost, false);
		}

		private static void RechargeItem(Character pChar, short nSlot)
		{
			var pItem = InventoryManipulator.GetItem(pChar, InventoryType.Consume, nSlot);

			if (pItem is null)
			{
				pChar.SendMessage("Trying to recharge null item.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.RechargeIncorrectRequest));
				return;
			}

			if (!ItemConstants.IsRechargeableItem(pItem.nItemID))
			{
				pChar.SendMessage("Trying to recharge item that isn't rechargeable.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.RechargeIncorrectRequest));
				return;
			}

			var slotmax = pItem.SlotMax;

			if (pChar.Skills.Get(false,
				(int) Skills.NIGHTWALKER_JAVELIN_MASTERY,
				(int) Skills.ASSASSIN_JAVELIN_MASTERY) is SkillEntry se)
			{
				slotmax += (short)se.Y_Effect;
			}

			if (pItem.nNumber >= slotmax)
			{
				pChar.SendMessage("Trying to recharge item that is fully charged.");
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.RechargeIncorrectRequest));
				return;
			}

			var dUnitPrice = (pItem.Template as ConsumeItemTemplate).UnitPrice;

			var nRechargeNeeded = slotmax - pItem.nNumber;
			var nTotalRechargePrice = (int)Math.Floor(nRechargeNeeded * dUnitPrice);

			if (pChar.Stats.nMoney < nTotalRechargePrice)
			{
				pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.RechargeNoMoney));
				return;
			}

			pChar.Modify.GainMeso(-nTotalRechargePrice);

			pItem.nNumber = slotmax;

			pChar.Modify.Inventory(ctx =>
			{
				ctx.UpdateQuantity(InventoryType.Consume, nSlot, pItem.nNumber);
			});

			pChar.SendPacket(CPacket.CShopDlg.ShopResult(ShopRes.RechargeSuccess));
		}

		private static void Close(Character pChar)
		{
			pChar.Socket.ActiveShop = null;
		}
	}
}
