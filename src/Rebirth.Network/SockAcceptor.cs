using System;
using System.Net;
using System.Net.Sockets;

namespace Rebirth.Network
{
    public class SockAcceptor : IDisposable
    {
        private readonly IPAddress m_address;
        private int m_port;
        private Socket m_sock;
        private bool m_active;
        private bool m_disposed;

        public IPAddress Addresss => m_address;
        public int Port => m_port;

        public bool Active => m_active;

        public bool ExclusiveAddress { get; set; }
        //public bool DualMode { get; set; }

        public bool Disposed => m_disposed;

        public event Action<Socket> OnClientAccepted;

        public SockAcceptor(IPAddress address)
        {
            m_address = address;

            ExclusiveAddress = true;
            //DualMode = false;

            m_active = false;
            m_disposed = false;
        }

        public void SetPort(int port)
        {
            //TODO: Check port before setting it lol
            m_port = port;
            ThrowIfBadPort();
        }

        public void StartListen(int backlog = 100)
        {
            //backlog default 2147483647

            ThrowIfDisposed();
            ThrowIfBadPort();

            if (m_active)
                throw new InvalidOperationException();

            m_sock = SocketEx.CreateTcpSock();
            //m_sock.DualMode = DualMode;
            m_sock.ExclusiveAddressUse = ExclusiveAddress;

            m_sock.Bind(new IPEndPoint(m_address, m_port));

            m_active = true;

            try
            {
                m_sock.Listen(backlog);
            }
            catch (SocketException)
            {
                StopListen();
                throw;
            }

            BeginAccept(); //Maple does 10
        }
        public void StopListen()
        {
            ThrowIfDisposed();

            if (!m_active)
                throw new InvalidOperationException();

            m_active = false;
            m_sock.Close();
        }

        private void BeginAccept()
        {
            if (!m_disposed && m_active)
                m_sock.BeginAccept(EndAccept, null);
        }
        private void EndAccept(IAsyncResult iar)
        {
            try
            {
                var socket = m_sock.EndAccept(iar);
                OnClientAccepted?.Invoke(socket);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
            finally
            {
                BeginAccept();
            }
        }

        public bool Pending()
        {
            if (!m_active)
                throw new InvalidOperationException();

            return m_sock.Poll(0, SelectMode.SelectRead);
        }

        protected void ThrowIfDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(GetType().Name);
        }
        protected void ThrowIfBadPort()
        {
            var port = m_port;

            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), @"Port is out of range");
        }

        public virtual void Dispose()
        {
            if (!m_disposed)
            {
                if (m_active)
                    StopListen();

                m_disposed = true;
            }
        }
    }
}
