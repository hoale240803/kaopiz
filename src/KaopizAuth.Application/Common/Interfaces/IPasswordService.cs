namespace KaopizAuth.Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification using BCrypt
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using BCrypt with salt
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="hashedPassword">The hashed password from storage</param>
    /// <param name="providedPassword">The plaintext password to verify</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
