using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class BookingSourceConfiguration : IEntityTypeConfiguration<BookingSource>
{
    public void Configure(EntityTypeBuilder<BookingSource> builder)
    {
        builder.ToTable("booking_sources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
    }
}
