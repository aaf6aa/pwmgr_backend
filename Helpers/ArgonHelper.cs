using pwmgr_backend.Models;
using Konscious.Security.Cryptography;
using System.Text;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace pwmgr_backend.Helpers
{
    public static class ArgonHelper
    {
        static readonly int SALT_SIZE = 16;

        public static string HashPassword(string identifier, string password, IConfiguration configuration)
        {
            var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2id.DegreeOfParallelism = int.Parse(configuration["Parallelism"] ?? "1");
            argon2id.MemorySize = int.Parse(configuration["Memory"] ?? "12288");
            argon2id.Iterations = int.Parse(configuration["Iterations"] ?? "3");

            argon2id.Salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
            argon2id.KnownSecret = Convert.FromBase64String(configuration["Pepper"] ?? "AAAAAAAAAAAAAAAAAAAAAA==");
            argon2id.AssociatedData = Encoding.UTF8.GetBytes(identifier);

            var combinedSaltPepper = argon2id.Salt.Concat(argon2id.KnownSecret).ToArray();
            var digest = argon2id.GetBytes(32);
            return $"argon2id$v=19$m={argon2id.MemorySize},t={argon2id.Iterations},p={argon2id.DegreeOfParallelism}${Convert.ToBase64String(combinedSaltPepper)}${Convert.ToBase64String(digest)}";
        }

        public static PasswordVerificationResult VerifyHashedPassword(string identifier, string passwordHash, string password, IConfiguration configuration)
        {
            var parts = passwordHash.Split('$');
            var parameters = parts[2].Split(',');
            var saltPepper = Convert.FromBase64String(parts[3]);
            var expectedDigest = Convert.FromBase64String(parts[4]);

            var argon2id = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2id.DegreeOfParallelism = int.Parse(parameters[2].Substring(2));
            argon2id.MemorySize = int.Parse(parameters[0].Substring(2));
            argon2id.Iterations = int.Parse(parameters[1].Substring(2));

            argon2id.Salt = saltPepper.Take(SALT_SIZE).ToArray();
            argon2id.KnownSecret = saltPepper.Skip(SALT_SIZE).ToArray();
            argon2id.AssociatedData = Encoding.UTF8.GetBytes(identifier);

            var actualDigest = argon2id.GetBytes(32);
            if (!actualDigest.SequenceEqual(expectedDigest))
            {
                return PasswordVerificationResult.Failed;
            }

            // Check if any of the parameters have changed
            if (argon2id.DegreeOfParallelism != int.Parse(configuration["Parallelism"] ?? "1") ||
                argon2id.MemorySize != int.Parse(configuration["Memory"] ?? "12288") ||
                argon2id.Iterations != int.Parse(configuration["Iterations"] ?? "3") ||
                !argon2id.KnownSecret.SequenceEqual(Convert.FromBase64String(configuration["Pepper"] ?? "AAAAAAAAAAAAAAAAAAAAAA==")))
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            return PasswordVerificationResult.Success;
        }
    }
}
