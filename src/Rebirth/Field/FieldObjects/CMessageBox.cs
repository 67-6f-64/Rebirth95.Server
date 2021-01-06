using Rebirth.Network;
using System;

namespace Rebirth.Field.FieldObjects
{
	public class CMessageBox : CFieldObj
	{
		public DateTime StartTime { get; set; }
		public int Duration => 3600000;

		public int nItemID { get; set; }
		public string sCharacterName { get; set; }
		public string sMessage { get; set; }

		public override COutPacket MakeEnterFieldPacket() => CPacket.MessageBoxEnterField(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.MessageBoxLeaveField(dwId, true);
	}
}
