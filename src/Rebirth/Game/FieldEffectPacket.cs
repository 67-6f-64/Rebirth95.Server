using Rebirth.Characters;
using Rebirth.Field;
using Rebirth.Network;
using System;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Game
{
    public static class FieldEffectTypes
    {
        public enum FieldEffect
        {
            Summon = 0,
            Tremble = 1,
            Object = 2,
            Screen = 3,
            Sound = 4,
            MobHPTag = 5,
            ChangeBGM = 6,
            RewordRullet = 7
        }

        public enum DojoEffect
        {
            Clear,
            TimeOver,
        }

        public enum CakeEvent
        {
            CakeWin,
            PieWin,
            Start,
            TimeOver
        }
    }

    public class FieldEffectPacket
    {
        private readonly FieldEffect effectType;

        public string sEffect { get; set; }
        public short ptX { get; set; }
        public short ptY { get; set; }

        public byte sUOL { get; set; } // idk

        public bool bHeavyNShortTremble { get; set; }
        public int tDelay { get; set; }

        public string sName { get; set; }

        public int dwMobTemplateID { get; set; }
        public int nHP { get; set; }
        public int nMaxHP { get; set; }
        public byte nColor { get; set; }
        public byte nBgColor { get; set; }

        public int nRewardJobIdx { get; set; }
        public int nRewardPartIdx { get; set; }
        public int nRewardLevIdx { get; set; }

        public FieldEffectPacket(FieldEffect type)
        {
            effectType = type;
        }

        /// <summary>
        /// Send effect packet.
        /// </summary>
        /// <param name="c">Character</param>
        /// <param name="bLocal">True if only sent to c, otherwise its sent to entire field</param>
        public void Broadcast(Character c, bool bLocal = false)
        {
            if (bLocal)
            {
                c.SendPacket(GenerateForBroadcast());
            }
            else
            {
                c.Field.Broadcast(GenerateForBroadcast());
            }
        }

		public void Broadcast(CField f)
		{
			f.Broadcast(GenerateForBroadcast());
		}

		public static void BroadcastCakeVsPieEffect(CField field, CakeEvent nEffectType)
        {
            string sName;

            switch (nEffectType)
            {
                case CakeEvent.CakeWin:
                    sName = "event/5th/cakewin";
                    break;
                case CakeEvent.PieWin:
                    sName = "event/5th/piewin";
                    break;
                case CakeEvent.Start:
                    sName = "event/5th/start";
                    break;
                case CakeEvent.TimeOver:
                    sName = "event/5th/timeover";
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled cake vs pie map effect ({nameof(nEffectType)}).");
            }

            field.Broadcast(
                new FieldEffectPacket(FieldEffect.Screen)
                { sName = sName }
                .GenerateForBroadcast());
        }

        public static void BroadcastDojoEffect(CField field, DojoEffect nEffectType)
        {
            string sName;

            switch (nEffectType)
            {
                case DojoEffect.Clear:
                    sName = "dojang/end/clear";
                    break;
                case DojoEffect.TimeOver:
                    sName = "dojang/timeOver";
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled dojo map effect ({nameof(nEffectType)}).");
            }

            field.Broadcast(
                new FieldEffectPacket(FieldEffect.Screen)
                { sName = sName }
                .GenerateForBroadcast());
        }

        public COutPacket GenerateForBroadcast()
        {
            var p = new COutPacket(SendOps.LP_FieldEffect);
            p.Encode1((byte)effectType);
            Encode(p);
            return p;
        }

        private void Encode(COutPacket p)
        {
            switch (effectType)
            {
                case FieldEffect.Summon: // CAnimationDisplayer::Effect_General
                    p.Encode1(sUOL);//v3 = CInPacket::Decode1(iPacket); // sEffect
                    p.Encode4(ptX);//v4 = CInPacket::Decode4(iPacket); // ptX
                    p.Encode4(ptY);//v5 = CInPacket::Decode4(iPacket); // ptY
                    break;
                case FieldEffect.Tremble: // CAnimationDisplayer::Effect_Tremble
                    p.Encode1(bHeavyNShortTremble);//v21 = CInPacket::Decode1(iPacket); // bHeavyNShortTremble
                    p.Encode4(tDelay);//v22 = CInPacket::Decode4(iPacket); // tDelay
                    break;
                case FieldEffect.Object: // CMapLoadable::SetObjectState
                case FieldEffect.Screen: // CField::ShowScreenEffect
                case FieldEffect.Sound: // play_field_sound
                    p.EncodeString(sName); // v10 = CInPacket::DecodeStr(iPacket, &v32)->_m_pStr; // sName
                    break;
                case FieldEffect.MobHPTag: // CField::ShowMobHPTag(CField *this, unsigned int dwMobID, int nColor, int nBgColor, int nHP, int nMaxHP)
                    p.Encode4(dwMobTemplateID);
                    p.Encode4(nHP);
                    p.Encode4(nMaxHP);
                    p.Encode1(nColor);
                    p.Encode1(nBgColor);
                    //if (v12 - 8810118 <= 4)
                    //{
                    //    nMaxHP = 0;
                    //    v14 = 8810118;
                    //    do
                    //    {
                    //        v15 = CMobTemplate::GetMobTemplate(v14);
                    //        nMaxHP += _ZtlSecureFuse<long>(v15->_ZtlSecureTear_nMaxHP, v15->_ZtlSecureTear_nMaxHP_CS) / 1000;
                    //        ++v14;
                    //    }
                    //    while (v14 <= 8810122);
                    //    v16 = 8810122;
                    //    v13 /= 1000;
                    //    if (v12 < 0x866E8A)
                    //    {
                    //        do
                    //        {
                    //            v17 = CMobTemplate::GetMobTemplate(v16--);
                    //            v13 += _ZtlSecureFuse<long>(v17->_ZtlSecureTear_nMaxHP, v17->_ZtlSecureTear_nMaxHP_CS) / 1000;
                    //        }
                    //        while (v16 > v12);
                    //    }
                    //}
                    //CField::ShowMobHPTag(v35, v12, nColor, sName._m_pStr, v13, nMaxHP);
                    break;
                case FieldEffect.ChangeBGM: // CSoundMan::PlayBGM
                    p.EncodeString(sName);
                    break;
                case FieldEffect.RewordRullet: // CAnimationDisplayer::Effect_RewardRullet
                    p.Encode4(nRewardJobIdx);
                    p.Encode4(nRewardPartIdx);
                    p.Encode4(nRewardLevIdx);
                    break;
            }
        }
    }
}