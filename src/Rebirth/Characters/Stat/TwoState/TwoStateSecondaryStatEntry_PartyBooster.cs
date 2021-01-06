using Rebirth.Network;

namespace Rebirth.Characters.Stat
{
    public class TwoStateSecondaryStatEntry_PartyBooster : TwoStateSecondaryStatEntry
    {
        public long tCurrentTime { get; set; }
        public TwoStateSecondaryStatEntry_PartyBooster() 
			: base(false) { }

        public override void EncodeLocal(COutPacket p)
        {
            base.EncodeLocal(p);
			EncodeTime(p, tCurrentTime);
			p.Encode2(m_usExpireTerm);
        }
    }
}
