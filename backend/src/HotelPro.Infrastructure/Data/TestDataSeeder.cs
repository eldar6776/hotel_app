using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;

namespace HotelPro.Infrastructure.Data;

public static class TestDataSeeder
{
    private static readonly Random Rng = new(42);

    public static void Seed(HotelProDbContext db)
    {
        if (db.Rooms.Any(r => r.IsActive && r.Building != null)) return;

        // Buildings
        var b1 = new Building { Id = Guid.NewGuid(), Name = "Glavna zgrada", Code = "A", IsActive = true };
        var b2 = new Building { Id = Guid.NewGuid(), Name = "Depandansa", Code = "B", IsActive = true };
        var b3 = new Building { Id = Guid.NewGuid(), Name = "Vila", Code = "C", IsActive = true };
        db.Buildings.AddRange(b1, b2, b3);
        db.SaveChanges();

        var rtSingle = db.RoomTypes.First(r => r.Code == "SGL");
        var rtDouble = db.RoomTypes.First(r => r.Code == "DBL");
        var rtSuite = db.RoomTypes.First(r => r.Code == "STE");

        var rooms = new List<Room>();
        for (int floor = 1; floor <= 3; floor++)
            for (int r = 1; r <= 4; r++)
            {
                var rt = (floor == 3) ? rtSuite : (r % 2 == 0 ? rtDouble : rtSingle);
                rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"{floor}0{r}", Floor = floor, BuildingId = b1.Id, RoomTypeId = rt.Id, Status = RoomStatus.Free, BasePrice = rt.DefaultPrice, IsActive = true });
            }
        for (int floor = 1; floor <= 2; floor++)
            for (int r = 1; r <= 4; r++)
            {
                var rt = r <= 2 ? rtDouble : rtSingle;
                rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"B{floor}0{r}", Floor = floor, BuildingId = b2.Id, RoomTypeId = rt.Id, Status = RoomStatus.Free, BasePrice = rt.DefaultPrice, IsActive = true });
            }
        for (int r = 1; r <= 5; r++)
            rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"C{r:D2}", Floor = 1, BuildingId = b3.Id, RoomTypeId = rtSuite.Id, Status = RoomStatus.Free, BasePrice = rtSuite.DefaultPrice, IsActive = true });
        db.Rooms.AddRange(rooms);
        db.SaveChanges();

        var guestNames = new (string F, string L, string? E, string? P)[]
        {
            ("Marko","Horvat","marko.horvat@email.hr","+385 91 123 4567"),("Ivana","Kovačić","ivana.kovacic@email.hr","+385 92 234 5678"),
            ("Petar","Babić","petar.babic@email.hr",null),("Ana","Marić","ana.maric@email.hr","+385 95 345 6789"),
            ("Tomislav","Novak",null,"+385 98 456 7890"),("Marija","Jurić","marija.juric@email.hr",null),
            ("Ivan","Vuković","ivan.vukovic@email.hr","+385 99 567 8901"),("Katarina","Pavić",null,null),
            ("Stjepan","Radić","stjepan.radic@email.hr","+385 91 678 9012"),("Jelena","Tomić","jelena.tomic@email.hr","+385 92 789 0123"),
            ("Ante","Knežević","ante.knezevic@email.hr",null),("Martina","Vidović","martina.vidovic@email.hr","+385 95 890 1234"),
            ("Josip","Blažević",null,"+385 98 901 2345"),("Lucija","Grgić","lucija.grgic@email.hr","+385 99 012 3456"),
            ("Davor","Filipović","davor.filipovic@email.hr",null),("Nikolina","Perić","nikolina.peric@email.hr","+385 91 111 2222"),
            ("Hrvoje","Šimić","hrvoje.simic@email.hr","+385 92 333 4444"),("Sara","Lončar",null,null),
            ("Mladen","Duvnjak","mladen.duvnjak@email.hr","+385 95 555 6666"),("Tamara","Bogdan","tamara.bogdan@email.hr","+385 98 777 8888"),
        };

        var guests = guestNames.Select((g, i) => new Guest
        {
            Id = Guid.NewGuid(), FirstName = g.F, LastName = g.L, Email = g.E, Phone = g.P,
            DateOfBirth = new DateTime(1965 + Rng.Next(40), Rng.Next(1, 13), Rng.Next(1, 28), 0, 0, 0, DateTimeKind.Utc),
            Gender = i % 3 == 0 ? "M" : "F", City = i % 4 == 0 ? "Zagreb" : i % 4 == 1 ? "Split" : "Rijeka",
            Address = $"Ulica {i + 1}", GdprConsentGiven = true, GdprConsentDate = DateTime.UtcNow,
            GdprConsentVersion = "1.0", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            Documents = new List<GuestDocument> {
                new() { Id = Guid.NewGuid(), DocumentType = DocumentType.IDCard, DocumentNumber = $"HR{1000000 + i:D7}",
                    IssuingCountry = "HRV", ExpiryDate = DateTime.UtcNow.AddYears(3), IsVerified = true, CreatedAt = DateTime.UtcNow }
            }
        }).ToList();
        db.Guests.AddRange(guests);
        db.SaveChanges();

        var today = DateTime.UtcNow.Date;
        var bookings = new List<Booking>();

        for (int i = 0; i < 5; i++) { var room = rooms[i]; room.Status = RoomStatus.Reserved;
            bookings.Add(new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.BookingCom, Type = BookingType.Normal, Status = BookingStatus.Confirmed, ArrivalDate = today, DepartureDate = today.AddDays(3 + i % 3), AdultCount = 1 + i % 2, ChildCount = i % 3 == 0 ? 1 : 0, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomId = room.Id, RoomTypeId = room.RoomTypeId, RatePlanId = Guid.NewGuid(), PricePerNight = room.BasePrice ?? 80, Status = BookingRoomStatus.Blocked } } });
        }

        for (int i = 5; i < 8; i++) { var room = rooms[i]; room.Status = RoomStatus.Occupied; var nights = 2 + i % 3;
            var b = new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.Direct, Type = BookingType.Normal, Status = BookingStatus.CheckedIn, ArrivalDate = today.AddDays(-1), DepartureDate = today.AddDays(nights - 1), AdultCount = 2, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomId = room.Id, RoomTypeId = room.RoomTypeId, RatePlanId = Guid.NewGuid(), PricePerNight = room.BasePrice ?? 80, Status = BookingRoomStatus.Occupied } } };
            b.TotalPrice = b.BookingRooms.Sum(r => r.PricePerNight * nights); b.ExchangeRateTotal = b.TotalPrice;
            bookings.Add(b);
            var folio = new Folio { Id = Guid.NewGuid(), FolioNumber = $"F-{b.Id:N}"[..12], BookingId = b.Id, GuestId = b.GuestId, Status = FolioStatus.Open, Balance = b.TotalPrice, CreatedAt = b.ArrivalDate };
            db.Folios.Add(folio);
            db.StayNights.Add(new StayNight { Id = Guid.NewGuid(), FolioId = folio.Id, Date = today, RoomPrice = b.BookingRooms.First().PricePerNight, IsComp = false });
            if (i == 5) { db.Charges.Add(new Charge { Id = Guid.NewGuid(), FolioId = folio.Id, ChargeType = ChargeType.Minibar, Description = "Minibar - Coca Cola", Quantity = 2, UnitPrice = 4m, TotalPrice = 8m, ChargeDate = today, IsTaxable = true }); folio.Balance += 8m; }
        }

        for (int i = 8; i < 11; i++) { var rt = i % 3 == 0 ? rtSuite : rtDouble;
            bookings.Add(new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.HotelWebsite, Type = BookingType.Normal, Status = BookingStatus.Pending, ArrivalDate = today.AddDays(7), DepartureDate = today.AddDays(10), AdultCount = 1, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomTypeId = rt.Id, RatePlanId = Guid.NewGuid(), PricePerNight = rt.DefaultPrice, Status = BookingRoomStatus.Blocked } } });
        }

        for (int i = 11; i < 13; i++)
            bookings.Add(new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.Phone, Type = BookingType.Normal, Status = BookingStatus.Cancelled, ArrivalDate = today.AddDays(-5), DepartureDate = today.AddDays(-2), AdultCount = 2, CancellationReason = "Gost otkazao", CancelledAt = DateTime.UtcNow, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomTypeId = rtDouble.Id, RatePlanId = Guid.NewGuid(), PricePerNight = rtDouble.DefaultPrice, Status = BookingRoomStatus.Released } } });

        for (int i = 13; i < 15; i++) { var room = rooms[i];
            var b = new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.WalkIn, Type = BookingType.Normal, Status = BookingStatus.CheckedOut, ArrivalDate = today.AddDays(-5), DepartureDate = today.AddDays(-2), AdultCount = 2, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomId = room.Id, RoomTypeId = room.RoomTypeId, RatePlanId = Guid.NewGuid(), PricePerNight = room.BasePrice ?? 80, Status = BookingRoomStatus.Released } } };
            b.TotalPrice = b.BookingRooms.Sum(r => r.PricePerNight * 3); b.ExchangeRateTotal = b.TotalPrice;
            bookings.Add(b);
            db.Folios.Add(new Folio { Id = Guid.NewGuid(), FolioNumber = $"F-{b.Id:N}"[..12], BookingId = b.Id, GuestId = b.GuestId, Status = FolioStatus.Closed, Balance = 0, CreatedAt = b.ArrivalDate, ClosedAt = b.DepartureDate });
            db.Payments.Add(new Payment { Id = Guid.NewGuid(), FolioId = Guid.Empty, Amount = b.TotalPrice, PaymentMethod = "Card", Status = PaymentStatus.Paid, PaymentDate = b.DepartureDate });
        }

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }
}
