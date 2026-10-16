using System.Security.Cryptography;
using System.Text;

namespace CrmApi.Auth;

public static class PasswordHasher
{
    public static string GenerateSalt()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexString(bytes);
    }

    public static string Hash(string password, string salt)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string salt, string hash) =>
        Hash(password, salt).Equals(hash, StringComparison.OrdinalIgnoreCase);
}
