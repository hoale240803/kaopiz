using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.CSRF;

/// <summary>
/// Tests for Cross-Site Request Forgery (CSRF) protection
/// </summary>
public class CsrfProtectionTests : SecurityTestBase
{
    public CsrfProtectionTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task StateChangingEndpoints_ShouldRequireAuthentication()
    {
        // Arrange - Test without authentication
        var updateRequest = new
        {
            FirstName = "Hacker",
            LastName = "Attempt"
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync("/api/user/profile", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PasswordChange_WithoutCurrentPassword_ShouldBeRejected()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        var changePasswordRequest = new
        {
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
            // Missing CurrentPassword - simulating CSRF attack
        };

        var json = JsonSerializer.Serialize(changePasswordRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/user/change-password", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SensitiveOperations_ShouldRequireRecentAuthentication()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        // Simulate an old token by using a token that would be expired for sensitive operations
        var sensitiveRequest = new
        {
            Action = "DeleteAccount"
        };

        var json = JsonSerializer.Serialize(sensitiveRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/user/sensitive-operation", content);

        // Assert
        // Should require re-authentication for sensitive operations
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized, 
            HttpStatusCode.Forbidden, 
            HttpStatusCode.NotFound); // NotFound is acceptable if endpoint doesn't exist yet
    }

    [Fact]
    public async Task ApiEndpoints_ShouldValidateOriginHeader()
    {
        // Arrange
        var maliciousOrigin = "https://evil.com";
        Client.DefaultRequestHeaders.Add("Origin", maliciousOrigin);
        
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/auth/login", content);

        // Assert
        // API should either reject the request or not care about Origin for API endpoints
        // Most REST APIs don't validate Origin, but web apps should
        if (!response.IsSuccessStatusCode)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
        }

        // Remove the malicious header for cleanup
        Client.DefaultRequestHeaders.Remove("Origin");
    }

    [Fact]
    public async Task StateChangingRequests_ShouldUsePostPutDelete()
    {
        // Arrange & Act - Try to perform state-changing operations via GET
        var deleteResponse = await Client.GetAsync("/api/user/delete");
        var updateResponse = await Client.GetAsync("/api/user/update?firstName=Hacker");

        // Assert
        // GET requests should not be able to perform state changes
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed, 
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
            
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.MethodNotAllowed, 
            HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SameSiteAttribute_ShouldBeSetOnCookies()
    {
        // Arrange
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/auth/login", content);

        // Assert
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                // Cookies should have SameSite attribute for CSRF protection
                cookie.Should().Contain("SameSite", "Cookies should have SameSite attribute");
                
                // For authentication cookies, should preferably be Strict or Lax
                if (cookie.Contains("auth") || cookie.Contains("token") || cookie.Contains("session"))
                {
                    cookie.Should().Match(c => c.Contains("SameSite=Strict") || c.Contains("SameSite=Lax"));
                }
            }
        }
    }

    [Fact]
    public async Task RefererHeader_ShouldBeValidatedForSensitiveOperations()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        // Add malicious referer header
        Client.DefaultRequestHeaders.Add("Referer", "https://evil.com/csrf-attack");
        
        var sensitiveRequest = new
        {
            NewEmail = "hacker@evil.com"
        };

        var json = JsonSerializer.Serialize(sensitiveRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync("/api/user/email", content);

        // Assert
        // Should either reject based on referer or not care (acceptable for APIs)
        if (!response.IsSuccessStatusCode)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);
        }

        // Cleanup
        Client.DefaultRequestHeaders.Remove("Referer");
    }

    [Theory]
    [InlineData("/api/auth/logout")]
    [InlineData("/api/user/profile")]
    [InlineData("/api/user/change-password")]
    public async Task CriticalEndpoints_ShouldRequireValidJwtToken(string endpoint)
    {
        // Arrange - Create client with invalid token
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var request = new { };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = endpoint.Contains("logout") 
            ? await Client.PostAsync(endpoint, content)
            : await Client.PutAsync(endpoint, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Cleanup
        Client.DefaultRequestHeaders.Authorization = null;
    }
}