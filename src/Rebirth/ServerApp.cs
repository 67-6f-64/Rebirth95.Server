using Autofac;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Rebirth.Commands;
using Rebirth.Python;
using Rebirth.Redis;
using Rebirth.Scripts;
using Rebirth.Server.Center;
using StackExchange.Redis;
using System;
using System.IO;
using System.Reflection;

namespace Rebirth
{
    public static class ServerApp
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(ServerApp));

        //TODO: Phase this nasty anti pattern when we split up servers
        public static IContainer Container { get; private set; }

        private static void ConfigureLogging()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var assembly = Assembly.GetEntryAssembly();
            var repo = LogManager.GetRepository(assembly);

            var path = AppDomain.CurrentDomain.BaseDirectory + "log4net.config";
            var file = new FileInfo(path);

            XmlConfigurator.ConfigureAndWatch(repo, file);
        }

        private static bool ConfigureServices()
        {
            try
            {
                PyEngine.Initialize();
            }
            catch
            {
                Log.Error("Unable to connect to python driver.");
                Log.Error("Please uninstall all python builds (2.7, 3.5, etc) and reinstall python 3.7");
                return false;
            }

            var basePath = Directory.GetCurrentDirectory();

            const string configFileName = "config.json";

            try
            {

                var config = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(configFileName, false)
                    .Build();

                var mainThread = new ServerAppThread(Constants.ServerThreadFPS); //TODO: From config

                var builder = new ContainerBuilder();

                builder.RegisterInstance(config).As<IConfiguration>();
                builder.RegisterInstance(mainThread);

                builder.RegisterType<WvsCenter>().AsSelf().SingleInstance();
                builder.RegisterType<CenterStorage>().AsSelf().SingleInstance();
                builder.RegisterType<CommandHandle>().AsSelf().SingleInstance();
                builder.RegisterType<ScriptManager>().AsSelf().SingleInstance();

                Container = builder.Build();

                return true;
            }
            catch
			{
                Log.Error("Unable to build config.json file.");
                Log.Error($"File name: {configFileName}");
                Log.Error($"File directory: {basePath}");
                Log.Error($"Full file path: {basePath}\\{configFileName}");
                return false;
			}
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.ErrorFormat("Unhandled Exception: {0}", e.ExceptionObject);
        }

        private static bool OnConsoleCommand(string input)
        {

#if DEBUG
            if (string.IsNullOrEmpty(input)) // Exit if you just press enter ( no text )
                return false;
#endif
            var split = input.Split(' ');

            switch (input.ToLower().Trim())
            {
                case "online":
                    ShowOnline();
                    break;
                case "saveall":
                    SaveAll();
                    break;
                case "flushscript":
                    FlushScripts();
                    break;
                case "shutdown":
                case "quit":
                case "exit":
                    SaveAll();
                    SafeDisconnectAllClients();
                    return false;
            }

            return true;
        }

        private static void SafeDisconnectAllClients()
        {
            int count = 0;
            Container.Resolve<WvsCenter>().WvsGames.ForEach(g =>
            {
                g.Clients.ForEach(c =>
                {
                    c.Disconnect();
                    count += 1;
                }); // using this for now so we dont need to flush redis
            });
            Log.Info($"[Server] Safely disconnected {count} clients.");
        }

        private static void SaveAll()
        {
            var errorCount = 0;
            foreach (var item in MasterManager.CharacterPool.ToArray())
            {
                try
                {
                    item.Save();
                }
                catch { errorCount += 1; }
            }

            MasterManager.TempInvManager.SaveAll();
			MasterManager.GuildManager.SaveGuilds();

            Log.Info($"[Server] Saved {MasterManager.CharacterPool.ToArray().Length} characters with {errorCount} errors.");
        }

        private static void FlushScripts()
        {
            var provider = Container.Resolve<ScriptManager>();
            provider.ClearScriptCache();

            Log.Info("[Server] Flushed script cache.");
        }

        private static void ShowOnline()
        {
            var aCharacters = MasterManager.CharacterPool.ToArray();

            Log.InfoFormat("[Server] {0} players online", aCharacters.Length);

            foreach (var pUser in aCharacters)
            {
                Log.InfoFormat("[Server] {0} - {1} - Channel {2} - Map {3}",
                    pUser.dwId,
                    pUser.Stats.sCharacterName,
                    pUser.ChannelID + 1,
                    pUser.Stats.dwPosMap
                    );
            }
        }

        private static void Run()
        {
            var executor = Container.Resolve<ServerAppThread>();
            executor.Start();

            using (var app = Container.Resolve<WvsCenter>())
            {
                app.Start();

                while (true)
                {
					var input = Console.ReadLine();

                    if (!OnConsoleCommand(input))
                        break;
                }

                app.Stop();
            }

            executor.Stop();
            executor.Join();
        }

        private static void Main(string[] args)
        {
            Console.Title = $"Rebirth v{Constants.VersionMajor}.{Constants.VersionMinor}";
            Console.ForegroundColor = ConsoleColor.White;

            ConfigureLogging();
            if (ConfigureServices())
            {
                Run();
            }
        }
    }
}