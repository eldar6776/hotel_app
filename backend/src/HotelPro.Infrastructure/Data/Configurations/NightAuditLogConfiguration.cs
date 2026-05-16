using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class NightAuditLogConfiguration : IEntityTypeConfiguration<NightAuditLog>
{
    public void Configure(EntityTypeBuilder<NightAuditLog> builder)
    {
        builder.ToTable("night_audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AuditDate).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ErrorMessage).HasColumnType("text");

        builder.HasIndex(x => x.AuditDate).IsUnique();
    }
}
