using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public sealed class PetItemTemplate : CashItemTemplate
	{
		[ProviderIgnore]
		public override int SlotMax => 1;
		
		[ProviderProperty("info/hungry")]
		public int Hungry { get; set; }

		[ProviderProperty("info/nameTag")]
		public int NameTag { get; set; }

		[ProviderProperty("info/chatBalloon")]
		public int ChatBalloon { get; set; }

		public int[] Evol { get; set; } // manually set by provider
		public int[] EvolProb { get; set; }


		[ProviderProperty("info/evolReqItemID")]
		public int EvolReqItemID { get; set; }

		[ProviderProperty("info/evolNo")]
		public int EvolNo { get; set; }

		[ProviderProperty("info/evolReqPetLvl")]
		public int EvolReqPetLvl { get; set; }

		[ProviderProperty("info/limitedLife")]
		public int LimitedLife { get; set; }


		[ProviderProperty("info/permanent")]
		public bool Permanent { get; set; }

		[ProviderProperty("info/autoReact")]
		public bool AutoReact { get; set; }

		[ProviderProperty("info/noRevive")]
		public bool NoRevive { get; set; }

		[ProviderProperty("info/noMoveToLocker")]
		public bool NoMoveToLocker { get; set; }

		[ProviderProperty("info/interactByUserAction")]
		public bool InteractByUserAction { get; set; }

		public PetItemTemplate(int templateId)
			: base(templateId)
		{
			EvolProb = new int[0];
			Evol = new int[0];
		}
	}
}
