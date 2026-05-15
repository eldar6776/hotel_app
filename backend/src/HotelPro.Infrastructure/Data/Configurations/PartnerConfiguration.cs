using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PartnerType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.CommissionPercent).HasColumnType("decimal(5,2)");
        builder.HasQueryFilter(x => x.IsActive);

        builder.HasIndex(x => x.Name);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Partners)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
