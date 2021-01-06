using Rebirth.Network;
using System;

namespace Rebirth.Field.FieldObjects
{
	public class CTownPortal : CFieldObj
	{
		public DateTime StartTime { get; set; }
		public int Duration { get; set; }

		public byte nState { get; set; }
		public int dwCharacterID { get; set; }

		public override COutPacket MakeEnterFieldPacket() => CPacket.TownPortalCreated(this, 1);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.TownPortalRemoved(dwCharacterID, 1);
	}
}
