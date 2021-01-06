using Rebirth.Characters.Modify;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rebirth.Common.Types;

namespace Rebirth.Characters
{
	public class CharacterTamingMob
	{
		public Character Parent { get; private set; }
		public int dwParentID { get; }

		public int nItemID => Parent.EquippedInventoryNormal.Get(-(short)BodyPart.BP_TAMINGMOB)?.nItemID ?? 0;

		public int TamingMobLevel { get; set; } // m_nTamingMobLevel
		public int TamingMobExp { get; set; } // m_nTamingMobExp
		public int TamingMobFatigue { get; set; } // m_nTamingMobFatigue

		public CharacterTamingMob(Character c)
		{
			Parent = c;
			dwParentID = c.dwId;
		}

		public void Dispose()
		{
			Parent = null;

		}

		// CUser::UpdateTamingMobInfo(CUser *this, int tCur)
		public void UpdateTamingMobInfo()
		{
			// TODO
		}

		/// <summary>
		/// Used for informing client of changes to taming mob information.
		/// </summary>
		/// <param name="bLevelUp"></param>
		/// <returns></returns>
		public COutPacket SetTamingMobInfo(bool bLevelUp)
		{
			//v7->m_nTamingMobLevel = CInPacket::Decode4(v3);
			//v7->m_nTamingMobExp = CInPacket::Decode4(v3);
			//v7->m_nTamingMobFatigue = CInPacket::Decode4(v3);
			//v21 = 0;
			//v7->m_bTamingMobTired = CUser::IsTamingMobTired(v7);
			//CAvatar::ClearActionLayer((CAvatar*)&v7->vfptr, v21);
			//v7->vfptr->PrepareActionLayer((CAvatar*)&v7->vfptr, 6, 100, 0);
			//v8 = CInPacket::Decode1(v3);

			var p = new COutPacket(SendOps.LP_SetTamingMobInfo);
			p.Encode4(dwParentID);
			Encode(p);
			p.Encode1(bLevelUp); // some kind of boolean, maybe levelup notification??
			return p;
		}

		public void Encode(COutPacket p)
		{
			p.Encode4(TamingMobLevel);
			p.Encode4(TamingMobExp);
			p.Encode4(TamingMobFatigue);
		}

		public async Task LoadFromDB()
		{
			// todo
			TamingMobExp = 1;
			TamingMobFatigue = 1;
			TamingMobLevel = 1;
		}

		public void SaveToDB()
		{
			// todo
		}
	}
}
