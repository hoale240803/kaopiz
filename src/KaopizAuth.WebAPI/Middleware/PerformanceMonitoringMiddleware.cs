using System.Diagnostics;

namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Middleware to monitor and log API performance metrics
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    private const string ResponseTimeHeaderName = "X-Response-Time-ms";

    public async Task InvokeAsync(HttpContext context)
    {
        var performanceEnabled = _configuration.GetValue<bool>("Performance:EnableMetrics", true);
        var requestLoggingEnabled = _configuration.GetValue<bool>("Performance:RequestLogging:Enabled", true);
        var logSlowRequests = _configuration.GetValue<bool>("Performance:RequestLogging:LogSlowRequests", true);
        var slowRequestThreshold = _configuration.GetValue<int>("Performance:RequestLogging:SlowRequestThresholdMs", 1000);

        if (!performanceEnabled && !requestLoggingEnabled)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Add request ID to response headers
        context.Response.Headers.Append("X-Request-ID", requestId);

        // Log request start
        if (requestLoggingEnabled)
        {
            _logger.LogInformation("Request started: {Method} {Path} - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                requestId);
        }

        Exception? requestException = null;

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            requestException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Add performance headers
            if (performanceEnabled)
            {
                context.Response.Headers.Append("X-Response-Time-ms", elapsedMs.ToString());
            }

            // Log request completion
            if (requestLoggingEnabled)
            {
                var logLevel = DetermineLogLevel(context.Response.StatusCode, elapsedMs, slowRequestThreshold, requestException);

                _logger.Log(logLevel, requestException,
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMs}ms - RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsedMs,
                    requestId);
            }

            // Log slow requests
            if (logSlowRequests && elapsedMs > slowRequestThreshold)
            {
                _logger.LogWarning("Slow request detected: {Method} {Path} - Duration: {ElapsedMs}ms - RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    requestId);
            }

            // Collect metrics (in a real application, you might send these to a metrics collector like Prometheus)
            if (performanceEnabled)
            {
                CollectMetrics(context, elapsedMs, requestException);
            }
        }
    }

    private static LogLevel DetermineLogLevel(int statusCode, long elapsedMs, int slowThreshold, Exception? exception)
    {
        if (exception != null)
            return LogLevel.Error;

        if (statusCode >= 500)
            return LogLevel.Error;

        if (statusCode >= 400)
            return LogLevel.Warning;

        if (elapsedMs > slowThreshold)
            return LogLevel.Warning;

        return LogLevel.Information;
    }

    private void CollectMetrics(HttpContext context, long elapsedMs, Exception? exception)
    {
        // In a production environment, you would send these metrics to a monitoring system
        // like Prometheus, Application Insights, or New Relic

        var endpoint = $"{context.Request.Method} {context.Request.Path.Value}";
        var statusCode = context.Response.StatusCode;
        var success = exception == null && statusCode < 400;

        // Example: Log metrics that could be consumed by external monitoring
        _logger.LogInformation("METRICS: Endpoint={Endpoint} StatusCode={StatusCode} Duration={Duration}ms Success={Success}",
            endpoint, statusCode, elapsedMs, success);
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