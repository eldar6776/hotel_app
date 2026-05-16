using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => x.FolioId);

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PaymentMethodEntity)
            .WithMany()
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(x => x.ProcessedBy)
            .WithMany()
            .HasForeignKey(x => x.ProcessedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
