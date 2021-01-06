using System;
using System.Collections.Generic;
using System.Text;

namespace Rebirth.Common.GameLogic
{
	public static class ItemLogic
	{
		public static bool WhiteScroll(int nItemID) => nItemID / 10000 == 234;
		public static bool PotentialScroll(int nItemID) => nItemID / 100 == 20494;
		public static bool StarScroll(int nItemID) => nItemID / 100 == 20493;
		public static bool MagnifyingGlass(int nItemID) => nItemID / 10000 == 246;
	}
}
