using System.Collections.Generic;

namespace Rebirth.Commands.Player
{
	public partial class PlayerCommand
	{
		public class Goto : Command
		{
			private readonly Dictionary<string, int> aDestinationMap = new Dictionary<string, int>
				{
					{ "henesys", 100000000 },
					{ "ellinia", 101000000 },
					{ "perion", 102000000 },
					{ "kerning", 103000000 },
					{ "lith", 104000000 },
					{ "nautilus", 120000000 },
                   // { "amoria", 680000000 },
               //     { "sleepywood", 105000000 }, //105040300 - old maple o-o
              //      { "florina", 110000000 },
                    { "orbis", 200000000 },
                   // { "happy", 209000000 },
                    { "elnath", 211000000 },
                //    { "ereve", 130000000 },
                    { "ludi", 220000000 },
					{ "omega", 221000000 },
					{ "korean", 222000000 },
					{ "aqua", 230000000 },
					{ "maya", 100000001 },
					{ "leafre", 240000000 },
					{ "mulung", 250000000 },
					{ "herb", 251000000 },
					{ "nlc", 600000000 },
					{ "shrine", 800000000 },
					{ "showa", 801000000 },
             //       { "fm", 910000000 }, // this one is handled specially
                   // { "guild", 200000301 },
                    { "fog", 105040306 },
					{ "kaede", 800040000 },
					{ "ellin", 300000000 },
                  //  { "coke", 219000000 },
                    { "kampung", 551000000 },
					{ "tot", 270000100 },
					{ "magatia", 261000000 },
					{ "future", 271000000 },
					{ "ariant", 260000000 },
                  //  { "rien", 140000000 },
           //         { "edelstein", 310000000 },
                };

			public override string Name => "goto";
			public override string Parameters => "<town name / list>";

			public override void Execute(CommandCtx ctx)
			{
				if (ctx.Character.Field.Template.HasMigrateLimit())//|| ctx.Character.Field.Template.HasTeleportItemLimit())
				{
					ctx.Character.SendMessage("This command can not be used in this map.");
					return;
				}

				string input;

				if (ctx.Empty) input = null;
				else input = ctx.NextString();

				if (input is null || input.ToLower().Equals("list") || !aDestinationMap.ContainsKey(input.ToLower()))
				{
					var toPrint = "Destinations: ";
					foreach (var item in aDestinationMap)
					{
						toPrint += $"{item.Key}, ";
					}

					ctx.Character.SendMessage(toPrint);
				}
				else
				{
					if (input.ToLower().Equals("fm"))
					{
						ctx.Character.Quests.UpdateQuestRecordInternal(7600, ctx.Character.Field.MapId.ToString());
					}

					ctx.Character.Action.SetField(aDestinationMap[input.ToLower()]);
				}

			}
		}
	}
}
