using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

/// <summary>
/// Repository khusus user yang mampu memuat relasi Roles secara eager
/// (karena token membutuhkan list roles user).
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
}