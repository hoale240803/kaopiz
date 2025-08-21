using System.Text.RegularExpressions;

namespace KaopizAuth.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the email address value
    /// </summary>
    public string Value { get; private set; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Email instance
    /// </summary>
    /// <param name="email">Email address string</param>
    /// <returns>Email instance</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid</exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > 256)
            throw new ArgumentException("Email cannot be longer than 256 characters.", nameof(email));

        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new ArgumentException("Invalid email format.", nameof(email));

        return new Email(normalizedEmail);
    }

    /// <summary>
    /// Tries to create an Email instance
    /// </summary>
    /// <param name="email">Email address string</param>
    /// <param name="result">Created Email instance or null</param>
    /// <returns>True if creation was successful</returns>
    public static bool TryCreate(string email, out Email? result)
    {
        try
        {
            result = Create(email);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Email email) => email.Value;

    public static bool operator ==(Email? left, Email? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Email? left, Email? right)
    {
        return !Equals(left, right);
    }
}
