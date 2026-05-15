using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class StayNightConfiguration : IEntityTypeConfiguration<StayNight>
{
    public void Configure(EntityTypeBuilder<StayNight> builder)
    {
        builder.ToTable("stay_nights");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RoomPrice).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.StayNights)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
