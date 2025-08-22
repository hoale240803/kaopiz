using FluentAssertions;
using KaopizAuth.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Xunit;

namespace KaopizAuth.UnitTests.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
    {
        _handler = new PermissionAuthorizationHandler();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithRequiredPermission_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanManageUsers)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithoutRequiredPermission_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithoutPermissionClaims_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserWithMultiplePermissions_RequiresOne_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile),
            new Claim("Permission", Permissions.CanManageUsers),
            new Claim("Permission", Permissions.CanUpdateProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleRequiredPermissions_UserHasAll_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanManageUsers),
            new Claim("Permission", Permissions.CanViewAuditLogs),
            new Claim("Permission", Permissions.CanManageSystem)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(
            Permissions.CanManageUsers, 
            Permissions.CanViewAuditLogs);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleRequiredPermissions_UserHasSome_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanManageUsers),
            new Claim("Permission", Permissions.CanViewProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(
            Permissions.CanManageUsers, 
            Permissions.CanViewAuditLogs);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_MultipleRequiredPermissions_UserHasNone_ShouldFail()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile),
            new Claim("Permission", Permissions.CanUpdateProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(
            Permissions.CanManageUsers, 
            Permissions.CanViewAuditLogs);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_EmptyRequiredPermissions_ShouldSucceed()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(); // No required permissions
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue(); // No permissions required, so it should succeed
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_CaseSensitivePermissionCheck()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", "canmanageusers") // lowercase
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers); // proper case
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse(); // Should be case sensitive
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_DuplicatePermissions_ShouldWork()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanManageUsers),
            new Claim("Permission", Permissions.CanManageUsers), // Duplicate
            new Claim("Permission", Permissions.CanViewProfile)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(Permissions.CanManageUsers);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ComplexScenario_AdminPermissions_ShouldSucceed()
    {
        // Arrange - Simulate admin user with multiple permissions
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile),
            new Claim("Permission", Permissions.CanUpdateProfile),
            new Claim("Permission", Permissions.CanChangePassword),
            new Claim("Permission", Permissions.CanManageUsers),
            new Claim("Permission", Permissions.CanManageRoles),
            new Claim("Permission", Permissions.CanViewAuditLogs),
            new Claim("Permission", Permissions.CanManageSystem),
            new Claim("Permission", Permissions.CanAccessAdminPanel)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(
            Permissions.CanManageUsers, 
            Permissions.CanViewAuditLogs,
            Permissions.CanManageSystem);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        context.HasFailed.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ComplexScenario_PartnerPermissions_ShouldFail()
    {
        // Arrange - Simulate partner user trying to access admin functions
        var claims = new[]
        {
            new Claim("Permission", Permissions.CanViewProfile),
            new Claim("Permission", Permissions.CanUpdateProfile),
            new Claim("Permission", Permissions.CanAccessPartnerPortal),
            new Claim("Permission", Permissions.CanViewPartnerReports),
            new Claim("Permission", Permissions.CanManagePartnerData)
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var requirement = new PermissionRequirement(
            Permissions.CanManageUsers, 
            Permissions.CanViewAuditLogs);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }
}