using System.Diagnostics;

namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Middleware to monitor and log API performance metrics
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private const string ResponseTimeHeaderName = "X-Response-Time-ms";

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var responseTimeMs = stopwatch.ElapsedMilliseconds;

            // Add response time to headers
            context.Response.Headers.Append(ResponseTimeHeaderName, responseTimeMs.ToString());

            // Log performance metrics
            LogPerformanceMetrics(context, responseTimeMs, correlationId);
        }
    }

    private void LogPerformanceMetrics(HttpContext context, long responseTimeMs, string correlationId)
    {
        var request = context.Request;
        var response = context.Response;

        var logLevel = GetLogLevel(responseTimeMs, response.StatusCode);

        _logger.Log(logLevel,
            "Performance: {Method} {Path} completed in {ResponseTime}ms - Status: {StatusCode} - CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            responseTimeMs,
            response.StatusCode,
            correlationId);

        // Log warning for slow requests
        if (responseTimeMs > 1000) // More than 1 second
        {
            _logger.LogWarning(
                "Slow request detected: {Method} {Path} took {ResponseTime}ms - CorrelationId: {CorrelationId}",
                request.Method,
                request.Path,
                responseTimeMs,
                correlationId);
        }

        // Additional structured logging for metrics collection
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestMethod"] = request.Method,
            ["RequestPath"] = request.Path.Value ?? string.Empty,
            ["ResponseStatusCode"] = response.StatusCode,
            ["ResponseTimeMs"] = responseTimeMs,
            ["UserAgent"] = request.Headers.UserAgent.ToString(),
            ["RemoteIpAddress"] = GetClientIpAddress(context)
        });

        _logger.LogInformation("Request metrics logged");
    }

    private LogLevel GetLogLevel(long responseTimeMs, int statusCode)
    {
        // Log as warning for slow requests or error status codes
        if (responseTimeMs > 5000 || statusCode >= 500)
        {
            return LogLevel.Warning;
        }

        // Log as information for normal requests
        if (responseTimeMs > 1000 || statusCode >= 400)
        {
            return LogLevel.Information;
        }

        // Log as debug for fast, successful requests
        return LogLevel.Debug;
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (when behind proxy/load balancer)
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ip = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(ip))
                return ip;
        }

        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.ToString();
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}