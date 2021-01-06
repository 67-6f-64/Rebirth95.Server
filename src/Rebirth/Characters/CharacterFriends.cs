using log4net;
using Npgsql;
using Rebirth.Characters.Actions;
using Rebirth.Entities;
using Rebirth.Network;
using Rebirth.Server.Center;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Rebirth.Common.Types;
using Rebirth.Tools;

namespace Rebirth.Characters
{
	public sealed class CharacterFriends : NumericKeyedCollection<GW_Friend>
	{
		public static ILog Log = LogManager.GetLogger(typeof(CharacterFriends));

		public Character Parent { get; private set; }

		public byte nFriendMax => (byte)Parent.Stats.nFriendMax;

		public int nChannel { get; private set; } // dont point this to char stats cuz we dont want that value
		private void SyncChannel() => nChannel = Parent.ChannelID;
		public bool FriendListFull() => Count > nFriendMax;

		public CharacterFriends(Character c)
		{
			Parent = c;
		}

		public void Dispose()
		{
			Clear();
			Parent = null;
		}

		public void SendLoad()
		{
			Parent.SendPacket(LoadPacket());
		}

		private COutPacket LoadPacket()
		{
			var p = new COutPacket(SendOps.LP_FriendResult);
			p.Encode1(FriendOps.FriendRes_LoadFriend_Done);

			p.Encode1((byte)Count);

			foreach (var friend in this)
			{

				friend.Encode(p);
			}

			foreach (var friend in this)
			{
				p.Encode4(friend.bCashShop ? 1 : 0);
			}

			return p;
		}

		public void NotifyChangeFriendInfo(SocialNotiflag nFlag)
		{
			switch (nFlag)
			{
				case SocialNotiflag.LogIn:
				case SocialNotiflag.ChangeChannel:
					SyncChannel();
					break;
				case SocialNotiflag.MigrateCashShop:
					break;
				case SocialNotiflag.LogOut:
					nChannel = -1;
					break;
				default: return;
			}

			foreach (var friend in this)
			{
				var charFriend = MasterManager.CharacterPool.Get(friend.dwFriendID, false);

				if (charFriend is null) continue;

				var me = charFriend.Friends[Parent.dwId];

				me.nChannelID = nChannel;
				me.bCashShop = nFlag == SocialNotiflag.MigrateCashShop;
				me.bOnline = nChannel >= 0;

				if (me.bCashShop)
				{
					me.nFlag = 2; // busy
				}
				else if (me.bOnline)
				{
					me.nFlag = 0; // online
				}

				charFriend.SendPacket(NotifyChangeFriendInfoPacket(me));
			}
		}

		private COutPacket NotifyChangeFriendInfoPacket(GW_Friend friendData)
		{
			var p = new COutPacket(SendOps.LP_FriendResult);

			p.Encode1((byte)FriendOps.FriendRes_Notify);
			p.Encode4(friendData.dwFriendID); // dwFriendID
			p.Encode1(friendData.bCashShop); // m_aInShop
			p.Encode4(friendData.nChannelID); // nChannelID

			return p;
		}

		public void HandleBuddyChat(string sText, int[] aMemberList)
		{
			foreach (var dwId in aMemberList)
			{
				if (!Contains(dwId)) continue;

				GetOrDefault(dwId).SendPacket(CPacket.GroupMessage(GroupMessageType.BuddyChat, Parent.Stats.sCharacterName, sText));
			}
		}

		public void OnPacket(CInPacket p)
		{
			var opCode = (FriendOps)p.Decode1();

			switch (opCode)
			{
				case FriendOps.FriendReq_SetFriend:
					{
						var sTarget = p.DecodeString();
						var sFriendGroup = p.DecodeString();

						if (sTarget.ToLower().Equals(Parent.Stats.sCharacterName.ToLower()))
						{
							// packet?
							return; // no imaginary friends
						}

						if (sTarget.Length > Constants.MaxCharNameLength || sFriendGroup.Length > 17)
						{
							// packet?
							return; // no hax
						}

						if (MasterManager.CharacterPool.Get(sTarget, false) is Character pTarget) // online user
						{
							if (TryGetValue(pTarget.dwId, out GW_Friend pFriend)) // already in friend list
							{
								if (pFriend.nFlag == 10)
								{
									Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_PleaseWait)); // "You've already made the Friend Request. Please try again later."
								}
								else if (pFriend.sFriendGroup.ToLower().Equals(sFriendGroup.ToLower()))
								{
									Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_AlreadySet));
								}
								else
								{
									pFriend.sFriendGroup = sFriendGroup;
									return;
								}
							}
							else if (FriendListFull()) // FFF
							{
								Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_FullMe));
							}
							else if (pTarget.Friends.FriendListFull())
							{
								Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_FullOther));
							}
							else
							{
								Add(new GW_Friend() // adding them to me
								{
									dwFriendID = pTarget.dwId,
									sFriendName = pTarget.Stats.sCharacterName,
									nFlag = 10,
									nChannelID = pTarget.ChannelID,
									sFriendGroup = sFriendGroup
								});

								pTarget.Friends.Add(new GW_Friend() // adding myself to them
								{
									dwFriendID = Parent.dwId,
									sFriendName = Parent.Stats.sCharacterName,
									nFlag = 1,
									nChannelID = Parent.ChannelID,
									sFriendGroup = "Group Unknown"
								});

								pTarget.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_Invite));

								//pTarget.Friends.SendLoad();
								Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_Done));
							}
						}
						else
						{
							Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_Unknown, "Unable to find user or user is not online."));
						}
					}
					break;
				case FriendOps.FriendReq_AcceptFriend:
					{
						var dwCharId = p.Decode4();

						if (GetOrDefault(dwCharId) is GW_Friend pFriend && pFriend.nFlag == 1)
						{
							pFriend.nFlag = 0;
							Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_LoadFriend_Done));

							if (MasterManager.CharacterPool.Get(dwCharId) is Character pTarget) // character is online
							{
								pTarget.Friends.NotifyChangeFriendInfo(SocialNotiflag.LogIn);
								NotifyChangeFriendInfo(SocialNotiflag.LogIn);
							}
							else
							{
								UpdateFlagInDB(dwCharId, Parent.dwId, 0);
							}
						}
						else
						{
							Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_SetFriend_Unknown, "No pending requests with given character ID."));
						}
					}
					break;
				case FriendOps.FriendReq_DeleteFriend:
					{
						var dwTargetId = p.Decode4();
						if (GetOrDefault(dwTargetId) is GW_Friend pFriend)
						{
							Remove(dwTargetId);
							Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_DeleteFriend_Done));

							if (MasterManager.CharacterPool.Get(dwTargetId) is Character pTarget) // fix them if theyre online
							{
								pTarget.Friends.Remove(Parent.dwId);
								pTarget.Friends.SendLoad();
							}

							DeleteFriendFromDB(dwTargetId);
						}
						else
						{
							Parent.SendPacket(GenericFriendResPacket(FriendOps.FriendRes_DeleteFriend_Unknown, "Unable to find character in friend list."));
						}
						break;
					}
				default:
					Parent.SendMessage($"Unhandled friend operation: {nameof(opCode)}.");
					break;
			}
		}

		private void DeleteFriendFromDB(int dwFriendID)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.friends WHERE character_id = {Parent.dwId} AND friend_id = {dwFriendID};");
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.friends WHERE character_id = {dwFriendID} AND friend_id = {Parent.dwId};");

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					cmd.ExecuteNonQuery();
			}
		}

		private void UpdateFlagInDB(int dwCharacterID, int dwFriendID, int nFlag)
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine($"UPDATE {Constants.DB_All_World_Schema_Name}.friends SET flag = {nFlag} WHERE character_id = {dwCharacterID} AND friend_id = {dwFriendID} WHERE EXISTS (SELECT * FROM {Constants.DB_All_World_Schema_Name}.friends WHERE character_id = {dwCharacterID} AND friend_id = {dwFriendID})");

				using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Creates and returns a friend list response packet.
		/// </summary>
		/// <param name="nType">Type of packet to be sent.</param>
		/// <param name="sMsg">Message to send (only used for types ending in _Unknown)</param>
		/// <returns></returns>
		public COutPacket GenericFriendResPacket(FriendOps nType, string sMsg = null)
		{
			var p = new COutPacket(SendOps.LP_FriendResult);
			p.Encode1((byte)nType);

			switch (nType)
			{
				// CWvsContext::CFriend::Reset(v2->m_pFriendArray.p, v3);
				case FriendOps.FriendRes_LoadFriend_Done:
				case FriendOps.FriendRes_SetFriend_Done:
				case FriendOps.FriendRes_DeleteFriend_Done:
					{
						p.Encode1((byte)Count);

						foreach (var friend in this)
						{
							friend.Encode(p);
						}

						foreach (var friend in this)
						{
							p.Encode4(friend.bCashShop ? 1 : 0);
						}
						return p;
					}
				// CWvsContext::CFriend::UpdateFriend(v2->m_pFriendArray.p, v3, 1);
				case FriendOps.FriendRes_NotifyChange_FriendInfo: // this is the response when u block someone
					{
						return p; // uhh todo
					}
				case FriendOps.FriendRes_SetFriend_FullMe: // buddy list full
				case FriendOps.FriendRes_SetFriend_FullOther: // buddy list full
				case FriendOps.FriendRes_SetFriend_AlreadySet: // already a buddy
				case FriendOps.FriendRes_SetFriend_Master: //  cant buddy a gm
				case FriendOps.FriendRes_SetFriend_UnknownUser:
				case FriendOps.FriendRes_PleaseWait: // friend request already made, please wait
					{
						return p; // no more encoding
					}
				//case FriendOps.FriendRes_Notify:
				//	{
				//		p.Encode4(Parent.dwId); // dwFriendID
				//		p.Encode1(MasterManager.CharacterPool.CharInCashShop(Parent.dwId)); // m_aInShop
				//		p.Encode4(nChannel); // nChannelID
				//		return p;
				//	}
				case FriendOps.FriendRes_SetFriend_Unknown:
				case FriendOps.FriendRes_AcceptFriend_Unknown:
				case FriendOps.FriendRes_DeleteFriend_Unknown:
				case FriendOps.FriendRes_IncMaxCount_Unknown:
					{
						p.Encode1(sMsg != null); // if false, shows "The request was denied due to an unknown error."
						p.EncodeString(sMsg); // encode string to be displayed as error message
						return p;
					}
				case FriendOps.FriendRes_IncMaxCount_Done: // need to make sure the stored value has changed before sending packet
					{
						p.Encode1(nFriendMax);
						return p;
					}
				case FriendOps.FriendRes_Invite:
					{
						p.Encode4(Parent.dwId);
						p.EncodeString(Parent.Stats.sCharacterName);
						p.Encode4(Parent.Stats.nLevel);
						p.Encode4(Parent.Stats.nJob);
						// CWvsContext::CFriend::Insert(v2->m_pFriendArray.p, v3);
						p.Encode4(Parent.dwId);
						p.EncodeStringFixed(Parent.Stats.sCharacterName, 13); // why tf is this sent twice
						p.Encode1(1); // flag
						p.Encode4(nChannel);
						p.EncodeStringFixed("Group Unknown", 17);
						p.Encode1(0); // in cash shop.. obv not possible if sending an invite
						return p;
					}
				default:
					{
						throw new NotImplementedException($"Unhandled FriendResult Op: {nameof(nType)}");
					}
			}
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();
				dbQuery.AppendLine("SELECT f.friend_id, c.name, f.flag, f.friend_group");
				dbQuery.AppendLine($"FROM {Constants.DB_All_World_Schema_Name}.friends f");
				dbQuery.AppendLine($"JOIN {Constants.DB_All_World_Schema_Name}.characters c on c.id = f.friend_id");
				dbQuery.AppendLine($"WHERE character_id = {Parent.dwId}");

#if DEBUG
				Log.Debug(dbQuery);
#endif
				try
				{
					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					using (var r = await cmd.ExecuteReaderAsync())
					{
						while (r.Read())
						{
							var f = new GW_Friend
							{
								dwFriendID = Convert.ToInt32(r["friend_id"]),
								sFriendName = r["name"] as string,
								nFlag = Convert.ToInt32(r["flag"]),
								sFriendGroup = r["friend_group"] as string,
							};

							var friend = MasterManager.CharacterPool.Get(f.dwFriendID, false);

							if (friend != null)
							{
								f.bOnline = true;
								f.nChannelID = friend.ChannelID;
							}
							else
							{
								f.bOnline = false;
								f.nChannelID = -1;
							}

							Add(f);
						}
					}
				}
				catch (Exception EX)
				{
					Log.Error(dbQuery);
					Log.Error(EX);
				}
			}
		}

		public void SaveToDB()
		{
			if (Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.friends");
				dbQuery.AppendLine($"WHERE character_id = {Parent.dwId};");

				//using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
				//	cmd.ExecuteNonQuery();

				//dbQuery.Clear();

				foreach (var item in this)
				{
					dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.friends (character_id, friend_id, flag, friend_group)");
					dbQuery.AppendLine($"VALUES ({Parent.dwId}, {item.dwFriendID}, {item.nFlag}, '{item.sFriendGroup.Replace('\'', '\"')}');");
				}

				try
				{
					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
					{
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception EX)
				{
					Log.Error(dbQuery);
					Log.Error(EX);
				}
			}
		}

		protected override int GetKeyForItem(GW_Friend item) => item.dwFriendID;
	}
}
