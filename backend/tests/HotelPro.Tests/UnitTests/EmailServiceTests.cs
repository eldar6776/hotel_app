using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class EmailServiceTests
{
    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_Email_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics
                    .InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private EmailService CreateService(HotelProDbContext context)
    {
        var emailConfig = Options.Create(new EmailConfiguration
        {
            SmtpHost = "smtp.test.local",
            SmtpPort = 25,
            SmtpUsername = "",
            SmtpPassword = "",
            FromAddress = "test@hotelpro.com",
            FromName = "HotelPRO Test",
            UseTls = false,
            MaxRetries = 1,
            RetryDelaySeconds = 0
        });

        var logger = new Mock<ILogger<EmailService>>();
        return new EmailService(context, emailConfig, logger.Object);
    }

    private async Task<Booking> SeedBookingWithGuestAsync(HotelProDbContext context, string? guestEmail = "guest@test.com")
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = guestEmail,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Double",
            Code = "DBL",
            BaseCapacity = 2,
            MaxCapacity = 4,
            DefaultPrice = 100,
            IsActive = true,
            SortOrder = 1
        };

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Guest = guest,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 7, 1),
            DepartureDate = new DateTime(2026, 7, 5),
            AdultCount = 2,
            ChildCount = 1,
            TotalPrice = 400,
            ExchangeRateTotal = 400,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomTypeId = roomType.Id,
                    RoomType = roomType,
                    RatePlanId = Guid.NewGuid(),
                    PricePerNight = 100,
                    Status = BookingRoomStatus.Blocked
                }
            }
        };

        context.Guests.Add(guest);
        context.RoomTypes.Add(roomType);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return booking;
    }

    [Fact]
    public async Task SendConfirmationAsync_ReturnsFailure_WhenBookingNotFound()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var service = CreateService(context);

        var result = await service.SendConfirmationAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task SendConfirmationAsync_ReturnsFailure_WhenGuestEmailMissing()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: null);

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        var result = await service.SendConfirmationAsync(booking.Id);

        Assert.False(result.Success);
        Assert.Contains("email", (result.ErrorMessage ?? "").ToLower());
    }

    [Fact]
    public async Task SendConfirmationAsync_ReturnsFailure_WhenGuestEmailEmpty()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: "");

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        var result = await service.SendConfirmationAsync(booking.Id);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task SendCancellationAsync_ReturnsFailure_WhenBookingNotFound()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var service = CreateService(context);

        var result = await service.SendCancellationAsync(Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task SendConfirmationAsync_CreatesEmailLog_WhenGuestHasEmail()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: "guest@test.com");

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        var result = await service.SendConfirmationAsync(booking.Id);

        var emailLog = await context.EmailLogs.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.BookingId == booking.Id);

        Assert.NotNull(emailLog);
        Assert.Equal("guest@test.com", emailLog.Recipient);
        Assert.True(emailLog.IsHtml);
        Assert.Contains("John Doe", emailLog.Body);
        Assert.Equal(EmailStatus.Failed, emailLog.Status);
        Assert.Equal(1, emailLog.RetryCount);
    }

    [Fact]
    public async Task SendCancellationAsync_CreatesEmailLog_WhenGuestHasEmail()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: "guest@test.com");

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        var result = await service.SendCancellationAsync(booking.Id);

        var emailLog = await context.EmailLogs.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.BookingId == booking.Id);

        Assert.NotNull(emailLog);
        Assert.Equal("guest@test.com", emailLog.Recipient);
        Assert.Contains("otkazivanju", emailLog.Subject.ToLower());
    }

    [Fact]
    public async Task SendConfirmationAsync_EmailLogHasCorrectSubject()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: "guest@test.com");

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        await service.SendConfirmationAsync(booking.Id);

        var emailLog = await context.EmailLogs.IgnoreQueryFilters().FirstAsync(e => e.BookingId == booking.Id);

        Assert.Contains("Potvrda", emailLog.Subject);
        Assert.Contains(booking.Id.ToString().Substring(0, 8).ToUpper(), emailLog.Subject);
    }

    [Fact]
    public async Task SendCancellationAsync_EmailLogHasCorrectSubject()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        await SeedBookingWithGuestAsync(context, guestEmail: "guest@test.com");

        var service = CreateService(context);

        var booking = await context.Bookings.IgnoreQueryFilters().FirstAsync();
        await service.SendCancellationAsync(booking.Id);

        var emailLog = await context.EmailLogs.IgnoreQueryFilters().FirstAsync(e => e.BookingId == booking.Id);

        Assert.Contains("otkazivanju", emailLog.Subject.ToLower());
    }
}
