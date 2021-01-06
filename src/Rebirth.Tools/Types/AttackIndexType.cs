namespace Rebirth.Common.Types
{
    public enum AttackIndexType
    {
        AttackIndex_Mob_Physical = 0,
        AttackIndex_Mob_Magic = 0xFF, //-1,
        AttackIndex_Counter = 0xFE, //-2,
        AttackIndex_Obstacle = 0xFD, // -3,
        AttackIndex_Stat = 0xFC // -4,
    }
}
