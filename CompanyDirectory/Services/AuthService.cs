using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace CompanyDirectory.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        public AuthService(IConfiguration config) { _config = config; }

        public Task<bool> AuthenticateAsync(string username, string password)
        {
            // Demo: visitor login requires password "user"
            if (!string.IsNullOrWhiteSpace(username) && password == "user") return Task.FromResult(true);
            return Task.FromResult(false);
        }

        public Task<bool> ValidateAdminPasswordAsync(string password)
        {
            var stored = _config["Admin:PasswordHash"];
            if (string.IsNullOrEmpty(stored)) return Task.FromResult(false);
            return Task.FromResult(VerifyHash(password, stored));
        }

        // PBKDF2 helpers (same as generator)
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            var salted = new byte[49];
            Buffer.BlockCopy(salt, 0, salted, 1, 16);
            Buffer.BlockCopy(hash, 0, salted, 17, 32);
            salted[0] = 0x00;
            return Convert.ToBase64String(salted);
        }

        private static bool VerifyHash(string password, string storedBase64)
        {
            try
            {
                var salted = Convert.FromBase64String(storedBase64);
                var salt = new byte[16];
                Buffer.BlockCopy(salted, 1, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);
                for (int i = 0; i < 32; i++)
                {
                    if (salted[17 + i] != hash[i]) return false;
                }
                return true;
            }
            catch { return false; }
        }
    }
}
