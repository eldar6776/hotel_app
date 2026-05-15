using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(h => h.Code).IsUnique();

        builder.Property(h => h.Address).HasMaxLength(300);
        builder.Property(h => h.City).HasMaxLength(100);
        builder.Property(h => h.Country).HasMaxLength(100);
        builder.Property(h => h.Phone).HasMaxLength(50);
        builder.Property(h => h.Email).HasMaxLength(200);
        builder.Property(h => h.Currency).HasMaxLength(3).HasDefaultValue("EUR");
        builder.Property(h => h.TimeZone).HasMaxLength(50).HasDefaultValue("Europe/Zagreb");
        builder.Property(h => h.VatNumber).HasMaxLength(50);
        builder.Property(h => h.LogoUrl).HasMaxLength(500);

        builder.Property(h => h.IsActive).HasDefaultValue(true);
        builder.Property(h => h.CreatedAt).HasDefaultValueSql("NOW()");
    }
}
