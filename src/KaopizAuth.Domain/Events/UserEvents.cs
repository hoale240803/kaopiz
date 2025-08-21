using KaopizAuth.Domain.Enums;

namespace KaopizAuth.Domain.Events;

/// <summary>
/// Event raised when a new user is registered
/// </summary>
public sealed class UserRegisteredEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the user's email
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the user's full name
    /// </summary>
    public string FullName { get; }

    /// <summary>
    /// Gets the user type
    /// </summary>
    public UserType UserType { get; }

    public UserRegisteredEvent(Guid userId, string email, string fullName, UserType userType)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        UserType = userType;
    }
}

/// <summary>
/// Event raised when a user logs in successfully
/// </summary>
public sealed class UserLoggedInEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the login IP address
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    /// Gets the user agent
    /// </summary>
    public string? UserAgent { get; }

    public UserLoggedInEvent(Guid userId, string ipAddress, string? userAgent = null)
    {
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}

/// <summary>
/// Event raised when a user's account is deactivated
/// </summary>
public sealed class UserDeactivatedEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the reason for deactivation
    /// </summary>
    public string? Reason { get; }

    public UserDeactivatedEvent(Guid userId, string? reason = null)
    {
        UserId = userId;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a user's account is activated
/// </summary>
public sealed class UserActivatedEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    public UserActivatedEvent(Guid userId)
    {
        UserId = userId;
    }
}
