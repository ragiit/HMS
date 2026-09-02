using FluentValidation;
using Identity.Application.Behaviours;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Identity.Application;

/// <summary>
/// Registrasi dependensi Application layer (MediatR + validators).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}