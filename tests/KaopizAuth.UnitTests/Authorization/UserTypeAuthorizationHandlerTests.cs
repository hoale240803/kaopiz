using FluentAssertions;
using KaopizAuth.Application.Authorization;
using KaopizAuth.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace KaopizAuth.UnitTests.Authorization;

public class UserTypeAuthorizationHandlerTests
{
    private readonly UserTypeAuthorizationHandler _handler;

    public UserTypeAuthorizationHandlerTests()
    {
        _handler = new UserTypeAuthorizationHandler();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithMatchingUserType_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", UserType.Admin.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithNonMatchingUserType_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", UserType.EndUser.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithoutUserTypeClaim_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithInvalidUserTypeClaim_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", "InvalidUserType")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleAllowedUserTypes_UserMatchesOne_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", UserType.Partner.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin, UserType.Partner);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleAllowedUserTypes_UserMatchesNone_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", UserType.EndUser.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin, UserType.Partner);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_AllUserTypes_ShouldHandleCorrectly()
    {
        // Arrange & Act & Assert for EndUser
        await VerifyUserTypeRequirement(UserType.EndUser, UserType.EndUser, shouldSucceed: true);
        await VerifyUserTypeRequirement(UserType.EndUser, UserType.Admin, shouldSucceed: false);
        await VerifyUserTypeRequirement(UserType.EndUser, UserType.Partner, shouldSucceed: false);

        // Arrange & Act & Assert for Admin
        await VerifyUserTypeRequirement(UserType.Admin, UserType.Admin, shouldSucceed: true);
        await VerifyUserTypeRequirement(UserType.Admin, UserType.EndUser, shouldSucceed: false);
        await VerifyUserTypeRequirement(UserType.Admin, UserType.Partner, shouldSucceed: false);

        // Arrange & Act & Assert for Partner
        await VerifyUserTypeRequirement(UserType.Partner, UserType.Partner, shouldSucceed: true);
        await VerifyUserTypeRequirement(UserType.Partner, UserType.EndUser, shouldSucceed: false);
        await VerifyUserTypeRequirement(UserType.Partner, UserType.Admin, shouldSucceed: false);
    }

    [Fact]
    public async Task HandleRequirementAsync_EmptyUserTypeClaim_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", string.Empty)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_NullUserTypeClaim_ShouldFail()
    {
        // Arrange - User without UserType claim at all
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(UserType.Admin);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    private async Task VerifyUserTypeRequirement(UserType userClaim, UserType requiredType, bool shouldSucceed)
    {
        // Arrange
        var claims = new[]
        {
            new Claim("UserType", userClaim.ToString())
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new UserTypeRequirement(requiredType);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        if (shouldSucceed)
        {
            context.HasSucceeded.Should().BeTrue($"User with {userClaim} should have access to {requiredType} requirement");
            context.HasFailed.Should().BeFalse();
        }
        else
        {
            context.HasSucceeded.Should().BeFalse($"User with {userClaim} should not have access to {requiredType} requirement");
            context.HasFailed.Should().BeTrue();
        }
    }
}