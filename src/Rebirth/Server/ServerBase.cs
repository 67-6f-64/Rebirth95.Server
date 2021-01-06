using Autofac;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using log4net;
using Rebirth.Network;
using System.Threading;
using System.Net;
using Rebirth.Tools;

namespace Rebirth.Server
{
	public abstract class ServerBase<TClient> where TClient : CSocketBase
	{
		public static ILog Log = LogManager.GetLogger(typeof(ServerBase<TClient>));

		private readonly ServerAppThread m_thread;
		private readonly SockAcceptorEx m_acceptor;

		public bool Running { get; private set; }

		public string Name { get; }
		public List<TClient> Clients { get; }
		public int Port => m_acceptor.Port;

		//public ServerAppThread MasterThread => m_thread;

		public ServerBase(string name, int port)
		{
			Name = name;
			Clients = new List<TClient>();

			m_thread = ServerApp.Container.Resolve<ServerAppThread>(); //TODO: DI with ctor

			m_acceptor = new SockAcceptorEx(IPAddress.Any, port);
			m_acceptor.OnClientAccepted += OnClientAccepted;
		}

		public CTimer CreateTimer() => new CTimer(m_thread);

		public abstract void OnKeepAlive();

		private void OnClientAccepted(Socket socket)
		{
			m_thread.AddConnectTask(() =>
			{
				var client = CreateClient(socket);

				//TODO: IP Ban Checks

				Log.InfoFormat("[{0}] Accepted {1}", Name, client.Host);

				client.OnPacket += (packet) => m_thread.AddReceiveTask(() => HandlePacket(client, packet));
				client.OnDisconnected += () => m_thread.AddDisconnectTask(() => HandleDisconnect(client));

				Clients.Add(client);

				client.Initialize(); //Enqueue(() => client.Initialize());
			});
		}

		protected virtual void HandlePacket(TClient client, CInPacket packet)
		{

		}
		protected virtual void HandleDisconnect(TClient client)
		{
			Clients.Remove(client);

			Log.InfoFormat("[{0}] Disconnected {1}", Name, client.Host);
		}

		protected abstract TClient CreateClient(Socket socket);

		public void Start()
		{
			m_acceptor.StartListen();

			var n = "[" + Name + "]";
			Log.InfoFormat($"{n.PadRight(13)} -> listening on port {m_acceptor.Port}");

			Running = true;
		}
		public void Stop()
		{
			m_acceptor.StopListen();

			var arr = Clients.ToArray(); //TODO: Redo

			foreach (var client in arr)
				client.Dispose();
			
			Running = false;
		}
	}
}
