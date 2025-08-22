using FluentAssertions;
using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Application.Services.Authentication;
using KaopizAuth.Domain.Entities;
using KaopizAuth.Domain.Enums;
using Moq;
using System.Security.Claims;
using Xunit;

namespace KaopizAuth.UnitTests.Services.Authentication;

public class AdminAuthStrategyTests
{
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly AdminAuthStrategy _strategy;

    public AdminAuthStrategyTests()
    {
        _passwordServiceMock = new Mock<IPasswordService>();
        _strategy = new AdminAuthStrategy(_passwordServiceMock.Object);
    }

    [Fact]
    public void Constructor_NullPasswordService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdminAuthStrategy(null!));
    }

    [Fact]
    public async Task AuthenticateAsync_ValidActiveAdminWithCorrectPassword_ShouldReturnSuccessWithTwoFactor()
    {
        // Arrange
        var user = CreateValidAdminUser();
        var password = "AdminPassword123!";
        
        _passwordServiceMock.Setup(x => x.VerifyPassword(user.PasswordHash!, password))
            .Returns(true);

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.RequiresTwoFactor.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_ValidAdminWithIncorrectPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        var password = "WrongPassword";
        
        _passwordServiceMock.Setup(x => x.VerifyPassword(user.PasswordHash!, password))
            .Returns(false);

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveAdminUser_ShouldReturnValidationFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.IsActive = false;
        var password = "AdminPassword123!";

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Administrator account is deactivated");
    }

    [Fact]
    public async Task AuthenticateAsync_UnconfirmedEmailAdmin_ShouldReturnValidationFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.EmailConfirmed = false;
        var password = "AdminPassword123!";

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Administrator email address must be confirmed");
    }

    [Fact]
    public async Task AuthenticateAsync_AdminWithLongIdleTime_ShouldReturnValidationFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.LastLoginAt = DateTime.UtcNow.AddDays(-100); // More than 90 days idle
        var password = "AdminPassword123!";

        // Act
        var result = await _strategy.AuthenticateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Administrator account has been idle for too long. Please contact system administrator.");
    }

    [Fact]
    public async Task GetUserClaimsAsync_ValidAdminUser_ShouldReturnCorrectClaims()
    {
        // Arrange
        var user = CreateValidAdminUser();

        // Act
        var claims = await _strategy.GetUserClaimsAsync(user);

        // Assert
        claims.Should().NotBeNull();
        var claimsList = claims.ToList();
        
        // Check basic identity claims
        claimsList.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.FirstName);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Surname && c.Value == user.LastName);
        claimsList.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Administrator");
        claimsList.Should().Contain(c => c.Type == "UserType" && c.Value == UserType.Admin.ToString());
        claimsList.Should().Contain(c => c.Type == "FullName" && c.Value == user.FullName);
        claimsList.Should().Contain(c => c.Type == "SecurityLevel" && c.Value == "High");

        // Check admin permissions
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewProfile");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanUpdateProfile");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanChangePassword");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManageUsers");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManageRoles");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewAuditLogs");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManageSystem");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanAccessAdminPanel");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanDeleteUsers");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanModifySystemSettings");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanViewSecurityReports");
        claimsList.Should().Contain(c => c.Type == "Permission" && c.Value == "CanManagePartners");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_ValidActiveAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateValidAdminUser();

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_InactiveAdmin_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.IsActive = false;

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Administrator account is deactivated");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_UnconfirmedEmailAdmin_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.EmailConfirmed = false;

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Administrator email address must be confirmed");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_AdminWithLongIdleTime_ShouldReturnFailure()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.LastLoginAt = DateTime.UtcNow.AddDays(-100); // More than 90 days

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Administrator account has been idle for too long. Please contact system administrator.");
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_RecentLoginAdmin_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.LastLoginAt = DateTime.UtcNow.AddDays(-30); // Within 90 days

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateUserTypeRulesAsync_InactiveAdminWithUnconfirmedEmailAndLongIdleTime_ShouldReturnMultipleErrors()
    {
        // Arrange
        var user = CreateValidAdminUser();
        user.IsActive = false;
        user.EmailConfirmed = false;
        user.LastLoginAt = DateTime.UtcNow.AddDays(-100);

        // Act
        var result = await _strategy.ValidateUserTypeRulesAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain("Administrator account is deactivated");
        result.Errors.Should().Contain("Administrator email address must be confirmed");
        result.Errors.Should().Contain("Administrator account has been idle for too long. Please contact system administrator.");
    }

    private ApplicationUser CreateValidAdminUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User",
            UserType = UserType.Admin,
            IsActive = true,
            LastLoginAt = DateTime.UtcNow.AddDays(-1), // Recent login
            PasswordHash = "$2a$12$hashedadminpassword"
        };
    }
}