using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain;

namespace PromoCodeFactory.DataAccess;

public static class HostExtensions
{
    public static IHost MigrateDatabase(this IHost host) => host.MigrateDatabase<PromoCodeFactoryDbContext>();

    public static async Task SeedDatabase(this IHost host, CancellationToken ct = default)
    {
        using var scope = host.Services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("SeedDatabase");
        logger.LogInformation("Starting database seed...");
        var unitOfWork = sp.GetRequiredService<IUnitOfWork>();

        await SeedEntity(sp, SeedData.Roles, unitOfWork, ct);
        await SeedEntity(sp, SeedData.Preferences, unitOfWork, ct);
        await SeedEntity(sp, SeedData.Employees, unitOfWork, ct);
        await SeedEntity(sp, SeedData.Customers, unitOfWork, ct);
        await SeedEntity(sp, SeedData.PromoCodes, unitOfWork, ct);
        await SeedEntity(sp, SeedData.CustomerPromoCodes, unitOfWork, ct);

        logger.LogInformation("Database seed completed.");
    }

    private static IHost MigrateDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
    {
        using var scope = host.Services.CreateScope();
        using var appContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("MigrateDatabase");

        var connectionString = appContext.Database.GetConnectionString();
        logger.LogInformation($"Use connectionString: '{connectionString}'");

        var pendingMigrations = appContext.Database.GetPendingMigrations().ToArray();
        var message = pendingMigrations.Length > 0
            ? $"There are pending migrations '{string.Join(',', pendingMigrations)}'"
            : "No pending migrations";

        logger.LogInformation(message);

        appContext.Database.Migrate();
        return host;
    }

    public static async Task SeedEntity<T>(IServiceProvider serviceProvider, IReadOnlyCollection<T> entities,
        IUnitOfWork unitOfWork, CancellationToken ct) where T : BaseEntity
    {
        var repository = serviceProvider.GetRequiredService<IRepository<T>>();

        var ids = entities.Select(x => x.Id);
        var existing = await repository.GetByRangeId(ids, ct: ct);

        var existingIds = existing.Select(x => x.Id).ToHashSet();

        foreach (var entity in entities)
        {
            if (!existingIds.Contains(entity.Id))
                repository.Add(entity);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
