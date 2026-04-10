using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Linq.Expressions;

namespace PromoCodeFactory.Core.Abstractions.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<IReadOnlyCollection<T>> GetAll(bool withIncludes = false, CancellationToken ct = default);

    /// <returns>The entity tracked by the current DbContext if found; otherwise, <see langword="null"/>.</returns>
    Task<T?> GetById(Guid id, bool withIncludes = false, CancellationToken ct = default);

    /// <returns>
    /// Collection of entities matching the specified ids. 
    /// Entities are tracked by the current DbContext.
    /// </returns>
    Task<IReadOnlyCollection<T>> GetByRangeId(IEnumerable<Guid> ids, bool withIncludes = false,
        CancellationToken ct = default);

    /// <returns>
    /// Collection of entities matching the specified predicate. 
    /// Entities are tracked by the current DbContext.
    /// </returns>
    Task<IReadOnlyCollection<T>> GetWhere(Expression<Func<T, bool>> predicate, bool withIncludes = false,
        CancellationToken ct = default);

    /// <summary>
    /// Adds a new entity to the current DbContext. Use IUnitOfWork to SaveChangesAsync
    /// </summary>
    /// <param name="entity">
    /// A new entity with tracked references to existing related entities.
    /// </param>
    void Add(T entity);

    /// <summary>
    /// Updates a detached entity by applying its values to the existing entity in the DbContext and saving changes.
    /// </summary>
    /// <exception cref="EntityNotFoundException">Thrown if the entity with the given Id is not found in the database.</exception>
    Task UpdateDetached(T entity, CancellationToken ct);

    /// <exception cref="EntityNotFoundException"/>
    Task Delete(Guid id, CancellationToken ct);

    /// <summary>
    /// Adds new entities to the current DbContext. Use IUnitOfWork to SaveChangesAsync.
    /// </summary>
    /// <param name="entities">
    /// New entities with tracked references to existing related entities.
    /// </param>
    void AddRange(IEnumerable<T> entities);

    Task<bool> IsNotEmptyAsync(CancellationToken ct);
}
