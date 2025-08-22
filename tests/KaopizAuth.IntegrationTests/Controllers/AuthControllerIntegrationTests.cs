using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using KaopizAuth.Application.DTOs.Auth;
using KaopizAuth.IntegrationTests.TestDataBuilders;
using Microsoft.Extensions.DependencyInjection;

namespace KaopizAuth.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public AuthControllerIntegrationTests(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        await _factory.InitializeAsync();
        
        var registerRequest = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Test@123456",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await _factory.InitializeAsync();
        
        var registerRequest = new RegisterRequestDto
        {
            Email = "invalid-email",
            Password = "Test@123456",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        await _factory.InitializeAsync();
        
        // First register a user
        var registerRequest = new RegisterRequestDto
        {
            Email = "login@example.com",
            Password = "Test@123456",
            FirstName = "Jane",
            LastName = "Smith"
        };
        
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login@example.com",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("wrong@example.com", "Test@123456")] // Wrong email
    [InlineData("login@example.com", "WrongPassword")] // Wrong password
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized(string email, string password)
    {
        // Arrange
        await _factory.InitializeAsync();
        
        var loginRequest = new LoginRequestDto
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}