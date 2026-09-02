using HMS.Identity.Application.Abstractions;
using HMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserRepository(IdentityDbContext dbContext) => _dbContext = dbContext;

    public Task<User?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        => _dbContext.Users
            .Include(u => u.Roles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == login || u.Email == login, cancellationToken);
}