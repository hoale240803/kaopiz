using KaopizAuth.Application.Common.Models;
using MediatR;

namespace KaopizAuth.Application.Commands.Auth;

/// <summary>
/// Logout command for revoking user tokens
/// </summary>
public record LogoutCommand : IRequest<ApiResponse<bool>>
{
    /// <summary>
    /// The refresh token to revoke
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// JWT access token to blacklist (alternative to refresh token)
    /// </summary>
    public string? JwtToken { get; init; }

    /// <summary>
    /// IP address of the client requesting the logout
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Whether to revoke all user tokens or just the specific one
    /// </summary>
    public bool RevokeAllTokens { get; init; } = false;
}
