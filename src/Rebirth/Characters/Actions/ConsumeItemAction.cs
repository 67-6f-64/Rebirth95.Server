using log4net;
using Rebirth.Characters.Modify;
using Rebirth.Characters.Skill;
using Rebirth.Entities.Item;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Field.FieldTypes;
using Rebirth.Game;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Provider.Template.Item.ItemOption;
using Rebirth.Tools;

namespace Rebirth.Characters.Actions
{
	/**
     * Handles all the 200xxxx-250xxxx item actions.
     */
	public sealed class ConsumeItemAction
	{
		public static ILog Log = LogManager.GetLogger(typeof(ConsumeItemAction));

		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		public ConsumeItemAction(int c)
		{
			dwParentID = c;
		}

		/**
         * Consumeable item buffs will be routed here.
         * This includes HP/MP items along with temp stat increase items
         */
		public void UsePotion(int nItemID, short nPOS)
		{
			if (Parent.Stats.nHP <= 0) return;

			if (InventoryManipulator.GetItem(Parent, InventoryType.Consume, nPOS)?.Template is ConsumeItemTemplate template)
			{
				if (Parent.Field is CField_Battlefield bf) // if not remove item anyway cuz they shouldnt have it
				{
					// TODO effects => Effect/BasicEff/ItemSkill/Success

					if (template.BFSkill < 0)
					{
						if (BattlefieldData.SheepConsumable(nItemID) || BattlefieldData.WolfConsumable(nItemID))
						{
							var newBuff = new BuffConsume(nItemID);
							newBuff.GenerateConsumeable(Parent);
						}
					}
					else
					{
						switch (template.BFSkill) // we hardcodin' bois
						{
							case 0:
								Parent.SendMessage("You cry at the Wolf, and it slows down as if it's in shock.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) == BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[126][9], 0, 0);
									}
								});
								break;
							case 1:
								Parent.SendMessage("You attack the Wolf. He should be momentarily stunned.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) == BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[123][14], 0, 0);
									}
								});
								break;
							case 2:
								Parent.Field.BroadcastNotice("The Sheep are in a state of confusion.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) != BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[132][4], 0, 0);
									}
								});
								break;
							case 3:
								Parent.Field.BroadcastNotice("The Wolves have slowed down.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) == BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[126][10], 0, 0);
									}
								});
								break;
							case 4:
								Parent.Field.BroadcastNotice("The Sheep are momentarily stunned.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) != BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[123][15], 0, 0);
									}
								});
								break;
							case 5:
								Parent.Field.BroadcastNotice("The Sheep have momentarily weakened.");
								bf.Users.ForEach(u =>
								{
									if (bf.GetPlayerTeam(u.dwId) != BattlefieldData.BattlefieldTeams.Wolves)
									{
										u.Buffs.OnStatChangeByMobSkill(MasterManager.MobSkillTemplates[122][12], 0, 0);
									}
								});
								break;
						}
					}
				}
				else
				{
					if (Parent.Field.Template.HasStatChangeItemLimit())
					{
						Parent.SendMessage("Not allowed in this map.");
						return;
					}

					var newBuff = new BuffConsume(nItemID);
					newBuff.GenerateConsumeable(Parent);
					// buff gets added in the generator if it has bufftime
				}
			}

			InventoryManipulator.RemoveFrom(Parent, InventoryType.Consume, nPOS); // remove regardless
		}

		/// <summary>
		/// Teleport scroll handling.
		/// </summary>
		/// <param name="nPOS"></param>
		/// <param name="nItemID"></param>
		public void UsePortalScroll(int nItemID, short nPOS)
		{
			if (Parent.Stats.nHP <= 0) return;

			if (Parent.Field.Template.HasPortalScrollLimit()) return;

			if (InventoryManipulator.GetItem(Parent, InventoryType.Consume, nPOS)?.Template is ConsumeItemTemplate template)
			{
				if (template.MoveTo > 0)
				{
					Parent.Action.SetField(template.MoveTo == 999999999 ? Parent.Field.ReturnMapId : template.MoveTo);
				}
			}

			InventoryManipulator.RemoveFrom(Parent, InventoryType.Consume, nPOS);
		}

		public void UseUpgradeScroll(short nScrollPOS, short nEquipPOS, bool bWhiteScroll)
		{
			if (Parent.Stats.nHP <= 0) return;

			var pScrollTemplate = InventoryManipulator.GetItem(Parent, InventoryType.Consume, nScrollPOS).Template as ConsumeItemTemplate;
			var pEquip = InventoryManipulator.GetItem(Parent, InventoryType.Equip, nEquipPOS) as GW_ItemSlotEquip;
			var nWhiteScrollPOS = InventoryManipulator.GetAnyItem(Parent, InventoryType.Consume, ItemConstants.WhiteScroll).Item1;

			if (pEquip == null || pScrollTemplate == null) return;
			if (pEquip.CashItem) return;
			if (pEquip.RemainingUpgradeCount <= 0 && pScrollTemplate.Recover <= 0) return;

			var bSuccess = pScrollTemplate.ScrollSuccess(Constants.Rand);
			var bDestroy = !bSuccess && pScrollTemplate.ScrollDestroy(Constants.Rand);
			bWhiteScroll = bWhiteScroll && nWhiteScrollPOS > 0;

			if (!ItemConstants.is_correct_upgrade_equip(pScrollTemplate.TemplateId, pEquip.nItemID)) // PE, validated client-side
				return;

			if (bSuccess)
			{
				if (pScrollTemplate.Recover > 0)
				{
					pEquip.RemainingUpgradeCount += (byte)pScrollTemplate.Recover;
				}
				else
				{
					if (pScrollTemplate.PreventSlip)
					{
						pEquip.nAttribute |= ItemAttributeFlags.Spikes;
					}
					else if (pScrollTemplate.WarmSupport)
					{
						pEquip.nAttribute |= ItemAttributeFlags.Cold;
					}
					else if (pScrollTemplate.RandStat)
					{
						var randRange = 2 + (2 * pScrollTemplate.IncRandVol);

						var gX = new GaussianRandom();

						if (pEquip.niSTR > 0)
							pEquip.niSTR = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niSTR, randRange, false));
						if (pEquip.niLUK > 0)
							pEquip.niLUK = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niLUK, randRange, false));
						if (pEquip.niINT > 0)
							pEquip.niINT = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niINT, randRange, false));
						if (pEquip.niDEX > 0)
							pEquip.niDEX = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niDEX, randRange, false));

						if (pEquip.niACC > 0)
							pEquip.niACC = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niACC, randRange, false));
						if (pEquip.niEVA > 0)
							pEquip.niEVA = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niEVA, randRange, false));

						if (pEquip.niMaxHP > 0)
							pEquip.niMaxHP = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niMaxHP, randRange, false));
						if (pEquip.niMaxMP > 0)
							pEquip.niMaxMP = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niMaxMP, randRange, false));

						if (pEquip.niPAD > 0)
							pEquip.niPAD = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niPAD, randRange, false));
						if (pEquip.niMAD > 0)
							pEquip.niMAD = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niMAD, randRange, false));
						if (pEquip.niPDD > 0)
							pEquip.niPDD = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niPDD, randRange, false));
						if (pEquip.niMDD > 0)
							pEquip.niMDD = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niMDD, randRange, false));

						if (pEquip.niSpeed > 0)
							pEquip.niSpeed = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niSpeed, randRange, false));
						if (pEquip.niJump > 0)
							pEquip.niJump = (short)Math.Max(0, gX.GaussianDistributionVariation(pEquip.niJump, randRange, false));
					}
					else
					{
						pEquip.niSTR += (short)pScrollTemplate.IncSTR;
						pEquip.niLUK += (short)pScrollTemplate.IncLUK;
						pEquip.niINT += (short)pScrollTemplate.IncINT;
						pEquip.niDEX += (short)pScrollTemplate.IncDEX;
						pEquip.niMaxHP += (short)pScrollTemplate.IncMHP;
						pEquip.niMaxMP += (short)pScrollTemplate.IncMMP;
						pEquip.niPAD += (short)pScrollTemplate.IncPAD; // watk
						pEquip.niMAD += (short)pScrollTemplate.IncMAD;// matk
						pEquip.niPDD += (short)pScrollTemplate.IncPDD; // wdef
						pEquip.niMDD += (short)pScrollTemplate.IncMDD; // mdef
						pEquip.niACC += (short)pScrollTemplate.IncACC; // accuracy
						pEquip.niEVA += (short)pScrollTemplate.IncEVA; // avoid
						pEquip.niCraft += (short)pScrollTemplate.IncCraft; // still not sure wtf this is
						pEquip.niSpeed += (short)pScrollTemplate.IncSpeed;
						pEquip.niJump += (short)pScrollTemplate.IncJump;
					}
				}
			}

			if (bDestroy)
			{
				InventoryManipulator.RemoveFrom(Parent, InventoryType.Equip, nEquipPOS);
			}
			else // success or fail
			{
				if (pScrollTemplate.Recover <= 0) // not an upgrade count recovery scroll
				{
					pEquip.RemainingUpgradeCount -= 1; // reduce remaining upgrade count if no white scroll

					if (bSuccess)
					{
						pEquip.CurrentUpgradeCount += 1; // increase upgrade count
					}
					else if (bWhiteScroll)
					{
						pEquip.RemainingUpgradeCount += 1;
					}

					if (bWhiteScroll)
					{
						InventoryManipulator.RemoveFrom(Parent, InventoryType.Consume, nWhiteScrollPOS);
					}
				}

				Parent.Modify.Inventory(ctx =>
				{
					ctx.UpdateEquipInformation(pEquip, nEquipPOS);
				});
			}

			InventoryManipulator.RemoveFrom(Parent, InventoryType.Consume, nScrollPOS);

			Parent.StatisticsTracker.IncrementScrollUse(pScrollTemplate.TemplateId, bSuccess, bDestroy);
			Parent.Field.Broadcast(pEquip.ShowItemUpgradeEffect(Parent, bSuccess, bDestroy, bWhiteScroll));
		}

		public void UseMagnifyingGlass(short nMagnifyingPOS, short nEquipPOS)
		{
			if (Parent.Stats.nHP <= 0) return;

			var pTemplate = InventoryManipulator.GetItem(Parent, InventoryType.Consume, nMagnifyingPOS).Template as ConsumeItemTemplate;
			var pEquip = InventoryManipulator.GetItem(Parent, InventoryType.Equip, nEquipPOS) as GW_ItemSlotEquip;

			if (pEquip is null || pTemplate is null) return;

			if (pEquip.HasVisiblePotential) return;
			if (pEquip.nGrade == PotentialGradeCode.Normal) return;
			if (pEquip.CashItem) return;
			if (!ItemLogic.MagnifyingGlass(pTemplate.TemplateId)) return;

			if (pEquip.EquipTemplate.ReqLevel > ItemConstants.GlassRevealLevel(pTemplate.TemplateId)) return;

			if (pEquip.nOption1 == 0)
			{
				pEquip.nGrade = PotentialGradeCode.Visible_Rare; // cant get epic on first reveal
			}
			else
			{
				switch (pEquip.nGrade)
				{
					case PotentialGradeCode.Hidden_Rare:
						if (Constants.Rand.NextDouble() < RateConstants.ItemPotential_EpicOdds)
						{
							pEquip.nGrade = PotentialGradeCode.Visible_Epic;
						}
						else
						{
							pEquip.nGrade = PotentialGradeCode.Visible_Rare;
						}
						break;
					case PotentialGradeCode.Hidden_Epic:
						if (Constants.Rand.NextDouble() < RateConstants.ItemPotential_UniqueOdds)
						{
							pEquip.nGrade = PotentialGradeCode.Visible_Unique;
						}
						else
						{
							pEquip.nGrade = PotentialGradeCode.Visible_Epic;
						}
						break;
					case PotentialGradeCode.Hidden_Unique:
						pEquip.nGrade = PotentialGradeCode.Visible_Unique;
						break;
				}
			}

			var potentialCollection = MasterManager.ItemOptionTemplates
				.GetAll()
				.Where(p
					=> p.ReqLevel <= pEquip.EquipTemplate.ReqLevel // throw out incorrect level range
					&& ItemConstants.OptionTypeFitsEquipType(pEquip.nItemID, p.OptionType) // throw out incorrect equip types
					&& ItemConstants.OptionGradeFitsEquipGrade(p.TemplateId, pEquip.nGrade) // throw out incorrect grade codes
					&& p.OptionType != 90 // these show "Hidden" for some reason o___o
				);

			var itemOptionTemplates = potentialCollection as ItemOptionTemplate[] ?? potentialCollection.ToArray();

			if (itemOptionTemplates.Length <= 3)
				throw new IndexOutOfRangeException($"unable to put potential on item. CID: {Parent.dwId} | EPOS: {nEquipPOS} | Grade: {nameof(pEquip.nGrade)} | Level: {pEquip.EquipTemplate.ReqLevel}");

			var lines = new List<int>
			{
				itemOptionTemplates.Random().TemplateId,
				itemOptionTemplates.Random().TemplateId
			};

			lines.Sort(); // so the potential grades are in order

			if (pEquip.nOption3 == 0 || pEquip.nOption1 == 0)
			{
				var odds = pEquip.nOption1 == 0
					? RateConstants.ItemPotential_ThirdLineFirstReveal
					: RateConstants.ItemPotential_ThirdLineOdds;

				// lower odds for third line on first reveal
				if (Constants.Rand.NextDouble() < odds)
				{
					pEquip.nOption3 = (short)lines[0];
				}
			}
			else // option1 > 0 && option3 != 0
			{
				pEquip.nOption3 = (short)lines[0];
			}

			pEquip.nOption2 = (short)lines[1];

			// recalculate to ensure the first option is always the same grade as the equip
			pEquip.nOption1 = (short)itemOptionTemplates
				.Where(p => ItemConstants.GradeCodeByID(p.TemplateId) == pEquip.nGrade
							&& p.TemplateId >= lines[1]).Random()
				.TemplateId;

			pEquip.sTitle = $"{pEquip.nOption1}|{pEquip.nOption2}";

			Parent.Modify.Inventory(ctx =>
			{
				ctx.UpdateEquipInformation(pEquip, nEquipPOS);
			});

			InventoryManipulator.RemoveFrom(Parent, InventoryType.Consume, nMagnifyingPOS);

			Parent.SendPacket(pEquip.ShowItemReleaseEffect(Parent.dwId, nEquipPOS));
		}

		public void UseLotteryItem(short nPOS, int nItemID)
		{
			if (Parent.Stats.nHP <= 0) return;

			var pItemRaw = InventoryManipulator.GetItem(Parent, ItemConstants.GetInventoryType(nItemID), nPOS); // TODO determine if we can hardcode the inventory type

			var itemResult = MasterManager.CreateItem(RandomBoxes.GetRandom(nItemID));

			if (itemResult != null && pItemRaw is GW_ItemSlotBundle pItem)
			{
				if (InventoryManipulator.CountFreeSlots(Parent, InventoryType.Equip) > 0 && InventoryManipulator.CountFreeSlots(Parent, InventoryType.Consume) > 0)
				{
					InventoryManipulator.RemoveFrom(Parent, pItemRaw.InvType, nPOS);
					InventoryManipulator.InsertInto(Parent, itemResult);
				}
				else
				{
					Parent.SendMessage("Please make some room in your inventory.");
				}
			}
			else
			{
				Parent.SendMessage("This item has not been implemented yet. If you believe this to be an error, please report it on the discord server.");
			}
		}
	}
}
