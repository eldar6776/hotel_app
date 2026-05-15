using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("access_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(30).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(45).IsRequired();

        builder.HasIndex(x => x.Timestamp);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.AccessLogs)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
