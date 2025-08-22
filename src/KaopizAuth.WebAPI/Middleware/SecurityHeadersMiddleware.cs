namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Middleware for adding security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
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
        var securityEnabled = _configuration.GetValue<bool>("Security:EnableSecurityHeaders", true);

        if (!securityEnabled)
        {
            return;
        }

        var headers = context.Response.Headers;

        // Remove server information
        headers.Remove("Server");

        // X-Content-Type-Options: Prevent MIME type sniffing
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Append("X-Content-Type-Options", "nosniff");
        }

        // X-Frame-Options: Prevent clickjacking
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Append("X-Frame-Options", "DENY");
        }

        // X-XSS-Protection: Enable XSS filtering
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Append("X-XSS-Protection", "1; mode=block");
        }

        // Referrer-Policy: Control referrer information
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        // Content-Security-Policy: Prevent XSS and data injection attacks
        if (!headers.ContainsKey("Content-Security-Policy") && !IsApiRequest(context))
        {
            headers.Append("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';");
        }

        // Strict-Transport-Security: Force HTTPS (only add if request is HTTPS)
        if (context.Request.IsHttps && !headers.ContainsKey("Strict-Transport-Security"))
        {
            headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        // Permissions-Policy: Control browser features
        if (!headers.ContainsKey("Permissions-Policy"))
        {
            headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), interest-cohort=()");
        }

        // X-Permitted-Cross-Domain-Policies: Control cross-domain requests
        if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
        {
            headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        }

        // Cache-Control for API endpoints
        if (IsApiRequest(context) && !headers.ContainsKey("Cache-Control"))
        {
            headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            headers.Append("Pragma", "no-cache");
            headers.Append("Expires", "0");
        }

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

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api") ||
               context.Request.Headers.Accept.Any(x => x?.Contains("application/json") == true);
    }
}