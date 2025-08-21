using KaopizAuth.Domain.Common;

namespace KaopizAuth.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking entity changes
/// </summary>
public class AuditEntry : BaseEntity
{
    /// <summary>
    /// Gets or sets the entity type that was changed
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity ID that was changed
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of change (Insert, Update, Delete, SoftDelete, Restore)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the changes in JSON format
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Gets or sets the old values in JSON format (for updates)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the new values in JSON format (for inserts/updates)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the change
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the change
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional metadata in JSON format
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Creates a new audit entry
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="changeType">Type of change</param>
    /// <param name="tableName">Database table name</param>
    /// <param name="changedBy">Who made the change</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>New audit entry</returns>
    public static AuditEntry Create(
        string entityType, 
        string entityId, 
        string changeType, 
        string tableName,
        string? changedBy = null,
        string? ipAddress = null)
    {
        return new AuditEntry
        {
            EntityType = entityType,
            EntityId = entityId,
            ChangeType = changeType,
            TableName = tableName,
            CreatedBy = changedBy,
            IpAddress = ipAddress
        };
    }
}
