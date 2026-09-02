using HMS.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace HMS.Identity.Infrastructure.Security;

/// <summary>
/// Hashing password PBKDF2 (Rfc2898) dengan per-iterations tinggi dan salt acak,
/// sesuai praktik keamanan kredensial.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

    public (string Hash, string Salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = KeyDerivation.Pbkdf2(password, salt, Prf, Iterations, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var computed = KeyDerivation.Pbkdf2(password, saltBytes, Prf, Iterations, HashSize);
        return CryptographicOperations.FixedTimeEquals(
            computed, Convert.FromBase64String(hash));
    }
}