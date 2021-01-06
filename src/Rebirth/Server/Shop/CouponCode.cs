using Npgsql;
using NpgsqlTypes;
using Rebirth.Client;

namespace Rebirth.Server.Shop
{
    /// <summary>
    /// Temporary class to hold coupon data that the handler validates against.
    /// </summary>
    public sealed class CouponCode
    {
        WvsShopClient Client { get; set; }

        public string Code { get; set; }
        public int NX { get; set; }
        public int[] Items { get; set; }

        public bool IncorrectAccount { get; set; }
        public bool Expired { get; set; }
        public bool Invalid { get; set; }
        public bool Used { get; set; }

        public CouponCode(string code, WvsShopClient c)
        {
            if (code.Length > 16)
                return;

            IncorrectAccount = true;
            Expired = true;
            Invalid = true;
            Used = true;

            Client = c;
            Code = code;

            // --------------- DB Tables To Load
            // coupon_code <- check this to see if coupon exists
            // acc_created_for <- is coupon locked to a specific account
            // char_used_by <- has coupon been used
            // nx_amount
            // nx_items

            using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
            {
                conn.Open();

                var select = $"SELECT acc_created_for, char_used_by, nx_amount, nx_items, expiration " +
                             $"FROM {Constants.DB_All_World_Schema_Name}.cs_coupon_codes " +
                             $"WHERE coupon_code = @code";

                using (var cmd = new NpgsqlCommand(select, conn))
                {
                    cmd.Parameters.AddWithValue("code", Code);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            Invalid = false;

                            if (r.GetInt32(0) == c.Account.ID || r.GetInt32(0) <= 0)
                                IncorrectAccount = false;

                            if (r.GetInt32(1) <= 0)
                                Used = false;

                            try
                            { // seems to throw error when value is null so wrapping it in try-catch to avoid that
                                if (r.GetTimeStamp(4) >= NpgsqlDateTime.Now)
                                {
                                    Expired = false;
                                }
                            }
                            catch
                            {
                                Expired = false;
                            }

                            NX = r.GetInt32(2);
                            Items = r.GetValue(3) as int[];
                        }
                }
            }
        }

        public void Dispose()
        {   // --------------- DB Tables To Update
            // char_used_by
            // date_used
            // used_by_ip

            using (var conn = new NpgsqlConnection(Constants.DB_World0_ConString))
            {
                conn.Open();

                var update = $"UPDATE {Constants.DB_All_World_Schema_Name}.cs_coupon_codes " +
                             $"SET char_used_by = @charId, date_used = @datetime, used_by_ip = @ip " +
                             $"WHERE coupon_code = @code";

                using (var cmd = new NpgsqlCommand(update, conn))
                {
                    cmd.Parameters.AddWithValue("charId", Client.Character.Stats.dwCharacterID);
                    cmd.Parameters.AddWithValue("datetime", NpgsqlDateTime.Now);
                    cmd.Parameters.AddWithValue("ip", Client.Host);
                    cmd.Parameters.AddWithValue("code", Code);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
