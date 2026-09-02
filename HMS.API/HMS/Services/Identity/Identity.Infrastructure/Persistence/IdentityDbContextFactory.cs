using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HMS.Identity.Infrastructure.Persistence;

/// <summary>
/// Design-time factory agar `dotnet ef` bisa build DbContext tanpa runtime DI.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=HMS_Identity;User Id=sa;Password=Your_password123;TrustServerCertificate=True;");

        return new IdentityDbContext(optionsBuilder.Options);
    }
}