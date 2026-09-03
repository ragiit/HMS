using HMS.Shared.Abstractions.Persistence;
using HMS.Shared.Messaging;
using HMS.Shared.Outbox;
using HMS.Shared.Security;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

/// <summary>
/// Registrasi dependensi Infrastructure layer (EF Core, repositori, keamanan, outbox).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' tidak ditemukan.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories & UoW
        services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IDomainEventMapper, DomainEventMapper>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        // Auth & security
        services.AddHmsSecurity(configuration);
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        // Messaging + outbox processor
        //services.AddHmsMessaging(configuration);
        //services.AddHmsOutboxProcessor();

        // Seeder
        services.AddScoped<IdentityDbSeeder>();

        return services;
    }
}