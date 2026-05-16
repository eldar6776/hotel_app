using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class CheckOutService : ICheckOutService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IFolioService _folioService;
    private readonly ILogger<CheckOutService> _logger;

    public CheckOutService(
        HotelProDbContext dbContext,
        IFolioService folioService,
        ILogger<CheckOutService> logger)
    {
        _dbContext = dbContext;
        _folioService = folioService;
        _logger = logger;
    }

    public async Task<CheckOutResponse> CheckOutAsync(CheckOutRequest request)
    {
        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.BookingRooms)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException($"Booking must be in CheckedIn status. Current status: {booking.Status}");

        var bookingRoom = booking.BookingRooms.FirstOrDefault();
        if (bookingRoom == null || !bookingRoom.RoomId.HasValue)
            throw new InvalidOperationException("No assigned room found for this booking.");

        var room = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == bookingRoom.RoomId.Value);

        var nights = Math.Max(1, (booking.DepartureDate - booking.ArrivalDate).Days);
        var stayCharges = booking.BookingRooms.Sum(br => br.PricePerNight * nights);

        var folios = await _folioService.GetFoliosByBookingAsync(booking.Id);
        var folioCharges = folios.Sum(f => f.Balance);

        decimal lateCheckoutFee = 0;
        if (request.LateCheckout)
        {
            var checkoutTime = DateTime.UtcNow;
            var departureTime = booking.DepartureDate.AddHours(12);
            var hoursLate = (checkoutTime - departureTime).TotalHours;

            if (hoursLate > 6)
            {
                lateCheckoutFee = bookingRoom.PricePerNight;
            }
            else if (hoursLate > 0)
            {
                lateCheckoutFee = bookingRoom.PricePerNight * 0.5m;
            }
        }

        decimal discountAmount = 0;
        if (request.ApplyDiscounts)
        {
            if (booking.Type == BookingType.Group)
            {
                discountAmount = stayCharges * 0.1m;
            }
            else if (booking.Type == BookingType.Corporate)
            {
                discountAmount = stayCharges * 0.05m;
            }

            var previousStays = await _dbContext.GuestStayHistories
                .IgnoreQueryFilters()
                .CountAsync(sh => sh.GuestId == booking.GuestId && sh.CheckedOutAt != null);

            if (previousStays >= 5)
            {
                var loyaltyDiscount = stayCharges * 0.03m;
                discountAmount += loyaltyDiscount;
            }
        }

        var totalAmount = stayCharges + folioCharges + lateCheckoutFee - discountAmount;

        booking.Status = BookingStatus.CheckedOut;
        booking.UpdatedAt = DateTime.UtcNow;

        if (room != null)
        {
            room.Status = RoomStatus.Dirty;
        }

        var stayHistory = await _dbContext.GuestStayHistories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(sh => sh.BookingId == booking.Id && sh.CheckedOutAt == null);

        if (stayHistory != null)
        {
            stayHistory.CheckedOutAt = DateTime.UtcNow;
        }

        foreach (var br in booking.BookingRooms)
        {
            br.Status = BookingRoomStatus.Released;
        }

        foreach (var folio in folios)
        {
            await _folioService.CloseFolioAsync(Guid.Parse(folio.Id.ToString()));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            FolioId = folios.FirstOrDefault()?.Id ?? Guid.Empty,
            Amount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            Status = request.PaymentMethod == "Invoice" ? PaymentStatus.Unpaid : PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            Reference = request.PaymentReference,
            Notes = $"Check-out payment for booking {booking.Id}"
        };

        _dbContext.Payments.Add(payment);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
            FolioId = folios.FirstOrDefault()?.Id ?? Guid.Empty,
            GuestId = booking.GuestId,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            TotalNet = totalAmount,
            TotalVat = 0,
            TotalGross = totalAmount,
            Status = request.PaymentMethod == "Invoice" ? InvoiceStatus.Sent : InvoiceStatus.Paid,
            CreatedAt = DateTime.UtcNow,
        };
        _dbContext.Invoices.Add(invoice);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Check-out completed for booking {BookingId}, total: {Total}", booking.Id, totalAmount);

        var mainFolioNumber = folios.FirstOrDefault()?.FolioNumber ?? "N/A";

        return new CheckOutResponse(
            booking.Id,
            totalAmount,
            stayCharges,
            folioCharges,
            lateCheckoutFee,
            discountAmount,
            request.PaymentMethod,
            mainFolioNumber
        );
    }
}
