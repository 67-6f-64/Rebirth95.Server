using Autofac;
using log4net;
using Rebirth.Redis;
using System;

namespace Rebirth.Tools
{
	public static class Discord
	{
		static readonly ILog Log = LogManager.GetLogger(typeof(Discord));

		public static void PostMessage(string message)
		{
			try
			{
				var pCenter = ServerApp.Container.Resolve<CenterStorage>();
				pCenter.Multiplexer().GetSubscriber().Publish("discord", message);

				Log.InfoFormat("[PostMessage] {0}", message);
			}
			catch (Exception ex)
			{
				Log.WarnFormat("[PostMessage] Exception: {0}", ex);
			}
		}

	}
}
