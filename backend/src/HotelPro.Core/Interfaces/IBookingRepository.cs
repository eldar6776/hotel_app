using HotelPro.Core.Entities;
using HotelPro.Core.Enums;

namespace HotelPro.Core.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking?> GetByIdWithRoomsAsync(Guid id);
    Task<IEnumerable<Booking>> GetAllAsync(
        List<BookingStatus>? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? guestId = null,
        Guid? roomId = null,
        int page = 1,
        int pageSize = 20);
    Task<int> CountAsync(
        List<BookingStatus>? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? guestId = null,
        Guid? roomId = null);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);
    Task<bool> ExistsAsync(Guid id);
}
