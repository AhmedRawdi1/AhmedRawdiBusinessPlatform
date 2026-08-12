using System.Security.Cryptography;

namespace AhmedRawdiBusinessPlatform.Services
{
    public sealed class PasswordService : IPasswordService
    {
        private const string Algorithm = "pbkdf2-sha256";
        private const int Iterations = 310_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"${Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(passwordHash)) return false;

            try
            {
                var parts = passwordHash.Split('$', StringSplitOptions.None);
                if (parts.Length != 5 || parts[1] != Algorithm || !int.TryParse(parts[2], out var iterations)) return false;
                if (iterations < 100_000 || iterations > 1_000_000) return false;

                var salt = Convert.FromBase64String(parts[3]);
                var expected = Convert.FromBase64String(parts[4]);
                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
