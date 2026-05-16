using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class BookingGroupService : IBookingGroupService
{
    private readonly IBookingGroupRepository _groupRepository;
    private readonly HotelProDbContext _dbContext;
    private readonly ITenantService _tenantService;

    public BookingGroupService(
        IBookingGroupRepository groupRepository,
        HotelProDbContext dbContext,
        ITenantService tenantService)
    {
        _groupRepository = groupRepository;
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<PagedResult<BookingGroupDto>> GetGroupsAsync(BookingGroupFilter filter)
    {
        return await _groupRepository.GetAllAsync(filter);
    }

    public async Task<BookingGroupDto?> GetGroupByIdAsync(Guid id)
    {
        var group = await _groupRepository.GetByIdWithDetailsAsync(id);
        return group != null ? MapToDto(group) : null;
    }

    public async Task<BookingGroupDto> CreateGroupAsync(CreateBookingGroupDto dto)
    {
        if (dto.Arrival >= dto.Departure)
            throw new InvalidOperationException("Arrival date must be before departure date.");

        if (dto.ReleaseDate.HasValue && dto.ReleaseDate.Value >= dto.Arrival)
            throw new InvalidOperationException("Release date must be before arrival date.");

        if (dto.BlockedRoomCount < 1)
            throw new InvalidOperationException("At least one room must be blocked.");

        var totalRequested = dto.RoomTypes.Sum(r => r.Quantity);
        if (totalRequested != dto.BlockedRoomCount)
            throw new InvalidOperationException("Sum of room type quantities must equal blocked room count.");

        var hotelId = _tenantService.GetCurrentHotelId() ?? Guid.Empty;

        var group = new BookingGroup
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            Name = dto.Name,
            ContactPersonId = dto.ContactPersonId,
            Arrival = dto.Arrival,
            Departure = dto.Departure,
            BlockedRoomCount = dto.BlockedRoomCount,
            ConfirmedRoomCount = 0,
            RatePlanId = dto.RatePlanId,
            DiscountPercent = dto.DiscountPercent,
            ReleaseDate = dto.ReleaseDate,
            Status = GroupStatus.Active,
            UseMasterBill = dto.UseMasterBill,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        foreach (var rt in dto.RoomTypes)
        {
            var roomType = await _dbContext.RoomTypes.FindAsync(rt.RoomTypeId)
                ?? throw new InvalidOperationException($"Room type {rt.RoomTypeId} not found.");

            for (int i = 0; i < rt.Quantity; i++)
            {
                var guest = await _dbContext.Guests.FindAsync(dto.ContactPersonId)
                    ?? throw new InvalidOperationException($"Contact person {dto.ContactPersonId} not found.");

                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    GuestId = dto.ContactPersonId,
                    GroupId = group.Id,
                    Source = BookingSource.Direct,
                    Type = BookingType.Group,
                    Status = BookingStatus.Pending,
                    ArrivalDate = dto.Arrival,
                    DepartureDate = dto.Departure,
                    AdultCount = 1,
                    ChildCount = 0,
                    Notes = $"Group booking: {dto.Name}",
                    Currency = "EUR",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    BookingRooms = new List<BookingRoom>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            RoomTypeId = rt.RoomTypeId,
                            RatePlanId = dto.RatePlanId ?? Guid.Empty,
                            PricePerNight = ApplyDiscount(roomType.DefaultPrice, dto.DiscountPercent),
                            Status = BookingRoomStatus.Blocked
                        }
                    }
                };

                booking.TotalPrice = CalculateBookingTotal(booking);

                var groupBooking = new GroupBooking
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    BookingId = booking.Id,
                    RoomTypeId = rt.RoomTypeId,
                    CreatedAt = DateTime.UtcNow
                };

                group.GroupBookings.Add(groupBooking);
                _dbContext.Bookings.Add(booking);
            }
        }

        if (dto.UseMasterBill && dto.PayerGuestId.HasValue)
        {
            var payer = await _dbContext.Guests.FindAsync(dto.PayerGuestId.Value)
                ?? throw new InvalidOperationException($"Payer guest {dto.PayerGuestId.Value} not found.");

            group.MasterBill = new MasterBill
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                PayerGuestId = dto.PayerGuestId.Value,
                TotalStayCharges = 0,
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        await _groupRepository.AddAsync(group);

        var created = await _groupRepository.GetByIdWithDetailsAsync(group.Id)
            ?? throw new InvalidOperationException("Group not found after creation.");

        return MapToDto(created);
    }

    public async Task<BookingGroupDto> UpdateGroupAsync(Guid id, UpdateBookingGroupDto dto)
    {
        var group = await _groupRepository.GetByIdWithDetailsAsync(id)
            ?? throw new InvalidOperationException($"Group {id} not found.");

        if (group.Status != GroupStatus.Active)
            throw new InvalidOperationException("Cannot modify a non-active group.");

        if (dto.Name != null)
            group.Name = dto.Name;

        if (dto.ContactPersonId.HasValue)
            group.ContactPersonId = dto.ContactPersonId.Value;

        if (dto.Arrival.HasValue && dto.Departure.HasValue)
        {
            if (dto.Arrival.Value >= dto.Departure.Value)
                throw new InvalidOperationException("Arrival must be before departure.");
            group.Arrival = dto.Arrival.Value;
            group.Departure = dto.Departure.Value;
        }
        else if (dto.Arrival.HasValue)
        {
            if (dto.Arrival.Value >= group.Departure)
                throw new InvalidOperationException("Arrival must be before departure.");
            group.Arrival = dto.Arrival.Value;
        }
        else if (dto.Departure.HasValue)
        {
            if (group.Arrival >= dto.Departure.Value)
                throw new InvalidOperationException("Arrival must be before departure.");
            group.Departure = dto.Departure.Value;
        }

        if (dto.ReleaseDate.HasValue)
        {
            if (dto.ReleaseDate.Value >= group.Arrival)
                throw new InvalidOperationException("Release date must be before arrival.");
            group.ReleaseDate = dto.ReleaseDate.Value;
        }

        if (dto.RatePlanId.HasValue)
            group.RatePlanId = dto.RatePlanId.Value;

        if (dto.DiscountPercent.HasValue)
            group.DiscountPercent = dto.DiscountPercent.Value;

        if (dto.UseMasterBill.HasValue)
            group.UseMasterBill = dto.UseMasterBill.Value;

        group.UpdatedAt = DateTime.UtcNow;

        await _groupRepository.UpdateAsync(group);

        var updated = await _groupRepository.GetByIdWithDetailsAsync(group.Id)
            ?? throw new InvalidOperationException("Group not found after update.");

        return MapToDto(updated);
    }

    public async Task<MasterBillDto> GetMasterBillAsync(Guid groupId)
    {
        var group = await _groupRepository.GetByIdWithDetailsAsync(groupId)
            ?? throw new InvalidOperationException($"Group {groupId} not found.");

        if (!group.UseMasterBill)
            throw new InvalidOperationException("Group does not use master billing.");

        if (group.MasterBill == null)
            throw new InvalidOperationException("Master bill not found for this group.");

        var totalCharges = await _dbContext.BookingRooms
            .IgnoreQueryFilters()
            .Where(br => br.Booking.GroupId == groupId && br.Booking.Status != BookingStatus.Cancelled)
            .SumAsync(br => br.PricePerNight * (br.Booking.DepartureDate - br.Booking.ArrivalDate).Days);

        group.MasterBill.TotalStayCharges = totalCharges;
        group.MasterBill.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new MasterBillDto(
            group.MasterBill.Id,
            group.MasterBill.GroupId,
            group.MasterBill.PayerGuestId,
            $"{group.MasterBill.PayerGuest?.FirstName} {group.MasterBill.PayerGuest?.LastName}".Trim(),
            group.MasterBill.TotalStayCharges,
            group.MasterBill.IsClosed,
            group.MasterBill.CreatedAt,
            group.MasterBill.UpdatedAt
        );
    }

    public async Task<int> ReleaseGroupAsync(Guid groupId)
    {
        var group = await _groupRepository.GetByIdWithDetailsAsync(groupId)
            ?? throw new InvalidOperationException($"Group {groupId} not found.");

        if (group.Status != GroupStatus.Active)
            throw new InvalidOperationException("Cannot release a non-active group.");

        var blockedBookings = group.GroupBookings
            .Where(gb => gb.Booking.Status == BookingStatus.Pending)
            .ToList();

        foreach (var gb in blockedBookings)
        {
            foreach (var br in gb.Booking.BookingRooms)
            {
                if (br.Status == BookingRoomStatus.Blocked)
                {
                    br.Status = BookingRoomStatus.Released;
                }
            }
            gb.Booking.Status = BookingStatus.Cancelled;
            gb.Booking.CancellationReason = "Group release date expired";
            gb.Booking.CancelledAt = DateTime.UtcNow;
            gb.Booking.UpdatedAt = DateTime.UtcNow;
        }

        group.Status = GroupStatus.Released;
        group.ConfirmedRoomCount = 0;
        group.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return blockedBookings.Count;
    }

    public async Task<int> ProcessExpiredReleasesAsync()
    {
        var groups = await _groupRepository.GetGroupsPendingReleaseAsync(DateTime.UtcNow);
        var totalReleased = 0;

        foreach (var group in groups)
        {
            var released = await ReleaseGroupAsync(group.Id);
            totalReleased += released;
        }

        return totalReleased;
    }

    private static decimal ApplyDiscount(decimal basePrice, decimal discountPercent)
    {
        if (discountPercent <= 0 || discountPercent >= 100)
            return basePrice;
        return basePrice * (1 - discountPercent / 100);
    }

    private static decimal CalculateBookingTotal(Booking booking)
    {
        var nights = Math.Max(1, (booking.DepartureDate - booking.ArrivalDate).Days);
        return booking.BookingRooms.Sum(r => r.PricePerNight * nights);
    }

    private static BookingGroupDto MapToDto(BookingGroup g)
    {
        var bookings = g.GroupBookings.Select(gb => new BookingGroupBookingDto(
            gb.Booking.Id,
            $"{gb.Booking.Guest?.FirstName} {gb.Booking.Guest?.LastName}".Trim(),
            gb.RoomTypeId,
            gb.RoomType?.Name ?? "",
            gb.Booking.Status.ToString(),
            gb.Booking.ArrivalDate,
            gb.Booking.DepartureDate
        )).ToList();

        MasterBillDto? masterBill = null;
        if (g.MasterBill != null)
        {
            masterBill = new MasterBillDto(
                g.MasterBill.Id,
                g.MasterBill.GroupId,
                g.MasterBill.PayerGuestId,
                $"{g.MasterBill.PayerGuest?.FirstName} {g.MasterBill.PayerGuest?.LastName}".Trim(),
                g.MasterBill.TotalStayCharges,
                g.MasterBill.IsClosed,
                g.MasterBill.CreatedAt,
                g.MasterBill.UpdatedAt
            );
        }

        return new BookingGroupDto(
            g.Id,
            g.HotelId,
            g.Name,
            g.ContactPersonId,
            $"{g.ContactPerson?.FirstName} {g.ContactPerson?.LastName}".Trim(),
            g.Arrival,
            g.Departure,
            g.BlockedRoomCount,
            g.ConfirmedRoomCount,
            g.RatePlanId,
            null,
            g.DiscountPercent,
            g.ReleaseDate,
            g.Status.ToString(),
            g.UseMasterBill,
            g.CreatedAt,
            g.UpdatedAt,
            bookings,
            masterBill
        );
    }
}
