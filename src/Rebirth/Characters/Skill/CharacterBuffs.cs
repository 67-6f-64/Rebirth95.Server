using log4net;
using Rebirth.Characters.Stat;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Provider.Template.Skill;

namespace Rebirth.Characters.Skill
{
	public class CharacterBuffs : NumericKeyedCollection<AbstractBuff>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CharacterBuffs));

		public static void Handle_PassiveskillInfoUpdate(WvsGameClient c, CInPacket p) => c.Character.Buffs.Update();

		// ------------------------------------

		public BuffSkill this[Skills nSkillID]
			=> this[(int)nSkillID] as BuffSkill;

		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);
		public int dwParentID { get; set; }

		private DateTime tLastUpdate = DateTime.Now;

		public CharacterBuffs(int parent)
		{
			dwParentID = parent;

			MasterManager.BuffStorage.GetBuffsByCharId(dwParentID).ForEach(Add);
		}

		public void SaveForMigrate() => MasterManager.BuffStorage.Add(this);

		public void Update()
		{
			if (tLastUpdate.MillisSinceStart() < 1000) return;

			tLastUpdate = DateTime.Now;

			var toRemove = new List<AbstractBuff>();

			foreach (var buff in this)
			{
				if (buff.nBuffID == (int)Skills.MECHANIC_SIEGE2)
				{
					if (buff.StartTime.MillisSinceStart() < 5000) continue;

					if (Parent.Stats.nMP < 100)
					{
						toRemove.Add(buff);
					}
					else
					{
						Parent.Modify.Stats(ctx => ctx.MP -= 100);
						buff.StartTime = DateTime.Now;
					}
				}
				else
				if (buff.tDuration > 0)
				{
					switch (buff.StatType)
					{
						case SecondaryStatFlag.Infinity:
							{
								buff.State += 1;

								if (Parent.Stats.nHP <= 0) continue;

								if (buff.State % 4 == 0)
								{
									var incR = (buff as BuffSkill).Template.X(buff.nSLV) * 0.01; // same for hp and mp
									Parent.Modify.Heal((int)(Parent.BasicStats.nMHP * incR), (int)(Parent.BasicStats.nMMP * incR));
								}

								break;
							}

						case SecondaryStatFlag.Regen:
							{
								if (Parent.Stats.nHP <= 0) continue;

								Parent.Modify.Heal(buff.Stat[SecondaryStatFlag.Regen].nValue);
							}
							break;
						case SecondaryStatFlag.DragonBlood:
							{
								var hpCon = (int)(buff as BuffSkill).Template.X(buff.nSLV);
								if (Parent.Stats.nHP <= hpCon * 4)
								{
									toRemove.Add(buff);
								}
								else
								{
									Parent.Modify.Heal(-hpCon);
								}
							}
							break;
						case SecondaryStatFlag.Poison:
							{
								Parent.Modify.Heal(-buff.Stat[SecondaryStatFlag.Poison].nValue);
							}
							break;
					}

					if (buff.StartTime.MillisSinceStart() >= buff.tDuration) toRemove.Add(buff);
				}
				else if (buff.tDuration < 0)
				{
					if (buff.nSkillID == (int)Skills.MECHANIC_AR_01) // TODO consider this
					{
						if (Parent.Field?.Users[buff.dwCharFromId] is null)
						{
							toRemove.Add(buff);
						}
					}
					else
					{
						switch (buff.StatType)
						{
							case SecondaryStatFlag.BlueAura:
							case SecondaryStatFlag.DarkAura:
							case SecondaryStatFlag.YellowAura:
								if (buff.dwCharFromId != dwParentID)
								{
									if (Parent.Party is null) toRemove.Add(buff);
									else
									{
										var buffOwner = Parent.Field.Users[buff.dwCharFromId];
										if (buffOwner is null
											|| buffOwner.Party is null
											|| buffOwner.Party.PartyID != Parent.Party.PartyID)
										{
											toRemove.Add(buff);
										}
									}
								}
								break;
						}
					}
				}
			}

			toRemove.ForEach(buff => Remove(buff));
		}

		/// <summary>
		/// Removes all buffs in collection that meet the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		public void RemoveIf(Predicate<AbstractBuff> predicate)
		{
			foreach (var item in this.ToList())
			{
				if (predicate.Invoke(item)) Remove(item);
			}
		}


		public void RemoveFirst(Predicate<AbstractBuff> predicate)
		{
			AbstractBuff toRemove = null;
			foreach (var item in this)
			{
				if (predicate.Invoke(item))
				{
					toRemove = item;
					break;
				}
			}

			Remove(toRemove);
		}

		public void UserTryCancelBuff(int nSkillID)
		{
#if DEBUG
			Parent.SendMessage("Cancelling buff with ID: " + nSkillID);
#endif

			var item = this[nSkillID];
			if (item is Debuff) return; // cant cancel debuffs

			if (item is BuffSkill)
			{
				// auras -- TODO enums
				switch (nSkillID)
				{
					case 32001003:
						Remove(32120000);
						break;
					case 32101002:
						Remove(32110000);
						break;
					case 32101003:
						Remove(32120001);
						break;
					case (int)Skills.MECHANIC_SATELITE:
					case (int)Skills.MECHANIC_SATELITE2:
					case (int)Skills.MECHANIC_SATELITE3:
						Remove((int)Skills.MECHANIC_SAFETY);
						break;
				}

				if (Parent.Skills.Get(nSkillID, true) is SkillEntry skill)
				{
					if (skill.Template.IsPrepareAttackSkill)
					{
						Parent.Field.Broadcast(UserSkillCancelRemote(nSkillID), Parent);
					}
				}
			}
			else if (nSkillID == 35001002) // basic mech
			{
				Remove(35120000); // mech advanced
				return;
			}
			// else is potion

			Remove(item);
		}

		public SecondaryStat GetSecondaryStatCollection()
		{
			var retVal = new SecondaryStat();

			foreach (var item in this)
			{
				foreach (var entry in item.Stat)
				{
					if (retVal.ContainsKey(entry.Key)) continue;

					retVal.Add(entry.Key, entry.Value);
				}
			}

			return retVal;
		}

		public void CancelAllDebuffs() => RemoveIf(b => b is Debuff);

		public void CancelAllBuffs(bool exceptRateModifiers = true)
		{
			if (exceptRateModifiers)
			{
				RemoveIf(b => !b.IsRateModifier);
			}
			else
			{
				// don't want to Clear() because we have to notify client
				this.ToList().ForEach(b => Remove(b));
			}
		}

		public double PotionEffectivenessModifier()
		{
			switch (Parent.Stats.nJob)
			{
				case 411:
				case 412:
					return Parent.Skills.Get((int)Skills.HERMIT_ALCHEMIST)?.X_Effect * 0.01 ?? 1;
				case 1411: // maybe 1410??
					return Parent.Skills.Get((int)Skills.NIGHTWALKER_ALCHEMIST)?.X_Effect * 0.01 ?? 1;
			}
			return 1;
		}

		public double BuffTimeModifier()
		{
			switch (Parent.Stats.nJob)
			{
				case 212:
					return 1 + (Parent.Skills.Get((int)Skills.ARCHMAGE1_MASTER_MAGIC)?.X_Effect * 0.01 ?? 0);
				case 222:
					return 1 + (Parent.Skills.Get((int)Skills.ARCHMAGE2_MASTER_MAGIC)?.X_Effect * 0.01 ?? 0);
				case 232:
					return 1 + (Parent.Skills.Get((int)Skills.BISHOP_MASTER_MAGIC)?.X_Effect * 0.01 ?? 0);
			}
			return 1;
		}

		/// <summary>
		/// Updates and sends a TemporaryStatSet packet to parent and remote client around parent
		/// This informs the client of changes to the buff without the overhead of removing the buff and re-adding it
		/// </summary>
		/// <param name="toUpdate"></param>
		public void UpdateBuffInfo(AbstractBuff toUpdate)
		{
			toUpdate.Stat.Clear();
			toUpdate.Generate();

			// no need to remove first cuz it will overwrite the previous value
			Parent.Stats.SecondaryStats.SetFromSecondaryStat(Parent, toUpdate.Stat);

			toUpdate.Stat.BroadcastSet(Parent);

			Parent.ValidateStat();

			if (toUpdate is BuffSkill buff && buff.Template.HasAffected)
			{
				var effect = new UserEffectPacket(UserEffect.SkillAffected)
				{
					nSkillID = buff.nSkillID,
				};

				effect.BroadcastEffect(Parent);
			}
		}

		/// <summary>
		/// Use this function for buffs that have already been generated.
		/// </summary>
		/// <param name="item"></param>
		public void AddItemBuff(AbstractBuff item)
		{
			if (Contains(item.nBuffID))
			{
				UpdateBuffInfo(this[item.nBuffID]);
			}
			else
			{
				Add(item);
			}
		}

		public void AddItemBuff(int nItemID, double durationMultiplier = 1.0)
		{
			var key = -Math.Abs(nItemID);

			if (Contains(key))
			{
				UpdateBuffInfo(this[key]);
				return;
			}

			var newBuff = new BuffConsume(nItemID);
			newBuff.Generate(durationMultiplier);

			Add(newBuff);
		}

		public void OnStatChangeByMobSkill(MobSkillLevelData template, int dwMobTemplate, int tDelay)
		{
			if (!template.DoProp(Constants.Rand)) return;

			if (template.Time <= 0)
			{
				if (template.nSkillID == (int)MobSkill.MobSkillID.DISPEL)
				{
					CancelAllDebuffs();
					// Parent.ValidateStat(); // no need to validate stat here cuz it does it when removed
				}
			}
			else
			{
				var debuff = new Debuff(template.nSkillID, template.nSLV);

				switch ((MobSkill.MobSkillID)template.nSkillID) // TODO determine the rest of these
				{
					case MobSkill.MobSkillID.ATTRACT:
						debuff.StatType = SecondaryStatFlag.Attract;
						debuff.nOption = template.X;
						break;
					case MobSkill.MobSkillID.BANMAP:
						debuff.StatType = SecondaryStatFlag.BanMap;
						debuff.dwMobId = dwMobTemplate;
						break;
					case MobSkill.MobSkillID.STUN:
						debuff.StatType = SecondaryStatFlag.Stun;
						debuff.nOption = 1;
						break;
					case MobSkill.MobSkillID.SEAL:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Seal;
						debuff.nOption = 1;
						break;
					case MobSkill.MobSkillID.DARKNESS:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Darkness;
						debuff.nOption = 1;
						break;
					case MobSkill.MobSkillID.WEAKNESS:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Weakness;
						debuff.nOption = 1;
						break;
					case MobSkill.MobSkillID.CURSE:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Curse;
						debuff.nOption = 1;
						break;
					case MobSkill.MobSkillID.POISON:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Poison;
						debuff.nOption = template.X;
						break;
					case MobSkill.MobSkillID.SLOW:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.Slow;
						debuff.nOption = template.X;
						break;
					case MobSkill.MobSkillID.REVERSE_INPUT:
						//if (GetByType(SecondaryStatFlag.Holyshield) != null) return;
						if (Parent.Stats.SecondaryStats.nHolyShield != 0) return;
						debuff.StatType = SecondaryStatFlag.ReverseInput;
						debuff.nOption = template.X;
						break;
					default: throw new InvalidOperationException();
				}
				debuff.tDuration = template.Time * 1000 + tDelay;
				debuff.Generate(tDelay);

				Remove(debuff.nBuffID);

				Add(debuff);
			}
		}

		public void AddSkillBuff(int nSkillID, int nSLV, double durationMultiplier = 1.0, int nOption = 0)
		{
			if (nSkillID == (int)Skills.MECHANIC_SAFETY)
			{
				nOption = Parent.Stats.SecondaryStats.rBeholder;
			}

			if (GetOrDefault(nSkillID) is BuffSkill bs)
			{
				if (bs.nSLV < nSLV)
				{
					bs.nSLV = (byte)nSLV;
				}

				bs.State = nOption;

				UpdateBuffInfo(bs);
			}
			else
			{
				var newBuff = new BuffSkill(nSkillID, (byte)nSLV);

				if (newBuff.Template.Morph > 0)
				{
					newBuff.State = Parent.Stats.nGender; // haxed 
				}

				newBuff.Generate(durationMultiplier);

				if (newBuff.StatType != SecondaryStatFlag.None_DONT_USE) // TODO phase this out
				{
					RemoveIf(b => b.StatType == newBuff.StatType);
				}

				if (newBuff.Template.HasAffected)
				{
					var effect = new UserEffectPacket(UserEffect.SkillAffected)
					{
						nSkillID = nSkillID,
					};

					effect.BroadcastEffect(Parent);
				}

				newBuff.State = nOption;

				Add(newBuff);
			}
		}

		public void AddAura(SecondaryStatFlag nType, int dwCharIdFrom, int nAuraSkillID, int nAuraLevel)
		{
			// TODO fuse removal logic with SecondaryStatValues class
			AbstractBuff existingBuff = null;
			foreach (var buff in this)
			{
				if (buff.StatType == nType)
				{
					if (nAuraSkillID == buff.nSkillID)
					{
						if (buff.nSLV >= nAuraLevel) return;
					}
					existingBuff = buff;
					break;
				}
			}

			Remove(existingBuff);

			// client expects two packets, so we generate two but only save one (cuz cant save same key twice)
			var auraBuff = new BuffSkill(nAuraSkillID, (byte)nAuraLevel);
			auraBuff.GenerateAuraSkill(SecondaryStatFlag.Aura);
			Parent.Buffs.Add(auraBuff);

			auraBuff.Stat.Clear(); // recycling ftw
			auraBuff.dwCharFromId = dwCharIdFrom;

			auraBuff.GenerateAuraSkill(nType);
			Parent.Buffs.Add(auraBuff);
			auraBuff.Stat.Add(SecondaryStatFlag.Aura,
				new SecondaryStatEntry
				{
					nValue = nAuraSkillID,
					rValue = nAuraLevel,
					tValue = -1000,
				});
		}

		//public AbstractBuff GetByType(SecondaryStatFlag type) => this.FirstOrDefault(c => c.StatType == type);

		///// <summary>
		///// This is not foolproof, it only checks the buff StatType variable and returns the first
		/////		stat entry that is equal to nType in the Stat collection.
		///// </summary>
		///// <param name="nType"></param>
		///// <param name="nDefaultValue"></param>
		///// <returns></returns>
		//public int nOptionData(SecondaryStatFlag nType, int nDefaultValue = 0)
		//{
		//	// TODO phase this whole function out, its bad

		//	foreach (var item in this)
		//	{
		//		if (item.StatType != nType) continue;

		//		foreach (var stat in item.Stat)
		//		{
		//			if (stat.Key != nType) continue;

		//			return stat.Value.nValue;
		//		}
		//	}

		//	return nDefaultValue;
		//}

		public void DoCombatOrders(int nSkillID, byte nSLV)
		{
			if (GetOrDefault(nSkillID) is BuffSkill curCO && curCO.nSLV >= nSLV)
			{
				UpdateBuffInfo(curCO);
				return;
			}

			var entry = new BuffSkill(nSkillID, nSLV);
			entry.State = (int)entry.Template.X(nSLV);
			entry.Generate();
			Add(entry);

			// TODO just make nSLV look at combat orders SecondaryStat value instead of modifying all these

			foreach (var skill in Parent.Skills)
			{
				if (skill.nSLV > 0 && skill.nSkillID != nSkillID) // combat orders will affect hidden skills but not itself
				{
					skill.CombatOrders = entry.State;
				}
			}
		}

		protected override void ClearItems()
		{
			// TODO send clear packet, perform all actions in RemoveItem
			base.ClearItems();
		}

		protected override void InsertItem(int index, AbstractBuff item)
		{
			if (item is null || item.Stat.Count <= 0) return;

			// check to see if we should store the buff or not TODO figure out a better way to do this, nexon doesnt so we shouldnt
			if (item.BuffStoreType != BuffStoreType.FireAndForget)
			{
				base.InsertItem(index, item);
			}

			// this needs to happen before ValidateStat is called
			Parent.Stats.SecondaryStats.SetFromSecondaryStat(Parent, item.Stat);

			// parent is null when we are adding items after migrating
			if (Parent?.Initialized ?? false)
			{
				item.Stat.BroadcastSet(Parent);
				Parent.ValidateStat();
			}
		}

		protected override void RemoveItem(int index)
		{
			var item = GetAtIndex(index);

			base.RemoveItem(index);

			// parent is null if player has logged out and the buffs are being cleared
			if (item is null || Parent is null) return;

			if (item is BuffSkill bs) // not a consumable
			{
				// skipping mechanic ar_01 cuz its both a stat buff and a summon so we handle it differently
				if (bs.Template.IsSummonSkill && item.nBuffID != (int)Skills.MECHANIC_AR_01)
				{
					foreach (var summon in new List<Field.FieldObjects.CSummon>(Parent.Field.Summons))
					{
						if (summon.dwParentID == dwParentID && summon.nSkillID == item.nBuffID)
						{
							Parent.Field.Summons.Remove(summon);
							break;
						}
					}
				}

				switch ((Skills)item.nBuffID)
				{
					case Skills.ARAN_COMBO_ABILITY:
						Parent.Combat.ComboCounter = 0;
						break;
					case Skills.CRUSADER_COMBO_ATTACK:
					case Skills.SOULMASTER_COMBO_ATTACK:
						Parent.Combat.ComboCounter = 0;
						break;
					case Skills.DUAL4_OWL_DEATH:
						Parent.Combat.OwlSpiritCount = 0;
						break;
					case Skills.WILDHUNTER_SWALLOW:
						if (Parent.Field.Mobs.Contains(Parent.m_dwSwallowMobID))
							Parent.Field.Mobs[Parent.m_dwSwallowMobID].m_dwSwallowCharacterID = 0;

						Parent.m_dwSwallowMobID = 0;
						break;
					case Skills.KNIGHT_COMBAT_ORDERS:
						Parent.Skills.ForEach(skill => skill.CombatOrders = 0);
						break;
					default:
						if (item.dwCharFromId == dwParentID && Parent.Party != null)
						{
							switch (item.StatType)
							{
								case SecondaryStatFlag.BlueAura:
								case SecondaryStatFlag.DarkAura:
								case SecondaryStatFlag.YellowAura:
									{
										Parent.Field.Users
											.ForEachPartyMember(Parent.Party.PartyID,
											pm => pm.Buffs
												.RemoveFirst(pmBuff =>
													pmBuff.dwCharFromId == dwParentID
													&& pmBuff.StatType == item.StatType));
									}
									break;
							}
						}
						break;
				}
			}

			if (item.Stat.IsRemovedFromAddingNewStat) return;

			// this needs to happen before ValidateStat is called
			Parent.Stats.SecondaryStats.RemoveFromSecondaryStat(item.Stat);

			item.Stat.BroadcastReset(Parent);

			Parent.ValidateStat();
		}

		private COutPacket UserSkillCancelRemote(int nSkillID) // TODO move this to a CPacket subclass
		{
			var p = new COutPacket(SendOps.LP_UserSkillCancel);
			p.Encode4(dwParentID);
			p.Encode4(nSkillID);
			return p;
		}

		protected override int GetKeyForItem(AbstractBuff item) => item.nBuffID;
	}
}
