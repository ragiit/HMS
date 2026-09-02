namespace HMS.Shared.Abstractions.Exceptions;

/// <summary>
/// Base exception untuk domain HMS. Mapping HTTP status di setiap API service.
/// </summary>
public abstract class HmsException : Exception
{
    protected HmsException(string message) : base(message)
    {
    }

    protected HmsException(string message, Exception inner) : base(message, inner)
    {
    }
}

/// <summary>Dilempar saat data/aggregate tidak ditemukan → HTTP 404.</summary>
public sealed class NotFoundException : HmsException
{
    public NotFoundException(string name, object key)
        : base($"Resource '{name}' dengan id '{key}' tidak ditemukan.") { }

    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Dilempar saat pelanggaran aturan bisnis/domain → HTTP 409.</summary>
public sealed class BusinessRuleViolationException : HmsException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}

/// <summary>Dilempar saat validasi input gagal → HTTP 400.</summary>
public sealed class ValidationException : HmsException
{
    public IReadOnlyList<(string Field, string Message)> Failures { get; }

    public ValidationException(IReadOnlyList<(string Field, string Message)> failures)
        : base("Validasi gagal.")
    {
        Failures = failures;
    }

    public ValidationException(string field, string message)
        : this(new[] { (field, message) }) { }
}

/// <summary>Dilempar saat user tidak terautentikasi → HTTP 401.</summary>
public sealed class UnauthorizedException : HmsException
{
    public UnauthorizedException(string message = "Anda harus login.") : base(message)
    {
    }
}

/// <summary>Dilempar saat user tidak punya hak akses → HTTP 403.</summary>
public sealed class PermissionDeniedException : HmsException
{
    public PermissionDeniedException(string message = "Anda tidak memiliki izin.") : base(message)
    {
    }
}

/// <summary>Dilempar saat terjadi konflik concurrency (optimistic lock) → HTTP 409.</summary>
public sealed class ConcurrencyException : HmsException
{
    public ConcurrencyException(string message = "Data telah diubah oleh pengguna lain.") : base(message)
    {
    }
}

/// <summary>Dilempar saat terjadi konflik resource (misal double-booking) → HTTP 409.</summary>
public sealed class ConflictException : HmsException
{
    public ConflictException(string message) : base(message)
    {
    }
}