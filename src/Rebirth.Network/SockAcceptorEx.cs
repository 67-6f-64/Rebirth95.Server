using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Rebirth.Network
{
    public class SockAcceptorEx : IDisposable
    {
        private Socket Socket { get; set; }
        private Thread Thread { get; set; }
        private ManualResetEvent Event { get; set; }

        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public bool Active { get; private set; }
        public bool Disposed { get; private set; }              

        public event Action<Socket> OnClientAccepted;
        public event Action<Exception> OnException;

        public SockAcceptorEx(IPAddress address,int port)
        {
            Event = new ManualResetEvent(false);

            Address = address;
            Port = port;

            Active = false;
            Disposed = false;
        }

        public void StartListen(int nBackLog = 10)
        {
            var endpoint = new IPEndPoint(Address, Port);

            Socket = SocketEx.CreateTcpSock();

            Socket.Bind(endpoint);
            Socket.Listen(nBackLog);

            Thread = new Thread(BeginAccept);
            Thread.Name = $"SockAcceptorEx-{Port}";
            Thread.IsBackground = true;
            Thread.Priority = ThreadPriority.AboveNormal;

            Thread.Start();
        }
        public void StopListen()
        {
            Active = false;
            Event?.Set();
            Thread?.Join();
        }

        private void BeginAccept()
        {
            Active = true;

            while(Active)
            {
                Event.Reset();

                try
                {
                    Socket.BeginAccept(EndAccept, null);
                }
                catch(Exception ex)
                {
                    OnException?.Invoke(ex);
                    break;
                }

                Event.WaitOne();
            }

            Active = false;
            Event.Dispose();
        }

        private void EndAccept(IAsyncResult iar)
        {
            try
            {
                Event.Set();

                var socket = Socket.EndAccept(iar);
                OnClientAccepted?.Invoke(socket);
            }
            catch(ObjectDisposedException)
            {
                //Nothing
            }
            catch(Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }

        public void Dispose()
        {
            if(!Disposed)
            {
                Disposed = true;

                StopListen();
            }
        }
    }
}
