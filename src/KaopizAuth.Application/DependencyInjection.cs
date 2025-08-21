using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace KaopizAuth.Application;

/// <summary>
/// Application layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        // Add AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        
        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}