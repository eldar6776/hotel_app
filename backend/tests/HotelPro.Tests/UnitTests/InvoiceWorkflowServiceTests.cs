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

public class InvoiceWorkflowServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"InvoiceWorkflow_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private static Mock<IConfigurationService> CreateConfigMock(decimal vatRate = 17m)
    {
        var mock = new Mock<IConfigurationService>();
        mock.Setup(s => s.GetValueAsync("vat_rate")).ReturnsAsync(vatRate.ToString());
        return mock;
    }

    private async Task<(HotelProDbContext ctx, Folio folio, Guest guest, Room room)> SeedFolioWithNights(
        HotelProDbContext ctx, int nightCount = 2, decimal tariffPerNight = 100m)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(), FirstName = "Test", LastName = "Guest",
            Address = "123 St", IsActive = true, CreatedAt = DateTime.UtcNow
        };
        ctx.Guests.Add(guest);

        var folio = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-INV-001", GuestId = guest.Id,
            Status = FolioStatus.Open, Balance = nightCount * tariffPerNight,
            CreatedAt = DateTime.UtcNow
        };
        ctx.Folios.Add(folio);

        var building = new Building
        {
            Id = Guid.NewGuid(), Name = "Main", IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(), Name = "Standard", Code = "STD",
            BaseCapacity = 2, MaxCapacity = 3, DefaultPrice = tariffPerNight, IsActive = true
        };
        var room = new Room
        {
            Id = Guid.NewGuid(), RoomNumber = "101", Floor = 1,
            BuildingId = building.Id, RoomTypeId = roomType.Id,
            Status = RoomStatus.Occupied, IsActive = true,
            Building = building, RoomType = roomType
        };
        ctx.Buildings.Add(building);
        ctx.RoomTypes.Add(roomType);
        ctx.Rooms.Add(room);

        var stay = new Stay
        {
            Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id, GuestId = guest.Id,
            CheckInDate = new DateTime(2026, 6, 1), CheckOutDate = new DateTime(2026, 6, 1 + nightCount),
            CreatedAt = DateTime.UtcNow
        };
        ctx.Stays.Add(stay);

        for (var i = 0; i < nightCount; i++)
        {
            ctx.StayNights.Add(new StayNight
            {
                Id = Guid.NewGuid(), FolioId = folio.Id, RoomId = room.Id, StayId = stay.Id,
                Date = new DateTime(2026, 6, 1).AddDays(i),
                TariffAmount = tariffPerNight, DiscountPercent = 0,
                Status = NightStatus.Active, IsComp = false, Description = "Accommodation"
            });
        }

        await ctx.SaveChangesAsync();
        return (ctx, folio, guest, room);
    }

    [Fact]
    public async Task CreateInvoiceAsync_SnapshotsFolioWithVat()
    {
        await using var ctx = CreateContext();
        var (_, folio, _, _) = await SeedFolioWithNights(ctx, 3, 100m);

        var configMock = CreateConfigMock(17m);
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        var result = await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.StartsWith("INV-", result.InvoiceNumber);
        Assert.Equal(folio.Id, result.FolioId);
        Assert.Single(result.LineItems);
        Assert.Contains("Accommodation", result.LineItems[0].Description);
        Assert.True(result.TotalAmount > 0);
        Assert.False(result.IsStorno);

        var invoice = await ctx.Invoices.Include(i => i.Items).FirstAsync(i => i.Id == result.Id);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Single(invoice.Items);
    }

    [Fact]
    public async Task CreateInvoiceAsync_ThrowsWhenFolioNotFound()
    {
        await using var ctx = CreateContext();
        var configMock = CreateConfigMock();
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(Guid.NewGuid())));
    }

    [Fact]
    public async Task StornoInvoiceAsync_CancelsOriginalAndCreatesNegative()
    {
        await using var ctx = CreateContext();
        var (_, folio, _, _) = await SeedFolioWithNights(ctx, 2, 100m);

        var configMock = CreateConfigMock(17m);
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        var created = await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));

        var storno = await service.StornoInvoiceAsync(new StornoInvoiceRequest(created.Id, "Guest cancellation"));

        Assert.True(storno.IsStorno);
        Assert.Equal("Guest cancellation", storno.StornoReason);
        Assert.StartsWith("STN-", storno.InvoiceNumber);
        Assert.All(storno.LineItems, li => Assert.StartsWith("STORNO:", li.Description));
        Assert.True(storno.SubTotal < 0);

        var original = await ctx.Invoices.FindAsync(created.Id);
        Assert.Equal(InvoiceStatus.Cancelled, original!.Status);
    }

    [Fact]
    public async Task StornoInvoiceAsync_ThrowsWhenAlreadyCancelled()
    {
        await using var ctx = CreateContext();
        var (_, folio, _, _) = await SeedFolioWithNights(ctx, 1, 100m);

        var configMock = CreateConfigMock();
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        var created = await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));
        await service.StornoInvoiceAsync(new StornoInvoiceRequest(created.Id, "Cancel"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StornoInvoiceAsync(new StornoInvoiceRequest(created.Id, "Again")));
    }

    [Fact]
    public async Task GetInvoiceAsync_ReturnsInvoiceWithLineItems()
    {
        await using var ctx = CreateContext();
        var (_, folio, _, _) = await SeedFolioWithNights(ctx, 2, 80m);

        var configMock = CreateConfigMock(17m);
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        var created = await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));
        var fetched = await service.GetInvoiceAsync(created.Id);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.InvoiceNumber, fetched.InvoiceNumber);
        Assert.Equal(1, fetched.LineItems.Count);
    }

    [Fact]
    public async Task GetInvoicesForFolioAsync_ReturnsAllFolioInvoices()
    {
        await using var ctx = CreateContext();
        var (_, folio, _, _) = await SeedFolioWithNights(ctx, 2, 100m);

        var configMock = CreateConfigMock();
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));
        await service.CreateInvoiceAsync(new CreateFolioInvoiceRequest(folio.Id));

        var invoices = await service.GetInvoicesForFolioAsync(folio.Id);

        Assert.Equal(2, invoices.Count());
    }

    [Fact]
    public async Task GetInvoiceAsync_ThrowsWhenNotFound()
    {
        await using var ctx = CreateContext();
        var configMock = CreateConfigMock();
        var ledger = new FolioLedgerService(ctx, new Mock<ILogger<FolioLedgerService>>().Object);
        var service = new InvoiceWorkflowService(ctx, ledger, configMock.Object,
            new Mock<ILogger<InvoiceWorkflowService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetInvoiceAsync(Guid.NewGuid()));
    }
}
