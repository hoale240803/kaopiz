using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.PenetrationTesting;

/// <summary>
/// Comprehensive penetration testing scenarios for the authentication system
/// </summary>
public class PenetrationTestingScenarios : SecurityTestBase
{
    public PenetrationTestingScenarios(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    #region Authentication Bypass Scenarios

    [Fact]
    public async Task PenTest_AuthBypass_Scenario1_EmptyToken()
    {
        // Scenario: Attacker tries to bypass authentication with empty token
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");

        var response = await Client.GetAsync("/api/user/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task PenTest_AuthBypass_Scenario2_SqlInjectionInCredentials()
    {
        // Scenario: SQL injection attempt in login credentials
        var sqlInjectionAttempts = new[]
        {
            ("admin'--", "password"),
            ("' OR 1=1 --", "password"),
            ("admin", "' OR '1'='1"),
            ("'; EXEC xp_cmdshell('dir'); --", "password")
        };

        foreach (var (email, password) in sqlInjectionAttempts)
        {
            var loginRequest = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            VerifyNoSensitiveDataExposed(responseContent);
        }
    }

    [Fact]
    public async Task PenTest_AuthBypass_Scenario3_JwtManipulation()
    {
        // Scenario: JWT token manipulation attempts
        var validToken = await GetAuthTokenAsync();
        var parts = validToken.Split('.');

        var manipulationAttempts = new[]
        {
            $"{parts[0]}.{parts[1]}.manipulated_signature",
            $"manipulated_header.{parts[1]}.{parts[2]}",
            $"{parts[0]}.manipulated_payload.{parts[2]}",
            "fake.jwt.token",
            parts[0] + "." + parts[1] // Missing signature
        };

        foreach (var tamperedToken in manipulationAttempts)
        {
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

            var response = await Client.GetAsync("/api/user/profile");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        Client.DefaultRequestHeaders.Authorization = null;
    }

    #endregion

    #region Brute Force Attack Scenarios

    [Fact]
    public async Task PenTest_BruteForce_Scenario1_CredentialStuffing()
    {
        // Scenario: Credential stuffing attack with common email/password combinations
        var commonCredentials = new[]
        {
            ("admin@test.com", "admin"),
            ("test@test.com", "test123"),
            ("user@example.com", "password123"),
            ("demo@demo.com", "demo"),
            ("guest@guest.com", "guest")
        };

        var successfulLogins = 0;
        foreach (var (email, password) in commonCredentials)
        {
            var loginRequest = new { Email = email, Password = password };
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                successfulLogins++;
            }
        }

        successfulLogins.Should().Be(0, "No common credentials should work");
    }

    [Fact]
    public async Task PenTest_BruteForce_Scenario2_RateLimitBypass()
    {
        // Scenario: Attempt to bypass rate limiting
        var bypassAttempts = new[]
        {
            () => AddHeaderAndAttemptLogin("X-Forwarded-For", "192.168.1.100"),
            () => AddHeaderAndAttemptLogin("X-Real-IP", "10.0.0.100"),
            () => AddHeaderAndAttemptLogin("X-Originating-IP", "172.16.0.100"),
            () => AddHeaderAndAttemptLogin("User-Agent", "Different-Agent-" + Guid.NewGuid())
        };

        // First, trigger rate limit
        for (int i = 0; i < 6; i++)
        {
            await AttemptInvalidLogin();
        }

        // Then try bypass techniques
        foreach (var bypassAttempt in bypassAttempts)
        {
            var response = await bypassAttempt();
            response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests, 
                "Rate limit should not be bypassable with header manipulation");
        }
    }

    private async Task<HttpResponseMessage> AddHeaderAndAttemptLogin(string headerName, string headerValue)
    {
        if (Client.DefaultRequestHeaders.Contains(headerName))
        {
            Client.DefaultRequestHeaders.Remove(headerName);
        }
        Client.DefaultRequestHeaders.Add(headerName, headerValue);
        
        var response = await AttemptInvalidLogin();
        
        Client.DefaultRequestHeaders.Remove(headerName);
        return response;
    }

    private async Task<HttpResponseMessage> AttemptInvalidLogin()
    {
        var loginRequest = new
        {
            Email = TestCredentials.ValidEmail,
            Password = "WrongPassword"
        };
        
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        return await Client.PostAsync("/api/auth/login", content);
    }

    #endregion

    #region Information Disclosure Scenarios

    [Fact]
    public async Task PenTest_InfoDisclosure_Scenario1_ErrorMessages()
    {
        // Scenario: Extracting sensitive information from error messages
        var informationGatheringAttempts = new[]
        {
            "/api/auth/login",
            "/api/user/nonexistent",
            "/api/admin/secret",
            "/api/../../../etc/passwd",
            "/api/files/../../../../windows/system32/config/sam"
        };

        foreach (var endpoint in informationGatheringAttempts)
        {
            try
            {
                var response = await Client.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();
                
                // Verify no sensitive information is exposed
                var sensitivePatterns = new[]
                {
                    "database", "connection", "server", "path", "directory",
                    "version", "framework", "stack trace", "exception details",
                    "internal", "config", "settings", "password", "key", "token"
                };

                foreach (var pattern in sensitivePatterns)
                {
                    content.ToLowerInvariant().Should().NotContain(pattern);
                }
            }
            catch (Exception ex)
            {
                // Exception messages should not contain sensitive information
                ex.Message.ToLowerInvariant().Should().NotContain("database");
                ex.Message.ToLowerInvariant().Should().NotContain("connection");
            }
        }
    }

    [Fact]
    public async Task PenTest_InfoDisclosure_Scenario2_TimingAttacks()
    {
        // Scenario: Timing attack to enumerate valid user accounts
        var validEmail = TestCredentials.ValidEmail;
        var invalidEmail = "nonexistent" + Guid.NewGuid() + "@example.com";

        var validUserTimes = new List<TimeSpan>();
        var invalidUserTimes = new List<TimeSpan>();

        // Test multiple times to get average
        for (int i = 0; i < 5; i++)
        {
            // Test with valid user
            var validStart = DateTime.UtcNow;
            await AttemptLoginWith(validEmail, "wrongpassword");
            var validEnd = DateTime.UtcNow;
            validUserTimes.Add(validEnd - validStart);

            // Test with invalid user
            var invalidStart = DateTime.UtcNow;
            await AttemptLoginWith(invalidEmail, "wrongpassword");
            var invalidEnd = DateTime.UtcNow;
            invalidUserTimes.Add(invalidEnd - invalidStart);
        }

        var avgValidTime = validUserTimes.Average(t => t.TotalMilliseconds);
        var avgInvalidTime = invalidUserTimes.Average(t => t.TotalMilliseconds);

        // Response times should be similar to prevent user enumeration
        var timeDifference = Math.Abs(avgValidTime - avgInvalidTime);
        timeDifference.Should().BeLessThan(100, 
            "Response times should be similar to prevent user enumeration via timing attacks");
    }

    private async Task AttemptLoginWith(string email, string password)
    {
        var loginRequest = new { Email = email, Password = password };
        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await Client.PostAsync("/api/auth/login", content);
    }

    #endregion

    #region Session Management Scenarios

    [Fact]
    public async Task PenTest_SessionMgmt_Scenario1_TokenReuse()
    {
        // Scenario: Test token reuse and session fixation
        var token = await GetAuthTokenAsync();
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Use token multiple times
        var response1 = await Client.GetAsync("/api/user/profile");
        var response2 = await Client.GetAsync("/api/user/profile");

        response1.IsSuccessStatusCode.Should().BeTrue();
        response2.IsSuccessStatusCode.Should().BeTrue();

        // Logout
        await Client.PostAsync("/api/auth/logout", new StringContent("", Encoding.UTF8, "application/json"));

        // Try to use token after logout
        var response3 = await Client.GetAsync("/api/user/profile");
        response3.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "Token should be invalid after logout");
        
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task PenTest_SessionMgmt_Scenario2_ConcurrentSessions()
    {
        // Scenario: Test concurrent session management
        var token1 = await GetAuthTokenAsync();
        var token2 = await GetAuthTokenAsync();

        var client1 = Factory.CreateClient();
        var client2 = Factory.CreateClient();
        
        client1.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        client2.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var response1 = await client1.GetAsync("/api/user/profile");
        var response2 = await client2.GetAsync("/api/user/profile");

        // Both should work unless single-session policy is enforced
        response1.IsSuccessStatusCode.Should().BeTrue();
        response2.IsSuccessStatusCode.Should().BeTrue();
    }

    #endregion

    #region Input Validation Scenarios

    [Fact]
    public async Task PenTest_InputValidation_Scenario1_MassivePayloads()
    {
        // Scenario: Test with extremely large payloads
        var largeString = new string('A', 10000);
        var largePayload = new
        {
            Email = largeString + "@example.com",
            Password = largeString
        };

        var json = JsonSerializer.Serialize(largePayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        
        // Should reject oversized payloads
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task PenTest_InputValidation_Scenario2_SpecialCharacters()
    {
        // Scenario: Test with various special characters and encodings
        var specialCharacterTests = new[]
        {
            "test\x00@example.com", // Null byte
            "test\r\n@example.com", // CRLF injection
            "test@example.com\x7F", // DEL character
            "test@example.com\xFF", // High ASCII
            "test@例え.テスト", // Unicode
            "test@♠♣♥♦.com" // Special symbols
        };

        foreach (var testEmail in specialCharacterTests)
        {
            var loginRequest = new
            {
                Email = testEmail,
                Password = "TestPassword123!"
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            
            // Should handle special characters gracefully
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest, 
                HttpStatusCode.Unauthorized,
                HttpStatusCode.UnprocessableEntity);
        }
    }

    #endregion

    #region Comprehensive Attack Simulation

    [Fact]
    public async Task PenTest_FullAttackSimulation_MultiVectorAttack()
    {
        // Scenario: Simulate a comprehensive attack combining multiple vectors
        var attackVectors = new List<Func<Task<bool>>>
        {
            async () => await SimulateSqlInjectionAttack(),
            async () => await SimulateXssAttack(),
            async () => await SimulateBruteForceAttack(),
            async () => await SimulateAuthBypassAttack()
        };

        var successfulAttacks = 0;
        foreach (var attackVector in attackVectors)
        {
            try
            {
                var success = await attackVector();
                if (success) successfulAttacks++;
            }
            catch
            {
                // Attacks should fail safely
            }
        }

        successfulAttacks.Should().Be(0, "All attack vectors should be successfully defended against");
    }

    private async Task<bool> SimulateSqlInjectionAttack()
    {
        var payload = "admin'; DROP TABLE Users; --";
        var request = new { Email = payload, Password = "any" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/login", content);
        return response.IsSuccessStatusCode; // Should return false (attack failed)
    }

    private async Task<bool> SimulateXssAttack()
    {
        var payload = "<script>alert('XSS')</script>";
        var request = new { Email = $"test+{payload}@example.com", Password = "test" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync("/api/auth/register", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent.Contains("<script>"); // Should return false (XSS prevented)
        }
        return false;
    }

    private async Task<bool> SimulateBruteForceAttack()
    {
        for (int i = 0; i < 10; i++)
        {
            var request = new { Email = TestCredentials.ValidEmail, Password = $"wrong{i}" };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                return true; // Attack succeeded (bad)
            }
        }
        return false; // Attack failed (good)
    }

    private async Task<bool> SimulateAuthBypassAttack()
    {
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "bypass_token");

        var response = await Client.GetAsync("/api/user/profile");
        var success = response.IsSuccessStatusCode;
        
        Client.DefaultRequestHeaders.Authorization = null;
        return success; // Should return false (bypass failed)
    }

    #endregion
}