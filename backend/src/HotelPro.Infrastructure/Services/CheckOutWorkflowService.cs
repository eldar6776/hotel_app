using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class CheckOutWorkflowService : ICheckOutWorkflowService
{
    private readonly HotelProDbContext _dbContext;
    private readonly INightLedgerService _nightLedgerService;
    private readonly IRoomOccupancyPolicy _roomOccupancyPolicy;
    private readonly ILogger<CheckOutWorkflowService> _logger;

    public CheckOutWorkflowService(
        HotelProDbContext dbContext,
        INightLedgerService nightLedgerService,
        IRoomOccupancyPolicy roomOccupancyPolicy,
        ILogger<CheckOutWorkflowService> logger)
    {
        _dbContext = dbContext;
        _nightLedgerService = nightLedgerService;
        _roomOccupancyPolicy = roomOccupancyPolicy;
        _logger = logger;
    }

    public async Task<CheckOutWorkflowResponse> FullCheckOutAsync(FullCheckOutRequest request)
    {
        var now = DateTime.UtcNow;

        var activeStays = await _dbContext.Stays
            .Include(s => s.Guest)
            .Include(s => s.Room)
            .Where(s => s.RoomId == request.RoomId && !s.IsCheckedOut)
            .ToListAsync();

        if (activeStays.Count == 0)
            throw new InvalidOperationException($"No active stays found for room {request.RoomId}.");

        var room = activeStays[0].Room;
        var folioId = activeStays[0].FolioId;

        int nightsClosed = 0;
        foreach (var stay in activeStays)
        {
            stay.IsCheckedOut = true;
            stay.CheckedOutAt = now;
            stay.CheckedOutBy = request.CheckedOutBy;

            var closed = await _nightLedgerService.CloseNightsForStayAsync(stay.Id, now);
            nightsClosed += closed;

            _dbContext.GuestStayHistories.Add(new GuestStayHistory
            {
                Id = Guid.NewGuid(),
                GuestId = stay.GuestId,
                BookingId = stay.BookingId ?? Guid.Empty,
                RoomId = stay.RoomId,
                CheckedInAt = stay.CheckInDate,
                CheckedOutAt = now,
                RoomNumber = room.RoomNumber,
                CreatedAt = now
            });
        }

        var expensesLocked = 0;
        if (folioId.HasValue)
        {
            var openCharges = await _dbContext.Charges
                .Where(c => c.FolioId == folioId.Value)
                .ToListAsync();

            expensesLocked = openCharges.Count;

            var folio = await _dbContext.Folios.FindAsync(folioId.Value);
            if (folio != null)
            {
                folio.Status = FolioStatus.Closed;
                folio.ClosedAt = now;
                folio.UpdatedAt = now;
            }
        }

        var bookingIds = activeStays
            .Where(s => s.BookingId.HasValue)
            .Select(s => s.BookingId!.Value)
            .Distinct()
            .ToList();

        foreach (var bookingId in bookingIds)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.Status = BookingStatus.CheckedOut;
                booking.UpdatedAt = now;
            }
        }

        var bookingRooms = await _dbContext.BookingRooms
            .Where(br => br.RoomId == request.RoomId && br.Status == BookingRoomStatus.Occupied)
            .ToListAsync();

        foreach (var br in bookingRooms)
        {
            br.Status = BookingRoomStatus.Released;
        }

        var roomStatus = await _roomOccupancyPolicy.GetRoomStatusAsync(room.Id, now);
        room.Status = RoomStatus.Dirty;

        var hasUnpaid = false;
        var outstandingAmount = 0m;
        if (folioId.HasValue)
        {
            var folio = await _dbContext.Folios.FindAsync(folioId.Value);
            if (folio != null && folio.Balance > 0)
            {
                hasUnpaid = true;
                outstandingAmount = folio.Balance;

                if (request.CreateUnpaidRecords)
                {
                    _dbContext.OutstandingBalances.Add(new OutstandingBalance
                    {
                        Id = Guid.NewGuid(),
                        FolioId = folioId.Value,
                        Date = now,
                        Balance = outstandingAmount,
                        IsOverdue = false
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Full checkout: room {RoomNumber}, {GuestCount} guests, {NightsClosed} nights closed, folio {FolioClosed}",
            room.RoomNumber, activeStays.Count, nightsClosed, folioId.HasValue);

        return new CheckOutWorkflowResponse(
            request.RoomId,
            room.RoomNumber,
            folioId ?? Guid.Empty,
            folioId.HasValue ? (await _dbContext.Folios.FindAsync(folioId.Value))?.FolioNumber ?? "" : "",
            activeStays.Count,
            nightsClosed,
            expensesLocked,
            folioId.HasValue,
            hasUnpaid,
            outstandingAmount
        );
    }

    public async Task<PartialCheckOutResponse> PartialCheckOutAsync(PartialCheckOutRequest request)
    {
        var now = DateTime.UtcNow;

        var stay = await _dbContext.Stays
            .Include(s => s.Guest)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == request.StayId && !s.IsCheckedOut);

        if (stay == null)
            throw new InvalidOperationException($"Active stay {request.StayId} not found.");

        var otherActiveStays = await _dbContext.Stays
            .Where(s => s.RoomId == stay.RoomId && !s.IsCheckedOut && s.Id != stay.Id)
            .CountAsync();

        if (otherActiveStays == 0)
            throw new InvalidOperationException("Cannot perform partial checkout: only one guest remains in the room. Use full checkout instead.");

        var nightsClosedForGuest = await _nightLedgerService.CloseNightsForStayAsync(stay.Id, now);

        stay.IsCheckedOut = true;
        stay.CheckedOutAt = now;
        stay.CheckedOutBy = request.CheckedOutBy;

        _dbContext.GuestStayHistories.Add(new GuestStayHistory
        {
            Id = Guid.NewGuid(),
            GuestId = stay.GuestId,
            BookingId = stay.BookingId ?? Guid.Empty,
            RoomId = stay.RoomId,
            CheckedInAt = stay.CheckInDate,
            CheckedOutAt = now,
            RoomNumber = stay.Room.RoomNumber,
            CreatedAt = now
        });

        var nightsCreatedForRemaining = 0;
        if (stay.FolioId.HasValue && otherActiveStays > 0)
        {
            var activeNights = await _dbContext.StayNights
                .Where(n => n.FolioId == stay.FolioId.Value
                    && n.RoomId == stay.RoomId
                    && n.Status == NightStatus.Active
                    && n.StayId != stay.Id)
                .ToListAsync();

            foreach (var night in activeNights)
            {
                night.TariffAmount = night.TariffAmount / otherActiveStays > 0
                    ? stay.Room.BasePrice ?? night.TariffAmount
                    : night.TariffAmount;
            }

            nightsCreatedForRemaining = activeNights.Count;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Partial checkout: stay {StayId}, guest {GuestName}, {NightsClosed} nights closed",
            stay.Id, $"{stay.Guest.FirstName} {stay.Guest.LastName}".Trim(), nightsClosedForGuest);

        return new PartialCheckOutResponse(
            stay.Id,
            stay.GuestId,
            $"{stay.Guest.FirstName} {stay.Guest.LastName}".Trim(),
            stay.FolioId ?? Guid.Empty,
            nightsClosedForGuest,
            nightsCreatedForRemaining,
            true,
            otherActiveStays
        );
    }
}
