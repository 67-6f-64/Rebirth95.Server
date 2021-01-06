using Npgsql;

namespace Rebirth.Commands.Impl
{
    public sealed class PermNpcCommand : Command
    {
        public override string Name => "pnpc";
        public override string Parameters => "<npc id>";
        public override bool IsRestricted => true;

        public override void Execute(CommandCtx ctx)
        {
            var c = ctx.Character;

            var dwTemplateID = ctx.NextInt();

            using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"INSERT INTO {Constants.DB_All_World_Schema_Name}.custom_field_life (field_id, life_type, life_id, life_name, x_pos, y_pos, foothold, min_click_pos, max_click_pos, respawn_time, flags, cy, f) VALUES (@field_id, @life_type, @life_id, @life_name, @x_pos, @y_pos, @foothold, @min_click_pos, @max_click_pos, @respawn_time, @flags, @cy, @f)", conn))
                {
                    cmd.Parameters.AddWithValue("field_id", c.Field.MapId);
                    cmd.Parameters.AddWithValue("life_type", "npc");
                    cmd.Parameters.AddWithValue("life_id", dwTemplateID);
                    cmd.Parameters.AddWithValue("life_name", string.Empty);
                    cmd.Parameters.AddWithValue("x_pos", c.Position.X);
                    cmd.Parameters.AddWithValue("y_pos", c.Position.Y);
                    cmd.Parameters.AddWithValue("foothold", c.Position.Foothold);
                    cmd.Parameters.AddWithValue("min_click_pos", -100);
                    cmd.Parameters.AddWithValue("max_click_pos", 100);
                    cmd.Parameters.AddWithValue("respawn_time", 0);
                    cmd.Parameters.AddWithValue("flags", new string[] { });
                    cmd.Parameters.AddWithValue("cy", c.Position.Y);
                    cmd.Parameters.AddWithValue("f", 0);
                    cmd.ExecuteNonQuery();
                }
            }

            c.Field.Npcs.Load(c.Field.MapId); // reload
            c.SendMessage($"Added permenant NPC: {dwTemplateID}");
        }
    }
}
