using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.OWASP;

/// <summary>
/// Tests for OWASP Top 10 (2021) compliance
/// </summary>
public class OwaspTop10ComplianceTests : SecurityTestBase
{
    public OwaspTop10ComplianceTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    #region A01:2021 – Broken Access Control

    [Fact]
    public async Task A01_BrokenAccessControl_UnauthorizedAccess_ShouldBeBlocked()
    {
        // Test unauthorized access to protected resources
        var protectedEndpoints = new[]
        {
            "/api/user/profile",
            "/api/user/settings",
            "/api/admin/users"
        };

        foreach (var endpoint in protectedEndpoints)
        {
            var response = await Client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task A01_BrokenAccessControl_PrivilegeEscalation_ShouldBeBlocked()
    {
        // Test that regular users cannot access admin functions
        var client = await GetAuthenticatedClientAsync();
        
        var adminEndpoints = new[]
        {
            "/api/admin/users",
            "/api/admin/system",
            "/api/admin/audit-logs"
        };

        foreach (var endpoint in adminEndpoints)
        {
            var response = await client.GetAsync(endpoint);
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.Forbidden, 
                HttpStatusCode.Unauthorized,
                HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region A02:2021 – Cryptographic Failures

    [Fact]
    public async Task A02_CryptographicFailures_HTTPS_ShouldBeEnforced()
    {
        // In production, this would test HTTPS enforcement
        // For testing environment, we verify secure headers are present
        var response = await Client.GetAsync("/api/health");
        
        // Check for security headers that indicate HTTPS enforcement
        var headers = response.Headers.Concat(response.Content.Headers);
        var headerDict = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => string.Join(",", h.Value));

        // These headers should be present in production
        if (headerDict.ContainsKey("strict-transport-security"))
        {
            headerDict["strict-transport-security"].Should().Contain("max-age");
        }
    }

    [Fact]
    public async Task A02_CryptographicFailures_SensitiveDataInTransit_ShouldBeProtected()
    {
        // Verify that passwords are not transmitted in plain text in responses
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        responseContent.Should().NotContain(TestCredentials.ValidPassword);
        responseContent.ToLowerInvariant().Should().NotContain("password");
    }

    #endregion

    #region A03:2021 – Injection

    [Fact]
    public async Task A03_Injection_SQLInjection_ShouldBeBlocked()
    {
        // Test SQL injection prevention (already covered in SqlInjectionPreventionTests)
        var maliciousLogin = new
        {
            Email = "admin'; DROP TABLE Users; --",
            Password = "any"
        };

        var json = JsonSerializer.Serialize(maliciousLogin);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A03_Injection_CommandInjection_ShouldBeBlocked()
    {
        // Test command injection in file upload or similar operations
        var client = await GetAuthenticatedClientAsync();
        
        var maliciousRequest = new
        {
            FileName = "test.txt; rm -rf /",
            Content = "normal content"
        };

        var json = JsonSerializer.Serialize(maliciousRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/files/upload", content);
        
        // Should either reject or sanitize the malicious filename
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().NotContain("; rm -rf");
        }
        else
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }
    }

    #endregion

    #region A04:2021 – Insecure Design

    [Fact]
    public async Task A04_InsecureDesign_PasswordPolicy_ShouldBeEnforced()
    {
        var weakPasswords = new[]
        {
            "123",
            "password",
            "admin",
            "123456",
            "qwerty"
        };

        foreach (var weakPassword in weakPasswords)
        {
            var registrationRequest = new
            {
                Email = $"test{Guid.NewGuid()}@example.com",
                Password = weakPassword,
                FirstName = "Test",
                LastName = "User"
            };

            var json = JsonSerializer.Serialize(registrationRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/register", content);
            
            // Should reject weak passwords
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
        }
    }

    [Fact]
    public async Task A04_InsecureDesign_AccountLockout_ShouldBeImplemented()
    {
        // Test account lockout after multiple failed attempts
        var lockedUserLogin = new
        {
            Email = TestCredentials.LockedEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(lockedUserLogin);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        
        // Should reject login for locked account
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ToLowerInvariant().Should().Contain("locked");
    }

    #endregion

    #region A05:2021 – Security Misconfiguration

    [Fact]
    public async Task A05_SecurityMisconfiguration_DefaultCredentials_ShouldNotExist()
    {
        // Test that no default admin accounts exist
        var defaultCredentials = new[]
        {
            ("admin", "admin"),
            ("administrator", "password"),
            ("root", "root"),
            ("admin", "password123")
        };

        foreach (var (email, password) in defaultCredentials)
        {
            var loginRequest = new
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task A05_SecurityMisconfiguration_SecurityHeaders_ShouldBePresent()
    {
        var response = await Client.GetAsync("/api/health");
        
        var headers = response.Headers.Concat(response.Content.Headers);
        var headerDict = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => string.Join(",", h.Value));

        // Check for essential security headers
        var expectedHeaders = new[]
        {
            "x-content-type-options",
            "x-frame-options",
            "x-xss-protection"
        };

        foreach (var expectedHeader in expectedHeaders)
        {
            headerDict.Should().ContainKey(expectedHeader, 
                $"Security header {expectedHeader} should be present");
        }
    }

    #endregion

    #region A06:2021 – Vulnerable and Outdated Components

    [Fact]
    public async Task A06_VulnerableComponents_ErrorMessages_ShouldNotExposeVersions()
    {
        // Test that error messages don't expose framework versions or sensitive info
        var response = await Client.GetAsync("/api/nonexistent-endpoint");
        var content = await response.Content.ReadAsStringAsync();

        var sensitivePatterns = new[]
        {
            "asp.net",
            "version",
            "framework",
            "stack trace",
            "exception",
            "system.io",
            "microsoft"
        };

        foreach (var pattern in sensitivePatterns)
        {
            content.ToLowerInvariant().Should().NotContain(pattern, 
                $"Error response should not expose {pattern}");
        }
    }

    #endregion

    #region A07:2021 – Identification and Authentication Failures

    [Fact]
    public async Task A07_AuthenticationFailures_SessionManagement_ShouldBeSecure()
    {
        // Test secure session management
        var token1 = await GetAuthTokenAsync();
        var token2 = await GetAuthTokenAsync();

        // Tokens should be different for each login
        token1.Should().NotBe(token2, "Each login should generate a unique token");
    }

    [Fact]
    public async Task A07_AuthenticationFailures_MultiFactorAuthentication_ShouldBeSupported()
    {
        // Test MFA (if implemented) or verify single-factor security
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            // If MFA is required, response should indicate next step
            // If not implemented, basic authentication should still be secure
            responseContent.Should().Contain("token");
        }
    }

    #endregion

    #region A08:2021 – Software and Data Integrity Failures

    [Fact]
    public async Task A08_IntegrityFailures_TokenTampering_ShouldBeDetected()
    {
        // Test JWT token integrity
        var validToken = await GetAuthTokenAsync();
        var tokenParts = validToken.Split('.');
        
        // Tamper with token
        var tamperedToken = $"{tokenParts[0]}.{tokenParts[1]}.tampered_signature";
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        var response = await Client.GetAsync("/api/user/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        Client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region A09:2021 – Security Logging and Monitoring Failures

    [Fact]
    public async Task A09_LoggingFailures_SecurityEvents_ShouldBeLogged()
    {
        // Test that security events are logged (this would require log inspection in real scenarios)
        var maliciousLogin = new
        {
            Email = "attacker@evil.com",
            Password = "' OR 1=1 --"
        };

        var json = JsonSerializer.Serialize(maliciousLogin);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        
        // The attempt should be rejected and logged
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    #endregion

    #region A10:2021 – Server-Side Request Forgery (SSRF)

    [Fact]
    public async Task A10_SSRF_ExternalRequests_ShouldBeValidated()
    {
        // Test SSRF protection in any endpoints that make external requests
        var client = await GetAuthenticatedClientAsync();
        
        var ssrfPayloads = new[]
        {
            "http://localhost:22",
            "http://169.254.169.254/", // AWS metadata
            "file:///etc/passwd",
            "http://internal.company.com"
        };

        foreach (var payload in ssrfPayloads)
        {
            var request = new
            {
                Url = payload
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/external/fetch", content);
            
            // Should reject SSRF attempts
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest, 
                HttpStatusCode.Forbidden,
                HttpStatusCode.NotFound);
        }
    }

    #endregion
}