using BCrypt.Net;
using KaopizAuth.Application.Common.Interfaces;

namespace KaopizAuth.Infrastructure.Services.Authentication;

/// <summary>
/// Password service implementation using BCrypt for hashing
/// </summary>
public class PasswordService : IPasswordService
{
    private const int WorkFactor = 12; // BCrypt work factor for salt rounds

    /// <summary>
    /// Hashes a password using BCrypt with salt
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>The hashed password with salt</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="hashedPassword">The hashed password from storage</param>
    /// <param name="providedPassword">The plaintext password to verify</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentNullException(nameof(hashedPassword), "Hashed password cannot be null or empty");

        if (string.IsNullOrWhiteSpace(providedPassword))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        catch (Exception)
        {
            // Invalid hash format or other BCrypt errors
            return false;
        }
    }
}
