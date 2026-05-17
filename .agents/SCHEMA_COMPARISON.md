# DATABASE SCHEMA COMPARISON: Docs vs Actual PostgreSQL

## TABLES IN BOTH (docs + actual)

| Tabela | Legacy docs | PostgreSQL | Razlike |
|--------|------------|------------|---------|
| access_logs | ✅ | ✅ | Dokumenti: `AccessGranted`, `AccessDenied`, `PinVerified`; DB: bez tih enum vrednosti |
| amenities | ✅ | ✅ | OK |
| audit_logs | ✅ | ✅ | Docs: `entity_type`, `room_id`, `hotel_id`; DB: `EntityName`, `OldValues`, `NewValues`, `ChangedProperties`, `ChangedById`, `ChangedByEmail`, `IpAddress` |
| booking_histories | ✅ | ✅ | OK |
| booking_rooms | ✅ | ✅ | **Docs**: `guest_id`, `check_in_date`, `check_out_date`, `adults`, `children`, `is_main_guest`. **DB**: **NEMA nijedno** — ima `RoomTypeId`, `RatePlanId` |
| bookings | ✅ | ✅ | **Docs**: `booking_number UNIQUE`, `booking_type_id FK`, `booking_source_id FK`, `partner_id`, `sales_agent_id`, `payment_status`, `confirmation_code`, `created_by_id`. **DB**: **NEMA nijedno** — ima `HotelId`, `GroupId`, `ExchangeRateTotal`, `InternalNotes` |
| buildings | ✅ | ✅ | OK |
| charges | ✅ | ✅ | DB ima `ChargeType` (enum), `POSReference` — docs ih nemaju |
| countries | ✅ | ✅ | OK |
| employees | ✅ | ✅ | Docs: `pin_code VARCHAR(6)`; DB: `PinHash VARCHAR(256)` (BCrypt). Docs: `password_hash`; DB: `PasswordHash` |
| expenses | ✅ | ✅ | OK |
| folios | ✅ | ✅ | Docs: `status=Open,Closed,Archived`. DB: `UpdatedAt` dodatno |
| group_bookings | ✅ | ✅ | Docs: `group_name`, `booking_id`, `member_booking_id`. DB: `GroupId`, `BookingId`, `RoomTypeId` — **potpuno drugacije** |
| guest_documents | ✅ | ✅ | Docs: `DocumentType=VARCHAR`; DB: `DocumentType=INTEGER`. DB ima `MRZLine1`, `MRZLine2`, `FrontImagePath`, `BackImagePath`, `CreatedAt` |
| guests | ✅ | ✅ | Docs: `id_document_type`, `id_document_number`, `nationality VARCHAR`, `email UNIQUE`. DB: **NEMA** `id_document_type`, `id_document_number`, `nationality` — ima `NationalityCountryId INT`, `GdprConsentGiven`, `GdprConsentDate`, `GdprConsentVersion` |
| hotels | ✅ | ✅ | Docs: nema detalje; DB: `Code`, `Address`, `City`, `Country`, `Phone`, `Email`, `Currency`, `TimeZone`, `VatNumber`, `LogoUrl`, `IsActive` |
| housekeeping_logs | ✅ | ✅ | OK |
| invoice_items | ✅ | ✅ | OK |
| invoices | ✅ | ✅ | OK |
| outstanding_balances | ✅ | ✅ | OK |
| partners | ✅ | ✅ | OK |
| payment_methods | ✅ | ✅ | OK |
| payments | ✅ | ✅ | DB: `PaymentMethod VARCHAR(50)`, `Status VARCHAR(20)` dodatno |
| room_assignments | ✅ | ✅ | OK |
| room_types | ✅ | ✅ | OK |
| rooms | ✅ | ✅ | Docs: `sort_order`. DB: `SortOrder` |
| sales_agents | ✅ | ✅ | OK |
| service_catalog | ✅ | ✅ | OK |
| shifts | ✅ | ✅ | OK |
| stay_nights | ✅ | ✅ | OK |
| tariffs | ✅ | ✅ | OK |
| work_orders | ✅ | ✅ | OK |

## TABLES FROM DOCS BUT **MISSING** IN DB

| Tabela | Opis iz legacy dokumentacije |
|--------|------------------------------|
| **booking_sources** | Izvori rezervacije (BookingCom, Expedia...) — **pretvoreni u enum** |
| **booking_types** | Tipovi rezervacije (Normal, Group...) — **pretvoreni u enum** |
| **guest_check_ins** | Aktivni check-ini (legacy `gostipid`) |
| **prepayments** | Avansne uplate (legacy `printracuniavans`) |
| **invoice_summaries** | Footer računa (legacy `printracunifooter`) |
| **phone_calls** | Telefonski pozivi (legacy `telpozivi`) |
| **phone_call_archives** | Arhiva poziva (legacy `telpozivi_stara`) |
| **phone_directories** | Telefonski imenik (legacy `telefonskiimenik`) |
| **webhook_subscriptions** | Webhook pretplate za channel manager |

## TABLES IN DB BUT **NOT IN DOCS**

| Tabela | Odakle |
|--------|--------|
| AdvancePayments | T9.2 (dodato tokom implementacije) |
| DayLocks | T7.5 (dodato tokom implementacije) |
| ExchangeRates | T9.4 (dodato tokom implementacije) |
| GuestStayHistories | T7.1/T8.3 (dodato tokom implementacije) |
| InvoiceSequences | T9.1 (dodato tokom implementacije) |
| ProformaInvoices | T9.2 (dodato tokom implementacije) |
| __EFMigrationsHistory | EF Core automatski |
| booking_groups | T6.5 |
| email_logs | T6.4 |
| feature_flags | T2.12 |
| legacy_id_mapping | T2.14 (ETL migrator) |
| master_bills | T6.5 |
| night_audit_logs | T7.5 |
| phone_extensions | T12.4 |
| refresh_tokens | T3.1 |
| room_out_of_order | T5.4 |

## KLJUCNE KOLONE KOJE FALE — BOOKINGS

Ovo je legacy spec za `bookings` koju originalna aplikacija koristi:

```
booking_number VARCHAR(20) UNIQUE     ❌ NEMA
booking_type_id UUID FK               ❌ (enum umesto tabele)
booking_source_id UUID FK             ❌ (enum umesto tabele)
partner_id UUID FK                    ❌ NEMA
sales_agent_id UUID FK                ❌ NEMA
payment_status VARCHAR                ❌ NEMA
confirmation_code VARCHAR             ❌ NEMA
created_by_id UUID FK                 ❌ NEMA
```

Sve ovo postoji u originalnoj bazi. Mi nemamo **nijednu** od ovih kolona.

## KLJUCNE KOLONE KOJE FALE — BOOKING_ROOMS

```
guest_id UUID FK              ❌ NEMA
check_in_date TIMESTAMP       ❌ NEMA
check_out_date TIMESTAMP      ❌ NEMA
adults INT                    ❌ NEMA
children INT                  ❌ NEMA
is_main_guest BOOLEAN         ❌ NEMA
```
