using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class StayConfiguration : IEntityTypeConfiguration<Stay>
{
    public void Configure(EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("stays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GuestCategory).IsRequired();
        builder.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(x => x.DiscountReason).HasMaxLength(200);
        builder.Property(x => x.StayNote).HasMaxLength(1000);
        builder.Property(x => x.ServiceNote).HasMaxLength(1000);
        builder.Property(x => x.PaymentNote).HasMaxLength(1000);

        builder.HasIndex(x => new { x.RoomId, x.IsCheckedOut });
        builder.HasIndex(x => new { x.GuestId, x.IsCheckedOut });
        builder.HasIndex(x => x.FolioId);

        builder.HasOne(x => x.Guest)
            .WithMany()
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Folio)
            .WithMany()
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BookingRoom)
            .WithMany()
            .HasForeignKey(x => x.BookingRoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.CheckedInByEmployee)
            .WithMany()
            .HasForeignKey(x => x.CheckedInBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CheckedOutByEmployee)
            .WithMany()
            .HasForeignKey(x => x.CheckedOutBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
