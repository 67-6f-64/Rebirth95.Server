using System;
using Rebirth.Network;

namespace Rebirth.Game
{
	public sealed class CSnowBall
	{
		private readonly int[] anDelay = new int[10]
		{
			150, 200, 250, 300, 350, 400, 450, 500, 0, int.MaxValue
		};

		public readonly int nRecoveryAmount;
		public readonly int nSnowBallMaxHP = 8999;
		public readonly int nSnowballFinishLine = 900;
		public readonly int nSnowballStartLine = -60;

		public int nXPos { get; set; }
		public int nHP { get; set; }
		public DateTime tLastSpeedChanged { get; set; }
		public DateTime tLastRecovery { get; set; }

		// BMS has no snowball constructor
		public CSnowBall(int recoveryAmount)
		{
			nRecoveryAmount = recoveryAmount * 10;
			tLastSpeedChanged = DateTime.Now;
			tLastRecovery = DateTime.Now;
			nXPos = nSnowballStartLine;
			nHP = nSnowBallMaxHP;
		}

		// CSnowBall::Update(CSnowBall *this, int tCur)
		public bool Update()
		{
			const int nSnowManWait = 10000;

			var nDelay = anDelay[nHP / 1000];
			var diff = DateTime.Now - tLastSpeedChanged;
			if (nDelay > 0)
			{
				var nNewPos = nSnowballStartLine;
				var nCurPos = (int)(nXPos + diff.TotalMilliseconds / nDelay);
				if (nSnowballStartLine < nCurPos)
				{
					nNewPos = nCurPos;
				}

				if (nNewPos > nSnowballFinishLine)
				{
					nNewPos = nSnowballFinishLine;
				}
				nXPos = nNewPos;

				var v1 = DateTime.Now - tLastSpeedChanged;
				var remainder = v1.TotalMilliseconds % nDelay;
				tLastSpeedChanged = DateTime.Now.AddMilliseconds(-remainder);
			}
			else
			{
				tLastSpeedChanged = DateTime.Now;
			}

			var millisSinceStart = tLastRecovery.MillisSinceStart();

			if (millisSinceStart <= nSnowManWait) return false;

			nHP += nRecoveryAmount;
			if (nHP > nSnowBallMaxHP) nHP = nSnowBallMaxHP;

			tLastRecovery = DateTime.Now;

			return true;
		}

		// CSnowBall::Encode(CSnowBall *this, COutPacket *oPacket)
		public void Encode(COutPacket p)
		{
			p.Encode2((short)nXPos);
			p.Encode1((byte)(nHP / 1000));
		}
	}
}