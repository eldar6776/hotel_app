using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Data;

public static class TestDataSeeder
{
    private const string DemoMarker = "DEMO_T19";
    private const int DemoGuestCount = 100;
    private static readonly Random Rng = new(42);

    public static void Seed(HotelProDbContext db)
    {
        var hotel = db.Hotels.First();
        var roomTypes = db.RoomTypes.OrderBy(x => x.SortOrder).ToList();
        if (roomTypes.Count == 0) return;

        EnsureBaseDemoData(db, roomTypes);

        var building = EnsureDemoBuilding(db);
        var rooms = EnsureDemoRooms(db, building, roomTypes);
        var guests = EnsureDemoGuests(db);
        EnsureDemoGuestDocuments(db, guests);
        EnsureDemoTariffsAndAmenities(db, roomTypes);
        EnsureDemoBookingsAndStays(db, hotel.Id, rooms, guests);
        EnsureHousekeepingAndWorkOrders(db, rooms);

        db.SaveChanges();
    }

    private static void EnsureBaseDemoData(HotelProDbContext db, List<RoomType> roomTypes)
    {
        if (db.Buildings.Any(x => x.IsActive) || db.Rooms.Any(x => x.IsActive) || db.Guests.Any(x => x.IsActive))
            return;

        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Glavna zgrada",
            Code = "A",
            City = "Sarajevo",
            Country = "Bosna i Hercegovina",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Buildings.Add(building);

        for (var floor = 1; floor <= 2; floor++)
        {
            for (var number = 1; number <= 5; number++)
            {
                var roomType = roomTypes[(floor + number) % roomTypes.Count];
                db.Rooms.Add(new Room
                {
                    Id = Guid.NewGuid(),
                    RoomNumber = $"{floor}{number:D2}",
                    Floor = floor,
                    BuildingId = building.Id,
                    RoomTypeId = roomType.Id,
                    Status = RoomStatus.Free,
                    BasePrice = roomType.DefaultPrice,
                    IsActive = true,
                    SortOrder = floor * 100 + number
                });
            }
        }

        db.SaveChanges();
    }

    private static Building EnsureDemoBuilding(HotelProDbContext db)
    {
        var building = db.Buildings.IgnoreQueryFilters().FirstOrDefault(x => x.Code == "DEMO");
        if (building != null)
        {
            building.IsActive = true;
            building.Name = "Demo hotel - glavna zgrada";
            building.City = "Sarajevo";
            building.Country = "Bosna i Hercegovina";
            building.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();
            return building;
        }

        building = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Demo hotel - glavna zgrada",
            Code = "DEMO",
            Address = "Obala Kulina bana 1",
            City = "Sarajevo",
            PostalCode = "71000",
            Country = "Bosna i Hercegovina",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Buildings.Add(building);
        db.SaveChanges();
        return building;
    }

    private static List<Room> EnsureDemoRooms(HotelProDbContext db, Building building, List<RoomType> roomTypes)
    {
        var statuses = Enum.GetValues<RoomStatus>();

        for (var floor = 1; floor <= 4; floor++)
        {
            for (var number = 1; number <= 10; number++)
            {
                var roomNumber = $"{floor}{number:D2}";
                var room = db.Rooms.IgnoreQueryFilters()
                    .FirstOrDefault(x => x.BuildingId == building.Id && x.RoomNumber == roomNumber);
                var roomType = roomTypes[(floor + number) % roomTypes.Count];
                var status = statuses[((floor - 1) * 10 + number - 1) % statuses.Length];

                if (room == null)
                {
                    room = new Room
                    {
                        Id = Guid.NewGuid(),
                        RoomNumber = roomNumber,
                        BuildingId = building.Id
                    };
                    db.Rooms.Add(room);
                }

                room.Floor = floor;
                room.RoomTypeId = roomType.Id;
                room.Status = status;
                room.BasePrice = roomType.DefaultPrice + floor * 10 + number;
                room.IsActive = true;
                room.SortOrder = floor * 100 + number;
                room.Notes = $"Demo soba {roomNumber}: sprat {floor}, status {status}.";
            }
        }

        db.SaveChanges();
        return db.Rooms
            .Include(x => x.RoomType)
            .Where(x => x.BuildingId == building.Id)
            .OrderBy(x => x.Floor)
            .ThenBy(x => x.RoomNumber)
            .ToList();
    }

    private static List<Guest> EnsureDemoGuests(HotelProDbContext db)
    {
        var firstNames = new[] { "Amila", "Marko", "Ivana", "Haris", "Lejla", "Petar", "Sara", "Nermin", "Katarina", "Adnan", "Emina", "Luka", "Maja", "Jasmin", "Ana", "Mirza", "Elena", "Dino", "Selma", "Nikola" };
        var lastNames = new[] { "HadZic", "Kovacevic", "Horvat", "Novak", "Maric", "Babic", "Juric", "Pavic", "Tomic", "Vukovic", "Radic", "Matic", "Ilic", "Knezovic", "Begic", "Dedic", "Mehic", "Saric", "Kralj", "Bauer" };
        var cities = new[] { "Sarajevo", "Mostar", "Zagreb", "Split", "Beograd", "Ljubljana", "Wien", "Berlin", "Milano", "London" };
        var countries = db.Countries.ToList();

        for (var i = 1; i <= DemoGuestCount; i++)
        {
            var email = $"demo.gost{i:D3}@hotelpro.local";
            var guest = db.Guests.FirstOrDefault(x => x.Email == email);
            var country = countries.Count == 0 ? null : countries[i % countries.Count];

            if (guest == null)
            {
                guest = new Guest
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };
                db.Guests.Add(guest);
            }

            guest.FirstName = firstNames[(i - 1) % firstNames.Length];
            guest.LastName = $"{lastNames[(i - 1) % lastNames.Length]} {i:D3}";
            guest.Phone = $"+38761{i:D6}";
            guest.Address = $"Demo ulica {i}";
            guest.City = cities[(i - 1) % cities.Length];
            guest.PostalCode = $"{71000 + i}";
            guest.CountryId = country?.Id;
            guest.DateOfBirth = new DateTime(1955 + i % 45, 1 + i % 12, 1 + i % 27, 0, 0, 0, DateTimeKind.Utc);
            guest.Gender = i % 2 == 0 ? "F" : "M";
            guest.IsCompany = i % 17 == 0;
            guest.CompanyName = guest.IsCompany ? $"Demo Partner {i:D3} d.o.o." : null;
            guest.VatNumber = guest.IsCompany ? $"VAT{i:D8}" : null;
            guest.GdprConsentGiven = true;
            guest.GdprConsentDate = DateTime.UtcNow.AddDays(-40 + i % 30);
            guest.GdprConsentVersion = "1.0";
            guest.Notes = $"{DemoMarker}: gost za testiranje proslih, aktivnih i buducih boravaka.";
            guest.IsActive = true;
            guest.UpdatedAt = DateTime.UtcNow;
        }

        db.SaveChanges();
        return db.Guests
            .Include(x => x.Documents)
            .Where(x => x.Email != null && x.Email.StartsWith("demo.gost"))
            .OrderBy(x => x.Email)
            .Take(DemoGuestCount)
            .ToList();
    }

    private static void EnsureDemoGuestDocuments(HotelProDbContext db, List<Guest> guests)
    {
        foreach (var guest in guests.Where((_, index) => index < 40))
        {
            if (db.GuestDocuments.Any(x => x.GuestId == guest.Id)) continue;

            db.GuestDocuments.Add(new GuestDocument
            {
                Id = Guid.NewGuid(),
                GuestId = guest.Id,
                DocumentType = Rng.Next(0, 2) == 0 ? DocumentType.Passport : DocumentType.IDCard,
                DocumentNumber = $"DEMO{Rng.Next(100000, 999999)}",
                IssuingCountry = guest.Country?.Code ?? "BIH",
                IssueDate = DateTime.UtcNow.AddYears(-Rng.Next(1, 8)),
                ExpiryDate = DateTime.UtcNow.AddYears(Rng.Next(1, 6)),
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.SaveChanges();
    }

    private static void EnsureDemoTariffsAndAmenities(HotelProDbContext db, List<RoomType> roomTypes)
    {
        foreach (var roomType in roomTypes)
        {
            var tariffName = $"Demo fleksibilna tarifa - {roomType.Code}";
            if (!db.Tariffs.Any(x => x.Name == tariffName))
            {
                db.Tariffs.Add(new Tariff
                {
                    Id = Guid.NewGuid(),
                    Name = tariffName,
                    RoomTypeId = roomType.Id,
                    ValidFrom = DateTime.UtcNow.Date.AddDays(-45),
                    ValidTo = DateTime.UtcNow.Date.AddDays(120),
                    BasePrice = roomType.DefaultPrice + 15,
                    Currency = "EUR",
                    IsActive = true
                });
            }
        }

        var amenities = new (string Name, string Icon)[]
        {
            ("WiFi", "wifi"),
            ("Parking", "parking"),
            ("Dorucak", "coffee"),
            ("Spa", "waves"),
            ("Klima", "snowflake"),
            ("Mini bar", "glass-water")
        };

        foreach (var amenity in amenities)
        {
            if (db.Amenities.Any(x => x.Name == amenity.Name)) continue;
            db.Amenities.Add(new Amenity { Id = Guid.NewGuid(), Name = amenity.Name, Icon = amenity.Icon, IsActive = true });
        }

        db.SaveChanges();
    }

    private static void EnsureDemoBookingsAndStays(HotelProDbContext db, Guid hotelId, List<Room> rooms, List<Guest> guests)
    {
        if (db.Bookings.Any(x => x.InternalNotes == DemoMarker)) return;

        var today = DateTime.UtcNow.Date;
        var paymentMethods = db.PaymentMethods.ToList();

        for (var i = 0; i < guests.Count; i++)
        {
            var guest = guests[i];
            var room = rooms[i % rooms.Count];
            var nights = 1 + i % 5;
            var status = ResolveBookingStatus(i);
            var arrival = ResolveArrivalDate(today, i, status);
            var departure = arrival.AddDays(nights);
            if (status == BookingStatus.CheckedIn && departure <= today) departure = today.AddDays(1 + i % 3);

            var booking = CreateBooking(hotelId, guest, room, status, arrival, departure, i);
            db.Bookings.Add(booking);

            var bookingRoom = booking.BookingRooms.First();
            if (status is BookingStatus.CheckedIn or BookingStatus.CheckedOut)
            {
                var isCheckedOut = status == BookingStatus.CheckedOut;
                var folio = new Folio
                {
                    Id = Guid.NewGuid(),
                    FolioNumber = $"DMF-{i + 1:D5}",
                    BookingId = booking.Id,
                    BookingRoomId = bookingRoom.Id,
                    GuestId = guest.Id,
                    Status = isCheckedOut ? FolioStatus.Closed : FolioStatus.Open,
                    Balance = isCheckedOut ? 0 : booking.TotalPrice,
                    CreatedAt = arrival.AddHours(14),
                    ClosedAt = isCheckedOut ? departure.AddHours(10) : null,
                    UpdatedAt = DateTime.UtcNow,
                    Notes = DemoMarker
                };
                db.Folios.Add(folio);

                var stay = new Stay
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    GuestId = guest.Id,
                    RoomId = room.Id,
                    FolioId = folio.Id,
                    BookingId = booking.Id,
                    BookingRoomId = bookingRoom.Id,
                    CheckInDate = arrival.AddHours(14),
                    CheckOutDate = departure.AddHours(10),
                    CheckedOutAt = isCheckedOut ? departure.AddHours(10) : null,
                    IsCheckedOut = isCheckedOut,
                    IsRegistrationPrinted = i % 3 != 0,
                    IsReservationLink = true,
                    IsFromConfirmedReservation = true,
                    IsAccommodationPaid = isCheckedOut || i % 2 == 0,
                    GuestCategory = GuestCategory.Adult,
                    DiscountPercent = i % 10 == 0 ? 10 : 0,
                    DiscountReason = i % 10 == 0 ? "Demo loyalty popust" : null,
                    StayNote = DemoMarker,
                    CreatedAt = arrival.AddHours(13),
                    UpdatedAt = DateTime.UtcNow
                };
                db.Stays.Add(stay);

                for (var date = arrival.Date; date < departure.Date; date = date.AddDays(1))
                {
                    db.StayNights.Add(new StayNight
                    {
                        Id = Guid.NewGuid(),
                        FolioId = folio.Id,
                        StayId = stay.Id,
                        RoomId = room.Id,
                        Date = date,
                        TariffAmount = bookingRoom.PricePerNight,
                        DiscountPercent = stay.DiscountPercent,
                        Status = isCheckedOut ? NightStatus.Closed : NightStatus.Active,
                        IsComp = booking.Type == BookingType.Complementary,
                        Description = $"Demo nocenje {room.RoomNumber}",
                        Notes = DemoMarker,
                        ClosedAt = isCheckedOut ? date.AddHours(23) : null
                    });
                }

                db.Charges.Add(new Charge
                {
                    Id = Guid.NewGuid(),
                    FolioId = folio.Id,
                    ChargeType = ChargeType.Restaurant,
                    Description = "Demo restoran",
                    Quantity = 1,
                    UnitPrice = 18 + i % 35,
                    TotalPrice = 18 + i % 35,
                    VatAmount = Math.Round((18 + i % 35) * 0.17m, 2),
                    ChargeDate = arrival.AddDays(Math.Min(1, nights - 1)).AddHours(19),
                    IsTaxable = true,
                    POSReference = $"POS-DEMO-{i + 1:D4}"
                });

                var paidAmount = isCheckedOut ? booking.TotalPrice : Math.Round(booking.TotalPrice / 2, 2);
                db.Payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    FolioId = folio.Id,
                    PaymentMethodId = paymentMethods.Count == 0 ? null : paymentMethods[i % paymentMethods.Count].Id,
                    PaymentMethod = paymentMethods.Count == 0 ? "Cash" : paymentMethods[i % paymentMethods.Count].Name,
                    Amount = paidAmount,
                    Status = PaymentStatus.Paid,
                    PaymentDate = arrival.AddHours(15),
                    Reference = $"PAY-DEMO-{i + 1:D5}",
                    Notes = DemoMarker
                });
            }
        }

        db.SaveChanges();
    }

    private static Booking CreateBooking(Guid hotelId, Guest guest, Room room, BookingStatus status, DateTime arrival, DateTime departure, int index)
    {
        var nights = Math.Max(1, (departure - arrival).Days);
        var price = room.BasePrice ?? room.RoomType.DefaultPrice;
        var total = price * nights;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            HotelId = hotelId,
            GuestId = guest.Id,
            Source = (BookingSource)(index % Enum.GetValues<BookingSource>().Length),
            Type = index % 23 == 0 ? BookingType.Corporate : index % 19 == 0 ? BookingType.TravelAgency : BookingType.Normal,
            Status = status,
            ArrivalDate = arrival,
            DepartureDate = departure,
            AdultCount = 1 + index % 2,
            ChildCount = index % 7 == 0 ? 1 : 0,
            TotalPrice = total,
            ExchangeRateTotal = total,
            Currency = "EUR",
            Notes = "Demo rezervacija za smoke test aplikacije.",
            InternalNotes = DemoMarker,
            CancellationReason = status == BookingStatus.Cancelled ? "Demo otkazivanje" : null,
            CancelledAt = status == BookingStatus.Cancelled ? DateTime.UtcNow.AddDays(-index % 10) : null,
            CreatedAt = arrival.AddDays(-15 - index % 20),
            UpdatedAt = DateTime.UtcNow
        };

        booking.BookingRooms.Add(new BookingRoom
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            RoomId = status is BookingStatus.Pending or BookingStatus.Cancelled ? null : room.Id,
            RoomTypeId = room.RoomTypeId,
            RatePlanId = Guid.NewGuid(),
            PricePerNight = price,
            Status = status switch
            {
                BookingStatus.CheckedIn => BookingRoomStatus.Occupied,
                BookingStatus.CheckedOut => BookingRoomStatus.Released,
                BookingStatus.Cancelled => BookingRoomStatus.Released,
                _ => BookingRoomStatus.Assigned
            }
        });

        return booking;
    }

    private static BookingStatus ResolveBookingStatus(int index)
    {
        if (index < 30) return BookingStatus.CheckedOut;
        if (index < 55) return BookingStatus.CheckedIn;
        if (index < 80) return BookingStatus.Confirmed;
        if (index < 92) return BookingStatus.Pending;
        if (index < 97) return BookingStatus.Cancelled;
        return BookingStatus.NoShow;
    }

    private static DateTime ResolveArrivalDate(DateTime today, int index, BookingStatus status)
    {
        return status switch
        {
            BookingStatus.CheckedOut => today.AddDays(-35 + index),
            BookingStatus.CheckedIn => today.AddDays(-1 - index % 4),
            BookingStatus.Confirmed => index == 55 ? today : today.AddDays(1 + index % 21),
            BookingStatus.Pending => today.AddDays(7 + index % 25),
            BookingStatus.Cancelled => today.AddDays(4 + index % 30),
            BookingStatus.NoShow => today.AddDays(-1 - index % 7),
            _ => today
        };
    }

    private static void EnsureHousekeepingAndWorkOrders(HotelProDbContext db, List<Room> rooms)
    {
        var employees = db.Employees.OrderBy(x => x.Role).ToList();
        var reporter = employees.FirstOrDefault(x => x.Role is EmployeeRole.Admin or EmployeeRole.Manager) ?? employees.FirstOrDefault();
        var housekeeper = employees.FirstOrDefault(x => x.Role == EmployeeRole.Housekeeping) ?? reporter;
        if (reporter == null || housekeeper == null) return;

        if (!db.HousekeepingLogs.Any(x => x.Notes == DemoMarker))
        {
            foreach (var room in rooms.Take(20))
            {
                db.HousekeepingLogs.Add(new HousekeepingLog
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    EmployeeId = housekeeper.Id,
                    Action = (HousekeepingAction)(room.SortOrder % Enum.GetValues<HousekeepingAction>().Length),
                    Status = (HousekeepingStatus)(room.SortOrder % Enum.GetValues<HousekeepingStatus>().Length),
                    ScheduledAt = DateTime.UtcNow.Date.AddHours(9),
                    StartedAt = DateTime.UtcNow.Date.AddHours(9).AddMinutes(room.SortOrder % 60),
                    CompletedAt = DateTime.UtcNow.Date.AddHours(10).AddMinutes(room.SortOrder % 60),
                    Notes = DemoMarker,
                    IsVerified = room.SortOrder % 3 == 0,
                    VerifiedById = reporter.Id
                });
            }
        }

        if (!db.WorkOrders.Any(x => x.Description.Contains(DemoMarker)))
        {
            foreach (var room in rooms.Take(12))
            {
                var priority = (WorkOrderPriority)(room.SortOrder % Enum.GetValues<WorkOrderPriority>().Length);
                var status = (WorkOrderStatus)(room.SortOrder % Enum.GetValues<WorkOrderStatus>().Length);
                db.WorkOrders.Add(new WorkOrder
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    ReportedById = reporter.Id,
                    AssignedToId = housekeeper.Id,
                    Priority = priority,
                    Category = (WorkOrderCategory)(room.SortOrder % Enum.GetValues<WorkOrderCategory>().Length),
                    Description = $"{DemoMarker}: servisni nalog za sobu {room.RoomNumber}",
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-room.SortOrder % 9),
                    ResolvedAt = status is WorkOrderStatus.Resolved or WorkOrderStatus.Closed ? DateTime.UtcNow.AddDays(-1) : null,
                    ResolutionNotes = status is WorkOrderStatus.Resolved or WorkOrderStatus.Closed ? "Demo kvar rijesen." : null
                });
            }
        }

        db.SaveChanges();
    }
}
