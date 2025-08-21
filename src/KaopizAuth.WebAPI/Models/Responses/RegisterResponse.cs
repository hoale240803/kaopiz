namespace KaopizAuth.WebAPI.Models.Responses;

/// <summary>
/// Response model for user registration
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// ID of the newly created user
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Success message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
