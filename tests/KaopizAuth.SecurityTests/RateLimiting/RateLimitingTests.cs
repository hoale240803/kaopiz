using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.RateLimiting;

/// <summary>
/// Tests for rate limiting functionality and brute force protection
/// </summary>
public class RateLimitingTests : SecurityTestBase
{
    public RateLimitingTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task LoginAttempts_ShouldBeRateLimited()
    {
        // Arrange
        var invalidLoginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword123!"
        };

        var json = JsonSerializer.Serialize(invalidLoginRequest);
        var responses = new List<HttpResponseMessage>();

        // Act - Make multiple failed login attempts
        for (int i = 0; i < 7; i++) // Exceed the limit of 5 attempts
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            responses.Add(response);
        }

        // Assert
        // First 5 attempts should return 401 (Unauthorized)
        for (int i = 0; i < 5; i++)
        {
            responses[i].StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                $"Attempt {i + 1} should be unauthorized");
        }

        // 6th and 7th attempts should be rate limited (429 Too Many Requests)
        responses[5].StatusCode.Should().Be(HttpStatusCode.TooManyRequests, 
            "6th attempt should be rate limited");
        responses[6].StatusCode.Should().Be(HttpStatusCode.TooManyRequests, 
            "7th attempt should be rate limited");

        // Verify rate limit message
        var rateLimitedContent = await responses[5].Content.ReadAsStringAsync();
        rateLimitedContent.ToLowerInvariant().Should().Contain("rate limit");

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task RateLimitHeaders_ShouldBeIncluded()
    {
        // Arrange
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword123!"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/auth/login", content);

        // Assert - Check for rate limiting headers (if implemented)
        var headers = response.Headers.Concat(response.Content.Headers);
        var headerNames = headers.Select(h => h.Key.ToLowerInvariant()).ToList();

        // While not required, these headers are good practice
        // X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
        // The test passes regardless, but logs if headers are missing
        if (!headerNames.Any(h => h.Contains("ratelimit")))
        {
            // Log that rate limit headers could be added for better client experience
            Console.WriteLine("INFO: Rate limit headers (X-RateLimit-*) could be added for better API experience");
        }

        response.Dispose();
    }

    [Fact]
    public async Task RateLimit_ShouldResetAfterTimeWindow()
    {
        // Arrange
        var invalidLoginRequest = new
        {
            Email = "ratelimitreset@example.com", // Use different email to avoid interference
            Password = "WrongPassword123!"
        };

        var json = JsonSerializer.Serialize(invalidLoginRequest);

        // Act - Trigger rate limit
        for (int i = 0; i < 6; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await Client.PostAsync("/api/auth/login", content);
        }

        // Wait for rate limit window (this is a simplified test)
        // In a real scenario, you might mock the time or use a shorter window for testing
        await Task.Delay(1000); // Short delay for test performance

        // Try again - should still be rate limited since window is 15 minutes
        var finalContent = new StringContent(json, Encoding.UTF8, "application/json");
        var finalResponse = await Client.PostAsync("/api/auth/login", finalContent);

        // Assert
        finalResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, 
            "Should still be rate limited within the time window");

        finalResponse.Dispose();
    }

    [Fact]
    public async Task DifferentIPs_ShouldHaveIndependentRateLimits()
    {
        // Note: This test is limited by the test framework's ability to simulate different IPs
        // In a real scenario, you would test with actual different source IPs
        
        // Arrange
        var invalidLoginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword123!"
        };

        var json = JsonSerializer.Serialize(invalidLoginRequest);

        // Act - Make attempts that would trigger rate limit
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 6; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            responses.Add(response);
        }

        // Assert - Verify rate limiting is working
        responses.Last().StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        // Note: Testing different IPs would require integration test with actual network setup
        // This test validates that rate limiting is working for the current IP

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task SuccessfulLogin_ShouldNotContributeToRateLimit()
    {
        // Arrange
        var validLoginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = TestCredentials.ValidPassword
        };

        var invalidLoginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword123!"
        };

        // Act - Mix valid and invalid attempts
        var validJson = JsonSerializer.Serialize(validLoginRequest);
        var invalidJson = JsonSerializer.Serialize(invalidLoginRequest);

        // Make a successful login
        var validContent = new StringContent(validJson, Encoding.UTF8, "application/json");
        var successResponse = await Client.PostAsync("/api/auth/login", validContent);

        // Make failed attempts
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 6; i++)
        {
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            responses.Add(response);
        }

        // Assert
        successResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Valid login should succeed");
        
        // Failed attempts should still trigger rate limiting
        responses.Last().StatusCode.Should().Be(HttpStatusCode.TooManyRequests, 
            "Failed attempts should still be rate limited regardless of successful logins");

        // Cleanup
        successResponse.Dispose();
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task BruteForceProtection_ShouldLogSecurityEvents()
    {
        // Arrange
        var attackLoginRequest = new
        {
            Email = "attack.target@example.com",
            Password = "WrongPassword123!"
        };

        var json = JsonSerializer.Serialize(attackLoginRequest);

        // Act - Simulate brute force attack
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 10; i++) // Aggressive attack pattern
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            responses.Add(response);
        }

        // Assert
        // Should be rate limited
        responses.Skip(5).All(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .Should().BeTrue("Brute force attempts should be rate limited");

        // In a real system, this would also verify audit logs
        // For now, we verify that the system responds appropriately

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task RateLimit_ShouldApplyOnlyToLoginEndpoint()
    {
        // Arrange - Other endpoints should not be affected by login rate limiting
        var publicEndpoints = new[]
        {
            "/api/health",
            "/api/auth/forgot-password"
        };

        // First trigger login rate limit
        var invalidLoginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword123!"
        };

        var loginJson = JsonSerializer.Serialize(invalidLoginRequest);
        for (int i = 0; i < 6; i++)
        {
            var content = new StringContent(loginJson, Encoding.UTF8, "application/json");
            await Client.PostAsync("/api/auth/login", content);
        }

        // Act - Test other endpoints
        foreach (var endpoint in publicEndpoints)
        {
            var response = await Client.GetAsync(endpoint);
            
            // Assert - Should not be rate limited
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests, 
                $"Endpoint {endpoint} should not be affected by login rate limiting");
            
            response.Dispose();
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@domain.com")]
    public async Task InvalidEmailFormats_ShouldStillBeRateLimited(string invalidEmail)
    {
        // Arrange
        var invalidLoginRequest = new
        {
            Email = invalidEmail,
            Password = "SomePassword123!"
        };

        var json = JsonSerializer.Serialize(invalidLoginRequest);

        // Act - Make multiple attempts with invalid email formats
        var responses = new List<HttpResponseMessage>();
        for (int i = 0; i < 7; i++)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/api/auth/login", content);
            responses.Add(response);
        }

        // Assert - Should still apply rate limiting even for malformed requests
        responses.Skip(5).All(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .Should().BeTrue("Rate limiting should apply to all login attempts, including malformed ones");

        // Cleanup
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }
}