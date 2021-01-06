using log4net;
using Rebirth.Characters.Inventory;
using Rebirth.Entities.Item;
using Rebirth.Field;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Modify
{
	/// <summary>
	/// Used for functions that are directly used by the client to modify inventory.
	/// Also used for functions that check inventory without modifying it.
	/// </summary>
	public sealed class InventoryManipulator
	{
		public static ILog Log = LogManager.GetLogger(typeof(InventoryManipulator));

		public static bool HasSpace(Character pChar, int nItemID, short nAmount)
		{
			var nTI = ItemConstants.GetInventoryType(nItemID);

			if (nTI == InventoryType.Equip)
			{
				if (pChar.InventoryEquip.GetFreeSlot() > 0)
					return true;
			}
			else
			{
				var inv = GetInventory(pChar, nTI);

				if (inv.GetFreeSlot() > 0)
					return true;

				// determine if item can be merged in with existing slot
				{
					var item = inv.Get(inv.FindItemSlot(nItemID));

					if (item?.nNumber < nAmount + item?.SlotMax)
						return true;
				}
			}
			return false;
		}

		public static bool HasSpace(Character pChar, int[] items)
		{
			var map = new Dictionary<InventoryType, int>();

			foreach (var item in items)
			{
				var nTI = ItemConstants.GetInventoryType(item);
				if (map.ContainsKey(nTI))
				{
					map[nTI]++;
				}
				else
				{
					map.Add(nTI, 1);
				}
			}

			return HasSpace(pChar, map);
		}

		public static bool HasSpace(Character pChar, TempInventory tempInv)
		{
			var map = new Dictionary<InventoryType, int>();

			foreach (var item in tempInv.GetAll())
			{
				if (map.ContainsKey(item.Type))
				{
					map[item.Type]++;
				}
				else
				{
					map.Add(item.Type, 1);
				}
			}

			return HasSpace(pChar, map);
		}

		private static bool HasSpace(Character pChar, Dictionary<InventoryType, int> map)
		{
			foreach (var (type, amount) in map)
			{
				switch (type)
				{
					case InventoryType.Equip:
						if (amount > pChar.InventoryEquip.SpaceAvailable())
						{
							return false;
						}
						break;
					case InventoryType.Consume:
						if (amount > pChar.InventoryConsume.SpaceAvailable())
						{
							return false;
						}
						break;
					case InventoryType.Install:
						if (amount > pChar.InventoryInstall.SpaceAvailable())
						{
							return false;
						}
						break;
					case InventoryType.Etc:
						if (amount > pChar.InventoryEtc.SpaceAvailable())
						{
							return false;
						}
						break;
					case InventoryType.Cash:
						if (amount > pChar.InventoryCash.SpaceAvailable())
						{
							return false;
						}
						break;
				}
			}
			return true;
		}

		public static void UnEquip(Character pChar, short slotFrom, short slotTo)
		{
			//20:40:35[INFO] Recv[CP_UserChangeSlotPositionRequest] [4D 00] [1D A8 02 01] [01] [B3 FB] [01 00] [FF FF]
			//20:40:38[INFO] Recv[CP_UserChangeSlotPositionRequest] [4D 00] [7B B3 02 01] [01] [B3 FB] [01 00] [FF FF]

			var srcEquipInv = GetInventory(pChar, slotFrom);

			var source = srcEquipInv.GetKvp(slotFrom);

			if (slotTo != 0) //Not dropping
			{
				if (!pChar.InventoryEquip.IsFreeSlot(slotTo)) return; // PE
			}

			if (!srcEquipInv.Remove(slotFrom)) return; // false means they passed an empty slot

			var pItem = source.Value;

			if (-slotFrom == (int)BodyPart.BP_SADDLE || -slotFrom == (int)BodyPart.BP_TAMINGMOB)
			{
				pChar.Buffs.Remove(pChar.Stats.SecondaryStats.rRideVehicle);
			}
			else
			{
				// taming stuff cant have option skills
				pChar.Skills.ModifyOptionSkills(pItem, false);
			}

			if (slotTo == 0) //Dropping Item
			{
				pChar.Modify.Inventory(ctx =>
				{
					ctx.Remove(InventoryType.Equip, slotFrom);
				});

				if (ItemConstants.GetInventoryType(pItem.nItemID) == InventoryType.Equip)
				{
					CDropFactory.CreateDropItem(pChar.Field, pChar.Position.CurrentXY, pChar.dwId, pItem);
				}
				else
				{
					//Should never happen????
					pChar.SendMessage($"You unequipped and dropped a non-equip item. Please report to staff: {pItem.nItemID}");
				}
			}
			else //Unequip to inventory
			{
				pChar.InventoryEquip.Add(slotTo, source.Value);

				pChar.Modify.Inventory(ctx =>
				{
					ctx.Move(InventoryType.Equip, slotFrom, slotTo);
				});
			}
		}

		public static void Equip(Character pChar, short slotFrom, short slotTo)
		{
			var itemToEquip = pChar.InventoryEquip.GetKvp(slotFrom);

			if (itemToEquip.Value == null) return; // passed empty slot

			var v1 = Math.Abs(slotTo);
			if (v1 < 200 && v1 >= 100) v1 -= 100;

			if (!ItemConstants.is_correct_bodypart(itemToEquip.Value.nItemID, (BodyPart)v1))
			{
				pChar.SendMessage($"Unable to equip item with ID {itemToEquip.Value.nItemID} to slot {slotTo} ({v1})");
				return;
			}

			var dstEquipInv = GetInventory(pChar, slotTo);

			var itemToUnequip = dstEquipInv.GetKvp(slotTo);

			// remove item from equip inventory
			if (!pChar.InventoryEquip.Remove(itemToEquip.Key)) return; // means theres no item in the given slot

			// remove item from equipped inventory
			if (dstEquipInv.Remove(itemToUnequip.Key))
			{
				pChar.InventoryEquip.Add(itemToEquip.Key, itemToUnequip.Value);
				pChar.Skills.ModifyOptionSkills(itemToUnequip.Value, false);
			}

			pChar.Skills.ModifyOptionSkills(itemToEquip.Value, true);

			pChar.Modify.Inventory(ctx =>
			{
				if (itemToEquip.Value.EquipTemplate.EquipTradeBlock)
				{
					itemToEquip.Value.nAttribute |= ItemAttributeFlags.Untradeable;
					ctx.Remove(InventoryType.Equip, itemToEquip.Key);
					ctx.Add(InventoryType.Equip, slotTo, itemToEquip.Value); // essentially forcing an equip update into a new slot
				}
				else
				{
					ctx.Move(InventoryType.Equip, itemToEquip.Key, slotTo);
				}

				dstEquipInv.Add(slotTo, itemToEquip.Value);
			});
		}

		public static void Drop(Character pChar, InventoryType nInvType, short invSlot, short amount)
		{
			// validate raw input
			if (amount <= 0) return; // PE
			if ((byte)nInvType > 5) return; // cant drop things out of equipped inventories

			var pItem = GetItem(pChar, nInvType, invSlot);

			if (pItem is null) return; // PE or lag

			if (amount < 1) amount = 1; // not neccessarily PE

			if (amount > pItem.nNumber) // TODO maybe anticheat here
			{
				amount = pItem.nNumber;
			}

			if (ItemConstants.is_treat_singly(pItem.nItemID) || amount < 1)
			{
				RemoveFrom(pChar, nInvType, invSlot, -1);

				CDropFactory.CreateDropItem(pChar.Field, pChar.Position.CurrentXY.Clone(), pChar.dwId, pItem);
			}
			else
			{
				var cNewDropItem = pItem.DeepCopy();
				cNewDropItem.nNumber = amount;

				RemoveFrom(pChar, nInvType, invSlot, amount);

				CDropFactory.CreateDropItem(pChar.Field, pChar.Position.CurrentXY.Clone(), pChar.dwId, cNewDropItem);
			}
		}

		/// <summary>
		/// Set a negative amount to delete everything in slot.
		/// </summary>
		/// <param name="pChar"></param>
		/// <param name="invType"></param>
		/// <param name="nPOS"></param>
		/// <param name="nAmount"></param>
		public static void RemoveFrom(Character pChar, InventoryType nTI, short nPOS, short nAmount = 1)
		{
			if (nTI == InventoryType.Equip || nPOS < 0)
			{
				var inv = GetInventory(pChar, nPOS);

				if (inv.IsFreeSlot(nPOS)) return; // PE, slot is empty

				inv.Remove(nPOS);

				pChar.Modify.Inventory(ctx =>
				{
					ctx.Remove(nTI, nPOS);
				});
			}
			else
			{
				var inv = GetInventory(pChar, nTI);

				var pItem = inv.Get(nPOS);

				if (pItem is null) return; // PE, slot is empty

				if (nAmount < 0) // just less, not equal to
				{
					inv.Remove(nPOS);

					pChar.Modify.Inventory(ctx =>
					{
						ctx.Remove(nTI, nPOS);
					});
				}
				else
				{
					if (pItem.nNumber > nAmount) // update quantity
					{
						pItem.nNumber -= nAmount;

						pChar.Modify.Inventory(ctx =>
						{
							ctx.UpdateQuantity(nTI, nPOS, pItem.nNumber);
						});
					}
					else // remove item
					{
						if (pItem.IsRechargeable) // cant remove rechargeables
						{
							pItem.nNumber = 0;

							pChar.Modify.Inventory(ctx =>
							{
								ctx.UpdateQuantity(nTI, nPOS, pItem.nNumber);
							});
						}
						else
						{
							inv.Remove(nPOS);

							pChar.Modify.Inventory(ctx =>
							{
								ctx.Remove(nTI, nPOS);
							});
						}
					}
				}
			}
		}

		public static void Move(Character pChar, InventoryType nTI, short slotFrom, short slotTo)
		{
			if (slotFrom == slotTo) return; // PE

			if (nTI == InventoryType.Equip) // slotFrom and slotTo should never be negative -- movement is not possible between equipped inventories
			{
				var inv = pChar.InventoryEquip;

				if (slotTo > inv.SlotLimit) return; // PE
				if (!inv.Contains(slotFrom)) return; // PE

				inv.Swap(slotFrom, slotTo);

				pChar.Modify.Inventory(ctx =>
				{
					ctx.Move(nTI, slotFrom, slotTo);
				});
			}
			else
			{
				var inv = GetInventory(pChar, nTI);

				if (slotTo > inv.SlotLimit) return; // PE
				if (!inv.Contains(slotFrom)) return; // PE

				pChar.Modify.Inventory(ctx =>
				{
					if (inv.CanMergeSlots(slotFrom, slotTo))
					{
						if (inv.Merge(slotFrom, slotTo))
						{
							ctx.FullMerge(nTI, slotFrom, slotTo, inv.Get(slotTo).nNumber);
						}
						else
						{
							ctx.PartialMerge(nTI, slotFrom, slotTo, inv.Get(slotFrom).nNumber, inv.Get(slotTo).nNumber);
						}
					}
					else
					{
						inv.Swap(slotFrom, slotTo);

						ctx.Move(nTI, slotFrom, slotTo);
					}
				});
			}
		}

		/// <summary>
		/// Adds an item to the proper inventory.
		/// Does not handle inserting items into equipped inventories.
		/// </summary>
		/// <param name="pChar"></param>
		/// <param name="item"></param>
		/// <returns>Slot of newly created item.</returns>
		public static short InsertInto(Character pChar, GW_ItemSlotBase item)//, short nPOS = 0) // specifying the item pos is a pain -- make a separate function if its required in the future
		{
			if (item is null)
			{
				Log.Info($"{pChar} trying to add null item to slot {0}.");
				return 0;
			}

			if (item.InvType == InventoryType.Equip)
			{
				var inv = pChar.InventoryEquip;

				var nSlot = inv.GetFreeSlot();

				if (nSlot <= 0) return 0;

				inv.Add(nSlot, item as GW_ItemSlotEquip);
				pChar.Modify.Inventory(ctx => ctx.Add(item.InvType, nSlot, item));

				return nSlot;
			}
			else
			{
				var inv = GetInventory(pChar, item.InvType);

				if (item.IsRechargeable || item.liSN != 0)
				{
					var nSlot = inv.GetFreeSlot();

					if (nSlot > 0)
					{
						inv.Add(nSlot, item as GW_ItemSlotBundle);

						pChar.Modify.Inventory(ctx =>
						{
							ctx.Add(item.InvType, nSlot, item);
						});
					}

					return nSlot;
				}
				else // item is allowed to be merged
				{
					var nSlot = inv.GetFreeSlot(item.nItemID);

					if (nSlot == 0) return 0;

					var nRemainingCount = item.nNumber;

					pChar.Modify.Inventory(ctx =>
					{
						while (nRemainingCount > 0)
						{
							var existingItem = inv.Get(nSlot);

							if (existingItem is null) // slot is empty
							{
								inv.Add(nSlot, item as GW_ItemSlotBundle);
								ctx.Add(item.InvType, nSlot, item);
								break;
							}

							var diff = (short)(existingItem.SlotMax - existingItem.nNumber);

							if (diff > nRemainingCount) // incoming count fits in item slot
							{
								existingItem.nNumber += nRemainingCount;
								ctx.UpdateQuantity(item.InvType, nSlot, existingItem.nNumber);
								break;
							}
							else // incoming count does not fit in item slot
							{
								existingItem.nNumber = existingItem.SlotMax;
								ctx.UpdateQuantity(item.InvType, nSlot, existingItem.nNumber);
								item.nNumber -= diff;

								nRemainingCount -= diff;

								nSlot = inv.GetFreeSlot(item.nItemID);
							}
						}
					});

					return nSlot; // itll return the last modified slot, we really only check if this is 0 if were adding consumable anyway tho we might wanna alter this in the future
				}
			}
		}

		public static CharacterInventoryItems GetInventory(Character pChar, int nItemID)
			=> GetInventory(pChar, ItemConstants.GetInventoryType(nItemID));

		/// <summary>
		/// Gets item inventory based on inventory type
		/// </summary>
		/// <param name="nTI"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static CharacterInventoryItems GetInventory(Character pChar, InventoryType nTI)
		{
			switch (nTI)
			{
				case InventoryType.Consume:
					return pChar.InventoryConsume;
				case InventoryType.Install:
					return pChar.InventoryInstall;
				case InventoryType.Etc:
					return pChar.InventoryEtc;
				case InventoryType.Cash:
					return pChar.InventoryCash;
				default:
					throw new InvalidOperationException($"Trying to pass unhandled inventory type {nTI}.");
			}
		}

		/// <summary>
		/// Gets the equip inventory based on slot number.
		/// </summary>
		/// <param name="pChar"></param>
		/// <param name="nSlot"></param>
		/// <returns></returns>
		public static CharacterInventoryEquips GetInventory(Character pChar, short nSlot)
		{
			if (nSlot > 0)
				return pChar.InventoryEquip;

			if (nSlot > -100)
				return pChar.EquippedInventoryNormal;

			if (nSlot > -1000)
				return pChar.EquippedInventoryCash;

			if (nSlot > -1100)
				return pChar.aDragonEquipped;

			if (nSlot > -1200)
				return pChar.aMechanicEquipped;

			throw new InvalidOperationException($"Unable to find equip inventory for slot {nSlot}.");
		}

		public static (short, GW_ItemSlotBase) GetAnyItem(Character pChar, int nItemID)
			=> GetAnyItem(pChar, ItemConstants.GetInventoryType(nItemID), nItemID);

		public static (short, GW_ItemSlotBase) GetAnyItem(Character pChar, InventoryType nTI, int nItemID)
		{
			if (nTI == InventoryType.Equip)
			{
				var inv = pChar.InventoryEquip;

				var slot = inv.FindItemSlot(nItemID);
				if (slot != 0)
				{
					return (slot, inv.Get(slot));
				}
			}
			else
			{
				var inv = GetInventory(pChar, nTI);

				var slot = inv.FindItemSlot(nItemID);
				if (slot != 0)
				{
					return (slot, inv.Get(slot));
				}
			}

			return (0, null);
		}

		public static GW_ItemSlotEquip GetItem(Character pChar, BodyPart nPart, bool bCash)
		{
			short slot = (short)-(int)nPart; // TODO clean this up somehow

			if (bCash)
			{
				slot -= 100;
			}

			return GetItem(pChar, InventoryType.Equip, slot) as GW_ItemSlotEquip;
		}

		public static GW_ItemSlotBase GetItem(Character pChar, InventoryType nTI, short nPOS)
		{
			if (nTI == InventoryType.Equip || nPOS < 0)
			{
				var inv = GetInventory(pChar, nPOS);

				return inv.Get(nPOS);
			}
			else
			{
				var inv = GetInventory(pChar, nTI);

				return inv.Get(nPOS);
			}
		}

		public static bool RemoveQuantity(Character pChar, int nItemID, short nAmount) // TODO merge this function with RemoveFrom(...)
		{
			var (nPOS, pItemRaw) = GetAnyItem(pChar, nItemID);

			if (pItemRaw?.nNumber >= nAmount)
			{
				RemoveFrom(pChar, pItemRaw.InvType, nPOS, nAmount);
				return true;
			}
			return false;
		}

		public static (short, GW_ItemSlotBase) GetItemByCashSN(Character pChar, InventoryType nInvType, long liCashItemSN)
		{
			if (nInvType == InventoryType.Equip)
			{
				var inv = pChar.InventoryEquip;

				var slot = inv.FindItemSlotByCashSN(liCashItemSN);
				if (slot != 0)
				{
					return (slot, inv.Get(slot));
				}
			}
			else
			{
				var inv = GetInventory(pChar, nInvType);

				var slot = inv.FindItemSlotByCashSN(liCashItemSN);
				if (slot != 0)
				{
					return (slot, inv.Get(slot));
				}
			}

			return (0, null);
		}

		public static bool ContainsAny(Character pChar, params int[] itemIdRange)
		{
			foreach (var item in itemIdRange.ToList())
			{
				if (GetAnyItem(pChar, (byte)ItemConstants.GetInventoryType(item)).Item2 != null)
					return true;
			}
			return false;
		}

		public static bool ContainsItem(Character pChar, int itemId, short amount = 1)
		{
			var (_, item) = GetAnyItem(pChar, itemId);

			if (item is GW_ItemSlotEquip)
			{
				return true;
			}
			else if (item is GW_ItemSlotBundle isb)
			{
				if (isb.nNumber >= amount)
					return true;
			}

			return false;
		}

		public static byte CountFreeSlots(Character pChar, InventoryType nInvType)
		{
			if (nInvType == InventoryType.Equip)
			{
				var inv = pChar.InventoryEquip;
				return (byte)(inv.SlotLimit - inv.Count);
			}
			else
			{
				var inv = GetInventory(pChar, nInvType);
				return (byte)(inv.SlotLimit - inv.Count);
			}
		}

		public static bool ItemEquipped(Character pChar, int nItemID)
		{
			if (pChar.EquippedInventoryNormal.ContainsItem(nItemID))
				return true;

			var idx = nItemID / 10000;

			if (idx >= 160 && idx <= 165)
				return pChar.aMechanicEquipped.ContainsItem(nItemID);

			if (idx >= 194 && idx <= 197)
				return pChar.aDragonEquipped.ContainsItem(nItemID);

			return pChar.EquippedInventoryCash.ContainsItem(nItemID); // idk why we would even be checking for equipped cash items
		}

		/// <summary>
		/// Removes one item from char inventory that meets the given condition.
		/// Returns true if successful, otherwise false.
		/// NOTE: Can only be used with unequipped inventories.
		/// </summary>
		public static bool RemoveFrom(Character pChar, InventoryType nInvType, short nAmount, Predicate<GW_ItemSlotBase> predicate)
		{
			if (nInvType == InventoryType.Equip)
			{
				if (nAmount > 1) // idk why you would need to remove more than one equip
					return false;

				foreach (var item in pChar.InventoryEquip)
				{
					if (predicate.Invoke(item.Value))
					{
						return RemoveQuantity(pChar, item.Value.nItemID, nAmount);
					}
				}
			}
			else
			{
				foreach (var item in GetInventory(pChar, nInvType))
				{
					if (predicate.Invoke(item.Value))
					{
						if (RemoveQuantity(pChar, item.Value.nItemID, nAmount))
							return true;
					}
				}
			}

			return false;
		}

		public static void SortInventory(Character pChar, InventoryType nInvType)
		{
			if (nInvType == InventoryType.Equip)
			{
				var inventory = pChar.InventoryEquip;
				var newInventory = inventory.ToList();
				inventory.Dispose();

				short i = 1;
				pChar.Modify.Inventory(ctx =>
				{
					newInventory.ForEach(item => ctx.Remove(nInvType, item.Key));

					foreach (var item in newInventory
						.OrderBy(_item => _item.Value.nItemID))
					{
						inventory.Add(i, item.Value);
						ctx.Add(nInvType, i, item.Value);
						i += 1;
					}
				});
			}
			else
			{
				var inventory = GetInventory(pChar, nInvType);
				var newInventory = inventory.ToList();
				inventory.Dispose();

				short i = 1;

				pChar.Modify.Inventory(ctx =>
				{
					newInventory.ForEach(item => ctx.Remove(nInvType, item.Key));

					foreach (var item in newInventory
						.OrderBy(_item => _item.Value.nItemID))
					{
						inventory.Add(i, item.Value);
						ctx.Add(nInvType, i, item.Value);
						i += 1;
					}
				});
			}


		}

		public static void GatherInventory(Character pChar, InventoryType nInvType)
		{
			if (nInvType == InventoryType.Equip)
			{
				var inventory = pChar.InventoryEquip;
				var newInventory = inventory.ToList().OrderBy(item => item.Key);
				inventory.Dispose();

				short i = 1;

				pChar.Modify.Inventory(ctx =>
				{
					newInventory.ForEach(item => ctx.Remove(nInvType, item.Key));

					foreach (var item in newInventory)
					{
						inventory.Add(i, item.Value);
						ctx.Add(nInvType, i, item.Value);

						i += 1;
					}
				});
			}
			else
			{
				var inventory = GetInventory(pChar, nInvType);
				var newInventory = inventory.ToList().OrderBy(item => item.Key);
				inventory.Dispose();

				short i = 1;

				pChar.Modify.Inventory(ctx =>
				{
					newInventory.ForEach(item => ctx.Remove(nInvType, item.Key));

					foreach (var item in newInventory)
					{
						inventory.Add(i, item.Value);
						ctx.Add(nInvType, i, item.Value);

						i += 1;
					}
				});
			}
		}
	}
}
