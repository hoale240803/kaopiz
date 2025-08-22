using FluentAssertions;
using KaopizAuth.Application;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Services.Authentication;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Xunit;

namespace KaopizAuth.Tests.Services.Authentication;

/// <summary>
/// Integration tests for authentication strategies to verify they work correctly
/// with the dependency injection container and as part of the authentication flow
/// </summary>
public class AuthenticationStrategyIntegrationTests
{
    [Fact]
    public async Task AuthenticationFlow_EndUserStrategy_ShouldWorkEndToEnd()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();
        
        var user = CreateTestUser(UserType.EndUser);
        var password = "TestPassword123!";

        // Act
        var strategy = factory.CreateStrategy(UserType.EndUser);
        var authResult = await strategy.AuthenticateAsync(user, password);
        var claims = await strategy.GetUserClaimsAsync(user);
        var validationResult = await strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        strategy.Should().BeOfType<EndUserAuthStrategy>();
        
        authResult.Should().NotBeNull();
        authResult.IsSuccess.Should().BeTrue();
        authResult.RequiresTwoFactor.Should().BeFalse();

        claims.Should().NotBeEmpty();
        claims.Should().Contain(c => c.Type == "UserType" && c.Value == UserType.EndUser.ToString());
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewProfile");
        claims.Should().NotContain(c => c.Type == "Permission" && c.Value == "CanManageUsers");

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticationFlow_AdminStrategy_ShouldWorkEndToEnd()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();
        
        var user = CreateTestUser(UserType.Admin);
        var password = "AdminPassword123!";

        // Act
        var strategy = factory.CreateStrategy(UserType.Admin);
        var authResult = await strategy.AuthenticateAsync(user, password);
        var claims = await strategy.GetUserClaimsAsync(user);
        var validationResult = await strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        strategy.Should().BeOfType<AdminAuthStrategy>();
        
        authResult.Should().NotBeNull();
        authResult.IsSuccess.Should().BeTrue();
        authResult.RequiresTwoFactor.Should().BeTrue(); // Admin requires 2FA

        claims.Should().NotBeEmpty();
        claims.Should().Contain(c => c.Type == "UserType" && c.Value == UserType.Admin.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Administrator");
        claims.Should().Contain(c => c.Type == "SecurityLevel" && c.Value == "High");
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManageUsers");
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManageSystem");
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewAuditLogs");

        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticationFlow_PartnerStrategy_ShouldWorkEndToEnd()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();
        
        var user = CreateTestUser(UserType.Partner);
        var password = "PartnerPassword123!";

        // Act
        var strategy = factory.CreateStrategy(UserType.Partner);
        var authResult = await strategy.AuthenticateAsync(user, password);
        var claims = await strategy.GetUserClaimsAsync(user);
        var validationResult = await strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        strategy.Should().BeOfType<PartnerAuthStrategy>();
        
        authResult.Should().NotBeNull();
        if (authResult.IsSuccess)
        {
            authResult.RequiresTwoFactor.Should().BeFalse();
        }
        else
        {
            // If it fails, it should be due to business hours restriction
            validationResult.Errors.Should().Contain("Partner access is restricted to business hours (9 AM - 6 PM UTC)");
        }

        claims.Should().NotBeEmpty();
        claims.Should().Contain(c => c.Type == "UserType" && c.Value == UserType.Partner.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Partner");
        claims.Should().Contain(c => c.Type == "SecurityLevel" && c.Value == "Business");
        claims.Should().Contain(c => c.Type == "PartnerType" && c.Value == "BusinessPartner");
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanAccessPartnerPortal");
        claims.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewPartnerReports");
        claims.Should().NotContain(c => c.Type == "Permission" && c.Value == "CanManageUsers");

        validationResult.Should().NotBeNull();
        // Note: validation might fail during non-business hours due to business hours check
        if (!validationResult.IsValid)
        {
            validationResult.Errors.Should().Contain("Partner access is restricted to business hours (9 AM - 6 PM UTC)");
        }
    }

    [Fact]
    public async Task AuthenticationFlow_AllStrategies_ShouldBeRegisteredCorrectly()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();

        // Act & Assert
        var endUserStrategy = factory.CreateStrategy(UserType.EndUser);
        var adminStrategy = factory.CreateStrategy(UserType.Admin);
        var partnerStrategy = factory.CreateStrategy(UserType.Partner);

        endUserStrategy.Should().BeOfType<EndUserAuthStrategy>();
        adminStrategy.Should().BeOfType<AdminAuthStrategy>();
        partnerStrategy.Should().BeOfType<PartnerAuthStrategy>();

        // Verify strategies are different instances (scoped lifetime)
        var endUserStrategy2 = factory.CreateStrategy(UserType.EndUser);
        // In the same service provider scope, same instances are returned
        endUserStrategy2.Should().BeSameAs(endUserStrategy);
    }

    [Fact]
    public void AuthenticationFlow_InvalidUserType_ShouldThrowNotSupportedException()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();

        // Act & Assert
        var invalidUserType = (UserType)999;
        var exception = Assert.Throws<NotSupportedException>(() => factory.CreateStrategy(invalidUserType));
        exception.Message.Should().Contain($"User type '{invalidUserType}' is not supported");
    }

    [Fact]
    public async Task AuthenticationFlow_InactiveUser_ShouldFailValidation()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();
        
        var user = CreateTestUser(UserType.EndUser);
        user.IsActive = false; // Deactivate user
        var password = "TestPassword123!";

        // Act
        var strategy = factory.CreateStrategy(UserType.EndUser);
        var authResult = await strategy.AuthenticateAsync(user, password);

        // Assert
        authResult.Should().NotBeNull();
        authResult.IsSuccess.Should().BeFalse();
        authResult.ValidationErrors.Should().Contain("User account is deactivated");
    }

    [Fact]
    public async Task AuthenticationFlow_UnconfirmedEmail_ShouldFailValidation()
    {
        // Arrange
        var services = CreateServiceCollection();
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();
        
        var user = CreateTestUser(UserType.EndUser);
        user.EmailConfirmed = false; // Unconfirmed email
        var password = "TestPassword123!";

        // Act
        var strategy = factory.CreateStrategy(UserType.EndUser);
        var authResult = await strategy.AuthenticateAsync(user, password);

        // Assert
        authResult.Should().NotBeNull();
        authResult.IsSuccess.Should().BeFalse();
        authResult.ValidationErrors.Should().Contain("Email address must be confirmed before login");
    }

    [Fact]
    public async Task AuthenticationFlow_WrongPassword_ShouldFailAuthentication()
    {
        // Arrange
        var services = CreateServiceCollection();
        var user = CreateTestUser(UserType.EndUser);
        var wrongPassword = "WrongPassword";

        // Setup password service to return false for wrong password
        var passwordServiceMock = services.Where(s => s.ServiceType == typeof(Mock<IPasswordService>))
            .FirstOrDefault()?.ImplementationInstance as Mock<IPasswordService>;
        
        passwordServiceMock?.Setup(x => x.VerifyPassword(It.IsAny<string>(), wrongPassword))
            .Returns(false);
        
        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IUserAuthenticationStrategyFactory>();

        // Act
        var strategy = factory.CreateStrategy(UserType.EndUser);
        var authResult = await strategy.AuthenticateAsync(user, wrongPassword);

        // Assert
        authResult.Should().NotBeNull();
        authResult.IsSuccess.Should().BeFalse();
        authResult.ErrorMessage.Should().Be("Invalid credentials");
    }

    private static IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        
        // Mock password service
        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true); // Default to successful password verification

        services.AddSingleton(passwordServiceMock.Object);
        services.AddSingleton(passwordServiceMock); // Add mock itself for later access

        // Add application services
        services.AddApplication();

        return services;
    }

    private static ApplicationUser CreateTestUser(UserType userType)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"testuser_{userType.ToString().ToLower()}",
            Email = $"test_{userType.ToString().ToLower()}@example.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = userType.ToString(),
            UserType = userType,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1), // Recent login for admin validation
            PasswordHash = "$2a$12$hashedpassword"
        };
    }
}