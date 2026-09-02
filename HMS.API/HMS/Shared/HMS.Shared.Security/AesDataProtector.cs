using System.Security.Cryptography;
using System.Text;

namespace HMS.Shared.Security;

/// <summary>
/// Helper enkripsi AES untuk kolom sensitif (NIK, nomor polis, nomor kartu, dll)
/// selaras SDD 07-Cross-Cutting Data Protection. Default memakai AES-256-GCM.
/// </summary>
public interface IDataProtector
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}

public sealed class AesDataProtector : IDataProtector
{
    private readonly byte[] _key;

    // Pre-shared credential/base64 key. Dalam produksi ganti dari Secret Manager / Env.
    public AesDataProtector(string base64Key)
    {
        _key = DecodeKey(base64Key);
    }

    public AesDataProtector(byte[] key)
    {
        if (key.Length != 32)
            throw new ArgumentException("AES-256 membutuhkan key 32 bytes.", nameof(key));
        _key = key;
    }

    private static byte[] DecodeKey(string base64Key)
    {
        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64Key);
        }
        catch (FormatException)
        {
            // Kemungkinan string biasa → hashing ke 32 bytes deterministik.
            key = SHA256.HashData(Encoding.UTF8.GetBytes(base64Key));
        }

        if (key.Length != 32)
            throw new ArgumentException("Key harus 32 bytes untuk AES-256.", nameof(base64Key));
        return key;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(12);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using (var aes = new AesGcm(_key, 16))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        // nonce(12) + tag(16) + cipher
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var full = Convert.FromBase64String(cipherText);
        if (full.Length < 12 + 16)
            throw new CryptographicException("Data terenkripsi tidak valid.");

        var nonce = full.AsSpan(0, 12).ToArray();
        var tag = full.AsSpan(12, 16).ToArray();
        var cipherBytes = full.AsSpan(12 + 16).ToArray();

        var plainBytes = new byte[cipherBytes.Length];
        using (var aes = new AesGcm(_key, 16))
        {
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}