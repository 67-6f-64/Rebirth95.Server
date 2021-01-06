using Rebirth.Client;
using Rebirth.Custom;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using Rebirth.Tools;

namespace Rebirth.Server
{
    public abstract class WvsServerBase<TClient> : ServerBase<TClient> where TClient : ClientBase
    {
        public WvsCenter Center { get; }
        public PacketHandler<TClient> PacketHandler { get; }

        protected WvsServerBase(string name, int port, WvsCenter parent) : base(name, port)
        {
            Center = parent;

            PacketHandler = new PacketHandler<TClient>();
            PacketHandler.Add((short)RecvOps.CP_AliveAck, Handle_AliveAck, false);
            //PacketHandler.Add((short)RecvOps.CP_CustomSecurityInit, (c, p) => c.Security.HandleInit(p), false);
            //PacketHandler.Add((short)RecvOps.CP_CustomSecurityAlive, (c, p) => c.Security.HandleAlive(p), false);
        }

        public override void OnKeepAlive()
        {
            using (var pAliveReq = CPacket.AliveReq())
            {
                foreach (var client in Clients)
                {
                    client.SendPacket(pAliveReq);
                    //client.Security.Update();
                }
            }
        }

        protected override void HandlePacket(TClient client, CInPacket packet)
        {
            var buffer = packet.ToArray();
            var opcode = (RecvOps)BitConverter.ToInt16(buffer, 0);

            if (Constants.FilterRecvOpCode(opcode) == false)
            {
                var name = Enum.GetName(typeof(RecvOps), opcode);
                var str = HexTool.ToString(buffer);

                Log.InfoFormat("Recv [{0}] {1}", name, str);
            }

            PacketHandler.Handle(client, packet);
        }

        protected void Handle_AliveAck(TClient c, CInPacket p)
        {
            //TODO: Set ping time and handle in the PingProc
        }
    }
}
