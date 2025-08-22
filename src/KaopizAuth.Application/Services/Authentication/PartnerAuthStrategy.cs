using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using System.Security.Claims;

namespace KaopizAuth.Application.Services.Authentication;

/// <summary>
/// Authentication strategy for Partner users with specialized permissions
/// </summary>
public class PartnerAuthStrategy : IUserAuthenticationStrategy
{
    private readonly IPasswordService _passwordService;

    public PartnerAuthStrategy(IPasswordService passwordService)
    {
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    }

    /// <summary>
    /// Authenticates partner users with business partnership validation
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

        // Partner users have standard authentication but with business-specific validations
        return AuthenticationStrategyResult.Success();
    }

    /// <summary>
    /// Gets partner-specific claims and business permissions
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
            new(ClaimTypes.Role, "Partner"),
            new("UserType", user.UserType.ToString()),
            new("FullName", user.FullName),
            new("SecurityLevel", "Business"),
            
            // Partner-specific claims
            new("PartnerType", "BusinessPartner"), // Could be extended to different partner types
            new("AccessLevel", "Partner"),
            
            // Partner permissions
            new("Permission", "CanViewProfile"),
            new("Permission", "CanUpdateProfile"),
            new("Permission", "CanChangePassword"),
            new("Permission", "CanAccessPartnerPortal"),
            new("Permission", "CanViewPartnerReports"),
            new("Permission", "CanManagePartnerData"),
            new("Permission", "CanAccessApiEndpoints"),
            new("Permission", "CanViewBusinessAnalytics"),
            new("Permission", "CanManagePartnerUsers"),
            new("Permission", "CanAccessIntegrationTools"),
            new("Permission", "CanViewTransactionHistory"),
            new("Permission", "CanExportData"),
            new("Permission", "CanConfigureWebhooks"),
            new("Permission", "CanAccessSupportTickets")
        };

        return await Task.FromResult(claims);
    }

    /// <summary>
    /// Validates partner specific business rules and partnership status
    /// </summary>
    public async Task<ValidationResult> ValidateUserTypeRulesAsync(ApplicationUser user)
    {
        var errors = new List<string>();

        // Check if user account is active
        if (!user.IsActive)
        {
            errors.Add("Partner account is deactivated");
        }

        // Check if email is confirmed (required for business partnerships)
        if (!user.EmailConfirmed)
        {
            errors.Add("Partner email address must be confirmed for business operations");
        }

        // Business partnership specific validations
        // TODO: In production, add checks for:
        // - Partnership agreement status
        // - Business license validity
        // - Credit standing
        // - Compliance status
        // - Integration limits
        // - API usage quotas

        // Simulate partnership agreement validation
        var partnershipValid = await ValidatePartnershipAgreementAsync(user);
        if (!partnershipValid)
        {
            errors.Add("Partnership agreement is not valid or has expired");
        }

        // Check for business hours access (partners might have time restrictions)
        var businessHoursValid = ValidateBusinessHoursAccess();
        if (!businessHoursValid)
        {
            errors.Add("Partner access is restricted to business hours (9 AM - 6 PM UTC)");
        }

        return await Task.FromResult(errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors));
    }

    /// <summary>
    /// Validates partnership agreement status (placeholder implementation)
    /// </summary>
    private async Task<bool> ValidatePartnershipAgreementAsync(ApplicationUser user)
    {
        // Placeholder: In production, this would check:
        // - Partnership database records
        // - Contract expiration dates
        // - Legal compliance status
        // - Payment status
        
        await Task.Delay(1); // Simulate async operation
        return true; // Assume partnership is valid for now
    }

    /// <summary>
    /// Validates business hours access for partners
    /// </summary>
    private bool ValidateBusinessHoursAccess()
    {
        var currentHour = DateTime.UtcNow.Hour;
        // Business hours: 9 AM to 6 PM UTC
        return currentHour >= 9 && currentHour < 18;
    }
}