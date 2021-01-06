using Rebirth.Server.Center;

namespace Rebirth.Commands.Impl
{
    public sealed class ComeHereCommand : Command
    {
        public override string Name => "comehere";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var nMapId = ctx.Character.Field.MapId;

            MasterManager.CharacterPool.ForEach(c =>
            {
                if(c != ctx.Character)
                    c.Action.SetField(nMapId);
            });
        }
    }
}