using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Common.Models;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Interfaces;
using KaopizAuth.Domain.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KaopizAuth.Application.Commands.Auth.Handlers;

/// <summary>
/// Handler for refresh token command
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<RefreshTokenResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenDomainService _refreshTokenDomainService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenDomainService refreshTokenDomainService,
        IJwtTokenService jwtTokenService,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenDomainService = refreshTokenDomainService;
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the refresh token from database
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
            
            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken[..8] + "...");
                return ApiResponse<RefreshTokenResponse>.FailureResult("Invalid refresh token");
            }

            // Validate the refresh token
            if (!_refreshTokenDomainService.CanUseRefreshToken(refreshToken))
            {
                _logger.LogWarning("Invalid refresh token used for user {UserId}", refreshToken.UserId);
                return ApiResponse<RefreshTokenResponse>.FailureResult("Invalid or expired refresh token");
            }

            // Get the user
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                _logger.LogWarning("User not found for refresh token: {UserId}", refreshToken.UserId);
                return ApiResponse<RefreshTokenResponse>.FailureResult("User not found");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted to refresh token: {UserId}", refreshToken.UserId);
                return ApiResponse<RefreshTokenResponse>.FailureResult("User account is inactive");
            }

            // Revoke the old refresh token
            _refreshTokenDomainService.RevokeRefreshToken(
                refreshToken, 
                request.IpAddress ?? "Unknown", 
                "Token used for refresh");

            // Generate new tokens
            var newAccessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = _refreshTokenDomainService.GenerateRefreshToken(
                user.Id, 
                request.IpAddress ?? "Unknown");

            // Save the new refresh token
            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update user's last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = newRefreshToken.ExpiresAt
            };

            return ApiResponse<RefreshTokenResponse>.SuccessResult(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return ApiResponse<RefreshTokenResponse>.FailureResult("An error occurred while refreshing the token");
        }
    }
}
