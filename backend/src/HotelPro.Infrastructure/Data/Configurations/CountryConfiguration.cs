using HotelPro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelPro.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PhoneCode).HasMaxLength(5).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
    }
}
