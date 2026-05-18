using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class StayNightConfiguration : IEntityTypeConfiguration<StayNight>
{
    public void Configure(EntityTypeBuilder<StayNight> builder)
    {
        builder.ToTable("stay_nights");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TariffAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.StayNights)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Stay)
            .WithMany()
            .HasForeignKey(x => x.StayId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.StayId, x.Date });
        builder.HasIndex(x => new { x.RoomId, x.Date });
    }
}
