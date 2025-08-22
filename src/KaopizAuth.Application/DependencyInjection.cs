using FluentValidation;
using KaopizAuth.Application.Authorization;
using KaopizAuth.Application.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
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

        // Add Authentication Strategies
        services.AddScoped<EndUserAuthStrategy>();
        services.AddScoped<AdminAuthStrategy>();
        services.AddScoped<PartnerAuthStrategy>();
        services.AddScoped<IUserAuthenticationStrategyFactory, UserAuthenticationStrategyFactory>();

        // Add Authorization Handlers
        services.AddScoped<IAuthorizationHandler, UserTypeAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}