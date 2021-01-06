using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Network;
using System.Collections.Generic;
using System.Linq;

namespace Rebirth.Field.FieldPools
{
	public class CTownPortalPool : CObjectPool<CTownPortal>
	{
		public CTownPortalPool(CField parent)
			: base(parent) { }

		public void Update()
		{
			var toRemove = new List<CTownPortal>();
			foreach(var portal in this)
			{
				if (portal.StartTime.MillisSinceStart() >= portal.Duration) toRemove.Add(portal);
			}
			toRemove.ForEach(portal => Remove(portal));
		}

		public void OnPacket(WvsGameClient c, CInPacket p)
		{
			var ownerId = p.Decode4();
			if (Contains(ownerId))
			{
				if (c.Character.Party?.TryGetValue(ownerId, out PartyMember item) ?? false)
				{
					c.Character.Action.SetField(Field.ReturnMapId);
				}
			}
		}

		public void CreateTownPortal(Character c, int nSkillID, int x, int y, int tTime)
		{
			var portal = new CTownPortal
			{
				dwCharacterID = c.dwId,
				Position = new CMovePath { X = (short)x, Y = (short)y },
				Duration = tTime
			};

			c.SendPacket(TownPortalResponse(Field.ReturnMapId, Field.MapId, nSkillID, portal.Position.X, portal.Position.Y));

			Add(portal);
		}

		// CWvsContext::OnTownPortal(CWvsContext *this, CInPacket *iPacket)
		// CWvsContext::OnPacket() OpCode 0x45
		public COutPacket TownPortalResponse(int dwTownID, int dwFieldID, int nSkillID, short nPosX, short nPosY)
		{
			var p = new COutPacket(SendOps.LP_TownPortal);
			p.Encode4(dwTownID);
			p.Encode4(dwFieldID);
			if (dwTownID != 999999999 && dwFieldID != 999999999)
			{
				p.Encode4(nSkillID);
				p.Encode2(nPosX);
				p.Encode2(nPosY);
			}
			return p;
		}

		//Must set dwCharacterID before inserting!!
		protected override int GetUniqueId(CTownPortal item) => item.dwCharacterID;
		protected override int GetKeyForItem(CTownPortal item) => item.dwCharacterID;

		protected override void InsertItem(int index, CTownPortal item)
		{
			//item.StartTime = DateTime.Now;

			base.InsertItem(index, item);

			Field.Broadcast(item.MakeEnterFieldPacket());

			item.nState = 1; // no spawn animation
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			if (item != null)
			{
				Field.Broadcast(item.MakeLeaveFieldPacket());
			}

			base.RemoveItem(index);
		}
	}
}
