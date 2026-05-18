using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class HotelConfigConfiguration : IEntityTypeConfiguration<HotelConfig>
{
    public void Configure(EntityTypeBuilder<HotelConfig> builder)
    {
        builder.ToTable("hotel_configs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsSecret)
            .HasDefaultValue(false);

        builder.Property(x => x.IsEnabled)
            .HasDefaultValue(false);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(x => new { x.HotelId, x.Key }).IsUnique();
        builder.HasIndex(x => new { x.HotelId, x.Category });
    }
}
