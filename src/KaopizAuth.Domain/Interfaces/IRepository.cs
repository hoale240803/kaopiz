using System.Linq.Expressions;

namespace KaopizAuth.Domain.Interfaces;

/// <summary>
/// Generic repository interface for CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<T, TKey> where T : class where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">Entity identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate
    /// </summary>
    /// <param name="predicate">Search predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the predicate or null
    /// </summary>
    /// <param name="predicate">Search predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>First matching entity or null</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entity</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Removes an entity
    /// </summary>
    /// <param name="entity">Entity to remove</param>
    void Remove(T entity);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">Search predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the predicate
    /// </summary>
    /// <param name="predicate">Search predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
