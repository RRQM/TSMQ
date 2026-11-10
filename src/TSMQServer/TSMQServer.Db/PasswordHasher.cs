using System;
using System.Security.Cryptography;

namespace TSMQServer.Db;

/// <summary>
/// 使用随机盐和 PBKDF2-SHA256 存储及校验密码。
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "$tsmq$pbkdf2-sha256$v1$600000$";
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    /// <summary>
    /// 为密码生成包含算法版本、迭代次数及随机盐的哈希。
    /// </summary>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return Prefix + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 校验密码，不接受明文或无效格式。
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (password == null || !TryDecode(storedHash, out var salt, out var expectedHash))
        {
            return false;
        }

        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }

    /// <summary>
    /// 判断存储值是否为受支持的密码哈希。
    /// </summary>
    public static bool IsHash(string value) => TryDecode(value, out _, out _);

    private static bool TryDecode(string value, out byte[] salt, out byte[] hash)
    {
        salt = Array.Empty<byte>();
        hash = Array.Empty<byte>();
        if (value == null || value.Length != Prefix.Length + 24 + 1 + 44 || !value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var parts = value.Substring(Prefix.Length).Split('$');
        if (parts.Length != 2)
        {
            return false;
        }

        salt = new byte[SaltSize];
        hash = new byte[HashSize];
        return Convert.TryFromBase64String(parts[0], salt, out var saltLength) && saltLength == SaltSize
            && Convert.TryFromBase64String(parts[1], hash, out var hashLength) && hashLength == HashSize;
    }
}
