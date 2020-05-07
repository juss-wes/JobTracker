using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace JobTracker.Security
{
    /// <summary>
    /// Password hash logic to avoid storing passwords in plaintext on the db
    /// </summary>
    /// <remarks>Based on: https://medium.com/dealeron-dev/storing-passwords-in-net-core-3de29a3da4d2</remarks>
    public sealed class PasswordHasher : IPasswordHelper
    {
        private const int SaltSize = 16; // 128 bit 
        private const int KeySize = 32; // 256 bit

        public PasswordHasher(HashingOptions options)
        {
            Options = options;
        }

        private HashingOptions Options { get; }

        /// <summary>
        /// Helper function to generate a random salt to use when hashing the password. The salt should be stored in the database
        /// when the user is created, as you need it to hash the plaintext password
        /// </summary>
        /// <returns></returns>
        public static string GenerateSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using var keyGenerator = RandomNumberGenerator.Create();

            keyGenerator.GetBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        public string Hash(string password)
        {
            using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Options.Iterations, HashAlgorithmName.SHA512);
            var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
            var salt = Convert.ToBase64String(algorithm.Salt);

            return $"{Options.Iterations}.{salt}.{key}";
        }

        /// <summary>
        /// Check the hashed password against the supplied password and verifies the if the passwords match.
        /// </summary>
        /// <param name="hash">The hashed password from the database</param>
        /// <param name="password">The password entered by the user</param>
        /// <returns>Verified is true if the password matched, false if it didn't. Needs Upgrade identifies if the number of iterations needs increased (true) or is still ok (false)</returns>
        public (bool Verified, bool NeedsUpgrade) Check(string hash, string password)
        {
            var parts = hash.Split('.', 3);

            if (parts.Length != 3)
            {
                throw new FormatException("Unexpected hash format. " +
                  "Should be formatted as `{iterations}.{salt}.{hash}`");
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            var needsUpgrade = iterations != Options.Iterations;

            using var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
            var keyToCheck = algorithm.GetBytes(KeySize);

            var verified = keyToCheck.SequenceEqual(key);

            return (verified, needsUpgrade);
        }
    }
    public sealed class HashingOptions
    {
        public int Iterations { get; set; } = 10000;
    }
}
