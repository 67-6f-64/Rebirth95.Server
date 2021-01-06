using System;

namespace Rebirth.Common.Types
{
	[Flags]
	public enum SocialNotiflag
	{
		ChangeChannel = 0x1,
		ChangeJob = 0x2,
		ChangeLevel = 0x4,
		ChangeName = 0x8,
		//ChangeGender, // kek
		LogIn = 0x10,
		LogOut = 0x20,
		MigrateCashShop = 0x40,
		ChangeMap = 0x80
	}
}
