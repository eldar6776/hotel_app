using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class NightLedgerServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"NightLedger_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private async Task<(Room room, Guest guest, Folio folio, Stay stay)> SeedStayAsync(
        HotelProDbContext context, DateTime checkIn, DateTime checkOut, decimal basePrice = 100m)
    {
        var building = new Building { Id = Guid.NewGuid(), Name = "Main", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(), Name = "Standard", Code = "STD",
            BaseCapacity = 1, MaxCapacity = 2, DefaultPrice = basePrice, IsActive = true
        };
        var room = new Room
        {
            Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1,
            BuildingId = building.Id, RoomTypeId = roomType.Id,
            Status = RoomStatus.Occupied, IsActive = true, BasePrice = basePrice,
            Building = building, RoomType = roomType
        };
        var guest = new Guest
        {
            Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe",
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-TEST001",
            Status = FolioStatus.Open, Balance = 0, CreatedAt = DateTime.UtcNow
        };
        var stay = new Stay
        {
            Id = Guid.NewGuid(), GuestId = guest.Id, RoomId = room.Id,
            FolioId = folio.Id, CheckInDate = checkIn, CheckOutDate = checkOut,
            IsCheckedOut = false, GuestCategory = GuestCategory.Adult,
            DiscountPercent = 0, CreatedAt = DateTime.UtcNow
        };

        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        context.Guests.Add(guest);
        context.Folios.Add(folio);
        context.Stays.Add(stay);
        await context.SaveChangesAsync();

        return (room, guest, folio, stay);
    }

    [Fact]
    public async Task CloseNightsForStayAsync_ClosesActiveNightsOnly()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 4));

        var activeNight1 = new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        };
        var activeNight2 = new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 2), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        };
        context.StayNights.Add(activeNight1);
        context.StayNights.Add(activeNight2);
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);
        var closedAt = new DateTime(2026, 6, 4, 10, 0, 0);

        var count = await service.CloseNightsForStayAsync(stay.Id, closedAt);

        Assert.Equal(2, count);

        var n1 = await context.StayNights.FindAsync(activeNight1.Id);
        var n2 = await context.StayNights.FindAsync(activeNight2.Id);
        Assert.Equal(NightStatus.Closed, n1!.Status);
        Assert.Equal(NightStatus.Closed, n2!.Status);
        Assert.Equal(closedAt, n1.ClosedAt);
    }

    [Fact]
    public async Task UpdateNightTariffAsync_UpdatesFolioBalance()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 3));

        var nightId = Guid.NewGuid();
        context.StayNights.Add(new StayNight
        {
            Id = nightId, FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        folio.Balance = 100;
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        var result = await service.UpdateNightTariffAsync(nightId, 120);

        Assert.Equal(120, result.TariffAmount);

        var updatedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.Equal(120, updatedFolio!.Balance);
    }

    [Fact]
    public async Task UpdateNightTariffAsync_ThrowsForClosedNight()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 3));

        var nightId = Guid.NewGuid();
        context.StayNights.Add(new StayNight
        {
            Id = nightId, FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Closed, IsComp = false, Description = "Accommodation",
            ClosedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateNightTariffAsync(nightId, 120));
    }

    [Fact]
    public async Task GenerateNightsForDateAsync_CreatesMissingNights()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 5));

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        folio.Balance = 100;
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        var created = await service.GenerateNightsForDateAsync(new DateTime(2026, 6, 2));

        Assert.Equal(1, created);

        var night = await context.StayNights.FirstAsync(n => n.StayId == stay.Id && n.Date == new DateTime(2026, 6, 2));
        Assert.Equal(100, night.TariffAmount);
        Assert.Equal(NightStatus.Active, night.Status);

        var updatedFolio = await context.Folios.FindAsync(folio.Id);
        Assert.Equal(200, updatedFolio!.Balance);
    }

    [Fact]
    public async Task GenerateNightsForDateAsync_SkipsExistingNights()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 3));

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        var created = await service.GenerateNightsForDateAsync(new DateTime(2026, 6, 1));

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task CalculateNightsTotalAsync_SumsActiveOnly()
    {
        await using var context = CreateContext();
        var (room, _, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 4));

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 2), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 5, 30), TariffAmount = 80, DiscountPercent = 0,
            Status = NightStatus.Closed, IsComp = false, Description = "Accommodation",
            ClosedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        var total = await service.CalculateNightsTotalAsync(folio.Id);

        Assert.Equal(200, total);
    }

    [Fact]
    public async Task CloseNightsForRoomAsync_ClosesAllActiveForFolio()
    {
        await using var context = CreateContext();
        var (room, guest, folio, stay) = await SeedStayAsync(context, new DateTime(2026, 6, 1), new DateTime(2026, 6, 4));

        var stay2 = new Stay
        {
            Id = Guid.NewGuid(), GuestId = guest.Id, RoomId = room.Id,
            FolioId = folio.Id, CheckInDate = new DateTime(2026, 6, 1), CheckOutDate = new DateTime(2026, 6, 4),
            IsCheckedOut = false, GuestCategory = GuestCategory.Adult,
            DiscountPercent = 0, CreatedAt = DateTime.UtcNow
        };
        context.Stays.Add(stay2);

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 50, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, StayId = stay2.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 50, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        await context.SaveChangesAsync();

        var service = new NightLedgerService(context, new Mock<ILogger<NightLedgerService>>().Object);

        var count = await service.CloseNightsForRoomAsync(room.Id, folio.Id, DateTime.UtcNow);

        Assert.Equal(2, count);
    }
}
