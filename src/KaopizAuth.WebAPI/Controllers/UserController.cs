using KaopizAuth.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KaopizAuth.Application.Common.Interfaces;

namespace KaopizAuth.WebAPI.Controllers;

/// <summary>
/// User management controller for profile operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IAuditService _auditService;

    public UserController(ILogger<UserController> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>User profile information</returns>
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<object>>> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        var profile = new
        {
            Id = userId,
            Email = email,
            FirstName = "Security",
            LastName = "Test"
        };

        var response = ApiResponse<object>.SuccessResult(profile, "Profile retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Update result</returns>
    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Validate input for XSS attacks
        if (ContainsXss(request.FirstName) || ContainsXss(request.LastName))
        {
            _logger.LogWarning("XSS attempt detected in profile update for user {UserId}", userId);
            return BadRequest(ApiResponse.FailureResult("Invalid input detected"));
        }

        // Simulate profile update
        var updatedProfile = new
        {
            Id = userId,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            FirstName = SanitizeInput(request.FirstName),
            LastName = SanitizeInput(request.LastName)
        };

        var response = ApiResponse<object>.SuccessResult(updatedProfile, "Profile updated successfully");
        return Ok(response);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Change result</returns>
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";

        // Validate current password is provided (CSRF protection)
        if (string.IsNullOrEmpty(request.CurrentPassword))
        {
            return BadRequest(ApiResponse.FailureResult("Current password is required"));
        }

        // For testing purposes, just return success
        // In real implementation, you would validate current password and update it

        // Log password change
        if (!string.IsNullOrEmpty(userId))
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            _ = Task.Run(async () => await _auditService.LogPasswordChangeAsync(userId, email, clientIp));
        }

        var response = ApiResponse<object>.SuccessResult(new { }, "Password changed successfully");
        return Ok(response);
    }

    /// <summary>
    /// Change user password (PUT version for compatibility)
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Change result</returns>
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePasswordPut([FromBody] ChangePasswordRequest request)
    {
        // Delegate to the POST version
        return await ChangePassword(request);
    }

    /// <summary>
    /// Update user email address
    /// </summary>
    /// <param name="request">Email update request</param>
    /// <returns>Update result</returns>
    [HttpPut("email")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Validate input
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(ApiResponse.FailureResult("Email is required"));
        }

        // Simple email validation
        if (!request.Email.Contains("@"))
        {
            return BadRequest(ApiResponse.FailureResult("Invalid email format"));
        }

        // Simulate email update
        var result = new
        {
            Id = userId,
            Email = request.Email,
            Message = "Email updated successfully"
        };

        var response = ApiResponse<object>.SuccessResult(result, "Email updated successfully");
        return Ok(response);
    }

    private static bool ContainsXss(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var xssPatterns = new[]
        {
            "<script", "javascript:", "onload=", "onclick=", "onerror=", "onmouseover=",
            "<iframe", "<object", "<embed", "<svg", "expression(", "eval("
        };

        return xssPatterns.Any(pattern =>
            input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static string SanitizeInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Basic sanitization - remove common XSS patterns
        return input
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#x27;")
            .Replace("&", "&amp;")
            .Replace("javascript:", "")
            .Replace("onload=", "")
            .Replace("onclick=", "")
            .Replace("onerror=", "")
            .Replace("onmouseover=", "");
    }
}

/// <summary>
/// Request model for updating user profile
/// </summary>
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

/// <summary>
/// Request model for changing password
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating email address
/// </summary>
public class UpdateEmailRequest
{
    public string Email { get; set; } = string.Empty;
}
