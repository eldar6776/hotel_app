using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.PinHash).HasMaxLength(256);
        builder.Property(x => x.PasswordHash).HasMaxLength(256);
        builder.HasQueryFilter(x => x.IsActive);

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
