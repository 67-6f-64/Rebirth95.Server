using Autofac;
using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Entities;
using Rebirth.Field.FieldObjects;
using Rebirth.Field.FieldPools;
using Rebirth.Field.MiniRoom;
using Rebirth.Game;
using Rebirth.Network;
using Rebirth.Provider.Template.Map;
using Rebirth.Scripts;
using Rebirth.Scripts.Map;
using Rebirth.Server.Center;
using Rebirth.Server.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using Rebirth.Common.GameLogic;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Field.FieldTypes;
using Rebirth.Provider.Template.Item.Cash;
using Rebirth.Provider.Template.Item.Consume;
using Rebirth.Tools;
using static Rebirth.Game.FieldEffectTypes;

namespace Rebirth.Field
{
	/// <summary>
	/// Main field class.
	/// One of these exists for each field that is active at any given time.
	/// All objects in the field are assigned to a field pool, and the pools are managed by the field class.
	/// </summary>
	public class CField : IDisposable
	{
		public static ILog Log = LogManager.GetLogger(typeof(CField));
		/// <summary>
		/// Game (channel) instance parent
		/// </summary>
		public WvsGame ParentInstance { get; }
		/// <summary>
		/// Non-unique map ID
		/// </summary>
		public int MapId { get; }
		/// <summary>
		/// Instance ID for this map in this game (channel) instance
		/// </summary>
		public int nInstanceID { get; }
		/// <summary>
		/// Map UID for this game (channel) instance
		/// </summary>
		public long dwUniqueId { get; }

		public bool IsInstanced => nInstanceID != 0;
		public bool CompareTo(CField field) => dwUniqueId == field?.dwUniqueId;

		public MapTemplate Template // map data
			=> MasterManager.MapTemplates[MapId];

		/// <summary>
		/// Fetches return field if character dies in map.
		/// Almost always uses return field from map data but 
		///		can be overriden in child classes (special event maps).
		/// </summary>
		public virtual int ReturnMapId => Template.ReturnMap;

		public string OnUserEnterScript => Template.OnUserEnter;
		public string OnFirstUserEnterScript => Template.OnFirstUserEnter;

		public bool HasUserEnterScript => OnUserEnterScript != "";
		public bool HasFirstUserEnterScript => OnFirstUserEnterScript != "";

		public MapScript MapScript { get; private set; } // get active instance, is null if no FUE script exists
		public bool FirstUserEnterScriptActivated { get; private set; } // set to true when first user enters

		public CPortalMap Portals { get; }
		public CFootholdMap Footholds { get; }

		public CUserPool Users { get; }
		public CMobPool Mobs { get; }
		public CNpcPool Npcs { get; }
		public CDropPool Drops { get; }
		public CReactorPool Reactors { get; }
		public CAffectedAreaPool AffectedAreas { get; }
		public CMiniRoomPool MiniRooms { get; }
		public CMessageBoxPool Kites { get; }
		public CTownPortalPool TownPortals { get; }
		public COpenGatePool OpenGates1 { get; }
		public COpenGatePool OpenGates2 { get; }
		public CSummonedPool Summons { get; set; }

		public BlowWeather CurrentWeather { get; }

		public DateTime tFieldTimerExpiration { get; protected set; }

		public int nFieldDeathCount { get; private set; } = -1;

		/// <summary>
		/// Tracks if there is an Acceleration Bot summon in the field
		/// Contains the acceleration bot summon dwId
		/// </summary>
		public int nVelocityControllerdwId { get; set; }
		public bool bPauseSpawn { get; set; }
		/// <summary>
		/// Used to track specific map data.
		/// Don't remove, some scripts access this.
		/// </summary>
		public string QR { get; set; } = ""; // make sure it's initialized

		public CField(WvsGame parentInstance, int nMapId, int nInstanceId)
		{
			ParentInstance = parentInstance;
			MapId = nMapId;

			nInstanceID = nInstanceId;
			dwUniqueId = (nMapId << 4) | nInstanceId;

			Portals = new CPortalMap();
			Footholds = new CFootholdMap();

			Users = new CUserPool(this);
			Mobs = new CMobPool(this);
			Npcs = new CNpcPool(this);
			Drops = new CDropPool(this);
			Reactors = new CReactorPool(this);
			AffectedAreas = new CAffectedAreaPool(this);
			Kites = new CMessageBoxPool(this);
			TownPortals = new CTownPortalPool(this);
			MiniRooms = new CMiniRoomPool(this);
			Summons = new CSummonedPool(this);

			OpenGates1 = new COpenGatePool(this, true);
			OpenGates2 = new COpenGatePool(this, false);

			CurrentWeather = new BlowWeather(this);
		}

		/// <summary>
		/// Returns true if map has no users and no active minirooms.
		/// </summary>
		/// <returns></returns>
		public virtual bool CanBeDestroyed() =>
			Users.Count <= 0
			&& MiniRooms.ShopCount <= 0
			&& !Mobs.bHasIntervalMobs;

		/// <summary>
		/// Sends migratein packet depending on character migration status.
		/// Triggers OnUserEnter() for child classes
		/// Notifies proper social channels
		/// </summary>
		/// <param name="c"></param>
		public void AddClient(WvsGameClient c)
		{
			var character = c.Character;

			var bSendPartyMapChangeUpdate = c.MigratedIn;

			if (Template.HasNoPetLimit())
			{
				character.Stats.aliPetLockerSN = new long[3]; // clear pets
				character.Pets.Dispose();
			}

			if (Template.HasSummonLimit())
			{
				MasterManager.SummonStorage.Retrieve(character.dwId); // clear incoming summons
			}

			if (c.MigratedIn)
			{
				var p = CPacket.SetField(character, c.ChannelId, c.WorldID);
				c.SendPacket(p);
			}
			else
			{
				c.MigratedIn = true;

#if DEBUG
				character.Stats.nPortal = 0; //Spawn everyone in same place
#else
                var sp = Portals.GetRandStartPoint();

                if(sp != null)
                {
                    var foothold = Footholds.FindBelow(sp.nX, sp.nY);

                    if (foothold != null)
                    {
                        character.Stats.nPortal = (byte)sp.nIdx;
                        character.Position.X = foothold.X1;
                        character.Position.Y = foothold.Y1;
                        character.Position.Foothold = foothold.Id;
                    }
                }
#endif

				//TODO: Refine our flags for production
				var dbFlag = (DbCharFlags)0;

				//dbFlag |= DbCharFlags.ALL;

				dbFlag |= DbCharFlags.CHARACTER;
				dbFlag |= DbCharFlags.MONEY;
				dbFlag |= DbCharFlags.INVENTORYSIZE;
				dbFlag |= DbCharFlags.ITEMSLOTEQUIP;
				dbFlag |= DbCharFlags.ITEMSLOTCONSUME;
				dbFlag |= DbCharFlags.ITEMSLOTINSTALL;
				dbFlag |= DbCharFlags.ITEMSLOTETC;
				dbFlag |= DbCharFlags.ITEMSLOTCASH;
				dbFlag |= DbCharFlags.SKILLRECORD;

				dbFlag |= DbCharFlags.SKILLCOOLTIME;
				dbFlag |= DbCharFlags.QUESTRECORD;
				dbFlag |= DbCharFlags.QUESTCOMPLETE;

				//dbFlag |= DbCharFlags.MINIGAMERECORD;
				dbFlag |= DbCharFlags.COUPLERECORD;

				dbFlag |= DbCharFlags.MONSTERBOOKCOVER;
				dbFlag |= DbCharFlags.MONSTERBOOKCARD;

				dbFlag |= DbCharFlags.QUESTRECORDEX;

				if (JobLogic.IsWildhunterJob(character.Stats.nJob))
				{
					dbFlag |= DbCharFlags.WILDHUNTERINFO;
				}

				dbFlag |= DbCharFlags.MAPTRANSFER;

				c.SendPacket(CPacket.SetField(character, c.ChannelId, c.WorldID, Constants.LogoutGift, dbFlag));
			}

			OnUserEnter(c.Character);

			if (bSendPartyMapChangeUpdate)
			{
				c.Character.NotifySocialChannels(SocialNotiflag.ChangeMap);
			}
		}

		/// <summary>
		/// Is called before a player enters the field if there are no other characters in the field.
		/// </summary>
		protected virtual void Init()
		{
			// not used in base class
		}

		/// <summary>
		/// Adds user to user pool.
		/// Sends map scripts if they exists.
		/// </summary>
		/// <param name="pUser"></param>
		protected virtual void OnUserEnter(Character pUser)
		{
			if (Users.Count <= 0) Init();

			Users.Add(pUser);

			var provider = ServerApp.Container.Resolve<ScriptManager>();

			if (HasFirstUserEnterScript && !FirstUserEnterScriptActivated)
			{
#if DEBUG
				Log.Debug($"Begin FUE script in CField ({OnFirstUserEnterScript})");
#endif
				// script already executed -> should not be possible
				if (MapScript != null)
				{
					throw new ScriptException($"First user enter script has already been activated. MapId: {MapId}. Script Name {OnFirstUserEnterScript}");
				}

				MapScript = provider.GetMapScript(OnFirstUserEnterScript, this, pUser.Socket);
				MapScript.Execute();
				FirstUserEnterScriptActivated = true;
#if DEBUG
				Log.Debug("End FUE script in CField");
#endif
			}
			else if (HasUserEnterScript)
			{
#if DEBUG
				Log.Debug($"Executing UE script in CField ({OnUserEnterScript})");
#endif
				provider.GetMapScript(OnUserEnterScript, pUser.Socket).Execute();
			}
		}

		/// <summary>
		/// Removes user from user pool
		/// Is primarily called from WvsGame.OnDisconnect and CharacterActions.SetField
		/// </summary>
		/// <param name="pUser"></param>
		/// <param name="bMigrateOut"></param>
		public virtual void OnUserLeave(Character pUser, bool bMigrateOut = false)
		{
			Users.Remove(pUser);
		}

		/// <summary>
		/// Handles HP/MP/EXP reduction on death.
		/// Cancels all buffs.
		/// Warps player to return map.
		/// </summary>
		/// <param name="user"></param>
		public virtual void OnUserWarpDie(Character user, bool bLoseExp = true)
		{
			user.StatisticsTracker.nDeaths += 1;
			if (nFieldDeathCount > 0)
			{
				UpdateDeathCount(--nFieldDeathCount);
			}

			if (Template.HasNoExpDecrease() || !bLoseExp)
			{
				user.Modify.Stats(ctx =>
				{
					ctx.HP = user.BasicStats.nMHP;
					ctx.MP = user.BasicStats.nMMP;
				});
			}
			else
			{
				const int nCharmId = 5130000;

				if (InventoryManipulator.RemoveQuantity(user, nCharmId, 1))
				{
					// TODO use real notification packet for this
					user.SendMessage($"A safety charm has been used to avoid losing exp!");
					user.Modify.Heal((int)(user.BasicStats.nMHP * 0.33), (int)(user.BasicStats.nMMP * 0.33));
				}
				else
				{
					user.Modify.Stats(ctx =>
					{
						ctx.decrease_exp(user.Field.Template.Town || user.Field.Mobs.aMobGen.Count == 0);
						ctx.HP = 1;
						ctx.MP = 1;
					});
				}
			}

			user.Buffs.CancelAllBuffs();

			user.Action.SetField(user.Field.ReturnMapId);
		}

		/// <summary>
		/// Warps player to the map associated with the given portal.
		/// No positional validation is done; if the portal exists, the player will be warped through it.
		/// Has extra handling in child classes.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="sPortalName"></param>
		public virtual void OnUserEnterPortal(Character user, string sPortalName)
		{
			var portal = Portals.FindPortal(sPortalName);

			if (portal == null)
			{
				user.Action.Enable();
				Log.WarnFormat("Client tried to enter non existant portal {0}", sPortalName);
				return;
			}

			var nFieldID = portal.nTMap;

			if (portal.nTMap != 999999999 || CFieldMan.IsFieldValid(nFieldID))
			{
				var newField = user.Socket.Server.CFieldMan.GetField(nFieldID);

				var spawn = newField.Portals.FindPortal(portal.sTName) ?? newField.Portals.FindPortal(0);

				var foothold = newField.Footholds.FindBelow(spawn.nX, (short)(spawn.nY - 25));

				if (nInstanceID != 0)
				{
					user.Action.SetFieldInstance(nFieldID, nInstanceID, (byte)spawn.nIdx, foothold?.Id ?? 0);
				}
				else
				{
					user.Action.SetField(nFieldID, (byte)spawn.nIdx, foothold?.Id ?? 0);
				}
			}
			else
			{
				user.Action.Enable();
				Log.WarnFormat("Client tried to enter portal {0} with bad dest {1}", sPortalName, nFieldID);
			}
		}

		public virtual void OnUserEnterScriptedPortal(Character user, string sPortal)
		{
			var sPortalScriptName = Template.PortalByPortalName(sPortal)?.Script.ToLower();

			if (sPortalScriptName is null) return;

			var provider = ServerApp.Container.Resolve<ScriptManager>();

			provider.GetPortalScript(sPortalScriptName, user.Socket).Execute(); // maybe check for already active NPC scripts first.. idk
			user.Action.Enable();
		}

		/// <summary>
		/// Is called before a mob is inserted into the mob pool.
		/// </summary>
		/// <param name="mob"></param>
		public virtual void OnInsertMob(CMob mob)
		{
			// not used in base class
		}

		/// <summary>
		/// Is mainly used for child classes to control mob death events.
		/// If the mob is dead (<= 0 HP) then OnMobDead function is called.
		/// </summary>
		/// <param name="removedMob"></param>
		public virtual void OnMobDie(CMob removedMob)
		{
			if (removedMob.Dead)
			{
				removedMob.OnMobDead();
			}
		}

		/// <summary>
		/// Not used in the base class, only used in certain child classes.
		/// Return true if mob HP bar should be displayed, otherwise return false.
		/// Mob has already been damaged when this function is called.
		/// </summary>
		/// <param name="mob">The damaged mob.</param>
		/// <param name="nDamage">Final damage done to mob.</param>
		/// <returns></returns>
		public virtual bool OnMobDamaged(CMob mob, int nDamage)
		{
			return true;
		}

		/// <summary>
		/// Performs consume on pickup logic and inserts item into inventory
		///		if it fits. Will return false if item does not fit.
		/// Some field child classes change drop pickup behavior.
		/// If returns true, the drop pool will remove the drop and display
		///		proper drop pickup messages, else calling function will return.
		/// </summary>
		/// <param name="pUser">Character picking up the item</param>
		/// <param name="pDrop">Item being picked up</param>
		public virtual bool TryDropPickup(Character pUser, CDrop pDrop)
		{
			if (pDrop.Item.Template is ConsumeItemTemplate template)
			{
				if (template.MonsterBook)
				{
					pUser.MonsterBook.AddCard(pDrop.ItemId);
					pUser.SendMessage("You have found a monster book card!");
					pUser.Modify.GainNX(1000);

					return false;
				}

				if (template.ConsumeOnPickup)
				{
					pUser.Buffs.AddItemBuff(template.TemplateId);
					return false;
				}
			}

			return InventoryManipulator.InsertInto(pUser, pDrop.Item) > 0;
		}

		/// <summary>
		/// Triggered when a player takes damage from a mob.
		/// Will not trigger from map damage or misses.
		/// </summary>
		/// <param name="pUser">Player taking damage</param>
		/// <param name="pMob">Mob damaging player</param>
		/// <param name="nDamage">Final damage amount that player took after any reductions</param>
		public virtual void OnUserDamaged(Character pUser, CMob pMob, int nDamage)
		{
			// not used in base class
		}

		/// <summary>
		/// Used for player weather requests.
		/// Item ID validation is performed.
		/// </summary>
		/// <param name="nItemID">Requested item ID</param>
		/// <param name="sMsg">Message to display on screen</param>
		/// <returns>Returns false if request is denied</returns>
		public bool TryAddWeatherEffect(int nItemID, string sMsg)
		{
			if (!CurrentWeather.StartTime.AddedSecondsExpired(CurrentWeather.Duration)) return false;

			if (MasterManager.ItemTemplate(nItemID) is CashItemTemplate template)
			{
				CurrentWeather.UpdateItemInfo(nItemID, sMsg, template.WeatherType);
			}
			return true;
		}

		/// <summary>
		/// Used to force the weather effect on the map.
		/// Item validation is done.
		/// Will override existing weather effect.
		/// </summary>
		/// <param name="nItemID">Weather effect item ID</param>
		/// <param name="sMsg">Message to display on screen</param>
		protected void ForceWeatherEffect(int nItemID, string sMsg)
		{
			if (MasterManager.ItemTemplate(nItemID) is CashItemTemplate template)
			{
				CurrentWeather.UpdateItemInfo(nItemID, sMsg, template.WeatherType);
			}
		}

		//--------------------------------------------------

		/// <summary>
		/// Sends a packet to all players in the field
		/// </summary>
		/// <param name="packet">Packet to send</param>
		public void Broadcast(COutPacket packet)
		{
			using (packet)
			{
				foreach (var client in Users)
				{
					client.SendPacket(packet);
				}
			}
		}

		/// <summary>
		/// Sends a packet to all players in field
		/// </summary>
		/// <param name="packet">Packet to send</param>
		/// <param name="excludes">Exception list</param>
		public void Broadcast(COutPacket packet, params WvsGameClient[] excludes)
		{
			using (packet)
			{
				foreach (var client in Users)
				{
					if (!excludes.Contains(client.Socket))
						client.SendPacket(packet);
				}
			}
		}

		/// <summary>
		/// Sends a packet to all players in field
		/// </summary>
		/// <param name="packet">Packet to send</param>
		/// <param name="excludes">Exception list</param>
		public void Broadcast(COutPacket packet, params Character[] excludes)
		{
			using (packet)
			{
				foreach (var client in Users)
				{
					if (!excludes.Contains(client))
						client.SendPacket(packet);
				}
			}
		}

		//--------------------------------------------------

		/// <summary>
		/// Try to broadcast existing weather effect to character.
		/// Does not send anything if weather effect is not active.
		/// </summary>
		/// <param name="c"></param>
		public void SendActiveWeatherEffect(Character c)
		{
			// less taxing to check item id cuz it gets reset when the effect is over
			if (CurrentWeather.nItemID > 0)
			{
				CurrentWeather.SendWeatherEffect(c);
			}
		}

		/// <summary>
		/// Send all drops currently in map to character.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnDrops(Character c)
		{
			foreach (var drop in Drops)
			{
				c.SendPacket(drop.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send mob enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnMobs(Character c)
		{
			foreach (var mob in Mobs)
			{
				mob.Position.Y -= 10; // offset so mobs dont fall thru map
				c.SendPacket(mob.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send npc enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnNpcs(Character c)
		{
			var imitateNPCs = Npcs.OfType<CNpcImitate>().ToArray();
			var normalNPCS = Npcs.Except(imitateNPCs);

			foreach (var npc in normalNPCS)
			{
				c.SendPacket(npc.MakeEnterFieldPacket());
			}

			if (imitateNPCs.Length > 0)
			{
				c.SendPacket(CPacket.ImitatedNPCData(imitateNPCs));
			}
		}

		/// <summary>
		/// Send reactor enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnReactors(Character c)
		{
			foreach (var reactor in Reactors)
			{
				c.SendPacket(reactor.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send summon enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnSummons(Character c)
		{
			foreach (var summon in Summons)
			{
				c.SendPacket(summon.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send affected area enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnAffectedAreas(Character c)
		{
			foreach (var area in AffectedAreas)
			{
				c.SendPacket(area.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send miniroom enter field packets to new character entering field.
		/// Only sends Entrusted Shop, others are currently unhandled.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnMinirooms(Character c)
		{
			var shops = MiniRooms.OfType<CEntrustedShop>();

			foreach (var shop in shops)
			{
				c.SendPacket(shop.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send kite enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnKites(Character c)
		{
			foreach (var kite in Kites)
			{
				c.SendPacket(kite.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send town portal enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnTownPortals(Character c)
		{
			foreach (var portal in TownPortals) // TODO reconsider how this stuff is handled
			{
				c.SendPacket(portal.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send open gate enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnOpenGates(Character c)
		{
			foreach (var gate in OpenGates1) // first gate
			{
				c.SendPacket(gate.MakeEnterFieldPacket());
			}

			foreach (var gate in OpenGates2) // second gate
			{
				c.SendPacket(gate.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Sends dragon enter field packets to new character entering field.
		/// </summary>
		/// <param name="c"></param>
		public void SendSpawnDragons(Character c)
		{
			foreach (var user in Users)
			{
				if (user == c)
					continue;

				if (user.Dragon != null)
					c.SendPacket(user.Dragon.MakeEnterFieldPacket());
			}
		}

		/// <summary>
		/// Send currently active stalkers (literal character followers).
		/// </summary>
		/// <param name="c"></param>
		public void SendStalkees(Character c)
		{
			var toAdd = new List<Stalkee>();

			foreach (var user in Users)
			{
				toAdd.Add(new Stalkee()
				{
					dwId = user.dwId,
					sName = user.Stats.sCharacterName,
					bRemove = false,
					nX = user.Position.X,
					nY = user.Position.Y
				});
			}

			c.SendPacket(CPacket.StalkResult(toAdd));
		}

		//--------------------------------------------------

		/// <summary>
		/// Resets controller for all mobs that have given char ID as their controller.
		/// Resets both mob and NPC controllers.
		/// Controllees get a new random controller assigned automatically.
		/// </summary>
		/// <param name="dwCharId"></param>
		public void RemoveController(int dwCharId)
		{
			foreach (var pMob in Mobs)
			{
				if (pMob.Controller?.dwId == dwCharId)
				{
					pMob.SetController(Users.Random(), MobCtrlType.Active_Int);
				}
			}

			foreach (var pNpcs in Npcs)
			{
				if (pNpcs.Controller?.dwId == dwCharId)
					pNpcs.SetController(null);
			}

			AssignControllerNpcs();
		}

		/// <summary>
		/// Sets an NPC controller to the given character.
		/// Sends proper notification packets to new and previous characters
		/// </summary>
		/// <param name="pUser"></param>
		/// <param name="pNpc"></param>
		public void ChangeNpcController(Character pUser, CNpc pNpc)
		{
			var pOldUser = pNpc.Controller;

			if (pUser is null)
			{
				pUser = Users.Random();
				if (pUser is null) return; // no chars in map
			}

			if (pOldUser?.dwId != pUser.dwId) //TODO: Confirm this doesnt glitch
			{
				pOldUser?.SendPacket(CPacket.NpcChangeController(pNpc, 0));

				byte nLevel = 1;
				pNpc.SetController(pUser);
				pUser.SendPacket(CPacket.NpcChangeController(pNpc, nLevel));
			}
		}

		/// <summary>
		/// Assigns controllers to all mobs in the field that do not currently have one.
		/// </summary>
		public void AssignControllerMobs()
		{
			foreach (var pMob in Mobs)
			{
				if (pMob.Controller == null)
				{
					var pUser = Users.Random(); //TODO: Set default controller to lowest used client

					if (pUser != null)
					{
						//ChangeMobController(pUser, pMob, false);
						pMob.SetController(pUser, MobCtrlType.Active_Int);
					}
				}
			}
		}

		/// <summary>
		/// Asssigns controllers to all NPCs in the field that do not currently have one.
		/// </summary>
		public void AssignControllerNpcs()
		{
			foreach (var pNPC in Npcs)
			{
				if (pNPC.Controller != null) continue;

				//TODO: Set default controller to lowest used client
				var pUser = Users.Random();

				if (pUser != null)
				{
					ChangeNpcController(pUser, pNPC);
				}
			}
		}

		//--------------------------------------------------

		/// <summary>
		/// Updates all field objects.
		/// </summary>
		public virtual void Update()
		{
			Mobs.Update();

			Reactors.RedistributeLife();

			Drops.Update();

			if (tFieldTimerExpiration != DateTime.MinValue && tFieldTimerExpiration.SecondsUntilEnd() <= 0)
			{
				OnClockEnd();
			}

			foreach (var user in Users)
			{
				user.Update(this);
			}

			// less taxing to check item id cuz it gets reset when the effect is over
			if (CurrentWeather.nItemID > 0 && CurrentWeather.StartTime.AddedSecondsExpired(CurrentWeather.Duration))
			{
				CurrentWeather.Clear();
			}

			MiniRooms.Update();
			Kites.Update();
			TownPortals.Update();
			OpenGates1.Update();
			OpenGates2.Update();
			AffectedAreas.Update();
			Summons.Update();

			if (Template.DecHP > 0 || Template.DecMP > 0)
			{
				// TODO
				// CUserLocal::OnNotifyHPDecByField(this, iPacket);
			}
		}

		/// <summary>
		/// Clears all higher and lower object pools.
		/// Field will be ready for disposal and can not be used after this is called.
		/// </summary>
		public void Dispose()
		{
			Users.Dispose();
			Npcs.Dispose();
			Footholds.Dispose();
			Portals.Dispose();
			Mobs.Dispose();
			Reactors.Dispose();
			Drops.Dispose();
			AffectedAreas.Dispose();
			MiniRooms.Dispose();
			Kites.Dispose();
			TownPortals.Dispose();
			OpenGates1.Dispose();
			OpenGates2.Dispose();
			CurrentWeather.Dispose();
		}

		/// <summary>
		/// Clears all lower object pools and resets all higher object pools.
		/// Map can still be used after this is called.
		/// DO NOT CALL DISPOSE FROM INSIDE THIS FUNCTION
		/// </summary>
		protected virtual void Reset(bool bFromDispose = false)
		{
			if (Users.Count > 0) // not good
				throw new InvalidOperationException("Trying to reset map that still has users in it.");

			QR = "";
			nFieldDeathCount = -1;
			tFieldTimerExpiration = DateTime.MinValue;

			Mobs.Reset();
			Reactors.Reset();

			Drops.Clear();
			AffectedAreas.Clear();
			MiniRooms.Clear();
			Kites.Clear();
			TownPortals.Clear();
			OpenGates1.Clear();
			OpenGates2.Clear();
			CurrentWeather.Clear();
		}

		/// <summary>
		/// Adds a clock to the field and notifies all clients to display on screen.
		/// </summary>
		/// <param name="tDurationSec"></param>
		public void CreateFieldClock(int tDurationSec)
		{
			if (tDurationSec < 0) tDurationSec = 0;

			tFieldTimerExpiration = DateTime.Now.AddSeconds(tDurationSec + 2);
			Broadcast(CPacket.CreateClock((int)tFieldTimerExpiration.SecondsUntilEnd()));
		}

		/// <summary>
		/// Called when clock ends.
		/// Notifies all clients that clock has ended and to remove it from screen.
		/// Has more handling in child classes.
		/// </summary>
		protected virtual void OnClockEnd()
		{
			Broadcast(CPacket.DestroyClock());
			tFieldTimerExpiration = DateTime.MinValue;
		}

		/// <summary>
		/// Updates and broadcasts death count to players in field. Set negative to remove death counter.
		/// </summary>
		/// <param name="nCount">Number of deaths remaining.</param>
		public void UpdateDeathCount(int nCount)
		{
			if (nCount < 0 || nCount > 99) nCount = -1;
			nFieldDeathCount = nCount;
			//Broadcast(CPacket.Custom.FieldDeathCount(nCount));
		}

		/// <summary>
		/// Displays a map effect on the screen to all players.
		/// </summary>
		/// <param name="sDir"></param>
		public void BroadcastWZMapEffect(string sDir)
		{
			new FieldEffectPacket(FieldEffect.Screen)
			{ sName = sDir }
			.Broadcast(this);
		}

		/// <summary>
		/// Sends a chat box notice to all characters in the field.
		/// </summary>
		/// <param name="sMsg"></param>
		public void BroadcastNotice(string sMsg)
		{
			Broadcast(CPacket.SystemMessage(sMsg));
		}

		/// <summary>
		/// Is encoded at the tail end of OnUserEnterField packet for certain field child classes.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="c"></param>
		public virtual void EncodeFieldSpecificData(COutPacket p, Character c)
		{
			// not used in base class
		}

		/// <summary>
		/// Warps all characters in this field to a new map
		/// Leave instance as default to keep the same instance as this field
		/// </summary>
		/// <param name="nMapID">Target map</param>
		/// <param name="nInstanceUID">Target instance.</param>
		public void WarpMapTo(int nMapID, byte nPortalID = 0, short nFH = 0, int nInstanceUID = -1)
		{
			if (nInstanceUID < 0) nInstanceUID = nInstanceID;

			foreach (var user in Users.ToArray())
			{
				user.Action.SetFieldInstance(nMapID, nInstanceUID, nPortalID, nFH);
			}
		}
	}
}
