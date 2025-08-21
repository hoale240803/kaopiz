using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KaopizAuth.WebAPI.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// User login endpoint
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = GetClientIpAddress()
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get client IP address from request
    /// </summary>
    /// <returns>Client IP address</returns>
    private string GetClientIpAddress()
    {
        // Check for forwarded IP first (useful for reverse proxies)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for real IP
        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Health check for authentication service
    /// </summary>
    /// <returns>Authentication service status</returns>
    [HttpGet("health")]
    public ActionResult<object> Health()
    {
        return Ok(new { Service = "Authentication", Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}