using System;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth
{
	public class ItemConstants
	{
		public static int CUIRaiseWnd__GetLevel(int nQRData, int nIncExpUnit, int nGrade)
		{
			// CUIRaiseWnd::GetLevel
			var result = nQRData / nIncExpUnit;

			return result > nGrade ? nGrade : result;
		}

		public static bool IsExpCoupon(int nItemID) => nItemID / 10000 == 521;
		/// <summary>
		/// All drop coupons affect both meso and drop rate
		/// </summary>
		/// <param name="nItemID"></param>
		/// <returns></returns>
		public static bool IsDropCoupon(int nItemID) => nItemID / 10000 == 536;

		public static InventoryType GetInventoryType(int nItemId)
			=> (InventoryType)(nItemId / 1000000);

		public static bool IsRing(int nItemID) 
			=> nItemID / 1000 == 1112;

		public static bool IsPet(int nItemID)
			=> nItemID / 10000 == 500;

		public static bool LongCoat(int nItemID)
			=> nItemID / 10000 == 105;

		public static bool Overall(int id)
			=> id / 10000 == 105;

		public static bool Weapon(int id)
			=> id >= 1300000 && id < 1600000;

		public static bool MagicWeapon(int id)
			=> id / 10000 == 137 || id / 10000 == 138;

		public static bool Shield(int id)
			=> (id / 10000) == 109;

		public static bool SummonSack(int id)
			=> id / 10000 == 210;

		public static bool WeaponSticker(int nItemID)
			=> nItemID / 100000 == 17;

		public static bool TamedMob(int nItemID)
			=> nItemID / 10000 == 190;

		public static bool EvanDragonRiding(int nItemID)
			=> nItemID == 1902040 || nItemID == 1902041 || nItemID == 1902042;

		public static bool is_entrusted_shop_item(int nItemID)
			=> nItemID / 10000 == 514 || nItemID / 10000 == 503;

		public static bool is_treat_singly(int nItemID)
		{
			int v1 = nItemID / 1000000;
			return nItemID / 1000000 != 2 && v1 != 3 && v1 != 4 || nItemID / 10000 == 207 || nItemID / 10000 == 233;
		}

		public static bool IsHammerItem(int nItemID)
			=> nItemID == 5570000;

		public static int MiracleCubeFragment // since we only have one type of cube in v95
			=> 2430112;
		public static int WhiteScroll
			=> 2340000;

		// ======== // Pet Stuff \\ ======== \\

		public static bool is_pet_equip_item_idx(int nIdx) // nIdx is the bodypart / itemslot
			=> nIdx == 14 || nIdx > 20 && nIdx <= 48;

		public static bool is_pet_ability_item(int nItemID)
			=> nItemID / 10000 == 181;

		public static bool is_petring_item(int nItemID)
			=> nItemID == 1822000 || nItemID == 1832000;

		public static bool is_pet_equip_item(int nItemID)
			=> nItemID / 100000 == 18;

		public static bool is_pet_food_item(int nItemID)
			=> nItemID / 10000 == 212 || is_cash_pet_food_item(nItemID);

		public static bool is_cash_pet_food_item(int nItemID)
			=> nItemID / 10000 == 524;

		// ======== // ======== \\ ======== \\

		public static EquipType GetEquipTypeScrollID(int nItemID)
			=> (EquipType)Math.Floor(nItemID % 10000 / 100.0);

		public static bool RegularUpgradeScroll(int nItemID)
			=> nItemID.InRange(2040000, 2050000, false);

		public static bool is_black_upgrade_item(int nItemID)
		{
			return nItemID / 100 == 20491;
		}

		public static bool is_durability_upgrade_item(int nItemID)
		{
			return nItemID / 1000 == 2047;
		}

		public static bool is_new_upgrade_item(int nItemID)
		{
			return nItemID / 1000 == 2046;
		}

		public static bool is_hyper_upgrade_item(int nItemID)
		{
			return nItemID / 100 == 20493;
		}

		public static bool is_itemoption_upgrade_item(int nItemID)
		{
			return nItemID / 100 == 20494;
		}

		public static bool is_acc_upgrade_item(int nItemID)
		{
			return nItemID / 100 == 20492;
		}

		public static bool is_friendship_equip_item(int nItemID)
		{
			return nItemID / 100 == 11128 && nItemID % 10 <= 2;
		}

		public static bool is_couple_equip_item(int nItemID)
		{
			return nItemID / 100 == 11120 && nItemID != 1112000;
		}

		public static bool is_wedding_ring_item(int nItemID)
		{
			return nItemID == 1112803 || nItemID == 1112806 || nItemID == 1112807 || nItemID == 1112809;
		}

		public static bool is_correct_upgrade_equip(int nUItemID, int nEItemID)
		{
			int v2; // eax
			int v4; // esi
			int v5; // ebx

			v2 = nUItemID / 10000;
			if (nUItemID / 10000 == 249 || v2 == 247)
				return true;
			if (v2 != 204 || nEItemID / 1000000 != 1)
				return false;
			if ((nUItemID / 100 == 20490 || is_black_upgrade_item(nUItemID)) && !is_pet_equip_item(nEItemID)
			  || is_hyper_upgrade_item(nUItemID)
			  || is_itemoption_upgrade_item(nUItemID))
			{
				return true;
			}
			v4 = (nUItemID - 2040000) / 100;
			v5 = nEItemID / 10000 % 100;
			if (is_acc_upgrade_item(nUItemID))
			{
				if (v5 >= 11 && v5 <= 13)
					return true;
			}
			else
			{
				if (is_new_upgrade_item(nUItemID) || is_durability_upgrade_item(nUItemID))
				{
					switch (v4 % 10)
					{
						case 0:
							return (v5 - 30) <= 9;
						case 1:
							return (v5 - 40) <= 9;
						case 2:
							if (v5 > 0 && (v5 <= 3 || v5 > 10))
								return false;
							return true;
						case 3:
							switch (v5)
							{
								case 1:
								case 2:
								case 3:
								case 11:
								case 12:
								case 13:
								case 14:
									return true;
								default:
									return false;
							}
						default:
							return true;
					}
				}
				return v4 == v5;
			}
			return false;
		}


		// =======================// ==========================================//

		public static CashItemType get_consume_cash_item_type(int nItemID)
		{
			CashItemType result = get_cashslot_item_type(nItemID);

			var resCast = (int)result;

			if (resCast < 12 || resCast > 78)
				return CashItemType.NONE;

			if (resCast >= 12 && resCast <= 32)
				return result;

			if (resCast >= 47 && resCast <= 54)
				return result;

			if (resCast >= 71 && resCast <= 75)
				return result;

			switch (result)
			{
				// 0 - 11
				// 33
				case CashItemType.CONSUMEAREABUFFITEM:
				case CashItemType.COLORLENS:
				// 36 - 37
				case CashItemType.SELECTNPC:
				// 39 - 40
				case CashItemType.MORPH:
				// 42
				case CashItemType.AVATARMEGAPHONE:
				case CashItemType.HEARTSPEAKER:
				case CashItemType.SKULLSPEAKER:
				// 46
				// 55 - 60
				case CashItemType.ARTSPEAKERWORLD:
				case CashItemType.EXTENDEXPIREDATE:
				// 63
				case CashItemType.KARMASCISSORS:
				case CashItemType.EXPIREDPROTECTING:
				case CashItemType.CHARACTERSALE:
				case CashItemType.ITEMUPGRADE:
				// 68 - 70
				// 76 - 77
				case CashItemType.QUESTDELIVERY:
					// 79+
					return result;
			}
			return CashItemType.NONE;
		}

		public static CashItemType get_etc_cash_item_type(int nItemID)
		{
			var result = get_cashslot_item_type(nItemID);

			var cast = (int)result;

			if (cast >= 1 && cast <= 7)
				return result;

			switch (result)
			{
				case CashItemType.WEDDINGTICKET:
				case CashItemType.INVITATIONTICKET:
				case CashItemType.GACHAPONCOUPON:
				case CashItemType.PETEVOL:
				case CashItemType.REMOVABLE:
				case CashItemType.HAIRSHOPMEMBERSHIPCOUPON:
				case CashItemType.FACESHOPMEMBERSHIPCOUPON:
				case CashItemType.SKINSHOPMEMBERSHIPCOUPON:
				case CashItemType.GACHAPONREMOTE:
				case CashItemType.UPGRADETOMB:
				case CashItemType.RECOVERUPGRADECOUNT:
					return result;
				default:
					return CashItemType.NONE;
			}
		}

		public static CashItemType get_cashslot_item_type(int nItemID)
		{
			switch (nItemID / 10000)
			{
				case 500:
					return CashItemType.PET;
				case 501:
					return CashItemType.EFFECT;
				case 502:
					return CashItemType.BULLET;
				case 503:
					return CashItemType.SHOPEMPLOYEE;
				case 504:
					return CashItemType.MAPTRANSFER;
				case 505:
					{
						return nItemID == 5050000
							? CashItemType.STATCHANGE
							: CashItemType.SKILLCHANGE;
					}
				case 506:
					if (nItemID >= 05062000)
					{
						return CashItemType.ITEM_UNRELEASE;
					}
					if (nItemID < 05062000 && nItemID >= 05061000)
					{
						return CashItemType.EXPIREDPROTECTING;
					}
					switch (nItemID % 10)
					{
						case 0:
							return CashItemType.NAMING;
						case 1:
							return CashItemType.PROTECTING;
						case 2:
						case 3:
							return CashItemType.INCUBATOR;
					}
					return CashItemType.NONE;
				case 507:
					switch (nItemID % 10000 / 1000)
					{
						case 1:
							return CashItemType.SPEAKERCHANNEL;
						case 2:
							return CashItemType.SPEAKERWORLD;
						case 4:
							return CashItemType.SKULLSPEAKER;
						case 5:
							switch (nItemID % 10)
							{
								case 0:
									return CashItemType.MAPLETV;
								case 1:
									return CashItemType.MAPLESOLETV;
								case 2:
									return CashItemType.MAPLELOVETV;
								case 3:
									return CashItemType.MEGATV;
								case 4:
									return CashItemType.MEGASOLETV;
								case 5:
									return CashItemType.MEGALOVETV;
								default:
									return CashItemType.NONE;
							}
						case 6:
							return CashItemType.ITEMSPEAKER;
						case 7:
							return CashItemType.ARTSPEAKERWORLD;
						case 8:
							return CashItemType.SPEAKERBRIDGE; // surrogate for ItemSpeaker w/o item
						default:
							return CashItemType.NONE;
					}
				case 508:
					return CashItemType.MESSAGEBOX;
				case 509:
					return CashItemType.SENDMEMO;
				case 510:
					return CashItemType.JUKEBOX;
				case 512:
					return CashItemType.WEATHER;
				case 513:
					return CashItemType.PROTECTONDIE;
				case 514:
					return CashItemType.SHOP;
				case 515:
					{
						if (nItemID >= 05150000 && nItemID < 05152000) // 0 & 1
							return CashItemType.HAIR;
						if (nItemID >= 05152000 && nItemID < 05152100) // 2
							return CashItemType.FACE;
						if (nItemID >= 05152100 && nItemID < 05153000) // 2.5
							return CashItemType.COLORLENS;
						if (nItemID >= 05153000 && nItemID < 05154000) // 3
							return CashItemType.SKIN;
						if (nItemID >= 05154000 && nItemID < 05155000) // 4
							return CashItemType.HAIR;

						return CashItemType.NONE;
					}
				case 516:
					return CashItemType.EMOTION;
				case 517:
					return CashItemType.SETPETNAME;
				case 518:
					return CashItemType.SETPETLIFE;
				case 519:
					return CashItemType.PETSKILL;
				case 520:
					return CashItemType.MONEYPOCKET; // meso bag
				case 522:
					return CashItemType.GACHAPONCOUPON;
				case 523:
					return CashItemType.SHOPSCANNER;
				case 524:
					return CashItemType.PETFOOD;
				case 525:
					{
						if (nItemID >= 05251000 && nItemID < 05252000)
							return CashItemType.WEDDINGTICKET;

						return CashItemType.INVITATIONTICKET;
					}
				case 528:
					{
						if (nItemID >= 05280000 && nItemID < 05281000)
							return CashItemType.CONSUMEEFFECTITEM;

						return CashItemType.CONSUMEAREABUFFITEM;
					}
				case 530:
					return CashItemType.MORPH;
				case 533:
					return CashItemType.QUICKDELIVERY;
				case 537:
					return CashItemType.ADBOARD;
				case 538:
					return CashItemType.PETEVOL;
				case 539:
					return CashItemType.AVATARMEGAPHONE;
				case 540:
					{
						if (nItemID == 05400000)
							return CashItemType.CHANGECHARACTERNAME;
						if (nItemID == 05401000)
							return CashItemType.TRANSFERWORLDCOUPON;

						return CashItemType.NONE;
					}
				case 542:
					return CashItemType.HAIRSHOPMEMBERSHIPCOUPON;
				case 543:
					return CashItemType.CHARACTERSALE;
				case 545:
					{
						if (nItemID == 05450000)
							return CashItemType.SELECTNPC;
						if (nItemID == 05451000)
							return CashItemType.GACHAPONREMOTE;

						return CashItemType.NONE;
					}
				case 546:
					return CashItemType.PETSNACK;
				case 547:
					return CashItemType.REMOTESHOP;
				case 549:
					return CashItemType.GACHAPONBOX_MASTERKEY;
				case 550:
					return CashItemType.EXTENDEXPIREDATE;
				case 551:
					return CashItemType.UPGRADETOMB;
				case 552:
					return CashItemType.KARMASCISSORS;
				case 553:
					return CashItemType.REWARD;
				case 557:
					return CashItemType.ITEMUPGRADE;
				case 561:
					return CashItemType.VEGA;
				case 562:
					return CashItemType.MASTERYBOOK;
				case 564:
					return CashItemType.RECOVERUPGRADECOUNT;
				case 566:
					return CashItemType.QUESTDELIVERY;
			}
			return CashItemType.NONE;
		}

		public static bool IsAccessory(int nEquipItemID)
		{
			var div = nEquipItemID / 10000;

			return div == 115 || div == 114 || div == 113 || div == 112 || div == 103 || div == 102 || div == 101;
		}

		// ======================== // ================================= //

		public static int get_weapon_type(int nItemID) // todo replace return value with an enum
		{
			int result; // eax

			if (nItemID / 1000000 != 1)
				return 0;
			result = nItemID / 10000 % 100;
			switch (result)
			{
				case 30:
				case 31:
				case 32:
				case 33:
				case 34:
				case 37:
				case 38:
				case 39:
				case 40:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45: // WT_BOW
				case 46: // WT_CROSSBOW
				case 47: // WT_THROWINGGLOVE
				case 48:
				case 49: // WT_GUN
					return result;
				default:
					return 0;
			}
		}

		public static bool Vehicle(int nItemID)
			=> nItemID / 10000 == 190
				|| nItemID / 10000 == 193
				|| nItemID == 1902040
				|| nItemID == 1902041
				|| nItemID == 1902042
				|| nItemID / 1000 == 1983;

		public static bool EventVehicle(int nItemID)
		{
			switch (nItemID)
			{
				case 1932003:
				case 1932004:
				case 1932006:
				case 1932007:
				case 1932008:
				case 1932009:
				case 1932010:
				case 1932011:
				case 1932012:
				case 1932013:
				case 1932014:
				case 1932017:
				case 1932018:
				case 1932019:
				case 1932020:
				case 1932021:
				case 1932022:
				case 1932023:
				case 1932025:
				case 1932026:
				case 1932027:
				case 1932028:
				case 1932029:
				case 1932034:
				case 1932035:
				case 1932037:
				case 1932038:
				case 1932039:
				case 1932040:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBookItem(int nItemID)
		{
			return nItemID / 10000 == 416;
		}

		public static bool IsMasteryBook(int nItemID)
		{
			return nItemID / 10000 == 229;
		}

		public static bool IsRechargeableItem(int nItemID)
		{
			return IsThrowingStar(nItemID) || IsBullet(nItemID);
		}

		public static bool IsThrowingStar(int nItemID)
		{
			return nItemID / 10000 == 207;
		}

		public static bool IsBullet(int nItemID)
		{
			return nItemID / 10000 == 233;
		}

		public static bool IsArrow(int nItemID)
		{
			return IsBowArrow(nItemID) || IsXBowArrow(nItemID);
		}

		public static bool IsBowArrow(int nItemID)
		{
			return nItemID / 1000 == 2060;
		}

		public static bool IsXBowArrow(int nItemID)
		{
			return nItemID / 1000 == 2061;
		}

		public static bool IsHiredMerchantItem(int nItemID)
		{
			return nItemID / 10000 == 514 || nItemID / 10000 == 503;
		}

		public static bool IsMiniGameItem(int nItemID)
		{
			return nItemID / 10000 == 408;
		}

		public static bool IsMonsterBookCardItem(int nItemID)
		{
			return nItemID / 10000 == 238;
		}

		public static bool IsMakerGem(int nItemID)
		{
			var category = nItemID / 10000;
			return category == 402 || category == 425 || category == 426;
		}

		public static int GlassRevealLevel(int glassItem)
		{
			switch (glassItem)
			{
				case 2460000:
					return 30;
				case 2460001:
					return 70;
				case 2460002:
					return 120;
				case 2460003:
					return 256;
				default:
					return 0;
			}
		}

		public static bool OptionTypeFitsEquipType(int nEquipItemID, int nOptionType)
		{
			var div = nEquipItemID / 10000; // TODO use BodyPart for this

			switch (nOptionType)
			{
				case 10: // weapons
					return Weapon(nEquipItemID);
				case 11: // everything except weapons
					return !Weapon(nEquipItemID);
				case 20: // armor (not accessory)
					return !IsAccessory(nEquipItemID) && !Weapon(nEquipItemID);
				case 40: // accessories
					return IsAccessory(nEquipItemID);
				case 51: // hat
					return div == 100;
				case 52: // top and overall
					return div == 104 || div == 105;
				case 53: // bottom and overall
					return div == 106 || div == 105;
				case 54: // gloves
					return div == 108;
				case 55: // shoes
					return div == 107;
				default:
					return true;
			}
		}

		public static PotentialGradeCode GradeCodeByID(int nPotentialID)
			=> (PotentialGradeCode)((nPotentialID / 10000) + 4);

		/// <summary>
		/// Calculates if the potential line is within 1 grade level of the equip grade code.
		/// </summary>
		/// <param name="nPotentialID">Potential grade</param>
		/// <param name="nEquipGradeCode">Equip grade</param>
		/// <returns>True/false :pepega:</returns>
		public static bool OptionGradeFitsEquipGrade(int nPotentialID, PotentialGradeCode nEquipGradeCode)
		{
			if (nPotentialID == 030602)
			{
				;
			}
			var diff = (int)nEquipGradeCode - (int)GradeCodeByID(nPotentialID);
			return diff == 1 || diff == 0;
		}

		public static int to_pet_index_from_pet_ability_item_index(int nPetAbilityItemIdx)
		{
			int result; // eax

			switch (nPetAbilityItemIdx)
			{
				case 31: // TODO bodypart enum
				case 32:
				case 33:
				case 34:
				case 35:
				case 36:
				case 37:
				case 47:
					result = 1;
					break;
				case 39:
				case 40:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45:
				case 48:
					result = 2;
					break;
				default:
					result = 0;
					break;
			}
			return result;
		}

		public static bool is_fieldtype_upgradetomb_usable(FieldType nFieldType, int nFieldID)
		{
			switch (nFieldType)
			{
				case FieldType.SNOWBALL:
				case FieldType.TOURNAMENT:
				case FieldType.COCONUT:
				case FieldType.OXQUIZ:
				case FieldType.WAITINGROOM:
				case FieldType.MONSTERCARNIVAL:
				case FieldType.MONSTERCARNIVALREVIVE:
				case FieldType.MONSTERCARNIVAL_S2:
					return false;
				default:
					if (nFieldID / 100000000 == 9 || nFieldID / 1000 == 200090)
						return false;
					else
						return nFieldID / 1000000 != 390;
			}
		}

		public static bool is_petability_item(int nItemID)
			=> nItemID / 10000 == 181;

		public static BodyPart get_petitem_bodypart(int nIdx, int nItemID)
		{
			switch (nItemID)
			{
				case 01802100: // Pet Collar
					{
						// unsure about this one
					}
					break;
				case 01812000: // Meso Magnet
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_MESO;
							case 1:
								return BodyPart.BP_PETABIL_MESO2;
							case 2:
								return BodyPart.BP_PETABIL_MESO3;
						}
					}
					break;
				case 01812001: // Item Pouch
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_ITEM;
							case 1:
								return BodyPart.BP_PETABIL_ITEM2;
							case 2:
								return BodyPart.BP_PETABIL_ITEM3;
						}
					}
					break;
				case 01812002: // Auto HP Potion Pouch
					{
						return BodyPart.BP_PETABIL_HPCONSUME;
					}
				case 01812003: // Auto MP Potion Pouch
					{
						return BodyPart.BP_PETABIL_MPCONSUME;
					}
				case 01812004: // Wing Boots
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_SWEEPFORDROP;
							case 1:
								return BodyPart.BP_PETABIL_SWEEPFORDROP2;
							case 2:
								return BodyPart.BP_PETABIL_SWEEPFORDROP3;
						}
					}
					break;
				case 01812005: // Binocular
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_LONGRANGE;
							case 1:
								return BodyPart.BP_PETABIL_LONGRANGE2;
							case 2:
								return BodyPart.BP_PETABIL_LONGRANGE3;
						}
					}
					break;
				case 01812006: // Magic Scales
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_PICKUPOTHERS;
							case 1:
								return BodyPart.BP_PETABIL_PICKUPOTHERS2;
							case 2:
								return BodyPart.BP_PETABIL_PICKUPOTHERS3;
						}
					}
					break;
				case 01812007: // Item Ignore Pendant
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETABIL_IGNOREITEMS1;
							case 1:
								return BodyPart.BP_PETABIL_IGNOREITEMS2;
							case 2:
								return BodyPart.BP_PETABIL_IGNOREITEMS3;
						}
					}
					break;
				case 01822000: // Pet Label Ring
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETRING_LABEL;
							case 1:
								return BodyPart.BP_PETRING_LABEL2;
							case 2:
								return BodyPart.BP_PETRING_LABEL3;
						}
					}
					break;
				case 01832000: // Pet Quote Ring
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETRING_QUOTE;
							case 1:
								return BodyPart.BP_PETRING_QUOTE2;
							case 2:
								return BodyPart.BP_PETRING_QUOTE3;
						}
					}
					break;
				default: // regular equips
					{
						switch (nIdx)
						{
							case 0:
								return BodyPart.BP_PETWEAR;
							case 1:
								return BodyPart.BP_PETWEAR2;
							case 2:
								return BodyPart.BP_PETWEAR3;
						}
					}
					break;
			}

			return 0;
		}



		// TODO
		//		public static int get_bodypart_from_item(int nItemID, int nGender, int* pnBodyPart, int bAll)
		//		{
		//			int v4; // eax
		//			int result; // eax
		//			int v6; // ecx
		//			int v7; // eax

		//			v4 = get_gender_from_id(nItemID);
		//			if (nGender != 2 && v4 != 2 && v4 != nGender)
		//				return 0;
		//			v6 = nItemID / 10000;
		//			switch (nItemID / 10000)
		//			{
		//				case 100:
		//					*pnBodyPart = 1;
		//					result = 1;
		//					break;
		//				case 101:
		//					*pnBodyPart = 2;
		//					result = 1;
		//					break;
		//				case 102:
		//					*pnBodyPart = 3;
		//					result = 1;
		//					break;
		//				case 103:
		//					*pnBodyPart = 4;
		//					result = 1;
		//					break;
		//				case 104:
		//				case 105:
		//					*pnBodyPart = 5;
		//					result = 1;
		//					break;
		//				case 106:
		//					*pnBodyPart = 6;
		//					result = 1;
		//					break;
		//				case 107:
		//					*pnBodyPart = 7;
		//					result = 1;
		//					break;
		//				case 108:
		//					*pnBodyPart = 8;
		//					result = 1;
		//					break;
		//				case 109:
		//				case 119:
		//				case 134:
		//					*pnBodyPart = 10;
		//					result = 1;
		//					break;
		//				case 110:
		//					*pnBodyPart = 9;
		//					result = 1;
		//					break;
		//				case 111:
		//					if (bAll)
		//					{
		//						*pnBodyPart = 12;
		//						pnBodyPart[1] = 13;
		//						pnBodyPart[2] = 15;
		//						pnBodyPart[3] = 16;
		//						result = 4;
		//					}
		//					else
		//					{
		//						*pnBodyPart = 12;
		//						result = 1;
		//					}
		//					break;
		//				case 112:
		//					*pnBodyPart = 17;
		//					if (!bAll)
		//						goto LABEL_71;
		//					pnBodyPart[1] = 59;
		//					result = 2;
		//					break;
		//				case 113:
		//					*pnBodyPart = 50;
		//					result = 1;
		//					break;
		//				case 114:
		//					*pnBodyPart = 49;
		//					result = 1;
		//					break;
		//				case 115:
		//					*pnBodyPart = 51;
		//					result = 1;
		//					break;
		//				case 161:
		//					*pnBodyPart = 1100;
		//					result = 1;
		//					break;
		//				case 162:
		//					*pnBodyPart = 1101;
		//					result = 1;
		//					break;
		//				case 163:
		//					*pnBodyPart = 1102;
		//					result = 1;
		//					break;
		//				case 164:
		//					*pnBodyPart = 1103;
		//					result = 1;
		//					break;
		//				case 165:
		//					*pnBodyPart = 1104;
		//					result = 1;
		//					break;
		//				case 180:
		//					if (nItemID == 1802100)
		//					{
		//						if (bAll)
		//							goto LABEL_39;
		//						*pnBodyPart = 21;
		//						result = 1;
		//					}
		//					else if (bAll)
		//					{
		//						*pnBodyPart = 14;
		//						pnBodyPart[1] = 30;
		//						pnBodyPart[2] = 38;
		//						result = 3;
		//					}
		//					else
		//					{
		//						*pnBodyPart = 14;
		//						result = 1;
		//					}
		//					break;
		//				case 181:
		//					switch (nItemID)
		//					{
		//						case 1812000:
		//							*pnBodyPart = 23;
		//							if (!bAll)
		//								goto LABEL_71;
		//							pnBodyPart[1] = 34;
		//							pnBodyPart[2] = 42;
		//							result = 3;
		//							break;
		//						case 1812001:
		//							if (bAll)
		//							{
		//								*pnBodyPart = 22;
		//								pnBodyPart[1] = 33;
		//								pnBodyPart[2] = 41;
		//								result = 3;
		//							}
		//							else
		//							{
		//								*pnBodyPart = 22;
		//								result = 1;
		//							}
		//							break;
		//						case 1812002:
		//							*pnBodyPart = 24;
		//							result = 1;
		//							break;
		//						case 1812003:
		//							*pnBodyPart = 25;
		//							result = 1;
		//							break;
		//						case 1812004:
		//							if (bAll)
		//							{
		//								*pnBodyPart = 26;
		//								pnBodyPart[1] = 35;
		//								pnBodyPart[2] = 43;
		//								result = 3;
		//							}
		//							else
		//							{
		//								*pnBodyPart = 26;
		//								result = 1;
		//							}
		//							break;
		//						case 1812005:
		//							if (bAll)
		//							{
		//								*pnBodyPart = 27;
		//								pnBodyPart[1] = 36;
		//								pnBodyPart[2] = 44;
		//								result = 3;
		//							}
		//							else
		//							{
		//								*pnBodyPart = 27;
		//								result = 1;
		//							}
		//							break;
		//						case 1812006:
		//							*pnBodyPart = 28;
		//							if (!bAll)
		//								goto LABEL_71;
		//							pnBodyPart[1] = 37;
		//							pnBodyPart[2] = 45;
		//							result = 3;
		//							break;
		//						case 1812007:
		//							if (bAll)
		//							{
		//								*pnBodyPart = 46;
		//								pnBodyPart[1] = 47;
		//								pnBodyPart[3] = 48;
		//								result = 3;
		//							}
		//							else
		//							{
		//								*pnBodyPart = 46;
		//								result = 1;
		//							}
		//							break;
		//						default:
		//							goto $LN10_10;
		//					}
		//					break;
		//				case 182:
		//$LN10_10:
		//					if (bAll)
		//					{
		//					LABEL_39:
		//						*pnBodyPart = 21;
		//						pnBodyPart[1] = 31;
		//						pnBodyPart[2] = 39;
		//						result = 3;
		//					}
		//					else
		//					{
		//						*pnBodyPart = 21;
		//						result = 1;
		//					}
		//					break;
		//				case 183:
		//					*pnBodyPart = 29;
		//					if (!bAll)
		//						goto LABEL_71;
		//					pnBodyPart[1] = 32;
		//					pnBodyPart[2] = 40;
		//					result = 3;
		//					break;
		//				case 190:
		//					*pnBodyPart = 18;
		//					result = 1;
		//					break;
		//				case 191:
		//					*pnBodyPart = 19;
		//					result = 1;
		//					break;
		//				case 192:
		//					*pnBodyPart = 20;
		//					result = 1;
		//					break;
		//				case 194:
		//					*pnBodyPart = 1000;
		//					result = 1;
		//					break;
		//				case 195:
		//					*pnBodyPart = 1001;
		//					result = 1;
		//					break;
		//				case 196:
		//					*pnBodyPart = 1002;
		//					result = 1;
		//					break;
		//				case 197:
		//					*pnBodyPart = 1003;
		//					result = 1;
		//					break;
		//				default:
		//					v7 = v6 / 10;
		//					if (v6 / 10 != 13 && v7 != 14 && v7 != 16 && v7 != 17)
		//						return 0;
		//					*pnBodyPart = 11;
		//				LABEL_71:
		//					result = 1;
		//					break;
		//			}
		//			return result;
		//		}

		public static bool is_correct_bodypart(int nItemID, BodyPart nBodyPart)
		{
			var v5 = nItemID / 10000;
			switch (v5)
			{
				case 100:
					return nBodyPart == BodyPart.BP_CAP;
				case 101:
					return nBodyPart == BodyPart.BP_FACEACC;
				case 102:
					return nBodyPart == BodyPart.BP_EYEACC;
				case 103:
					return nBodyPart == BodyPart.BP_EARACC;
				case 104:
				case 105:
					return nBodyPart == BodyPart.BP_CLOTHES;
				case 106:
					return nBodyPart == BodyPart.BP_PANTS;
				case 107:
					return nBodyPart == BodyPart.BP_SHOES;
				case 108:
					return nBodyPart == BodyPart.BP_GLOVES;
				case 109:
				case 119:
				case 134:
					return nBodyPart == BodyPart.BP_SHIELD;
				case 110:
					return nBodyPart == BodyPart.BP_CAPE;
				case 111:
					return nBodyPart == BodyPart.BP_RING1
						|| nBodyPart == BodyPart.BP_RING2
						|| nBodyPart == BodyPart.BP_RING3
						|| nBodyPart == BodyPart.BP_RING4;
				case 112:
					return nBodyPart == BodyPart.BP_PENDANT || nBodyPart == BodyPart.BP_EXT_PENDANT1;
				case 113:
					return nBodyPart == BodyPart.BP_BELT;
				case 114:
					return nBodyPart == BodyPart.BP_MEDAL;
				case 115:
					return nBodyPart == BodyPart.BP_SHOULDER;
				case 161:
					return nBodyPart == BodyPart.MP_BASE; // MECHANIC START
				case 162:
					return nBodyPart == BodyPart.MP_ARM;
				case 163:
					return nBodyPart == BodyPart.MP_LEG;
				case 164:
					return nBodyPart == BodyPart.MP_FRAME;
				case 165:
					return nBodyPart == BodyPart.MP_TRANSISTER; // MECHANIC END
				case 180:
					if (nItemID == 1802100)
					{
						return nBodyPart == BodyPart.BP_PETRING_LABEL
							|| nBodyPart == BodyPart.BP_PETRING_LABEL2
							|| nBodyPart == BodyPart.BP_PETRING_LABEL3;
					}
					else
					{
						return nBodyPart == BodyPart.BP_PETWEAR
							|| nBodyPart == BodyPart.BP_PETWEAR2
							|| nBodyPart == BodyPart.BP_PETWEAR3;
					}
				case 181:
					switch (nItemID)
					{
						case 1812000:
							return nBodyPart == BodyPart.BP_PETABIL_MESO
								|| nBodyPart == BodyPart.BP_PETABIL_MESO2
								|| nBodyPart == BodyPart.BP_PETABIL_MESO3;
						case 1812001:
							return nBodyPart == BodyPart.BP_PETABIL_ITEM
								|| nBodyPart == BodyPart.BP_PETABIL_ITEM2
								|| nBodyPart == BodyPart.BP_PETABIL_ITEM3;
						case 1812002:
							return nBodyPart == BodyPart.BP_PETABIL_HPCONSUME;
						case 1812003:
							return nBodyPart == BodyPart.BP_PETABIL_MPCONSUME;
						case 1812004:
							return nBodyPart == BodyPart.BP_PETABIL_SWEEPFORDROP
								|| nBodyPart == BodyPart.BP_PETABIL_SWEEPFORDROP2
								|| nBodyPart == BodyPart.BP_PETABIL_SWEEPFORDROP3;
						case 1812005:
							return nBodyPart == BodyPart.BP_PETABIL_LONGRANGE
								|| nBodyPart == BodyPart.BP_PETABIL_LONGRANGE2
								|| nBodyPart == BodyPart.BP_PETABIL_LONGRANGE3;
						case 1812006:
							return nBodyPart == BodyPart.BP_PETABIL_PICKUPOTHERS
								|| nBodyPart == BodyPart.BP_PETABIL_PICKUPOTHERS2
								|| nBodyPart == BodyPart.BP_PETABIL_PICKUPOTHERS3;
						case 1812007:
							return nBodyPart == BodyPart.BP_PETABIL_IGNOREITEMS1
								|| nBodyPart == BodyPart.BP_PETABIL_IGNOREITEMS2
								|| nBodyPart == BodyPart.BP_PETABIL_IGNOREITEMS2;
						default:
							return false;
					}
				case 182:
					return nBodyPart == BodyPart.BP_PETRING_LABEL
						|| nBodyPart == BodyPart.BP_PETRING_LABEL2
						|| nBodyPart == BodyPart.BP_PETRING_LABEL3;
				case 183:
					return nBodyPart == BodyPart.BP_PETRING_QUOTE
						|| nBodyPart == BodyPart.BP_PETRING_QUOTE2
						|| nBodyPart == BodyPart.BP_PETRING_QUOTE3;
				case 190:
					return nBodyPart == BodyPart.BP_TAMINGMOB; // MOUNT START
				case 191:
					return nBodyPart == BodyPart.BP_SADDLE;
				case 192:
					return nBodyPart == BodyPart.BP_MOBEQUIP; // MOUNT END
				case 194:
					return nBodyPart == BodyPart.DP_BASE; // DRAGON START
				case 195:
					return nBodyPart == BodyPart.DP_PENDANT;
				case 196:
					return nBodyPart == BodyPart.DP_WING;
				case 197:
					return nBodyPart == BodyPart.DP_SHOES; // DRAGON END
				default:
					var v7 = v5 / 10;

					if (v7 != 13 && v7 != 14 && v7 != 16 && v7 != 17)
						return false;

					return nBodyPart == BodyPart.BP_WEAPON;
			}
		}
	}
}
