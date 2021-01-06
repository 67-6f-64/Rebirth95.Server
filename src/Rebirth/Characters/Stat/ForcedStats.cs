using Rebirth.Common.Types;
using Rebirth.Network;

namespace Rebirth.Characters.Stat
{
    public class ForcedStats
    {
        public ForcedStatFlags Flag { get; private set; }

        private short m_nSTR;
        private short m_nDEX;
        private short m_nINT;
        private short m_nLUK;
        private short m_nPAD;
        private short m_nPDD;
        private short m_nMAD;
        private short m_nMDD;
        private short m_nACC;
        private short m_nEVA;
        private byte m_nSpeed;
        private byte m_nJump;
        private byte m_nSpeedMax;

        public short STR
        {
            get => m_nSTR;
            set
            {
                Flag |= ForcedStatFlags.STR;
                m_nSTR = value;
            }
        }
        public short DEX
        {
            get => m_nDEX;
            set
            {
                Flag |= ForcedStatFlags.DEX;
                m_nDEX = value;
            }
        }
        public short INT
        {
            get => m_nINT;
            set
            {
                Flag |= ForcedStatFlags.INT;
                m_nINT = value;
            }
        }
        public short LUK
        {
            get => m_nLUK;
            set
            {
                Flag |= ForcedStatFlags.LUK;
                m_nLUK = value;
            }
        }

        public short PAD
        {
            get => m_nPAD;
            set
            {
                Flag |= ForcedStatFlags.PAD;
                m_nPAD = value;
            }
        }
        public short PDD
        {
            get => m_nPDD;
            set
            {
                Flag |= ForcedStatFlags.PDD;
                m_nPDD = value;
            }
        }

        public short MAD
        {
            get => m_nMAD;
            set
            {
                Flag |= ForcedStatFlags.MAD;
                m_nMAD = value;
            }
        }
        public short MDD
        {
            get => m_nMDD;
            set
            {
                Flag |= ForcedStatFlags.MDD;
                m_nMDD = value;
            }
        }

        public short ACC
        {
            get => m_nACC;
            set
            {
                Flag |= ForcedStatFlags.ACC;
                m_nACC = value;
            }
        }
        public short EVA
        {
            get => m_nEVA;
            set
            {
                Flag |= ForcedStatFlags.EVA;
                m_nEVA = value;
            }
        }

        public byte Speed
        {
            get => m_nSpeed;
            set
            {
                Flag |= ForcedStatFlags.SPEED;
                m_nSpeed = value;
            }
        }
        public byte Jump
        {
            get => m_nJump;
            set
            {
                Flag |= ForcedStatFlags.JUMP;
                m_nJump = value;
            }
        }

        public byte SpeedMax
        {
            get => m_nSpeedMax;
            set
            {
                Flag |= ForcedStatFlags.SPEEDMAX;
                m_nSpeedMax = value;
            }
        }

        public void Encode(COutPacket packet)
        {
            packet.Encode4((int)Flag);
            if ((Flag & ForcedStatFlags.STR) != 0) packet.Encode2(m_nSTR);
            if ((Flag & ForcedStatFlags.DEX) != 0) packet.Encode2(m_nDEX);
            if ((Flag & ForcedStatFlags.INT) != 0) packet.Encode2(m_nINT);
            if ((Flag & ForcedStatFlags.LUK) != 0) packet.Encode2(m_nLUK);
            if ((Flag & ForcedStatFlags.PAD) != 0) packet.Encode2(m_nPAD);
            if ((Flag & ForcedStatFlags.PDD) != 0) packet.Encode2(m_nPDD);
            if ((Flag & ForcedStatFlags.MAD) != 0) packet.Encode2(m_nMAD);
            if ((Flag & ForcedStatFlags.MDD) != 0) packet.Encode2(m_nMDD);
            if ((Flag & ForcedStatFlags.EVA) != 0) packet.Encode2(m_nEVA);
            if ((Flag & ForcedStatFlags.ACC) != 0) packet.Encode2(m_nACC);
            if ((Flag & ForcedStatFlags.SPEED) != 0) packet.Encode1(m_nSpeed);
            if ((Flag & ForcedStatFlags.JUMP) != 0) packet.Encode1(m_nJump);
            if ((Flag & ForcedStatFlags.SPEEDMAX) != 0) packet.Encode1(m_nSpeedMax);
        }
    }
}
