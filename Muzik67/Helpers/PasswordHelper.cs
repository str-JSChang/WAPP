using System.Security.Cryptography;

namespace Muzik67.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Hashes a plaintext password with a unique salt using PBKDF2-SHA256.
        /// </summary>
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a plaintext password against the stored salt + hash string.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] originalHash = Convert.FromBase64String(parts[1]);

                byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
                return CryptographicOperations.FixedTimeEquals(originalHash, testHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
