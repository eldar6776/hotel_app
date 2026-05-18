using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class ReservationPolicyService : IReservationPolicyService
{
    private readonly HotelProDbContext _dbContext;
    private readonly ILogger<ReservationPolicyService> _logger;

    public ReservationPolicyService(
        HotelProDbContext dbContext,
        ILogger<ReservationPolicyService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReservationResultDto> ConfirmReservationAsync(ConfirmReservationRequest request)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Histories)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm booking in status {booking.Status}. Only Pending bookings can be confirmed.");

        var previousStatus = booking.Status.ToString();
        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Action = BookingHistoryAction.Modified,
            PreviousValue = previousStatus,
            NewValue = BookingStatus.Confirmed.ToString(),
            ChangedById = request.ConfirmedById,
            ChangedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} confirmed", booking.Id);

        return MapToResult(booking);
    }

    public async Task<ReservationResultDto> CancelReservationAsync(CancelReservationRequest request)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Histories)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.CheckedOut)
            throw new InvalidOperationException("Cannot cancel a checked-out booking.");

        var previousStatus = booking.Status.ToString();
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = request.Reason;
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Action = BookingHistoryAction.Cancelled,
            PreviousValue = previousStatus,
            NewValue = BookingStatus.Cancelled.ToString(),
            ChangedById = request.CancelledById,
            ChangedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} cancelled: {Reason}", booking.Id, request.Reason);

        return MapToResult(booking);
    }

    public async Task<ReservationResultDto> MarkNoShowAsync(MarkNoShowRequest request)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Histories)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot mark NoShow for booking in status {booking.Status}. Only Pending or Confirmed bookings can be marked as NoShow.");

        var previousStatus = booking.Status.ToString();
        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = DateTime.UtcNow;

        _dbContext.BookingHistories.Add(new BookingHistory
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Action = BookingHistoryAction.NoShow,
            PreviousValue = previousStatus,
            NewValue = BookingStatus.NoShow.ToString(),
            ChangedById = request.MarkedById,
            ChangedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} marked as NoShow", booking.Id);

        return MapToResult(booking);
    }

    public async Task<ReservationResultDto> GetReservationStatusAsync(Guid bookingId)
    {
        var booking = await _dbContext.Bookings
            .Include(b => b.Histories)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {bookingId} not found.");

        return MapToResult(booking);
    }

    private static ReservationResultDto MapToResult(Booking booking)
    {
        var auditTrail = booking.Histories
            .OrderBy(h => h.ChangedAt)
            .Select(h => new ReservationAuditEntryDto(
                h.Action.ToString(),
                h.PreviousValue,
                h.NewValue,
                h.ChangedAt
            )).ToList();

        return new ReservationResultDto(
            booking.Id,
            booking.Status.ToString(),
            booking.CancellationReason,
            booking.CancelledAt,
            auditTrail
        );
    }
}
