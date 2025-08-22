namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Middleware to add security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing the request
        AddSecurityHeaders(context);

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var response = context.Response;

        // X-Frame-Options: Prevents clickjacking attacks
        if (!response.Headers.ContainsKey("X-Frame-Options"))
        {
            response.Headers.Append("X-Frame-Options", "DENY");
        }

        // X-Content-Type-Options: Prevents MIME type sniffing
        if (!response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            response.Headers.Append("X-Content-Type-Options", "nosniff");
        }

        // X-XSS-Protection: Enables XSS filtering
        if (!response.Headers.ContainsKey("X-XSS-Protection"))
        {
            response.Headers.Append("X-XSS-Protection", "1; mode=block");
        }

        // Strict-Transport-Security (HSTS): Forces HTTPS
        if (context.Request.IsHttps && !response.Headers.ContainsKey("Strict-Transport-Security"))
        {
            response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        // Referrer-Policy: Controls referrer information
        if (!response.Headers.ContainsKey("Referrer-Policy"))
        {
            response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        // Content-Security-Policy: Prevents various injection attacks
        if (!response.Headers.ContainsKey("Content-Security-Policy"))
        {
            response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none'");
        }

        _logger.LogDebug("Security headers added to response for {Path}", context.Request.Path);
    }
}