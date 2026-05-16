using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class MasterBillConfiguration : IEntityTypeConfiguration<MasterBill>
{
    public void Configure(EntityTypeBuilder<MasterBill> builder)
    {
        builder.ToTable("master_bills");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalStayCharges).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsClosed).IsRequired();

        builder.HasIndex(x => x.GroupId).IsUnique();
        builder.HasIndex(x => x.PayerGuestId);

        builder.HasOne(x => x.PayerGuest)
            .WithMany()
            .HasForeignKey(x => x.PayerGuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
