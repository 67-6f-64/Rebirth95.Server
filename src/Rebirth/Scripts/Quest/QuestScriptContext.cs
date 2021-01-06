using System;
using Rebirth.Characters.Quest;
using Rebirth.Common.Types;
using Rebirth.Game;

namespace Rebirth.Scripts.Quest
{
	public class QuestScriptContext : ScriptContextBase<QuestScript>
	{
		private QuestEntry Quest => Script.Quest;

		public QuestScriptContext(QuestScript script)
			: base(script) { }

		public byte GetQuestState => (byte)Quest.nState;
		public bool IsQuestComplete => GetQuestState.Equals((byte)QuestActType.QuestComplete);
		public bool IsQuestInProgress => GetQuestState.Equals((byte)QuestActType.QuestAccept);
		public bool IsQuestNotStarted => GetQuestState.Equals((byte)QuestActType.NotStarted);

		public void StartQuest()
		{
			var Parent = Script.Parent.Character;

			Quest.nState = QuestActType.QuestAccept;
			Quest.tCompleted = DateTime.MinValue;

			Parent.SendPacket(CPacket.CQuestMan
				.QuestRecordMessage(Quest));

			Parent.SendPacket(CPacket.CQuestMan
				.UserQuestResult(QuestResultType.Success, Quest, Quest.Template.StartNPC));
		}

		public void UpdateQuestQR(string sMsg)
		{
			var Parent = Script.Parent.Character;

			Quest.sQRValue = sMsg;

			Parent.SendPacket(CPacket.CQuestMan
				.QuestRecordMessage(Quest));
		}

		public void CompleteQuest()
		{
			var Parent = Script.Parent.Character;

			Quest.nState = QuestActType.QuestComplete;
			Quest.tCompleted = DateTime.Now;

			var effect = new UserEffectPacket(UserEffect.QuestComplete);
			effect.BroadcastEffect(Parent);

			Parent.SendPacket(CPacket.CQuestMan.QuestCompleted(Quest.nQuestID));

			Parent.SendPacket(CPacket.CQuestMan
				.QuestRecordMessage(Quest));

			Parent.SendPacket(CPacket.CQuestMan
				.UserQuestResult(QuestResultType.Success, Quest, Quest.Template.EndNPC));
		}
	}
}
