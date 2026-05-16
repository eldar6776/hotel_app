using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class PhoneExtensionConfiguration : IEntityTypeConfiguration<PhoneExtension>
{
    public void Configure(EntityTypeBuilder<PhoneExtension> builder)
    {
        builder.ToTable("phone_extensions");
        builder.HasKey(x => x.Extension);
        builder.Property(x => x.Extension).HasMaxLength(10);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.SetNull);
    }
}
