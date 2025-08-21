using System.Diagnostics;

namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Middleware for performance monitoring and request logging
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
        context.Response.Headers.Add("X-Request-ID", requestId);

        // Log request start
        if (requestLoggingEnabled)
        {
            _logger.LogInformation("Request started: {Method} {Path} - RequestId: {RequestId}", 
                context.Request.Method, 
                context.Request.Path, 
                requestId);
        }

        Exception? requestException = null;
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
                context.Response.Headers.Add("X-Response-Time-ms", elapsedMs.ToString());
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
}