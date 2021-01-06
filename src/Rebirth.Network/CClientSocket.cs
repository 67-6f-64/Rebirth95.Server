using System;
using System.Net.Sockets;
using System.Text;
using Rebirth.Network.Crypto;

namespace Rebirth.Network
{
    public class CClientSocket : CSocketBase
    {
        private static readonly Random IVRand = new Random();

        private ushort m_version;
        private MapleIV m_siv;
        private MapleIV m_riv;

        public CClientSocket(Socket socket) : base(socket) { }

        //Eventually move this out of socket
        public void Initialize(ushort version, string subversion, byte locale)
        {
	        m_version = version;

	        m_siv = new MapleIV((uint)IVRand.Next());
	        m_riv = new MapleIV((uint)IVRand.Next());

	        using (var p = new COutPacket())
	        {
		        p.Encode2(0x0E);
		        p.Encode2((short)version);
		        p.EncodeString(subversion);
		        p.Encode4((int)m_riv.Value);
		        p.Encode4((int)m_siv.Value);
		        p.Encode1(locale);

		        var buffer = p.ToArray();
		        SendSync(buffer, 0, buffer.Length);
	        }

	        Receive();
        }

        protected override void ManipulateBuffer()
        {
            while (m_offset >= 4)
            {
                int size = MapleAes.GetLength(ref m_buffer);

                if (size <= 0)
                {
                    Dispose();
                    return;
                }

                //TODO: Check Header

                if (m_offset < size + 4)
                {
                    break;
                }

                var span = new Span<byte>(m_buffer, 4, size);
                MapleAes.Transform(ref span, ref m_riv);
                Shanda.DecryptTransform(ref span);

                var payload = new byte[size];
                Buffer.BlockCopy(m_buffer, 4, payload, 0, size);

                m_offset -= size + 4;

                if (m_offset > 0)
                {
                    Buffer.BlockCopy(m_buffer, size + 4, m_buffer, 0, m_offset);
                }

                InvokeOnPacket(new CInPacket(payload, 0, size)); //OnPacket?.Invoke(new CInPacket(m_buffer, 4, size));
            }
        }

        public override void SendPacket(COutPacket outPacket)
        {
            if (Disposed)
                return;

            lock (m_sendSync)    //TODO: Phase this lock out one day :^)
            {
                if (Disposed)
                    return;

                var packetLen = outPacket.Length;
                var packet = outPacket.ToArray(); //TODO: phase out this allocation

                var finalLen = packet.Length + 4;
                var final = BufferPool.Rent(finalLen);

                MapleAes.GetHeader(ref final, ref m_siv, packetLen, m_version);
                Buffer.BlockCopy(packet, 0, final, 4, packetLen);

                var span = new Span<byte>(final, 4, packetLen);
                Shanda.EncryptTransform(ref span);
                MapleAes.Transform(ref span, ref m_siv);

                SendAsync(final, 0, finalLen, final);
            }
        }
    }
}
