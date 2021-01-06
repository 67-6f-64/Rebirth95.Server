using System.Collections.Generic;
using Rebirth.Client;
using Rebirth.Network;

namespace Rebirth.Tools
{
	public class PacketHandler<TClient> where TClient : ClientBase
	{
		public delegate void PacketHandlerType(TClient c, CInPacket p);

		private struct HandlerEntry
		{
			//public bool TickCountPacket;
			//public int RequiredInterval;
			public bool RequireLogin;
			public PacketHandlerType Handler;
		}

		private readonly Dictionary<short, HandlerEntry> m_handlers;

		public PacketHandler()
		{
			m_handlers = new Dictionary<short, HandlerEntry>();
		}

		public void Add(short opCode, PacketHandlerType handler, bool requireLogin = true)
		{
			var entry = new HandlerEntry()
			{
				RequireLogin = requireLogin,
				Handler = handler
			};

			m_handlers.Add(opCode, entry);
		}

		public bool Remove(short opCode) => m_handlers.Remove(opCode);

		public void Handle(TClient c, CInPacket p)
		{
			var opCode = p.Decode2();

			if (m_handlers.TryGetValue(opCode, out var handler))
			{
				if (handler.RequireLogin && !c.LoggedIn)
				{
					c.Disconnect();
				}
				else
				{
					handler.Handler(c, p);
				}
			}
		}
	}
}
