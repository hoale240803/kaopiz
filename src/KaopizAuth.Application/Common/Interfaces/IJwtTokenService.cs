namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// JWT token service interface
/// </summary>
public interface IJwtTokenService
{
    // TODO: Add methods when Domain entities are implemented
    // Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token);
    Task<string?> GetUserIdFromTokenAsync(string token);
}