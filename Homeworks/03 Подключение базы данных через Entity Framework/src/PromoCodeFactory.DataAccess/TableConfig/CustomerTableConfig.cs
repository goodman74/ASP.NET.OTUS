using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.TableConfig
{
    internal class CustomerTableConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Ignore(x => x.FullName);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasMany<Preference>(x => x.Preferences)
                .WithMany(p => p.Customers)
                .UsingEntity(j => j.ToTable("JoinCustomerPreference"));
        }
    }
}
