using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Shared.Security;

/// <summary>
/// Extension untuk mendaftarkan layanan Security ke DI container.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    /// Mendaftarkan CurrentUser, JWT token service, dan (opsional) data protector.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Konfigurasi; membaca section "Jwt" dan "DataProtection".</param>
    public static IServiceCollection AddHmsSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<HMS.Shared.Abstractions.IExecutionContext, CurrentUser>();
        services.AddScoped<CurrentUser>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.AddSingleton(jwtOptions);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Data protection
        var protectionKey = configuration["DataProtection:Base64Key"]
            ?? configuration["Security:DataProtectionKey"];
        if (!string.IsNullOrWhiteSpace(protectionKey))
        {
            services.AddScoped<IDataProtector>(_ => new AesDataProtector(protectionKey));
        }

        return services;
    }
}