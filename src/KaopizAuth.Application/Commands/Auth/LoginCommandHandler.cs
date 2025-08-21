using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Common.Models;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KaopizAuth.Application.Commands.Auth;

/// <summary>
/// Handler for LoginCommand using MediatR pattern
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt failed: User not found for email {Email} from IP {IpAddress}", 
                    request.Email, request.IpAddress ?? "Unknown");
                return ApiResponse<LoginResponse>.FailureResult("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt failed: User account is inactive for email {Email} from IP {IpAddress}", 
                    request.Email, request.IpAddress ?? "Unknown");
                return ApiResponse<LoginResponse>.FailureResult("Account is inactive");
            }

            // Attempt to sign in
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Login attempt failed: Account locked out for email {Email} from IP {IpAddress}", 
                        request.Email, request.IpAddress ?? "Unknown");
                    return ApiResponse<LoginResponse>.FailureResult("Account is locked out due to too many failed attempts");
                }

                _logger.LogWarning("Login attempt failed: Invalid password for email {Email} from IP {IpAddress}", 
                    request.Email, request.IpAddress ?? "Unknown");
                return ApiResponse<LoginResponse>.FailureResult("Invalid email or password");
            }

            // Generate tokens
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);
            var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

            // Create refresh token entity
            var refreshToken = RefreshToken.Create(
                token: refreshTokenValue,
                expiresAt: DateTime.UtcNow.AddDays(7), // 7 days as per requirements
                userId: user.Id.ToString(),
                createdByIp: request.IpAddress,
                createdBy: user.Id.ToString()
            );

            // Save refresh token
            await _refreshTokenRepository.AddAsync(refreshToken);
            
            // Update user last login
            user.UpdateLastLogin();
            await _userManager.UpdateAsync(user);

            // Get user roles for response
            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes as per requirements
                User = new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    FullName = user.FullName,
                    Roles = roles
                }
            };

            _logger.LogInformation("Login successful for user {Email} from IP {IpAddress}", 
                request.Email, request.IpAddress ?? "Unknown");
            return ApiResponse<LoginResponse>.SuccessResult(response, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for email {Email} from IP {IpAddress}", 
                request.Email, request.IpAddress ?? "Unknown");
            return ApiResponse<LoginResponse>.FailureResult("An error occurred during login");
        }
    }
}