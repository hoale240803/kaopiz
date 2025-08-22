using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using System.Security.Claims;

namespace KaopizAuth.Application.Services.Authentication;

/// <summary>
/// Authentication strategy for EndUser type
/// </summary>
public class EndUserAuthStrategy : IUserAuthenticationStrategy
{
    private readonly IPasswordService _passwordService;

    public EndUserAuthStrategy(IPasswordService passwordService)
    {
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    }

    /// <summary>
    /// Authenticates end users with basic validation rules
    /// </summary>
    public async Task<AuthenticationStrategyResult> AuthenticateAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
    {
        // Validate user type specific rules first
        var validationResult = await ValidateUserTypeRulesAsync(user);
        if (!validationResult.IsValid)
        {
            return AuthenticationStrategyResult.ValidationFailure(validationResult.Errors);
        }

        // Verify password
        if (!_passwordService.VerifyPassword(user.PasswordHash!, password))
        {
            return AuthenticationStrategyResult.Failure("Invalid credentials");
        }

        // EndUsers don't require additional security measures
        return AuthenticationStrategyResult.Success();
    }

    /// <summary>
    /// Gets basic claims for end users
    /// </summary>
    public async Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new("UserType", user.UserType.ToString()),
            new("FullName", user.FullName),
            // Basic permissions for end users
            new("Permission", "CanViewProfile"),
            new("Permission", "CanUpdateProfile"),
            new("Permission", "CanChangePassword")
        };

        return await Task.FromResult(claims);
    }

    /// <summary>
    /// Validates end user specific business rules
    /// </summary>
    public async Task<ValidationResult> ValidateUserTypeRulesAsync(ApplicationUser user)
    {
        var errors = new List<string>();

        // Check if user account is active
        if (!user.IsActive)
        {
            errors.Add("User account is deactivated");
        }

        // Check if email is confirmed (basic requirement for end users)
        if (!user.EmailConfirmed)
        {
            errors.Add("Email address must be confirmed before login");
        }

        // Additional end user specific validations can be added here
        // For example: check subscription status, terms acceptance, etc.

        return await Task.FromResult(errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors));
    }
}