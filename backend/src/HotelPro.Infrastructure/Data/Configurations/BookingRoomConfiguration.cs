using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class BookingRoomConfiguration : IEntityTypeConfiguration<BookingRoom>
{
    public void Configure(EntityTypeBuilder<BookingRoom> builder)
    {
        builder.ToTable("booking_rooms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PricePerNight).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.RoomId);

        builder.HasOne(x => x.Booking)
            .WithMany(x => x.BookingRooms)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Guest)
            .WithMany()
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
