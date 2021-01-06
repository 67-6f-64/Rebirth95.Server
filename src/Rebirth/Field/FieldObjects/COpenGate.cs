using Rebirth.Network;
using Rebirth.Server.Center;
using System;

namespace Rebirth.Field.FieldObjects
{
	public class COpenGate : CFieldObj
	{
		public DateTime StartTime { get; set; }
		public bool bFirst { get; set; }
		public byte nState { get; set; }
		public int dwCharacterID { get; set; }
		public int nPartyID => MasterManager.CharacterPool.Get(dwCharacterID)?.Party?.PartyID ?? 0;
		public int tDoorOpen { get; set; }
		public int bActive { get; set; }

		public COpenGate(int dwParentID)
		{
			StartTime = DateTime.Now;
			dwCharacterID = dwParentID;
		}

		//COpenGatePool::OnPacket
		private COutPacket OpenGateCreated()
		{
			var p = new COutPacket(SendOps.LP_OpenGateCreated);
			p.Encode1(1); // nEnterType
			p.Encode4(dwCharacterID);
			Position.EncodePos(p);
			p.Encode1(bFirst);
			p.Encode4(nPartyID); //Unkthey 
			return p;
		}

		private COutPacket OpenGateRemoved()
		{
			var p = new COutPacket(SendOps.LP_OpenGateRemoved);
			p.Encode1(1); // nLeaveType
			p.Encode4(dwCharacterID);
			p.Encode1(bFirst);
			return p;
		}

		public override COutPacket MakeEnterFieldPacket()
			=> OpenGateCreated();
		public override COutPacket MakeLeaveFieldPacket()
			=> OpenGateRemoved();
	}
}
