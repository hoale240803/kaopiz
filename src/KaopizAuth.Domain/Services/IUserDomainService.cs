using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.ValueObjects;

namespace KaopizAuth.Domain.Services;

/// <summary>
/// Domain service interface for user-related business operations
/// </summary>
public interface IUserDomainService
{
    /// <summary>
    /// Validates if the provided password meets security requirements
    /// </summary>
    /// <param name="password">The password to validate</param>
    /// <returns>Validation result with any error messages</returns>
    Task<ValidationResult> ValidatePasswordAsync(string password);

    /// <summary>
    /// Validates if the email is unique in the system
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from uniqueness check</param>
    /// <returns>True if email is unique, false otherwise</returns>
    Task<bool> IsEmailUniqueAsync(Email email, Guid? excludeUserId = null);

    /// <summary>
    /// Validates if the username is unique in the system
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <param name="excludeUserId">Optional user ID to exclude from uniqueness check</param>
    /// <returns>True if username is unique, false otherwise</returns>
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null);

    /// <summary>
    /// Generates a secure password reset token for the user
    /// </summary>
    /// <param name="user">The user requesting password reset</param>
    /// <returns>Password reset token</returns>
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);

    /// <summary>
    /// Validates a password reset token
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="token">The reset token</param>
    /// <returns>True if token is valid, false otherwise</returns>
    Task<bool> ValidatePasswordResetTokenAsync(ApplicationUser user, string token);

    /// <summary>
    /// Checks if a user can be deactivated
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <returns>True if user can be deactivated, false otherwise</returns>
    bool CanDeactivateUser(ApplicationUser user);

    /// <summary>
    /// Checks if a user can be activated
    /// </summary>
    /// <param name="user">The user to check</param>
    /// <returns>True if user can be activated, false otherwise</returns>
    bool CanActivateUser(ApplicationUser user);
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the validation error messages
    /// </summary>
    public IReadOnlyList<string> Errors { get; private set; } = new List<string>();

    private ValidationResult(bool isValid, IEnumerable<string>? errors = null)
    {
        IsValid = isValid;
        if (errors != null)
        {
            Errors = errors.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new(true);

    /// <summary>
    /// Creates a failed validation result with error messages
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public static ValidationResult Failure(params string[] errors) => new(false, errors);

    /// <summary>
    /// Creates a failed validation result with error messages
    /// </summary>
    /// <param name="errors">The validation errors</param>
    public static ValidationResult Failure(IEnumerable<string> errors) => new(false, errors);
}
