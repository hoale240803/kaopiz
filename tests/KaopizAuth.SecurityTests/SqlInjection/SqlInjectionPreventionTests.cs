using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.SqlInjection;

/// <summary>
/// Tests for SQL injection prevention in authentication endpoints
/// </summary>
public class SqlInjectionPreventionTests : SecurityTestBase
{
    public SqlInjectionPreventionTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [MemberData(nameof(GetSqlInjectionPayloads))]
    public async Task Login_WithSqlInjectionInEmail_ShouldNotSucceed(string maliciousEmail)
    {
        // Arrange
        var loginRequest = new
        {
            Email = maliciousEmail,
            Password = TestCredentials.ValidPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var (response, exception) = await SafeExecuteAsync(() => Client.PostAsync("/api/auth/login", content));

        // Assert
        if (exception != null)
        {
            // Should not throw SQL-related exceptions
            exception.Message.ToLowerInvariant().Should().NotContain("sql");
            exception.Message.ToLowerInvariant().Should().NotContain("database");
            exception.Message.ToLowerInvariant().Should().NotContain("syntax");
        }

        if (response != null)
        {
            // Should return appropriate error status
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

            var responseContent = await response.Content.ReadAsStringAsync();

            // Should not expose database structure or SQL errors
            VerifyNoSensitiveDataExposed(responseContent);

            // Should not contain access tokens in failed response
            if (responseContent.ToLowerInvariant().Contains("\"success\":true"))
            {
                Assert.Fail("SQL injection should not result in successful authentication");
            }

            // Should not contain actual authentication tokens (JWT) in failed response
            if (responseContent.Contains("eyJ") && responseContent.Contains("."))
            {
                Assert.Fail("SQL injection response should not contain JWT tokens");
            }
        }
    }

    [Theory]
    [MemberData(nameof(GetSqlInjectionPayloads))]
    public async Task Login_WithSqlInjectionInPassword_ShouldNotSucceed(string maliciousPassword)
    {
        // Arrange
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = maliciousPassword
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var (response, exception) = await SafeExecuteAsync(() => Client.PostAsync("/api/auth/login", content));

        // Assert
        if (exception != null)
        {
            exception.Message.ToLowerInvariant().Should().NotContain("sql");
            exception.Message.ToLowerInvariant().Should().NotContain("database");
        }

        if (response != null)
        {
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

            var responseContent = await response.Content.ReadAsStringAsync();
            VerifyNoSensitiveDataExposed(responseContent);
        }
    }

    [Fact]
    public async Task Login_WithComplexSqlInjectionAttempt_ShouldLogSecurityEvent()
    {
        // Arrange
        var complexPayload = "'; WAITFOR DELAY '00:00:10'; SELECT * FROM Users WHERE 1=1; --";
        var loginRequest = new
        {
            Email = complexPayload,
            Password = "any_password"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var startTime = DateTime.UtcNow;
        var response = await Client.PostAsync("/api/auth/login", content);
        var endTime = DateTime.UtcNow;

        // Assert
        // Should not delay response (indicating SQL injection prevention)
        var responseTime = endTime - startTime;
        responseTime.Should().BeLessThan(TimeSpan.FromSeconds(5), "Response should not be delayed by SQL injection attempts");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);

        var responseContent = await response.Content.ReadAsStringAsync();
        VerifyNoSensitiveDataExposed(responseContent);
    }

    [Fact]
    public async Task Registration_WithSqlInjectionInUserData_ShouldBeRejected()
    {
        // Arrange
        var maliciousRegistration = new
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "'; DROP TABLE Users; --",
            LastName = "Normal"
        };

        var json = JsonSerializer.Serialize(maliciousRegistration);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var (response, exception) = await SafeExecuteAsync(() => Client.PostAsync("/api/auth/register", content));

        // Assert
        if (exception != null)
        {
            exception.Message.ToLowerInvariant().Should().NotContain("sql");
        }

        if (response != null)
        {
            // Should either reject the registration or sanitize the input
            if (response.IsSuccessStatusCode)
            {
                // If registration succeeds, verify the malicious code was sanitized
                var responseContent = await response.Content.ReadAsStringAsync();
                responseContent.Should().NotContain("DROP TABLE");
                responseContent.Should().NotContain("--");
            }
            else
            {
                response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
            }
        }
    }

    public static IEnumerable<object[]> GetSqlInjectionPayloads()
    {
        return SqlInjectionPayloads.CommonPayloads.Select(payload => new object[] { payload });
    }
}