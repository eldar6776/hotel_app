using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.ToTable("charges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ChargeType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.VatAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(x => x.FolioId);

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.Charges)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceCatalog)
            .WithMany()
            .HasForeignKey(x => x.ServiceCatalogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PostedBy)
            .WithMany()
            .HasForeignKey(x => x.PostedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
