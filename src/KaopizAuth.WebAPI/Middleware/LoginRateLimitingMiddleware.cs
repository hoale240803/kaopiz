using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Common.Models;

namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Rate limiting middleware for login attempts
/// </summary>
public class LoginRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoginRateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, List<DateTime>> _loginAttempts = new();
    private const int MaxAttempts = 5;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromMinutes(15);

    public LoginRateLimitingMiddleware(RequestDelegate next, ILogger<LoginRateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Clears rate limiting state - for testing purposes only
    /// </summary>
    public static void ClearRateLimitState()
    {
        _loginAttempts.Clear();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to login endpoint
        if (context.Request.Path.StartsWithSegments("/api/auth/login") &&
            context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            var clientIp = GetClientIpAddress(context);
            _logger.LogInformation("Rate limiting check for IP: {ClientIP}", clientIp);

            // Check if already rate limited BEFORE proceeding
            if (IsRateLimited(clientIp))
            {
                var attemptCount = _loginAttempts.TryGetValue(clientIp, out var attempts) ? attempts.Count : 0;
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIP} with {Count} failed attempts",
                    clientIp, attemptCount);

                // Log brute force attempt via audit service
                try
                {
                    using var scope = context.RequestServices.CreateScope();
                    var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                    // Don't use Task.Run here as it can cause scope disposal issues
                    await auditService.LogBruteForceAttemptAsync(clientIp, attemptCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log brute force audit for IP {ClientIP}", clientIp);
                }

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.FailureResult(
                    "Too many login attempts. Please try again later.",
                    new Dictionary<string, string[]>
                    {
                        ["RateLimit"] = new[] { "Rate limit exceeded. Maximum 5 attempts per 15 minutes." }
                    }
                );

                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
                return;
            }

            // Proceed with the request
            await _next(context);

            // After request processing, check if login failed and record attempt only for failures
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                RecordLoginAttempt(clientIp);
                _logger.LogInformation("Recorded failed login attempt for IP: {ClientIP}, total failed attempts: {Count}",
                    clientIp, GetFailedAttemptCount(clientIp));
            }
        }
        else
        {
            await _next(context);
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP first (useful for reverse proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private bool IsRateLimited(string clientIp)
    {
        if (!_loginAttempts.TryGetValue(clientIp, out var attempts))
        {
            return false;
        }

        // Clean up old attempts
        var cutoffTime = DateTime.UtcNow - TimeWindow;
        attempts.RemoveAll(time => time < cutoffTime);

        return attempts.Count >= MaxAttempts;
    }

    private void RecordLoginAttempt(string clientIp)
    {
        _loginAttempts.AddOrUpdate(
            clientIp,
            new List<DateTime> { DateTime.UtcNow },
            (key, existingAttempts) =>
            {
                // Clean up old attempts
                var cutoffTime = DateTime.UtcNow - TimeWindow;
                existingAttempts.RemoveAll(time => time < cutoffTime);

                existingAttempts.Add(DateTime.UtcNow);
                return existingAttempts;
            }
        );
    }

    private int GetFailedAttemptCount(string clientIp)
    {
        if (!_loginAttempts.TryGetValue(clientIp, out var attempts))
        {
            return 0;
        }

        // Clean up old attempts and return current count
        var cutoffTime = DateTime.UtcNow - TimeWindow;
        attempts.RemoveAll(time => time < cutoffTime);
        return attempts.Count;
    }
}