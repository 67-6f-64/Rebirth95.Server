using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Skill;
using Rebirth.Field.FieldObjects;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using Rebirth.Characters.Skill.Buff;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;

namespace Rebirth.Field.FieldPools
{
	public class CUserPool : CObjectPool<Character>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CUserPool));

		public CUserPool(CField parentField)
			: base(parentField) { }

		public void UserInfoRequest(Character c, CInPacket p)
		{
			var dwDickCount = p.Decode4();
			var nUserID = p.Decode4();

			var pUser = this[nUserID];

			if (pUser != null)
				c.SendPacket(pUser.CharacterInfo());
		}

		public void ForEach(Action<Character> action)
		{
			// TODO ability to skip action on admin characters
			foreach (var user in this)
			{
				action.Invoke(user);
			}
		}

		public void ForEachPartyMember(int dwPartyID, Action<Character> action)
		{
			foreach (var user in this)
			{
				if (user.Party is null) return;
				if (user.Party.PartyID == dwPartyID)
				{
					action.Invoke(user);
				}
			}
		}

		protected override void InsertItem(int index, Character user)
		{
			try
			{
				if (user.Party != null)
				{
					foreach (var buff in user.Buffs)
					{
						if (buff.dwCharFromId != user.dwId) continue;

						if (buff.StatType != SecondaryStatFlag.BlueAura &&
						    buff.StatType != SecondaryStatFlag.DarkAura &&
						    buff.StatType != SecondaryStatFlag.YellowAura) continue;

						ForEachPartyMember(user.Party.PartyID,
							member => member.Buffs
								.AddAura(buff.StatType, buff.dwCharFromId, buff.nSkillID, buff.nSLV));
						break;
					}
				}

				//Spawn the player being added to everyone in the room
				{
					var pPacketEnter = user.MakeEnterFieldPacket();
					Field.EncodeFieldSpecificData(pPacketEnter, user);
					Field.Broadcast(pPacketEnter);
				}

				//So i need to fix a bug where when u change map - u dont receive ur own dmg skin lol

				//Spawn [other players + ext data] in room to person entering
				foreach (var c in this)
				{
					var pPacketEnter = c.MakeEnterFieldPacket();
					Field.EncodeFieldSpecificData(pPacketEnter, c);
					user.SendPacket(pPacketEnter);

					//var pPacketEnterExt = CPacket.Custom.UserEnter(c.dwId, c.nDamageSkin);
					//user.SendPacket(pPacketEnterExt);
				}

				Field.Drops.Update(); // remove expired drops (required cuz map might be empty and thus not removing drops)

				//This must be here !!!!
				base.InsertItem(index, user);

				//Spawn entering person's extension data to everyone on map
				{
					//var pPacketEnterExt = CPacket.Custom.UserEnter(user.dwId, user.nDamageSkin);
					//Field.Broadcast(pPacketEnterExt);
				}

				//Spawn entering person's entities to everyone on map
				if (JobLogic.IsEvan(user.Stats.nJob) && user.Stats.nJob != 2001)
				{
					user.Dragon = new CDragon(user);
					user.Dragon.SpawnDragonToMap();
				}

				user.Pets.SpawnPetsToMap();

				//Spawn field entities to person entering
				Field.SendSpawnDragons(user);
				// pets are handled in UserEnterField packet

				Field.SendSpawnSummons(user);
				Field.Summons.InsertFromStorage(user); // needs to be after SendSpawnSummons

				Field.SendSpawnMobs(user);
				Field.SendSpawnNpcs(user);
				Field.SendSpawnReactors(user);
				Field.SendSpawnAffectedAreas(user);
				Field.SendSpawnMinirooms(user);
				Field.SendSpawnKites(user);
				Field.SendSpawnTownPortals(user);
				Field.SendSpawnOpenGates(user);
				Field.SendSpawnDrops(user);
				Field.SendActiveWeatherEffect(user);

				Field.AssignControllerMobs();
				Field.AssignControllerNpcs();

				if (Field.tFieldTimerExpiration.SecondsUntilEnd() > 0)
				{
					user.SendPacket(CPacket.CreateClock((int)Field.tFieldTimerExpiration.SecondsUntilEnd()));
				}

				if (Field.nFieldDeathCount > -1)
				{
					//user.SendPacket(CPacket.Custom.FieldDeathCount(Field.nFieldDeathCount));
				}

				if (user.Party != null)
				{
					foreach (var c in this)
					{
						if (c.Party?.PartyID != user.Party.PartyID) continue;
						foreach (var buff in c.Buffs)
						{
							if (buff.dwCharFromId != c.dwId) continue;
							if (buff.StatType == SecondaryStatFlag.BlueAura
								|| buff.StatType == SecondaryStatFlag.DarkAura
								|| buff.StatType == SecondaryStatFlag.YellowAura)
							{
								user.Buffs.AddAura(buff.StatType, buff.dwCharFromId, buff.nSkillID, buff.nSLV);
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				user?.Socket?.Disconnect();
			}
		}

		protected override void RemoveItem(int index)
		{
			var user = GetAtIndex(index);

			if (user == null) return;

			var toTransfer = new List<CSummon>();
			foreach (var summon in new List<CSummon>(Field.Summons))
			{
				if (summon.dwParentID != user.dwId) continue;

				summon.nLeaveType = SummonLeaveType.LEAVE_TYPE_LEAVE_FIELD;

				if (summon.nMoveAbility != SummonMoveAbility.NoMove && summon.nMoveAbility != SummonMoveAbility.FlyRandom && summon.nMoveAbility != SummonMoveAbility.WalkRandom)
				{
					toTransfer.Add(summon);
				}

				Field.Summons.Remove(summon);
			}

			if (toTransfer.Count > 0)
			{
				MasterManager.SummonStorage.Insert(user.dwId, toTransfer);
			}

			user.Buffs.Remove((int)Skills.MECHANIC_AR_01);
			user.Buffs.Remove((int)Skills.CAPTAIN_ADVANCED_HOMING);
			user.Buffs.Remove((int)Skills.VALKYRIE_HOMING);
			user.Buffs.Remove((int)Skills.EVAN_KILLING_WING);

			base.RemoveItem(index);

			Field.RemoveController(user.dwId);

			Field.Broadcast(user.MakeLeaveFieldPacket());
			//Field.Broadcast(CPacket.Custom.UserLeave(user.dwId));
		}

		protected override int GetKeyForItem(Character item) => item.dwId;
	}
}
