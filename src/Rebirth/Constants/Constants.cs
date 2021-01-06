using System;
using Microsoft.Extensions.Configuration;
using Rebirth.Characters;
using Rebirth.Entities;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Server.Center;

namespace Rebirth
{
	public static class Constants
	{
		// ----------------------------------------------------------------\\
		// Below variables are meant to be configured                      ||
		// ----------------------------------------------------------------//

		public const string ClientVersion = "Build v9.5 Tespia";
		public const string ServerMessage = "The main hub has moved from Henesys to the Free Market. You can click the Maple button to access the Rebirth Menu for an easy teleport!";

#if DEBUG
		public static readonly byte[] ServerAddress = new byte[] { 127, 0, 0, 1 }; // local ip
		//public static readonly byte[] ServerAddress = new byte[] { 10, 0, 0, 127 }; // local ip
#else
        public static readonly byte[] ServerAddress = new byte[] { 178, 63, 94, 190 }; // vps ip
#endif

		public static readonly byte MaxCharSlot = 15; // not the default, just the max
		public static readonly bool AutoMaxSkillMastery = true;
		public static readonly bool AutoJobAdvance = true;

		public static bool DisplayPacketsInConsole =
#if DEBUG
		true;
#else
        false;
#endif

		public static readonly bool AllowAccountLoginOverride = true;

		public static readonly bool LogNxAttributes = false; // this will create log files of all the game data attributes that get scraped
		public static readonly bool AutoRegister = true;
		public static readonly bool AutoLogin = false;

		public static readonly string AutoLoginUsername = "admin";
		public static readonly string AutoLoginPassword = "admin";

		public static string GameDataPath => MainFilePath + @"\data\";
		public static string GameDataNXPath => MainFilePath + @"\data\nx\";
		public static string ScriptsFolderPath => MainFilePath + @"\scripts\";
		public static string EmojiFolderPath => MainFilePath + @"\data\emoji\";

		//public static string DropDataPath => MainFilePath + @"\data\CurrentDropData.txt";
		//public static string RewardDataPath => MainFilePath + "\\data\\Reward_OutPut.txt";

		public static readonly CLoginBalloon[] Balloons =
		{
			new CLoginBalloon()
			{
				nX = 130,
				nY = 275,
				sMessage = "Welcome to      Rebirth Alpha!          Please report  bugs in our       discord."
			},
		};


		// ----------------------------------------------------------------\\
		// Below variables are not really meant to be configured           ||
		// ----------------------------------------------------------------//

		public static readonly Random Rand = new Random();
		public static readonly LogoutGiftConfig LogoutGift = new LogoutGiftConfig();

		public const int ServerThreadFPS = 15;
		public const int GlobalUpdateDelay = 1500; // milliseconds - Party HP Update, TV, avatar mega (1.5 sec, this needs to happen frequently)
		public const int KeepAliveDelay = 15 * 1000; // milliseconds - Ping Packet Interval

		public const ushort VersionMajor = 95;
		public const string VersionMinor = "1";
		public const byte VersionLocale = 8;

		public const int LoginPort = 8484;
		public const int GamePort = 8585;
		public const int ShopPort = 8787;

		public const int MigrateTimeoutInterval = 15;
		public const int PartyUpdateInterval = 1500; // milliseconds
		public const int FieldDestroyInterval = 3 * 60 * 1000; // milliseconds

		public const int InvalidMap = 999999999;
		public const byte MaxCharNameLength = 13; // for internal verification only
		public const byte MaxChatMessageLength = 127; // for internal verification only
		public const byte MaxAdBoardLength = 127;

		public static int MEGAPHONE_LEVEL_LIMIT = 15;

		public const int DB_ITEMSTORAGE_SLOTMIN = 10000; // items in a slot within this range will be considered inside of a temp inv
		public const int DB_ITEMSTORAGE_SLOTMAX = 10100;

		public const bool MULTIPET_ACTIVATED = false;

		public const int DAMAGE_CAP = 5_000_000;

		// TODO move custom spawn stuff to RateConstants

		public const bool CustomSpawn_Enabled = true;
		/// <summary>
		/// Seconds between mob spawns
		/// </summary>
		public const int MobSpawnInterval = CustomSpawn_Enabled ? 8 : 5; // BMS is 7
		/// <summary>
		/// Highest chance a mob will spawn on a non-wall FH.
		/// This doesn't change the number of mobs that will spawn in a map,
		///     this is just used to make sure all the footholds get 
		///     iterated before the spawn limit is reached
		/// </summary>
		public const float CustomerSpawn_OddsLimit = 0.02f;
		/// <summary>
		/// Max slope a foothold can have in order for a mob to spawn on it.
		/// </summary>
		public const float CustomSpawn_MaxSlope = 0.45f;
		public const float MobDeathRoulette_Odds = 0.02f;
		public const int MaxCharLevel = 200;

		//-----------------------------------------------------------------
		// Don't change the below unless you are refactoring
		//-----------------------------------------------------------------
		private static int DB_Connection_Pool_Size = 256;
		public static string MainFilePath => WvsCenter.Config.GetValue<string>("mainfilepath");
		public static string DB_All_World_Schema_Name => WvsCenter.Config.GetValue<string>("world0_schema"); // rebirth
		private static string DB_World0_Host => WvsCenter.Config.GetValue<string>("world0_host");
		private static string DB_World0_Port => WvsCenter.Config.GetValue<string>("world0_port"); // 5432
		private static string DB_World0_Username => WvsCenter.Config.GetValue<string>("world0_username");
		private static string DB_World0_Password => WvsCenter.Config.GetValue<string>("world0_password");
		public static string DB_World0_Database => WvsCenter.Config.GetValue<string>("world0_database"); // world0_database

		public static string DB_World0_ConString = $"Host={DB_World0_Host};Port={DB_World0_Port};Username={DB_World0_Username};Password={DB_World0_Password};Database={DB_World0_Database};Pooling=true;MinPoolSize=0;MaxPoolSize={DB_Connection_Pool_Size};";

		/// <summary>
		/// This DB holds the account information and logging related to the website
		/// </summary>
		public static string DB_Global_Schema => WvsCenter.Config.GetValue<string>("global_schema");
		private static string DB_Global_Host => WvsCenter.Config.GetValue<string>("global_host");
		private static string DB_Global_Port => WvsCenter.Config.GetValue<string>("global_post");
		private static string DB_Global_Username => WvsCenter.Config.GetValue<string>("global_username");
		private static string DB_Global_Password => WvsCenter.Config.GetValue<string>("global_password");
		public static string DB_Global_Database => WvsCenter.Config.GetValue<string>("global_database"); // rebirth_global

		public static string DB_Global_ConString = $"Host={DB_Global_Host};Port={DB_Global_Port};Username={DB_Global_Username};Password={DB_Global_Password};Database={DB_Global_Database};Pooling=true;MinPoolSize=0;MaxPoolSize={DB_Connection_Pool_Size};";

		//-----------------------------------------------------------------
		// Time
		//-----------------------------------------------------------------

		public const long MAX_TIME = 150842304000000000L;
		public const long ZERO_TIME = 94354848000000000L;
		public const long PERMANENT = 150841440000000000L;
		public const long FT_UT_OFFSET = 116444592000000000L; // EDT
		public const long DEFAULT_TIME = 150842304000000000L;// 00 80 05 BB 46 E6 17 02

		public static int[] DailyQuestRecordIDs = // not currently used
		{
			7001, // zakum
			7002, // chaos zakum
			7003, // horntail
			7004, // chaos horntail
		};

		public static long Time(long realTimestamp)
		{
			if (realTimestamp == -1)
			{
				return DEFAULT_TIME;// high number ll
			}
			else if (realTimestamp == -2)
			{
				return ZERO_TIME;
			}
			else if (realTimestamp == -3) { return PERMANENT; }
			return realTimestamp * 10000 + FT_UT_OFFSET;
		}

		//-----------------------------------------------------------------

		public static bool FilterRecvOpCode(RecvOps recvOp)
		{
			if (!DisplayPacketsInConsole)
				return true;

			switch (recvOp)
			{
				case RecvOps.CP_CreateSecurityHandle:
				case RecvOps.CP_UpdateScreenSetting:
				case RecvOps.CP_AliveAck:
				case RecvOps.CP_ClientDumpLog:
				case RecvOps.CP_ExceptionLog:
				case RecvOps.CP_UserMove:
				case RecvOps.CP_MobMove:
				case RecvOps.CP_NpcMove:
				case RecvOps.CP_DragonMove:
				case RecvOps.CP_UserChangeStatRequest:
				case RecvOps.CP_UserEmotion:
				//case RecvOps.CP_CustomSecurityAlive:
				////case RecvOps.CP_UserHit:
				//case RecvOps.CP_RequireFieldObstacleStatus:
				//case RecvOps.CP_PetMove:
				//case RecvOps.CP_SummonedMove:
				//case RecvOps.CP_CANCEL_INVITE_PARTY_MATCH:
				//case RecvOps.CP_MobApplyCtrl:
				//case RecvOps.CP_UserQuestRequest:
					return true;
			}
			return false;
		}

		public static bool FilterSendOpCode(SendOps sendOp)
		{
			if (!DisplayPacketsInConsole)
				return true;

			switch (sendOp)
			{
				case SendOps.LP_AliveReq:
				case SendOps.LP_MobCtrlAck:
				case SendOps.LP_UserMove:
				case SendOps.LP_MobMove:
				case SendOps.LP_NpcMove:
				case SendOps.LP_SummonedMove:
				case SendOps.LP_DragonMove:
				case SendOps.LP_StatChanged:
				case SendOps.LP_UserEmotion:
				case SendOps.LP_UserHP: // party hp update
				case SendOps.LP_MobHPIndicator: // party hp update
												//case SendOps.LP_Message:
				case SendOps.LP_NpcEnterField:
				case SendOps.LP_NpcChangeController:
				case SendOps.LP_MobEnterField:
				case SendOps.LP_MobChangeController:
				//case SendOps.LP_CustomSecurity:
				//case SendOps.LP_CustomEmoji:
				//case SendOps.LP_CustomGainNX:
					return true;
			}
			return false;
		}

		//\\   Startup Constants   //\\

		public static string[] DATA_FILES =
		{
			"Base",
			"Character",
			"Effect",
			"Etc",
			"Item",
			"Map",
			"Mob",
			"Morph",
			"Npc",
			"Quest",
			"Reactor",
			"Server",
			"Skill",
			"Sound",
			"String",
			"TamingMob",
			"UI",
		};

		public static string DATA_FILE_EXTENSION = ".nx";
	}
}
