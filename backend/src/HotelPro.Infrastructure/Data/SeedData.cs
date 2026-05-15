using HotelPro.Core.Entities;
using HotelPro.Core.Enums;

namespace HotelPro.Infrastructure.Data;

public static class SeedData
{
    public static void Initialize(HotelProDbContext context)
    {
        if (context.BookingSources.Any()) return;

        context.BookingSources.AddRange(
            new BookingSource { Id = Guid.NewGuid(), Name = "Direct", Code = "DIR", IsActive = true },
            new BookingSource { Id = Guid.NewGuid(), Name = "Booking.com", Code = "BCOM", IsActive = true },
            new BookingSource { Id = Guid.NewGuid(), Name = "Airbnb", Code = "ABNB", IsActive = true },
            new BookingSource { Id = Guid.NewGuid(), Name = "Expedia", Code = "EXP", IsActive = true },
            new BookingSource { Id = Guid.NewGuid(), Name = "Walk-in", Code = "WALK", IsActive = true }
        );

        context.BookingTypes.AddRange(
            new BookingType { Id = Guid.NewGuid(), Name = "Individual", Code = "IND", Color = "#4CAF50", IsActive = true },
            new BookingType { Id = Guid.NewGuid(), Name = "Group", Code = "GRP", Color = "#2196F3", IsActive = true },
            new BookingType { Id = Guid.NewGuid(), Name = "Corporate", Code = "CORP", Color = "#FF9800", IsActive = true },
            new BookingType { Id = Guid.NewGuid(), Name = "VIP", Code = "VIP", Color = "#9C27B0", IsActive = true }
        );

        context.PaymentMethods.AddRange(
            new PaymentMethod { Id = Guid.NewGuid(), Name = "Cash", Code = "CASH", IsActive = true },
            new PaymentMethod { Id = Guid.NewGuid(), Name = "Credit Card", Code = "CC", IsActive = true },
            new PaymentMethod { Id = Guid.NewGuid(), Name = "Debit Card", Code = "DC", IsActive = true },
            new PaymentMethod { Id = Guid.NewGuid(), Name = "Bank Transfer", Code = "BT", IsActive = true },
            new PaymentMethod { Id = Guid.NewGuid(), Name = "Voucher", Code = "VCH", IsActive = true }
        );

        context.Countries.AddRange(
            new Country { Id = Guid.NewGuid(), Code = "HRV", Name = "Croatia", Nationality = "Croatian", PhoneCode = "+385", CurrencyCode = "EUR" },
            new Country { Id = Guid.NewGuid(), Code = "DEU", Name = "Germany", Nationality = "German", PhoneCode = "+49", CurrencyCode = "EUR" },
            new Country { Id = Guid.NewGuid(), Code = "AUT", Name = "Austria", Nationality = "Austrian", PhoneCode = "+43", CurrencyCode = "EUR" },
            new Country { Id = Guid.NewGuid(), Code = "ITA", Name = "Italy", Nationality = "Italian", PhoneCode = "+39", CurrencyCode = "EUR" },
            new Country { Id = Guid.NewGuid(), Code = "USA", Name = "United States", Nationality = "American", PhoneCode = "+1", CurrencyCode = "USD" },
            new Country { Id = Guid.NewGuid(), Code = "GBR", Name = "United Kingdom", Nationality = "British", PhoneCode = "+44", CurrencyCode = "GBP" }
        );

        context.RoomTypes.AddRange(
            new RoomType { Id = Guid.NewGuid(), Name = "Single", Code = "SGL", BaseCapacity = 1, MaxCapacity = 1, DefaultPrice = 50, IsActive = true, SortOrder = 1 },
            new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 2, DefaultPrice = 80, IsActive = true, SortOrder = 2 },
            new RoomType { Id = Guid.NewGuid(), Name = "Twin", Code = "TWN", BaseCapacity = 2, MaxCapacity = 2, DefaultPrice = 80, IsActive = true, SortOrder = 3 },
            new RoomType { Id = Guid.NewGuid(), Name = "Suite", Code = "STE", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 150, IsActive = true, SortOrder = 4 }
        );

        context.SaveChanges();
    }
}
