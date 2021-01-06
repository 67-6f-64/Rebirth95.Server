using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Provider.Template.String;
using Rebirth.Server.Center;
using Rebirth.Server.Center.Template;

namespace Rebirth.Commands.Player
{
    public class DropCommand : Command
	{
		public override string Name => "mobdrops";
		public override string Parameters => string.Empty;

		public override void Execute(CommandCtx ctx)
		{
			var anMobTemplate = 
				ctx.Character.Field.Mobs
				.Select(mob => mob.nMobTemplateId)
				.Distinct();

			ctx.Character.SendMessage("Monster Drop List:");

			foreach (var nTemplateId in anMobTemplate)
			{
				var szMobName = MasterManager.StringData[StringDataType.Mob][nTemplateId].Name;
				var pDropNode = MasterManager.MobDropGenerator[nTemplateId];

				ctx.Character.SendMessage($"{szMobName} ({nTemplateId})");

				foreach(var pDrop in pDropNode.Drops)
                {
					var szItemName = MasterManager.StringData[StringDataType.Item][pDrop.ItemID].Name;

					if(szItemName != null)
                    {
						ctx.Character.SendMessage($"\t - {szItemName}");
					}
				}
			}
		}
	}
}
