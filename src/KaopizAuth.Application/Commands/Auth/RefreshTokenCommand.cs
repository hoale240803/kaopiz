using KaopizAuth.Application.Common.Models;
using MediatR;

namespace KaopizAuth.Application.Commands.Auth;

/// <summary>
/// Refresh token command for generating new access tokens
/// </summary>
public record RefreshTokenCommand : IRequest<ApiResponse<RefreshTokenResponse>>
{
    /// <summary>
    /// The refresh token to use for generating new access token
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// IP address of the client requesting the refresh
    /// </summary>
    public string? IpAddress { get; init; }
}
