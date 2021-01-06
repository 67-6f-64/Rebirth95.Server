using log4net;
using Rebirth.Characters;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Rebirth.Commands
{
    public class CommandHandle : KeyedCollection<string, Command>
    {
        public static ILog Log = LogManager.GetLogger(typeof(CommandHandle));

        public CommandHandle()
        {
            Assembly.GetExecutingAssembly().GetTypes().ForEach(t =>
            {
                var cmdType = typeof(Command);

                if (t.IsSubclassOf(cmdType))
                {
                    var cmdInst = Activator.CreateInstance(t) as Command;
                    Add(cmdInst);
                }
            });
        }

        private void ExecuteCommand(Character caller, string[] split, Command command)
        {
            if (caller.Account.AccountData.Admin == 0)
            {
                if (GameConstants.BlockedFromCommands(caller))
                {
                    caller.Action.SystemMessage("[Command] You are blocked from using commands.");
                    return;
                }
            }

            if (command.IsDisabled)
            {
                caller.Action.SystemMessage("[Command] Disabled command.");
                return;
            }

            if (command.IsRestricted && caller.Account.AccountData.Admin == 0) //TODO: GM Levels
            {
                caller.Action.SystemMessage("[Command] Restricted command.");
                return;
            }
                       
            try
            {
                var ctx = new CommandCtx(caller, split);
                command.Execute(ctx);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("CommandException: {0}", ex);

                if (caller.Account.AccountData.Admin > 0)
                {
                    caller.SendMessage($"[Command] Error: " + ex.Message);
                }

                caller.Action.SystemMessage("[Command] An error occurred. Syntax: @{0} {1}", command.Name, command.Parameters);
            }
        }

        private void ExecuteHelpCommand(Character caller)
        {
            caller.Action.SystemMessage("[Command] You have access to the following:");

            var cmds = this.OrderBy(cmd => cmd.Name);

            foreach (var cmd in cmds)
            {
                if (cmd.IsDisabled)
                    continue;

                if (cmd.IsRestricted && caller.Account.AccountData.Admin == 0)
                    continue;

                caller.Action.SystemMessage("\t@{0} {1}", cmd.Name, cmd.Parameters);
            }
        }

        public bool Execute(Character caller, string text)
        {
            const int IndicatorLen = 1;

            if (text.StartsWith("@") || text.StartsWith("!"))
            {
                var split = text.Split(' ');

                var commandName = split[0].Remove(0, IndicatorLen);

                if (commandName.EqualsIgnoreCase("help") || commandName.EqualsIgnoreCase("command") || commandName.EqualsIgnoreCase("commands"))
                {
                    ExecuteHelpCommand(caller);
                }
                else if (TryGetValue(commandName, out var command))
                {
                    ExecuteCommand(caller, split, command);
                }
                else
                {
                    caller.SendMessage("[Command] Unable to find command. See @help for a list of commands.");
                }

                return true;
            }
            return false;
        }

        protected override string GetKeyForItem(Command item) => item.Name;
    }
}
