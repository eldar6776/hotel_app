using HotelPro.Core.Entities;
using HotelPro.Core.Enums;
using HotelPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelPro.Infrastructure.Data;

public static class TestDataSeeder
{
    private static readonly Random Rng = new(42);

    public static void Seed(HotelProDbContext db)
    {
        // Re-entrant checks - only seed what's missing
        bool needB = !db.Buildings.Any(x => x.IsActive);
        bool needR = !db.Rooms.Any(x => x.IsActive && x.Building != null);
        bool needG = !db.Guests.Any(x => x.IsActive && x.GdprConsentGiven);
        bool needBK = !db.Bookings.Any();

        if (!needB && !needR && !needG && !needBK) return;

        Building b1 = null!, b2 = null!, b3 = null!;
        if (needB)
        {
            b1 = new Building { Id = Guid.NewGuid(), Name = "Glavna zgrada", Code = "A", IsActive = true };
            b2 = new Building { Id = Guid.NewGuid(), Name = "Depandansa", Code = "B", IsActive = true };
            b3 = new Building { Id = Guid.NewGuid(), Name = "Vila", Code = "C", IsActive = true };
            db.Buildings.AddRange(b1, b2, b3);
            db.SaveChanges();
        }

        var rtS = db.RoomTypes.First(x => x.Code == "SGL");
        var rtD = db.RoomTypes.First(x => x.Code == "DBL");
        var rtX = db.RoomTypes.First(x => x.Code == "STE");

        List<Room> rooms = new();
        if (needR)
        {
            if (!needB) { b1 = db.Buildings.First(x => x.Code == "A"); b2 = db.Buildings.First(x => x.Code == "B"); b3 = db.Buildings.First(x => x.Code == "C"); }
            for (int f = 1; f <= 3; f++) for (int r = 1; r <= 4; r++) { var rt = f == 3 ? rtX : (r % 2 == 0 ? rtD : rtS); rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"{f}0{r}", Floor = f, BuildingId = b1.Id, RoomTypeId = rt.Id, Status = RoomStatus.Free, BasePrice = rt.DefaultPrice, IsActive = true }); }
            for (int f = 1; f <= 2; f++) for (int r = 1; r <= 4; r++) { var rt = r <= 2 ? rtD : rtS; rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"B{f}0{r}", Floor = f, BuildingId = b2.Id, RoomTypeId = rt.Id, Status = RoomStatus.Free, BasePrice = rt.DefaultPrice, IsActive = true }); }
            for (int r = 1; r <= 5; r++) rooms.Add(new Room { Id = Guid.NewGuid(), RoomNumber = $"C{r:D2}", Floor = 1, BuildingId = b3.Id, RoomTypeId = rtX.Id, Status = RoomStatus.Free, BasePrice = rtX.DefaultPrice, IsActive = true });
            db.Rooms.AddRange(rooms);
            db.SaveChanges();
        }

        List<Guest> guests = new();
        if (needG)
        {
            var names = new (string F, string L, string? E, string? P)[] { ("Marko","Horvat","m.horvat@email.hr","+385911234567"),("Ivana","Kovacic","i.kovacic@email.hr","+385922345678"),("Petar","Babic","p.babic@email.hr",null),("Ana","Maric","a.maric@email.hr","+385953456789"),("Tomislav","Novak",null,"+385984567890"),("Marija","Juric","m.juric@email.hr",null),("Ivan","Vukovic","i.vukovic@email.hr","+385995678901"),("Katarina","Pavic",null,null),("Stjepan","Radic","s.radic@email.hr","+385916789012"),("Jelena","Tomic","j.tomic@email.hr","+385927890123") };
            for (int i = 0; i < names.Length; i++) { var g = names[i]; guests.Add(new Guest { Id = Guid.NewGuid(), FirstName = g.F, LastName = g.L, Email = g.E, Phone = g.P, DateOfBirth = new DateTime(1970 + Rng.Next(30), Rng.Next(1, 13), Rng.Next(1, 28), 0, 0, 0, DateTimeKind.Utc), Gender = i % 2 == 0 ? "M" : "F", City = "Zagreb", Address = $"Ulica {i + 1}", GdprConsentGiven = true, GdprConsentDate = DateTime.UtcNow, GdprConsentVersion = "1.0", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }); }
            db.Guests.AddRange(guests);
            db.SaveChanges();
        }

        if (needBK)
        {
            if (needG) guests = db.Guests.ToList();
            if (needR) rooms = db.Rooms.ToList();
            else { rooms = db.Rooms.Where(r => r.IsActive && r.Building != null).ToList(); guests = db.Guests.Where(g => g.IsActive).ToList(); }

            var today = DateTime.UtcNow.Date;
            var bookings = new List<Booking>();

            for (int i = 0; i < 5; i++) { var r = rooms[i]; r.Status = RoomStatus.Reserved;
                bookings.Add(CreateBooking(guests[i].Id, BookingStatus.Confirmed, today, today.AddDays(3 + i % 3), 1 + i % 2, r, BookingSource.BookingCom)); }

            for (int i = 5; i < 8; i++) { var r = rooms[i]; r.Status = RoomStatus.Occupied; var n = 2 + i % 3;
                var b = CreateBooking(guests[i].Id, BookingStatus.CheckedIn, today.AddDays(-1), today.AddDays(n - 1), 2, r, BookingSource.Direct);
                bookings.Add(b);
                db.Folios.Add(new Folio { Id = Guid.NewGuid(), FolioNumber = "F-" + Guid.NewGuid().ToString()[..12], BookingId = b.Id, GuestId = b.GuestId, Status = FolioStatus.Open, Balance = b.TotalPrice, CreatedAt = b.ArrivalDate }); }

            for (int i = 8; i < 10; i++) { var rt = i % 2 == 0 ? rtD : rtX;
                bookings.Add(new Booking { Id = Guid.NewGuid(), GuestId = guests[i].Id, Source = BookingSource.HotelWebsite, Type = BookingType.Normal, Status = BookingStatus.Pending, ArrivalDate = today.AddDays(7), DepartureDate = today.AddDays(10), AdultCount = 1, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomTypeId = rt.Id, RatePlanId = Guid.NewGuid(), PricePerNight = rt.DefaultPrice, Status = BookingRoomStatus.Blocked } } }); }

            db.Bookings.AddRange(bookings);
            db.SaveChanges();
        }
    }

    private static Booking CreateBooking(Guid guestId, BookingStatus status, DateTime arr, DateTime dep, int adults, Room room, BookingSource source)
    {
        var n = (dep - arr).Days; if (n < 1) n = 1;
        var price = room.BasePrice ?? 80;
        var b = new Booking { Id = Guid.NewGuid(), GuestId = guestId, Source = source, Type = BookingType.Normal, Status = status, ArrivalDate = arr, DepartureDate = dep, AdultCount = adults, Currency = "EUR", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, BookingRooms = new List<BookingRoom> { new() { Id = Guid.NewGuid(), RoomId = room.Id, RoomTypeId = room.RoomTypeId, RatePlanId = Guid.NewGuid(), PricePerNight = price, Status = status == BookingStatus.CheckedIn ? BookingRoomStatus.Occupied : BookingRoomStatus.Blocked } } };
        b.TotalPrice = price * n; b.ExchangeRateTotal = b.TotalPrice;
        return b;
    }
}
