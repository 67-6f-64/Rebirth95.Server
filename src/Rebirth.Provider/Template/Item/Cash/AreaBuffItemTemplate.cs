using System.Drawing;
using Rebirth.Common.Tools;
using Rebirth.Provider.Attribute;

namespace Rebirth.Provider.Template.Item.Cash
{
	public sealed class AreaBuffItemTemplate : CashItemTemplate
	{
		[ProviderProperty("info/rb")]
		public Point RB { get; set; }

		[ProviderProperty("info/lt")]
		public Point LT { get; set; }

		public TagRect AffectedArea => new TagRect(LT.X, LT.Y, RB.X, RB.Y);
		public int TotalProp { get; set; }
		public int[] EmotionProp { get; set; } // size 24 in pdb

		public AreaBuffItemTemplate(int templateId)
			: base(templateId) { }
	}
}