using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class BookingGroupConfiguration : IEntityTypeConfiguration<BookingGroup>
{
    public void Configure(EntityTypeBuilder<BookingGroup> builder)
    {
        builder.ToTable("booking_groups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BlockedRoomCount).IsRequired();
        builder.Property(x => x.ConfirmedRoomCount).IsRequired();
        builder.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.UseMasterBill).IsRequired();

        builder.HasIndex(x => x.HotelId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Arrival);
        builder.HasIndex(x => x.Departure);
        builder.HasIndex(x => x.ReleaseDate);

        builder.HasOne(x => x.ContactPerson)
            .WithMany()
            .HasForeignKey(x => x.ContactPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MasterBill)
            .WithOne(x => x.Group)
            .HasForeignKey<MasterBill>(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
