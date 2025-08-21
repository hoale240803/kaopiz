using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KaopizAuth.Application;

/// <summary>
/// Application layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add AutoMapper
        services.AddAutoMapper(assembly);

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}