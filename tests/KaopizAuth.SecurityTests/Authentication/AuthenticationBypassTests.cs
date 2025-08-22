using KaopizAuth.SecurityTests.Infrastructure;

namespace KaopizAuth.SecurityTests.Authentication;

/// <summary>
/// Tests for authentication bypass vulnerabilities
/// </summary>
public class AuthenticationBypassTests : SecurityTestBase
{
    public AuthenticationBypassTests(SecurityTestWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("")]
    [InlineData("Bearer")]
    [InlineData("Bearer ")]
    [InlineData("InvalidScheme token")]
    [InlineData("Bearer invalid.jwt.token")]
    public async Task ProtectedEndpoints_WithInvalidTokens_ShouldRejectAccess(string authHeader)
    {
        // Arrange
        if (!string.IsNullOrEmpty(authHeader))
        {
            var parts = authHeader.Split(' ');
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    parts[0], 
                    parts.Length > 1 ? parts[1] : null);
        }

        // Act
        var response = await Client.GetAsync("/api/user/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Cleanup
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task ExpiredToken_ShouldBeRejected()
    {
        // This test would require a way to generate expired tokens
        // For now, we simulate with a malformed token that looks expired
        
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.invalid";
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await Client.GetAsync("/api/user/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Cleanup
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Theory]
    [InlineData("admin@system.com")]
    [InlineData("root@localhost")]
    [InlineData("administrator@domain.com")]
    public async Task DefaultCredentials_ShouldNotExist(string adminEmail)
    {
        // Arrange
        var commonPasswords = new[] { "admin", "password", "123456", "admin123", "root", "password123" };

        foreach (var password in commonPasswords)
        {
            var loginRequest = new
            {
                Email = adminEmail,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                $"Default admin credentials should not exist: {adminEmail}/{password}");
        }
    }

    [Fact]
    public async Task PasswordReset_WithoutValidToken_ShouldBeRejected()
    {
        // Arrange
        var resetRequest = new
        {
            Token = "invalid-reset-token",
            Email = TestCredentials.ValidEmail,
            NewPassword = "NewPassword123!"
        };

        var json = JsonSerializer.Serialize(resetRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/auth/reset-password", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("null")]
    [InlineData("undefined")]
    public async Task Login_WithEmptyCredentials_ShouldBeRejected(string emptyValue)
    {
        // Arrange
        var loginRequest = new
        {
            Email = emptyValue,
            Password = emptyValue
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/auth/login", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenValidation_ShouldVerifySignature()
    {
        // Arrange - Create a token with invalid signature
        var validToken = await GetAuthTokenAsync();
        var tokenParts = validToken.Split('.');
        
        // Tamper with the signature
        var tamperedToken = $"{tokenParts[0]}.{tokenParts[1]}.invalid_signature";
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await Client.GetAsync("/api/user/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Cleanup
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task JwtClaims_ShouldBeValidated()
    {
        // Arrange - Try to manipulate JWT payload (this should fail due to signature validation)
        var validToken = await GetAuthTokenAsync();
        var tokenParts = validToken.Split('.');
        
        // Try to decode and modify payload (in real scenario, this would require valid signature)
        var originalPayload = Convert.FromBase64String(AddPadding(tokenParts[1]));
        var payloadJson = Encoding.UTF8.GetString(originalPayload);
        
        // The token should be validated properly, this test verifies the endpoint doesn't accept tampered tokens
        var tamperedToken = $"{tokenParts[0]}.{Base64UrlEncode("{\"sub\":\"admin\",\"role\":\"Administrator\"}")}.{tokenParts[2]}";
        
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tamperedToken);

        // Act
        var response = await Client.GetAsync("/api/user/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Cleanup
        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task SessionFixation_ShouldBePreventedAfterLogin()
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
        var firstResponse = await Client.PostAsync("/api/auth/login", content);
        var secondResponse = await Client.PostAsync("/api/auth/login", content);

        // Assert
        firstResponse.IsSuccessStatusCode.Should().BeTrue();
        secondResponse.IsSuccessStatusCode.Should().BeTrue();

        // Extract tokens from both responses
        var firstContent = await firstResponse.Content.ReadAsStringAsync();
        var secondContent = await secondResponse.Content.ReadAsStringAsync();

        var firstToken = JsonSerializer.Deserialize<JsonElement>(firstContent)
            .GetProperty("data").GetProperty("accessToken").GetString();
        var secondToken = JsonSerializer.Deserialize<JsonElement>(secondContent)
            .GetProperty("data").GetProperty("accessToken").GetString();

        // Tokens should be different to prevent session fixation
        firstToken.Should().NotBe(secondToken, "New login should generate new token to prevent session fixation");
    }

    [Fact]
    public async Task ConcurrentSessions_ShouldBeHandledSecurely()
    {
        // Arrange
        var token1 = await GetAuthTokenAsync();
        var token2 = await GetAuthTokenAsync();

        // Act - Use both tokens concurrently
        var client1 = Factory.CreateClient();
        var client2 = Factory.CreateClient();
        
        client1.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token1);
        client2.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token2);

        var response1 = await client1.GetAsync("/api/user/profile");
        var response2 = await client2.GetAsync("/api/user/profile");

        // Assert
        // Both tokens should work unless the system implements single-session policy
        response1.IsSuccessStatusCode.Should().BeTrue();
        response2.IsSuccessStatusCode.Should().BeTrue();
    }

    private static string AddPadding(string base64String)
    {
        var padding = base64String.Length % 4;
        if (padding == 2) base64String += "==";
        else if (padding == 3) base64String += "=";
        return base64String;
    }

    private static string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}