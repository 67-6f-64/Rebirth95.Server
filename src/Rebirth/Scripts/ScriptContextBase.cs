using log4net;
using Rebirth.Characters;
using Rebirth.Characters.Modify;
using Rebirth.Common.Types;
using Rebirth.Entities;
using Rebirth.Entities.Item;
using Rebirth.Field;
using Rebirth.Field.FieldObjects;
using Rebirth.Game;
using Rebirth.Server.Center;
using System;
using System.Collections.Generic;
using System.Linq;
using Rebirth.Characters.Quest;
using Rebirth.Characters.Stat;
using Rebirth.Field.FieldTypes;
using Rebirth.Provider.Template.Item.Cash;

namespace Rebirth.Scripts
{
	public abstract class ScriptContextBase<TScript> : IDisposable where TScript : ScriptBase
	{
		public ILog Log = LogManager.GetLogger(typeof(ScriptContextBase<TScript>));

		public TScript Script { get; }
		public Character Character => Script.Parent.Character;
		public CField Field => Character.Field;
		/// <summary>
		/// Used to determine 
		/// </summary>
		public bool IsNpcScript { get; set; }

		protected ScriptContextBase(TScript script)
		{
			Script = script;
		}

		public string ScriptName => Script.ScriptName;

		//------------------General Script Methods Go Here------------------
		//
		// -------------- Character Variables
		//
		public string PlayerName => Character.Stats.sCharacterName;
		public string PlayerAccountName => Character.Account.Username;

		public long MerchantMesos => Character.Stats.MerchantMesos;

		public int PlayerAccountId => Character.Account.ID;
		public int PlayerId => Character.dwId;

		public bool PlayerInGuild => MasterManager.GuildManager.GetGuildID(PlayerId) != 0;
		public bool PlayerIsGuildMaster => MasterManager.GuildManager.IsGuildMaster(PlayerId);
		public int PlayerGuildSize => MasterManager.GuildManager.GuildCapacity(PlayerId);
		public int GuildCapacityIncreaseCost => MasterManager.GuildManager.GetCapacityIncreaseCost(PlayerId);

		public int PlayerMapId => Character.Field.MapId;
		public long PlayerFieldUID => Character.Field.dwUniqueId;
		public int PlayerMeso => Character.Stats.nMoney;

		public int RebirthPoints => Character.Account.RebirthPoints;
		public int VotePoints => Character.Account.VotePoints;

		public short PlayerAP => Character.Stats.nAP;
		public short PlayerSP => Character.Stats.nSP;
		public short PlayerPosX => Character.Position.X;
		public short PlayerPosY => Character.Position.Y;
		public short FriendListLimit => Character.Stats.nFriendMax;
		public short PlayerJob => Character.Stats.nJob;
		public short PlayerSubJob => Character.Stats.nSubJob;

		public short PlayerSTR => Character.Stats.nSTR;
		public short PlayerDEX => Character.Stats.nDEX;
		public short PlayerINT => Character.Stats.nINT;
		public short PlayerLUK => Character.Stats.nLUK;

		public byte PlayerGender => Character.Stats.nGender;
		public byte PlayerLevel => Character.Stats.nLevel;
		public int PlayerHair => Character.Stats.nHair;
		public int PlayerFace => Character.Stats.nFace;
		public int PlayerSkin => Character.Stats.nSkin;

		public bool PlayerAdmin => Character.Account.AccountData.Admin > 0;
		public bool PlayerInParty => Character.Party != null;
		public bool PlayerPartyLeader => PlayerInParty && Character.Party.dwOwnerId == PlayerId;
		public int PlayerPartyMemberCount => Character.Party?.Count ?? 0;
		public int PlayerPartyMembersInMap => Character.Party.Count(pm => pm.InSameMap(PlayerFieldUID));
		public int PlayerPartyLowestCharLevel => Character.Party?.Min(member => member.nLevel) ?? PlayerLevel;
		public int PlayerPartyLevelDifference =>
			(Character.Party?.Max(member => member.nLevel) ?? 0) - PlayerPartyLowestCharLevel;
		public bool AllPartyMembersGuildless => Character.Party.All(member => MasterManager.GuildManager.GetGuildID(member.dwCharId) == 0);

		public int PlayerBattlefieldTeam
		{
			get
			{
				if (Field is CField_Battlefield field)
				{
					return (int)field.GetPlayerTeam(PlayerId);
				}
				return -1;
			}
		}

		public int PlayerDamageSkin => Character.nDamageSkin;

		//
		// -------------- Communication / Misc.
		//
		public void SystemMessage(string sMessage) => Character.SendMessage(sMessage);
		//
		// -------------- Map Manipulators
		//
		public void SpawnGuide() => Character.SendPacket(CPacket.UserHireTutor(true));
		public void RemoveGuide() => Character.SendPacket(CPacket.UserHireTutor(false));
		public void DisplayGuide(int nIdx) => Character.SendPacket(CPacket.UserTutorMsg(nIdx, 10 * 1000));

		public void TutorialEffect(int number) => Character.SendPacket(CPacket.TutorialEffect(number));

		public void ShowAranIntro()
		{
			string intro = "";
			switch (PlayerMapId)
			{
				case 914090010:
					intro = "Effect/Direction1.img/aranTutorial/Scene0";
					break;
				case 914090011:
					intro = "Effect/Direction1.img/aranTutorial/Scene1" + (PlayerGender == 0 ? "0" : "1");
					break;
				case 914090012:
					intro = "Effect/Direction1.img/aranTutorial/Scene2" + (PlayerGender == 0 ? "0" : "1");
					break;
				case 914090013:
					intro = "Effect/Direction1.img/aranTutorial/Scene3";
					break;
				case 914090100:
					intro = "Effect/Direction1.img/aranTutorial/HandedPoleArm" + (PlayerGender == 0 ? "0" : "1");
					break;
				case 914090200:
					intro = "Effect/Direction1.img/aranTutorial/Maha";
					break;
			}
			Character.SendPacket(CPacket.ShowReservedEffect(intro));
		}

		public void BalloonMessage(string sMessage) => Character.SendPacket(CPacket.BalloonMessage(sMessage, 0));
		public void ScriptProgressMessage(string message) => Character.SendPacket(CPacket.ScriptProgressMessage(message));
		//
		// -------------- Character Manipulators
		//
		public virtual void Warp(int mapId, int portal = 0)
			=> Character.Action.SetField(mapId, (byte)portal, 0);
		public virtual void Warp(int mapId, string sPortalName)
		{
			var pField = Character.Socket.Server.CFieldMan.GetField(mapId);

			var pPortal = pField.Portals.FindPortal(sPortalName);

			if(pPortal != null)
            {
				Character.Action.SetField(mapId, (byte)pPortal.nIdx);
			}
            else
            {
				Character.Action.SetField(mapId, 0);
			}
		}


		public virtual void WarpToInstance(int nMapID, int nPortalID, bool party = false)
		{
			if (party && Character.Party != null)
			{
				WarpPartyToInstance(PlayerFieldUID, nMapID, nPortalID);
			}
			else
			{
				Character.Action.SetFieldInstance(nMapID, Character.dwId, (byte)nPortalID, 0);
			}
		}

		public void WarpToInstance(int nMapID, int nPortalID, int nInstanceID, bool party = false)
		{
			if (party && Character.Party != null)
			{
				Character.Party.WarpParty(PlayerFieldUID, nMapID, true, nPortalID, nInstanceID);
			}
			else
			{
				Character.Action.SetFieldInstance(nMapID, nInstanceID, (byte)nPortalID);
			}
		}

		public void WarpParty(long nFromMapUID, int mapId, int portal = 0)
		{
			if (Character.Party is null)
			{
				Character.Action.SetField(mapId, (byte)portal);
			}
			else
			{
				Character.Party.WarpParty(nFromMapUID, mapId, false, portal);
			}
		}

		public void WarpPartyToInstance(int nToMapId, int portal = 0) => WarpPartyToInstance(PlayerFieldUID, nToMapId, portal);
		public void WarpPartyToInstance(long nFromMapUID, int nToMapId, int portal)
		{
			if (Character.Party is null)
			{
				Character.Action.SetFieldInstance(nToMapId, Character.dwId, (byte)portal);
			}
			else
			{
				Character.Party.WarpParty(nFromMapUID, nToMapId, true, portal);
			}
		}

		public void WarpMapToInstance(int nToMapId, int nInstanceID, int nPortalID = 0)
		{
			foreach (var user in Character.Field.Users.ToArray())
			{
				user.Action.SetFieldInstance(nToMapId, nInstanceID, (byte)nPortalID);
			}
		}

		public void SetSkin(int nValue) => Character.Modify.Stats(ctx => ctx.Skin = (byte)nValue);
		public void SetHair(int nValue) => Character.Modify.Stats(ctx => ctx.Hair = nValue);
		public void SetFace(int nValue) => Character.Modify.Stats(ctx => ctx.Face = nValue);
		//
		// -------------- Character Stat Manipulators
		//
		public void IncreaseAP(short amount) => Character.Modify.Stats(ctx => ctx.AP = (short)Math.Min(short.MaxValue, ctx.AP + amount));
		public void SetAP(short amount) => Character.Modify.Stats(ctx => ctx.AP = Math.Min(short.MaxValue, amount));

		public void IncreaseSP(short amount)
			=> SetSP((short)(Character.Stats.nSP + amount));

		public void SetSP(short amount)
			=> Character.Modify.Stats(ctx =>
			{
				Character.SendPacket(CPacket.IncSPMessage((byte)amount, ctx.Job));

				ctx.SP = amount;
			});

		public void IncreaseSTR(short amount) => Character.Modify.Stats(ctx => ctx.STR = (short)Math.Min(short.MaxValue, ctx.STR + amount));
		public void SetSTR(short amount) => Character.Modify.Stats(ctx => ctx.STR = Math.Min(short.MaxValue, amount));
		public void IncreaseDEX(short amount) => Character.Modify.Stats(ctx => ctx.DEX = (short)Math.Min(short.MaxValue, ctx.DEX + amount));
		public void SetDEX(short amount) => Character.Modify.Stats(ctx => ctx.DEX = Math.Min(short.MaxValue, amount));
		public void IncreaseINT(short amount) => Character.Modify.Stats(ctx => ctx.INT = (short)Math.Min(short.MaxValue, ctx.INT + amount));
		public void SetINT(short amount) => Character.Modify.Stats(ctx => ctx.INT = Math.Min(short.MaxValue, amount));
		public void IncreaseLUK(short amount) => Character.Modify.Stats(ctx => ctx.LUK = (short)Math.Min(short.MaxValue, ctx.LUK + amount));
		public void SetLUK(short amount) => Character.Modify.Stats(ctx => ctx.LUK = Math.Min(short.MaxValue, amount));

		public void GainFame(short amount) => Character.Modify.GainFame(amount);

		public void SetSubJob(short number) => Character.Modify.Stats(ctx => ctx.SubJob = number);

		public void Heal() => Character.Modify.Stats(ctx => { ctx.HP = Character.BasicStats.nMHP; ctx.MP = Character.BasicStats.nMMP; });
		public void SetMaxHP(int hp) => Character.Modify.Stats(ctx => { ctx.MHP = hp; ctx.HP = hp; });
		public void SetMaxMP(int mp) => Character.Modify.Stats(ctx => { ctx.MMP = mp; ctx.MP = mp; });

		// TODO code something that will automatically fetch the next job code and the required level
		public void SetJob(short jobCode) => Character.Modify.Stats(ctx => ctx.Job = jobCode);
		public void GainEXP(short expAmount) => Character.Modify.GainExp(expAmount);
		public void SetLevel(short level) => Character.Modify.Stats(ctx => ctx.Level = Math.Min((byte)250, (byte)level));
		public void GainNX(int amount) => Character.Modify.GainNX(amount);
		public void GainMeso(int amount)
		{
			Character.Modify.GainMeso(amount);
			Character.SendPacket(CPacket.IncMoneyMessage(amount));
		}

		public void RemoveMeso(int amount) => GainMeso(-amount);

		public void SetDamageSkin(int nSkin) => Character.nDamageSkin = nSkin;

		//
		// -------------- Quest Functions
		//
		public bool HasQuestInProgress(short nQuestID) => Character.Quests[nQuestID]?.nState == QuestActType.QuestAccept;
		public bool HasQuestComplete(short nQuestID) => Character.Quests[nQuestID]?.nState == QuestActType.QuestComplete;
		public bool HasQuestNotStarted(short nQuestID) => Character.Quests[nQuestID]?.nState == QuestActType.NotStarted;
		public string GetQuestRecord(short nQuestID, string sDefaultQRCode = "")
			=> Character.Quests[nQuestID]?.sQRValue ?? sDefaultQRCode;
		public void UpdateQuestRecordInternal(short nQuestID, string sQRCode)
			=> Character.Quests.UpdateQuestRecordInternal(nQuestID, sQRCode);

		public void UpdateQuestRecordInternalParty(short nQuestID, string sQRCode)
		{
			if (Character.Party != null)
			{
				foreach (var user in Character.Party)
				{
					user.CharObj?.Quests.UpdateQuestRecordInternal(nQuestID, sQRCode);
				}
			}
			else
			{
				UpdateQuestRecordInternal(nQuestID, sQRCode);
			}
		}

		public List<string> GetPartyQuestRecordValues(short nQuestID)
		{
			if (Character.Party != null)
			{
				var retVal = new List<string>();

				foreach (var user in Character.Party)
				{
					if (user.CharObj is null) continue;

					retVal.Add(user.CharObj.Quests[nQuestID]?.sQRValue ?? "");
				}

				return retVal;
			}
			else
			{
				return new List<string>() { GetQuestRecord(nQuestID) };
			}
		}

		public void SendWeatherEffect(int nItemID, string sMsg)
		{
			var itemInfo = MasterManager.ItemTemplate(nItemID);

			if (itemInfo is CashItemTemplate item)
			{
				Character.SendPacket(CPacket.BlowWeather((byte)item.WeatherType, item.TemplateId, sMsg));
			}
		}

		public void SendWeatherEffectParty(int nItemID, string sMsg)
		{
			if (Character.Party is null)
			{
				SendWeatherEffect(nItemID, sMsg);
			}
			else
			{
				var itemInfo = MasterManager.ItemTemplate(nItemID);

				if (itemInfo is CashItemTemplate item)
				{
					Character.Party.Broadcast(CPacket.BlowWeather((byte)item.WeatherType, item.TemplateId, sMsg));
				}
			}
		}

		public void SendWeatherEffectMap(int nItemID, string sMsg)
		{
			var itemInfo = MasterManager.ItemTemplate(nItemID);

			if (itemInfo is CashItemTemplate item)
			{
				Character.Field.CurrentWeather.UpdateItemInfo(item.TemplateId, sMsg, item.WeatherType);
			}
		}

		//
		// -------------- Spawn Functions
		//
		public void SpawnMonster(int mobId, short nX, short nY)
		{
			Character.Field.Mobs.CreateMob(mobId, null, nX, nY, 0, 0xFE, 0, 0, MobType.Normal, null);

			//var mob = new CMob(mobId);
			//mob.Position.X = nX;
			//mob.Position.Y = nY;
			//mob.Position.Foothold = 0;
			//mob.SpawnIndex = -1;
			//Character.Field.Mobs.CreateMob(mob);

		}
		public void SummonMobFromSack(int nItemID, short nX, short nY)
			=> Field.Mobs.OnMobSummonItemUseRequest(nItemID, nX, nY);
		//
		// -------------- Inventory Manipulators
		//
		public int GetItemCount(int nItemID) => InventoryManipulator.GetAnyItem(Character, nItemID).Item2?.nNumber ?? 0; // TODO count all items instead of just one
		public bool HasEquipped(int nItemID) => InventoryManipulator.ItemEquipped(Character, nItemID);
		public bool HasSpace(int nItemID, short nAmount = 1) => InventoryManipulator.HasSpace(Character, nItemID, nAmount);
		/// <summary>
		/// deprecated
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool CanHoldItem(int itemId, short amount = 1) => InventoryManipulator.HasSpace(Character, itemId, amount);
		public bool HasItem(int itemId, short amount = 1) => InventoryManipulator.ContainsItem(Character, itemId, amount);
		public bool RemoveItem(int itemId, short amount = 1) => InventoryManipulator.RemoveQuantity(Character, itemId, amount);
		public void RemoveFrom(int itemId, short slot, short amount = 1) => InventoryManipulator.RemoveFrom(Character, ItemConstants.GetInventoryType(itemId), slot, amount);
		public bool ContainsAnyItems(int[] itemIds) => InventoryManipulator.ContainsAny(Character, itemIds); // returns true if single item in array exists in inventory
		public byte CountFreeSlots(byte nInvType) => InventoryManipulator.CountFreeSlots(Character, (InventoryType)nInvType);
		public void AddItem(int itemId, short amount = 1)
		{
			var item = MasterManager.CreateItem(itemId);

			if (item is GW_ItemSlotBundle isb)
			{
				if (item.IsRechargeable == false)
				{
					isb.nNumber = Math.Max((short)1, amount);
				}
			}

			InventoryManipulator.InsertInto(Character, item);

			Character.SendPacket(CPacket.DropPickUpMessage_Item(itemId, amount, true));
		}

		public void AddItems(int[] itemIds)
		{
			foreach (var i in itemIds)
			{
				AddItem(i, 1);
			}
		}

		public bool AddRandomMasteryBook(int nJobID = -1)
		{
			int bookid;

			if (nJobID == -1)
			{
				bookid = MasterManager.ItemTemplates
					.MasteryBooks()
					.Random() // grab random set
					.Random(); // grab random item from set
			}
			else
			{
				bookid = MasterManager.ItemTemplates
					.MasteryBooksByJob(PlayerJob)
					.Random();
			}

			if (bookid <= 0) return false;

			var item = MasterManager.CreateItem(bookid);

			var slot = InventoryManipulator.InsertInto(Character, item);

			return slot != 0;
		}

		public int GetSkillMastery(int nSkillID)
		{
			return Character.Skills.Get(nSkillID)?.CurMastery ?? 0;
		}

		//------------------------------------------------------------------

		public bool DisbandGuild(int nMesoCost)
		{
			return false; // disabled for now

			if (PlayerMeso < nMesoCost) return false;
			RemoveMeso(nMesoCost);
			MasterManager.GuildManager.DisbandGuild(Character);

			return true;
		}

		public bool IncreaseGuildCapacity(int nMesoCost)
		{
			return MasterManager.GuildManager.IncreaseGuildCapacity(Character, nMesoCost);
		}

		public void StartCreateGuild()
		{
			Character.SendPacket(CPacket.CGuildMan.InputGuildName());
		}

		public void SendModifyGuildMark()
		{
			Character.SendPacket(CPacket.CGuildMan.InputMark());
		}

		//------------------------------------------------------------------

		public bool RemoveNpc(int nNpcId)
			=> Character.Field.Npcs.RemoveFirstByID(nNpcId);
		public void SpawnNpc(int nNpcId, int nX, int nY)
			=> Character.Field.Npcs.Add(new CNpc(nNpcId)
			{
				Position = new CMovePath { X = (short)nX, Y = (short)nY },
			});

		//------------------------------------------------------------------

		public virtual void Dispose()
		{
			Script.Dispose();
		}
	}
}
