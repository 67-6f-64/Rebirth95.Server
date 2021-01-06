using Rebirth.Server.Center;

namespace Rebirth.Commands.Impl
{
    public sealed class SaveAllCommand : Command
    {
        public override string Name => "saveall";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {

            foreach (var item in MasterManager.CharacterPool.ToArray())
            {
                item.Save();
            }

            MasterManager.TempInvManager.SaveAll();

            ctx.Character.SendMessage("All characters and temporary inventories have been saved");
        }
    }
}
