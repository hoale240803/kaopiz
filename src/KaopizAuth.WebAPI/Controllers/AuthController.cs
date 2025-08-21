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
            RememberMe = request.RememberMe
        };

        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
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