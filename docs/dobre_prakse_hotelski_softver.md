# Dobre prakse hotelskih softvera

Stavke koje nisu u planu a standard su u modernim PMS sistemima.

---

## 1. POS integracija (restoran/bar)

Legacy fiskalni printeri su pokriveni, ali nedostaje **billing integracija** — gost jede u restoranu, račun automatski ide na njegovu sobu.

```csharp
// POS šalje trošak direktno na folio sobe
POST /api/folio/101/charge
{
  "from": "RESTAURANT_POS",
  "items": [
    { "name": "Pizza Margherita", "qty": 1, "price": 12.00 },
    { "name": "Coca-Cola", "qty": 2, "price": 3.00 }
  ],
  "total": 18.00,
  "operator": "Konobar Ivan"
}
```

Standard: POS sistem → webhook → backend → knjiženje na folio gosta. Sve popularne POS platforme u regiji (Pixel, Ontech, Loco) imaju REST API.

---

## 2. Grupne rezervacije i eventi

Legacy rezervacije podržavaju grupe kroz `rezervacijegrupe`, ali nedostaje:
- **Blokiranje soba** (10 soba za svatove, otpuštaju se 72h prije)
- **Master račun** (mladenci plaćaju sobe za goste)
- **Posebni cjenovnici** za grupe (15% popusta za 20+ soba)
- **Raspored sjedenja** za evente

```json
{
  "groupName": "Vjenčanje Ivanović",
  "type": "WEDDING",
  "blockedRooms": 15,
  "releaseDate": "2026-06-01",
  "masterBill": { "payingGuestId": 123 },
  "specialRate": { "discount": 15, "includes": ["breakfast"] }
}
```

---

## 3. Work Orders (Održavanje)

Odvojeno od housekeeping-a. Sistemski zapisi za kvarove:

| Prijava | Hitnost | Status |
|---------|---------|--------|
| "Klima ne hladi, soba 204" | VISOKA | U toku |
| "Slavina kaplje, soba 112" | NISKA | Čeka |
| "TV ne radi, soba 305" | SREDNJA | Završen |

```csharp
public class WorkOrder
{
    Guid Id;
    string RoomId;
    string ReportedBy;       // sobarica, gost, recepcija
    string Description;
    Priority Priority;       // LOW, MEDIUM, HIGH, EMERGENCY
    string AssignedTo;       // tehničar
    DateTime? CompletedAt;
    string Resolution;
}
```

**Prijava od gosta** — smart room form: "Pokvaren TV" → automatski kreira work order.

---

## 4. Multi-Property (lanac hotela)

Ako ista kompanija ima više hotela. Trenutni plan ima `hotel_id` u tabelama ali nema:
- Centralna rezervacija — jedan poziv rezerviše u bilo kom hotelu lanca
- Shared inventory — sobe u Hotel A + Hotel B kao jedan inventar
- Centralni CRM — gost koji dolazi u bilo koji hotel vidi se na jednom profilu
- Unificirani izvještaji — revenue za sve hotele

Najjednostavniji pristup: `hotel_id` kolona na svakoj tabeli + filter kroz middleware.

---

## 5. Loyalty program

Ohrabriti direktne rezervacije (bez Booking.com/Airbnb provizije od 15-20%):

| Nivo | Bodovi po € | Pogodnosti |
|------|------------|------------|
| Standard | 1x | — |
| Silver | 1.5x | Free upgrade |
| Gold | 2x | Upgrade + late checkout |
| Platinum | 3x | Upgrade + late checkout + welcome gift |

Implementacija:
```sql
CREATE TABLE loyalty_points (
    guest_id UUID REFERENCES guests(id),
    points INTEGER DEFAULT 0,
    tier VARCHAR(20) DEFAULT 'standard',
    valid_until DATE
);
```

**Auto-upgrade:** `SUM(points) > threshold` → sljedeći nivo.

---

## 6. PCI DSS i GDPR compliance

### PCI DSS (payment card data)
Ako sistem obrađuje kartična plaćanja:
- Nikad ne čuvati pun broj kartice, CVV, PIN
- Koristiti tokenizaciju (Stripe/PayPal/Monri)
- Sve transakcije kroz payment gateway, ne direktno

### GDPR (guest data)
- Guest može zahtijevati izvoz svih svojih podataka (`GET /api/gdpr/export/{guestId}`)
- Guest može zahtijevati brisanje (`DELETE /api/gdpr/forget/{guestId}`)
- Log pristupa ličnim podacima (ko je gledao profil gosta)

Predlažem poseban task u Fazi 18 (Stabilizacija):
```
T18.5: GDPR export/forget endpointi + PCI DSS audit
```

---

## 7. Data Analytics i forecasting

Predvidjeti kapacitet i prihod:
- Historijska popunjenost (isti mjesec prošle godine)
- Forecasting na osnovu trenutnih rezervacija + historije
- Revenue alerts: "Popunjenost za august je 30% niža nego prošle godine"

Implementacija kroz pozadinske jobove:
```csharp
public class OccupancyForecastJob : IJob
{
    public async Task Execute(IJobExecutionContext ctx)
    {
        // 1. Izračunaj popunjenost za sljedećih 90 dana
        var forecast = await _analytics.ForecastOccupancy(90);

        // 2. Uporedi sa prošlom godinom
        var lastYear = await _analytics.GetHistoricalOccupancy(
            DateTime.UtcNow.AddYears(-1), 90);

        // 3. Ako je pad > 20%, pošalji upozorenje
        if (forecast.Average < lastYear.Average * 0.8)
            await _notificationService.AlertManagement(
                "Značajan pad popunjenosti u odnosu na prošlu godinu");
    }
}
```

---

## Status implementacije u planu

| Stavka | Prioritet | Faza | Task | Status |
|--------|-----------|------|------|--------|
| POS integracija | HIGH | Faza 9 | T9.5 | ✅ U planu |
| Grupne rezervacije | HIGH | Faza 6 | T6.5 | ✅ U planu |
| Work Orders | HIGH | Faza 11 | T11.4 | ✅ U planu |
| GDPR compliance | HIGH | Faza 18 | T18.4 | ✅ U planu |
| PCI DSS audit | HIGH | Faza 18 | T18.5 | ✅ U planu |
| Multi-Property | LOW | Faza 18 | — | 📝 Razmotriti |
| Loyalty program | MEDIUM | Faza 16 | — | 📝 Razmotriti |
| Analytics/forecast | MEDIUM | Faza 10 | — | 📝 Razmotriti |
