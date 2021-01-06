using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Npgsql;
using Rebirth.Characters.Modify;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Provider.Template.Quest;
using Rebirth.Scripts;
using Rebirth.Server.Center;

namespace Rebirth.Characters.Quest
{
	public sealed class CharacterQuests : NumericKeyedCollection<QuestEntry>
	{
		public static void OnQuestRequest(WvsGameClient c, CInPacket p)
		{
			if (c.Character.Stats.nHP <= 0) return;

			var nType = (QuestRequestType)p.Decode1();
			var nQuestId = p.Decode2();
			var pQuest = MasterManager.QuestTemplates[nQuestId];

			if (pQuest is null)
			{
				//c.Character.SendMessage($"Unable to find quest ID {nQuestId}.");
				return;
			}

			var dwNpcTemplateID = 0;

			if (nType != QuestRequestType.LostItem && nType != QuestRequestType.ResignQuest)
			{
				dwNpcTemplateID = p.Decode4();

				var npcTemplate = MasterManager.NpcTemplates[dwNpcTemplateID];

				// TODO check for if NPC should be in the same map as the player
				//if (!c.Character.Field.Npcs.Contains(dwNpcTemplateID))
				//{
				//	c.Character.SendMessage($"Unable to find NPC {nQuestId} in current field.");
				//	// return; 
				//}

				if (!pQuest.AutoStart)
				{
					var ptUserPosX = p.Decode2();
					var ptUserPosY = p.Decode2();

					// TODO quest location check
					// if ( abs(v16) > 1200 || (v8 = *(v20 + 136) - pTemplate, v17 = *(v20 + 132), abs(v8) > 800) )
					// tick AB 
				}
			}

			switch (nType)
			{
				case QuestRequestType.LostItem:
					c.Character.Quests.OnLostQuestItem(p, nQuestId);
					break;
				case QuestRequestType.AcceptQuest:
					c.Character.Quests.OnAcceptQuest(p, nQuestId, dwNpcTemplateID);
					break;
				case QuestRequestType.CompleteQuest:
					c.Character.Quests.OnCompleteQuest(p, nQuestId, dwNpcTemplateID, false);
					break;
				case QuestRequestType.ResignQuest:
					c.Character.Quests.OnResignQuest(p, nQuestId);
					break;
				case QuestRequestType.OpeningScript:
					c.Character.Quests.OnScriptLinkedQuest(nQuestId, dwNpcTemplateID, 0);
					break;
				case QuestRequestType.CompleteScript:
					c.Character.Quests.OnScriptLinkedQuest(nQuestId, dwNpcTemplateID, 1);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(nType));
			}
		}

		public int dwParentID { get; set; }
		public Character Parent => MasterManager.CharacterPool.Get(dwParentID);

		public CharacterQuests(int parent)
		{
			dwParentID = parent;
		}

		public void OnLostQuestItem(CInPacket p, short nQuestID)
		{
			var nLostCount = p.Decode4();

			var pAct = MasterManager.QuestTemplates[nQuestID].StartAct;

			if (pAct is null) return; // invalid quest ID

			if (nLostCount <= 0 || pAct.Items.Length <= 0) return; // TODO trigger AB, close socket

			var aLostItem = p.DecodeIntArray(nLostCount);

			foreach (var item in pAct.Items)
			{
				if (!aLostItem.Contains(item.Item.ItemID)) continue; // TODO trigger AB?

				if (ItemConstants.GetInventoryType(item.Item.ItemID) == InventoryType.Equip)
				{
					Parent.SendMessage("Not currently supported. Check again later.");
					return; // TODO
				}

				if (!MasterManager.ItemTemplates[item.Item.ItemID]?.Quest ?? true) continue;

				if (InventoryManipulator.HasSpace(Parent, item.Item.ItemID, (short)item.Item.Count))
				{
					var itemToAdd = MasterManager.CreateItem(item.Item.ItemID);
					itemToAdd.nNumber = (short)item.Item.Count;
					InventoryManipulator.InsertInto(Parent, itemToAdd);
				}
				else
				{
					// TODO proper response packet
					Parent.SendMessage("Please make space in your inventory.");
					return;
				}
			}

			// TODO
			// if ( aChangedItem.a && *(aChangedItem.a - 1) > 0u )
			//		CUser::PostQuestEffect(&v16->vfptr, 1, &aChangedItem, 0, 0);
		}

		public void OnAcceptQuest(CInPacket p, short nQuestID, int dwNpcTemplateID)
		{
			if (!CheckDemand(nQuestID, dwNpcTemplateID, 0))
			{
				Parent.SendPacket(CPacket.CQuestMan.UserQuestResult(QuestResultType.Failed_Unknown, null));
				return;
			}

			var nStartResult = TryQuestAct(nQuestID, 0);

			if (nStartResult == QuestResultType.Failed_Unknown) return;

			if (!Contains(nQuestID))
				Add(new QuestEntry(nQuestID)
				{
					tCompleted = DateTime.MinValue,
					nState = nStartResult == QuestResultType.Success ? QuestActType.QuestAccept : QuestActType.NotStarted
				});

			if (nStartResult == QuestResultType.Success) this[nQuestID].nState = QuestActType.QuestAccept;

			var pQuest = MasterManager.QuestTemplates[nQuestID];
			if (pQuest.AutoComplete)
			{
				if (pQuest.EndScript.Length > 0)
				{
					OnScriptLinkedQuest(nQuestID, dwNpcTemplateID, 1);
				}
				else
				{
					OnCompleteQuest(p, nQuestID, dwNpcTemplateID, true);
				}
				return;
			}

			Parent.SendPacket(CPacket.CQuestMan.QuestRecordMessage(this[nQuestID]));
			Parent.SendPacket(CPacket.CQuestMan.UserQuestResult(nStartResult, this[nQuestID], dwNpcTemplateID, 0));
		}

		public void OnCompleteQuest(CInPacket p, short nQuestID, int dwNpcTemplateID, bool bIsAutoComplete)
		{
			var nSelect = bIsAutoComplete ? -1 : p.Decode4();

			if (!CheckDemand(nQuestID, dwNpcTemplateID, 1))
			{
				Parent.SendPacket(CPacket.CQuestMan.UserQuestResult(QuestResultType.Failed_Unknown, null));
				return;
			}

			var nActCompleteResult = TryQuestAct(nQuestID, 1);

			if (nActCompleteResult == QuestResultType.Failed_Unknown) return;

			if (nActCompleteResult == QuestResultType.Success)
			{
				this[nQuestID].tCompleted = DateTime.Now;
				this[nQuestID].nState = QuestActType.QuestComplete;
			}

			var endActNextQuest = MasterManager.QuestTemplates[nQuestID].EndAct?.NextQuest ?? 0;

			Parent.SendPacket(CPacket.CQuestMan
				.UserQuestResult(nActCompleteResult, this[nQuestID], dwNpcTemplateID, (short)endActNextQuest));
		}

		public void OnResignQuest(CInPacket p, short nQuestID)
		{
			// TODO
		}

		public void OnScriptLinkedQuest(short nQuestID, int dwNpcTemplateID, int nScriptActCategory)
		{
			var quest = MasterManager.QuestTemplates[nQuestID];

			if (quest is null) return; // :( bonkers

			if (nScriptActCategory == 0)
			{
				if (quest.StartScript.Length <= 0) return;
			}
			else if (nScriptActCategory == 1)
			{
				if (quest.EndScript.Length <= 0) return;
			}
			else throw new ArgumentOutOfRangeException(nameof(nScriptActCategory), nScriptActCategory, "value outside of inclusive range (0,1)");

			var provider = ServerApp.Container.Resolve<ScriptManager>();

			var script = provider.GetQuestScript
				(nQuestID, nScriptActCategory, this[nQuestID], Parent.Socket);

			script?.Execute();
		}

		public bool CheckDemand(short nQuestID, int dwNpcTemplateID, int nAct)
		{
			// TODO proper response codes

			var pQuest = MasterManager.QuestTemplates[nQuestID];

			var pDemand = nAct == 0 ? pQuest.StartDemand : pQuest.EndDemand;

			if (pDemand is null) return true; // no requirements

			var correctNpc = nAct == 0 ? pQuest.StartNPC : pQuest.EndNPC;

			if (correctNpc != 0 && correctNpc != dwNpcTemplateID) return false;

			switch (nAct)
			{
				case 0 when this[nQuestID] is null:
					return true;
				case 1 when this[nQuestID] is null:
				case 1 when this[nQuestID].nState == QuestActType.NotStarted:
				case 0 when this[nQuestID].nState == QuestActType.QuestAccept:
					return false;
				default:
					if (this[nQuestID].IsComplete) return false;
					break;
			}

			if (Parent.Stats.nLevel < pDemand.LevelMin) return false;
			if (Parent.Stats.nPOP < pDemand.Pop) return false;
			if (pQuest.Start > DateTime.MinValue && pQuest.Start.SecondsSinceStart() < 0) return false;

			// hmm the client is still allowing some quests that have an end date and max level to be triggered...
			//if (pQuest.End > DateTime.MinValue && pQuest.End.SecondsSinceStart() > 0) return false;
			//if (pDemand.LevelMax != 0 && Parent.Stats.nLevel > pDemand.LevelMax) return false;

			if (pDemand.SubJobFlags != 0 && Parent.Stats.nSubJob != pDemand.SubJobFlags) return false;

			if (pDemand.Job.Length > 0
				&& pDemand.Job.All(job => job != Parent.Stats.nJob)) return false;

			foreach (var item in pDemand.DemandItem)
			{
				if (ItemConstants.GetInventoryType(item.ItemID) == InventoryType.Equip)
				{
					if (InventoryManipulator.ItemEquipped(Parent, item.ItemID)) continue;
				}

				if (!InventoryManipulator.ContainsItem(Parent, item.ItemID, (short)item.Count)) return false;
			}

			if (pDemand.EquipAllNeed.Any(
				item => !InventoryManipulator.ItemEquipped(Parent, item))) return false;

			if (pDemand.EquipSelectNeed.Length > 0 && pDemand.EquipSelectNeed.All( // TODO verify
				item => !InventoryManipulator.ItemEquipped(Parent, item))) return false;

			foreach (var quest in pDemand.DemandQuest)
			{
				if (quest.State == 0 && !Contains(quest.QuestID)) return false;
				if ((QuestActType)quest.State != this[quest.QuestID].nState) return false;
			}

			foreach (var skill in pDemand.DemandSkill)
			{
				if (skill.Acquire == 0 && Parent.Skills[skill.SkillID]?.nSLV != 0) return false;
				if (Parent.Skills[skill.SkillID].nSLV == 0) return false;
			}

			foreach (var mob in pDemand.DemandMob)
			{
				var quest = this[nQuestID];
				if (!quest.DemandRecords.ContainsKey(mob.MobID)) return false;
				if (quest.DemandRecords[mob.MobID].nValue != mob.Count) return false;
			}

			foreach (var map in pDemand.DemandMap)
			{
				var quest = this[nQuestID];
				if (!quest.DemandRecords.ContainsKey(map.MapID)) return false;
				if (quest.DemandRecords[map.MapID].nValue <= 0) return false;
			}

			if (pDemand.FieldEnter.Length > 0
				&& pDemand.FieldEnter.All(map => map == Parent.Stats.dwPosMap)) return false;

			// TODO taming mob
			// TODO pet closeness

			return true;
		}

		public QuestResultType TryQuestAct(short nQuestID, int nAct)
		{
			var pAct = nAct == 0
				? MasterManager.QuestTemplates[nQuestID].StartAct
				: MasterManager.QuestTemplates[nQuestID].EndAct;

			if (pAct is null) return QuestResultType.Success; // no requirements

			if (pAct.IncPetTameness != 0
				&& Parent.Pets.Pets[0] is null) return QuestResultType.Failed_Pet;

			var nExchangeResult = TryExchange(pAct.IncMoney, pAct.Items);

			if (nExchangeResult == QuestResultType.Success)
			{
				if (pAct.IncPetTameness != 0)
				{
					var pet = Parent.Pets.Pets[0];

					pet.IncTameness(pAct.IncPetTameness);
					pet.UpdatePetItem(); // sends notification packet to client
				}

				if (pAct.IncExp != 0)
				{
					Parent.Modify.GainExp(pAct.IncExp * RateConstants.ExpRate);
				}

				if (pAct.IncPop != 0)
				{
					Parent.Modify.GainFame((short)pAct.IncPop);
				}

				if (pAct.BuffItemID != 0)
				{
					Parent.Buffs.AddItemBuff(pAct.BuffItemID);
				}

				foreach (var skill in pAct.Skills)
				{
					if (skill.Job.All(job => job != Parent.Stats.nJob)) continue;

					Parent.Modify.Skills(ctx =>
					{
						ctx.AddEntry(skill.SkillID, act =>
						{
							if (skill.SkillLevel > 0) act.nSLV = (byte)skill.SkillLevel;
							if (skill.MasterLevel > 0) act.CurMastery = (byte)skill.MasterLevel;
						});
					});
				}

				SetQuestRecord(nQuestID, ""); // TODO
			}

			return nExchangeResult;
		}

		public QuestResultType TryExchange(int nIncMoney, QuestAct.ActItem[] aActItem)
		{
			if (nIncMoney == 0 && aActItem.Length <= 0) return QuestResultType.Success;

			if (nIncMoney < 0
				&& Parent.Stats.nMoney < nIncMoney) return QuestResultType.Failed_Meso;

			foreach (var item in aActItem)
			{
				if (item.Item.Count > 0)
				{
					if (!InventoryManipulator.HasSpace(Parent, item.Item.ItemID, (short)item.Item.Count))
						return QuestResultType.Failed_Inventory; // -> Etc inventory is full
				}
				else
				{
					if (!InventoryManipulator.ContainsItem(Parent, item.Item.ItemID, (short)item.Item.Count))
						return QuestResultType.Failed_Unknown; // idk what code to give them
				}
			}

			Parent.Modify.GainMeso(nIncMoney);

			var weight = 0;

			foreach (var item in aActItem)
			{
				if (item.ProbRate != 0)
				{
					weight += item.ProbRate;
				}
			}

			var itemGiven = false;

			foreach (var item in aActItem.Shuffle())
			{
				if (item.Item.Count < 0)
				{
					InventoryManipulator.RemoveQuantity(Parent, item.Item.ItemID, (short)item.Item.Count);
				}
				else
				{
					if (item.ProbRate == 0)
					{
						var newItem = MasterManager.CreateItem(item.Item.ItemID, false);
						if (!newItem.IsEquip) newItem.nNumber = (short)item.Item.Count;

						InventoryManipulator.InsertInto(Parent, newItem);
					}
					else if (!itemGiven)
					{
						var rand = Constants.Rand.Next(0, weight);

						if (rand < item.ProbRate)
						{
							var newItem = MasterManager.CreateItem(item.Item.ItemID, false);
							if (!newItem.IsEquip) newItem.nNumber = (short)item.Item.Count;

							InventoryManipulator.InsertInto(Parent, newItem);
							itemGiven = true;
						}
						else
						{
							weight -= item.ProbRate;
						}
					}
				}
			}

			return QuestResultType.Success;
		}

		public void SetQuestRecord(short nQuestID, string sInfo)
		{
			if (sInfo is null || nQuestID == 0 || sInfo.Length <= 0 || sInfo.Length > 16) return;

			if (this[nQuestID] is null) Add(new QuestEntry(nQuestID));

			this[nQuestID].sQRValue = sInfo;

			Parent.SendPacket(CPacket.CQuestMan.QuestRecordMessage(this[nQuestID]));
		}

		public void UpdateMobKills(int nMobTemplateId, int nIncreaseAmount)
		{
			var mobQuests = MasterManager.QuestTemplates.GetQuestByMobID(nMobTemplateId);

			if (mobQuests is null) return;

			foreach (var questId in mobQuests)
			{
				var quest = this[questId];

				if (quest?.nState != QuestActType.QuestAccept) continue;

				quest.DemandRecords[nMobTemplateId].nValue += nIncreaseAmount;

				quest.sQRValue = quest.DemandRecords[nMobTemplateId].nValue.ToString("000");
				Parent.SendPacket(CPacket.CQuestMan.QuestRecordMessage(quest));
			}
		}

		/// <summary>
		/// Updates QR internally and notifies the client about the change.
		/// </summary>
		/// <param name="dwQuestId">ID of the quest to modify</param>
		/// <param name="sText">Updated QR. Leave null to send existing record.</param>
		public void SendUpdateQuestRecordMessage(short dwQuestId, string sText = null)
		{
			if (!Contains(dwQuestId))
			{
				Add(new QuestEntry(dwQuestId));
			}

			this[dwQuestId].nState = QuestActType.QuestAccept;
			this[dwQuestId].tCompleted = DateTime.MinValue;
			if (sText != null)
			{
				this[dwQuestId].sQRValue = sText;
			}

			Parent.SendPacket(CPacket.CQuestMan.QuestRecordMessage(this[dwQuestId]));
		}

		public void UpdateQuestRecordInternal(short dwQuestId, object sText)
		{
			if (!Contains(dwQuestId))
			{
				Add(new QuestEntry(dwQuestId));
			}

			this[dwQuestId].nState = QuestActType.NotStarted;
			this[dwQuestId].tCompleted = DateTime.MinValue;
			this[dwQuestId].sQRValue = sText.ToString();
		}

		public string QuestRecordOrDefault(short nQuestID, string sDefault = "")
			=> this[nQuestID]?.sQRValue ?? sDefault;

		public void EncodeMedalQuests(COutPacket p)
		{
			var list = this.Where(q => q.nQuestID >= 29000 && q.nQuestID <= 30000);
			var questEntries = list as QuestEntry[] ?? list.ToArray();

			p.Encode2((short)questEntries.Length);
			foreach (var item in questEntries)
			{
				p.Encode2(item.nQuestID);
			}
		}

		public void SaveToDB()
		{
			if (Count <= 0) return;

			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				// COLUMNS: char_id, quest_id, state, qr_value, completed

				var dbQuery = new StringBuilder();

				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.quest_record WHERE char_id = {dwParentID};");
				dbQuery.AppendLine($"DELETE FROM {Constants.DB_All_World_Schema_Name}.quest_record_requirements WHERE char_id = {dwParentID};");

				foreach (var quest in this)
				{
					dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.quest_record (char_id, quest_id, state, qr_value, completed)");
					dbQuery.AppendLine($"VALUES ({dwParentID}, {quest.nQuestID}, {(short)quest.nState}, '{quest.sQRValue}', '{quest.tCompleted}');");

					if (quest.DemandRecords == null) continue;

					foreach (var req in quest.DemandRecords.Values)
					{
						dbQuery.AppendLine($"INSERT INTO {Constants.DB_All_World_Schema_Name}.quest_record_requirements (char_id, quest_id, type, key, value)");
						dbQuery.AppendLine($"VALUES ({dwParentID}, {quest.nQuestID}, {(short)req.nType}, {req.nKey}, {req.nValue});");
					}
				}

				try
				{
					using (var cmd = new NpgsqlCommand(dbQuery.ToString(), conn))
						cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					dbQuery.AppendLine("DATABASE FAILED TO EXECUTE THE FOLLOWING QUERY:");
					MasterManager.Log.Error(dbQuery);
					MasterManager.Log.Error(ex);
				}
			}
		}

		public async Task LoadFromDB()
		{
			using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
			{
				conn.Open();

				using (var cmd = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.quest_record WHERE char_id = {dwParentID}", conn))
				using (var r = await cmd.ExecuteReaderAsync())
				{
					while (r.Read())
					{
						var state = (short)r["state"];

						var dt = r["completed"];

						var entry = new QuestEntry((short)r["quest_id"])
						{
							nState = (QuestActType)state,
							sQRValue = r["qr_value"] as string,
						};

						if (dt is DateTime realDT)
						{
							entry.tCompleted = realDT;
						}
						else
						{
							entry.tCompleted = DateTime.MinValue;
						}

						Add(entry);
					}
				}

				// COLUMNS: char_id, quest_id, type, key, value
				using (var cmd2 = new NpgsqlCommand($"SELECT * FROM {Constants.DB_All_World_Schema_Name}.quest_record_requirements WHERE char_id = {dwParentID} ", conn))
				using (var r2 = await cmd2.ExecuteReaderAsync())
				{
					while (r2.Read())
					{
						var quest = this[(short)r2["quest_id"]];

						// we dont need to populate this if its completed
						if (quest.nState != QuestActType.QuestAccept) continue;

						var key = (int)r2["key"];
						var type = (short)r2["type"];

						if (quest.DemandRecords.ContainsKey(key))
						{
							quest.DemandRecords[key].nValue = (int)r2["value"];
						}
					}
				}
			}
		}

		protected override int GetKeyForItem(QuestEntry item) => item.nQuestID;
	}
}
