using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class GroupBookingConfiguration : IEntityTypeConfiguration<GroupBooking>
{
    public void Configure(EntityTypeBuilder<GroupBooking> builder)
    {
        builder.ToTable("group_bookings");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.GroupId, x.BookingId }).IsUnique();
        builder.HasIndex(x => x.RoomTypeId);

        builder.HasOne(x => x.Group)
            .WithMany(x => x.GroupBookings)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RoomType)
            .WithMany()
            .HasForeignKey(x => x.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
