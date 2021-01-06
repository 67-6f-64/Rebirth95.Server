using Rebirth.Characters.Modify;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Characters
{
	/// <summary>
	/// Okay so here's the rundown for how serial numbers work:
	/// liSN - Unique ID of pet. No two items should have the same liSN.
	/// liPetLockerSN - This is the same as the CashItemSN which is the cash item index that Nexon gives all cash items. 
	///  - It's basically another Item ID.
	/// </summary>
	public sealed class CharacterPets
	{
		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		public CPet[] Pets { get; } = new CPet[3];

		public CharacterPets(int parent)
		{
			dwParentID = parent;
		}

		public void Dispose()
		{
			for (var i = 0; i < 3; i++)
			{
				Pets[i] = null;
			}
		}

		// CUser::UpdateActivePet(CUser *this, int tCur)
		public void UpdateActivePets()
		{
			foreach (var pet in Pets)
			{
				pet?.Update();
			}
		}

		public bool ActivateSinglePet(short nPos, bool bForce = false)
		{
			if (Pets[0] != null && !bForce)
			{
				DeactivateSinglePet();

				return true;
			}

			if (Parent.Field.Template.HasNoPetLimit())
			{
				Parent.SendMessage("Pets are not allowed in this map");
				return false;
			}

			var pItemRaw = InventoryManipulator.GetItem(Parent, InventoryType.Cash, nPos);

			if (pItemRaw is GW_ItemSlotPet pet)
			{
				if (pet.PetTemplate.EvolNo > 0)
				{
					Parent.SendMessage("Evolving have not been coded yet, sorry :(");
					return false;
				}

				ActivatePet(pet, nPos, 0);

				return true;
			}

			return false;
		}

		public void DeactivateSinglePet()
		{
			Parent.Field.Broadcast(Pets[0].MakeLeaveFieldPacket());
			Pets[0] = null;
			Parent.Modify.UpdatePetLocker();
		}

		// use this when we implement mulit-pet
		public bool ActivatePet(short nPos, bool bLeader)
		{
			throw new InvalidOperationException("Multipet not active");
			var pItemRaw = InventoryManipulator.GetItem(Parent, InventoryType.Cash, nPos);

			if (pItemRaw is GW_ItemSlotPet cPetItem)
			{
				if (cPetItem.PetTemplate.EvolNo > 0)
				{
					Parent.SendMessage("Evolving pets are not currently handled. Please try again later.");
					return false;
				}

				if (DeactivatePet(cPetItem.liCashItemSN))
					return true;

				byte nIdx = 0xFF;

				for (byte i = 0; i < 3; i++)
				{
					if (Pets[i] == null)
					{
						nIdx = i;
						break;
					}
				}

				if (nIdx == 0xFF) return false;

				ActivatePet(cPetItem, nPos, nIdx);
				Parent.Modify.UpdatePetLocker();

				return true;
			}

			return false;
		}

		public bool ActivatePet(GW_ItemSlotPet gwPetItem, short nItemPOS, byte nIdx)
		{
			if (gwPetItem is null)
			{
				gwPetItem = Parent.InventoryCash.Get(nItemPOS) as GW_ItemSlotPet;
			}

			if (gwPetItem is null) return false; // retard passing invalid slot

			CPet cPet = new CPet(dwParentID, nItemPOS, gwPetItem);

			cPet.Position.ResetPosTo(Parent.Position);

			cPet.nIdx = nIdx;
			Pets[nIdx] = cPet;
			Parent.Field.Broadcast(cPet.MakeEnterFieldPacket());
			Parent.Modify.UpdatePetLocker();
			return true;
		}

		public bool DeactivatePet(long liPetLockerSN)
		{
			for (var nIdx = 0; nIdx < 3; nIdx++)
			{
				if (Pets[nIdx]?.liPetLockerSN == liPetLockerSN)
				{
					Parent.Field.Broadcast(Pets[nIdx].MakeLeaveFieldPacket());
					Pets[nIdx] = null;
					Parent.Modify.UpdatePetLocker();
					return true;
				}
			}
			return false;
		}

		public void SortPets()
		{
			var list = Pets.ToList().Where(pet => pet != null).OrderBy(pet => pet.nIdx).ToList();

			for (var i = 0; i < 3; i++)
			{
				if (list.Count > i)
				{
					Pets[i] = list[i];
				}
				else
				{
					Pets[i] = null;
				}
			}
		}

		public void SpawnPetsToMap()
		{
			if (Parent.Field.Template.HasNoPetLimit())
			{
				//Parent.SendMessage("Pets are not allowed in this map."); // uhh dont want to spam them with this notice
				return;
			}

			foreach (var item in Pets)
			{
				if (item is null) continue;

				item.Field = Parent.Field;
				item.Position.MoveAction = 2; // standing
				item.Position.ResetPosTo(Parent.Position);
				Parent.SendPacket(item.MakeEnterFieldPacket());
			}
		}

		private readonly BodyPart[] petwearparts = { BodyPart.BP_PETWEAR, BodyPart.BP_PETWEAR2, BodyPart.BP_PETWEAR3 };
		public void EncodeMultiPetInfo(COutPacket p) // this is just used for the CharacterInfo packet
		{
			for (byte i = 0; i < 3; i++)
			{
				if (Pets[i] != null)
				{
					p.Encode1(1);

					var cp = Pets[i];
					p.Encode4(cp.dwTemplateID);
					p.EncodeString(cp.sName);
					p.Encode1(cp.Level);
					p.Encode2(cp.Tameness);
					p.Encode1(cp.Repleteness); //i think
					p.Encode2(cp.Skill);
					p.Encode4(InventoryManipulator.GetItem(Parent, petwearparts[i], true)?.nItemID ?? 0); // TODO verify
				}
			}

			p.Encode1(0);
		}

		public void OnPetPacket(RecvOps opCode, CInPacket p)
		{
			switch (opCode)
			{
				case RecvOps.CP_PetDropPickUpRequest:

					break;
				case RecvOps.CP_PetInteractionRequest:

					break;
				case RecvOps.CP_UserActivatePetRequest:

					break;
				case RecvOps.CP_UserDestroyPetItemRequest:

					break;
				default:
					var liPetLockerSN = p.Decode8();

					var item = Pets.FirstOrDefault(pet => pet.liPetLockerSN == liPetLockerSN);

					if (item is null) return;

					switch (opCode)
					{
						case RecvOps.CP_PetMove:
							item.Move(p);
							break;
						case RecvOps.CP_PetStatChangeItemUseRequest: //  CPet::OnNameChanged(v4, iPacket);
							break;
						case RecvOps.CP_PetUpdateExceptionListRequest:
							break;

						case RecvOps.CP_PetAction:
						case RecvOps.CP_PetActionCommand:

							var tick = p.Decode4();
							var nType = p.Decode1();
							var nAction = p.Decode1();
							var sMsg = p.DecodeString(); // rebroadcasting this is bad practice

							Parent.Field.Broadcast(item.PetActionCommand((PetActType)nType, nAction, true, true), Parent);

							break;
					}
					break;
			}
		}

		/// <summary>
		/// Feeds a users Pet.
		/// </summary>
		/// <param name="nOption">Repleteness or tameness</param>
		/// <param name="bCash">If it's cash pet food or not</param>
		public void EatFood(int nOption, bool bCash)
		{
			if (Constants.MULTIPET_ACTIVATED)
			{
				Parent.SendMessage("Feeding for multipets is not complete.");
				return;
			}

			var pet = Parent.Pets.Pets[0];

			if (pet == null)
			{
				Parent.SendMessage("No active pet");
				return;
			}

			if (bCash)
			{
				pet.OnEatCashFood(nOption);
			}
			else
			{
				pet.OnEatFood(nOption);
			}
		}

		public void PetChangeNameRequest(int nItemID, string sName)
		{
			// validate
			// change name internally

			var pet = Pets.First();

			if (pet is null)
			{
				Parent.SendMessage("Please activate a pet first.");
				return;
			}

			if (sName.Length < 4 || sName.Length > 13)
			{
				Parent.SendMessage("name too long u maggot");
				return;
			}

			pet.PetItem.sPetName = sName;
			pet.UpdatePetItem();
			Parent.SendPacket(pet.NameChanged());
		}
	}
}
