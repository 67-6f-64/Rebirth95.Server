using Rebirth.Characters;
using Rebirth.Network;

namespace Rebirth.Field.FieldObjects
{
	public class CNpc : CFieldObj
	{
		//public CLifeTemplate LifeTemplate { get; set; }

		public int TemplateId { get; }
		public short X { get; set; }
		public short Y { get; set; }
		public short Cy { get; set; }
		public bool F { get; set; }
		public short Rx0 { get; set; }
		public short Rx1 { get; set; }
		public short Foothold { get; set; }

		public Character Controller { get; private set; }

		public CNpc(int nTemplateID)
		{
			TemplateId = nTemplateID;
		}

		public void SetController(Character pUser)
		{
			Controller = pUser;
		}

		public void EncodeInitData(COutPacket p)
		{
			p.Encode2(X); //m_ptPosPrev.x
			p.Encode2(Cy); //m_ptPosPrev.y

			p.Encode1(!F);// & 1 | 2 * 2); //m_nMoveAction | life.getF() == 1 ? 0 : 1
			p.Encode2(Foothold); //dwSN Foothold

			p.Encode2(Rx0); //m_ptPosPrev.x
			p.Encode2(Rx1); //m_ptPosPrev.y

			p.Encode1(true); //I hope this works lol | //mplew.write((show && !life.isHidden()) ? 1 : 0);
		}

		public override COutPacket MakeEnterFieldPacket() => CPacket.NpcEnterField(this);
		public override COutPacket MakeLeaveFieldPacket() => CPacket.NpcLeaveField(this);
	}
}
