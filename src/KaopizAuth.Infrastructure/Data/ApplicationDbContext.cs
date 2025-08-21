using KaopizAuth.Application.Common.Interfaces;
using KaopizAuth.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public new DbSet<ApplicationRole> Roles => Set<ApplicationRole>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Configure Identity table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            if (entityEntry.Entity is ApplicationUser user)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    user.CreatedAt = DateTime.UtcNow;
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}