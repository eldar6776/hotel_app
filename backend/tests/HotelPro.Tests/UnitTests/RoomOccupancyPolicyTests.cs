using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class RoomOccupancyPolicyTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"RoomStatus_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsFree_WhenNoGuestsOrReservations()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 1));

        Assert.Equal(RoomStatus.Free, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsOccupied_WhenRoomHasActiveStay()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        await SeedBookingAsync(context, room, BookingStatus.CheckedIn, BookingRoomStatus.Occupied, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.Occupied, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsDeparting_WhenCheckoutIsToday()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        await SeedBookingAsync(context, room, BookingStatus.CheckedIn, BookingRoomStatus.Occupied, new DateTime(2026, 6, 1), new DateTime(2026, 6, 2));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.Departing, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsReservedConfirmed_WhenConfirmedReservationHasNoGuests()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        await SeedBookingAsync(context, room, BookingStatus.Confirmed, BookingRoomStatus.Blocked, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.ReservedConfirmed, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsOccupiedReserved_WhenActiveStayAndConfirmedReservationOverlap()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        await SeedBookingAsync(context, room, BookingStatus.CheckedIn, BookingRoomStatus.Occupied, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        await SeedBookingAsync(context, room, BookingStatus.Confirmed, BookingRoomStatus.Blocked, new DateTime(2026, 6, 2), new DateTime(2026, 6, 4));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 3));

        Assert.Equal(RoomStatus.OccupiedReserved, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsReservedUnconfirmed_WhenPendingReservationHasNoGuests()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        await SeedBookingAsync(context, room, BookingStatus.Pending, BookingRoomStatus.Blocked, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.ReservedUnconfirmed, result.Status);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsDirty_WhenRoomIsMarkedDirty()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context, RoomStatus.Dirty);
        await SeedBookingAsync(context, room, BookingStatus.Confirmed, BookingRoomStatus.Blocked, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.Dirty, result.Status);
        Assert.True(result.IsOverride);
        Assert.Equal(RoomStatus.ReservedConfirmed, result.BaseStatus);
    }

    [Fact]
    public async Task GetRoomStatusAsync_ReturnsOutOfOrder_WhenRoomIsOooAndDirty()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context, RoomStatus.Dirty);
        context.RoomOutOfOrders.Add(new RoomOutOfOrder
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            Reason = "Maintenance",
            Status = "Active",
            StartDate = new DateTime(2026, 6, 1),
            CreatedById = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var policy = new RoomOccupancyPolicy(context);

        var result = await policy.GetRoomStatusAsync(room.Id, new DateTime(2026, 6, 2));

        Assert.Equal(RoomStatus.OutOfOrder, result.Status);
    }

    private async Task<Room> SeedRoomAsync(HotelProDbContext context, RoomStatus status = RoomStatus.Free)
    {
        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Main",
            Code = Guid.NewGuid().ToString("N")[..8],
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Double",
            Code = Guid.NewGuid().ToString("N")[..8],
            BaseCapacity = 1,
            MaxCapacity = 2,
            DefaultPrice = 100,
            IsActive = true
        };

        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = Guid.NewGuid().ToString("N")[..4],
            Floor = 1,
            BuildingId = building.Id,
            RoomTypeId = roomType.Id,
            Status = status,
            IsActive = true,
            Building = building,
            RoomType = roomType
        };

        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        await context.SaveChangesAsync();

        return room;
    }

    private async Task SeedBookingAsync(
        HotelProDbContext context,
        Room room,
        BookingStatus bookingStatus,
        BookingRoomStatus roomStatus,
        DateTime arrival,
        DateTime departure)
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

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = _hotelId,
            GuestId = guest.Id,
            Source = BookingSource.Direct,
            Type = BookingType.Normal,
            Status = bookingStatus,
            ArrivalDate = arrival,
            DepartureDate = departure,
            AdultCount = 1,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms =
            [
                new BookingRoom
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    RoomTypeId = room.RoomTypeId,
                    RatePlanId = Guid.NewGuid(),
                    PricePerNight = 100,
                    Status = roomStatus
                }
            ]
        };

        context.Guests.Add(guest);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
    }
}
