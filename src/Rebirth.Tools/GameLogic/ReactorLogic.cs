using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Common.GameLogic
{
	public static class ReactorLogic
	{
		public static int HitTypePriorityLevel(int nOption, int nType)
		{
			var nOpt1 = (nOption & 1) != 0 ? 1 : 0;
			var nOpt2 = (nOption & 1) == 0 ? 1 : 0;

			if ((nOption & 2) != 0)
			{
				switch (nType)
				{
					case 0:
						return 1;
					case 1:
						return -nOpt1;
					case 2:
						return nOpt1 - 1;
				}
			}
			else
			{
				switch (nType)
				{
					case 0:
						return 2;
					case 1:
						return 2 * nOpt2 - 1;
					case 2:
						return 2 * nOpt1 - 1;
					case 3:
						return -nOpt1;
					case 4:
						return nOpt1 - 1;
				}
			}

			return -1;
		}
	}
}
