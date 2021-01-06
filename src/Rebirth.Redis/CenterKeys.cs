namespace Rebirth.Redis
{
    static class CenterKeys
    {
        public static string GetAccountKey(int id) => $"account-online-{id}";

        public static string GetCharacterKey(int id) => $"character-online-{id}";

        public static string GetMigrateKey(int id) => $"character-migrate-{id}";

        public static string GetCSITCKey(int id) => $"character-csitc-{id}";
    }
}
