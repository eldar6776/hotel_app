using HotelPro.Core.DTOs;
using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Core.Services;
using HotelPro.Infrastructure.Data;
using HotelPro.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HotelPro.Tests.UnitTests;

public class StayLifecycleServiceTests
{
    private readonly Guid _hotelId = Guid.NewGuid();

    private HotelProDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HotelProDbContext>()
            .UseInMemoryDatabase($"StayLifecycle_{Guid.NewGuid()}")
            .Options;

        var httpContext = new DefaultHttpContext();
        httpContext.Items["HotelId"] = _hotelId;

        return new HotelProDbContext(options, new HttpContextAccessor { HttpContext = httpContext });
    }

    private async Task<Room> SeedRoomAsync(HotelProDbContext context, RoomStatus status = RoomStatus.Free, decimal? basePrice = 100m)
    {
        var building = new Building { Id = Guid.NewGuid(), Name = "Main", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var roomType = new RoomType
        {
            Id = Guid.NewGuid(),
            Name = "Standard",
            Code = "STD",
            BaseCapacity = 1,
            MaxCapacity = 2,
            DefaultPrice = basePrice ?? 100,
            IsActive = true
        };
        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = "101",
            Floor = 1,
            BuildingId = building.Id,
            RoomTypeId = roomType.Id,
            Status = status,
            IsActive = true,
            BasePrice = basePrice,
            Building = building,
            RoomType = roomType
        };

        context.Buildings.Add(building);
        context.RoomTypes.Add(roomType);
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return room;
    }

    private async Task<Guest> SeedGuestAsync(HotelProDbContext context, DateTime? dateOfBirth = null)
    {
        var guest = new Guest
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = dateOfBirth,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Guests.Add(guest);
        await context.SaveChangesAsync();
        return guest;
    }

    private StayLifecycleService CreateService(
        HotelProDbContext context,
        Mock<IRoomOccupancyPolicy>? policyMock = null,
        Mock<IConfigurationService>? configMock = null)
    {
        policyMock ??= new Mock<IRoomOccupancyPolicy>();

        var useDefaultConfig = configMock == null;
        configMock ??= new Mock<IConfigurationService>();

        if (useDefaultConfig)
        {
            configMock.Setup(x => x.GetValueAsync("billing_mode")).ReturnsAsync("0");
            configMock.Setup(x => x.GetValueAsync("tourist_tax_amount")).ReturnsAsync("0");
            configMock.Setup(x => x.GetValueAsync("insurance_amount")).ReturnsAsync("0");
            configMock.Setup(x => x.GetValueAsync("child_age_threshold")).ReturnsAsync("0");
            configMock.Setup(x => x.GetValueAsync("child_discount_percent")).ReturnsAsync("0");
        }

        var logger = new Mock<ILogger<StayLifecycleService>>();

        return new StayLifecycleService(context, policyMock.Object, configMock.Object, logger.Object);
    }

    [Fact]
    public async Task CheckInAsync_SingleGuest_CreatesStayAndNights()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var guest = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var service = CreateService(context, policyMock);

        var request = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry>
            {
                new(GuestId: guest.Id, GuestCategory: GuestCategory.Adult)
            },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 4)
        );

        var result = await service.CheckInAsync(request);

        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal("101", result.RoomNumber);
        Assert.NotEqual(Guid.Empty, result.FolioId);
        Assert.Single(result.Guests);
        Assert.Equal(3, result.Guests[0].NightsCreated);

        var stays = await context.Stays.Where(s => s.RoomId == room.Id).ToListAsync();
        Assert.Single(stays);
        Assert.False(stays[0].IsCheckedOut);
        Assert.Equal(GuestCategory.Adult, stays[0].GuestCategory);

        var nights = await context.StayNights.Where(n => n.FolioId == result.FolioId).ToListAsync();
        Assert.Equal(3, nights.Count);
        Assert.All(nights, n => Assert.Equal(NightStatus.Active, n.Status));
    }

    [Fact]
    public async Task CheckInAsync_MultipleGuests_CreatesStaysAndSplitTariff()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context, basePrice: 120m);
        var guest1 = await SeedGuestAsync(context);
        var guest2 = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var service = CreateService(context, policyMock);

        var request = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry>
            {
                new(GuestId: guest1.Id, GuestCategory: GuestCategory.Adult),
                new(GuestId: guest2.Id, GuestCategory: GuestCategory.Adult)
            },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var result = await service.CheckInAsync(request);

        Assert.Equal(2, result.Guests.Count);

        var stays = await context.Stays.Where(s => s.RoomId == room.Id).ToListAsync();
        Assert.Equal(2, stays.Count);

        var nights = await context.StayNights.Where(n => n.FolioId == result.FolioId).ToListAsync();
        Assert.Equal(4, nights.Count);

        Assert.All(nights, n => Assert.Equal(60m, n.TariffAmount));
    }

    [Fact]
    public async Task CheckInAsync_ReusesFolio_WhenOpenFolioExists()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var guest1 = await SeedGuestAsync(context);
        var guest2 = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var service = CreateService(context, policyMock);

        var firstCheckIn = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry> { new(GuestId: guest1.Id, GuestCategory: GuestCategory.Adult) },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var result1 = await service.CheckInAsync(firstCheckIn);

        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Occupied, RoomStatus.Occupied, "Occupied", false));

        var secondCheckIn = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry> { new(GuestId: guest2.Id, GuestCategory: GuestCategory.Adult) },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(secondCheckIn));
        Assert.Contains("must be Free or Reserved", ex.Message);
    }

    [Fact]
    public async Task CheckInAsync_ThrowsWhenNoGuests()
    {
        await using var context = CreateContext();
        var service = CreateService(context);

        var request = new StayCheckInRequest(
            RoomId: Guid.NewGuid(),
            Guests: new List<StayGuestEntry>()
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(request));
    }

    [Fact]
    public async Task CheckInAsync_ThrowsWhenCheckoutBeforeCheckin()
    {
        await using var context = CreateContext();
        var guest = await SeedGuestAsync(context);

        var service = CreateService(context);

        var request = new StayCheckInRequest(
            RoomId: Guid.NewGuid(),
            Guests: new List<StayGuestEntry> { new(GuestId: guest.Id) },
            CheckInDate: new DateTime(2026, 6, 5),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(request));
    }

    [Fact]
    public async Task CheckInAsync_DetectsDuplicateGuests()
    {
        await using var context = CreateContext();
        var guest = await SeedGuestAsync(context);

        var service = CreateService(context);

        var request = new StayCheckInRequest(
            RoomId: Guid.NewGuid(),
            Guests: new List<StayGuestEntry>
            {
                new(GuestId: guest.Id),
                new(GuestId: guest.Id)
            }
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckInAsync(request));
    }

    [Fact]
    public async Task CheckInAsync_AutoAssignsChildCategory()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var childDob = DateTime.UtcNow.AddYears(-8);
        var guest = await SeedGuestAsync(context, childDob);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var configMock = new Mock<IConfigurationService>();
        configMock.Setup(x => x.GetValueAsync("billing_mode")).ReturnsAsync("1");
        configMock.Setup(x => x.GetValueAsync("tourist_tax_amount")).ReturnsAsync("2");
        configMock.Setup(x => x.GetValueAsync("insurance_amount")).ReturnsAsync("1");
        configMock.Setup(x => x.GetValueAsync("child_age_threshold")).ReturnsAsync("12");
        configMock.Setup(x => x.GetValueAsync("child_discount_percent")).ReturnsAsync("50");

        var service = CreateService(context, policyMock, configMock);

        var request = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry> { new(GuestId: guest.Id, GuestCategory: GuestCategory.Unknown) },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var result = await service.CheckInAsync(request);

        Assert.Equal(GuestCategory.Child, result.Guests[0].GuestCategory);
        Assert.Equal(50m, result.Guests[0].DiscountPercent);

        var stay = await context.Stays.FirstAsync(s => s.GuestId == guest.Id);
        Assert.Equal(GuestCategory.Child, stay.GuestCategory);
        Assert.Equal(50m, stay.DiscountPercent);
        Assert.Contains("8", stay.DiscountReason);
    }

    [Fact]
    public async Task CheckInAsync_MinimumCharge_WhenTariffIsZero()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context, basePrice: 0m);
        var guest = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var configMock = new Mock<IConfigurationService>();
        configMock.Setup(x => x.GetValueAsync("billing_mode")).ReturnsAsync("1");
        configMock.Setup(x => x.GetValueAsync("tourist_tax_amount")).ReturnsAsync("3");
        configMock.Setup(x => x.GetValueAsync("insurance_amount")).ReturnsAsync("2");
        configMock.Setup(x => x.GetValueAsync("child_age_threshold")).ReturnsAsync("0");
        configMock.Setup(x => x.GetValueAsync("child_discount_percent")).ReturnsAsync("0");

        var service = CreateService(context, policyMock, configMock);

        var request = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry> { new(GuestId: guest.Id, GuestCategory: GuestCategory.Adult) },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var result = await service.CheckInAsync(request);

        var nights = await context.StayNights.Where(n => n.FolioId == result.FolioId).ToListAsync();
        Assert.All(nights, n => Assert.Equal(5m, n.TariffAmount));
    }

    [Fact]
    public async Task CheckInAsync_WarnsOnExpiredDocument()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var guest = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var service = CreateService(context, policyMock);

        var request = new StayCheckInRequest(
            RoomId: room.Id,
            Guests: new List<StayGuestEntry>
            {
                new(GuestId: guest.Id, GuestCategory: GuestCategory.Adult,
                    Documents: new List<GuestDocumentDto>
                    {
                        new(DocumentType: "Passport", DocumentNumber: "P123", ExpiryDate: DateTime.UtcNow.AddDays(-10))
                    })
            },
            CheckInDate: new DateTime(2026, 6, 1),
            CheckOutDate: new DateTime(2026, 6, 3)
        );

        var result = await service.CheckInAsync(request);

        Assert.Single(result.Warnings);
        Assert.Contains("expired", result.Warnings[0].ToLower());
    }

    [Fact]
    public async Task GetActiveStaysForRoomAsync_ReturnsOnlyActiveStays()
    {
        await using var context = CreateContext();
        var room = await SeedRoomAsync(context);
        var guest1 = await SeedGuestAsync(context);
        var guest2 = await SeedGuestAsync(context);

        var policyMock = new Mock<IRoomOccupancyPolicy>();
        policyMock.Setup(x => x.GetRoomStatusAsync(room.Id, It.IsAny<DateTime>()))
            .ReturnsAsync(new RoomStatusDetailDto(room.Id, "101", RoomStatus.Free, RoomStatus.Free, "Free", false));

        var service = CreateService(context, policyMock);

        var folio = new Folio
        {
            Id = Guid.NewGuid(),
            FolioNumber = "F-TEST001",
            Status = FolioStatus.Open,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };
        context.Folios.Add(folio);

        context.Stays.Add(new Stay
        {
            Id = Guid.NewGuid(),
            GuestId = guest1.Id,
            RoomId = room.Id,
            FolioId = folio.Id,
            CheckInDate = new DateTime(2026, 6, 1),
            CheckOutDate = new DateTime(2026, 6, 5),
            IsCheckedOut = true,
            GuestCategory = GuestCategory.Adult,
            DiscountPercent = 0,
            CreatedAt = DateTime.UtcNow
        });

        context.Stays.Add(new Stay
        {
            Id = Guid.NewGuid(),
            GuestId = guest2.Id,
            RoomId = room.Id,
            FolioId = folio.Id,
            CheckInDate = new DateTime(2026, 6, 1),
            CheckOutDate = new DateTime(2026, 6, 5),
            IsCheckedOut = false,
            GuestCategory = GuestCategory.Adult,
            DiscountPercent = 0,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var result = await service.GetActiveStaysForRoomAsync(room.Id);

        Assert.Single(result);
        Assert.Equal(guest2.Id, result.First().GuestId);
    }
}
