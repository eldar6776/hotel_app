using System.Security.Cryptography;
using System.Text;

namespace HotelPro.Core.Helpers;

public static class PinHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string HashPin(string pin)
    {
        var salt = GenerateSalt();
        var hash = ComputeHash(pin, salt);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    public static bool VerifyPin(string pin, string pinHash)
    {
        var parts = pinHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = ComputeHash(pin, salt);
        var storedHash = Convert.FromBase64String(parts[1]);

        return CryptographicOperations.FixedTimeEquals(hash, storedHash);
    }

    private static byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    private static byte[] ComputeHash(string pin, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(pin, salt, 10000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(HashSize);
    }
}
