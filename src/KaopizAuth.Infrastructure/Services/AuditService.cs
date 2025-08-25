using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace KaopizAuth.Infrastructure.Services;

/// <summary>
/// Audit logging service implementation
/// </summary>
public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IApplicationDbContext dbContext, ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogSuccessfulLoginAsync(string userId, string email, string? ipAddress, string? userAgent = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                Email = email,
                LoginTime = DateTime.UtcNow,
                Success = true
            });

            var auditEntry = AuditEntry.Create(
                entityType: "Authentication",
                entityId: userId,
                changeType: "Login",
                tableName: "Users",
                ipAddress: ipAddress
            );

            auditEntry.Metadata = metadata;
            auditEntry.UserAgent = userAgent;

            _dbContext.AuditEntries.Add(auditEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Audit: Successful login for user {Email} from IP {IpAddress}", email, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log successful login audit for user {Email}", email);
        }
    }

    public async Task LogFailedLoginAsync(string email, string? ipAddress, string reason, string? userAgent = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                Email = email,
                LoginTime = DateTime.UtcNow,
                Success = false,
                Reason = reason
            });

            var auditEntry = AuditEntry.Create(
                entityType: "Authentication",
                entityId: "Unknown", // No user ID for failed login
                changeType: "FailedLogin",
                tableName: "Users",
                ipAddress: ipAddress
            );

            auditEntry.Metadata = metadata;
            auditEntry.UserAgent = userAgent;

            _dbContext.AuditEntries.Add(auditEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("Audit: Failed login attempt for email {Email} from IP {IpAddress}, reason: {Reason}", email, ipAddress, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log failed login audit for email {Email}", email);
        }
    }

    public async Task LogPasswordChangeAsync(string userId, string email, string? ipAddress, string? userAgent = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                Email = email,
                ChangeTime = DateTime.UtcNow,
                Action = "PasswordChange"
            });

            var auditEntry = AuditEntry.Create(
                entityType: "User",
                entityId: userId,
                changeType: "PasswordChange",
                tableName: "Users",
                ipAddress: ipAddress
            );

            auditEntry.Metadata = metadata;
            auditEntry.UserAgent = userAgent;

            _dbContext.AuditEntries.Add(auditEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Audit: Password changed for user {Email} from IP {IpAddress}", email, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log password change audit for user {Email}", email);
        }
    }

    public async Task LogBruteForceAttemptAsync(string? ipAddress, int attemptCount, string? email = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                AttemptCount = attemptCount,
                DetectionTime = DateTime.UtcNow,
                TargetEmail = email,
                ThreatLevel = attemptCount > 10 ? "High" : "Medium"
            });

            var auditEntry = AuditEntry.Create(
                entityType: "Security",
                entityId: ipAddress ?? "Unknown",
                changeType: "BruteForce",
                tableName: "SecurityEvents",
                ipAddress: ipAddress
            );

            auditEntry.Metadata = metadata;

            _dbContext.AuditEntries.Add(auditEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("Audit: Brute force attempt detected from IP {IpAddress}, {AttemptCount} failed attempts", ipAddress, attemptCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log brute force audit for IP {IpAddress}", ipAddress);
        }
    }

    public async Task LogSensitiveDataAccessAsync(string userId, string dataType, string? ipAddress, string? userAgent = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                DataType = dataType,
                AccessTime = DateTime.UtcNow,
                UserId = userId
            });

            var auditEntry = AuditEntry.Create(
                entityType: "DataAccess",
                entityId: userId,
                changeType: "SensitiveAccess",
                tableName: "Users",
                ipAddress: ipAddress
            );

            auditEntry.Metadata = metadata;
            auditEntry.UserAgent = userAgent;

            _dbContext.AuditEntries.Add(auditEntry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Audit: Sensitive data access for user {UserId}, data type: {DataType} from IP {IpAddress}", userId, dataType, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log sensitive data access audit for user {UserId}", userId);
        }
    }
}
