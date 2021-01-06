using Npgsql;
using Rebirth.Entities;

namespace Rebirth.Server.Login
{
    public class AutoRegister
    {
        public static bool Handle(Account account, string username, string password)
        {
            if (!Constants.AutoRegister)
                return false;

            account.Username = username;
            account.Password = HashFactory.GenerateHashedPassword(password);

            using (var conn = new NpgsqlConnection(Constants.DB_Global_ConString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"INSERT INTO {Constants.DB_Global_Schema}.accounts (username, password) " +
                                                   $"VALUES (@user, @pass)", conn))
                {
                    cmd.Parameters.AddWithValue("user", account.Username);
                    cmd.Parameters.AddWithValue("pass", account.Password);
                    cmd.ExecuteNonQuery();
                }
            }

            return account.Init();
        }
    }
}
