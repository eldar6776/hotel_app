using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class CheckOutWorkflowServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"CheckOutWorkflow_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private async Task<(Room room, Folio folio, Stay stay1, Stay stay2)> SeedRoomWithStaysAsync(
        HotelProDbContext context, int guestCount = 1)
    {
        var building = new Building { Id = Guid.NewGuid(), Name = "Main", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(), Name = "Standard", Code = "STD",
            BaseCapacity = 2, MaxCapacity = 4, DefaultPrice = 100, IsActive = true
        };
        var room = new Room
        {
            Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1,
            BuildingId = building.Id, RoomTypeId = roomType.Id,
            Status = RoomStatus.Occupied, IsActive = true, BasePrice = 100,
            Building = building, RoomType = roomType
        };
        var guest1 = new Guest
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-TEST001",
            Status = FolioStatus.Open, Balance = 300, CreatedAt = DateTime.UtcNow
        };
        var stay1 = new Stay
        {
            Id = Guid.NewGuid(), GuestId = guest1.Id, RoomId = room.Id,
            FolioId = folio.Id, CheckInDate = new DateTime(2026, 6, 1), CheckOutDate = new DateTime(2026, 6, 4),
            IsCheckedOut = false, GuestCategory = GuestCategory.Adult,
            DiscountPercent = 0, CreatedAt = DateTime.UtcNow
        };

        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        context.Guests.Add(guest1);
        context.Folios.Add(folio);
        context.Stays.Add(stay1);

        Stay? stay2 = null;
        if (guestCount >= 2)
        {
            var guest2 = new Guest
            {
                Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe",
                IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            stay2 = new Stay
            {
                Id = Guid.NewGuid(), GuestId = guest2.Id, RoomId = room.Id,
                FolioId = folio.Id, CheckInDate = new DateTime(2026, 6, 1), CheckOutDate = new DateTime(2026, 6, 4),
                IsCheckedOut = false, GuestCategory = GuestCategory.Adult,
                DiscountPercent = 0, CreatedAt = DateTime.UtcNow
            };
            context.Guests.Add(guest2);
            context.Stays.Add(stay2);
        }

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay1.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay1.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 2), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay1.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 3), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });

        if (stay2 != null)
        {
            context.StayNights.Add(new StayNight
            {
                Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay2.Id, RoomId = room.Id,
                Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
                Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
            });
        }

        await context.SaveChangesAsync();
        return (room, folio, stay1, stay2!);
    }

    private CheckOutWorkflowService CreateService(
        HotelProDbContext context,
        Mock<INightLedgerService>? nightLedgerMock = null,
        Mock<IRoomOccupancyPolicy>? policyMock = null)
    {
        nightLedgerMock ??= new Mock<INightLedgerService>();
        policyMock ??= new Mock<IRoomOccupancyPolicy>();

        policyMock.Setup(x => x.GetRoomStatusAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync((Guid roomId, DateTime date) =>
                new RoomStatusDetailDto(roomId, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var logger = new Mock<ILogger<CheckOutWorkflowService>>();
        return new CheckOutWorkflowService(context, nightLedgerMock.Object, policyMock.Object, logger.Object);
    }

    [Fact]
    public async Task FullCheckOutAsync_ChecksOutAllGuests()
    {
        await using var context = CreateContext();
        var (room, folio, stay1, stay2) = await SeedRoomWithStaysAsync(context, guestCount: 2);

        var service = CreateService(context);

        var result = await service.FullCheckOutAsync(new FullCheckOutRequest(RoomId: room.Id));

        Assert.Equal(2, result.GuestsCheckedOut);
        Assert.Equal("101", result.RoomNumber);
        Assert.True(result.FolioClosed);

        var stays = await context.Stays.Where(s => s.RoomId == room.Id).ToListAsync();
        Assert.All(stays, s => Assert.True(s.IsCheckedOut));

        var updatedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.Equal(FolioStatus.Closed, updatedFolio!.Status);
    }

    [Fact]
    public async Task FullCheckOutAsync_MarksRoomDirty()
    {
        await using var context = CreateContext();
        var (room, _, stay1, _) = await SeedRoomWithStaysAsync(context, guestCount: 1);

        var service = CreateService(context);

        await service.FullCheckOutAsync(new FullCheckOutRequest(RoomId: room.Id));

        var updatedRoom = await context.Rooms.FindAsync(room.Id);
        Assert.Equal(RoomStatus.Dirty, updatedRoom!.Status);
    }

    [Fact]
    public async Task FullCheckOutAsync_CreatesUnpaidBalance_WhenFolioHasBalance()
    {
        await using var context = CreateContext();
        var (room, folio, stay1, _) = await SeedRoomWithStaysAsync(context, guestCount: 1);

        var service = CreateService(context);

        var result = await service.FullCheckOutAsync(new FullCheckOutRequest(RoomId: room.Id, CreateUnpaidRecords: true));

        Assert.True(result.HasUnpaidBalance);
        Assert.Equal(300, result.OutstandingAmount);

        var outstanding = await context.OutstandingBalances.FirstOrDefaultAsync(ob => ob.FolioId == folio.Id);
        Assert.NotNull(outstanding);
        Assert.Equal(300, outstanding!.Balance);
    }

    [Fact]
    public async Task FullCheckOutAsync_ThrowsWhenNoActiveStays()
    {
        await using var context = CreateContext();

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.FullCheckOutAsync(new FullCheckOutRequest(RoomId: Guid.NewGuid())));
    }

    [Fact]
    public async Task PartialCheckOutAsync_ChecksOutSingleGuest()
    {
        await using var context = CreateContext();
        var (room, folio, stay1, stay2) = await SeedRoomWithStaysAsync(context, guestCount: 2);

        var service = CreateService(context);

        var result = await service.PartialCheckOutAsync(new PartialCheckOutRequest(StayId: stay1.Id));

        Assert.Equal(stay1.Id, result.StayId);
        Assert.Equal(1, result.RemainingGuests);
        Assert.True(result.FolioStillOpen);

        var checkedOutStay = await context.Stays.FindAsync(stay1.Id);
        Assert.True(checkedOutStay!.IsCheckedOut);

        var remainingStay = await context.Stays.FindAsync(stay2.Id);
        Assert.False(remainingStay!.IsCheckedOut);

        var updatedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.Equal(FolioStatus.Open, updatedFolio!.Status);
    }

    [Fact]
    public async Task PartialCheckOutAsync_ThrowsWhenOnlyOneGuestRemains()
    {
        await using var context = CreateContext();
        var (room, _, stay1, _) = await SeedRoomWithStaysAsync(context, guestCount: 1);

        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.PartialCheckOutAsync(new PartialCheckOutRequest(StayId: stay1.Id)));
    }
}
