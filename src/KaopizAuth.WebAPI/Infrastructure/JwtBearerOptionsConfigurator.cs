using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace KaopizAuth.WebAPI.Infrastructure;

/// <summary>
/// Configures JWT Bearer options with the RSA key service
/// </summary>
public class JwtBearerOptionsConfigurator : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly IRsaKeyService _rsaKeyService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtBearerOptionsConfigurator> _logger;

    public JwtBearerOptionsConfigurator(
        IRsaKeyService rsaKeyService,
        IConfiguration configuration,
        ILogger<JwtBearerOptionsConfigurator> logger)
    {
        _rsaKeyService = rsaKeyService;
        _configuration = configuration;
        _logger = logger;
    }

    public void PostConfigure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = _rsaKeyService.GetRsaSecurityKey(),
                ClockSkew = TimeSpan.Zero
            };

            // Add JWT blacklist checking
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var blacklistService = context.HttpContext.RequestServices.GetRequiredService<IJwtBlacklistService>();
                    
                    // Get the JWT token from the Authorization header
                    if (context.Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
                    {
                        var authHeader = authHeaderValues.FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                        {
                            var jwtToken = authHeader.Substring("Bearer ".Length);
                            var isBlacklisted = await blacklistService.IsTokenBlacklistedAsync(jwtToken);

                            if (isBlacklisted)
                            {
                                _logger.LogWarning("Blocked blacklisted JWT token");
                                context.Fail("Token has been revoked");
                            }
                        }
                    }
                }
            };
        }
    }
}
