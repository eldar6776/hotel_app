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

public class FolioLedgerServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"FolioLedger_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    [Fact]
    public async Task GetFolioBalanceAsync_ComputesFromLedger()
    {
        await using var context = CreateContext();
        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-001",
            Status = FolioStatus.Open, Balance = 999, CreatedAt = DateTime.UtcNow
        };
        context.Folios.Add(folio);

        var building = new Building { Id = Guid.NewGuid(), Name = "M", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "S", Code = "S", BaseCapacity = 1, MaxCapacity = 2, DefaultPrice = 100, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1, BuildingId = building.Id, RoomTypeId = roomType.Id, Status = RoomStatus.Occupied, IsActive = true, Building = building, RoomType = roomType };
        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 2), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });

        context.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, Amount = 50,
            PaymentMethod = "Cash", Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new FolioLedgerService(context, new Mock<ILogger<FolioLedgerService>>().Object);
        var balance = await service.GetFolioBalanceAsync(folio.Id);

        Assert.Equal(150, balance);
    }

    [Fact]
    public async Task ReconcileFolioBalanceAsync_UpdatesStoredBalance()
    {
        await using var context = CreateContext();
        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-002",
            Status = FolioStatus.Open, Balance = 999, CreatedAt = DateTime.UtcNow
        };
        context.Folios.Add(folio);

        var building = new Building { Id = Guid.NewGuid(), Name = "M", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "S", Code = "S", BaseCapacity = 1, MaxCapacity = 2, DefaultPrice = 100, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1, BuildingId = building.Id, RoomTypeId = roomType.Id, Status = RoomStatus.Occupied, IsActive = true, Building = building, RoomType = roomType };
        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });

        await context.SaveChangesAsync();

        var service = new FolioLedgerService(context, new Mock<ILogger<FolioLedgerService>>().Object);
        await service.ReconcileFolioBalanceAsync(folio.Id);

        var updated = await context.Folios.FindAsync(folio.Id);
        Assert.Equal(100, updated!.Balance);
    }

    [Fact]
    public async Task GetFolioLedgerAsync_ReturnsAllEntries()
    {
        await using var context = CreateContext();
        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-003",
            Status = FolioStatus.Open, Balance = 200, CreatedAt = DateTime.UtcNow
        };
        context.Folios.Add(folio);

        var building = new Building { Id = Guid.NewGuid(), Name = "M", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType { Id = Guid.NewGuid(), Name = "S", Code = "S", BaseCapacity = 1, MaxCapacity = 2, DefaultPrice = 100, IsActive = true };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1, BuildingId = building.Id, RoomTypeId = roomType.Id, Status = RoomStatus.Occupied, IsActive = true, Building = building, RoomType = roomType };
        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);

        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 1), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });
        context.StayNights.Add(new StayNight
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id,
            Date = new DateTime(2026, 6, 2), TariffAmount = 100, DiscountPercent = 0,
            Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
        });

        await context.SaveChangesAsync();

        var service = new FolioLedgerService(context, new Mock<ILogger<FolioLedgerService>>().Object);
        var ledger = await service.GetFolioLedgerAsync(folio.Id);

        Assert.Equal(200, ledger.NightCharges);
        Assert.Equal(0, ledger.OtherCharges);
        Assert.Equal(0, ledger.TotalPayments);
        Assert.Equal(200, ledger.Balance);
        Assert.Equal(2, ledger.Entries.Count);
    }
}
