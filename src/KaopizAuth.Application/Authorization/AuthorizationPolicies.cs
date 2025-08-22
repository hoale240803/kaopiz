using KaopizAuth.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KaopizAuth.Application.Authorization;

/// <summary>
/// Authorization requirement for user type validation
/// </summary>
public class UserTypeRequirement : IAuthorizationRequirement
{
    public IEnumerable<UserType> AllowedUserTypes { get; }

    public UserTypeRequirement(params UserType[] allowedUserTypes)
    {
        AllowedUserTypes = allowedUserTypes;
    }
}

/// <summary>
/// Authorization requirement for permission validation
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public IEnumerable<string> RequiredPermissions { get; }

    public PermissionRequirement(params string[] requiredPermissions)
    {
        RequiredPermissions = requiredPermissions;
    }
}

/// <summary>
/// Handler for user type authorization requirements
/// </summary>
public class UserTypeAuthorizationHandler : AuthorizationHandler<UserTypeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserTypeRequirement requirement)
    {
        var userTypeClaim = context.User.FindFirst("UserType");
        if (userTypeClaim == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (Enum.TryParse<UserType>(userTypeClaim.Value, out var userType) &&
            requirement.AllowedUserTypes.Contains(userType))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for permission-based authorization requirements
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User.FindAll("Permission").Select(c => c.Value);

        if (requirement.RequiredPermissions.All(permission => userPermissions.Contains(permission)))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Static class for defining authorization policies
/// </summary>
public static class AuthorizationPolicies
{
    // User type policies
    public const string RequireEndUser = "RequireEndUser";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequirePartner = "RequirePartner";
    public const string RequireAdminOrPartner = "RequireAdminOrPartner";

    // Permission-based policies
    public const string RequireUserManagement = "RequireUserManagement";
    public const string RequireSystemManagement = "RequireSystemManagement";
    public const string RequirePartnerManagement = "RequirePartnerManagement";
    public const string RequireAuditAccess = "RequireAuditAccess";

    /// <summary>
    /// Configures all authorization policies
    /// </summary>
    /// <param name="options">Authorization options</param>
    public static void Configure(AuthorizationOptions options)
    {
        // User type policies
        options.AddPolicy(RequireEndUser, policy =>
            policy.Requirements.Add(new UserTypeRequirement(UserType.EndUser)));

        options.AddPolicy(RequireAdmin, policy =>
            policy.Requirements.Add(new UserTypeRequirement(UserType.Admin)));

        options.AddPolicy(RequirePartner, policy =>
            policy.Requirements.Add(new UserTypeRequirement(UserType.Partner)));

        options.AddPolicy(RequireAdminOrPartner, policy =>
            policy.Requirements.Add(new UserTypeRequirement(UserType.Admin, UserType.Partner)));

        // Permission-based policies
        options.AddPolicy(RequireUserManagement, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageUsers)));

        options.AddPolicy(RequireSystemManagement, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.CanManageSystem)));

        options.AddPolicy(RequirePartnerManagement, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.CanManagePartners)));

        options.AddPolicy(RequireAuditAccess, policy =>
            policy.Requirements.Add(new PermissionRequirement(Permissions.CanViewAuditLogs)));
    }
}