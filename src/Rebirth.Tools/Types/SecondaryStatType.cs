namespace Rebirth.Common.Types
{
    public enum SecondaryStatFlag
    {
        PAD = 0x0,
        PDD = 0x1,
        MAD = 0x2,
        MDD = 0x3,
        ACC = 0x4,
        EVA = 0x5,
        Craft = 0x6,
        Speed = 0x7,
        Jump = 0x8,
        MagicGuard = 0x9, // Done
        DarkSight = 0xA, // Done
        Booster = 0xB, // Done
        PowerGuard = 0xC, // Done
        MaxHP = 0xD, // hyperbodyhp
        MaxMP = 0xE, // hyperbodymp
        Invincible = 0xF, // Done
        SoulArrow = 0x10,  // Done
        Stun = 0x11,
        Poison = 0x12,
        Seal = 0x13,
        Darkness = 0x14,
        ComboCounter = 0x15, // combo
        WeaponCharge = 0x16, // WhiteKnight Charge
        DragonBlood = 0x17, // Done
        HolySymbol = 0x18, // Done
        MesoUp = 0x19, // Done
        ShadowPartner = 0x1A, // Done
        PickPocket = 0x1B, // Done
        MesoGuard = 0x1C, // Done
        Thaw = 0x1D,
        Weakness = 0x1E,
        Curse = 0x1F,
        Slow = 0x20, // Done
        Morph = 0x21,
        Regen = 0x22, // recovery
        BasicStatUp = 0x23, // maple warrior
        Stance = 0x24, // Done
        SharpEyes = 0x25, // Done
        ManaReflection = 0x26, // Done
        Attract = 0x27,  // seduce
        SpiritJavelin = 0x28, // shadow claw
        Infinity = 0x29, // Done
        Holyshield = 0x2A, // Done
        HamString = 0x2B, // Done
        Blind = 0x2C, // Done
        Concentration = 0x2D, // Done
        BanMap = 0x2E,
        MaxLevelBuff = 0x2F, // echo of hero
        MesoUpByItem = 0x30, // Done
        Ghost = 0x31, // ghost morph
        Barrier = 0x32,
        ReverseInput = 0x33, // confuse
        ItemUpByItem = 0x34, // Done
        RespectPImmune = 0x35,
        RespectMImmune = 0x36,
        DefenseAtt = 0x37,
        DefenseState = 0x38,
        IncEffectHPPotion = 0x39,
        IncEffectMPPotion = 0x3A,
        DojangBerserk = 0x3B, // berserk fury
        DojangInvincible = 0x3C, // divine body
        Spark = 0x3D, // Done
        DojangShield = 0x3E,
        SoulMasterFinal = 0x3F, // Done ?
        WindBreakerFinal = 0x40, // Done ?
        ElementalReset = 0x41, // Done
        WindWalk = 0x42, // Done
        EventRate = 0x43,
        ComboAbilityBuff = 0x44, // aran combo
        ComboDrain = 0x45, // Done
        ComboBarrier = 0x46, // Done
        BodyPressure = 0x47, // Done
        SmartKnockback = 0x48, // Done
        RepeatEffect = 0x49,
        ExpBuffRate = 0x4A, // Done
        StopPortion = 0x4B,
        StopMotion = 0x4C,
        Fear = 0x4D, // debuff done
        EvanSlow = 0x4E, // Done
        MagicShield = 0x4F, // Done
        MagicResistance = 0x50, // Done
        SoulStone = 0x51,
        Flying = 0x52,
        Frozen = 0x53,
        AssistCharge = 0x54,
        Enrage = 0x55, //mirror imaging
        SuddenDeath = 0x56,
        NotDamaged = 0x57,
        FinalCut = 0x58,
        ThornsEffect = 0x59,
        SwallowAttackDamage = 0x5A,
        MorewildDamageUp = 0x5B,
        Mine = 0x5C,
        EMHP = 0x5D,
        EMMP = 0x5E,
        EPAD = 0x5F,
        EPDD = 0x60,
        EMDD = 0x61,
        Guard = 0x62,

        SafetyDamage = 0x63,
        SafetyAbsorb = 0x64,

        Cyclone = 0x65,

        SwallowCritical = 0x66,
        SwallowMaxMP = 0x67,
        SwallowDefence = 0x68,
        SwallowEvasion = 0x69,

        Conversion = 0x6A,
        Revive = 0x6B, // summon reaper buff
        Sneak = 0x6C,
        Mechanic = 0x6D,
        Aura = 0x6E,
        DarkAura = 0x6F,
        BlueAura = 0x70,
        YellowAura = 0x71,
        SuperBody = 0x72, // body boost
        MorewildMaxHP = 0x73,
        Dice = 0x74,
        BlessingArmor = 0x75, // Paladin Divine Shield
        DamR = 0x76,
        TeleportMasteryOn = 0x77,
        CombatOrders = 0x78,
        Beholder = 0x79,

        // twostates
        EnergyCharged = 0x7A,
        Dash_Speed = 0x7B,
        Dash_Jump = 0x7C,
        RideVehicle = 0x7D,
        PartyBooster = 0x7E,
        GuidedBullet = 0x7F,
        Undead = 0x80,

        SummonBomb = 0x81,

        Dash = Dash_Speed | Dash_Jump,

        MoreWild = MorewildDamageUp | MorewildMaxHP,

        HyperBody = MaxHP | MaxMP,

        SwallowBuff = SwallowAttackDamage
                      | SwallowDefence
                      | SwallowCritical
                      | SwallowMaxMP
                      | SwallowEvasion,

        MovementAffectingStat = Speed
                                | Jump
                                | Stun
                                | Weakness
                                | Slow
                                | Morph
                                | Ghost
                                | BasicStatUp
                                | Attract
                                | RideVehicle
                                | Dash_Jump
                                | Dash_Speed
                                | Flying
                                | Frozen
                                | YellowAura,

        None_DONT_USE = 0x100 // for server-sided purposes
    }
}
