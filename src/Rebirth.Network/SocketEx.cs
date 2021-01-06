using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Rebirth.Network
{
    public static class SocketEx
    {
        /// <summary>
        /// Creates a tcp socket with:
        /// AddressFamily.InterNetwork | SocketType.Stream | ProtocolType.Tcp
        /// </summary>
        /// <returns>Socket with tcp config</returns>
        public static Socket CreateTcpSock() => new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Sets socket to keep alive and nodelay
        /// </summary>
        /// <param name="socket">Socket to set options </param>
        /// <returns>Sockets remote endpoint</returns>
        public static string SetSockOpt(ref Socket socket)
        {
            if (socket != null)
            {
                try
                {
                    var temp = socket.RemoteEndPoint != null ?
                        socket.RemoteEndPoint.ToString() : "null"; //Cache before setting socket options

                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                    return temp;
                }
                catch (SocketException) { /*Socket No Longer Connected*/ }
            }

            return "<error>";
        }

        public static int GetAvailablePort(int startingPort)
        {
            var portArray = IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Where(ep => ep.Port >= startingPort)
                .Select(ep => ep.Port)
                .ToArray();

            return Enumerable
                .Range(startingPort, UInt16.MaxValue)
                .FirstOrDefault(i => !portArray.Contains(i));
        }
    }
}
