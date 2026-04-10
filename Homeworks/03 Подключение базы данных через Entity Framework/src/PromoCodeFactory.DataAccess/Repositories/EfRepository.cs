using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;
using PromoCodeFactory.Core.Exceptions;
using System.Linq.Expressions;

namespace PromoCodeFactory.DataAccess.Repositories;

internal class EfRepository<T>(PromoCodeFactoryDbContext context) : IRepository<T> where T : BaseEntity
{
    protected virtual IQueryable<T> ApplyIncludes(IQueryable<T> query) => query;

    public void Add(T entity) => context.Set<T>().Add(entity);

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var rowDeleted = await context.Set<T>()
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ct);

        if (rowDeleted == 0)
            throw new EntityNotFoundException(typeof(T), id);
    }

    public async Task<IReadOnlyCollection<T>> GetAll(bool withIncludes = false, CancellationToken ct = default)
    {
        IQueryable<T> query = context.Set<T>().AsNoTracking();

        return await (withIncludes ? ApplyIncludes(query) : query).ToListAsync(ct);
    }

    public async Task<T?> GetById(Guid id, bool withIncludes = false, CancellationToken ct = default)
    {
        IQueryable<T> query = context.Set<T>()
            .Where(x => x.Id == id);

        return await (withIncludes ? ApplyIncludes(query) : query).SingleOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<T>> GetByRangeId(IEnumerable<Guid> ids, bool withIncludes = false,
        CancellationToken ct = default)
    {
        Guid[] idArray = ids.Distinct().ToArray();

        IQueryable<T> query = context.Set<T>()
            .Where(e => idArray.Contains(e.Id));

        return await (withIncludes ? ApplyIncludes(query) : query).ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<T>> GetWhere(Expression<Func<T, bool>> predicate, bool withIncludes = false,
        CancellationToken ct = default)
    {
        IQueryable<T> query = context.Set<T>()
            .Where(predicate);
        return await (withIncludes ? ApplyIncludes(query) : query).ToListAsync(ct);
    }

    public async Task UpdateDetached(T entity, CancellationToken ct)
    {
        var row = await context.Set<T>()
            .Where(x => x.Id == entity.Id)
            .SingleOrDefaultAsync(ct);

        if (row == null)
            throw new EntityNotFoundException(typeof(T), entity.Id);

        context.Entry(row).CurrentValues.SetValues(entity);
    }

    public void AddRange(IEnumerable<T> entities) => context.Set<T>().AddRange(entities);

    public async Task<bool> IsNotEmptyAsync(CancellationToken ct) => await context.Set<T>().AnyAsync(ct);
}
