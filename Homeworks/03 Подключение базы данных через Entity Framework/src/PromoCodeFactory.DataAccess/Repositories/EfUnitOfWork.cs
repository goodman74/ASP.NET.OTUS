using PromoCodeFactory.Core.Abstractions.Repositories;

namespace PromoCodeFactory.DataAccess.Repositories;

public sealed class EfUnitOfWork(PromoCodeFactoryDbContext _dbContext) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}
