using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Domain.Services;
using KaopizAuth.Infrastructure.Data;
using KaopizAuth.Infrastructure.Data.Repositories;
using KaopizAuth.Infrastructure.Services;
using KaopizAuth.Infrastructure.Services.Authentication;
using KaopizAuth.Infrastructure.Services.Background;
using KaopizAuth.Infrastructure.Services.Domain;
using KaopizAuth.Infrastructure.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KaopizAuth.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Register the context interface
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Add repositories
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add domain services
        services.AddScoped<IRefreshTokenDomainService, RefreshTokenDomainService>();

        // Add services
        services.AddSingleton<IRsaKeyService, RsaKeyService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IDeviceFingerprintService, DeviceFingerprintService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IJwtBlacklistService, JwtBlacklistService>();

        // Add background services
        services.AddHostedService<TokenCleanupService>();

        return services;
    }
}