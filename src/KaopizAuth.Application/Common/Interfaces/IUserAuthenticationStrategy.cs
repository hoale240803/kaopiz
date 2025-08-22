using KaopizAuth.Domain.Entities;
using System.Security.Claims;

namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Interface for user type specific authentication strategies
/// </summary>
public interface IUserAuthenticationStrategy
{
    /// <summary>
    /// Validates user credentials and applies user type specific rules
    /// </summary>
    /// <param name="user">The user to authenticate</param>
    /// <param name="password">The provided password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with any additional validation requirements</returns>
    Task<AuthenticationStrategyResult> AuthenticateAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user type specific claims for the authenticated user
    /// </summary>
    /// <param name="user">The authenticated user</param>
    /// <returns>Collection of claims specific to the user type</returns>
    Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user);

    /// <summary>
    /// Validates user type specific business rules during authentication
    /// </summary>
    /// <param name="user">The user to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateUserTypeRulesAsync(ApplicationUser user);
}

/// <summary>
/// Result of authentication strategy processing
/// </summary>
public class AuthenticationStrategyResult
{
    public bool IsSuccess { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<string> ValidationErrors { get; set; } = Enumerable.Empty<string>();

    public static AuthenticationStrategyResult Success() => new() { IsSuccess = true };
    public static AuthenticationStrategyResult SuccessWithTwoFactor() => new() { IsSuccess = true, RequiresTwoFactor = true };
    public static AuthenticationStrategyResult Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    public static AuthenticationStrategyResult ValidationFailure(IEnumerable<string> errors) => new() { IsSuccess = false, ValidationErrors = errors };
}

/// <summary>
/// Validation result for user type specific rules
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors };
    public static ValidationResult Failure(IEnumerable<string> errors) => new() { IsValid = false, Errors = errors };
}