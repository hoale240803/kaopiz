using System.Net.Http.Json;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NBomber.CSharp;
using Bogus;
using KaopizAuth.Application.DTOs.Auth;

namespace KaopizAuth.PerformanceTests;

public class AuthEndpointsPerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthEndpointsPerformanceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldComplete_WithinTimeout()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var registerRequest = new RegisterRequestDto
        {
            Email = "perf-test@example.com",
            Password = "Test@123456",
            FirstName = "Performance",
            LastName = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "Registration should complete within 200ms");
    }

    [Fact]
    public async Task Login_ShouldComplete_WithinTimeout()
    {
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            Email = "login-perf@example.com",
            Password = "Test@123456",
            FirstName = "Login",
            LastName = "Performance"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login-perf@example.com",
            Password = "Test@123456"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        stopwatch.Stop();
        response.IsSuccessStatusCode.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200, "Login should complete within 200ms");
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldHandleLoad()
    {
        // Arrange
        var tasks = new List<Task<bool>>();
        const int concurrentRequests = 50;

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            var requestIndex = i;
            tasks.Add(Task.Run(async () =>
            {
                var registerRequest = new RegisterRequestDto
                {
                    Email = $"concurrent{requestIndex}@example.com",
                    Password = "Test@123456",
                    FirstName = "Concurrent",
                    LastName = $"Test{requestIndex}"
                };

                var stopwatch = Stopwatch.StartNew();
                var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
                stopwatch.Stop();

                return response.IsSuccessStatusCode && stopwatch.ElapsedMilliseconds < 500;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.All(r => r).Should().BeTrue("All concurrent requests should succeed within timeout");
    }
}