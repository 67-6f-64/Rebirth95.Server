using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Rebirth.Game;
using Rebirth.Provider.Template.String;
using Rebirth.Server.Center;

namespace Rebirth.Commands.Player
{
	public class PlayTestCommand : Command
	{
		public override string Name => "playtest";
		public override string Parameters => string.Empty;

		private const bool _bActive = true;
		public override void Execute(CommandCtx ctx)
		{
			if (ctx.Character.Field.Template.HasMigrateLimit())
			{
				ctx.Character.SendMessage("Can't be used in this map.");
				return;
			}

			if (_bActive)
			{
				if (!ctx.Empty)
				{
					switch (ctx.NextString().ToLowerInvariant())
					{
						case "scrolluse":
							{
								ctx.Character.SendMessage("Top 5 Used Scrolls:");
								var limit = 5;

								foreach (var scroll in ctx.Character.StatisticsTracker.GetScrollUseOrdered())
								{
									var usecount = scroll.Value.nSuccessCount + scroll.Value.nFailedCount;
									ctx.Character.SendMessage(
										$"Uses: {usecount,5} Success: {Math.Floor((double)scroll.Value.nSuccessCount / usecount * 100.0),3}% Destroy: {Math.Floor((double)scroll.Value.nDestroyCount / usecount * 100.0),3}% -> {MasterManager.StringData[StringDataType.Item][scroll.Key].Name}");

									if (--limit <= 0) break;
								}
							}
							break;
						case "mobkills":
							{
								ctx.Character.SendMessage("Top 5 Mob Kills:");
								var limit = 5;

								foreach (var mob in ctx.Character.StatisticsTracker.GetMobKillsOrdered())
								{
									ctx.Character.SendMessage(
										$"{mob.Value.nCount,5} -> {MasterManager.StringData[StringDataType.Mob][mob.Key].Name}");

									if (--limit <= 0) break;
								}
							}
							break;
						case "online":
							var t = ctx.Character.nTotalSecondsOnline;
							var seconds = t % 60;
							var minutes = ((t - seconds) / 60) % 60;
							var hours = (t - minutes - seconds) / 60 / 60;

							ctx.Character.SendMessage($"You have been online for {hours} hours, {minutes} minutes, and {seconds} seconds.");
							break;
						case "sheepwolf":
							ctx.Character.Quests.SetQuestRecord(QuestConstants.SHEEP_RANCH_PREVMAP_QID, ctx.Character.Field.MapId.ToString());
							ctx.Character.Action.SetField(BattlefieldData.RanchEntranceMap);
							break;
						default:
							ctx.Character.SendMessage("Current play-test options: sheepwolf, online, mobkills, scrolluse");
							ctx.Character.SendMessage("Syntax: @playtest <option>");
							break;
					}
				}
				else
				{
					ctx.Character.SendMessage("Current play-test options: sheepwolf, online, mobkills, scrolluse");
					ctx.Character.SendMessage("Syntax: @playtest <option>");
				}
			}
			else
			{
				ctx.Character.SendMessage("There are no current features available for play-testing.");
			}
		}
	}
}
