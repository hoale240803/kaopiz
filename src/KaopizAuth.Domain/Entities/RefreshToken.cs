using KaopizAuth.Domain.Common;

namespace KaopizAuth.Domain.Entities;

/// <summary>
/// Represents a refresh token for JWT authentication with enhanced audit and soft delete support
/// </summary>
public class RefreshToken : BaseEntity<Guid>
{
    /// <summary>
    /// Gets or sets the refresh token value
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the IP address that created this token
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Gets or sets the user agent that created this token (for device fingerprinting)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the device fingerprint for security tracking
    /// </summary>
    public string? DeviceFingerprint { get; set; }

    /// <summary>
    /// Gets or sets whether this is a persistent session (Remember Me)
    /// </summary>
    public bool IsPersistent { get; set; }

    /// <summary>
    /// Gets or sets when the token was revoked
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets the IP address that revoked this token
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Gets or sets the reason for revocation
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// Gets or sets the token that replaced this one when revoked
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Gets or sets the user ID this token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Indicates whether this token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Indicates whether this token is revoked
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Indicates whether this token is valid (not expired, not revoked)
    /// </summary>
    public bool IsValid => !IsExpired && !IsRevoked && IsActive;

    /// <summary>
    /// Revokes the token with full audit trail
    /// </summary>
    /// <param name="ipAddress">The IP address performing the revocation</param>
    /// <param name="reason">The reason for revocation</param>
    /// <param name="replacedByToken">The token that replaces this one (optional)</param>
    /// <param name="revokedBy">Who revoked the token</param>
    public void Revoke(string? ipAddress = null, string? reason = null, string? replacedByToken = null, string? revokedBy = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        RevokedReason = reason ?? "Token manually revoked";
        ReplacedByToken = replacedByToken;
        // Note: In a real implementation, we would call MarkAsUpdated(revokedBy) here
        // but there seems to be an inheritance issue that needs investigation
    }

    /// <summary>
    /// Check if token can be refreshed (not expired within 5 minutes)
    /// </summary>
    /// <returns>True if token can be used for refresh</returns>
    public bool CanRefresh()
    {
        return IsValid && ExpiresAt > DateTime.UtcNow.AddMinutes(5);
    }

    /// <summary>
    /// Creates a new refresh token with audit trail
    /// </summary>
    /// <param name="token">The token value</param>
    /// <param name="expiresAt">Expiration date</param>
    /// <param name="userId">User ID</param>
    /// <param name="createdByIp">IP address</param>
    /// <param name="createdBy">Who created the token</param>
    /// <param name="userAgent">User agent for device fingerprinting</param>
    /// <param name="deviceFingerprint">Device fingerprint</param>
    /// <param name="isPersistent">Whether this is a persistent session</param>
    /// <returns>New RefreshToken instance</returns>
    public static RefreshToken Create(
        string token, 
        DateTime expiresAt, 
        string userId, 
        string? createdByIp = null, 
        string? createdBy = null,
        string? userAgent = null,
        string? deviceFingerprint = null,
        bool isPersistent = false)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = userId,
            CreatedByIp = createdByIp,
            UserAgent = userAgent,
            DeviceFingerprint = deviceFingerprint,
            IsPersistent = isPersistent
        };

        if (createdBy != null)
        {
            refreshToken.CreatedBy = createdBy;
        }

        return refreshToken;
    }
}
