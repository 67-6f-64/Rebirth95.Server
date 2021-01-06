using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Rebirth.Characters;
using Rebirth.Characters.Actions;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Redis;
using Rebirth.Server.Center;
using Rebirth.Tools.Formulas;
using static Rebirth.Game.MobSkill;

namespace Rebirth.Game
{
	public sealed class PartyMember
	{
		public int dwCharId;
		public string sCharacterName;
		public int nJob;
		public int nLevel;
		public int nChannel;

		// town portal 
		public int dwTownID;
		public int dwFieldID;
		public int nSkillID;
		public int ptFieldPortalX;
		public int ptFieldPortalY;

		/// <summary>
		/// Returns true if character is in charpool but isn't in cash shop.
		/// Make sure to perform proper null checks when calling this
		/// </summary>
		public Character CharObj => MasterManager.CharacterPool.Get(dwCharId, false);

		public bool bCashShop => MasterManager.CharacterPool.CharInCashShop(dwCharId);

		public bool bOnline
		{
			get
			{
				var storage = ServerApp.Container.Resolve<CenterStorage>();
				return storage.IsCharacterOnline(dwCharId);
			}
		}
		public bool bInGame => CharObj != null;

		/// <summary>
		/// Performs a null check and map UID check
		/// </summary>
		public bool InSameMap(long fieldUid) => CharObj?.Field?.dwUniqueId == fieldUid;

		public PartyMember(int dwId)
		{
			dwCharId = dwId;
			dwTownID = Constants.InvalidMap;
			dwFieldID = Constants.InvalidMap;
			Update();
		}

		public void Update() //Character c) // -- TODO
		{
			if (CharObj is null) return;

			sCharacterName = CharObj.Stats.sCharacterName;
			nJob = CharObj.Stats.nJob;
			nLevel = CharObj.Stats.nLevel;
			nChannel = CharObj.Stats.Channel;
		}

		public void SendPacket(COutPacket p) => MasterManager.CharacterPool.Get(dwCharId)?.SendPacket(p);
	}

	public sealed class Party : KeyedCollection<int, PartyMember>
	{
		public int PartyID { get; set; }
		public int dwOwnerId { get; set; }
		public LinkedList<int> Invites { get; } = new LinkedList<int>();

		public void WarpParty(long nFromMapUID, int nToMap, bool bInstancedMap, int nPortalId = 0, int nInstanceID = 0)
		{
			if (bInstancedMap)
			{
				foreach (var item in this)
				{
					if (item.InSameMap(nFromMapUID))
					{
						item.CharObj.Action.SetFieldInstance(nToMap, nInstanceID == 0 ? PartyID : nInstanceID, (byte)nPortalId);
					}
				}
			}
			else
			{
				foreach (var item in this)
				{
					if (item.InSameMap(nFromMapUID))
					{
						item.CharObj?.Action.SetField(nToMap, (byte)nPortalId);
					}
				}
			}
		}

		public void HandlePartyChat(int dwId, string sFrom, string sText)
		{
			Broadcast(CPacket.GroupMessage(GroupMessageType.PartyChat, sFrom, sText), dwId);
		}

		public void ForEachMemberInField(long dwFieldUid, Action<Character> action)
		{
			foreach (var member in this)
			{
				if (member.InSameMap(dwFieldUid))
				{
					action.Invoke(member.CharObj);
				}
			}
		}

		/// <summary>
		/// SkillID and nSLV are assumed to have already been validated before calling this function
		/// </summary>
		public void ApplyBuffToParty(Character sender, long dwFieldUId, int nSkillID, byte nSLV, double buffTimeModifier = 1.0)
		{
			var pSkill = MasterManager.SkillTemplates[nSkillID];

			var effect = new UserEffectPacket(UserEffect.SkillAffected) { nSkillID = nSkillID };

			switch ((Skills)nSkillID)
			{
				case Skills.MECHANIC_HEALING_ROBOT_H_LX:
					{
						ForEachMemberInField(dwFieldUId, member =>
						{
							if (member.Stats.nHP > 0)
							{
								member.Modify.Heal((int)(member.BasicStats.nMHP * pSkill.HP(nSLV) * 0.01));
							}
						});
					}
					break;
				case Skills.BISHOP_RESURRECTION:
					ForEachMemberInField(dwFieldUId, member =>
					{
						if (member.Stats.nHP <= 0 && member.dwId != sender.dwId)
						{
							member.Modify.Heal(member.BasicStats.nMHP);
							effect.BroadcastEffect(member);
						}
					});
					break;
				case Skills.PRIEST_DISPEL:
					ForEachMemberInField(dwFieldUId, member =>
					{
						if (member.Stats.nHP > 0)
						{
							member.Buffs.CancelAllBuffs();
							if (member.dwId != sender.dwId)
							{
								effect.BroadcastEffect(member);
							}
						}
					});
					break;
				case Skills.KNIGHT_COMBAT_ORDERS:
					ForEachMemberInField(dwFieldUId, member =>
					{
						if (member.Stats.nHP > 0) // sorry buddy no combat orders for u
						{
							member.Buffs.DoCombatOrders(nSkillID, nSLV);
							if (member.dwId != sender.dwId)
							{
								effect.BroadcastEffect(member);
							}
						}
					});
					break;
				default:
					ForEachMemberInField(dwFieldUId, member =>
					{
						member.Buffs.AddSkillBuff(nSkillID, nSLV, Math.Max(buffTimeModifier, member.Buffs.BuffTimeModifier()));
					});
					break;
			}
		}

		public void DistributeMobExp(long dwFieldUId, Dictionary<int, int> attackInfo, int mobLevel, int mobMaxHp, int mobExp)
		{
			var partyLevel = AveragePartyLevel(dwFieldUId);

			var attackingMembers = 0;

			foreach (var item in this)
			{
				if (attackInfo.ContainsKey(item.dwCharId))
					attackingMembers += 1;

				if (attackingMembers >= 2) break;
			}

			var highestCharLevel = CalculateHighestLevel(dwFieldUId);

			foreach (var partymember in this)
			{
				if (!partymember.InSameMap(dwFieldUId)) continue;

				if (partymember.CharObj.Stats.nHP <= 0) continue;

				var nDmg = attackInfo.GetValueOrDefault(partymember.dwCharId);

				var regularGain = (int)(partymember.CharObj.BasicStats.ExpRate * 0.01 * RateConstants.ExpRate * mobExp *
					(0.2 * nDmg / mobMaxHp + (0.8 * partymember.CharObj.Stats.nLevel / highestCharLevel))
					* ExpRate.CalculateLevelDiffModifier(mobLevel, partymember.CharObj.Stats.nLevel));

				var bonusGain = (int)(1 + (regularGain
										   * ExpRate.CalculatePartyBonusMultiplier(CalculateLeechableMembers(dwFieldUId, highestCharLevel), CalculateBishops(dwFieldUId), attackingMembers)));

				if (partymember.CharObj.Buffs.Contains((int)MobSkillID.CURSE))
				{
					bonusGain /= 2;
					regularGain /= 2;
				}

				partymember.CharObj.Modify.GainExp(regularGain, bonusGain);
			}
		}

		// only counts bishops that are in the same map as target
		// TODO make this a variable and increment/decrement it when players join/leave/change job
		public int CalculateBishops(long dwFieldUId)
			=> this.Count(i => i.InSameMap(dwFieldUId) && i.CharObj.Stats.nJob / 10 == 23);

		public int CalculateHighestLevel(long dwFieldUId)
			=> this.Where(i => i.InSameMap(dwFieldUId))
				.Max(i => i.CharObj.Stats.nLevel);

		/// <summary>
		/// Counts the leechable members in the given field
		/// </summary>
		private int CalculateLeechableMembers(long dwFieldUId, int highestCharLevel)
			=> this.Count(i => i.InSameMap(dwFieldUId)
				&& highestCharLevel - i.CharObj.Stats.nLevel <= 40);

		public int AveragePartyLevel(long dwFieldUId)
			=> (int)this
				.Where(i => i.InSameMap(dwFieldUId))
				.Average(i => i.CharObj.Stats.nLevel);

		public int MembersInMap(long dwFieldUId)
			=> this.Count(i => i.InSameMap(dwFieldUId) && i.CharObj.Stats.nHP > 0);

		// ----------------------

		public void PlayerLeave(int dwCharId, string sName, bool bKicked)
		{
			if (!Contains(dwCharId)) return;

			// have to send packet to owner before removing owner from party
			this[dwCharId].SendPacket(CPacket.CPartyMan.WithdrawParty_Done(this, dwCharId, sName, bKicked));

			Remove(dwCharId);

			Broadcast(CPacket.CPartyMan.WithdrawParty_Done(this, dwCharId, sName, bKicked));

			if (dwOwnerId == dwCharId)
			{
				foreach (var member in this)
				{
					if (member.CharObj != null)
					{
						member.CharObj.dwPartyID = 0;
					}
				}

				Clear(); // flags object for removal from party manager
			}
		}

		public void PlayerJoin(int dwCharId, string sCharName)
		{
			if (Contains(dwCharId)) return;

			if (!Invites.Remove(dwCharId)) return;

			Add(new PartyMember(dwCharId));

			var user = this[dwCharId].CharObj;

			if (user is null) // uhh
			{
				Remove(dwCharId);
				return;
			}

			user.dwPartyID = PartyID;

			foreach (var member in this.Where(pm => pm.InSameMap(user.Field.dwUniqueId)))
			{
				foreach (var buff in member.CharObj.Buffs.Where(b => b.dwCharFromId == member.dwCharId))
				{
					if (buff.StatType == SecondaryStatFlag.BlueAura
						|| buff.StatType == SecondaryStatFlag.DarkAura
						|| buff.StatType == SecondaryStatFlag.YellowAura)
					{
						user.Buffs.AddAura(buff.StatType, buff.dwCharFromId, buff.nSkillID, buff.nSLV);
						break;
					}
				}
			}

			Broadcast(CPacket.CPartyMan.JoinParty_Done(this, sCharName));
		}

		public void PlayerDC(int dwCharId)
		{
			Broadcast(CPacket.CPartyMan.LoadParty(this), dwCharId);

			if (dwOwnerId == dwCharId && Count > 1)
				ChangePartyBoss(this.FirstOrDefault(c => c.dwCharId != dwOwnerId)?.dwCharId ?? 0, true);
		}

		public void BroadcastLoadParty()
		{
			Broadcast(CPacket.CPartyMan.LoadParty(this));
		}

		public void ChangePartyBoss(int dwCharId, bool bDC)
		{
			if (!Contains(dwCharId))
			{
				this[dwOwnerId]?.SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_Unknown));
				return;
			}

			if (this[dwCharId].CharObj is null) return;

			var member = this[dwCharId].CharObj;

			if (member.ChannelID != this[dwOwnerId].CharObj?.ChannelID)
			{
				this[dwOwnerId].SendPacket(CPacket.CPartyMan.Party_Error(PartyOps.PartyRes_ChangePartyBoss_NotSameChannel));
				return;
			}

			dwOwnerId = dwCharId;
			Broadcast(CPacket.CPartyMan.ChangePartyBoss_Done(this, dwCharId, bDC));
		}

		// ----------------------------------- invite list stuff

		public bool Invited(int dwCharId)
			=> Invites.Contains(dwCharId);

		public void AddInvite(int dwCharId)
		{
			if (!Invites.Contains(dwCharId))
				Invites.AddLast(dwCharId);
		}

		// -----------------------------------

		public void EncodePartyData(COutPacket p)
		{
			var aPartyMembers = new PartyMember[6];
			var idx = 0;

			foreach (var item in this)
			{
				aPartyMembers[idx++] = item;
			}

			//PartyMember-----------------------------

			foreach (var c in aPartyMembers)
			{
				if (c is null) p.Skip(4);
				else p.Encode4(c.dwCharId);
			}

			foreach (var c in aPartyMembers)
			{
				if (c is null) p.Skip(13);
				else p.EncodeStringFixed(c.sCharacterName, 13);
			}

			foreach (var c in aPartyMembers)
			{
				if (c is null) p.Skip(4);
				else p.Encode4(c.nJob);
			}

			foreach (var c in aPartyMembers)
			{
				if (c is null) p.Skip(4);
				else p.Encode4(c.nLevel);
			}

			foreach (var c in aPartyMembers)
			{
				if (c is null || !c.bOnline) p.Encode4(-2);
				else if (c.bCashShop) p.Encode4(-1);
				else p.Encode4(c.nChannel);
			}

			p.Encode4(dwOwnerId);

			//-----------------------------

			//adwFieldID-----------------------------

			foreach (var c in aPartyMembers)
			{
				if (c is null || !c.bOnline)
				{
					p.Encode4(Constants.InvalidMap);
				}
				else if (MasterManager.CharacterPool.CharInCashShop(c.dwCharId) || c.CharObj.Field is null)
				{
					p.Encode4(-1);
				}
				else
				{
					p.Encode4(c.CharObj.Field.MapId);
				}
			}
			//-----------------------------

			//aTownPortal-----------------------------
			foreach (var c in aPartyMembers)
			{
				if (c is null)
				{
					p.Skip(20);
				}
				else
				{
					p.Encode4(c.dwTownID);
					p.Encode4(c.dwFieldID);
					p.Encode4(c.nSkillID);
					p.Encode4(c.ptFieldPortalX);
					p.Encode4(c.ptFieldPortalY);
				}
			}
			//-----------------------------

			//aPQReward-----------------------------
			for (int i = 0; i < 6; i++)
			{
				p.Encode4(0);
			}
			//-----------------------------

			//aPQRewardType-----------------------------
			for (int i = 0; i < 6; i++)
			{
				p.Encode4(0);
			}
			//-----------------------------

			p.Encode4(0); //dwPQRewardMobTemplateID
			p.Encode4(0); //bPQReward
		}

		public void Broadcast(COutPacket packet, params int[] excludes)
		{
			using (packet)
			{
				foreach (var member in this)
				{
					if (!excludes.Contains(member.dwCharId))
						member.SendPacket(packet);
				}
			}
		}

		protected override int GetKeyForItem(PartyMember item) => item.dwCharId;
	}
}
