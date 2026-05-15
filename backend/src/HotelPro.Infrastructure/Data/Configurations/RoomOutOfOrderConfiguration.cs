using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class RoomOutOfOrderConfiguration : IEntityTypeConfiguration<RoomOutOfOrder>
{
    public void Configure(EntityTypeBuilder<RoomOutOfOrder> builder)
    {
        builder.ToTable("room_out_of_order");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ResolutionNotes).HasMaxLength(500);

        builder.HasIndex(x => x.RoomId);
        builder.HasIndex(x => new { x.RoomId, x.Status });

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
