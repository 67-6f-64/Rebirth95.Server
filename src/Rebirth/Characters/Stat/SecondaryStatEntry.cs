using Rebirth.Network;
using System.Linq;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Stat
{
    public class SecondaryStatEntry
    {
        public static readonly SecondaryStatFlag[] aTwoStates = new SecondaryStatFlag[]
        {
            SecondaryStatFlag.EnergyCharged,
            SecondaryStatFlag.Dash_Speed,
            SecondaryStatFlag.Dash_Jump,
            SecondaryStatFlag.RideVehicle,
            SecondaryStatFlag.PartyBooster,
            SecondaryStatFlag.GuidedBullet,
            SecondaryStatFlag.Undead
        };

        public int nValue { get; set; } //Quantity (Short) ( Value of stat usually taken from wz)
        public int rValue { get; set; } //Reason ( Skill Id Typically, but confirm later )
        public int tValue { get; set; } //Time ( ms )

        public virtual void EncodeLocal(COutPacket p)
        {
            p.Encode2((short)nValue);
            p.Encode4(rValue);
            p.Encode4(tValue);
        }

        public static SecondaryStatEntry Create(SecondaryStatFlag type)
        {
            if (aTwoStates.Contains(type))
            {
                switch (type)
                {
                    case SecondaryStatFlag.PartyBooster:
                        return new TwoStateSecondaryStatEntry_PartyBooster();
                    case SecondaryStatFlag.GuidedBullet:
                        return new TwoStateSecondaryStatEntry_GuidedBullet();
                    case SecondaryStatFlag.RideVehicle:
                        return new TwoStateSecondaryStatEntry(false);
                    case SecondaryStatFlag.Undead:
                    case SecondaryStatFlag.Dash_Jump:
                    case SecondaryStatFlag.Dash_Speed:
                    case SecondaryStatFlag.EnergyCharged:
                    default:
                        return new TwoStateSecondaryStatEntry(true);
                }
            }
            else
            {
                return new SecondaryStatEntry();
            }
        }
    }
}
