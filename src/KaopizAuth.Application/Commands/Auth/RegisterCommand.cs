using MediatR;
using KaopizAuth.Domain.Enums;

namespace KaopizAuth.Application.Commands.Auth;

/// <summary>
/// Command to register a new user
/// </summary>
public record RegisterCommand : IRequest<RegisterResult>
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Password confirmation
    /// </summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Type of user being registered
    /// </summary>
    public UserType UserType { get; init; } = UserType.EndUser;
}

/// <summary>
/// Result of user registration command
/// </summary>
public record RegisterResult
{
    /// <summary>
    /// Indicates if the registration was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// ID of the newly created user (if successful)
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Success or error message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Validation errors organized by field name
    /// </summary>
    public Dictionary<string, string[]> Errors { get; init; } = new();

    /// <summary>
    /// Creates a successful registration result
    /// </summary>
    public static RegisterResult CreateSuccess(string userId, string message = "User registered successfully")
    {
        return new RegisterResult
        {
            Success = true,
            UserId = userId,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed registration result
    /// </summary>
    public static RegisterResult CreateFailure(string message, Dictionary<string, string[]>? errors = null)
    {
        return new RegisterResult
        {
            Success = false,
            Message = message,
            Errors = errors ?? new Dictionary<string, string[]>()
        };
    }
}
