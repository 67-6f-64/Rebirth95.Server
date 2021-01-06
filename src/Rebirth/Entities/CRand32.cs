using System;
using System.Diagnostics;
using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Entities
{
	// https://github.com/mechaviv/Krypton/blob/master/src/main/java/util/Rand32.java

	public sealed class CRand32
	{
		public uint m_s1 { get; private set; }
		public uint m_s2 { get; private set; }
		public uint m_s3 { get; private set; }

		private uint m_past_s1, m_past_s2, m_past_s3;

		private readonly object m_lock;

		public CRand32()
		{
			m_lock = new object();
		}

		public CRand32(bool bGenerateSeeds)
		{
			var time = (uint)(nanoTime() / 1000000);
			var randNum = 214013 * (214013 * (214013 * time + 2531011) + 2531011) + 2531011;

			m_lock = new object();

			lock (m_lock)
			{
				m_s1 = randNum | 0x100000;
				m_past_s1 = m_s1;
				m_s2 = randNum | 0x1000;
				m_past_s2 = m_s2;
				m_s3 = randNum | 0x10;
				m_past_s3 = m_s3;
			}
		}

		public void Skip(int nLen = 7)
		{
			for (var i = 0; i < nLen; i++)
			{
				Random();
			}
		}

		public void SetSeed(uint s1, uint s2, uint s3)
		{
			lock (m_lock)
			{
				m_s1 = s1;
				m_past_s1 = m_s1;
				m_s2 = s2;
				m_past_s2 = m_s2;
				m_s3 = s3;
				m_past_s3 = m_s3;
			}
		}

		public void Seed(uint s1, uint s2, uint s3)
		{
			lock (m_lock)
			{
				m_s1 = s1 | 0x100000;
				m_past_s1 = m_s1;
				m_s2 = s2 | 0x1000;
				m_past_s2 = m_s2;
				m_s3 = s3 | 0x10;
				m_past_s3 = m_s3;
			}
		}

		private static long nanoTime()
		{
			long nano = 10000L * Stopwatch.GetTimestamp();
			nano /= TimeSpan.TicksPerMillisecond;
			nano *= 100L;
			return nano;
		}

		public uint Random()
		{
			lock (m_lock)
			{
				//m_past_s1 = m_s1;
				//m_past_s2 = m_s2;
				//m_past_s3 = m_s3;

				//m_s1 = (m_s1 << 12) ^ (m_s1 >> 19) ^ ((m_s1 >> 6) ^ (m_s1 << 12)) & 0x1FFF;
				//m_s2 = 16 * m_s2 ^ (m_s2 >> 25) ^ ((16 * m_s2) ^ (m_s2 >> 23)) & 0x7F;
				//m_s3 = (m_s3 >> 11) ^ (m_s3 << 17) ^ ((m_s3 >> 8) ^ (m_s3 << 17)) & 0x1FFFFF;

				//return m_s1 ^ m_s2 ^ m_s3;

				var v2 = m_s1;
				var v3 = m_s2;
				var v4 = m_s3;
				var v5 = m_s1;
				var v6 = m_s1 >> 6;
				m_past_s1 = m_s1;
				var v7 = (v2 << 12) ^ (v2 >> 19) ^ (v6 ^ (v5 << 12)) & 0x1FFF;
				m_past_s2 = v3;
				var v8 = 16 * v3 ^ (v3 >> 25) ^ ((16 * v3) ^ (v3 >> 23)) & 0x7F;
				m_past_s3 = v4;
				var v9 = (v4 >> 11) ^ (v4 << 17) ^ ((v4 >> 8) ^ (v4 << 17)) & 0x1FFFFF;
				m_s3 = v9;
				m_s1 = v7;
				var result = v7 ^ v8 ^ v9;
				m_s2 = v8;
				return result;
			}
		}

		public int GetPastRandom()
		{
			lock (m_lock)
			{
				return (int)(16 * (((m_past_s3 & 0xFFFFFFF0) << 13) ^ (m_past_s2 ^ ((m_past_s1 & 0xFFFFFFFE) << 8)) & 0xFFFFFFF8)
							 ^ (
								 (m_past_s1 & 0x7FFC0
								  ^ (
									  (m_past_s3 & 0x1FFFFF00
									   ^ (
										   (m_past_s3
											^ (
												(m_past_s1
												 ^ (((m_past_s2 >> 2) ^ m_past_s2 & 0x3F800000)
													>> 4))
												>> 8))
										   >> 3))
									  >> 2))
								 >> 6)
							 );
			}
		}

		public void Encode(COutPacket p)
		{
			p.Encode4(m_s1);
			p.Encode4(m_s2);
			p.Encode4(m_s3);
		}
	}
}