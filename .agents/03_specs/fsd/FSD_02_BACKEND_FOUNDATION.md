# FSD 2: Backend Foundation

Status: AUTHORITATIVE
Last validated: 2026-05-15

## 1. Cilj

Kreirati .NET 8 Web API sa EF Core koji sluzi kao centralni backend za sve module. Migrirati shemu baze iz starog MySQL sistema (legacy) u PostgreSQL sa modernim imenima i relacijama.

## 2. Projekat

Folder: `backend/`
Solution: `HotelPro.sln`

Projekti:
- `HotelPro.Api` — Web API host, kontrolleri, middleware
- `HotelPro.Core` — entiteti, interfejsi, domain logika
- `HotelPro.Infrastructure` — EF Core DbContext, migracije, repozitoriji
- `HotelPro.Tests` — unit i integration testovi

## 3. Mapiranje legacy tabela na nove entitete

| Legacy (MySQL) | Novi entitet (PostgreSQL) | Napomena |
|----------------|--------------------------|----------|
| `sobe` | `Room` | Dodati `Status` enum |
| `sobavrsta` | `RoomType` | Preimenovati polja na engleski |
| `zgrade` | `Building` | — |
| `gosti` | `Guest` | Dodati `Email`, `Phone` |
| `gostdokument` | `GuestDocument` | — |
| `partneri` | `Partner` | Agencije, kompanije |
| `rezervacije` | `Reservation` | Dodati `Source` enum (direct, booking, airbnb) |
| `rezervacijegrupe` | `ReservationGroup` | — |
| `relgostsoba` | `RoomAssignment` | — |
| `folio` | `FolioEntry` | — |
| `placanje` | `Payment` | Dodati `PaymentMethod` enum |
| `troskovi` | `Expense` | — |
| `radnici` | `Employee` | — |
| `smjene` | `Shift` | — |
| `sobaricalog` | `HousekeepingLog` | — |

## 4. API Verzioniranje

In-house hoteli mogu imati razlicite verzije. API mora podrzavati vise verzija istovremeno.

```
GET /api/v1/rooms          → stari (dok hotel ne upgrade-uje)
GET /api/v2/rooms          → novi (prosireni response)
```

Implementacija:
```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;  // header: api-supported-versions
});
```

## 5. Rate Limiting

Posebno vazno za guest-facing API (smart room, NFC auth). Hotel WiFi je javan.

```csharp
builder.Services.AddRateLimiter(options =>
{
    // Recepcijski API: 100 zahtjeva / 10s po radniku
    options.AddFixedWindowLimiter("staff", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(10);
    });

    // Guest API (smart room): 10 zahtjeva / 10s po sobi
    options.AddFixedWindowLimiter("guest", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
    });

    // Auth endpoint (NFC/PIN): 5 pokusaja / min po IP
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
```

Stope limita se mogu mijenjati per-hotel preko `setings` tabele.

## 6. Audit Log

Svaka promjena u sistemu mora biti zabiljezena — ko je, kad, sta, koju sobu/gosta mijenjao.

```sql
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_type VARCHAR(50) NOT NULL,     -- 'booking', 'payment', 'guest'...
    entity_id UUID NOT NULL,
    action VARCHAR(20) NOT NULL,           -- 'create', 'update', 'delete'
    old_values JSONB,                      -- prije promjene
    new_values JSONB,                      -- poslije promjene
    changed_by VARCHAR(100),               -- radnik ID ili 'system'
    room_id UUID,                          -- opciono: koja soba
    hotel_id UUID,                         -- za multi-tenant
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_audit_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_time ON audit_logs(created_at DESC);
```

Implementacija kroz EF Core interceptor:
```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditable && entry.State != EntityState.Unchanged)
            {
                _auditLogger.Log(new AuditEntry
                {
                    EntityType = entry.Entity.GetType().Name,
                    EntityId = GetId(entry),
                    Action = entry.State.ToString(),
                    OldValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue),
                    NewValues = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue),
                    ChangedBy = _currentUserService.GetUserId(),
                });
            }
        }
    }
}
```

## 7. Migracija legacy MySQL podataka

Legacy baza ima specifivnosti koje zahtijevaju ETL proces:

| Problem | Legacy (MySQL) | Novi (PostgreSQL) | Rjesenje |
|---------|---------------|-------------------|----------|
| Finansijski iznosi | `char(10)` sa zarezima | `decimal(18,2)` | `REPLACE(iznos, ',', '.')` + `CAST` |
| ID-evi | `int AUTO_INCREMENT` | `UUID` | Tabela mapiranja `old_id → new_uuid` |
| Godisnje baze | `hotel2024`, `hotel2025` | Jedinstvena baza | `hotel_id` + `year` kolona |
| Stored procedure | 30+ procedura | C# servisi | Sva logika u aplikaciji |
| Stari tipovi | `tinyint`, `decimal(19,0)` | `boolean`, `integer` | Automatska konverzija |

ETL skripta:
```csharp
// HotelPro.DataMigration/Program.cs — jednokratno pokretanje
public class LegacyMigrator
{
    public async Task MigrateAsync()
    {
        // 1. Citaj iz MySQL
        using var mysql = new MySqlConnection(legacyConnStr);
        var rooms = await mysql.QueryAsync("SELECT * FROM sobe");

        // 2. Mapiraj UUID
        foreach (var room in rooms)
        {
            var newId = Guid.NewGuid();
            _idMapping.Save("sobe", room.id, newId);
        }

        // 3. Upisi u PostgreSQL
        foreach (var room in rooms)
        {
            await _pgContext.Rooms.AddAsync(new Room
            {
                Id = _idMapping.GetNewId("sobe", room.id),
                Name = room.naziv,
                // ...
            });
        }
        await _pgContext.SaveChangesAsync();
    }
}
```

## 8. Konvencije

- Sva imena tabela u PostgreSQL: `snake_case` (npr. `room_types`)
- Svi entiteti u C#: `PascalCase` (npr. `RoomType`)
- Svi API endpointi: `kebab-case` (npr. `/api/room-types`)
- UUID za primarne kljuceve umjesto auto-increment integera
- Audit polja na svakom entitetu: `created_at`, `updated_at`, `created_by`

## 9. Referenca za reverzni inzenjering

Stara MySQL shema: `legacy_app/novaBazaJHotel 20150602 0848.sql`
Stara aplikacija: `legacy_app/Radna.sln`

## 9. Audit log

### 9.1 Konfiguracija (admin panel)
Sve postavke se pode�avaju u admin konzoli (Settings ? Audit):
- Ukljucivanje/iskljucivanje audit-a po tabeli (checkboxes za svaki entitet)
- Rok cuvanja: podesiv (default 90 dana aktivni + 5 godina arhiva)
- Cirkularno logiranje za lokalni fajl sistem (kad fajl dostigne max velicinu, bri�e se najstariji zapis)

### 9.2 �ta se auditira
Minimalno: finansijske transakcije, check-in/out, promjene na rezervacijama, kreiranje/izmjena gostiju.
Opciono (per tabela): svaka CRUD operacija na svim entitetima.

### 9.3 Lokacija
Ista baza, tabela `audit_logs` sa JSONB poljima.
Lokalni fajl sistem: cirkularni log fajl (`/data/hotelpro/logs/audit.log`) kao dodatna opcija.

## 10. Scheduled Tasks (Background Jobs)

Sistemski taskovi koji se izvrsavaju automatski, bez ljudske intervencije.

| Task | Schedule | Opis |
|------|----------|------|
| No-Show detection | hourly | `rezervacije.prijava = 2` za prosle check-in datume |
| Night audit | 00:00 daily | Zakljucavanje prethodnog dana, generisanje nocenja |
| Daily report | 06:00 daily | Generisanje i slanje emailom dnevni promet |
| Backup | 03:00 daily | pg_dump + S3 upload |
| IoT device check | every 5min | Ping kontrolera, alert ako je offline > 15min |
| DND expiry | every 15min | Ako je DND aktivno > 24h, alarm menadzmentu |
| Session cleanup | hourly | Brisanje expired guest sesija (check-out prosao) |

Implementacija:
```csharp
// HotelPro.Api/Jobs/NoShowDetectionJob.cs
public class NoShowDetectionJob : IJob
{
    public async Task Execute(IJobExecutionContext ctx)
    {
        await _db.Bookings
            .Where(b => b.CheckInDate < DateTime.UtcNow.AddDays(-1)
                     && b.Status == BookingStatus.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, BookingStatus.NoShow));
    }
}
```

## 11. Feature Flags — postepeno uvodjenje funkcionalnosti

In-house hoteli ne upgrade-uju svi istovremeno. Feature flags omogucavaju postepeno uvodjenje.

```json
// appsettings.json per hotel
{
  "Features": {
    "SmartRoom": {
      "Enabled": false,   // nijedan hotel jos nema smart room
      "Hotels": []        // lista UUID hotel-a koji imaju
    },
    "ChannelManager": {
      "Enabled": true,
      "Hotels": ["uuid-1", "uuid-2"]
    },
    "NewCheckoutFlow": {
      "Enabled": true,
      "Percentage": 10    // samo 10% hotela
    }
  }
}
```

Backend:
```csharp
public class FeatureFlagService
{
    public bool IsEnabled(string feature, string hotelId)
    {
        var config = _configuration.GetSection($"Features:{feature}");
        if (!config["Enabled"].Equals("true")) return false;

        var hotels = config.GetSection("Hotels").Get<string[]>();
        if (hotels?.Contains(hotelId) == true) return true;

        var percentage = config.GetValue<int>("Percentage");
        if (percentage > 0 && hotelId.GetHashCode() % 100 < percentage) return true;

        return false;
    }
}
```

## 12. Multi-language

Hoteli u razlicitim regijama zahtijevaju razlicite jezike.

```json
{
  "settings": {
    "language": "hr",     // hrvatski
    "languages": ["hr", "en", "de", "it"]
  }
}
```

Sve poruke u sistemu (errori, notifikacije, emailovi) idu kroz `ITranslationService`:
```csharp
public interface ITranslationService
{
    string Translate(string key, string language);
}
```

## 13. Restrikcije

- Ne kopirati staru shemu doslovno — modernizovati imenovanje i tipove
- Ne koristiti stored procedure — sva logika u C# servisu
- Svaki API endpoint mora imati Swagger dokumentaciju
- Svaka migracija mora biti reverzibilna (`Up` + `Down`)
- SVMaki API mora podrzavati najmanje 2 verzije unazad
