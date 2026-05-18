using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelPro.Infrastructure.Services;

public class StayLifecycleService : IStayLifecycleService
{
    private readonly HotelProDbContext _dbContext;
    private readonly IRoomOccupancyPolicy _roomOccupancyPolicy;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<StayLifecycleService> _logger;

    public StayLifecycleService(
        HotelProDbContext dbContext,
        IRoomOccupancyPolicy roomOccupancyPolicy,
        IConfigurationService configurationService,
        ILogger<StayLifecycleService> logger)
    {
        _dbContext = dbContext;
        _roomOccupancyPolicy = roomOccupancyPolicy;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<StayCheckInResponse> CheckInAsync(StayCheckInRequest request)
    {
        var warnings = new List<string>();
        var checkInDate = request.CheckInDate ?? DateTime.UtcNow;
        var checkOutDate = request.CheckOutDate ?? checkInDate.AddDays(1);

        if (checkOutDate <= checkInDate)
            throw new InvalidOperationException("Check-out date must be after check-in date.");

        if (request.Guests == null || request.Guests.Count == 0)
            throw new InvalidOperationException("At least one guest is required for check-in.");

        var room = await _dbContext.Rooms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId);

        if (room == null)
            throw new InvalidOperationException($"Room {request.RoomId} not found.");

        var roomStatus = await _roomOccupancyPolicy.GetRoomStatusAsync(room.Id, checkInDate);
        if (roomStatus.Status != RoomStatus.Free && roomStatus.Status != RoomStatus.Reserved
            && roomStatus.Status != RoomStatus.ReservedConfirmed && roomStatus.Status != RoomStatus.ReservedUnconfirmed)
        {
            throw new InvalidOperationException(
                $"Room must be Free or Reserved for check-in. Current status: {roomStatus.Status} ({roomStatus.Reason})");
        }

        var duplicateGuestIds = request.Guests
            .Select(g => g.GuestId)
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateGuestIds.Any())
            throw new InvalidOperationException($"Duplicate guests detected: {string.Join(", ", duplicateGuestIds)}");

        var existingActiveStays = await _dbContext.Stays
            .Where(s => s.RoomId == request.RoomId && !s.IsCheckedOut)
            .Select(s => s.GuestId)
            .ToListAsync();

        var alreadyInRoom = request.Guests
            .Select(g => g.GuestId)
            .Intersect(existingActiveStays)
            .ToList();

        if (alreadyInRoom.Any())
            throw new InvalidOperationException(
                $"Guests already checked into this room: {string.Join(", ", alreadyInRoom)}");

        var openFolio = await _dbContext.Folios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.BookingRoomId == null
                && _dbContext.Stays.Any(s => s.RoomId == request.RoomId && !s.IsCheckedOut && s.FolioId == f.Id && f.Status == FolioStatus.Open));

        if (openFolio == null)
        {
            var activeStays = await _dbContext.Stays
                .Where(s => s.RoomId == request.RoomId && !s.IsCheckedOut)
                .FirstOrDefaultAsync();

            if (activeStays != null && activeStays.FolioId.HasValue)
            {
                openFolio = await _dbContext.Folios
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(f => f.Id == activeStays.FolioId.Value && f.Status == FolioStatus.Open);
            }
        }

        Folio folio;
        if (openFolio != null)
        {
            folio = openFolio;
            _logger.LogInformation("Reusing existing open folio {FolioId} for room {RoomId}", folio.Id, room.Id);
        }
        else
        {
            folio = new Folio
            {
                Id = Guid.NewGuid(),
                FolioNumber = $"F-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                Status = FolioStatus.Open,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                Notes = $"Folio created at check-in for room {room.RoomNumber}"
            };
            _dbContext.Folios.Add(folio);
        }

        var billingMode = await GetBillingModeAsync();
        var touristTax = await GetConfigValueAsync("tourist_tax_amount", 0m);
        var insurance = await GetConfigValueAsync("insurance_amount", 0m);
        var childAgeThreshold = await GetConfigValueAsync("child_age_threshold", 0m);
        var childDiscountPercent = await GetConfigValueAsync("child_discount_percent", 0m);

        var guestCount = request.Guests.Count;
        var tariff = room.BasePrice ?? 0m;

        var stayResults = new List<StayGuestResult>();
        var stays = new List<Stay>();

        foreach (var guestEntry in request.Guests)
        {
            var guest = await _dbContext.Guests.FindAsync(guestEntry.GuestId);
            if (guest == null)
                throw new InvalidOperationException($"Guest {guestEntry.GuestId} not found.");

            var category = guestEntry.GuestCategory;
            if (category == GuestCategory.Unknown && guest.DateOfBirth.HasValue)
            {
                var age = CalculateAge(guest.DateOfBirth.Value, checkInDate);
                category = age < 12 ? GuestCategory.Child
                    : age < 18 ? GuestCategory.Minor
                    : GuestCategory.Adult;
            }

            var discountPercent = guestEntry.DiscountPercent;
            var discountReason = guestEntry.DiscountReason;
            if (discountPercent == 0 && category == GuestCategory.Child && childAgeThreshold > 0)
            {
                var age = guest.DateOfBirth.HasValue ? CalculateAge(guest.DateOfBirth.Value, checkInDate) : 0;
                if (age > 0 && age < childAgeThreshold)
                {
                    discountPercent = childDiscountPercent;
                    discountReason = $"Osoba ima {age}. godina";
                }
            }

            var stay = new Stay
            {
                Id = Guid.NewGuid(),
                GuestId = guestEntry.GuestId,
                RoomId = request.RoomId,
                FolioId = folio.Id,
                BookingId = request.BookingId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                CheckedInBy = request.CheckedInBy,
                IsCheckedOut = false,
                IsRegistrationPrinted = false,
                IsReservationLink = false,
                IsFromConfirmedReservation = false,
                IsAccommodationPaid = false,
                GuestCategory = category,
                DiscountPercent = discountPercent,
                DiscountReason = discountReason,
                TaxOverride = guestEntry.TaxOverride,
                StayNote = guestEntry.StayNote,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Stays.Add(stay);
            stays.Add(stay);

            if (guestEntry.Documents != null)
            {
                foreach (var doc in guestEntry.Documents)
                {
                    if (doc.ExpiryDate.HasValue && doc.ExpiryDate.Value < DateTime.UtcNow)
                    {
                        warnings.Add($"Document {doc.DocumentType} ({doc.DocumentNumber}) for guest {guest.LastName} has expired.");
                    }

                    _dbContext.GuestDocuments.Add(new GuestDocument
                    {
                        Id = Guid.NewGuid(),
                        GuestId = guestEntry.GuestId,
                        DocumentType = Enum.TryParse<DocumentType>(doc.DocumentType, true, out var dt) ? dt : DocumentType.Other,
                        DocumentNumber = doc.DocumentNumber,
                        ExpiryDate = doc.ExpiryDate,
                    });
                }
            }

            _dbContext.GuestStayHistories.Add(new GuestStayHistory
            {
                Id = Guid.NewGuid(),
                GuestId = guestEntry.GuestId,
                BookingId = request.BookingId ?? Guid.Empty,
                RoomId = room.Id,
                CheckedInAt = checkInDate,
                RoomNumber = room.RoomNumber,
                CreatedAt = DateTime.UtcNow
            });

            var guestTariff = billingMode == BillingMode.SplitPerPerson && guestCount > 0
                ? tariff / guestCount
                : tariff;

            if (guestTariff == 0 && (touristTax > 0 || insurance > 0))
                guestTariff = touristTax + insurance;

            var nightsCount = (int)(checkOutDate.Date - checkInDate.Date).TotalDays;
            if (nightsCount < 1) nightsCount = 1;

            for (var i = 0; i < nightsCount; i++)
            {
                var nightDate = checkInDate.Date.AddDays(i);
                var nightPrice = guestTariff;
                if (discountPercent > 0)
                    nightPrice = nightPrice * (1 - discountPercent / 100m);

                _dbContext.StayNights.Add(new StayNight
                {
                    Id = Guid.NewGuid(),
                    FolioId = folio.Id,
                    StayId = stay.Id,
                    RoomId = room.Id,
                    Date = nightDate,
                    TariffAmount = nightPrice,
                    DiscountPercent = discountPercent,
                    Status = NightStatus.Active,
                    IsComp = false,
                    Description = discountPercent > 0 ? $"Accommodation ({discountPercent}% discount)" : "Accommodation",
                    ClosedAt = null
                });

                folio.Balance += nightPrice;
            }

            stayResults.Add(new StayGuestResult(
                stay.Id,
                guestEntry.GuestId,
                $"{guest.FirstName} {guest.LastName}".Trim(),
                category,
                discountPercent,
                nightsCount
            ));
        }

        if (request.BookingId.HasValue)
        {
            var booking = await _dbContext.Bookings.FindAsync(request.BookingId.Value);
            if (booking != null)
            {
                booking.Status = BookingStatus.CheckedIn;
                booking.UpdatedAt = DateTime.UtcNow;
            }
        }

        var updatedStatus = await _roomOccupancyPolicy.GetRoomStatusAsync(room.Id, checkInDate);
        room.Status = updatedStatus.Status;

        folio.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Check-in completed for room {RoomNumber}: {GuestCount} guests, folio {FolioNumber}",
            room.RoomNumber, request.Guests.Count, folio.FolioNumber);

        return new StayCheckInResponse(
            room.Id,
            room.RoomNumber,
            folio.Id,
            folio.FolioNumber,
            stayResults,
            warnings
        );
    }

    public async Task<StayCheckInResponse> CheckInFromReservationAsync(ReservationCheckInRequest request)
    {
        var booking = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.BookingRooms)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
            throw new InvalidOperationException($"Booking {request.BookingId} not found.");

        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Booking must be Confirmed or Pending. Current: {booking.Status}");

        var bookingRoom = booking.BookingRooms.FirstOrDefault();
        if (bookingRoom == null || !bookingRoom.RoomId.HasValue)
            throw new InvalidOperationException("No room assigned to this booking.");

        var guestEntries = new List<StayGuestEntry>
        {
            new(
                GuestId: booking.GuestId,
                GuestCategory: GuestCategory.Unknown,
                DiscountPercent: 0,
                StayNote: "Checked in from reservation"
            )
        };

        for (var i = 1; i < booking.AdultCount + booking.ChildCount; i++)
        {
            guestEntries.Add(new StayGuestEntry(
                GuestId: booking.GuestId,
                GuestCategory: i < booking.AdultCount ? GuestCategory.Adult : GuestCategory.Child,
                DiscountPercent: 0,
                StayNote: "Additional guest from reservation"
            ));
        }

        var checkInRequest = new StayCheckInRequest(
            RoomId: bookingRoom.RoomId.Value,
            Guests: guestEntries,
            CheckInDate: booking.ArrivalDate,
            CheckOutDate: booking.DepartureDate,
            BookingId: booking.Id,
            CheckedInBy: request.CheckedInBy
        );

        var result = await CheckInAsync(checkInRequest);

        if (bookingRoom != null)
        {
            bookingRoom.Status = BookingRoomStatus.Occupied;
        }

        await _dbContext.SaveChangesAsync();

        return result;
    }

    public async Task<IEnumerable<StayDto>> GetActiveStaysForRoomAsync(Guid roomId)
    {
        var stays = await _dbContext.Stays
            .Include(s => s.Guest)
            .Include(s => s.Room)
            .Where(s => s.RoomId == roomId && !s.IsCheckedOut)
            .OrderBy(s => s.CheckInDate)
            .ToListAsync();

        return stays.Select(s => new StayDto(
            s.Id,
            s.GuestId,
            $"{s.Guest.FirstName} {s.Guest.LastName}".Trim(),
            s.RoomId,
            s.Room.RoomNumber,
            s.FolioId,
            s.CheckInDate,
            s.CheckOutDate,
            s.IsCheckedOut,
            s.GuestCategory,
            s.DiscountPercent,
            s.DiscountReason
        ));
    }

    public async Task<StayDto> GetStayAsync(Guid stayId)
    {
        var stay = await _dbContext.Stays
            .Include(s => s.Guest)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == stayId);

        if (stay == null)
            throw new InvalidOperationException($"Stay {stayId} not found.");

        return new StayDto(
            stay.Id,
            stay.GuestId,
            $"{stay.Guest.FirstName} {stay.Guest.LastName}".Trim(),
            stay.RoomId,
            stay.Room.RoomNumber,
            stay.FolioId,
            stay.CheckInDate,
            stay.CheckOutDate,
            stay.IsCheckedOut,
            stay.GuestCategory,
            stay.DiscountPercent,
            stay.DiscountReason
        );
    }

    private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (birthDate.Date > referenceDate.AddYears(-age)) age--;
        return age;
    }

    private async Task<BillingMode> GetBillingModeAsync()
    {
        var value = await _configurationService.GetValueAsync("billing_mode");
        if (int.TryParse(value, out var mode))
            return (BillingMode)mode;
        return BillingMode.SplitPerPerson;
    }

    private async Task<decimal> GetConfigValueAsync(string key, decimal defaultValue)
    {
        var value = await _configurationService.GetValueAsync(key);
        if (decimal.TryParse(value, out var result))
            return result;
        return defaultValue;
    }
}
