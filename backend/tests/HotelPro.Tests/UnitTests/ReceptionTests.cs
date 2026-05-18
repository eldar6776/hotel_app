using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class ReceptionTests
{
    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_Reception_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics
                    .InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private CheckInService CreateCheckInService(HotelProDbContext context)
    {
        var folioService = new FolioService(context);
        var logger = new Mock<ILogger<CheckInService>>();
        return new CheckInService(context, folioService, logger.Object);
    }

    private CheckOutService CreateCheckOutService(HotelProDbContext context)
    {
        var folioService = new FolioService(context);
        var logger = new Mock<ILogger<CheckOutService>>();
        return new CheckOutService(context, folioService, logger.Object);
    }

    private NightAuditService CreateNightAuditService(HotelProDbContext context)
    {
        var logger = new Mock<ILogger<NightAuditService>>();
        var nightLedger = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);
        return new NightAuditService(context, nightLedger, logger.Object);
    }

    private async Task<Guest> EnsureGuestAsync(HotelProDbContext context)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Guest",
            Email = "test@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();
        return guest;
    }

    private async Task<Room> EnsureRoomAsync(HotelProDbContext context, RoomStatus status = RoomStatus.Free)
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = "101",
            Floor = 1,
            BuildingId = Guid.NewGuid(),
            RoomTypeId = Guid.NewGuid(),
            Status = status,
            BasePrice = 100,
            IsActive = true
        };
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return room;
    }

    private async Task<Booking> EnsureConfirmedBookingAsync(HotelProDbContext context, Guid guestId, Guid roomTypeId)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guestId,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = DateTime.UtcNow.Date,
            DepartureDate = DateTime.UtcNow.Date.AddDays(3),
            AdultCount = 1,
            ChildCount = 0,
            TotalPrice = 300,
            ExchangeRateTotal = 300,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomTypeId = roomTypeId,
                    RatePlanId = Guid.NewGuid(),
                    PricePerNight = 100,
                    Status = BookingRoomStatus.Blocked
                }
            }
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return booking;
    }

    [Fact]
    public async Task CheckIn_Throws_WhenBookingNotConfirmed()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var room = await EnsureRoomAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Pending,
            ArrivalDate = DateTime.UtcNow.Date,
            DepartureDate = DateTime.UtcNow.Date.AddDays(3),
            AdultCount = 1,
            TotalPrice = 300,
            ExchangeRateTotal = 300,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new() { Id = Guid.NewGuid(), RoomTypeId = rt.Id, RatePlanId = Guid.NewGuid(), PricePerNight = 100, Status = BookingRoomStatus.Blocked }
            }
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateCheckInService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(new CheckInRequest(booking.Id, room.Id, null, null)));
    }

    [Fact]
    public async Task CheckIn_Success_CreatesFolioAndUpdatesStatus()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var room = await EnsureRoomAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = await EnsureConfirmedBookingAsync(context, guest.Id, rt.Id);

        var service = CreateCheckInService(context);

        var result = await service.CheckInAsync(new CheckInRequest(booking.Id, room.Id, null, null));

        Assert.Equal(booking.Id, result.BookingId);
        Assert.Equal(room.Id, result.RoomId);
        Assert.NotEmpty(result.FolioNumber);

        var updatedBooking = await context.Bookings.IgnoreQueryFilters().FirstAsync(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.CheckedIn, updatedBooking.Status);

        var updatedRoom = await context.Rooms.IgnoreQueryFilters().FirstAsync(r => r.Id == room.Id);
        Assert.Equal(RoomStatus.Occupied, updatedRoom.Status);

        var folio = await context.Folios.IgnoreQueryFilters().FirstOrDefaultAsync(f => f.BookingId == booking.Id);
        Assert.NotNull(folio);
    }

    [Fact]
    public async Task CheckIn_ReturnsWarning_ForExpiredDocument()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var room = await EnsureRoomAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = await EnsureConfirmedBookingAsync(context, guest.Id, rt.Id);

        var service = CreateCheckInService(context);

        var docs = new List<GuestDocumentDto>
        {
            new("Passport", "AB123456", DateTime.UtcNow.AddDays(-30))
        };

        var result = await service.CheckInAsync(new CheckInRequest(booking.Id, room.Id, docs, null));

        Assert.NotEmpty(result.Warnings);
        Assert.Contains("expired", result.Warnings.First().ToLower());
    }

    [Fact]
    public async Task CheckOut_Throws_WhenBookingNotCheckedIn()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = await EnsureConfirmedBookingAsync(context, guest.Id, rt.Id);

        var service = CreateCheckOutService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckOutAsync(new CheckOutRequest(booking.Id, "Cash", null, false, false)));
    }

    [Fact]
    public async Task CheckOut_Success_UpdatesStatusAndCreatesPayment()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var room = await EnsureRoomAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = await EnsureConfirmedBookingAsync(context, guest.Id, rt.Id);

        var checkInService = CreateCheckInService(context);
        await checkInService.CheckInAsync(new CheckInRequest(booking.Id, room.Id, null, null));

        var checkOutService = CreateCheckOutService(context);
        var result = await checkOutService.CheckOutAsync(new CheckOutRequest(booking.Id, "Cash", null, false, false));

        Assert.Equal(booking.Id, result.BookingId);
        Assert.True(result.TotalAmount > 0);

        var updatedBooking = await context.Bookings.IgnoreQueryFilters().FirstAsync(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.CheckedOut, updatedBooking.Status);

        var updatedRoom = await context.Rooms.IgnoreQueryFilters().FirstAsync(r => r.Id == room.Id);
        Assert.Equal(RoomStatus.Dirty, updatedRoom.Status);

        var payment = await context.Payments.FirstOrDefaultAsync(p => p.FolioId != Guid.Empty);
        Assert.NotNull(payment);
    }

    [Fact]
    public async Task NightAudit_CreatesStayNightCharges_ForCheckedInBookings()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var room = await EnsureRoomAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = await EnsureConfirmedBookingAsync(context, guest.Id, rt.Id);

        var folioService = new FolioService(context);
        await folioService.CreateFolioAsync(new CreateFolioDto(booking.Id, guest.Id, null));

        var checkInService = CreateCheckInService(context);
        await checkInService.CheckInAsync(new CheckInRequest(booking.Id, room.Id, null, null));

        var auditService = CreateNightAuditService(context);
        var result = await auditService.RunAuditAsync(DateTime.UtcNow.Date);

        Assert.True(result.Success);
        Assert.Equal(1, result.BookingsProcessed);
        Assert.True(result.TotalStayCharges > 0);

        var stayNight = await context.StayNights.FirstOrDefaultAsync();
        Assert.NotNull(stayNight);
    }

    [Fact]
    public async Task NightAudit_Skips_WhenAlreadyRan()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);

        var auditService = CreateNightAuditService(context);
        var result1 = await auditService.RunAuditAsync(DateTime.UtcNow.Date);
        var result2 = await auditService.RunAuditAsync(DateTime.UtcNow.Date);

        Assert.True(result1.Success);
        Assert.False(result2.Success);
        Assert.Contains("already", result2.ErrorMessage!.ToLower());
    }

    [Fact]
    public async Task NightAudit_DetectsNoShows_ForOverdueConfirmedBookings()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = new RoomType { Id = Guid.NewGuid(), Name = "Double", Code = "DBL", BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = DateTime.UtcNow.Date.AddDays(-2),
            DepartureDate = DateTime.UtcNow.Date.AddDays(1),
            AdultCount = 1,
            TotalPrice = 300,
            ExchangeRateTotal = 300,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new() { Id = Guid.NewGuid(), RoomTypeId = rt.Id, RatePlanId = Guid.NewGuid(), PricePerNight = 100, Status = BookingRoomStatus.Blocked }
            }
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var auditService = CreateNightAuditService(context);
        var result = await auditService.RunAuditAsync(DateTime.UtcNow.Date);

        Assert.True(result.Success);
        Assert.Equal(1, result.NoShowsDetected);

        var updatedBooking = await context.Bookings.IgnoreQueryFilters().FirstAsync(b => b.Id == booking.Id);
        Assert.Equal(BookingStatus.NoShow, updatedBooking.Status);
    }
}
