using log4net;

using Npgsql;

using Rebirth.Characters;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebirth.Server.Center
{
	public sealed class GuildManager : NumericKeyedCollection<Guild>
	{
		public static ILog Log = LogManager.GetLogger(typeof(GuildManager));

		private int _nextgid;
		private int NextGuildID
		{
			get => ++_nextgid; // increment then pass ID to requester 
			set => _nextgid = value;
		}

		private DateTime tLastSave;
		private const int SAVE_INTERVAL_SECONDS = 10 * 60; // 10 min

		public int GetGuildID(int dwCharID) => mCharIDToGuildID.GetValueOrDefault(dwCharID);
		public bool IsGuildMaster(int dwCharID) => this[GetGuildID(dwCharID)]?[dwCharID].GuildRank == 1;
		public bool GuildNameExists(string input) => this.FirstOrDefault(g => g.GuildName.EqualsIgnoreCase(input)) != null;
		public int GuildCapacity(int dwCharID) => this[GetGuildID(dwCharID)]?.Capacity ?? -1;
		public Guild GetGuild(int dwCharID) => this[GetGuildID(dwCharID)];
		public Guild GetGuild(Character c) => GetGuild(c.dwId);


		private readonly Dictionary<int, int> mCharIDToGuildID; // populated from inside guild objects -> InsertItem()
		public void RemoveCharIDFromCache(int dwCharID) => mCharIDToGuildID.Remove(dwCharID);
		public void AddCharIDToCache(int dwCharID, int nGuildID) => mCharIDToGuildID.Add(dwCharID, nGuildID);

		private bool _initialized;

		public GuildManager()
		{
			mCharIDToGuildID = new Dictionary<int, int>();
			tLastSave = DateTime.Now;
		}

		public void Update()
		{
			if (Count <= 0) return;

			if (tLastSave.SecondsSinceStart() >= SAVE_INTERVAL_SECONDS) SaveGuilds();

			// TODO guild quest stuff here
		}

		/// <summary>
		/// Load and cache all current guild information
		/// </summary>
		public void Init()
		{
			if (_initialized) return;
			_initialized = true;

			if (Count > 0) throw new InvalidOperationException("Trying to initialize guild information when there already exists guild objects in collection.");

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.guilds", conn))
				{
					using (var r = cmd.ExecuteReader())
					{
						while (r.Read())
						{
							Add(new Guild(this)
							{
								GuildID = r.GetInt32(0),
								AllianceID = r.GetInt32(1),
								GuildName = r.GetString(2),
								GuildPoints = r.GetInt32(3),

								GuildMark = r.GetInt16(4),
								GuildMarkColor = (byte)r.GetInt16(5),
								GuildMarkBg = r.GetInt16(6),
								GuildMarkBgColor = (byte)r.GetInt16(7),

								Capacity = r.GetInt16(8),
								Notice = r.GetString(9),

								Rankt1Title = r.GetString(10),
								Rankt2Title = r.GetString(11),
								Rankt3Title = r.GetString(12),
								Rankt4Title = r.GetString(13),
								Rankt5Title = r.GetString(14),

								LeaderID = r.GetInt32(15),
							});
						}
					}
				}

				var sbQuery = new StringBuilder();

				foreach (var guild in this)
				{
					sbQuery.AppendLine($"SELECT g.character_id, g.guild_id, g.guild_rank, g.guild_points, g.guild_contribution, g.alliance_rank, c.name, c.job, c.level");
					sbQuery.AppendLine($"FROM {Constants.DB_All_World_Schema_Name}.characters c");
					sbQuery.AppendLine($"JOIN rebirth.guild_members g ON c.id = g.character_id");
					sbQuery.AppendLine($"WHERE g.guild_id = {guild.GuildID};");

					using (var cmd = new NpgsqlCommand(sbQuery.ToString(), conn))
					{
						using (var r = cmd.ExecuteReader())
						{
							while (r.Read())
							{
								var dwCharID = (int)r["character_id"];

								guild.Add(
									new GuildMember(
										dwId: dwCharID,
										nGuildRank: (int)r["guild_rank"],
										nGuildPoints: (int)r["guild_points"],
										nContribution: (int)r["guild_contribution"],
										nAllianceRank: (int)r["alliance_rank"])
									);

								guild[dwCharID]
										.Init(
										name: r["name"] as string,
										job: (short)r["job"],
										level: (short)r["level"]
									);
							}
						}
					}

					if (guild.GuildID > _nextgid) NextGuildID = guild.GuildID;

					sbQuery.Clear();

					if (guild.Count <= 0) throw new NullReferenceException($"Guild {guild.GuildID} has zero members.");
				}
			}
		}

		public Guild CreateNewGuild(Character owner, string sGuildName)
		{
			var guild = new Guild(this)
			{
				GuildName = sGuildName,
				GuildID = NextGuildID,
				LeaderID = owner.dwId,
				Rankt1Title = "Master",
				Rankt2Title = "Jr. Master",
				Rankt3Title = "Member",
				Rankt4Title = "Member",
				Rankt5Title = "Member",
				Notice = "Default Notice",
				GuildLevel = (short)GuildConstants.DEFAULT_GUILD_LEVEL,
				Capacity = (short)GuildConstants.DEFAULT_GUILD_CAPACITY,
			};

			foreach (var member in owner.Party)
			{
				member.Update();
				var rank = member.dwCharId == owner.dwId ? 1 : 5;
				var guildmember = new GuildMember(member.dwCharId, rank, 500, 500, 0);
				guildmember.Init(member.sCharacterName, (short)member.nJob, (short)member.nLevel);

				guild.GuildPoints += 500;
				member.CharObj.SendPacket(CPacket.IncGPMessage(500));

				guild.Add(guildmember);
			}

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sbQuery = new StringBuilder();

				try
				{
					sbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.guilds (guild_id, guild_name, guild_points, guild_mark, guild_mark_color, guild_mark_bg, guild_mark_bg_color, capacity, notice, rank1title, rank2title, rank3title, rank4title, rank5title, leader)");
					sbQuery.AppendLine($"VALUES ({guild.GuildID}, @name, {guild.GuildPoints}, {guild.GuildMark}, {guild.GuildMarkColor}, {guild.GuildMarkBg}, {guild.GuildMarkBgColor}, {guild.Capacity}, '{guild.Notice}', '{guild.Rankt1Title}', '{guild.Rankt2Title}', '{guild.Rankt3Title}', '{guild.Rankt4Title}', '{guild.Rankt5Title}', {guild.LeaderID});");

					foreach (var member in guild)
					{
						sbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.guild_members");
						sbQuery.AppendLine($"(character_id, guild_id, guild_rank, guild_points, guild_contribution, alliance_rank) VALUES");
						sbQuery.AppendLine($"({member.dwParentID},{guild.GuildID},{member.GuildRank},{member.GuildPoints},{member.GuildContribution},{member.AllianceRank});");
					}

					using (var cmd = new NpgsqlCommand(sbQuery.ToString(), conn))
					{
						cmd.Parameters.AddWithValue($"name", guild.GuildName);
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					Log.Error(sbQuery);
					Log.Error(ex.ToString());
					return null;
				}
			}

			Add(guild);

			Log.Debug($"Created new guild with ID {guild.GuildID}.");

			return guild;
		}

		public void OnResultPacket(WvsGameClient c, CInPacket p)
		{
			var pChar = c.Character;
			var operation = p.Decode1();


			switch (operation)
			{
				// TODO
			}

			c.Character.Action.Enable();
		}

		public void UpdateGuildMember(Character c, SocialNotiflag nFlag)
		{
			if (this[GetGuildID(c.dwId)]?[c.dwId] is GuildMember member)
			{
				switch (nFlag)
				{
					case SocialNotiflag.LogOut:
						member.bOnline = false;
						member.Guild.Broadcast(CPacket.CGuildMan.NotifyLoginOrLogout(member), c.dwId);
						break;
					case SocialNotiflag.LogIn:
					case SocialNotiflag.ChangeChannel:
						if (!member.bOnline)
						{
							member.bOnline = true;
							member.Guild.Broadcast(CPacket.CGuildMan.NotifyLoginOrLogout(member), c.dwId);
						}
						c.SendPacket(CPacket.CGuildMan.LoadGuild_Done(true, member.Guild));
						break;
					case SocialNotiflag.ChangeLevel:
						IncGP(c.dwId, 20);
						member.ParentLevel = c.Stats.nLevel;
						member.Guild.Broadcast(CPacket.CGuildMan.ChangeLevelOrJob(member));
						break;
					case SocialNotiflag.ChangeJob:
						IncGP(c.dwId, 100); // maybe scale this based on level and job (ie db and evan get lower since they have twice as many jobs)
						member.ParentJob = c.Stats.nJob;
						member.Guild.Broadcast(CPacket.CGuildMan.ChangeLevelOrJob(member));
						break;
					case SocialNotiflag.ChangeName:
						member.ParentName = c.Stats.sCharacterName;
						// TODO broadcast this to guild
						break;
				}
			}
		}

		public void OnRequestPacket(WvsGameClient c, CInPacket p)
		{
			var pChar = c.Character;

			// CField::SendCreateGuildAgreeMsg
			var opCode = p.Decode1();

			switch ((GuildOps)opCode)
			{
				case GuildOps.GuildReq_CheckGuildName: // CField::InputGuildName
					{
						var sGuildName = p.DecodeString();
						CheckGuildNameOk(c.Character, sGuildName);
					}
					break;
				case GuildOps.GuildRes_CreateGuildAgree_Reply: // CField::SendCreateGuildAgreeMsg
					{
						var nGuildID = p.Decode4();
						var bAgree = p.Decode1();
					}
					break;
				case GuildOps.GuildReq_InviteGuild: // CField::InputGuildName
					{
						var sCharName = p.DecodeString();
						InviteGuild(pChar, sCharName);
					}
					break;
				case GuildOps.GuildReq_JoinGuild:
					{
						var nGuildID = p.Decode4();
						JoinGuild(c.Character, nGuildID);
					}
					break;
				case GuildOps.GuildReq_WithdrawGuild:
					{
						WithdrawGuild(c.Character);
					}
					break;

				case GuildOps.GuildReq_KickGuild:
					{
						var dwCharID = p.Decode4();
						var sCharName = p.DecodeString();
						KickGuild(c.Character, dwCharID, sCharName);
					}
					break;
				case GuildOps.GuildReq_RemoveGuild:
					{
						DisbandGuild(pChar);
					}
					break;
				case GuildOps.GuildReq_SetGradeName:
					{
						var newGradeNames = new string[5];

						for (int i = 0; i < 5; i++)
						{
							newGradeNames[i] = p.DecodeString();
						}

						SetGradeName(pChar, newGradeNames);
					}
					break;
				case GuildOps.GuildReq_SetMemberGrade:
					{
						var memberGradeCharId = p.Decode4();
						var newMemberRank = p.Decode1();
						SetMemberGrade(pChar, memberGradeCharId, newMemberRank);
					}
					break;
				case GuildOps.GuildReq_SetMark: // CField::SendSetGuildMarkMsg
					{
						var nMarkBg = p.Decode2();
						var nMarkBgColor = p.Decode1();
						var nMark = p.Decode2();
						var nMarkColor = p.Decode1();
						ChangeGuildIcon(pChar, nMarkBg, nMarkBgColor, nMark, nMarkColor);
					}
					break;
				case GuildOps.GuildReq_SetNotice:
					{
						string newNotice = p.DecodeString();
						ChangeGuildNotice(pChar, newNotice);
					}
					break;
				default:
					Log.Error($"Client with ID: {c.Character.dwId} tried to send an unhandled guild operation with op code: {opCode}.");
					break;
			}

			c.Character.Action.Enable();
		}

		private void ChangeGuildIcon(Character pChar, short nMarkBg, byte nMarkBgColor, short nMark, byte nMarkColor)
		{
			// UI.wz/GuildMark.img
			if (nMarkBgColor > 16 || nMarkBgColor < 1
				|| nMarkColor > 16 || nMarkColor < 1
				|| nMarkBg > 1030 || nMarkBg < 1000)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_SetMark_Unknown));
				return;
			}

			switch (nMark / 1000)
			{
				case 2: // Animal
					if (nMark > 2020 || nMark < 2000) goto default;
					break;
				case 3: // Plant
					if (nMark > 3006 || nMark < 3000) goto default;
					break;
				case 4: // Pattern
					if (nMark > 4021 || nMark < 4000) goto default;
					break;
				case 5: // Letter
					if (nMark > 5035 || nMark < 5000) goto default;
					break;
				case 9: // Etc
					if (nMark > 9026 || nMark < 9000) goto default;
					break;
				default:
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_SetMark_Unknown));
					return;
			}

			if (GetGuild(pChar) is Guild guild)
			{
				if (guild.LeaderID != pChar.dwId || pChar.Stats.nMoney < 5000000) // validation has already occurred in npc script so user PE if this is tripped
				{
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_SetMark_Unknown));
				}
				else
				{
					pChar.Modify.GainMeso(-5000000);
					guild.GuildMarkBg = nMarkBg;
					guild.GuildMarkBgColor = nMarkBgColor;
					guild.GuildMark = nMark;
					guild.GuildMarkColor = nMarkColor;

					foreach (var member in guild)
					{
						if (MasterManager.CharacterPool.Get(member.dwParentID, false) is Character c)
						{
							c.SendPacket(CPacket.CGuildMan.SetMark_Done(guild));

							c.Field.Broadcast(CPacket.CGuildMan.GuildMarkChanged(c.dwId, guild), c);
						}
					}
					// send response packet to character
					// broadcast guild mark change packet to every character in maps that have a guild member in it
				}
			}
		}

		private void SetMemberGrade(Character pChar, int nTargetCharId, byte newRank)
		{
			if (GetGuild(pChar) is Guild guild && guild.LeaderID == pChar.dwId)
			{
				if (!guild.Contains(nTargetCharId) || newRank > 5 || newRank < 2)
				{
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_SetMemberGrade_Unknown));
				}
				else
				{
					guild[nTargetCharId].GuildRank = newRank;
					guild.Broadcast(CPacket.CGuildMan.SetMemberGrade_Done(guild[nTargetCharId]));
				}
			}
		}

		private void SetGradeName(Character pChar, string[] newRanks)
		{
			if (GetGuild(pChar) is Guild guild && guild.LeaderID == pChar.dwId)
			{
				if (newRanks.Any(str => str.Length > 10 || str.Length < 4)) // verified -> CWndGuildGrade::OnChangeGradeName
				{
					// TODO validate chars in strings
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_SetGradeName_Unknown));
				}
				else
				{
					guild.Rankt1Title = newRanks[0];
					guild.Rankt2Title = newRanks[1];
					guild.Rankt3Title = newRanks[2];
					guild.Rankt4Title = newRanks[3];
					guild.Rankt5Title = newRanks[4];
					guild.Broadcast(CPacket.CGuildMan.SetGradeName_Done(guild));
				}
			}
		}

		private void ChangeGuildNotice(Character pChar, string newNotice)
		{
			if (GetGuild(pChar) is Guild guild && guild.LeaderID == pChar.dwId)
			{
				if (newNotice.Length <= 100) // verified -> CTabGuild::OnSetNotice
				{
					// TODO validate chars in string
					guild.Notice = newNotice;
					guild.Broadcast(CPacket.CGuildMan.SetNotice_Done(guild));
				}
			}
		}

		private void WithdrawGuild(Character pChar)
		{
			var guild = GetGuild(pChar);

			if (guild is null)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_WithdrawGuild_NotJoined));
			}
			else if (guild.LeaderID == pChar.dwId)
			{
				pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "Guild masters cannot leave their guild."));
			}
			else // if (guild.Remove(pChar.dwId))
			{
				// order of operations is important here

				guild.Broadcast(CPacket.CGuildMan.GuildRes_WithdrawGuild_Done(guild.GuildID, pChar.dwId, pChar.Stats.sCharacterName));
				guild[pChar.dwId].Guild = null;
				guild.Remove(pChar.dwId);

				IncGP(pChar.dwId, -500);
				RemoveCharIDFromCache(pChar.dwId);

				pChar.SendPacket(CPacket.CGuildMan.LoadGuild_Done(false, null));
			}
		}

		private void KickGuild(Character pChar, int dwCharID, string sCharName)
		{
			var guild = GetGuild(pChar);

			if (guild is null || pChar.dwId == dwCharID || pChar.Stats.sCharacterName.EqualsIgnoreCase(sCharName))
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_KickGuild_Unknown));
			}
			else if (guild[dwCharID] == null)
			{
				pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "Unable to find member."));
			}
			else if (guild[pChar.dwId].GuildRank > 2 || guild[pChar.dwId].GuildRank == guild[dwCharID].GuildRank)
			{
				pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "You do not have permission to kick this member."));
			}
			else
			{
				// order of operations is important here

				guild.Broadcast(CPacket.CGuildMan.GuildRes_KickGuild_Done(guild.GuildID, dwCharID, guild[dwCharID].ParentName));
				guild[dwCharID].Guild = null;
				guild.Remove(dwCharID); // needs to be after the above packet

				IncGP(pChar.dwId, -500);
				RemoveCharIDFromCache(pChar.dwId);

				if (MasterManager.CharacterPool.Get(dwCharID, false) is Character c)
				{
					c.SendPacket(CPacket.CGuildMan.LoadGuild_Done(false, null));
				}
			}
		}

		private void JoinGuild(Character pChar, int nGuildID)
		{
			var guild = this[GetGuildID(nGuildID)];// TODO rename nGuildID variable to dwInviterID

			var inviteremoved = guild.Invites.Remove(pChar.dwId);

			if (guild == null)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_JoinGuild_Unknown));
			}
			else if (GetGuild(pChar) != null)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_JoinGuild_AlreadyJoined));
			}
			else if (!inviteremoved)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_JoinGuild_UnknownUser));
			}
			else if (guild.Count >= guild.Capacity)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_JoinGuild_AlreadyFull));
			}
			else
			{
				guild.Invites.Remove(pChar.dwId);
				var member = new GuildMember(pChar.dwId, 5, 0, 0, 0);
				member.Init(pChar.Stats.sCharacterName, pChar.Stats.nJob, pChar.Stats.nLevel);
				member.bOnline = true;

				guild.Add(member);
				guild.Broadcast(CPacket.CGuildMan.JoinGuild_Done(member));
				pChar.SendPacket(CPacket.CGuildMan.LoadGuild_Done(true, guild));

				IncGP(pChar.dwId, 500); // has to happen after member is added to guild
			}
		}

		private void InviteGuild(Character pChar, string targetName)
		{
			if (targetName.Length > 13 || targetName.Length < 4) return;

			var guildmember = GetGuild(pChar)?[pChar.dwId];

			if (guildmember is null || guildmember.GuildRank > 2)
			{
				pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "You are either not allowed to issue invites or you are not a member of a guild."));
			}
			else if (guildmember.Guild.Count >= guildmember.Guild.Capacity)
			{
				pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "Your guild is at max capacity."));
			}
			else
			{
				var targetChar = pChar.Field.ParentInstance.CFieldMan.GetCharacter(targetName);

				if (targetChar is null)
				{
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_JoinGuild_UnknownUser));
				}
				else if (targetChar.Stats.nLevel < GuildConstants.GUILD_MIN_CHAR_LEVEL) // 50 to discourage tryina farm points by having inactive low level accounts in guild
				{
					pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, $"Players must be at least level {GuildConstants.GUILD_MIN_CHAR_LEVEL} before they can join a guild."));
				}
				else if (GetGuild(targetChar) != null)
				{
					pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "Player is already in a guild."));
				}
				else if (guildmember.Guild.Invites.Contains(targetChar.dwId))
				{
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_InviteGuild_AlreadyInvited));
				}
				else
				{
					guildmember.Guild.Invites.Add(targetChar.dwId);
					targetChar.SendPacket(CPacket.CGuildMan.InviteGuild(pChar.dwId, pChar.Stats.sCharacterName, pChar.Stats.nLevel, pChar.Stats.nJob));
					pChar.SendPacket(CPacket.CGuildMan.ServerMsg(false, "The invite has been sent."));
				}
			}
		}

		public void DisbandGuild(Character pChar)
		{
			// TODO
			// process transaction
			// send response packet
			return;
		}

		public void CheckGuildNameOk(Character pChar, string sGuildName)
		{
			if (GetGuild(pChar) != null)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_CreateNewGuild_AlreayJoined));
			}
			else if (pChar.Party is null || pChar.Party.Count < GuildConstants.GUILD_MIN_PARTY_MEMBERS || pChar.Party.dwOwnerId != pChar.dwId)
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_CreateNewGuild_NotFullParty));
			}
			else if (pChar.Party.Any(pm => pm.nLevel < GuildConstants.GUILD_MIN_CHAR_LEVEL))
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_CreateNewGuild_Beginner));
			}
			else if (sGuildName.Length > 13 || sGuildName.Length < 4 || GuildNameExists(sGuildName))
			{
				pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_CreateNewGuild_GuildNameAlreadyExist));
			}
			else
			{
				if (pChar.Stats.nMoney >= GuildConstants.GUILD_COST_MILLION * 1000000 && CreateNewGuild(pChar, sGuildName) is Guild guild)
				{
					pChar.Modify.GainMeso(-GuildConstants.GUILD_COST_MILLION * 1000000);

					guild.Broadcast(CPacket.CGuildMan.CreateNewGuild_Done(guild));
				}
				else // DB save error
				{
					pChar.SendPacket(CPacket.CGuildMan.GuildRes_Error(GuildOps.GuildRes_CheckGuildName_Unknown));
				}
			}
		}

		public void IncGP(int dwCharID, int nAmount)
		{
			if (nAmount == 0) return;

			if (GetGuild(dwCharID) is Guild guild)
			{
				if (guild[dwCharID] is GuildMember member)
				{
					member.GuildPoints = Math.Max(0, member.GuildPoints + nAmount);

					if (nAmount > 0)
					{
						member.GuildContribution += nAmount;
					}
				}

				guild.GuildPoints = Math.Max(0, guild.GuildPoints + nAmount);
				guild.Broadcast(CPacket.CGuildMan.IncPoint_Done(guild));
				guild.Broadcast(CPacket.IncGPMessage(nAmount));
			}
		}

		public void SaveGuilds()
		{
			tLastSave = DateTime.Now;
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				var sbQuery = new StringBuilder();

				try
				{
					foreach (var guild in this)
					{
						sbQuery.AppendLine($"UPDATE {Constants.DB_All_World_Schema_Name}.guilds SET");
						sbQuery.AppendLine($"alliance_id = {guild.AllianceID},");

						sbQuery.AppendLine($"guild_name = @name{guild.GuildID},");

						sbQuery.AppendLine($"guild_points = {guild.GuildPoints},");
						sbQuery.AppendLine($"guild_mark = {guild.GuildMark},");
						sbQuery.AppendLine($"guild_mark_color = {guild.GuildMarkColor},");
						sbQuery.AppendLine($"guild_mark_bg = {guild.GuildMarkBg},");
						sbQuery.AppendLine($"guild_mark_bg_color = {guild.GuildMarkBgColor},");
						sbQuery.AppendLine($"capacity = {guild.Capacity},");

						sbQuery.AppendLine($"notice = @notice{guild.GuildID},");
						sbQuery.AppendLine($"rank1title = @r1t{guild.GuildID},");
						sbQuery.AppendLine($"rank2title = @r2t{guild.GuildID},");
						sbQuery.AppendLine($"rank3title = @r3t{guild.GuildID},");
						sbQuery.AppendLine($"rank4title = @r4t{guild.GuildID},");
						sbQuery.AppendLine($"rank5title = @r5t{guild.GuildID},");

						sbQuery.AppendLine($"leader = {guild.LeaderID}");
						sbQuery.AppendLine($"WHERE guild_id = {guild.GuildID};");

						sbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.guild_members WHERE guild_id = {guild.GuildID};");

						foreach (var member in guild)
						{
							sbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.guild_members");
							sbQuery.AppendLine($"(character_id, guild_id, guild_rank, guild_points, guild_contribution, alliance_rank) VALUES");
							sbQuery.AppendLine($"({member.dwParentID},{guild.GuildID},{member.GuildRank},{member.GuildPoints},{member.GuildContribution},{member.AllianceRank});");
						}
					}

					using (var cmd = new NpgsqlCommand(sbQuery.ToString(), conn))
					{
						foreach (var guild in this)
						{
							cmd.Parameters.AddWithValue($"name{guild.GuildID}", guild.GuildName);
							cmd.Parameters.AddWithValue($"notice{guild.GuildID}", guild.Notice);
							cmd.Parameters.AddWithValue($"r1t{guild.GuildID}", guild.Rankt1Title);
							cmd.Parameters.AddWithValue($"r2t{guild.GuildID}", guild.Rankt2Title);
							cmd.Parameters.AddWithValue($"r3t{guild.GuildID}", guild.Rankt3Title);
							cmd.Parameters.AddWithValue($"r4t{guild.GuildID}", guild.Rankt4Title);
							cmd.Parameters.AddWithValue($"r5t{guild.GuildID}", guild.Rankt5Title);
						}
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					Log.Error(sbQuery);
					Log.Error(ex.ToString());
				}
			}
		}

		public bool IncreaseGuildCapacity(Character pChar, int nCost)
		{
			// all of these checks happen in the script as well but i'm checking them again incase they try anything funny
			if (GetGuild(pChar) is Guild guild)
			{
				if (guild.Capacity >= 250) return false;
				if (pChar.Stats.nMoney < nCost) return false;

				pChar.Modify.GainMeso(-nCost);
				guild.Capacity += 5;

				guild.Broadcast(CPacket.CGuildMan.IncMaxMemberNum_Done(guild.GuildID, (byte)guild.Capacity));
				return true;
			}
			return false;
		}

		public int GetCapacityIncreaseCost(int dwCharID)
		{
			var nCapacity = GuildCapacity(dwCharID);
			if (nCapacity < 0 || nCapacity > GuildConstants.MAX_GUILD_CAPACITY) return 0;

			if (nCapacity <= 10) return 2_500_000;
			else if (nCapacity <= 15) return 5_000_000;
			else if (nCapacity <= 20) return 15_000_000;
			else if (nCapacity <= 25) return 25_000_000;
			else if (nCapacity <= 30) return 35_000_000;
			else if (nCapacity <= 35) return 45_000_000;
			else return 50_000_000;
		}

		protected override int GetKeyForItem(Guild item) => item.GuildID;

		protected override void RemoveItem(int index)
		{
			if (this[index] is Guild g)
			{
				g.Clear();
			}

			base.RemoveItem(index);
		}
	}
}
