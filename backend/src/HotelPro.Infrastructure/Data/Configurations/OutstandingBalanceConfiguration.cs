using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class OutstandingBalanceConfiguration : IEntityTypeConfiguration<OutstandingBalance>
{
    public void Configure(EntityTypeBuilder<OutstandingBalance> builder)
    {
        builder.ToTable("outstanding_balances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Balance).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne(x => x.Folio)
            .WithMany()
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
