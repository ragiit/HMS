namespace HMS.Shared.Abstractions;

/// <summary>
/// Abstraksi untuk mengakses konteks eksekusi saat ini (correlation id, user, tenant)
/// yang diisi oleh middleware dari header/claims. Sering digunakan bersama Serilog enricher.
/// </summary>
public interface IExecutionContext
{
    string CorrelationId { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? TenantId { get; }
    IReadOnlyList<string> Roles { get; }

    bool HasRole(string role);

    bool IsAuthenticated { get; }
}

/// <summary>
/// Generator correlation ID. Digunakan API Gateway / middleware untuk membuat ID unik saat
/// request masuk dan dibawa sepanjang alur sync maupun async.
/// </summary>
public static class CorrelationIdGenerator
{
    // Format tetap: 25 karakter alphanumeric (mirip .NET HubActivityContext)
    private const string Prefix = "hms-";

    private static readonly ThreadLocal<char[]> CharBuffer =
        new(() => new char[24 - Prefix.Length]);

    private static readonly long BaseTicks = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

    public static string Generate()
    {
        var ticks = (DateTime.UtcNow.Ticks - BaseTicks) * 31;
        var next = Interlocked.Increment(ref ReverseRng) + ticks;
        return Prefix + ToString((ulong)next);
    }

    private static long ReverseRng = long.MinValue;

    private static string ToString(ulong value)
    {
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var buffer = CharBuffer.Value!;
        var length = buffer.Length;
        for (var i = 0; i < length; i++)
        {
            buffer[i] = alphabet[(int)(value % 62)];
            value /= 62;
        }
        return new string(buffer);
    }
}