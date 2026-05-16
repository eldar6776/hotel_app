using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Interfaces;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Repositories;
using HotelPro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class BookingGroupTests
{
    private DbContextOptions<HotelProDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"TestDb_Group_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics
                    .InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private BookingGroupService CreateService(HotelProDbContext context)
    {
        var repo = new BookingGroupRepository(context);
        var tenant = new Mock<ITenantService>();
        var hotelId = Guid.NewGuid();
        tenant.Setup(t => t.GetCurrentHotelId()).Returns(hotelId);
        return new BookingGroupService(repo, context, tenant.Object);
    }

    private async Task<Guest> EnsureGuestAsync(HotelProDbContext context, string name = "Test")
    {
        var parts = name.Split(' ');
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = parts[0],
            LastName = parts.Length > 1 ? parts[1] : "",
            Email = $"{name.ToLower().Replace(" ", ".")}@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Guests.Add(guest);
        await context.SaveChangesAsync();
        return guest;
    }

    private async Task<RoomType> EnsureRoomTypeAsync(HotelProDbContext context, string name = "Double", decimal price = 100)
    {
        var rt = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = name.ToUpper().Substring(0, 3),
            BaseCapacity = 2,
            MaxCapacity = 4,
            DefaultPrice = price,
            IsActive = true,
            SortOrder = 1
        };
        context.RoomTypes.Add(rt);
        await context.SaveChangesAsync();
        return rt;
    }

    [Fact]
    public async Task CreateGroup_Success_ReturnsGroupWithBookings()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context, "Double", 100);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "TK Putovanja",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 3,
            RatePlanId: null,
            DiscountPercent: 10,
            ReleaseDate: new DateTime(2026, 7, 15),
            UseMasterBill: true,
            PayerGuestId: guest.Id,
            RoomTypes: new List<GroupRoomTypeDto>
            {
                new(rt.Id, 3)
            }
        );

        var result = await service.CreateGroupAsync(dto);

        Assert.Equal("TK Putovanja", result.Name);
        Assert.Equal(3, result.BlockedRoomCount);
        Assert.Equal(0, result.ConfirmedRoomCount);
        Assert.Equal(GroupStatus.Active.ToString(), result.Status);
        Assert.Equal(3, result.Bookings.Count);
        Assert.True(result.UseMasterBill);
        Assert.NotNull(result.MasterBill);
    }

    [Fact]
    public async Task CreateGroup_AppliesDiscountToRoomRates()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context, "Double", 100);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Discount Group",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 3),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 20,
            ReleaseDate: null,
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        var result = await service.CreateGroupAsync(dto);

        var booking = await context.Bookings
            .Include(b => b.BookingRooms)
            .IgnoreQueryFilters()
            .FirstAsync(b => b.GroupId == result.Id);

        Assert.Equal(80, booking.BookingRooms.First().PricePerNight);
    }

    [Fact]
    public async Task CreateGroup_Throws_WhenReleaseDateAfterArrival()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Bad Group",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: new DateTime(2026, 8, 2),
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateGroupAsync(dto));
    }

    [Fact]
    public async Task CreateGroup_Throws_WhenQuantityMismatch()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Mismatch Group",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 5,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: null,
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 3) }
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateGroupAsync(dto));
    }

    [Fact]
    public async Task ReleaseGroup_ReleasesAllBlockedBookings()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Release Test",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 2,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: new DateTime(2026, 7, 15),
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 2) }
        );

        var group = await service.CreateGroupAsync(dto);

        var released = await service.ReleaseGroupAsync(group.Id);

        Assert.Equal(2, released);

        var updatedGroup = await service.GetGroupByIdAsync(group.Id);
        Assert.Equal(GroupStatus.Released.ToString(), updatedGroup!.Status);
    }

    [Fact]
    public async Task ProcessExpiredReleases_ReleasesAllExpiredGroups()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto1 = new CreateBookingGroupDto(
            Name: "Expired 1",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: DateTime.UtcNow.AddDays(-1),
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        var dto2 = new CreateBookingGroupDto(
            Name: "Expired 2",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 9, 1),
            Departure: new DateTime(2026, 9, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: DateTime.UtcNow.AddDays(-2),
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        await service.CreateGroupAsync(dto1);
        await service.CreateGroupAsync(dto2);

        var released = await service.ProcessExpiredReleasesAsync();

        Assert.Equal(2, released);
    }

    [Fact]
    public async Task GetMasterBill_CalculatesTotalStayCharges()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context, "Double", 100);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Master Bill Test",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 4),
            BlockedRoomCount: 2,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: null,
            UseMasterBill: true,
            PayerGuestId: guest.Id,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 2) }
        );

        var group = await service.CreateGroupAsync(dto);

        var bill = await service.GetMasterBillAsync(group.Id);

        var expected = 2 * 100 * 3;
        Assert.Equal(expected, bill.TotalStayCharges);
    }

    [Fact]
    public async Task UpdateGroup_Throws_WhenNonActive()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto = new CreateBookingGroupDto(
            Name: "Update Test",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: null,
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        var group = await service.CreateGroupAsync(dto);
        await service.ReleaseGroupAsync(group.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateGroupAsync(group.Id, new UpdateBookingGroupDto(Name: "New Name", ContactPersonId: null, Arrival: null, Departure: null, RatePlanId: null, DiscountPercent: null, ReleaseDate: null, UseMasterBill: null, PayerGuestId: null)));
    }

    [Fact]
    public async Task GetGroups_ReturnsPagedResults()
    {
        var options = CreateInMemoryOptions();
        await using var context = new HotelProDbContext(options);
        var guest = await EnsureGuestAsync(context, "John Doe");
        var rt = await EnsureRoomTypeAsync(context);

        var service = CreateService(context);

        var dto1 = new CreateBookingGroupDto(
            Name: "Group A",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 8, 1),
            Departure: new DateTime(2026, 8, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: null,
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        var dto2 = new CreateBookingGroupDto(
            Name: "Group B",
            ContactPersonId: guest.Id,
            Arrival: new DateTime(2026, 9, 1),
            Departure: new DateTime(2026, 9, 5),
            BlockedRoomCount: 1,
            RatePlanId: null,
            DiscountPercent: 0,
            ReleaseDate: null,
            UseMasterBill: false,
            PayerGuestId: null,
            RoomTypes: new List<GroupRoomTypeDto> { new(rt.Id, 1) }
        );

        await service.CreateGroupAsync(dto1);
        await service.CreateGroupAsync(dto2);

        var result = await service.GetGroupsAsync(new BookingGroupFilter(Status: "Active", FromDate: null, ToDate: null, Page: 1, PageSize: 10));

        Assert.Equal(2, result.TotalCount);
        Assert.NotEmpty(result.Items);
    }
}
