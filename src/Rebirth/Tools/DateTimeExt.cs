using System;

namespace Rebirth
{
	public static class DateTimeExt
	{
		public static string ToSqlTimeStamp(this DateTime dt)
			=> dt.ToString("yyyy-MM-dd HH:mm:ss");

		public static bool AddedMillisExpired(this DateTime dtStartTime, long tAddedTimeMillis)
			=> (DateTime.Now - dtStartTime).TotalMilliseconds > tAddedTimeMillis;

		public static bool AddedSecondsExpired(this DateTime dtStartTime, int tAddedTimeSeconds)
			=> (DateTime.Now - dtStartTime).TotalSeconds > tAddedTimeSeconds;

		public static bool AddedHoursExpired(this DateTime dtStartTime, int tAddedTimeHours)
			=> (DateTime.Now - dtStartTime).TotalHours > tAddedTimeHours;

		public static long MillisSinceStart(this DateTime dtStartTime)
			=> (long)(DateTime.Now - dtStartTime).TotalMilliseconds;

		/// <summary>
		/// Beware casting this to a smaller datatype -> integer overflows can occur.
		/// </summary>
		/// <param name="dtEndTime"></param>
		/// <returns></returns>
		public static long SecondsSinceStart(this DateTime dtStartTime)
			=> (long)(DateTime.Now - dtStartTime).TotalSeconds;

		public static long MillisUntilEnd(this DateTime dtEndTime)
			=> (long)(dtEndTime - DateTime.Now).TotalMilliseconds;

		/// <summary>
		/// Beware casting this to a smaller datatype -> integer overflows can occur.
		/// </summary>
		/// <param name="dtEndTime"></param>
		/// <returns></returns>
		public static long SecondsUntilEnd(this DateTime dtEndTime)
			=> (long)(dtEndTime - DateTime.Now).TotalSeconds;
	}
}
