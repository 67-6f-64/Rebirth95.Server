using Rebirth.Characters;
using Rebirth.Characters.Inventory;
using Rebirth.Characters.Stat;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Entities
{
	public class AvatarLook
	{
		public byte nGender;
		public int nSkin;
		public int nFace;
		public int nHair;
		public int nWeaponStickerID;
		public int[] aEquip;
		public int[] aUnseenEquip;
		public int[] anPetID;

		public AvatarLook()
		{
			aEquip = new int[60];
			aUnseenEquip = new int[60];
			anPetID = new int[3];
		}

		public void CopyStats(CharacterStat stat)
		{
			nGender = stat.nGender;
			nSkin = stat.nSkin;
			nFace = stat.nFace;
			nHair = stat.nHair;
		}

		public void CopyInventory(CharacterInventoryEquips aInvEquippedNormal, CharacterInventoryEquips aInvEquippedCash)
		{
			foreach (var item in aInvEquippedCash)
			{
				var idx = System.Math.Abs(item.Key);

				idx -= 100;

				if (idx == (short)BodyPart.BP_WEAPON) continue; // handled at the end for some reason

				aEquip[idx] = item.Value.nItemID;
			}

			foreach (var item in aInvEquippedNormal)
			{
				var idx = System.Math.Abs(item.Key);

				if (aEquip[idx] == 0)
				{
					aEquip[idx] = item.Value.nItemID;
				}
				else
				{
					aUnseenEquip[idx] = item.Value.nItemID;
				}
			}

			var nWeaponStickerPOS = (short)-((int)BodyPart.BP_WEAPON + 100);

			nWeaponStickerID = aInvEquippedCash.Get(nWeaponStickerPOS)?.nItemID ?? 0;
		}

		public void CopyPets(CharacterPets pets)
		{
			for (var i = 0; i < 3; i++)
			{
				anPetID[i] = pets.Pets[i]?.dwTemplateID ?? 0;
			}
		}

		public void Encode(COutPacket p, bool bMega = false)
		{
			p.Encode1(nGender);
			p.Encode1((byte)nSkin);
			p.Encode4(nFace);
			p.Encode1(bMega); // mega
			p.Encode4(nHair);

			EncodeInventory(p, aEquip);
			EncodeInventory(p, aUnseenEquip);

			p.Encode4(nWeaponStickerID);//Cash Weapon

			foreach (int nPetId in anPetID) //3
				p.Encode4(nPetId);
		}

		private static void EncodeInventory(COutPacket p, int[] aInventory)
		{
			for (var i = 0; i < aInventory.Length; i++)
			{
				var value = aInventory[i];

				if (value == 0)
					continue;

				p.Encode1((byte)i);
				p.Encode4(value);
			}
			p.Encode1(0xFF);
		}
	}
}
