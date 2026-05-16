using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Repositories;
using HotelPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class BookingServiceTests
{
    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_BS_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics
                    .InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private BookingService CreateService(HotelProDbContext context)
    {
        var repo = new BookingRepository(context);
        var tenant = new Mock<ITenantService>();
        tenant.Setup(t => t.GetCurrentHotelId()).Returns(Guid.NewGuid());
        var email = new Mock<IEmailService>();
        var logger = new Mock<ILogger<BookingService>>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var ff = new FeatureFlagService(context, cache);
        var avail = new BookingAvailabilityService(context, repo, ff);
        return new BookingService(repo, context, tenant.Object, email.Object, logger.Object, avail);
    }

    private async Task<Guest> EnsureGuestAsync(HotelProDbContext context)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();
        return guest;
    }

    private async Task<RoomType> EnsureRoomTypeAsync(HotelProDbContext context, decimal price = 100)
    {
        var rt = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Double",
            Code = "DBL",
            BaseCapacity = 2,
            MaxCapacity = 4,
            DefaultPrice = price,
            IsActive = true,
            SortOrder = 1
        };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();
        return rt;
    }

    private async Task EnsureRoomsAsync(HotelProDbContext context, Guid roomTypeId, int count = 5)
    {
        var rooms = Enumerable.Range(1, count).Select(i => new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = $"{i}01",
            Floor = 1,
            BuildingId = Guid.NewGuid(),
            RoomTypeId = roomTypeId,
            Status = RoomStatus.Free,
            BasePrice = 100,
            IsActive = true
        }).ToList();
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateBooking_Success_WithValidData()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 2,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var result = await service.CreateBookingAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Pending.ToString(), result.Status);
        Assert.Equal(400, result.TotalPrice);
    }

    [Fact]
    public async Task CreateBooking_Throws_WhenArrivalAfterDeparture()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: Guid.NewGuid(),
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 5),
            DepartureDate: new DateTime(2026, 8, 1),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(dto));
    }

    [Fact]
    public async Task CreateBooking_Throws_WhenNoAdultsForNonComplementary()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: Guid.NewGuid(),
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 0,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateBookingAsync(dto));
    }

    [Fact]
    public async Task UpdateBookingStatus_FromPending_ToConfirmed_Succeeds()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var booking = await service.CreateBookingAsync(dto);
        await service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed);

        var updated = await service.GetBookingByIdAsync(booking.Id);
        Assert.Equal(BookingStatus.Confirmed.ToString(), updated!.Status);
    }

    [Fact]
    public async Task UpdateBookingStatus_CheckedIn_ToCancelled_Throws()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var booking = await service.CreateBookingAsync(dto);
        await service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed);
        await service.UpdateBookingStatusAsync(booking.Id, BookingStatus.CheckedIn);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Cancelled));
    }

    [Fact]
    public async Task DeleteBooking_OnlyPending_Allowed()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var booking = await service.CreateBookingAsync(dto);
        await service.DeleteBookingAsync(booking.Id);

        var deleted = await service.GetBookingByIdAsync(booking.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteBooking_Confirmed_Throws()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var booking = await service.CreateBookingAsync(dto);
        await service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteBookingAsync(booking.Id));
    }

    [Fact]
    public async Task GetBookings_ReturnsPagedResults()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        for (int i = 0; i < 3; i++)
        {
            var dto = new CreateBookingDto(
                GuestId: guest.Id,
                GroupId: null,
                Source: BookingSource.Direct,
                Type: BookingType.Normal,
                ArrivalDate: new DateTime(2026, 8, 1),
                DepartureDate: new DateTime(2026, 8, 5),
                AdultCount: 1,
                ChildCount: 0,
                Notes: null,
                InternalNotes: null,
                Rooms: new List<CreateBookingRoomDto>
                {
                    new(null, rt.Id, Guid.NewGuid(), 100)
                }
            );
            await service.CreateBookingAsync(dto);
        }

        var result = await service.GetBookingsAsync(new BookingFilter(null, null, null, null, null, 1, 10));
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task CancelledBooking_DoesNotTransitionFurther()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context);
        var rt = await EnsureRoomTypeAsync(context);
        await EnsureRoomsAsync(context, rt.Id, 5);

        var service = CreateService(context);

        var dto = new CreateBookingDto(
            GuestId: guest.Id,
            GroupId: null,
            Source: BookingSource.Direct,
            Type: BookingType.Normal,
            ArrivalDate: new DateTime(2026, 8, 1),
            DepartureDate: new DateTime(2026, 8, 5),
            AdultCount: 1,
            ChildCount: 0,
            Notes: null,
            InternalNotes: null,
            Rooms: new List<CreateBookingRoomDto>
            {
                new(null, rt.Id, Guid.NewGuid(), 100)
            }
        );

        var booking = await service.CreateBookingAsync(dto);
        await service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Cancelled);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateBookingStatusAsync(booking.Id, BookingStatus.Confirmed));
    }
}
