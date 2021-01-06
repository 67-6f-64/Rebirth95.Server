using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Game
{
	public class CCurseProcess
	{
		private static readonly Dictionary<string, string> replacements;

		static CCurseProcess()
		{
			replacements = new Dictionary<string, string>();
			replacements.Add("cloud", "butt");
		}

		public static void ProcessString(string sInput, out string sOutput)
		{
			foreach (var item in replacements)
			{
				sInput = sInput.Replace(item.Key, item.Value, StringComparison.OrdinalIgnoreCase);
			}
			sOutput = sInput;
		}
	}
}
