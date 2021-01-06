namespace Rebirth.Common.Types
{
	/// <summary>
	/// Anything above a 0 is a mob-specific special effect.
	/// </summary>
	public enum MobAppearType
	{
		MOBAPPEAR_EFFECT = 0,
		MOBAPPEAR_DELAY = 0xFB, // -5
		MOBAPPEAR_SUSPENDED = 0xFC, // -4 // suspended animation (unsure which mobs do this)
		MOBAPPEAR_REVIVED = 0xFD, // -3 // this is the animation when a mob is being revived (zakum etc)
		MOBAPPEAR_REGEN = 0xFE, // -2 // this is when the mob is spawned through the life pool
		MOBAPPEAR_NORMAL = 0xFF, // -1 // this is when the mob already exists in the map when the user enters
	}
}
