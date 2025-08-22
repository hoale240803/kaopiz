using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using System.Security.Claims;

namespace KaopizAuth.Application.Services.Authentication;

/// <summary>
/// Authentication strategy for Admin users with enhanced security requirements
/// </summary>
public class AdminAuthStrategy : IUserAuthenticationStrategy
{
    private readonly IPasswordService _passwordService;

    public AdminAuthStrategy(IPasswordService passwordService)
    {
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    }

    /// <summary>
    /// Authenticates admin users with enhanced security validation
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

        // Admin users require two-factor authentication (placeholder implementation)
        // In production, this would check if 2FA is enabled and properly configured
        return AuthenticationStrategyResult.SuccessWithTwoFactor();
    }

    /// <summary>
    /// Gets administrative claims and permissions
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
            new(ClaimTypes.Role, "Administrator"),
            new("UserType", user.UserType.ToString()),
            new("FullName", user.FullName),
            new("SecurityLevel", "High"),
            
            // Administrative permissions
            new("Permission", "CanViewProfile"),
            new("Permission", "CanUpdateProfile"),
            new("Permission", "CanChangePassword"),
            new("Permission", "CanManageUsers"),
            new("Permission", "CanManageRoles"),
            new("Permission", "CanViewAuditLogs"),
            new("Permission", "CanManageSystem"),
            new("Permission", "CanAccessAdminPanel"),
            new("Permission", "CanDeleteUsers"),
            new("Permission", "CanModifySystemSettings"),
            new("Permission", "CanViewSecurityReports"),
            new("Permission", "CanManagePartners")
        };

        return await Task.FromResult(claims);
    }

    /// <summary>
    /// Validates admin specific business rules with enhanced security checks
    /// </summary>
    public async Task<ValidationResult> ValidateUserTypeRulesAsync(ApplicationUser user)
    {
        var errors = new List<string>();

        // Check if user account is active
        if (!user.IsActive)
        {
            errors.Add("Administrator account is deactivated");
        }

        // Check if email is confirmed (critical for admin accounts)
        if (!user.EmailConfirmed)
        {
            errors.Add("Administrator email address must be confirmed");
        }

        // Check last login time - admin accounts should not be idle too long
        if (user.LastLoginAt.HasValue)
        {
            var daysSinceLastLogin = (DateTime.UtcNow - user.LastLoginAt.Value).TotalDays;
            if (daysSinceLastLogin > 90) // 90 days idle limit for admin accounts
            {
                errors.Add("Administrator account has been idle for too long. Please contact system administrator.");
            }
        }

        // Additional security validations for admin accounts
        // TODO: In production, add checks for:
        // - Two-factor authentication setup
        // - Strong password policy compliance
        // - Account lockout status
        // - IP address restrictions
        // - Time-based access restrictions

        return await Task.FromResult(errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors));
    }
}