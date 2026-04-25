using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.TableConfig
{
    internal class CustomerPromoCodeTableConfig : IEntityTypeConfiguration<CustomerPromoCode>
    {
        public void Configure(EntityTypeBuilder<CustomerPromoCode> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerId)
                .IsRequired();

            entity.Property(x => x.PromoCodeId)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.Property(x => x.AppliedAt);

            entity.HasOne<Customer>()
                .WithMany(x => x.CustomerPromoCodes)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<PromoCode>()
                .WithMany(x => x.CustomerPromoCodes)
                .HasForeignKey(x => x.PromoCodeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.CustomerId, x.PromoCodeId })
                .IsUnique();
        }
    }
}
