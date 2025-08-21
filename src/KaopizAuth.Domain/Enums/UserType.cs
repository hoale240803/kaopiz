namespace KaopizAuth.Domain.Enums;

/// <summary>
/// Represents the different types of users in the system
/// </summary>
public enum UserType
{
    /// <summary>
    /// Regular end user with basic permissions
    /// </summary>
    EndUser = 1,

    /// <summary>
    /// Administrator with elevated permissions
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Partner user with specialized permissions
    /// </summary>
    Partner = 3
}
