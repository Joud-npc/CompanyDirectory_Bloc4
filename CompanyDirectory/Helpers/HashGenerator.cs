using System.Security.Cryptography;

namespace CompanyDirectory.Helpers
{
    public class HashGenerator
    {
        public string Generate(string password)
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
    }
}