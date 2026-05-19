using HotelPro.Core.DTOs;
using HotelPro.Core.Enums;

namespace HotelPro.Core.Services;

public interface IBookingService
{
    Task<PagedResult<BookingDto>> GetBookingsAsync(BookingFilter filter);
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
    Task<BookingDto> UpdateBookingAsync(Guid id, UpdateBookingDto dto);
    Task<BookingDto> AssignRoomAsync(Guid id, AssignRoomDto dto);
    Task DeleteBookingAsync(Guid id);
    Task UpdateBookingStatusAsync(Guid id, BookingStatus newStatus);
}
