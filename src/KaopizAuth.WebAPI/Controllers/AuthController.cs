using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Models;
using KaopizAuth.WebAPI.Models.Requests;
using KaopizAuth.WebAPI.Models.Responses;
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
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var command = new RegisterCommand
            {
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = request.UserType
            };

            var result = await _mediator.Send(command);

            if (result.Success)
            {
                var response = new RegisterResponse
                {
                    UserId = result.UserId,
                    Message = result.Message
                };

                return CreatedAtAction(nameof(Register),
                    ApiResponse<RegisterResponse>.SuccessResult(response, "User registered successfully"));
            }

            return BadRequest(ApiResponse.FailureResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");

            return StatusCode(500, ApiResponse.FailureResult("An unexpected error occurred"));
        }
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
    /// Refresh token endpoint
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = clientIp
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Logout endpoint
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] LogoutRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = clientIp,
            RevokeAllTokens = request.RevokeAllTokens
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