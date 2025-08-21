namespace KaopizAuth.Domain.Constants;

/// <summary>
/// Domain constants for authentication system
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// Password policy constants
    /// </summary>
    public static class PasswordPolicy
    {
        /// <summary>
        /// Minimum password length
        /// </summary>
        public const int MinLength = 8;

        /// <summary>
        /// Maximum password length
        /// </summary>
        public const int MaxLength = 128;

        /// <summary>
        /// Requires at least one uppercase letter
        /// </summary>
        public const bool RequireUppercase = true;

        /// <summary>
        /// Requires at least one lowercase letter
        /// </summary>
        public const bool RequireLowercase = true;

        /// <summary>
        /// Requires at least one digit
        /// </summary>
        public const bool RequireDigit = true;

        /// <summary>
        /// Requires at least one special character
        /// </summary>
        public const bool RequireSpecialChar = true;

        /// <summary>
        /// Special characters allowed in passwords
        /// </summary>
        public const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    }

    /// <summary>
    /// Token-related constants
    /// </summary>
    public static class Tokens
    {
        /// <summary>
        /// JWT access token lifetime in minutes
        /// </summary>
        public const int AccessTokenLifetimeMinutes = 15;

        /// <summary>
        /// Refresh token lifetime in days
        /// </summary>
        public const int RefreshTokenLifetimeDays = 7;

        /// <summary>
        /// Maximum number of active refresh tokens per user
        /// </summary>
        public const int MaxActiveRefreshTokensPerUser = 5;

        /// <summary>
        /// Password reset token lifetime in hours
        /// </summary>
        public const int PasswordResetTokenLifetimeHours = 24;

        /// <summary>
        /// Email verification token lifetime in hours
        /// </summary>
        public const int EmailVerificationTokenLifetimeHours = 72;
    }

    /// <summary>
    /// User-related constants
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// Maximum length for first name
        /// </summary>
        public const int FirstNameMaxLength = 50;

        /// <summary>
        /// Maximum length for last name
        /// </summary>
        public const int LastNameMaxLength = 50;

        /// <summary>
        /// Maximum length for username
        /// </summary>
        public const int UsernameMaxLength = 50;

        /// <summary>
        /// Minimum length for username
        /// </summary>
        public const int UsernameMinLength = 3;

        /// <summary>
        /// Maximum login attempts before account lockout
        /// </summary>
        public const int MaxLoginAttempts = 5;

        /// <summary>
        /// Account lockout duration in minutes
        /// </summary>
        public const int AccountLockoutDurationMinutes = 30;
    }

    /// <summary>
    /// Security constants
    /// </summary>
    public static class Security
    {
        /// <summary>
        /// JWT signing algorithm
        /// </summary>
        public const string JwtSigningAlgorithm = "RS256";

        /// <summary>
        /// JWT issuer claim name
        /// </summary>
        public const string JwtIssuer = "KaopizAuth";

        /// <summary>
        /// JWT audience claim name
        /// </summary>
        public const string JwtAudience = "KaopizAuthUsers";

        /// <summary>
        /// Default session timeout in minutes
        /// </summary>
        public const int SessionTimeoutMinutes = 30;

        /// <summary>
        /// Rate limiting - max requests per minute per IP
        /// </summary>
        public const int MaxRequestsPerMinute = 60;

        /// <summary>
        /// Rate limiting - max login attempts per minute per IP
        /// </summary>
        public const int MaxLoginAttemptsPerMinute = 5;
    }

    /// <summary>
    /// Email-related constants
    /// </summary>
    public static class Email
    {
        /// <summary>
        /// Maximum email address length
        /// </summary>
        public const int MaxLength = 254;

        /// <summary>
        /// Email validation regex pattern
        /// </summary>
        public const string ValidationPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    }

    /// <summary>
    /// Error messages
    /// </summary>
    public static class ErrorMessages
    {
        public const string InvalidCredentials = "Invalid email or password.";
        public const string AccountLocked = "Account is temporarily locked due to multiple failed login attempts.";
        public const string AccountDeactivated = "Account has been deactivated.";
        public const string EmailNotConfirmed = "Email address has not been confirmed.";
        public const string InvalidRefreshToken = "Invalid or expired refresh token.";
        public const string EmailAlreadyExists = "An account with this email address already exists.";
        public const string UsernameAlreadyExists = "This username is already taken.";
        public const string WeakPassword = "Password does not meet security requirements.";
        public const string InvalidEmail = "Invalid email address format.";
        public const string TokenExpired = "Token has expired.";
        public const string UnauthorizedAccess = "You are not authorized to perform this action.";
    }
}
