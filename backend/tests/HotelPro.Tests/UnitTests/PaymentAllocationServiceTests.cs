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

public class PaymentAllocationServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"PaymentAlloc_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private static Mock<IFolioLedgerService> CreateLedgerMock()
    {
        var mock = new Mock<IFolioLedgerService>();
        mock.Setup(l => l.ReconcileFolioBalanceAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private async Task<(HotelProDbContext ctx, Folio f1, Folio f2)> SeedTwoFolios(HotelProDbContext ctx)
    {
        var f1 = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-PAY-001", Status = FolioStatus.Open,
            Balance = 200, CreatedAt = DateTime.UtcNow
        };
        var f2 = new Folio
        {
            Id = Guid.NewGuid(), FolioNumber = "F-PAY-002", Status = FolioStatus.Open,
            Balance = 150, CreatedAt = DateTime.UtcNow
        };
        ctx.Folios.AddRange(f1, f2);
        await ctx.SaveChangesAsync();
        return (ctx, f1, f2);
    }

    [Fact]
    public async Task AllocatePaymentAsync_SplitsPaymentAcrossFolios()
    {
        await using var ctx = CreateContext();
        var (_, f1, f2) = await SeedTwoFolios(ctx);
        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        var result = await service.AllocatePaymentAsync(new PaymentAllocationRequest(
            300m, "Cash",
            new List<FolioAllocationEntry>
            {
                new(f1.Id, 200m),
                new(f2.Id, 100m)
            }
        ));

        Assert.Equal(300m, result.TotalAmount);
        Assert.Equal(2, result.FolioPayments.Count);
        Assert.Equal(200m, result.FolioPayments[0].Amount);
        Assert.Equal(100m, result.FolioPayments[1].Amount);

        var payments = await ctx.Payments.Where(p => p.Reference == result.Reference).ToListAsync();
        Assert.Equal(2, payments.Count);
    }

    [Fact]
    public async Task AllocatePaymentAsync_ThrowsWhenOverAllocated()
    {
        await using var ctx = CreateContext();
        var (_, f1, _) = await SeedTwoFolios(ctx);
        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocatePaymentAsync(new PaymentAllocationRequest(
                100m, "Cash",
                new List<FolioAllocationEntry> { new(f1.Id, 150m) }
            )));
    }

    [Fact]
    public async Task AllocatePaymentAsync_ThrowsWhenFolioClosed()
    {
        await using var ctx = CreateContext();
        var (_, f1, f2) = await SeedTwoFolios(ctx);
        f1.Status = FolioStatus.Closed;
        f1.ClosedAt = DateTime.UtcNow;
        await ctx.SaveChangesAsync();

        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocatePaymentAsync(new PaymentAllocationRequest(
                100m, "Cash",
                new List<FolioAllocationEntry> { new(f1.Id, 100m) }
            )));
    }

    [Fact]
    public async Task AllocatePaymentAsync_ThrowsWhenFolioNotFound()
    {
        await using var ctx = CreateContext();
        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AllocatePaymentAsync(new PaymentAllocationRequest(
                100m, "Cash",
                new List<FolioAllocationEntry> { new(Guid.NewGuid(), 100m) }
            )));
    }

    [Fact]
    public async Task AllocatePaymentAsync_ReconcilesFolioBalances()
    {
        await using var ctx = CreateContext();
        var (_, f1, _) = await SeedTwoFolios(ctx);
        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        await service.AllocatePaymentAsync(new PaymentAllocationRequest(
            200m, "Card",
            new List<FolioAllocationEntry> { new(f1.Id, 200m) }
        ));

        ledger.Verify(l => l.ReconcileFolioBalanceAsync(f1.Id), Times.Once);
    }

    [Fact]
    public async Task GetAllocationsForPaymentAsync_ReturnsPaymentsByReference()
    {
        await using var ctx = CreateContext();
        var (_, f1, f2) = await SeedTwoFolios(ctx);
        var ledger = CreateLedgerMock();
        var service = new PaymentAllocationService(ctx, ledger.Object,
            new Mock<ILogger<PaymentAllocationService>>().Object);

        var created = await service.AllocatePaymentAsync(new PaymentAllocationRequest(
            300m, "Cash",
            new List<FolioAllocationEntry>
            {
                new(f1.Id, 200m),
                new(f2.Id, 100m)
            },
            Reference: "TEST-REF-001"
        ));

        var result = await service.GetAllocationsForPaymentAsync("TEST-REF-001");

        Assert.Equal("TEST-REF-001", result.Reference);
        Assert.Equal(300m, result.TotalAmount);
        Assert.Equal(2, result.FolioPayments.Count);
    }
}
