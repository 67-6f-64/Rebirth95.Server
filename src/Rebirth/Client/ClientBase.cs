using System;
using System.Net.Sockets;
using log4net;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth.Client
{
	//TODO: Gotta make this guy and his child* disposable
	public abstract class ClientBase : CClientSocket
	{
		public static ILog Log = LogManager.GetLogger(typeof(ClientBase));

		//public SecurityClient Security { get; }
		public bool LoggedIn { get; set; }

		public byte WorldID { get; set; }
		public byte ChannelId { get; set; }

		protected ClientBase(Socket socket) : base(socket)
		{
			//Security = new SecurityClient(this);
		}

		public override void Initialize()
		{
			Initialize(Constants.VersionMajor, Constants.VersionMinor, Constants.VersionLocale);
		}

		public new void SendPacket(COutPacket packet)
		{
			var buffer = packet.ToArray();
			var opcode = (SendOps)BitConverter.ToInt16(buffer, 0);

			if (Constants.FilterSendOpCode(opcode) == false)
			{
				var name = Enum.GetName(typeof(SendOps), opcode);
				var str = HexTool.ToString(buffer);

				Log.InfoFormat("Send [{0}] {1}", name, str);
			}

			base.SendPacket(packet);
		}

		public virtual void Disconnect()
		{
			Log.Debug("Disconnecting client.");
			Dispose();
		}
	}
}
