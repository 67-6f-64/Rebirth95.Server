using Microsoft.IO;
using System;
using System.IO;

namespace Rebirth.Network
{
    /// <summary>
    /// TODO: Implement this https://github.com/RajanGrewal/ByteBuffer
    /// </summary>
    public class COutPacket : IDisposable
    {
        private static readonly RecyclableMemoryStreamManager Manager = new RecyclableMemoryStreamManager();

        private RecyclableMemoryStream m_stream;
        private bool m_disposed;

        public int Length => (int)(m_stream?.Length ?? 0);

        public COutPacket()
        {
            m_stream = new RecyclableMemoryStream(Manager);
            m_disposed = false;
        }

        public COutPacket(SendOps OpCode) : this()
        {
            Encode2((short)OpCode);
        }

        //From LittleEndianByteConverter by Shoftee
        private void Append(long value, int byteCount)
        {
            for (int i = 0; i < byteCount; i++)
            {
                m_stream.WriteByte((byte)value);
                value >>= 8;
            }
        }

        public void Encode1(byte value)
        {
            ThrowIfDisposed();
            m_stream.WriteByte(value);
        }

        public void Encode1(Enum value)
        {
	        ThrowIfDisposed();
	        m_stream.WriteByte(Convert.ToByte(value));
        }

        public void Encode1(bool value)
        {
            ThrowIfDisposed();
            var x = (byte)(value ? 1 : 0);
            m_stream.WriteByte(x);
        }

        public void Encode2(short value)
        {
            ThrowIfDisposed();
            Append(value, 2);
        }

        public void Encode4(int value)
        {
            ThrowIfDisposed();
            Append(value, 4);
        }

        public void Encode4(uint value)
        {
	        ThrowIfDisposed();
	        Append(value, 4);
        }

        public void Encode8(long value)
        {
            ThrowIfDisposed();
            Append(value, 8);
        }

        public void Encode8(double value)
        {
            ThrowIfDisposed();
            Append(BitConverter.DoubleToInt64Bits(value), 8);
        }

        public void EncodeBuffer(byte[] value, int start, int length)
        {
            ThrowIfDisposed();
            m_stream.Write(value, start, length);
        }

        public void EncodeString(string value)
        {
            ThrowIfDisposed();

            if (value is null || value.Length == 0)
            {
                Skip(2);
                return;
            }

            Append(value.Length, 2);

            foreach (char c in value)
                Append(c, 1);
        }

        public void Skip(int count)
        {
            ThrowIfDisposed();
            var value = new byte[count];
            m_stream.Write(value, 0, value.Length);
        }
        
        public byte[] GetBuffer()
        {
            ThrowIfDisposed();
            return m_stream.GetBuffer();
        }

        public byte[] ToArray()
        {
            ThrowIfDisposed();
            return m_stream.ToArray();
        }

        private void ThrowIfDisposed()
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            m_disposed = true;
            m_stream?.Dispose();
            m_stream = null;
        }
    }
}
