using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KaopizAuth.Infrastructure.Services.Authentication;

/// <summary>
/// JWT token service implementation
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured"));
        
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "15")),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured"));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            await tokenHandler.ValidateTokenAsync(token, validationParameters);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string?> GetUserIdFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            return Task.FromResult(jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}