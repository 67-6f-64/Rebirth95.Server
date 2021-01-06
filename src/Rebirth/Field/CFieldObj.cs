using Rebirth.Characters;
using Rebirth.Game;
using Rebirth.Network;

namespace Rebirth.Field
{
	public abstract class CFieldObj
	{
		public virtual int dwId { get; set; } //CCreature
		public CMovePath Position { get; set; } //TODO: Change to only a tagPOINT

		public CField Field { get; set; }

		//CFieldSplit* m_apSplit[9];
		//__POSITION* m_aPosSplit[9];
		//__POSITION* m_posFieldObjList;

		protected CFieldObj()
		{
			dwId = -1;
			Position = new CMovePath();
		}

		public virtual void Dispose() { }

		public abstract COutPacket MakeEnterFieldPacket();
		public abstract COutPacket MakeLeaveFieldPacket();

		public virtual bool IsShowTo(Character user) => true;
	}
}