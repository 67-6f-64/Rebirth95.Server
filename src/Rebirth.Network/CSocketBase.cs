using System;
using System.Buffers;
using System.Net.Sockets;

namespace Rebirth.Network
{
    public abstract class CSocketBase : IDisposable
    {
        protected static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Create();

        public const int ReceiveSize = 16384;

        protected readonly Socket m_socket;

        protected byte[] m_buffer;
        protected int m_offset;

        protected readonly object m_sendSync;

        public string Host { get; }
        public bool Disposed { get; private set; }

        public event Action<CInPacket> OnPacket;
        public event Action OnDisconnected;

        public CSocketBase(Socket socket)
        {
            m_socket = socket;

            Host = SocketEx.SetSockOpt(ref m_socket);
            Disposed = false;

            m_buffer = new byte[ReceiveSize];
            m_offset = 0;

            m_sendSync = new object();
        }

        public virtual void Initialize() { }

        protected void Receive()
        {
            if (Disposed)
                return;

            var recvBuffer = BufferPool.Rent(ReceiveSize);
            var state = recvBuffer;

            m_socket.BeginReceive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None, out var errorCode, EndReceive, state);

            if (errorCode != SocketError.Success && errorCode != SocketError.IOPending)
                Dispose();
        }
        private void EndReceive(IAsyncResult iar)
        {
            if (!Disposed)
            {
                int length = m_socket.EndReceive(iar, out var errorCode);

                if (length == 0 || errorCode != SocketError.Success)
                {
                    Dispose();
                }
                else
                {
                    var recvBuffer = (byte[])iar.AsyncState;

                    Append(recvBuffer, length);
                    ManipulateBuffer();

                    BufferPool.Return(recvBuffer);

                    Receive();
                }
            }
        }

        protected void SendSync(byte[] buffer, int start, int length)
        {
            // Since the socket is in blocking mode this will always complete
            // after ALL the requested number of bytes was transferred.
            int sent = m_socket.Send(buffer, start, length, SocketFlags.None, out var errorCode);

            if (sent == 0 || errorCode != SocketError.Success)
                Dispose();
        }

        protected void SendAsync(byte[] buffer, int start, int length, object state)
        {
            if (Disposed)
                return;

            m_socket.BeginSend(buffer, start, length, SocketFlags.None, out var errorCode, EndSend, state);

            if (errorCode != SocketError.Success && errorCode != SocketError.IOPending)
                Dispose();
        }
        private void EndSend(IAsyncResult iar)
        {
            if (!Disposed)
            {
                int length = m_socket.EndSend(iar, out var errorCode);

                if (length == 0 || errorCode != SocketError.Success)
                {
                    Dispose();
                }

                var poolBuffer = (byte[])iar.AsyncState;
                BufferPool.Return(poolBuffer);
            }
        }
        
        protected void Append(byte[] input, int length)
        {
            if (m_buffer.Length - m_offset < length)
            {
                int newSize = m_buffer.Length * 2;

                while (newSize < m_offset + length)
                    newSize *= 2;

                Array.Resize(ref m_buffer, newSize);
            }

            Buffer.BlockCopy(input, 0, m_buffer, m_offset, length);

            m_offset += length;
        }

        public abstract void SendPacket(COutPacket packet);
        protected abstract void ManipulateBuffer();

        protected void InvokeOnPacket(CInPacket packet) => OnPacket?.Invoke(packet);

        public virtual void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                }
                catch { /*Nothing*/ }

                try
                {
                    m_socket.Close();
                }
                catch { /*Nothing*/ }

                m_buffer = null;
                m_offset = 0;

                OnDisconnected?.Invoke();
            }
        }
    }
}
