using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.XSS;

/// <summary>
/// Tests for Cross-Site Scripting (XSS) protection
/// </summary>
public class XssProtectionTests : SecurityTestBase
{
    public XssProtectionTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [MemberData(nameof(GetXssPayloads))]
    public async Task UserProfile_WithXssInUserData_ShouldBeSanitized(string xssPayload)
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        var updateRequest = new
        {
            FirstName = xssPayload,
            LastName = "SafeLastName"
        };

        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var (response, exception) = await SafeExecuteAsync(() => client.PutAsync("/api/user/profile", content));

        // Assert
        if (exception != null)
        {
            // Should not expose internal errors
            exception.Message.Should().NotContain("<script>");
            exception.Message.Should().NotContain("javascript:");
        }

        if (response != null)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Response should not contain executable scripts
            responseContent.Should().NotContain("<script>");
            responseContent.Should().NotContain("javascript:");
            responseContent.Should().NotContain("onload=");
            responseContent.Should().NotContain("onerror=");
            responseContent.Should().NotContain("onmouseover=");
            
            // Should either reject the request or sanitize the content
            if (response.IsSuccessStatusCode)
            {
                // Verify XSS payload was sanitized
                responseContent.Should().NotContain(xssPayload);
            }
            else
            {
                response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetXssPayloads))]
    public async Task Registration_WithXssInUserData_ShouldBeSanitized(string xssPayload)
    {
        // Arrange
        var registrationRequest = new
        {
            Email = $"test{Guid.NewGuid()}@example.com",
            Password = "SecurePassword123!",
            FirstName = xssPayload,
            LastName = "SafeLastName"
        };

        var json = JsonSerializer.Serialize(registrationRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var (response, exception) = await SafeExecuteAsync(() => Client.PostAsync("/api/auth/register", content));

        // Assert
        if (response != null)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Response should not echo back executable scripts
            responseContent.Should().NotContain("<script>");
            responseContent.Should().NotContain("javascript:");
            responseContent.Should().NotContain("alert(");
            
            if (response.IsSuccessStatusCode)
            {
                // If registration succeeds, XSS should be sanitized
                responseContent.Should().NotContain(xssPayload);
            }
        }
    }

    [Fact]
    public async Task ContentSecurityPolicy_HeadersShouldBePresent()
    {
        // Act
        var response = await Client.GetAsync("/api/health");

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options")
            .WhoseValue.Should().Contain("nosniff");
        
        response.Headers.Should().ContainKey("X-Frame-Options")
            .WhoseValue.Should().Contain("DENY");
        
        response.Headers.Should().ContainKey("X-XSS-Protection")
            .WhoseValue.Should().Contain("1; mode=block");
    }

    [Fact]
    public async Task ApiResponse_ShouldHaveSecureContentType()
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
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        response.Content.Headers.ContentType?.CharSet.Should().Be("utf-8");
    }

    [Theory]
    [InlineData("text/html")]
    [InlineData("text/xml")]
    [InlineData("application/xml")]
    public async Task ApiEndpoints_ShouldRejectNonJsonContentTypes(string contentType)
    {
        // Arrange
        var maliciousContent = "<script>alert('XSS')</script>";
        var content = new StringContent(maliciousContent, Encoding.UTF8, contentType);

        // Act
        var response = await Client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnsupportedMediaType, 
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ErrorPages_ShouldNotReflectUserInput()
    {
        // Arrange
        var maliciousInput = "<script>alert('XSS')</script>";
        
        // Act
        var response = await Client.GetAsync($"/api/nonexistent?param={Uri.EscapeDataString(maliciousInput)}");

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // Error response should not reflect the malicious input
        responseContent.Should().NotContain("<script>");
        responseContent.Should().NotContain("alert(");
        responseContent.Should().NotContain(maliciousInput);
    }

    public static IEnumerable<object[]> GetXssPayloads()
    {
        return XssPayloads.CommonPayloads.Select(payload => new object[] { payload });
    }
}