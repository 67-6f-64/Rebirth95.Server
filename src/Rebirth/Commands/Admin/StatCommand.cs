using Rebirth.Common.Types;
using System;

namespace Rebirth.Commands.Impl
{
    public sealed class StatCommand : Command
    {
        public override string Name => "stat";
        public override string Parameters => "<flag> <value>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var character = ctx.Character;
			var input = ctx.NextString();

			if (input.ToLower().Equals("heal"))
			{
				character.Modify.Stats(mod => 
				{
					mod.MHP = 10000;
					mod.MMP = 10000;
					mod.HP = 10000;
					mod.MP = 10000;
				});
                character.ValidateStat();
				return;
			}

			ModifyStatFlags stat;


			try
			{
				stat = (ModifyStatFlags)Enum.Parse(typeof(ModifyStatFlags), input, true);
			}
			catch
			{
				ctx.Character.SendMessage(Syntax);
				return;
			}

            var value = ctx.NextString();

            character.Modify.Stats(mod =>
            {
                switch (stat)
                {
                    case ModifyStatFlags.Skin:
                        mod.Skin = Convert.ToByte(value);
                        break;
                    case ModifyStatFlags.Face:
                        mod.Face = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.Hair:
                        mod.Hair = Convert.ToInt32(value);
                        break;
                    default:
                    case ModifyStatFlags.Pet:
                    case ModifyStatFlags.Pet2:
                    case ModifyStatFlags.Pet3:
                        // TODO: do something
                        break;
                    case ModifyStatFlags.Level:
                        mod.Level = Convert.ToByte(value);
                        break;
                    case ModifyStatFlags.Job:
                        mod.Job = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.STR:
                        mod.STR = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.DEX:
                        mod.DEX = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.INT:
                        mod.INT = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.LUK:
                        mod.LUK = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.HP:
                        mod.HP = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.MaxHP:
                        mod.MHP = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.MP:
                        mod.MP = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.MaxMP:
                        mod.MMP = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.AP:
                        mod.AP = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.SP:
                        mod.SP = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.EXP:
                        mod.EXP = Convert.ToInt32(value);
                        break;
                    case ModifyStatFlags.POP:
                        mod.POP = Convert.ToInt16(value);
                        break;
                    case ModifyStatFlags.Money:
                        mod.Money = Convert.ToInt32(value);
                        break;
                        //case ModifyStatFlags.TempEXP:
                        //    mod.TempEXP = Convert.ToInt32(value);
                        //    break;
                }
            });
        }
    }
}
