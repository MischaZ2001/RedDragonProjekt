using System;
using System.Security.Cryptography;
using System.Text;

namespace RedDragon
{
    public static class PasswordHasher
    {
        public static string CreateSalt(int bytes = 16)
        {
            var data = new byte[bytes];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public static string Hash(string password, string saltBase64)
        {
            var saltBytes = Convert.FromBase64String(saltBase64);
            var pwBytes = Encoding.UTF8.GetBytes(password ?? "");

            var combined = new byte[saltBytes.Length + pwBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
            Buffer.BlockCopy(pwBytes, 0, combined, saltBytes.Length, pwBytes.Length);

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(combined);
            return Convert.ToBase64String(hash);
        }
    }
}

