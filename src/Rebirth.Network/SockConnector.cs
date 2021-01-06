using System;
using System.Net.Sockets;

namespace Rebirth.Network
{
    public class SockConnector
    {
        public string Host { get; }
        public int Port { get; }

        public event Action<Socket> OnConnected;
        public event Action<SocketError> OnError;

        public SockConnector(string host,int port)
        {
            Host = host;
            Port = port;
        }

        public void Create()
        {
            var sock = SocketEx.CreateTcpSock();
            var iar = sock.BeginConnect(Host, Port, EndConnect, sock);
        }

        private void EndConnect(IAsyncResult iar)
        {
            var sock = iar.AsyncState as Socket;

            try
            {
                sock.EndConnect(iar);
                OnConnected?.Invoke(sock);
            }
            catch (SocketException se)
            {
                OnError?.Invoke(se.SocketErrorCode);
                sock.Dispose();
            }
        }
    }
}
