using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Provider.Template.Etc
{
	public class ItemMakeTemplate : AbstractTemplate
	{
		public int CatalystID { get; set; }
		public int ReqLevel { get; set; }
		public int ReqSkillLevel { get; set; }
		public int ReqItem { get; set; }
		public int ItemNum { get; set; }
		public int TUC { get; set; }
		public int Meso { get; set; }
		public ItemMakeRecipe[] Recipe { get; set; }
		public ItemMakeRandomReward[] RandomReward { get; set; }

		public ItemMakeTemplate(int templateId)
			: base(templateId)
		{
			Recipe = new ItemMakeRecipe[] { };
			RandomReward = new ItemMakeRandomReward[] { };
		}
	}
}
