using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Repositories;
using HotelPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class BookingAvailabilityTests
{
    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics
                    .InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private BookingAvailabilityService CreateService(HotelProDbContext context)
    {
        var repo = new BookingRepository(context);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var ff = new FeatureFlagService(context, cache);
        return new BookingAvailabilityService(context, repo, ff);
    }

    private async Task<RoomType> SeedTestDataAsync(HotelProDbContext context)
    {
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Double",
            Code = "DBL",
            BaseCapacity = 2,
            MaxCapacity = 2,
            DefaultPrice = 80,
            IsActive = true,
            SortOrder = 1
        };

        var rooms = Enumerable.Range(1, 5).Select(i => new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = $"{i}01",
            Floor = 1,
            BuildingId = Guid.NewGuid(),
            RoomTypeId = roomType.Id,
            Status = RoomStatus.Free,
            BasePrice = 80,
            IsActive = true
        }).ToList();

        context.RoomTypes.Add(roomType);
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        return roomType;
    }

    private async Task<Guest> EnsureGuestAsync(HotelProDbContext context)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Guest",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();
        return guest;
    }

    [Fact]
    public async Task CheckAvailability_ReturnsAvailable_WhenNoConflicts()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
        Assert.Equal(5, result.AvailableQuantity);
        Assert.Equal(5, result.TotalRoomsOfType);
        Assert.Empty(result.ConflictingPeriods);
    }

    [Fact]
    public async Task CheckAvailability_ReturnsNotAvailable_WhenAllRoomsBooked()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var allRooms = await context.Rooms.ToListAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 400,
            ExchangeRateTotal = 400,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = allRooms.Select(r => new BookingRoom
            {
                Id = Guid.NewGuid(),
                RoomId = r.Id,
                RoomTypeId = roomType.Id,
                RatePlanId = Guid.NewGuid(),
                PricePerNight = 80,
                Status = BookingRoomStatus.Blocked
            }).ToList()
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 2),
            Departure: new DateTime(2026, 6, 4),
            Quantity: 1);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.False(result.IsAvailable);
        Assert.Equal(0, result.AvailableQuantity);
        Assert.NotEmpty(result.ConflictingPeriods);
    }

    [Fact]
    public async Task CheckAvailability_ExcludesCancelledBookings()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var room = await context.Rooms.FirstAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Cancelled,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 80,
            ExchangeRateTotal = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    RoomTypeId = roomType.Id,
                    RatePlanId = Guid.NewGuid(),
                    PricePerNight = 80,
                    Status = BookingRoomStatus.Blocked
                }
            }
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
        Assert.Equal(5, result.AvailableQuantity);
    }

    [Fact]
    public async Task CheckAvailability_ExcludesSpecifiedBookingId()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var room = await context.Rooms.FirstAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 80,
            ExchangeRateTotal = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = new List<BookingRoom>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    RoomTypeId = roomType.Id,
                    RatePlanId = Guid.NewGuid(),
                    PricePerNight = 80,
                    Status = BookingRoomStatus.Blocked
                }
            }
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1,
            ExcludeBookingId: booking.Id);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
        Assert.Equal(5, result.AvailableQuantity);
    }

    [Fact]
    public async Task LockRooms_ReturnsSuccess_WhenAvailable()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var service = CreateService(context);

        var request = new LockRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1);

        var result = await service.LockRoomsAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.LockId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task LockRooms_ReturnsFailure_WhenNotAvailable()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var allRooms = await context.Rooms.ToListAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 400,
            ExchangeRateTotal = 400,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var bookingRooms = allRooms.Select(r => new BookingRoom
        {
            Id = Guid.NewGuid(),
            RoomId = r.Id,
            RoomTypeId = roomType.Id,
            RatePlanId = Guid.NewGuid(),
            PricePerNight = 80,
            Status = BookingRoomStatus.Blocked,
            Booking = booking
        }).ToList();

        booking.BookingRooms = bookingRooms;
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new LockRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 2),
            Departure: new DateTime(2026, 6, 4),
            Quantity: 1);

        var result = await service.LockRoomsAsync(request);

        Assert.False(result.Success);
        Assert.Null(result.LockId);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task OverlappingDates_DetectConflict_Correctly()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var room = await context.Rooms.FirstAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 5),
            DepartureDate = new DateTime(2026, 6, 10),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 80,
            ExchangeRateTotal = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var bookingRoom = new BookingRoom
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            RoomTypeId = roomType.Id,
            RatePlanId = Guid.NewGuid(),
            PricePerNight = 80,
            Status = BookingRoomStatus.Blocked,
            Booking = booking
        };

        booking.BookingRooms = new List<BookingRoom> { bookingRoom };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 8),
            Departure: new DateTime(2026, 6, 12),
            Quantity: 1);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
        Assert.Equal(4, result.AvailableQuantity);
        Assert.Single(result.ConflictingPeriods);
    }

    [Fact]
    public async Task ReleasedBooking_DoesNotBlockAvailability()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var room = await context.Rooms.FirstAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 80,
            ExchangeRateTotal = 80,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var bookingRoom = new BookingRoom
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            RoomTypeId = roomType.Id,
            RatePlanId = Guid.NewGuid(),
            PricePerNight = 80,
            Status = BookingRoomStatus.Released,
            Booking = booking
        };

        booking.BookingRooms = new List<BookingRoom> { bookingRoom };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task AllowOverbooking_WhenFeatureEnabled_AlwaysReturnsAvailable()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var guest = await EnsureGuestAsync(context);

        var allRooms = await context.Rooms.ToListAsync();
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = Guid.NewGuid(),
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = BookingStatus.Confirmed,
            ArrivalDate = new DateTime(2026, 6, 1),
            DepartureDate = new DateTime(2026, 6, 5),
            AdultCount = 2,
            ChildCount = 0,
            TotalPrice = 400,
            ExchangeRateTotal = 400,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = allRooms.Select(r => new BookingRoom
            {
                Id = Guid.NewGuid(),
                RoomId = r.Id,
                RoomTypeId = roomType.Id,
                RatePlanId = Guid.NewGuid(),
                PricePerNight = 80,
                Status = BookingRoomStatus.Blocked
            }).ToList()
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        context.FeatureFlags.Add(new FeatureFlag
        {
            Id = Guid.NewGuid(),
            FeatureName = "AllowOverbooking",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var request = new AvailabilityRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 2),
            Departure: new DateTime(2026, 6, 4),
            Quantity: 3);

        var result = await service.CheckAvailabilityAsync(request);

        Assert.True(result.IsAvailable);
        Assert.Equal(5, result.AvailableQuantity);
    }

    [Fact]
    public async Task ConcurrentBooking_TwoSequentialLocks_SucceedWhenAvailable()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var service = CreateService(context);

        var lockRequest = new LockRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 3);

        var result1 = await service.LockRoomsAsync(lockRequest);
        Assert.True(result1.Success);

        var result2 = await service.LockRoomsAsync(lockRequest);
        Assert.True(result2.Success);
    }

    [Fact]
    public async Task LockRooms_SetsPostgresLockTimeout_BeforeAcquiring()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var roomType = await SeedTestDataAsync(context);
        var service = CreateService(context);

        var lockRequest = new LockRequest(
            RoomTypeId: roomType.Id,
            Arrival: new DateTime(2026, 6, 1),
            Departure: new DateTime(2026, 6, 5),
            Quantity: 1);

        var result = await service.LockRoomsAsync(lockRequest);

        Assert.True(result.Success);
        Assert.NotNull(result.LockId);
    }
}
