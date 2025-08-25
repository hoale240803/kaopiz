using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Common.Models;
using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KaopizAuth.Application.Commands.Auth.Handlers;

/// <summary>
/// Handler for logout command
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenDomainService _refreshTokenDomainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtBlacklistService _jwtBlacklistService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenDomainService refreshTokenDomainService,
        IUnitOfWork unitOfWork,
        IJwtBlacklistService jwtBlacklistService,
        ILogger<LogoutCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenDomainService = refreshTokenDomainService;
        _unitOfWork = unitOfWork;
        _jwtBlacklistService = jwtBlacklistService;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool refreshTokenHandled = false;
            bool jwtTokenHandled = false;

            // Handle refresh token logout
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                refreshTokenHandled = await HandleRefreshTokenLogout(request, cancellationToken);
            }

            // Handle JWT token blacklisting
            if (!string.IsNullOrEmpty(request.JwtToken))
            {
                await _jwtBlacklistService.BlacklistTokenAsync(request.JwtToken, cancellationToken);
                jwtTokenHandled = true;
                _logger.LogInformation("JWT token blacklisted during logout");
            }

            if (!refreshTokenHandled && !jwtTokenHandled)
            {
                return ApiResponse<bool>.FailureResult("No valid token provided for logout");
            }

            return ApiResponse<bool>.SuccessResult(true, "Logout successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return ApiResponse<bool>.FailureResult("An error occurred during logout");
        }
    }

    private async Task<bool> HandleRefreshTokenLogout(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Get the refresh token from database
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken!, cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Logout attempt with non-existent refresh token: {Token}", request.RefreshToken![..8] + "...");
            return false;
        }

        var userId = refreshToken.UserId;

        if (request.RevokeAllTokens)
        {
            // Revoke all tokens for the user
            var userTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);
            _refreshTokenDomainService.RevokeAllUserTokens(
                userTokens,
                request.IpAddress ?? "Unknown",
                "User logout - all tokens revoked");

            _logger.LogInformation("All tokens revoked for user {UserId} during logout", userId);
        }
        else
        {
            // Revoke only the specific token
            _refreshTokenDomainService.RevokeRefreshToken(
                refreshToken,
                request.IpAddress ?? "Unknown",
                "User logout");

            _logger.LogInformation("Token revoked for user {UserId} during logout", userId);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
