using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;

namespace KaopizAuth.Application.Handlers.Auth;

/// <summary>
/// Handler for user registration command
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Handles the user registration command
    /// </summary>
    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting user registration for email: {Email}", request.Email);

            // Create new ApplicationUser
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email.ToLowerInvariant(),
                Email = request.Email.ToLowerInvariant(),
                NormalizedEmail = request.Email.ToUpperInvariant(),
                NormalizedUserName = request.Email.ToUpperInvariant(),
                EmailConfirmed = true, // Set to true for now, implement email confirmation later if needed
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                UserType = request.UserType,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            // Create user with password using Identity
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User registration successful for email: {Email}, UserId: {UserId}",
                    request.Email, user.Id);

                return RegisterResult.CreateSuccess(user.Id.ToString(), "User registered successfully");
            }

            // Log and return validation errors
            _logger.LogWarning("User registration failed for email: {Email}. Errors: {@Errors}",
                request.Email, result.Errors);

            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return RegisterResult.CreateFailure("Registration failed", errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during user registration for email: {Email}", request.Email);

            return RegisterResult.CreateFailure("An unexpected error occurred during registration");
        }
    }
}
