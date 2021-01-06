using Rebirth.Entities;
using Rebirth.Server.Login;
using System.Net.Sockets;

namespace Rebirth.Client
{
    public class WvsLoginClient : ClientBase
    {
        public WvsLogin ParentServer { get; }

        public Account Account { get; set; }

        public int LoginAttempts { get; set; }
		public int SelectedUser { get; set; }

		public WvsLoginClient(WvsLogin login, Socket socket) : base(socket)
        {
            ParentServer = login;

            LoginAttempts = 0;
            SelectedUser = 0;
        }

        public override void Initialize()
        {
            base.Initialize();

            //SendPacket(CPacket.Custom.Config(Constants.ClientVersion));
        }
    }
}
