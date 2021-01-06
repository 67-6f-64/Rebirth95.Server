using Rebirth.Tools.FromWvsGlobal;

namespace Rebirth.Server.Login
{
    /**
     * Takes care of all password encryption and decryption
     */
    public static class HashFactory
    {
        /**
         * Salt complexity ranges from 4 to 31 with 10 being the BCrypt default.
         */
        public static readonly byte SaltComplexity = 6;

        /**
         * Hashes a password with a unique salt.
         */
        public static string GenerateHashedPassword(string plainText) => BCrypt.HashPassword(plainText, GenerateHashSalt);

        /**
         * Generates a random salt with the pre-defined complexity.
         */
        private static string GenerateHashSalt => BCrypt.GenerateSalt(SaltComplexity);

        /**
         * Attempts to decrypt a plaintext password with provided hash.
         */
        public static bool CheckHashedPassword(string plainText, string hashedText) => BCrypt.CheckPassword(plainText, hashedText);
    }
}