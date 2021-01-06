using Rebirth.Server.Center;

namespace Rebirth.Commands.Admin
{
    public class MaxSkillCommand : Command
    {
        public override string Name => "maxskills";
        public override string Parameters => string.Empty;
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var p = ctx.Character;

            p.Skills.MaxSkillLevel(MasterManager.SkillTemplates.GetJobSkills(p.Stats.nJob).ToArray());
        }
    }
}
