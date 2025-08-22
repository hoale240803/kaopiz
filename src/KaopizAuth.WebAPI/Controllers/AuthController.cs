using KaopizAuth.Application.Commands.Auth;
using KaopizAuth.Application.Common.Models;
using KaopizAuth.WebAPI.Models.Requests;
using KaopizAuth.WebAPI.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace KaopizAuth.WebAPI.Controllers;

/// <summary>
/// Authentication controller for user registration, login, logout, and token refresh operations
/// </summary>
/// <remarks>
/// This controller handles all authentication-related operations including:
/// - User registration with email verification
/// - User login with JWT token generation
/// - Token refresh for maintaining authentication
/// - User logout with token revocation
/// 
/// All endpoints return consistent API responses with success/failure status and error details.
/// Rate limiting is applied to prevent abuse, especially on login endpoints.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[SwaggerTag("Authentication operations for user management")]
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
    /// Register a new user account
    /// </summary>
    /// <param name="request">User registration details including email, password, and personal information</param>
    /// <returns>Registration result with user ID and confirmation message</returns>
    /// <remarks>
    /// Creates a new user account with the provided information. 
    /// 
    /// **Requirements:**
    /// - Valid email address (will be used as username)
    /// - Password meeting security requirements (8+ chars, uppercase, lowercase, digit)
    /// - Matching password confirmation
    /// - First and last name
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "email": "john.doe@example.com",
    ///   "password": "SecurePass123!",
    ///   "confirmPassword": "SecurePass123!",
    ///   "firstName": "John",
    ///   "lastName": "Doe",
    ///   "userType": "User"
    /// }
    /// ```
    /// 
    /// **Success Response Example:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "User registered successfully",
    ///   "data": {
    ///     "userId": "123e4567-e89b-12d3-a456-426614174000",
    ///     "message": "Registration completed. Please check your email for verification."
    ///   },
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid request data or validation errors</response>
    /// <response code="409">User already exists with this email</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register new user",
        Description = "Creates a new user account with email verification",
        OperationId = "RegisterUser",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse((int)HttpStatusCode.Created, "User successfully registered", typeof(ApiResponse<RegisterResponse>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request data", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.Conflict, "User already exists", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error", typeof(ApiResponse))]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
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
    /// Authenticate user and generate access tokens
    /// </summary>
    /// <param name="request">Login credentials including email and password</param>
    /// <returns>Authentication result with JWT access token, refresh token, and user information</returns>
    /// <remarks>
    /// Authenticates a user with email and password, returning JWT tokens for API access.
    /// 
    /// **Rate Limiting:** 5 attempts per minute per IP address to prevent brute force attacks.
    /// 
    /// **Security Features:**
    /// - Account lockout after 5 failed attempts
    /// - IP address tracking for audit logs
    /// - Secure refresh token generation
    /// - Short-lived access tokens (15 minutes)
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "email": "john.doe@example.com",
    ///   "password": "SecurePass123!",
    ///   "rememberMe": false
    /// }
    /// ```
    /// 
    /// **Success Response Example:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Login successful",
    ///   "data": {
    ///     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///     "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7...",
    ///     "expiresAt": "2025-08-21T12:15:00Z",
    ///     "user": {
    ///       "id": "123e4567-e89b-12d3-a456-426614174000",
    ///       "email": "john.doe@example.com",
    ///       "firstName": "John",
    ///       "lastName": "Doe",
    ///       "fullName": "John Doe",
    ///       "roles": ["User"]
    ///     }
    ///   },
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Login successful with tokens</response>
    /// <response code="400">Invalid credentials or account locked</response>
    /// <response code="429">Too many login attempts</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "User authentication",
        Description = "Authenticate user credentials and return JWT tokens",
        OperationId = "LoginUser",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Login successful", typeof(ApiResponse<LoginResponse>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid credentials", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.TooManyRequests, "Rate limit exceeded", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error", typeof(ApiResponse))]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    /// Refresh expired access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request containing the refresh token</param>
    /// <returns>New access token and refresh token pair</returns>
    /// <remarks>
    /// Exchanges a valid refresh token for a new access token and refresh token.
    /// This endpoint should be used when the access token expires to maintain user session.
    /// 
    /// **Token Rotation:** Each refresh generates a new refresh token for enhanced security.
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"
    /// }
    /// ```
    /// 
    /// **Success Response Example:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Token refreshed successfully",
    ///   "data": {
    ///     "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///     "refreshToken": "c9d0e1f2g3h4i5j6k7l8m9n0o1p2q3r4...",
    ///     "expiresAt": "2025-08-21T12:30:00Z"
    ///   },
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid or expired refresh token</response>
    /// <response code="401">Refresh token not found or revoked</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Generate new access token using refresh token",
        OperationId = "RefreshToken",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Token refreshed successfully", typeof(ApiResponse<RefreshTokenResponse>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid refresh token", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Refresh token expired or revoked", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error", typeof(ApiResponse))]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    /// Logout user and revoke refresh tokens
    /// </summary>
    /// <param name="request">Logout request containing refresh token and optional settings</param>
    /// <returns>Logout confirmation</returns>
    /// <remarks>
    /// Logs out the user by revoking the specified refresh token. 
    /// Optionally can revoke all refresh tokens for the user across all devices.
    /// 
    /// **Security:** Always revoke refresh tokens on logout to prevent unauthorized access.
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
    ///   "revokeAllTokens": false
    /// }
    /// ```
    /// 
    /// **Success Response Example:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Logout successful",
    ///   "data": true,
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid refresh token</response>
    /// <response code="401">Authentication required</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [SwaggerOperation(
        Summary = "User logout",
        Description = "Logout user and revoke refresh tokens",
        OperationId = "LogoutUser",
        Tags = new[] { "Authentication" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Logout successful", typeof(ApiResponse<bool>))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid request", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Authentication required", typeof(ApiResponse))]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, "Internal server error", typeof(ApiResponse))]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
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
    /// Health check endpoint for authentication service
    /// </summary>
    /// <returns>Service health status and basic information</returns>
    /// <remarks>
    /// Returns the health status of the authentication service including timestamp.
    /// This endpoint can be used for monitoring and load balancer health checks.
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "service": "Authentication",
    ///   "status": "Healthy",
    ///   "timestamp": "2025-08-21T10:00:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Service is healthy</response>
    [HttpGet("health")]
    [SwaggerOperation(
        Summary = "Authentication service health check",
        Description = "Check the health status of the authentication service",
        OperationId = "GetAuthHealth",
        Tags = new[] { "Health" }
    )]
    [SwaggerResponse((int)HttpStatusCode.OK, "Service is healthy", typeof(object))]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult<object> Health()
    {
        return Ok(new { Service = "Authentication", Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}