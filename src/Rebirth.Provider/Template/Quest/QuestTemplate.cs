using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Provider.Template.Quest
{
	public sealed class QuestTemplate : AbstractTemplate
	{
		public string StartScript { get; set; }
		public string EndScript { get; set; }

		public bool AutoAccept { get; set; }
		public bool AutoStart { get; set; }
		public bool AutoComplete { get; set; }
		public bool AutoPreComplete { get; set; }

		public int StartNPC { get; set; }
		public int EndNPC { get; set; }

		public DateTime End { get; set; }
		public DateTime Start { get; set; }

		public int ViewMedalItem { get; set; }

		public QuestDemand StartDemand { get; set; }
		/// <summary>
		/// Required duration between start and finish
		/// </summary>
		public int CompletionWaitInterval { get; set; }
		public QuestDemand EndDemand { get; set; }

		public QuestAct StartAct { get; set; }
		/// <summary>
		/// Minimum wait duration between quest repeats.
		/// If zero, quest can not be repeated.
		/// </summary>
		public int RepeatInterval { get; set; }
		public QuestAct EndAct { get; set; }

		public QuestTemplate(int templateId) : base(templateId)
		{
			StartScript = string.Empty;
			EndScript = string.Empty;
			End = DateTime.MinValue;
			Start = DateTime.MinValue;
		}
	}

	public sealed class QuestDemand
	{
		public int TamingMobLevelMin { get; set; }
		public int TamingMobLevelMax { get; set; }
		public int PetTamenessMin { get; set; }
		public int PetTamenessMax { get; set; }
		public int LevelMin { get; set; }
		public int LevelMax { get; set; }
		public int Pop { get; set; }
		public bool RepeatByDay { get; set; }
		public QuestRecord[] DemandQuest { get; set; }
		public MobInfo[] DemandMob { get; set; }
		public ItemInfo[] DemandItem { get; set; }
		public SkillInfo[] DemandSkill { get; set; }
		public MapInfo[] DemandMap { get; set; }
		public int[] EquipAllNeed { get; set; }
		public int[] EquipSelectNeed { get; set; }
		public int[] FieldEnter { get; set; } // TODO
		public int[] Job { get; set; }
		public int SubJobFlags { get; set; }
		public bool NormalAutoStart { get; set; }

		public QuestDemand()
		{
			DemandQuest = new QuestRecord[0];
			DemandItem = new ItemInfo[0];
			DemandSkill = new SkillInfo[0];
			DemandMob = new MobInfo[0];
			DemandMap = new MapInfo[0];
			EquipAllNeed = new int[0];
			EquipSelectNeed = new int[0];
			FieldEnter = new int[0];
			Job = new int[0];
		}

		public sealed class QuestRecord
		{
			public short QuestID { get; set; }
			public int State { get; set; }
		}
	}

	public sealed class QuestAct
	{
		public int IncExp { get; set; }
		public int IncMoney { get; set; }
		public int IncPop { get; set; }
		public int IncPetTameness { get; set; }
		public int IncPetSpeed { get; set; }
		public int PetSkill { get; set; }
		public int NextQuest { get; set; }
		public int BuffItemID { get; set; }
		public int[] MapID { get; set; }
		public int LevelMin { get; set; }
		public int LevelMax { get; set; }
		public string Info { get; set; }

		public ActItem[] Items { get; set; }
		public ActSkill[] Skills { get; set; }

		public QuestAct()
		{
			Items = new ActItem[0];
			Skills = new ActSkill[0];
		}

		public sealed class ActItem
		{
			public ItemInfo Item { get; set; }
			// ItemOption
			public int Period { get; set; }
			public int JobFlag { get; set; }
			public int Gender { get; set; }
			public int ProbRate { get; set; }
			public int ItemVariation { get; set; }
		}

		public sealed class ActSkill
		{
			public int SkillID { get; set; }
			public int MasterLevel { get; set; }
			public int SkillLevel { get; set; }
			public int[] Job { get; set; } = new int[0];
		}
	}

	public sealed class ItemInfo
	{
		public int ItemID { get; set; }
		public int Count { get; set; }
	}

	public sealed class MobInfo
	{
		public int MobID { get; set; }
		public int Count { get; set; }
	}

	public sealed class SkillInfo
	{
		public int SkillID { get; set; }
		public int Acquire { get; set; }
	}

	public sealed class MapInfo
	{
		public int MapID { get; set; }
		public int Acquire { get; set; }
	}
}
