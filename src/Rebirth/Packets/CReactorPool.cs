using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;

namespace Rebirth
{
	public partial class CPacket
	{
		public static class CReactorPool
		{
			public static COutPacket ReactorChangeState(CReactor reactor, short tActionDelay, byte nProperEventIdx)
			{
				//CReactor::MakeStateChangePacket
				var p = new COutPacket(SendOps.LP_ReactorChangeState);
				p.Encode4(reactor.dwId); //m_dwId
				p.Encode1(reactor.nState); //nState
				reactor.Position.EncodePos(p); // m_ptPos.x | m_ptPos.y

				p.Encode2(tActionDelay);
				p.Encode1(nProperEventIdx);

				p.Encode1(reactor.nMaxState);

				return p;
			}

			public static COutPacket MakeEnterFieldPacket(CReactor reactor)
			{
				//CReactor::MakeEnterFieldPacket
				var p = new COutPacket(SendOps.LP_ReactorEnterField);
				p.Encode4(reactor.dwId);
				p.Encode4(reactor.nReactorTemplateID);
				p.Encode1(reactor.nState);
				reactor.Position.EncodePos(p);
				p.Encode1(reactor.bFlipped); //bFlip
				p.Skip(2); //A string?????

				return p;
			}

			public static COutPacket MakeLeaveFieldPacket(CReactor reactor)
			{
				//CReactor::MakeLeaveFieldPacket
				var p = new COutPacket(SendOps.LP_ReactorLeaveField);
				p.Encode4(reactor.dwId); //m_dwId
				p.Encode1(reactor.nState); //nState
				reactor.Position.EncodePos(p); //m_ptPos.x | m_ptPos.y
				return p;
			}
		}
	}
}
