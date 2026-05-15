using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class GuestDocumentConfiguration : IEntityTypeConfiguration<GuestDocument>
{
    public void Configure(EntityTypeBuilder<GuestDocument> builder)
    {
        builder.ToTable("guest_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IssuingCountry).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Guest)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
