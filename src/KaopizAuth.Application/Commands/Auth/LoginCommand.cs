using KaopizAuth.Application.Common.Models;
using MediatR;

namespace KaopizAuth.Application.Commands.Auth;

/// <summary>
/// Login command using MediatR pattern
/// </summary>
public record LoginCommand : IRequest<ApiResponse<LoginResponse>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public string? IpAddress { get; init; }
}