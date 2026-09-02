using Microsoft.Extensions.DependencyInjection;

namespace HMS.Shared.Inbox.Extensions;

/// <summary>
/// Extension untuk mendaftarkan Inbox pattern ke DI container.
/// Consumer service cukup memanggil AddHmsInbox lalu menyediakan IInboxStore
/// (biasanya implementasi EF Core) melalui AddScoped.
/// </summary>
public static class InboxServiceCollectionExtensions
{
    public static IServiceCollection AddHmsInbox(this IServiceCollection services)
    {
        // InboxService berisi logika idempotency yang dipakai semua consumer.
        services.AddScoped<IInboxService, InboxService>();
        return services;
    }
}