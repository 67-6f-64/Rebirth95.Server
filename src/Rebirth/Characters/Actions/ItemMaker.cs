using log4net;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities.Item;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.Etc;
using Rebirth.Provider.Template.Item.Etc;
using Rebirth.Tools;

namespace Rebirth.Characters.Actions
{
	public sealed class ItemMaker
	{
		public static ILog Log = LogManager.GetLogger(typeof(ItemMaker));

		public static void OnPacket(WvsGameClient c, CInPacket p)
		{
			var nRecipeClass = p.Decode4();
			var nItemID = p.Decode4();

			switch (nRecipeClass)
			{
				case 1:
				case 2: // create
					var bMounted = p.Decode1();
					var nNumGem_Mounted = p.Decode4();

					var aGemSlot_pItemp = new List<int>();

					for (int i = 0; i < nNumGem_Mounted; i++)
					{
						aGemSlot_pItemp.Add(p.Decode4());
					}

					HandleCreateItemOp(c.Character, nItemID, bMounted > 0, aGemSlot_pItemp);

					break;
				case 3: // build monster crystal

					HandleBuildMonsterCrystalOp(c.Character, nItemID);

					break;
				case 4: // disassemble

					var nInvType = (InventoryType)p.Decode4();
					var nSlotPosition = p.Decode4();

					HandleDisassembleOp(c.Character, nInvType, nItemID, nSlotPosition);

					break;
				default:
					Log.Info("Found unhandled ItemMakeRequest opcode : " + nRecipeClass);
					break;
			}

			c.SendPacket(MakerRelease());
		}

		private static void HandleDisassembleOp(Character pChar, InventoryType nInvType, int nItemID, int nSlotPos)
		{
			var pItem = InventoryManipulator.GetItem(pChar, nInvType, (short)nSlotPos);

			if (pItem is GW_ItemSlotEquip pItemEquip)
			{
				var nCost = pItemEquip.Template.Price * 0.1363;
			}
			else
			{

			}
		}

		private static void HandleBuildMonsterCrystalOp(Character pChar, int nItemID)
		{

		}

		private static void HandleCreateItemOp(Character pChar, int nItemID, bool bStimulantUsed, List<int> aGems)
		{
			// BEGIN VERIFICATION

			//var pMakerItem = MasterManager.EtcTemplates.MakerData(nItemID);
			var makerItem = MasterManager.ItemMakeTemplates[nItemID];

			// verify maker item exists
			if (makerItem is null)
			{
				pChar.SendMessage("Null item maker template.");
				return;
			}

			// verify player is high enough level to use recipe
			if (makerItem.ReqLevel > pChar.Stats.nLevel + 6)
			{
				pChar.SendMessage("Verify the item level you're trying to craft is no more than 6 above your own.");
				return;
			}

			// verify player has enough meso
			if (makerItem.Meso > pChar.Stats.nMoney)
			{
				pChar.SendMessage("Verify you have the correct amount of mesos.");
				return;
			}

			var pCharMakerSkill = pChar.Skills.Get(Common.GameLogic.SkillLogic.get_novice_skill_as_race(Common.GameLogic.SkillLogic.NoviceSkillID.MakerSkill, pChar.Stats.nJob));

			// verify player maker skill level is high enough
			if (makerItem.ReqSkillLevel > (pCharMakerSkill?.nSLV ?? 0))
			{
				pChar.SendMessage($"Verify maker skill level. {makerItem.ReqSkillLevel} < {(pCharMakerSkill?.nSLV ?? 0)}");
				return;
			}

			var (nStimulantItemSlot, pStimulantItem) = InventoryManipulator.GetAnyItem(pChar, InventoryType.Etc, makerItem.CatalystID);

			// verify stimulant (catalyst) exists in player inventory
			if (bStimulantUsed && pStimulantItem is null)
			{
				pChar.SendMessage("Verify you possess the correct stimulant item.");
				return;
			}

			// verify equip slot availability
			if (InventoryManipulator.CountFreeSlots(pChar, InventoryType.Equip) < 1)
			{
				pChar.SendMessage("Please make more room in your equip inventory.");
				return;
			}

			if (InventoryManipulator.CountFreeSlots(pChar, InventoryType.Etc) < makerItem.RandomReward.Length)
			{
				pChar.SendMessage("Please make more room in your etc inventory.");
				return;
			}

			// verify required items exist in player inventory
			if (makerItem.Recipe.Any(item => !InventoryManipulator.ContainsItem(pChar, item.ItemID, (short)item.Count)))
			{
				pChar.SendMessage("Verify you possess all the required components.");
				return;
			}

			var aGemItemIds = new List<int>();
			var aGemItemTypes = new List<int>();

			// remove duplicate items/types and get item slots for gems
			foreach (var entry in aGems)
			{
				var (nGemSlot, pGemItem) = InventoryManipulator.GetAnyItem(pChar, InventoryType.Etc, entry);

				if (pGemItem is null || nGemSlot == 0 || !(pGemItem.Template is GemEffectTemplate))
				{
					continue;
				}

				// idk how it would be 0 but we check anyway
				if (!aGemItemTypes.Contains(entry / 100)
					&& !aGemItemIds.Contains(pGemItem.nItemID)
					&& pGemItem.nNumber > 0)
				{
					aGemItemIds.Add(pGemItem.nItemID);
					aGemItemTypes.Add(entry / 100);
				}
			}

			// END VERIFICATION

			// BEGIN RECIPE PROCESSING

			// remove meso cost from inventory
			if (makerItem.Meso > 0)
			{
				pChar.Modify.GainMeso(-makerItem.Meso);
			}

			// remove stimulant from inventory
			if (bStimulantUsed)
			{
				InventoryManipulator.RemoveQuantity(pChar, pStimulantItem.nItemID, 1);
			}

			// remove recipe items from inventory
			foreach (var item in makerItem.Recipe)
			{
				InventoryManipulator.RemoveQuantity(pChar, item.ItemID, (short)item.Count);
			}

			var bSuccess = true;

			if (bStimulantUsed && Constants.Rand.Next(100) >= 90)
			{
				bSuccess = false;
			}
			else
			{
				// BEGIN MAKER ITEM CREATION

				var pNewItemRaw = MasterManager.CreateItem(makerItem.TemplateId);

				if (pNewItemRaw is GW_ItemSlotEquip pNewItemEquip)
				{
					pNewItemEquip.RemainingUpgradeCount = (byte)makerItem.TUC;

					// remove gems from inventory
					foreach (var nGemItemId in aGemItemIds)
					{
						var pGemItemRaw = InventoryManipulator.GetAnyItem(pChar, InventoryType.Etc, nGemItemId).Item2;

						if (pGemItemRaw.Template is GemEffectTemplate pGemItem)
						{
							// gems only have one modifier each
							if (pGemItem.incPAD > 0)
								pNewItemEquip.niPAD += (short)(pGemItem.incPAD * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incPDD > 0)
								pNewItemEquip.niPDD += (short)(pGemItem.incPDD * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incMAD > 0)
								pNewItemEquip.niMAD += (short)(pGemItem.incMAD * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incMDD > 0)
								pNewItemEquip.niMDD += (short)(pGemItem.incMDD * (bStimulantUsed ? 2 : 1));

							else if (pGemItem.incSTR > 0)
								pNewItemEquip.niSTR += (short)(pGemItem.incSTR * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incINT > 0)
								pNewItemEquip.niINT += (short)(pGemItem.incINT * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incDEX > 0)
								pNewItemEquip.niDEX += (short)(pGemItem.incDEX * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incLUK > 0)
								pNewItemEquip.niLUK += (short)(pGemItem.incLUK * (bStimulantUsed ? 2 : 1));

							else if (pGemItem.incACC > 0)
								pNewItemEquip.niACC += (short)(pGemItem.incACC * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incEVA > 0)
								pNewItemEquip.niEVA += (short)(pGemItem.incEVA * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incMaxHP > 0)
								pNewItemEquip.niMaxHP += (short)(pGemItem.incMaxHP * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incMaxMP > 0)
								pNewItemEquip.niMaxMP += (short)(pGemItem.incMaxMP * (bStimulantUsed ? 2 : 1));

							else if (pGemItem.incJump > 0)
								pNewItemEquip.niJump += (short)(pGemItem.incJump * (bStimulantUsed ? 2 : 1));
							else if (pGemItem.incSpeed > 0)
								pNewItemEquip.niSpeed += (short)(pGemItem.incSpeed * (bStimulantUsed ? 2 : 1));

							else if (pGemItem.incReqLevel < 0)
								pNewItemEquip.nLevel = (byte)(pNewItemEquip.nLevel + (pGemItem.incReqLevel * (bStimulantUsed ? 2 : 1)));

							else if (pGemItem.RandOption > 0)
							{
								var gX = new GaussianRandom();

								var nRange = pGemItem.RandOption * (bStimulantUsed ? 2 : 1);

								if (pNewItemEquip.niMaxHP > 0)
									pNewItemEquip.niMaxHP = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niMaxHP, nRange, false));
								if (pNewItemEquip.niMaxMP > 0)
									pNewItemEquip.niMaxMP = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niMaxMP, nRange, false));

								if (pNewItemEquip.niPAD > 0)
									pNewItemEquip.niPAD = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niPAD, nRange, false));
								if (pNewItemEquip.niMAD > 0)
									pNewItemEquip.niMAD = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niMAD, nRange, false));
								if (pNewItemEquip.niPDD > 0)
									pNewItemEquip.niPDD = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niPDD, nRange, false));
								if (pNewItemEquip.niMDD > 0)
									pNewItemEquip.niMDD = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niMDD, nRange, false));

								if (pNewItemEquip.niSpeed > 0)
									pNewItemEquip.niSpeed = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niSpeed, nRange, false));
								if (pNewItemEquip.niJump > 0)
									pNewItemEquip.niJump = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niJump, nRange, false));
							}
							// pNewItemEquip.ApplyRandStatOption(pGemItem.RandOption * (bStimulantUsed ? 2 : 1), false);
							else if (pGemItem.RandStat > 0)
							{
								var gX = new GaussianRandom();

								var nRange = pGemItem.RandStat * (bStimulantUsed ? 2 : 1);

								if (pNewItemEquip.niSTR > 0)
									pNewItemEquip.niSTR = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niSTR, nRange, false));
								if (pNewItemEquip.niLUK > 0)
									pNewItemEquip.niLUK = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niLUK, nRange, false));
								if (pNewItemEquip.niINT > 0)
									pNewItemEquip.niINT = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niINT, nRange, false));
								if (pNewItemEquip.niDEX > 0)
									pNewItemEquip.niDEX = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niDEX, nRange, false));

								if (pNewItemEquip.niACC > 0)
									pNewItemEquip.niACC = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niACC, nRange, false));
								if (pNewItemEquip.niEVA > 0)
									pNewItemEquip.niEVA = (short)Math.Max(0, gX.GaussianDistributionVariation(pNewItemEquip.niEVA, nRange, false));
							}
						}
					}

					InventoryManipulator.InsertInto(pChar, pNewItemEquip);
				}
				else
				{
					pNewItemRaw.nNumber = (short)makerItem.ItemNum;
					InventoryManipulator.InsertInto(pChar, pNewItemRaw);
				}

				// remove gems if any
				foreach (var nGemItemId in aGemItemIds)
				{
					InventoryManipulator.RemoveQuantity(pChar, nGemItemId, 1); //InventoryManipulator.RemoveFrom(pChar, (byte)InventoryType.Etc, itemSlot, 1);
				}

				foreach (var entry in makerItem.RandomReward)
				{
					if (Constants.Rand.Next(100) >= entry.Prob) continue;

					var pRandRewardItem = MasterManager.CreateItem(entry.ItemID);

					pRandRewardItem.nNumber = (short)entry.ItemNum;

					InventoryManipulator.InsertInto(pChar, pRandRewardItem);
				}

				// END MAKER ITEM CREATION
			}

			// END RECIPE PROCESSING

			// SEND RESPONSE PACKETS

			pChar.SendPacket(CreateItemResponse(bSuccess, bStimulantUsed, makerItem, aGemItemIds));

			pChar.SendPacket(MakerItemEffectLocal(bSuccess));
			pChar.Field.Broadcast(MakerItemEffectRemote(pChar.dwId, bSuccess));
		}

		private static COutPacket CreateItemResponse(bool bSuccess, bool bCatalystUsed, ItemMakeTemplate pItemMakeInfo, List<int> aGemsUsed)
		{
			var p = new COutPacket(SendOps.LP_UserMakerResult);
			p.Encode4(bSuccess ? 0 : 1);
			p.Encode4(1); // op code 
			p.Encode1(!bSuccess);
			if (bSuccess)
			{
				p.Encode4(pItemMakeInfo.TemplateId);
				p.Encode4(pItemMakeInfo.ItemNum);
			}

			p.Encode4(pItemMakeInfo.Recipe.Length);

			foreach (var item in pItemMakeInfo.Recipe)
			{
				p.Encode4(item.ItemID);
				p.Encode4(item.Count);
			}

			p.Encode4(aGemsUsed.Count);
			foreach (var item in aGemsUsed)
			{
				p.Encode4(item);
			}

			p.Encode4(bCatalystUsed ? 1 : 0);
			if (bCatalystUsed)
			{
				p.Encode4(pItemMakeInfo.CatalystID);
			}

			p.Encode4(pItemMakeInfo.Meso);
			return p;
		}

		private static COutPacket DisassembleItemResponse(int nDisasssembledItemID, int nMesoCost, List<KeyValuePair<int, int>> aItemsGained)
		{
			var p = new COutPacket(SendOps.LP_UserMakerResult);
			p.Encode4(1);
			p.Encode4(4); // op code

			p.Encode4(nDisasssembledItemID);

			p.Encode4(aItemsGained.Count);

			foreach (var kvp in aItemsGained)
			{
				p.Encode4(kvp.Key);
				p.Encode4(kvp.Value);
			}

			p.Encode4(nMesoCost);
			return p;
		}

		private static COutPacket BuildMonsterCrystalResponse(int nItemGained, int nItemLost)
		{
			var p = new COutPacket(SendOps.LP_UserMakerResult);
			p.Encode4(1);
			p.Encode4(3); // op code
			p.Encode4(nItemGained);
			p.Encode4(nItemLost);
			return p;
		}

		private static COutPacket MakerRelease()
		{
			var p = new COutPacket(SendOps.LP_UserMakerResult);
			p.Encode4(0);
			p.Encode4(0);
			p.Encode4(0);
			p.Encode4(0);
			return p;
		}

		public static COutPacket MakerItemEffectLocal(bool bSuccess)
		{
			var p = new COutPacket(SendOps.LP_UserEffectLocal);
			p.Encode1((byte)UserEffect.ItemMaker);
			p.Encode4(bSuccess ? 0 : 1);
			return p;
		}

		public static COutPacket MakerItemEffectRemote(int dwCharId, bool bSuccess)
		{
			var p = new COutPacket(SendOps.LP_UserEffectRemote);
			p.Encode4(dwCharId);
			p.Encode1((byte)UserEffect.ItemMaker); // local and remote are same
			p.Encode4(bSuccess ? 0 : 1);
			return p;
		}
	}
}