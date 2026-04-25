using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;

namespace PromoCodeFactory.DataAccess.TableConfig
{
    internal class PreferenceTableConfig : IEntityTypeConfiguration<Preference>
    {
        public void Configure(EntityTypeBuilder<Preference> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
