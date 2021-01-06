namespace Rebirth.Common.Types
{
	public enum AvatarMegaphoneResCode : byte
	{
		QueueFull = 0x60, // its technically a CashItemOp but putting them in their enum is better organization imo
		LevelLimit = 0x61,

		Success_Now = 0x62,
		Success_Later = 0x63,
	}
}
