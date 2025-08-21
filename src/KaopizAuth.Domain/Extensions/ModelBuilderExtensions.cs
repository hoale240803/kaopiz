using System.Linq.Expressions;
using System.Reflection;
using KaopizAuth.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace KaopizAuth.Domain.Extensions;

/// <summary>
/// Model builder extensions for soft delete and audit configuration
/// </summary>
public static class ModelBuilderExtensions
{
    private static readonly MethodInfo SetSoftDeleteFilterMethod = 
        typeof(ModelBuilderExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == nameof(SetSoftDeleteFilter));

    /// <summary>
    /// Applies soft delete filter to all entities implementing ISoftDeletableEntity
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void SetSoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                SetSoftDeleteFilter(modelBuilder, entityType.ClrType);
            }
        }
    }

    /// <summary>
    /// Applies soft delete filter to specific entity type
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="entityType">The entity type</param>
    public static void SetSoftDeleteFilter(ModelBuilder modelBuilder, Type entityType)
    {
        SetSoftDeleteFilterMethod.MakeGenericMethod(entityType)
            .Invoke(null, new object[] { modelBuilder });
    }

    /// <summary>
    /// Applies soft delete filter to specific entity type
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="modelBuilder">The model builder</param>
    public static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) 
        where TEntity : class, ISoftDeletableEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.DeletedTime == null);
    }

    /// <summary>
    /// Configures audit properties for all entities implementing IAuditableEntity
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void ConfigureAuditProperties(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                ConfigureAuditProperties(modelBuilder, entityType.ClrType);
            }
        }
    }

    /// <summary>
    /// Configures audit properties for specific entity type
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    /// <param name="entityType">The entity type</param>
    public static void ConfigureAuditProperties(ModelBuilder modelBuilder, Type entityType)
    {
        var entity = modelBuilder.Entity(entityType);
        
        // Configure audit fields
        entity.Property("CreatedTime").IsRequired();
        entity.Property("LastUpdatedTime").IsRequired();
        entity.Property("CreatedBy").HasMaxLength(450); // Same as Identity user ID
        entity.Property("LastUpdatedBy").HasMaxLength(450);
        
        // Configure soft delete fields if applicable
        if (typeof(ISoftDeletableEntity).IsAssignableFrom(entityType))
        {
            entity.Property("DeletedTime");
            entity.Property("DeletedBy").HasMaxLength(450);
        }
    }

    /// <summary>
    /// Configures all audit and soft delete features
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void ConfigureAuditingAndSoftDelete(this ModelBuilder modelBuilder)
    {
        modelBuilder.SetSoftDeleteFilter();
        modelBuilder.ConfigureAuditProperties();
    }

    /// <summary>
    /// Adds indexes for audit and soft delete queries
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    public static void AddAuditIndexes(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var entity = modelBuilder.Entity(entityType.ClrType);
                
                // Index for CreatedTime queries
                entity.HasIndex("CreatedTime").HasDatabaseName($"IX_{entityType.GetTableName()}_CreatedTime");
                
                // Index for LastUpdatedTime queries  
                entity.HasIndex("LastUpdatedTime").HasDatabaseName($"IX_{entityType.GetTableName()}_LastUpdatedTime");
                
                if (typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Index for soft delete queries
                    entity.HasIndex("DeletedTime").HasDatabaseName($"IX_{entityType.GetTableName()}_DeletedTime");
                    
                    // Composite index for active records
                    entity.HasIndex("DeletedTime", "CreatedTime")
                        .HasDatabaseName($"IX_{entityType.GetTableName()}_Active_CreatedTime")
                        .HasFilter("DeletedTime IS NULL");
                }
            }
        }
    }
}
