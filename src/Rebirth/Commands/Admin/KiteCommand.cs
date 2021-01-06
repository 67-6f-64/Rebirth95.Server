using Rebirth.Field.FieldObjects;

namespace Rebirth.Commands.Impl
{
	public sealed class KiteCommand : Command
    {
        public override string Name => "kite";
        public override string Parameters => "<message>";
        public override bool IsRestricted => true;

        private static readonly int[] Floaties =
        {
            5080000,    //Korean Kite
            5080001,    //Heart Balloon
            5080002,    //Graduation Banner
            5080003     //Admission Banner
        };

        public override void Execute(CommandCtx ctx)
        {
            var msg = new CMessageBox();

            msg.nItemID = Floaties.Random();
            msg.sCharacterName = ctx.Character.Stats.sCharacterName;
            msg.sMessage = ctx.Remaining();
            msg.Position.X = ctx.Character.Position.X;
            msg.Position.Y = ctx.Character.Position.Y;
            
            ctx.Character.Field.Kites.Add(msg);
        }
    }
}
