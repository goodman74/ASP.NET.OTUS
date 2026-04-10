namespace PromoCodeFactory.Core.Abstractions.Repositories;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists changes for all modified entities currently tracked by the DbContext.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct);
}
