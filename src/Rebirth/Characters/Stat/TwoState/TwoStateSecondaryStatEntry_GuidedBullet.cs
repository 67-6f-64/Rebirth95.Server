using Rebirth.Network;

namespace Rebirth.Characters.Stat
{
    public class TwoStateSecondaryStatEntry_GuidedBullet : TwoStateSecondaryStatEntry
    {
        public int m_dwMobID;
        public TwoStateSecondaryStatEntry_GuidedBullet() : base(false) { }

        public override void EncodeLocal(COutPacket p)
        {
            base.EncodeLocal(p);
            p.Encode4(m_dwMobID);
        }
    }
}
