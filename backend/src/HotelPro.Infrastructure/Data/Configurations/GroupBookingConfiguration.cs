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
        builder.Property(x => x.GroupName).HasMaxLength(200).IsRequired();

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MemberBooking)
            .WithMany()
            .HasForeignKey(x => x.MemberBookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
