using Rebirth.Network;
using System;

namespace Rebirth.Characters.Stat
{
    public class TwoStateSecondaryStatEntry : SecondaryStatEntry //TemporaryStatBase<long>
    {
        //int m_value;                  // using nOption
        //int m_reason;                 // using rOption
        //public int m_tLastUpdated;    // using tValue
        public short m_usExpireTerm;
        public readonly bool m_bDynamicTermSet;

        public TwoStateSecondaryStatEntry(bool bDynamicTermSet)
        {
            m_bDynamicTermSet = bDynamicTermSet;
        }

        public override void EncodeLocal(COutPacket p)
        {
            p.Encode4(nValue);
            p.Encode4(rValue);
            EncodeTime(p, tValue);

            if (m_bDynamicTermSet)
            {
                p.Encode2(m_usExpireTerm);
            }
        }

        protected void EncodeTime(COutPacket p, long tTime)
        {
            var tCur = DateTime.Now.Ticks;

            p.Encode1(tTime < tCur);
            p.Encode4((int)(tTime >= tCur ? tTime - tCur : tCur - tTime));
        }
    }
}
