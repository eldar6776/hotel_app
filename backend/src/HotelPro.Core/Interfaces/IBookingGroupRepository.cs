using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;

namespace HotelPro.Core.Interfaces;

public interface IBookingGroupRepository
{
    Task<BookingGroup?> GetByIdAsync(Guid id);
    Task<BookingGroup?> GetByIdWithDetailsAsync(Guid id);
    Task<PagedResult<BookingGroupDto>> GetAllAsync(BookingGroupFilter filter);
    Task AddAsync(BookingGroup group);
    Task UpdateAsync(BookingGroup group);
    Task ReleaseGroupAsync(Guid groupId);
    Task<int> GetActiveGroupsWithReleaseDateBeforeAsync(DateTime date);
    Task<List<BookingGroup>> GetGroupsPendingReleaseAsync(DateTime date);
}
