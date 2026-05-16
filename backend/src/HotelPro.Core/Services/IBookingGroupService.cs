using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;

namespace HotelPro.Core.Services;

public interface IBookingGroupService
{
    Task<PagedResult<BookingGroupDto>> GetGroupsAsync(BookingGroupFilter filter);
    Task<BookingGroupDto?> GetGroupByIdAsync(Guid id);
    Task<BookingGroupDto> CreateGroupAsync(CreateBookingGroupDto dto);
    Task<BookingGroupDto> UpdateGroupAsync(Guid id, UpdateBookingGroupDto dto);
    Task<MasterBillDto> GetMasterBillAsync(Guid groupId);
    Task<int> ReleaseGroupAsync(Guid groupId);
    Task<int> ProcessExpiredReleasesAsync();
}
