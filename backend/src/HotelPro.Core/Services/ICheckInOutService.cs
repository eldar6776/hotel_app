using HotelPro.Core.DTOs;

namespace HotelPro.Core.Services;

public interface ICheckInService
{
    Task<CheckInResponse> CheckInAsync(CheckInRequest request);
}

public interface ICheckOutService
{
    Task<CheckOutResponse> CheckOutAsync(CheckOutRequest request);
}
