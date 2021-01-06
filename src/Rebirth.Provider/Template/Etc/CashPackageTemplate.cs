using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Provider.Template.Etc
{
	public class CashPackageTemplate : AbstractTemplate
	{
		public long[] SNList { get; set; }

		public CashPackageTemplate(int templateId)
			: base(templateId)
		{
			SNList = new long[] { };
		}
	}
}
