using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class CheckInService : ICheckInService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IFolioService _folioService;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(
        HotelProDbContext dbContext,
        IFolioService folioService,
        ILogger<CheckInService> logger)
    {
        _dbContext = dbContext;
        _folioService = folioService;
        _logger = logger;
    }

    public async Task<CheckInResponse> CheckInAsync(CheckInRequest request)
    {
        var warnings = new List<string>();

        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.BookingRooms)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Booking must be in Confirmed status. Current status: {booking.Status}");

        var room = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId);

        if (room == null)
            throw new InvalidOperationException($"Room {request.RoomId} not found.");

        if (room.Status != RoomStatus.Free && room.Status != RoomStatus.Reserved)
            throw new InvalidOperationException($"Room must be Free or Reserved. Current status: {room.Status}");

        var bookingRoom = booking.BookingRooms.FirstOrDefault();
        if (bookingRoom == null)
            throw new InvalidOperationException("No booking room found for this booking.");

        bookingRoom.RoomId = request.RoomId;
        bookingRoom.Status = BookingRoomStatus.Occupied;

        booking.Status = BookingStatus.CheckedIn;
        booking.UpdatedAt = DateTime.UtcNow;

        room.Status = RoomStatus.Occupied;

        var folioDto = await _folioService.CreateFolioAsync(new CreateFolioDto(
            BookingId: booking.Id,
            GuestId: booking.GuestId,
            Notes: $"Folio created at check-in for room {room.RoomNumber}"
        ));

        if (request.GuestDocuments != null)
        {
            foreach (var doc in request.GuestDocuments)
            {
                if (doc.ExpiryDate.HasValue && doc.ExpiryDate.Value < DateTime.UtcNow)
                {
                    warnings.Add($"Document {doc.DocumentType} ({doc.DocumentNumber}) has expired.");
                }

                var guestDoc = new GuestDocument
                {
                    Id = Guid.NewGuid(),
                    GuestId = booking.GuestId,
                    DocumentType = doc.DocumentType,
                    DocumentNumber = doc.DocumentNumber,
                    ExpiryDate = doc.ExpiryDate,
                };
                _dbContext.GuestDocuments.Add(guestDoc);
            }
        }

        string? rfidEncodeUrl = null;
        if (!string.IsNullOrEmpty(request.RfidCardCode))
        {
            rfidEncodeUrl = $"/api/hardware/rfid/encode?code={request.RfidCardCode}&room={room.RoomNumber}";
            _logger.LogInformation("RFID card encode requested for booking {BookingId}, room {RoomNumber}, code {RfidCode}",
                booking.Id, room.RoomNumber, request.RfidCardCode);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Check-in completed for booking {BookingId}, room {RoomNumber}", booking.Id, room.RoomNumber);

        return new CheckInResponse(
            booking.Id,
            room.Id,
            room.RoomNumber,
            Guid.Parse(folioDto.Id.ToString()),
            folioDto.FolioNumber,
            warnings,
            rfidEncodeUrl
        );
    }
}
