using Rebirth.Characters;
using Rebirth.Entities.Item;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;
using System;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.String;

namespace Rebirth.Field.FieldObjects
{
	public class CPet : CFieldObj
	{
		public int dwParentId { get; }
		public Character Parent
			=> MasterManager.CharacterPool.Get(dwParentId);

		public int dwTemplateID { get; }

		public GW_ItemSlotPet PetItem => Parent.InventoryCash.Get(PetItemSlot) as GW_ItemSlotPet;
		public short PetItemSlot { get; set; }

		public long liPetLockerSN { get; }
		public long liSN { get; }

		//Think about these
		public byte nIdx { get; set; }
		public byte nRemoveReason { get; set; }

		public byte nNameTag => (byte)PetItem.PetTemplate.NameTag; // TODO should be zero until a name tag is equipped
		public byte nChatBalloon => (byte)PetItem.PetTemplate.ChatBalloon; // TODO should be zero until a chat balloon is equipped

		public string sName => PetItem.sPetName;
		public byte Level => PetItem.nLevel;
		public short Tameness => PetItem.nTameness;
		public byte Repleteness => PetItem.nRepleteness;
		public short Skill => PetItem.usPetSkill;

		public short nOvereat { get; private set; }
		public DateTime tRemainHungriness { get; set; }

		public DateTime tLastUpdated { get; set; }

		public CPet(int dwParentId, short nItemPOS, GW_ItemSlotPet gwPetItem)
		{
			this.dwParentId = dwParentId;
			liPetLockerSN = gwPetItem.liCashItemSN;
			liSN = gwPetItem.liSN;
			PetItemSlot = nItemPOS;

			tLastUpdated = DateTime.Now;
			tRemainHungriness = DateTime.Now;

			dwTemplateID = gwPetItem.nItemID;

			if (gwPetItem.sPetName.Length == 0)
			{
				gwPetItem.sPetName = MasterManager.StringData[StringDataType.Item][dwTemplateID].Name;
			}
		}

		public void UpdatePetItem()
		{
			Parent.Modify.Inventory(ctx => ctx.UpdatePetItem(PetItem, PetItemSlot));
		}

		public void UpdatePetAbility()
		{
			// BMS -- CPet::UpdatePetAbility
			// TODO
		}

		// CPet::Update(CPet *this, int tCur, int *bRemove)
		public void Update()
		{
			tRemainHungriness.AddMilliseconds((DateTime.Now - tLastUpdated).TotalMilliseconds); // v3->m_tRemainHungriness -= tCur - v3->m_tLastUpdated;

			tLastUpdated = DateTime.Now;

			UpdatePetAbility();

			var bModified = false;
			var bRemove = false;

			if ((tRemainHungriness - DateTime.Now).TotalMilliseconds < 0)
			{
				var hungerFactor = 6 * PetItem.PetTemplate.Hungry;

				if (hungerFactor == 36) hungerFactor = 30; // divide by zero -- not possible with current pets but maybe if we import new ones idk

				tRemainHungriness = DateTime.Now.AddMilliseconds(1000 * (Constants.Rand.Next() % (36 - hungerFactor) + 60));

				PetItem.nRepleteness -= 1;

				bModified = true;
			}

			if (Repleteness <= 0)
			{
				bRemove = true;
				bModified = true;

				PetItem.nTameness -= 1;

				if (PetItem.nTameness < 0) PetItem.nTameness = 0;

				PetItem.nRepleteness = 5;
			}

			if (bModified)
			{
				UpdatePetItem();
			}

			if (bRemove)
			{
				if (Constants.MULTIPET_ACTIVATED) throw new NotImplementedException();

				nRemoveReason = 1; // no food
				Parent.Pets.DeactivateSinglePet();
			}
		}

		/// <summary>
		/// For cash pet food
		/// From cash item description: Recovers entire Fullness. \nIncrease 100 Closeness.
		/// Handler contents copied from BMS
		/// </summary>
		/// <param name="niTameness">AKA fullness</param>
		public void OnEatCashFood(int niTameness)
		{
			PetItem.nRepleteness = 100; // p->nRepleteness = 100;
			tRemainHungriness = DateTime.Now.AddMinutes(5); // v2->m_tRemainHungriness = 600000;
			IncTameness(niTameness);

			UpdatePetItem();

			Parent.SendPacket(PetActionCommand(PetActType.Feed, 1, true, true));
			Parent.SendPacket(CashPetFoodResult(true)); // this would be false if item doenst match the pet but idc about checking that
		}

		/// <summary>
		/// For non-cash pet food
		/// </summary>
		/// <param name="niRepleteness"></param>
		public void OnEatFood(int niRepleteness)
		{
			var nInc = niRepleteness;

			if (Repleteness + niRepleteness > 100)
			{
				nInc = 100 - Repleteness;
			}

			PetItem.nRepleteness += (byte)nInc;

			var bModified = nInc > 0;

			tRemainHungriness = DateTime.Now.AddMilliseconds(1000 * (Constants.Rand.Next() % 10 + nOvereat * nOvereat + 10));

			if (10 * nInc / niRepleteness <= Constants.Rand.Next() % 12
				|| Repleteness / 10 <= Constants.Rand.Next() % 12)
			{
				if (nInc == 0)
				{
					var rand = Constants.Rand.Next();

					if (nOvereat != 10)
					{
						rand %= 10 - nOvereat;
					}

					if (rand > 0)
					{
						nOvereat += 1;
					}
					else
					{
						PetItem.nTameness = (short)Math.Max(Tameness - 1, 0);
						nOvereat = 10;
						bModified = true;
					}
				}
			}
			else
			{
				IncTameness(1);
			}

			if (bModified)
			{
				UpdatePetItem();
			}

			Parent.SendPacket(PetActionCommand(PetActType.Feed, 1, nInc > 0, true));
		}

		public void IncTameness(int nAmount)
		{
			if (Tameness + nAmount >= 0)
			{
				if (Tameness + nAmount > 30000)
				{
					nAmount = 30000 - Tameness;
				}
			}
			else
			{
				nAmount = -Tameness;
			}

			if (nAmount == 0) return;

			PetItem.nTameness = (short)(nAmount + Tameness);

			var level = GameConstants.GetPetLevel(Tameness);

			if (level > Level)
			{
				PetItem.nLevel = (byte)level;
				new UserEffectPacket(UserEffect.PetShowEffect)
				{
					nType = 0, // unsure rn
					cPet = nIdx,
				}.BroadcastEffect(Parent);
			}
		}

		public void Move(CInPacket p)
		{
			Parent.Field.Broadcast(PetMove(p), Parent);
		}

		public COutPacket PetMove(CInPacket iPacket)
		{
			var oPacket = new COutPacket(SendOps.LP_PetMove);
			oPacket.Encode4(dwParentId);
			oPacket.Encode1(nIdx);
			Position.UpdateMovePath(oPacket, iPacket);
			return oPacket;
		}

		public void Encode(COutPacket p)
		{
			p.Encode4(dwTemplateID);
			p.EncodeString(sName ?? "Nameless Pet");
			p.Encode8(liPetLockerSN);
			p.Encode2(Position.X);
			p.Encode2(Position.Y);
			p.Encode1(Position.MoveAction);
			p.Encode2(Position.Foothold);
			p.Encode1(nNameTag);
			p.Encode1(nChatBalloon);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nType"></param>
		/// <param name="nOption"></param>
		/// <param name="bSuccess"></param>
		/// <param name="bChatBalloon"></param>
		/// <returns></returns>
		public COutPacket PetActionCommand(PetActType nType, byte nOption, bool bSuccess, bool bChatBalloon)
		{
			var p = new COutPacket(SendOps.LP_PetActionCommand);
			p.Encode4(dwParentId);
			p.Encode1(nIdx);
			p.Encode1((byte)nType); // nType
			p.Encode1(nOption); // &v2->m_pTemplate->m_aInteraction.a[CInPacket::Decode1(iPacket)];
			p.Encode1(bSuccess);
			p.Encode1(bChatBalloon);
			return p;
		}

		public COutPacket CashPetFoodResult(bool bSuccess)
		{
			var p = new COutPacket(SendOps.LP_CashPetFoodResult);
			p.Encode1(!bSuccess); // inverse for some reason
			p.Encode1((byte)(dwTemplateID % 10000)); // sound effect
			return p;
		}

		public COutPacket NameChanged() // todo move this elsewhere
		{
			var p = new COutPacket(SendOps.LP_PetNameChanged);
			p.Encode8(liPetLockerSN);
			p.EncodeString(sName);
			p.Encode1(1); // test, this is a boolean

			return p;
		}

		public override COutPacket MakeEnterFieldPacket()
		{
			// Send [LP_PetActivated] [C6 00] [71 04 00 00] [00] [01] [01] 00 00 00 00 00 00 4E 87 93 03 00 00 00 00 00 00 EC FF 00 00 00 00 00

			var p = new COutPacket(SendOps.LP_PetActivated);
			p.Encode4(dwParentId);
			p.Encode1(nIdx);

			p.Encode1(1);  //Add 

			p.Encode1(0); //Research this?????
			Encode(p);
			return p;
		}

		public override COutPacket MakeLeaveFieldPacket()
		{
			var p = new COutPacket(SendOps.LP_PetActivated);
			p.Encode4(dwParentId);
			p.Encode1(nIdx);
			p.Encode1(0); //Remove
			p.Encode1(nRemoveReason);

			//nRemoveReason
			//1: The pet went back home because it's hungry.
			//2: The pet's magical time has run out, and so it has turned back into a doll.
			//3: You cannot use a pet in this location.
			//4: Cannot summon pet while Following.
			//Default: No Message

			return p;
		}
	}
}
