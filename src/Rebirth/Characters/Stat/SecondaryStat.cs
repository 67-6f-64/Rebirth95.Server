using Rebirth.Network;
using System;
using System.Collections.Generic;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;

namespace Rebirth.Characters.Stat
{
	public class SecondaryStat : Dictionary<SecondaryStatFlag, SecondaryStatEntry>
	{
		public byte tSwallowBuffTime { get; set; }
		public int[] aDiceInfo { get; set; } = new int[22];
		public int nDiceStat { get; set; }
		public int nBlessingArmorIncPAD { get; set; }
		public byte nDefenseAtt { get; set; }
		public byte nDefenseState { get; set; }
		public short tDelay { get; set; }
		public byte bSN { get; set; } // movement affecting stat

		public bool IsRemote { get; private set; } // for internal tracking
		public bool HasMovemementAffectingStat { get; private set; }
		public bool HasSwallowBuff { get; private set; }

		/// <summary>
		/// Set to true when a buff is being removed and overwritten by a new buff
		///		and used to determine if notification packet needs to be sent and
		///		validate stat needs to be called.
		/// If this is true, neither of the above actions will occur when buff is
		///		removed from CharacterBuffs.
		/// </summary>
		public bool IsRemovedFromAddingNewStat { get; set; }

		public void EncodeForLocal(COutPacket packet)
		{
			EncodeMask(packet);

			var dictionary = this;

			dictionary.GetValueOrDefault(SecondaryStatFlag.PAD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.PDD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MAD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MDD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ACC)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EVA)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Craft)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Speed)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Jump)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EMHP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EMMP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EPAD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EPDD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EMDD)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MagicGuard)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DarkSight)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Booster)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.PowerGuard)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Guard)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SafetyDamage)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SafetyAbsorb)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MaxHP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MaxMP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Invincible)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SoulArrow)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Stun)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Poison)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Seal)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Darkness)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ComboCounter)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.WeaponCharge)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DragonBlood)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.HolySymbol)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MesoUp)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ShadowPartner)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.PickPocket)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MesoGuard)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Thaw)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Weakness)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Curse)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Slow)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Morph)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Ghost)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Regen)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.BasicStatUp)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Stance)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SharpEyes)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ManaReflection)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Attract)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SpiritJavelin)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Infinity)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Holyshield)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.HamString)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Blind)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Concentration)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.BanMap)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MaxLevelBuff)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Barrier)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DojangShield)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ReverseInput)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MesoUpByItem)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ItemUpByItem)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.RespectPImmune)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.RespectMImmune)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DefenseAtt)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DefenseState)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DojangBerserk)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DojangInvincible)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Spark)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SoulMasterFinal)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.WindBreakerFinal)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ElementalReset)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.WindWalk)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EventRate)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ComboAbilityBuff)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ComboDrain)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ComboBarrier)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.BodyPressure)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SmartKnockback)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.RepeatEffect)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ExpBuffRate)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.IncEffectHPPotion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.IncEffectMPPotion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.StopPortion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.StopMotion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Fear)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.EvanSlow)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MagicShield)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MagicResistance)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SoulStone)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Flying)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Frozen)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.AssistCharge)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Enrage)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SuddenDeath)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.NotDamaged)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.FinalCut)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.ThornsEffect)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SwallowAttackDamage)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MorewildDamageUp)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Mine)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Cyclone)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SwallowCritical)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SwallowMaxMP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SwallowDefence)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SwallowEvasion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Conversion)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Revive)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Sneak)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Mechanic)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Aura)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DarkAura)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.BlueAura)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.YellowAura)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SuperBody)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.MorewildMaxHP)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Dice)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.BlessingArmor)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.DamR)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.TeleportMasteryOn)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.CombatOrders)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.Beholder)?.EncodeLocal(packet);
			dictionary.GetValueOrDefault(SecondaryStatFlag.SummonBomb)?.EncodeLocal(packet);

			packet.Encode1(nDefenseAtt);
			packet.Encode1(nDefenseState);

			HasSwallowBuff =
				ContainsKey(SecondaryStatFlag.SwallowAttackDamage)
			|| ContainsKey(SecondaryStatFlag.SwallowDefence)
			|| ContainsKey(SecondaryStatFlag.SwallowCritical)
			|| ContainsKey(SecondaryStatFlag.SwallowMaxMP)
			|| ContainsKey(SecondaryStatFlag.SwallowEvasion);

			if (HasSwallowBuff)
				packet.Encode1(tSwallowBuffTime);

			if (dictionary.ContainsKey(SecondaryStatFlag.Dice)) // todo add the actual buff values for each roll lmao
			{
				foreach (var num in aDiceInfo)
				{
					packet.Encode4(num);
				}

				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 3));
				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 3));
				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 4));
				//packet.Skip(20);
				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 2));
				//packet.Skip(12);
				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 5));
				//packet.Skip(16);
				//packet.Encode4(SkillLogic.getDiceStat(nDiceStat, 6));
				//packet.Skip(16);
			}

			if (dictionary.ContainsKey(SecondaryStatFlag.BlessingArmor))
				packet.Encode4(nBlessingArmorIncPAD);

			EncodeTwoStates(packet);

			packet.Encode2(tDelay);

			HasMovemementAffectingStat =
				ContainsKey(SecondaryStatFlag.Speed)
			|| ContainsKey(SecondaryStatFlag.Jump)
			|| ContainsKey(SecondaryStatFlag.Stun)
			|| ContainsKey(SecondaryStatFlag.Weakness)
			|| ContainsKey(SecondaryStatFlag.Slow)
			|| ContainsKey(SecondaryStatFlag.Morph)
			|| ContainsKey(SecondaryStatFlag.Ghost)
			|| ContainsKey(SecondaryStatFlag.BasicStatUp)
			|| ContainsKey(SecondaryStatFlag.Attract)
			|| ContainsKey(SecondaryStatFlag.RideVehicle)
			|| ContainsKey(SecondaryStatFlag.Dash_Jump)
			|| ContainsKey(SecondaryStatFlag.Dash_Speed)
			|| ContainsKey(SecondaryStatFlag.Flying)
			|| ContainsKey(SecondaryStatFlag.Frozen)
			|| ContainsKey(SecondaryStatFlag.YellowAura);

			if (HasMovemementAffectingStat)
				packet.Encode1(bSN);
		}

		public void BroadcastSet(Character parent)
		{
			using (var p = new COutPacket(SendOps.LP_TemporaryStatSet))
			{
				EncodeForLocal(p);

				parent.SendPacket(p);
			}

			{
				var p = new COutPacket(SendOps.LP_UserTemporaryStatSet);
				p.Encode4(parent.dwId);
				EncodeForRemote(p);
				p.Encode2(tDelay);

				if (!IsRemote) return; // is set in the encodeforremote func

				parent.Field.Broadcast(p, parent);
			}
		}

		public void BroadcastReset(Character parent)
		{
			if (Count <= 0) return;

			using (var p = new COutPacket(SendOps.LP_TemporaryStatReset))
			{
				EncodeMask(p);

				if (HasMovemementAffectingStat) p.Encode1(tSwallowBuffTime);
				if (HasSwallowBuff) p.Encode1(bSN);

				parent.SendPacket(p);
			}

			if (!IsRemote) return;

			{
				var p = new COutPacket(SendOps.LP_UserTemporaryStatReset);
				p.Encode4(parent.dwId);
				EncodeMask(p);

				parent.Field.Broadcast(p, parent);
			}
		}

		public void EncodeForRemote(COutPacket packet)
		{
			EncodeMask(packet);

			EncodeIf(SecondaryStatFlag.Speed, s => packet.Encode1((byte)s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.ComboCounter, s => packet.Encode1((byte)s.nValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.WeaponCharge, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Stun, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Darkness, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Seal, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Weakness, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Curse, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Poison, s =>  //Confirmed
			{
				packet.Encode2((short)s.nValue);
				packet.Encode4(s.rValue);
			});

			EncodeIf(SecondaryStatFlag.ShadowPartner, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Morph, s => packet.Encode2((short)s.nValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Ghost, s => packet.Encode2((short)s.nValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Attract, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.SpiritJavelin, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.BanMap, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.Barrier, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.DojangShield, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.ReverseInput, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.RespectPImmune, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.RespectMImmune, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.DefenseAtt, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.DefenseState, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.RepeatEffect, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.StopPortion, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.StopMotion, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.Fear, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.MagicShield, s => packet.Encode4(s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.Frozen, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.SuddenDeath, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.FinalCut, s => packet.Encode4(s.rValue));  //Confirmed
			EncodeIf(SecondaryStatFlag.Cyclone, s => packet.Encode1((byte)s.nValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.Mechanic, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.DarkAura, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.BlueAura, s => packet.Encode4(s.rValue)); //Confirmed
			EncodeIf(SecondaryStatFlag.YellowAura, s => packet.Encode4(s.rValue)); //Confirmed

			packet.Encode1(nDefenseAtt);
			packet.Encode1(nDefenseState);

			EncodeTwoStates(packet);
		}

		private void EncodeTwoStates(COutPacket packet)
		{
			foreach (var type in SecondaryStatEntry.aTwoStates)
			{
				EncodeIf(type, ctx => ctx.EncodeLocal(packet));
			}
		}

		private void EncodeIf(SecondaryStatFlag type, Action<SecondaryStatEntry> action)
		{
			if (TryGetValue(type, out var entry))
			{
				action(entry);
				IsRemote = true;
			}
		}

		public void EncodeMask(COutPacket packet)
		{
			var mask = new int[4];

			Keys.ForEach(s => //m_entries.ForEach(s =>
			{
				var type = s;

				var flag = 1 << (int)type % 32;
				var index = (int)type / 32;
				mask[index] |= flag;
			});

			for (var i = 4; i > 0; i--)
				packet.Encode4(mask[i - 1]);
		}
	}
}
