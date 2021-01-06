//using Rebirth.Commands;
//using Rebirth.Custom.Emoji;
//using System.Linq;

//namespace Rebirth.Custom.Commands
//{
//	public sealed class EmojiCmd : Command
//	{
//		public override string Name => "emoji";
//		public override string Parameters => "<name/help>";

//		public override void Execute(CommandCtx ctx)
//		{
//			var name = ctx.NextString().ToLower();

//			//if(name == "load")
//			//{
//			//foreach (var emoji in EmojiManager.Instance.Entries)
//			//{
//			//	ctx.SendPacket(CPacket.Custom.Emoji($":{emoji.Name}:", emoji.Data));
//			//}

//			//    ctx.Character.Action.SystemMessage("Loaded all emojis, please do not execute this command again");
//			//}
//			if (name == "help" || name == "name")
//			{
//				//ctx.Character.Action.SystemMessage("To use this feature use the command < @emoji load> ONCE per MapleStory instance");
//				ctx.Character.Action.SystemMessage("Available Emojis: ");

//				foreach (var emoji in EmojiManager.Instance.Entries)
//				{
//					ctx.Character.Action.SystemMessage("\t{0}", emoji.Name);
//				}

//			}
//			else
//			{
//				var emoji = EmojiManager.Instance.Entries.FirstOrDefault(x => x.Name == name);

//				if (emoji != null)
//				{
//					var user = ctx.Character;
//					var sText = $":{emoji.Name}:<br><br><br>";

//					ctx.Character.Field.Broadcast(CPacket.UserChat(user.dwId, sText, false, true));
//				}
//			}
//		}
//	}
//}
