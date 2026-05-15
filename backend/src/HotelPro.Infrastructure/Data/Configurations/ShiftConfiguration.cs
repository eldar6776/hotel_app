using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("shifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ShiftType).HasMaxLength(20).IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.ShiftDate });

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
