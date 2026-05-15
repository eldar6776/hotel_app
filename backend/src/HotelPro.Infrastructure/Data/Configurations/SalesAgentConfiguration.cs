using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class SalesAgentConfiguration : IEntityTypeConfiguration<SalesAgent>
{
    public void Configure(EntityTypeBuilder<SalesAgent> builder)
    {
        builder.ToTable("sales_agents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CommissionPercent).HasColumnType("decimal(5,2)");

        builder.HasOne(x => x.Partner)
            .WithMany(x => x.SalesAgents)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
