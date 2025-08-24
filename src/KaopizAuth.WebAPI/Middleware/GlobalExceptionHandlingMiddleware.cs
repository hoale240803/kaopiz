using System.Net;
using System.Text.Json;
using KaopizAuth.Application.Common.Models;

namespace KaopizAuth.WebAPI.Middleware;

/// <summary>
/// Global exception handling middleware to provide standardized error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        var response = CreateErrorResponse(exception, correlationId);
        var statusCode = GetStatusCode(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private ApiResponse<object> CreateErrorResponse(Exception exception, string correlationId)
    {
        var message = GetErrorMessage(exception);
        var errors = new Dictionary<string, string[]>
        {
            ["Exception"] = new[] { _environment.IsDevelopment() ? exception.Message : "An error occurred while processing your request." },
            ["CorrelationId"] = new[] { correlationId }
        };

        // Add stack trace in development mode
        if (_environment.IsDevelopment())
        {
            errors["StackTrace"] = new[] { exception.StackTrace ?? "No stack trace available" };
        }

        return ApiResponse<object>.FailureResult(message, errors);
    }

    private string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Required argument is missing.",
            ArgumentException => "Invalid argument provided.",
            UnauthorizedAccessException => "Access denied.",
            InvalidOperationException => "Invalid operation.",
            NotImplementedException => "Feature not implemented.",
            TimeoutException => "The operation has timed out.",
            _ => "An unexpected error occurred."
        };
    }

    private HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => HttpStatusCode.BadRequest,
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.BadRequest,
            NotImplementedException => HttpStatusCode.NotImplemented,
            TimeoutException => HttpStatusCode.RequestTimeout,
            _ => HttpStatusCode.InternalServerError
        };
    }
}