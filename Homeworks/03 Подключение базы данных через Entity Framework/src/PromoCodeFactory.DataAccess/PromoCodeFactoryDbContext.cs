using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.TableConfig;

namespace PromoCodeFactory.DataAccess;

public class PromoCodeFactoryDbContext : DbContext
{
    public PromoCodeFactoryDbContext(DbContextOptions<PromoCodeFactoryDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerPromoCode> CustomerPromoCodes => Set<CustomerPromoCode>();

    public DbSet<Preference> Preferences => Set<Preference>();

    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(EmployeeTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(RoleTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(CustomerPromoCodeTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(CustomerTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(EmployeeTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(PreferenceTableConfig).Assembly);

        builder.ApplyConfigurationsFromAssembly(typeof(PromoCodeTableConfig).Assembly);
    }
}
