using HMS.Identity.Application.Abstractions;
using HMS.Identity.Domain;
using HMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeder data awal Identity Service: role default & user admin.
/// Dipanggil saat aplikasi start (idempoten).
/// </summary>
public sealed class IdentityDbSeeder
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public IdentityDbSeeder(IdentityDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);

        // Seed roles
        foreach (var roleName in RoleNames.All)
        {
            var normalized = roleName.ToUpperInvariant();
            if (!await _dbContext.Roles.AnyAsync(r => r.NormalizedName == normalized, cancellationToken))
            {
                _dbContext.Roles.Add(new Role(roleName, $"Role {roleName}"));
            }
        }

        // Seed admin default (hanya bila belum ada)
        if (!await _dbContext.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
        {
            var (hash, salt) = _passwordHasher.Hash("Admin@12345");
            var admin = new User("admin", "admin@hms.local", "System Administrator", null);
            admin.SetPassword(hash, salt);

            var adminRole = await _dbContext.Roles.FirstOrDefaultAsync(
                r => r.Name == RoleNames.Admin, cancellationToken);
            if (adminRole is not null)
                admin.AssignRole(adminRole);

            await _dbContext.Users.AddAsync(admin, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}