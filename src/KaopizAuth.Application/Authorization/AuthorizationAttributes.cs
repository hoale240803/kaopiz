using KaopizAuth.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace KaopizAuth.Application.Authorization;

/// <summary>
/// Authorization attribute that restricts access based on user type
/// </summary>
public class RequireUserTypeAttribute : AuthorizeAttribute
{
    public RequireUserTypeAttribute(UserType userType) : base($"RequireUserType_{userType}")
    {
    }

    public RequireUserTypeAttribute(params UserType[] userTypes) 
        : base($"RequireUserType_{string.Join("_", userTypes)}")
    {
    }
}

/// <summary>
/// Authorization attribute for admin-only access
/// </summary>
public class RequireAdminAttribute : RequireUserTypeAttribute
{
    public RequireAdminAttribute() : base(UserType.Admin)
    {
    }
}

/// <summary>
/// Authorization attribute for partner-only access
/// </summary>
public class RequirePartnerAttribute : RequireUserTypeAttribute
{
    public RequirePartnerAttribute() : base(UserType.Partner)
    {
    }
}

/// <summary>
/// Authorization attribute for end user access
/// </summary>
public class RequireEndUserAttribute : RequireUserTypeAttribute
{
    public RequireEndUserAttribute() : base(UserType.EndUser)
    {
    }
}

/// <summary>
/// Authorization attribute that requires specific permissions
/// </summary>
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base($"RequirePermission_{permission}")
    {
    }

    public RequirePermissionAttribute(params string[] permissions) 
        : base($"RequirePermission_{string.Join("_", permissions)}")
    {
    }
}

/// <summary>
/// Common permission constants
/// </summary>
public static class Permissions
{
    // Basic permissions
    public const string CanViewProfile = "CanViewProfile";
    public const string CanUpdateProfile = "CanUpdateProfile";
    public const string CanChangePassword = "CanChangePassword";

    // Admin permissions
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageRoles = "CanManageRoles";
    public const string CanViewAuditLogs = "CanViewAuditLogs";
    public const string CanManageSystem = "CanManageSystem";
    public const string CanAccessAdminPanel = "CanAccessAdminPanel";
    public const string CanDeleteUsers = "CanDeleteUsers";
    public const string CanModifySystemSettings = "CanModifySystemSettings";
    public const string CanViewSecurityReports = "CanViewSecurityReports";
    public const string CanManagePartners = "CanManagePartners";

    // Partner permissions
    public const string CanAccessPartnerPortal = "CanAccessPartnerPortal";
    public const string CanViewPartnerReports = "CanViewPartnerReports";
    public const string CanManagePartnerData = "CanManagePartnerData";
    public const string CanAccessApiEndpoints = "CanAccessApiEndpoints";
    public const string CanViewBusinessAnalytics = "CanViewBusinessAnalytics";
    public const string CanManagePartnerUsers = "CanManagePartnerUsers";
    public const string CanAccessIntegrationTools = "CanAccessIntegrationTools";
    public const string CanViewTransactionHistory = "CanViewTransactionHistory";
    public const string CanExportData = "CanExportData";
    public const string CanConfigureWebhooks = "CanConfigureWebhooks";
    public const string CanAccessSupportTickets = "CanAccessSupportTickets";
}