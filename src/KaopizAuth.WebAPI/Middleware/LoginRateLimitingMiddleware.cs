using System.Collections.Concurrent;
using System.Net;
using KaopizAuth.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to login endpoint
        if (context.Request.Path.StartsWithSegments("/api/auth/login") && 
            context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            var clientIp = GetClientIpAddress(context);
            
            if (IsRateLimited(clientIp))
            {
                _logger.LogWarning("Rate limit exceeded for IP: {ClientIP}", clientIp);
                
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = ApiResponse<object>.FailureResult(
                    "Too many login attempts. Please try again later.",
                    new Dictionary<string, string[]> { { "rateLimitExceeded", new[] { "Rate limit exceeded. Maximum 5 attempts per 15 minutes." } } }
                );
                
                var jsonResponse = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(jsonResponse);
                return;
            }
            
            // Record the attempt
            RecordLoginAttempt(clientIp);
        }

        await _next(context);
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
}