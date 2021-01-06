using System;
using Rebirth.Provider.Attribute;
using Rebirth.Provider.Template.Item.Data;

namespace Rebirth.Provider.Template.Item.Consume
{
	public sealed class ConsumeItemTemplate : AbstractItemTemplate
	{
		[ProviderIgnore]
		public int BuffTime => Time;
		/// <summary>
		/// Used to determine the cost of rechargeable items.
		/// </summary>
		[ProviderProperty("info/unitPrice")]
		public double UnitPrice { get; set; }

		[ProviderProperty("info/masterLevel")]
		public int MasterLevel { get; set; }

		[ProviderProperty("info/reqSkillLevel")]
		public int ReqSkillLevel { get; set; }

		[ProviderProperty("info/monsterBook")]
		public bool MonsterBook { get; set; }

		[ProviderProperty("spec/consumeOnPickup")]
		public bool ConsumeOnPickup { get; set; }

		/// <summary>
		/// Battlefield skill - for sheep v wolf event
		/// Default value: -1
		/// </summary>
		[ProviderProperty("spec/BFSkill")]
		public int BFSkill { get; set; }

		[ProviderProperty("spec/dojangshield")]
		public int DojangShield { get; set; }

		[ProviderProperty("info/inc")]
		public int PetfoodInc { get; set; }

		[ProviderProperty("spec/mesoupbyitem")]
		public bool MesoUpByItem { get; set; }

		[ProviderProperty("spec/itemupbyitem")]
		public bool ItemUpByItem { get; set; }

		[ProviderProperty("spec/expBuff")]
		public bool ExpUpByItem { get; set; }

		[ProviderProperty("spec/expBuff")]
		public int ExpBuffRate { get; set; } // exp rate modifiers use this, meso/drop use prob isntead

		[ProviderProperty("spec/hp")]
		public int HP { get; set; }

		[ProviderProperty("spec/mp")]
		public int MP { get; set; }

		[ProviderProperty("spec/hpR")]
		public int HPR { get; set; }

		[ProviderProperty("spec/mpR")]
		public int MPR { get; set; }

		[ProviderProperty("spec/exp")]
		public int EXP { get; set; }

		[ProviderProperty("spec/mhpR")]
		public int MHPR { get; set; }

		[ProviderProperty("spec/mmpR")]
		public int MMPR { get; set; }

		[ProviderProperty("spec/pad")]
		public int PAD { get; set; }

		[ProviderProperty("spec/mad")]
		public int MAD { get; set; }

		[ProviderProperty("spec/pdd")]
		public int PDD { get; set; }

		[ProviderProperty("spec/mdd")]
		public int MDD { get; set; }

		[ProviderProperty("spec/padRate")]
		public int PADRate { get; set; }

		[ProviderProperty("spec/madRate")]
		public int MADRate { get; set; }

		[ProviderProperty("spec/pddRate")]
		public int PDDRate { get; set; }

		[ProviderProperty("spec/mddRate")]
		public int MDDRate { get; set; }

		[ProviderProperty("spec/acc")]
		public int ACC { get; set; }

		[ProviderProperty("spec/eva")]
		public int EVA { get; set; }

		[ProviderProperty("spec/accR")]
		public int ACCRate { get; set; }

		[ProviderProperty("spec/evaR")]
		public int EVARate { get; set; }

		[ProviderProperty("spec/speed")]
		public int Speed { get; set; }

		[ProviderProperty("spec/jump")]
		public int Jump { get; set; }

		[ProviderProperty("spec/speedRate")]
		public int SpeedRate { get; set; }

		[ProviderProperty("spec/jumpRate")]
		public int JumpRate { get; set; }

		[ProviderProperty("spec/morph")]
		public int Morph { get; set; }

		[ProviderProperty("spec/expinc")]
		public int ExpInc { get; set; }

		[ProviderProperty("spec/moveTo")]
		public int MoveTo { get; set; }

		[ProviderProperty("spec/ignoreContinent")]
		public bool IgnoreContinent { get; set; }

		[ProviderProperty("spec/prob")]
		public int Prob { get; set; }

		[ProviderProperty("spec/cp")]
		public int CP { get; set; }

		[ProviderProperty("spec/nuffSkill")]
		public int CPSkill { get; set; } // nuffSkill -> exists in Skill.wz

		[ProviderProperty("spec/seal")]
		public bool Cure_Seal { get; set; }

		[ProviderProperty("spec/curse")]
		public bool Cure_Curse { get; set; }

		[ProviderProperty("spec/poison")]
		public bool Cure_Poison { get; set; }

		[ProviderProperty("spec/weakness")]
		public bool Cure_Weakness { get; set; }

		[ProviderProperty("spec/darkness")]
		public bool Cure_Darkness { get; set; }

		[ProviderProperty("info/cursed")]
		public int CursedRate { get; set; }

		[ProviderProperty("info/success")]
		public int SuccessRate { get; set; }

		[ProviderProperty("info/incMHP")]
		public int IncMHP { get; set; }

		[ProviderProperty("info/incMMP")]
		public int IncMMP { get; set; }

		[ProviderProperty("info/incPAD")]
		public int IncPAD { get; set; }

		[ProviderProperty("info/incMAD")]
		public int IncMAD { get; set; }

		[ProviderProperty("info/incPDD")]
		public int IncPDD { get; set; }

		[ProviderProperty("info/incMDD")]
		public int IncMDD { get; set; }

		[ProviderProperty("info/incACC")]
		public int IncACC { get; set; }

		[ProviderProperty("info/incEVA")]
		public int IncEVA { get; set; }

		[ProviderProperty("info/incINT")]
		public int IncINT { get; set; }

		[ProviderProperty("info/incDEX")]
		public int IncDEX { get; set; }

		[ProviderProperty("info/incSTR")]
		public int IncSTR { get; set; }

		[ProviderProperty("info/incLUK")]
		public int IncLUK { get; set; }

		[ProviderProperty("info/incSpeed")]
		public int IncSpeed { get; set; }

		[ProviderProperty("info/incJump")]
		public int IncJump { get; set; }

		[ProviderProperty("info/preventslip")]
		public bool PreventSlip { get; set; }

		[ProviderProperty("info/warmsupport")]
		public bool WarmSupport { get; set; }

		[ProviderProperty("info/incCraft")]
		public int IncCraft { get; set; }

		[ProviderProperty("info/recover")]
		public int Recover { get; set; }

		[ProviderProperty("info/randstat")]
		public bool RandStat { get; set; }

		[ProviderProperty("info/incRandVol")]
		public int IncRandVol { get; set; }

		[ProviderProperty("info/type")]
		public int InfoType { get; set; }

		[ProviderProperty("spec/script")]
		public string ItemScript { get; set; }

		// these are for mastery books
		public int[] SkillData { get; set; }
		public int[] PetfoodPets { get; set; }
		public int[] SummoningSackIDs { get; set; }
		public int[] SummoningSackProbs { get; set; }
		public RewardData[] Reward { get; set; } // TODO hook this up to a provider

		public bool ScrollDestroy(Random rand)
			=> CursedRate > 0 && rand.Next(100) < CursedRate;
		public bool ScrollSuccess(Random rand)
			=> SuccessRate <= 0 || rand.Next(100) < SuccessRate;

		public ConsumeItemTemplate(int templateId) : base(templateId)
		{
			SkillData = new int[0];
			PetfoodPets = new int[0];
			SummoningSackIDs = new int[0];
			SummoningSackProbs = new int[0];
			Reward = new RewardData[0];
			ItemScript = "";
		}
	}
}