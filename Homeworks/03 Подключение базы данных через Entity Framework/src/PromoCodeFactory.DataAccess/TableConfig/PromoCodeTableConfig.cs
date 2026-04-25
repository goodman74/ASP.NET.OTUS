using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.TableConfig
{
    internal class PromoCodeTableConfig : IEntityTypeConfiguration<PromoCode>
    {
        public void Configure(EntityTypeBuilder<PromoCode> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.ServiceInfo)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.BeginDate)
                .IsRequired();

            entity.Property(x => x.EndDate)
                .IsRequired();

            entity.Property(x => x.PartnerName)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasOne<Employee>(x => x.PartnerManager)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasOne<Preference>(x => x.Preference)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
}
