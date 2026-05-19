using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace HotelPro.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly HotelProDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private readonly IBookingAvailabilityService _availabilityService;

    public BookingService(
        IBookingRepository bookingRepository,
        HotelProDbContext dbContext,
        ITenantService tenantService,
        IEmailService emailService,
        ILogger<BookingService> logger,
        IBookingAvailabilityService availabilityService)
    {
        _bookingRepository = bookingRepository;
        _dbContext = dbContext;
        _tenantService = tenantService;
        _emailService = emailService;
        _logger = logger;
        _availabilityService = availabilityService;
    }

    public async Task<PagedResult<BookingDto>> GetBookingsAsync(BookingFilter filter)
    {
        var items = await _bookingRepository.GetAllAsync(
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            filter.GuestId,
            filter.RoomId,
            filter.Page,
            filter.PageSize);

        var totalCount = await _bookingRepository.CountAsync(
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            filter.GuestId,
            filter.RoomId);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<BookingDto>(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var booking = await _bookingRepository.GetByIdWithRoomsAsync(id);
        if (booking == null) return null;
        return MapToDto(booking);
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
    {
        ValidateDates(dto.ArrivalDate, dto.DepartureDate);
        ValidateGuestCount(dto.AdultCount, dto.Type);

        var hotelId = _tenantService.GetCurrentHotelId() ?? Guid.Empty;

        var nights = (dto.DepartureDate - dto.ArrivalDate).Days;
        if (nights <= 0) nights = 1;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            GuestId = dto.GuestId,
            GroupId = dto.GroupId,
            Source = dto.Source,
            Type = dto.Type,
            Status = BookingStatus.Pending,
            ArrivalDate = dto.ArrivalDate,
            DepartureDate = dto.DepartureDate,
            AdultCount = dto.AdultCount,
            ChildCount = dto.ChildCount,
            Notes = dto.Notes,
            InternalNotes = dto.InternalNotes,
            Currency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BookingRooms = dto.Rooms.Select(r => new BookingRoom
            {
                Id = Guid.NewGuid(),
                RoomId = r.RoomId,
                RoomTypeId = r.RoomTypeId,
                RatePlanId = r.RatePlanId,
                PricePerNight = r.PricePerNight,
                Status = BookingRoomStatus.Blocked
            }).ToList()
        };

        booking.TotalPrice = CalculateTotalPrice(booking);
        booking.ExchangeRateTotal = booking.TotalPrice;

        if (_dbContext.Database.IsRelational())
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                var lockTimeoutSql = "SET LOCAL lock_timeout = '5s'";
                await _dbContext.Database.ExecuteSqlRawAsync(lockTimeoutSql);

                foreach (var roomGroup in dto.Rooms.GroupBy(r => r.RoomTypeId))
                {
                    await _bookingRepository.AcquireRoomTypeLockAsync(
                        roomGroup.Key, dto.ArrivalDate, dto.DepartureDate);

                    var conflictingCount = await _bookingRepository.CountConflictingBookingsAsync(
                        roomGroup.Key, dto.ArrivalDate, dto.DepartureDate, null);

                    var totalRoomsOfType = await _dbContext.Rooms
                        .CountAsync(r => r.RoomTypeId == roomGroup.Key);
                    var requestedQuantity = roomGroup.Count();

                    if (conflictingCount + requestedQuantity > totalRoomsOfType)
                    {
                        var allowOverbooking = await _dbContext.FeatureFlags
                            .AnyAsync(f => f.FeatureName == "AllowOverbooking" && f.IsEnabled);

                        if (!allowOverbooking)
                        {
                            await transaction.RollbackAsync();
                            throw new InvalidOperationException(
                                $"Nema dovoljno soba tipa {roomGroup.Key}. Dostupno: {totalRoomsOfType - conflictingCount}, Trazeno: {requestedQuantity}");
                        }
                    }
                }

                await _bookingRepository.AddAsync(booking);
                await transaction.CommitAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "55P03")
            {
                throw new InvalidOperationException(
                    "Soba je trenutno zakljucana od strane drugog korisnika. Pokusajte ponovo.");
            }
            catch (PostgresException ex) when (ex.SqlState == "57014")
            {
                throw new InvalidOperationException(
                    "Vremensko ogranicenje za zakljucavanje sobe je isteklo (5s). Pokusajte ponovo.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        else
        {
            await _bookingRepository.AddAsync(booking);
        }

        var result = await _bookingRepository.GetByIdWithRoomsAsync(booking.Id)
            ?? throw new InvalidOperationException($"Booking with ID {booking.Id} not found after creation.");

        return MapToDto(result);
    }

    public async Task<BookingDto> UpdateBookingAsync(Guid id, UpdateBookingDto dto)
    {
        var booking = await _bookingRepository.GetByIdWithRoomsAsync(id)
            ?? throw new InvalidOperationException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled booking.");

        if (dto.ArrivalDate.HasValue && dto.DepartureDate.HasValue)
        {
            ValidateDates(dto.ArrivalDate.Value, dto.DepartureDate.Value);
            booking.ArrivalDate = dto.ArrivalDate.Value;
            booking.DepartureDate = dto.DepartureDate.Value;
        }
        else if (dto.ArrivalDate.HasValue)
        {
            ValidateDates(dto.ArrivalDate.Value, booking.DepartureDate);
            booking.ArrivalDate = dto.ArrivalDate.Value;
        }
        else if (dto.DepartureDate.HasValue)
        {
            ValidateDates(booking.ArrivalDate, dto.DepartureDate.Value);
            booking.DepartureDate = dto.DepartureDate.Value;
        }

        if (dto.GuestId.HasValue)
            booking.GuestId = dto.GuestId.Value;

        if (dto.GroupId.HasValue)
            booking.GroupId = dto.GroupId;
        else if (dto.GroupId == Guid.Empty)
            booking.GroupId = null;

        if (dto.Source.HasValue)
            booking.Source = dto.Source.Value;

        if (dto.Type.HasValue)
        {
            booking.Type = dto.Type.Value;
            ValidateGuestCount(booking.AdultCount, booking.Type);
        }

        if (dto.AdultCount.HasValue)
        {
            booking.AdultCount = dto.AdultCount.Value;
            ValidateGuestCount(booking.AdultCount, booking.Type);
        }

        if (dto.ChildCount.HasValue)
            booking.ChildCount = dto.ChildCount.Value;

        if (dto.Notes != null)
            booking.Notes = dto.Notes;

        if (dto.InternalNotes != null)
            booking.InternalNotes = dto.InternalNotes;

        if (dto.Rooms != null && dto.Rooms.Count > 0)
        {
            _dbContext.BookingRooms.RemoveRange(booking.BookingRooms);

            booking.BookingRooms = dto.Rooms.Select(r => new BookingRoom
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                RoomId = r.RoomId,
                RoomTypeId = r.RoomTypeId,
                RatePlanId = r.RatePlanId,
                PricePerNight = r.PricePerNight,
                Status = BookingRoomStatus.Blocked
            }).ToList();
        }

        booking.TotalPrice = CalculateTotalPrice(booking);
        booking.ExchangeRateTotal = booking.TotalPrice;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.UpdateAsync(booking);

        var result = await _bookingRepository.GetByIdWithRoomsAsync(booking.Id)
            ?? throw new InvalidOperationException($"Booking with ID {booking.Id} not found after update.");

        return MapToDto(result);
    }

    public async Task<BookingDto> AssignRoomAsync(Guid id, AssignRoomDto dto)
    {
        var booking = await _bookingRepository.GetByIdWithRoomsAsync(id)
            ?? throw new InvalidOperationException($"Booking {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign room to a cancelled booking.");
        if (booking.Status == BookingStatus.CheckedOut)
            throw new InvalidOperationException("Cannot assign room to a checked-out booking.");

        if (dto.RoomId.HasValue && dto.RoomId.Value != Guid.Empty)
        {
            // Provjeri postoji li soba
            var roomExists = await _dbContext.Rooms.AnyAsync(r => r.Id == dto.RoomId.Value);
            if (!roomExists)
                throw new InvalidOperationException($"Room {dto.RoomId.Value} not found.");

            // Provjeri kolizije: drugi bookings koji imaju ovu sobu u istom periodu
            var conflictingBookingIds = await _dbContext.Bookings
                .Where(b =>
                    b.Id != id &&
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.NoShow &&
                    b.ArrivalDate < booking.DepartureDate &&
                    b.DepartureDate > booking.ArrivalDate)
                .Select(b => b.Id)
                .ToListAsync();

            var hasConflict = conflictingBookingIds.Count > 0 &&
                await _dbContext.BookingRooms
                    .AnyAsync(br =>
                        br.RoomId == dto.RoomId.Value &&
                        conflictingBookingIds.Contains(br.BookingId));

            if (hasConflict)
                throw new InvalidOperationException("Soba je zauzeta u tom terminu.");

            // Dodijeli sobu — za group bookings, dodijeli prvoj nedodjeljenoj BookingRoom
            // Ako sve imaju sobe, ažuriraj prvu
            var targetBookingRoom = booking.BookingRooms
                .FirstOrDefault(br => br.RoomId == null)
                ?? booking.BookingRooms.FirstOrDefault();

            if (targetBookingRoom != null)
                targetBookingRoom.RoomId = dto.RoomId.Value;
        }
        else
        {
            // Oslobodi sobu — ukloni RoomId sa svih BookingRooms
            foreach (var br in booking.BookingRooms)
                br.RoomId = null;
        }

        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        var result = await _bookingRepository.GetByIdWithRoomsAsync(id)
            ?? throw new InvalidOperationException($"Booking {id} not found after update.");

        return MapToDto(result);
    }

    public async Task DeleteBookingAsync(Guid id)    {
        var booking = await _bookingRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be deleted.");

        await _bookingRepository.DeleteAsync(booking);
    }

    public async Task UpdateBookingStatusAsync(Guid id, BookingStatus newStatus)
    {
        var booking = await _bookingRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Booking with ID {id} not found.");

        if (!IsValidStatusTransition(booking.Status, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {booking.Status} to {newStatus}.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Cannot change status of a cancelled booking.");

        booking.Status = newStatus;
        booking.UpdatedAt = DateTime.UtcNow;

        if (newStatus == BookingStatus.Cancelled)
        {
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason ??= "Manual cancellation";
        }

        await _bookingRepository.UpdateAsync(booking);

        if (newStatus == BookingStatus.Confirmed)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendConfirmationAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email for booking {BookingId}", id);
                }
            });
        }
        else if (newStatus == BookingStatus.Cancelled)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendCancellationAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send cancellation email for booking {BookingId}", id);
                }
            });
        }
    }

    private static bool IsValidStatusTransition(BookingStatus from, BookingStatus to)
    {
        return (from, to) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.CheckedIn) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.CheckedIn, BookingStatus.CheckedOut) => true,
            _ => false
        };
    }

    private static void ValidateDates(DateTime arrival, DateTime departure)
    {
        if (arrival >= departure)
            throw new InvalidOperationException("Arrival date must be before departure date.");
    }

    private static void ValidateGuestCount(int adults, BookingType type)
    {
        if (type != BookingType.Complementary && adults < 1)
            throw new InvalidOperationException("At least one adult is required.");
    }

    private static decimal CalculateTotalPrice(Booking booking)
    {
        var nights = Math.Max(1, (booking.DepartureDate - booking.ArrivalDate).Days);
        return booking.BookingRooms.Sum(r => r.PricePerNight * nights);
    }

    private static BookingDto MapToDto(Booking b)
    {
        var nights = Math.Max(1, (b.DepartureDate - b.ArrivalDate).Days);

        return new BookingDto(
            b.Id,
            b.HotelId,
            b.GuestId,
            $"{b.Guest?.FirstName} {b.Guest?.LastName}".Trim(),
            b.GroupId,
            b.Source.ToString(),
            b.Type.ToString(),
            b.Status.ToString(),
            b.ArrivalDate,
            b.DepartureDate,
            b.AdultCount,
            b.ChildCount,
            nights,
            b.TotalPrice,
            b.ExchangeRateTotal,
            b.Currency,
            b.Notes,
            b.InternalNotes,
            b.CancellationReason,
            b.CancelledAt,
            b.CreatedAt,
            b.UpdatedAt,
            b.BookingRooms.Select(r => new BookingRoomDto(
                r.Id,
                r.BookingId,
                r.RoomId,
                r.Room?.RoomNumber,
                r.RoomTypeId,
                r.RoomType?.Name ?? "",
                r.RatePlanId,
                r.PricePerNight,
                r.Status.ToString()
            )).ToList()
        );
    }
}
