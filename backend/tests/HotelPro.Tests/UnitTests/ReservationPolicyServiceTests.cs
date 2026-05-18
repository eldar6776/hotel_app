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

public class ReservationPolicyServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"ReservationPolicy_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private async Task<Booking> SeedBooking(HotelProDbContext ctx, BookingStatus status = BookingStatus.Pending)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(), FirstName = "Res", LastName = "Guest",
            IsActive = true, CreatedAt = DateTime.UtcNow
        };
        ctx.Guests.Add(guest);

        var booking = new Booking
        {
            Id = Guid.NewGuid(), HotelId = _hotelId, GuestId = guest.Id,
            Status = status, ArrivalDate = new DateTime(2026, 7, 1),
            DepartureDate = new DateTime(2026, 7, 5), AdultCount = 2, ChildCount = 0,
            TotalPrice = 400, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        ctx.Bookings.Add(booking);
        await ctx.SaveChangesAsync();
        return booking;
    }

    [Fact]
    public async Task ConfirmReservationAsync_UpdatesStatusAndCreatesAudit()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        var result = await service.ConfirmReservationAsync(new ConfirmReservationRequest(booking.Id));

        Assert.Equal("Confirmed", result.Status);
        Assert.Single(result.AuditTrail);
        Assert.Equal("Modified", result.AuditTrail[0].Action);
        Assert.Equal("Pending", result.AuditTrail[0].PreviousValue);
        Assert.Equal("Confirmed", result.AuditTrail[0].NewValue);
    }

    [Fact]
    public async Task ConfirmReservationAsync_ThrowsWhenNotPending()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.Confirmed);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ConfirmReservationAsync(new ConfirmReservationRequest(booking.Id)));
    }

    [Fact]
    public async Task CancelReservationAsync_UpdatesStatusWithReason()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.Confirmed);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        var result = await service.CancelReservationAsync(
            new CancelReservationRequest(booking.Id, "Flight cancelled"));

        Assert.Equal("Cancelled", result.Status);
        Assert.Equal("Flight cancelled", result.CancellationReason);
        Assert.NotNull(result.CancelledAt);
        Assert.Single(result.AuditTrail);
        Assert.Equal("Cancelled", result.AuditTrail[0].Action);
    }

    [Fact]
    public async Task CancelReservationAsync_ThrowsWhenAlreadyCancelled()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.Cancelled);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelReservationAsync(new CancelReservationRequest(booking.Id, "Again")));
    }

    [Fact]
    public async Task CancelReservationAsync_ThrowsWhenCheckedOut()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.CheckedOut);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CancelReservationAsync(new CancelReservationRequest(booking.Id, "Too late")));
    }

    [Fact]
    public async Task MarkNoShowAsync_UpdatesStatusFromConfirmed()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.Confirmed);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        var result = await service.MarkNoShowAsync(new MarkNoShowRequest(booking.Id));

        Assert.Equal("NoShow", result.Status);
        Assert.Equal("NoShow", result.AuditTrail[0].Action);
    }

    [Fact]
    public async Task MarkNoShowAsync_UpdatesStatusFromPending()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.Pending);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        var result = await service.MarkNoShowAsync(new MarkNoShowRequest(booking.Id));

        Assert.Equal("NoShow", result.Status);
    }

    [Fact]
    public async Task MarkNoShowAsync_ThrowsWhenAlreadyCheckedIn()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx, BookingStatus.CheckedIn);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.MarkNoShowAsync(new MarkNoShowRequest(booking.Id)));
    }

    [Fact]
    public async Task GetReservationStatusAsync_ReturnsAuditTrail()
    {
        await using var ctx = CreateContext();
        var booking = await SeedBooking(ctx);
        var service = new ReservationPolicyService(ctx, new Mock<ILogger<ReservationPolicyService>>().Object);

        await service.ConfirmReservationAsync(new ConfirmReservationRequest(booking.Id));
        var result = await service.GetReservationStatusAsync(booking.Id);

        Assert.Equal("Confirmed", result.Status);
        Assert.Single(result.AuditTrail);
    }
}
