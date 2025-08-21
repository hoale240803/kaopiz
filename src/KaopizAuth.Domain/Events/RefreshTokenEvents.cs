namespace KaopizAuth.Domain.Events;

/// <summary>
/// Event raised when a refresh token is generated
/// </summary>
public sealed class RefreshTokenCreatedEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the token ID
    /// </summary>
    public Guid TokenId { get; }

    /// <summary>
    /// Gets the IP address where token was created
    /// </summary>
    public string CreatedByIp { get; }

    /// <summary>
    /// Gets the token expiration date
    /// </summary>
    public DateTime ExpiresAt { get; }

    public RefreshTokenCreatedEvent(Guid userId, Guid tokenId, string createdByIp, DateTime expiresAt)
    {
        UserId = userId;
        TokenId = tokenId;
        CreatedByIp = createdByIp;
        ExpiresAt = expiresAt;
    }
}

/// <summary>
/// Event raised when a refresh token is revoked
/// </summary>
public sealed class RefreshTokenRevokedEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the token ID
    /// </summary>
    public Guid TokenId { get; }

    /// <summary>
    /// Gets the IP address where token was revoked
    /// </summary>
    public string? RevokedByIp { get; }

    /// <summary>
    /// Gets the reason for revocation
    /// </summary>
    public string? Reason { get; }

    /// <summary>
    /// Gets the replacement token ID if this token was replaced
    /// </summary>
    public Guid? ReplacedByTokenId { get; }

    public RefreshTokenRevokedEvent(Guid userId, Guid tokenId, string? revokedByIp = null, string? reason = null, Guid? replacedByTokenId = null)
    {
        UserId = userId;
        TokenId = tokenId;
        RevokedByIp = revokedByIp;
        Reason = reason;
        ReplacedByTokenId = replacedByTokenId;
    }
}

/// <summary>
/// Event raised when a refresh token is used to generate new tokens
/// </summary>
public sealed class RefreshTokenUsedEvent : DomainEvent
{
    /// <summary>
    /// Gets the user ID
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Gets the token ID that was used
    /// </summary>
    public Guid TokenId { get; }

    /// <summary>
    /// Gets the IP address where token was used
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    /// Gets the new token ID that was generated
    /// </summary>
    public Guid NewTokenId { get; }

    public RefreshTokenUsedEvent(Guid userId, Guid tokenId, string ipAddress, Guid newTokenId)
    {
        UserId = userId;
        TokenId = tokenId;
        IpAddress = ipAddress;
        NewTokenId = newTokenId;
    }
}
