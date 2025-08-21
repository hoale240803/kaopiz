using Microsoft.AspNetCore.Identity;

namespace KaopizAuth.Domain.Entities;

/// <summary>
/// Application role entity extending ASP.NET Core Identity
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}