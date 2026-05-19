using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Services;

public class GuestService : IGuestService
{
    private readonly HotelProDbContext _dbContext;

    public GuestService(HotelProDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<GuestDto>> GetGuestsAsync(GuestFilter filter)
    {
        var query = _dbContext.Guests
            .Include(g => g.Country)
            .Include(g => g.Documents)
            .Where(g => g.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var q = filter.Search.ToLower();
            query = query.Where(g =>
                g.FirstName.ToLower().Contains(q) ||
                g.LastName.ToLower().Contains(q) ||
                (g.Email != null && g.Email.ToLower().Contains(q)) ||
                (g.Phone != null && g.Phone.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
        {
            query = query.Where(g =>
                g.Documents.Any(d => d.DocumentNumber.Contains(filter.DocumentNumber)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<GuestDto>(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<GuestDto?> GetGuestByIdAsync(Guid id)
    {
        var guest = await _dbContext.Guests
            .Include(g => g.Country)
            .Include(g => g.Documents)
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

        if (guest == null) return null;

        await LogPrivacyAccessAsync(guest.Id);
        return MapToDto(guest);
    }

    public async Task<GuestDto> CreateGuestAsync(CreateGuestDto dto)
    {
        if (!dto.GdprConsentGiven)
            throw new InvalidOperationException("GDPR consent is required to create a guest.");

        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            CountryId = dto.CountryId,
            NationalityCountryId = dto.NationalityCountryId,
            IsCompany = dto.IsCompany,
            CompanyName = dto.CompanyName,
            VatNumber = dto.VatNumber,
            GdprConsentGiven = dto.GdprConsentGiven,
            GdprConsentDate = DateTime.UtcNow,
            GdprConsentVersion = dto.GdprConsentVersion ?? "1.0",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Guests.Add(guest);
        await _dbContext.SaveChangesAsync();

        return MapToDto(guest);
    }

    public async Task<GuestDto> UpdateGuestAsync(Guid id, UpdateGuestDto dto)
    {
        var guest = await _dbContext.Guests.FindAsync(id)
            ?? throw new InvalidOperationException($"Guest {id} not found.");

        if (dto.FirstName != null) guest.FirstName = dto.FirstName;
        if (dto.LastName != null) guest.LastName = dto.LastName;
        if (dto.DateOfBirth.HasValue) guest.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) guest.Gender = dto.Gender;
        if (dto.Email != null) guest.Email = dto.Email;
        if (dto.Phone != null) guest.Phone = dto.Phone;
        if (dto.Address != null) guest.Address = dto.Address;
        if (dto.City != null) guest.City = dto.City;
        if (dto.PostalCode != null) guest.PostalCode = dto.PostalCode;
        if (dto.CountryId.HasValue) guest.CountryId = dto.CountryId;
        if (dto.NationalityCountryId.HasValue) guest.NationalityCountryId = dto.NationalityCountryId;
        if (dto.IsCompany.HasValue) guest.IsCompany = dto.IsCompany.Value;
        if (dto.CompanyName != null) guest.CompanyName = dto.CompanyName;
        if (dto.VatNumber != null) guest.VatNumber = dto.VatNumber;
        if (dto.GdprConsentGiven.HasValue)
        {
            guest.GdprConsentGiven = dto.GdprConsentGiven.Value;
            if (dto.GdprConsentGiven.Value)
                guest.GdprConsentDate = DateTime.UtcNow;
        }
        if (dto.GdprConsentVersion != null) guest.GdprConsentVersion = dto.GdprConsentVersion;
        if (dto.Notes != null) guest.Notes = dto.Notes;

        guest.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(guest);
    }

    public async Task DeleteGuestAsync(Guid id)
    {
        var guest = await _dbContext.Guests.FindAsync(id)
            ?? throw new InvalidOperationException($"Guest {id} not found.");

        var activeBookings = await _dbContext.Bookings
            .AnyAsync(b => b.GuestId == id && b.Status == BookingStatus.CheckedIn);

        if (activeBookings)
            throw new InvalidOperationException("Cannot delete guest with active bookings.");

        guest.IsActive = false;
        guest.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<GuestAutoSuggestDto>> SearchGuestsAsync(string query, int limit = 10)
    {
        var q = (query ?? "").ToLower();
        return await _dbContext.Guests
            .Where(g => g.IsActive &&
                (g.FirstName.ToLower().Contains(q) ||
                 g.LastName.ToLower().Contains(q) ||
                 (g.Phone != null && g.Phone.Contains(q))))
            .OrderBy(g => g.LastName)
            .Take(limit)
            .Select(g => new GuestAutoSuggestDto(g.Id, g.FirstName, g.LastName, g.Phone))
            .ToListAsync();
    }

    public async Task<GuestProfileDto?> GetGuestProfileAsync(Guid id)
    {
        var guest = await _dbContext.Guests
            .Include(g => g.Country)
            .Include(g => g.Documents)
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

        if (guest == null) return null;

        var stayHistory = await _dbContext.Set<GuestStayHistory>()
            .IgnoreQueryFilters()
            .Where(s => s.GuestId == id)
            .OrderByDescending(s => s.CheckedInAt)
            .Select(s => new GuestStaySummaryDto(
                s.BookingId,
                s.CheckedInAt,
                s.CheckedOutAt,
                s.RoomNumber,
                s.CheckedOutAt.HasValue
                    ? (s.CheckedOutAt.Value - s.CheckedInAt).Days
                    : (int)(DateTime.UtcNow - s.CheckedInAt).TotalDays))
            .ToListAsync();

        var bookingHistory = await _dbContext.Bookings
            .IgnoreQueryFilters()
            .Include(b => b.BookingRooms)
            .ThenInclude(br => br.RoomType)
            .Where(b => b.GuestId == id)
            .OrderByDescending(b => b.ArrivalDate)
            .Take(50)
            .Select(b => new GuestBookingSummaryDto(
                b.Id,
                b.ArrivalDate,
                b.DepartureDate,
                b.Status.ToString(),
                Math.Max(1, (b.DepartureDate - b.ArrivalDate).Days),
                b.TotalPrice,
                b.BookingRooms.FirstOrDefault()!.RoomType!.Name))
            .ToListAsync();

        var totalSpent = bookingHistory.Where(b => b.Status != "Cancelled").Sum(b => b.TotalPrice);

        await LogPrivacyAccessAsync(guest.Id);

        return new GuestProfileDto(
            MapToDto(guest),
            stayHistory,
            bookingHistory,
            stayHistory.Count,
            totalSpent
        );
    }

    public async Task<PagedResult<GuestDto>> AdvancedSearchAsync(AdvancedGuestFilter filter)
    {
        var query = _dbContext.Guests
            .Include(g => g.Country)
            .Include(g => g.Documents)
            .Where(g => g.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FirstName))
            query = query.Where(g => g.FirstName.ToLower().Contains(filter.FirstName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.LastName))
            query = query.Where(g => g.LastName.ToLower().Contains(filter.LastName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Phone))
            query = query.Where(g => g.Phone != null && g.Phone.Contains(filter.Phone));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(g => g.Email != null && g.Email.ToLower().Contains(filter.Email.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
            query = query.Where(g => g.Documents.Any(d => d.DocumentNumber.Contains(filter.DocumentNumber)));

        if (filter.CountryId.HasValue)
            query = query.Where(g => g.CountryId == filter.CountryId);

        if (filter.FromDate.HasValue)
            query = query.Where(g => g.CreatedAt >= filter.FromDate);

        if (filter.ToDate.HasValue)
            query = query.Where(g => g.CreatedAt <= filter.ToDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<GuestDto>(dtos, totalCount, filter.Page, filter.PageSize);
    }

    private async Task LogPrivacyAccessAsync(Guid guestId)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = "Guest",
            EntityId = guestId.ToString(),
            Action = "PrivacyAccess",
            ChangedAt = DateTime.UtcNow,
            OldValues = null,
            NewValues = null,
            ChangedProperties = null
        };
        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync();
    }

    private static GuestDto MapToDto(Guest g)
    {
        return new GuestDto(
            g.Id,
            g.FirstName,
            g.LastName,
            g.DateOfBirth,
            g.Gender,
            g.Email,
            g.Phone,
            g.Address,
            g.City,
            g.PostalCode,
            g.CountryId,
            g.Country?.Name,
            g.NationalityCountryId,
            g.IsCompany,
            g.CompanyName,
            g.VatNumber,
            g.GdprConsentGiven,
            g.GdprConsentDate,
            g.GdprConsentVersion,
            g.Notes,
            g.IsActive,
            g.CreatedAt,
            g.UpdatedAt,
            g.Documents.Select(MapDocToDto).ToList()
        );
    }

    private static GuestDocumentDetailDto MapDocToDto(GuestDocument d)
    {
        return new GuestDocumentDetailDto(
            d.Id,
            d.GuestId,
            d.DocumentType.ToString(),
            d.DocumentNumber,
            d.IssuingCountry,
            d.MRZLine1,
            d.MRZLine2,
            d.IssueDate,
            d.ExpiryDate,
            d.FrontImagePath,
            d.BackImagePath,
            d.IsVerified,
            d.CreatedAt
        );
    }
}
