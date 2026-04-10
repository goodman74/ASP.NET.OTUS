using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoCodeFactory.Core.Domain.Administration;

namespace PromoCodeFactory.DataAccess.TableConfig
{
    internal class EmployeeTableConfig : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> entity)
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

            entity.HasOne(x => x.Role)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }
}
