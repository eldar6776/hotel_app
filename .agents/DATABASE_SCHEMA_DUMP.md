# DATABASE SCHEMA DUMP — HotelPro (PostgreSQL)

> **Generated:** 2026-05-17  
> **Sources:** FSD_02, FSD_04, FSD_05, FSD_06, FSD_07, FSD_08, FSD_10, FSD_11, FSD_12, analiza_hotelskog_sistema.md, T2.3–T2.7  
> **Convention:** PostgreSQL tables = `snake_case`, C# entities = `PascalCase`  
> **All PKs:** `UUID` (generated at application level, `gen_random_uuid()`)  
> **Audit columns** on most entities: `created_at TIMESTAMP`, `updated_at TIMESTAMP`  

---

## LEGACY → NEW TABLE MAPPING

| Legacy (MySQL) | New Entity (C#) | New Table (PG) |
|---|---|---|
| `sobe` | `Room` | `rooms` |
| `sobavrsta` | `RoomType` | `room_types` |
| `sobavrsta1` | (duplicate — verify usage) | — |
| `sobev` | (virtual rooms — verify) | — |
| `zgrade` | `Building` | `buildings` |
| `sobatarifa` | `Tariff` | `tariffs` |
| `sobasadrzaji` | `Amenity` | `amenities` |
| `logcont` | `AccessLog` | `access_logs` |
| `gosti` | `Guest` | `guests` |
| `gostdokument` | `GuestDocument` | `guest_documents` |
| `gostipid` | `GuestCheckIn` (aux) | `guest_check_ins` |
| `drzave` | `Country` | `countries` |
| `partneri` | `Partner` | `partners` |
| `komercijalista` | `SalesAgent` | `sales_agents` |
| `rezervacije` | `Booking` | `bookings` |
| `rezervacije1` | `BookingHistory` (archive) | `booking_histories` |
| `rezervacijasobe` | `BookingRoom` | `booking_rooms` |
| `rezervacijasobe1` | (archive — verify) | — |
| `rezervacijegrupe` | `GroupBooking` | `group_bookings` |
| `rezervacijeizvor` | `BookingSource` | `booking_sources` |
| `rezervacijetip` | `BookingType` | `booking_types` |
| `rezervacijaprijava` | (check-in sub — verify) | — |
| `relgostsoba` | `RoomAssignment` | `room_assignments` |
| `folio` | `Folio` | `folios` |
| `nocenja` | `StayNight` | `stay_nights` |
| `troskovi` | `Charge` | `charges` |
| `troskovivrste` | `ServiceCatalog` | `service_catalogs` |
| `troskovipojedinacni` | (individual charges — verify) | — |
| `troskovisuma` | (charge summary — verify) | — |
| `neplaceni` | `OutstandingBalance` | `outstanding_balances` |
| `posjetafolio` | (folio archive — verify) | — |
| `placanje` | `Payment` | `payments` |
| `placanjedetalji` | (payment details — verify) | — |
| `nplac` | `PaymentMethod` | `payment_methods` |
| `printracuni` | `Invoice` | `invoices` |
| `printracunidetalji` | `InvoiceItem` | `invoice_items` |
| `printracuniavans` | `Prepayment` | `prepayments` |
| `printracunifooter` | `InvoiceSummary` | `invoice_summaries` |
| `fiskalni` | (fiscal receipt — verify) | — |
| `radnici` | `Employee` | `employees` |
| `smjene` | `Shift` | `shifts` |
| `sobaricalog` | `HousekeepingLog` | `housekeeping_logs` |
| `telpozivi` | `PhoneCall` | `phone_calls` |
| `telpozivi_stara` | `PhoneCallArchive` | `phone_call_archives` |
| `telefonskiimenik` | `PhoneDirectory` | `phone_directories` |
| `kursna` | (exchange rate — verify) | — |
| (new) | `WorkOrder` | `work_orders` |
| (new) | `AuditLog` | `audit_logs` |
| (new) | `WebhookSubscription` | `webhook_subscriptions` |

---

## TABLE DEFINITIONS

### Table: buildings (`Building`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK, `DEFAULT gen_random_uuid()` |
| `name` | `VARCHAR(100)` | NOT NULL |
| `code` | `VARCHAR(10)` | NOT NULL |
| `address` | `VARCHAR` | NULL |
| `city` | `VARCHAR` | NULL |
| `postal_code` | `VARCHAR` | NULL |
| `country` | `VARCHAR` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `updated_at` | `TIMESTAMP` | NOT NULL |

**Soft delete filter:** `IsActive == true`

---

### Table: room_types (`RoomType`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `code` | `VARCHAR(10)` | NOT NULL |
| `base_capacity` | `INT` | NOT NULL |
| `max_capacity` | `INT` | NOT NULL |
| `default_price` | `DECIMAL(18,2)` | NOT NULL |
| `description` | `VARCHAR` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `sort_order` | `INT` | NOT NULL |

**Legacy field:** `brojKreveta` (migrated as `base_capacity` / `max_capacity`)

---

### Table: rooms (`Room`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `room_number` | `VARCHAR(10)` | NOT NULL |
| `floor` | `INT` | NOT NULL |
| `building_id` | `UUID` | NOT NULL, FK → `buildings.id` |
| `room_type_id` | `UUID` | NOT NULL, FK → `room_types.id` |
| `status` | `VARCHAR(20)` | NOT NULL (enum as string) |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `notes` | `VARCHAR` | NULL |
| `base_price` | `DECIMAL(18,2)` | NULL (override of RoomType.DefaultPrice) |
| `sort_order` | `INT` | NOT NULL |

**Legacy mapped fields:**  
| Legacy column | New mapping |
|---|---|
| `ooo` (tinyint) | `status = 'OutOfOrder'` |
| `clean` (tinyint) | `status = 'Dirty'` when 0 |
| `idkon` | (mapped to hardware controller config, not stored here) |
| `vrsta` (FK sobavrsta) | `room_type_id` |
| `zgradaID` (FK zgrade) | `building_id` |
| `razlog` | `notes` |
| `naziv` | `room_number` |
| `lokal` | (phone extension — stored on phone_calls side) |
| `sos`, `vatr` | (IoT alarm state, computed from hardware) |

**Index:** `(room_number, building_id)` UNIQUE

---

### RoomStatus Enum (C# — stored as VARCHAR(20) in PG)
```
Free, Occupied, Reserved, Dirty, OutOfOrder, OutOfService
```

**Legacy status mapping (`fnSobaStatus`):**
| Legacy int | Meaning | New enum |
|---|---|---|
| 0 | Slobodna | `Free` |
| 1 | Zauzeta | `Occupied` |
| 2 | Zauzeta/Odjava | `Occupied` (with checkout flag) |
| 3 | Rezervisana/Potvrđena | `Reserved` |
| 4 | Rezervisana i Zauzeta | `Reserved` + `Occupied` overlap |
| 5 | Van upotrebe (OOO) | `OutOfOrder` |
| 6 | Rezervisana/Nepotvrđena | `Reserved` |

**Lifecycle state machine:**
```
Free → Reserved → Occupied → Dirty → Free
Free → OutOfOrder → Free
```

---

### Table: tariffs (`Tariff`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `room_type_id` | `UUID` | NULL, FK → `room_types.id` |
| `valid_from` | `TIMESTAMP` | NULL |
| `valid_to` | `TIMESTAMP` | NULL |
| `base_price` | `DECIMAL(18,2)` | NOT NULL |
| `currency` | `VARCHAR` | NOT NULL, DEFAULT `'EUR'` |
| `is_active` | `BOOLEAN` | NOT NULL |

---

### Table: amenities (`Amenity`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `icon` | `VARCHAR` | NULL (icon identifier) |
| `is_active` | `BOOLEAN` | NOT NULL |

---

### Table: countries (`Country`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `code` | `VARCHAR(3)` | NOT NULL (ISO 3166-1 alpha-3) |
| `name` | `VARCHAR(100)` | NOT NULL |
| `nationality` | `VARCHAR(100)` | NOT NULL |
| `phone_code` | `VARCHAR(5)` | NOT NULL |
| `currency_code` | `VARCHAR(3)` | NOT NULL |

---

### Table: guests (`Guest`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `first_name` | `VARCHAR(100)` | NOT NULL |
| `last_name` | `VARCHAR(100)` | NOT NULL |
| `date_of_birth` | `TIMESTAMP` | NULL |
| `gender` | `VARCHAR(10)` | NOT NULL |
| `email` | `VARCHAR(255)` | NULL, UNIQUE |
| `phone` | `VARCHAR(50)` | NULL |
| `address` | `VARCHAR` | NULL |
| `city` | `VARCHAR` | NULL |
| `postal_code` | `VARCHAR` | NULL |
| `country_id` | `UUID` | NULL, FK → `countries.id` |
| `id_document_type` | `VARCHAR` | NULL |
| `id_document_number` | `VARCHAR` | NULL |
| `nationality` | `VARCHAR(100)` | NULL |
| `is_company` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` |
| `company_name` | `VARCHAR` | NULL |
| `vat_number` | `VARCHAR` | NULL |
| `notes` | `VARCHAR` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `updated_at` | `TIMESTAMP` | NOT NULL |

**Indexes:** `(last_name, first_name)` for search, `(email)` UNIQUE (when not null)  
**Soft delete filter:** `IsActive == true`  
**Legacy mapped:** `DID` → `country_id`, `Rid` → (via room_assignments), `dokument` (1=Pasos,2=Licna) → `id_document_type`, `brDokument` → `id_document_number`

---

### Table: guest_documents (`GuestDocument`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `guest_id` | `UUID` | NOT NULL, FK → `guests.id` |
| `document_type` | `VARCHAR` | NOT NULL |
| `document_number` | `VARCHAR(50)` | NOT NULL |
| `issuing_country` | `VARCHAR(100)` | NOT NULL |
| `issue_date` | `TIMESTAMP` | NULL |
| `expiry_date` | `TIMESTAMP` | NULL |
| `file_url` | `VARCHAR` | NULL (path to scanned doc, NOT base64) |
| `is_verified` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` |
| `notes` | `VARCHAR` | NULL |

**DocumentType values (C# enum, stored as string):** `Passport`, `IdCard`, `Visa`, `Other`

**MRZ parsing (OCR):** TD3 (Passport, 2 lines × 44 chars), TD1 (ID card, 3 lines × 30 chars).  
Backend validates: checksum, age >18, document expiry.

---

### Table: guest_check_ins (`GuestCheckIn`)
Auxiliary table for active check-ins (legacy `gostipid`). Linked to `room_assignments`.

---

### Table: partners (`Partner`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(200)` | NOT NULL |
| `partner_type` | `VARCHAR` | NOT NULL |
| `contact_person` | `VARCHAR` | NULL |
| `email` | `VARCHAR` | NULL |
| `phone` | `VARCHAR` | NULL |
| `address` | `VARCHAR` | NULL |
| `city` | `VARCHAR` | NULL |
| `country_id` | `UUID` | NULL, FK → `countries.id` |
| `contract_code` | `VARCHAR` | NULL |
| `commission_percent` | `DECIMAL(5,2)` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `notes` | `VARCHAR` | NULL |

**PartnerType values (stored as string):** `TourOperator`, `TravelAgency`, `Corporate`, `Other`  
**Soft delete filter:** `IsActive == true`  
**Legacy mapped:** `pdv`, `idd` → fiscal ID fields, `rabat` → `commission_percent`, `brdanodg` → payment terms (days deferred)

---

### Table: sales_agents (`SalesAgent`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `first_name` | `VARCHAR(100)` | NOT NULL |
| `last_name` | `VARCHAR(100)` | NOT NULL |
| `email` | `VARCHAR` | NULL |
| `phone` | `VARCHAR` | NULL |
| `partner_id` | `UUID` | NULL, FK → `partners.id` |
| `commission_percent` | `DECIMAL(5,2)` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL |

---

### Table: booking_sources (`BookingSource`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `code` | `VARCHAR(10)` | NOT NULL |
| `is_active` | `BOOLEAN` | NOT NULL |

**Legacy:** `rezervacijeizvor` / `SifreIzvorRez`

---

### Table: booking_types (`BookingType`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `code` | `VARCHAR(10)` | NOT NULL |
| `color` | `VARCHAR` | NULL (hex color for UI Gantt) |
| `is_active` | `BOOLEAN` | NOT NULL |

**Legacy:** `rezervacijetip` / `SifreVrstaRez`

---

### Table: bookings (`Booking`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `booking_number` | `VARCHAR(20)` | NOT NULL, UNIQUE |
| `guest_id` | `UUID` | NOT NULL, FK → `guests.id` |
| `booking_type_id` | `UUID` | NULL, FK → `booking_types.id` |
| `booking_source_id` | `UUID` | NULL, FK → `booking_sources.id` |
| `partner_id` | `UUID` | NULL, FK → `partners.id` |
| `sales_agent_id` | `UUID` | NULL, FK → `sales_agents.id` |
| `status` | `VARCHAR` | NOT NULL |
| `arrival_date` | `TIMESTAMP` | NOT NULL |
| `departure_date` | `TIMESTAMP` | NOT NULL |
| `adults` | `INT` | NOT NULL |
| `children` | `INT` | NOT NULL, DEFAULT `0` |
| `total_price` | `DECIMAL(18,2)` | NOT NULL |
| `currency` | `VARCHAR` | NOT NULL, DEFAULT `'EUR'` |
| `payment_status` | `VARCHAR` | NOT NULL |
| `notes` | `VARCHAR` | NULL |
| `internal_notes` | `VARCHAR` | NULL |
| `confirmation_code` | `VARCHAR` | NULL |
| `created_by_id` | `UUID` | NULL, FK → `employees.id` |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `updated_at` | `TIMESTAMP` | NOT NULL |
| `cancelled_at` | `TIMESTAMP` | NULL |
| `cancellation_reason` | `VARCHAR` | NULL |

**BookingStatus (stored as string):** `Provisional`, `Confirmed`, `CheckedIn`, `CheckedOut`, `Cancelled`, `NoShow`  
**PaymentStatus (stored as string):** `Unpaid`, `Partial`, `Paid`, `Refunded`  
**Indexes:** `(guest_id)`, `(booking_number)` UNIQUE  
**Legacy mapped:** `potvrda` (0/1) → Status, `stornirana` (0/1) → Status=Cancelled, `prijava` (0=Waiting, 1=CheckedIn, 2=NoShow) → Status, `alarmid` → reminder system

---

### Table: booking_rooms (`BookingRoom`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `booking_id` | `UUID` | NOT NULL, FK → `bookings.id` |
| `room_id` | `UUID` | NOT NULL, FK → `rooms.id` |
| `guest_id` | `UUID` | NULL, FK → `guests.id` (primary guest for this room) |
| `check_in_date` | `TIMESTAMP` | NOT NULL |
| `check_out_date` | `TIMESTAMP` | NOT NULL |
| `adults` | `INT` | NOT NULL |
| `children` | `INT` | NOT NULL |
| `price_per_night` | `DECIMAL(18,2)` | NOT NULL |
| `is_main_guest` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` |
| `status` | `VARCHAR` | NOT NULL |

**Status values (stored as string):** `Pending`, `CheckedIn`, `CheckedOut`, `Cancelled`  
**Indexes:** `(booking_id)`, `(room_id)`  
**Legacy:** `rezervacijasobe` — enables M:N between bookings and rooms

---

### Table: group_bookings (`GroupBooking`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `group_name` | `VARCHAR(200)` | NOT NULL |
| `booking_id` | `UUID` | NOT NULL, FK → `bookings.id` (master booking) |
| `member_booking_id` | `UUID` | NOT NULL, FK → `bookings.id` (child booking) |
| `created_at` | `TIMESTAMP` | NOT NULL |

**Relationship:** Many-to-many (master → children).  
**Legacy:** `rezervacijegrupe`  
**Group types (in JSON payload):** `WEDDING`, etc.  
**Business logic:** master bill guest pays for all rooms; individual guests pay personal charges (minibar, phone).

---

### Table: booking_histories (`BookingHistory`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `booking_id` | `UUID` | NOT NULL, FK → `bookings.id` |
| `action` | `VARCHAR` | NOT NULL |
| `previous_value` | `VARCHAR` | NULL (JSON snapshot) |
| `new_value` | `VARCHAR` | NULL (JSON snapshot) |
| `changed_by_id` | `UUID` | NULL, FK → `employees.id` |
| `changed_at` | `TIMESTAMP` | NOT NULL |

**Action values (stored as string):** `Created`, `Modified`, `Cancelled`, `CheckedIn`, `CheckedOut`, `NoShow`  
**Indexes:** `(booking_id)`, `(changed_at)`  
**Legacy:** `rezervacije1`, `rezervacijasobe1` (archive tables)

---

### Table: room_assignments (`RoomAssignment`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `room_id` | `UUID` | NOT NULL, FK → `rooms.id` |
| `guest_id` | `UUID` | NULL, FK → `guests.id` |
| `arrival_date` | `TIMESTAMP` | NOT NULL |
| `departure_date` | `TIMESTAMP` | NOT NULL |
| `status` | `VARCHAR` | NOT NULL |
| `notes` | `VARCHAR` | NULL |

**Status values (stored as string):** `Tentative`, `Confirmed`, `CheckedIn`, `CheckedOut`  
**Legacy:** `relgostsoba` — key junction for room status computation.  
Legacy columns: `odjavljen` (0/1).

---

### Table: payment_methods (`PaymentMethod`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(100)` | NOT NULL |
| `code` | `VARCHAR(10)` | NOT NULL |
| `is_active` | `BOOLEAN` | NOT NULL |

**Legacy:** `nplac` / `SifreNacinPlacanja`

---

### Table: service_catalogs (`ServiceCatalog`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR(200)` | NOT NULL |
| `code` | `VARCHAR(20)` | NOT NULL |
| `category` | `VARCHAR` | NOT NULL |
| `default_price` | `DECIMAL(18,2)` | NULL |
| `vat_percent` | `DECIMAL(5,2)` | NOT NULL, DEFAULT `0` |
| `is_active` | `BOOLEAN` | NOT NULL |

**Category values (stored as string):** `Room`, `Food`, `Beverage`, `Service`, `Tax`, `Other`  
**Legacy:** `troskovivrste` / `SifreUsluga`

---

### Table: folios (`Folio`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `folio_number` | `VARCHAR(20)` | NOT NULL, UNIQUE |
| `booking_id` | `UUID` | NULL, FK → `bookings.id` |
| `booking_room_id` | `UUID` | NULL, FK → `booking_rooms.id` |
| `guest_id` | `UUID` | NULL, FK → `guests.id` |
| `status` | `VARCHAR` | NOT NULL |
| `balance` | `DECIMAL(18,2)` | NOT NULL |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `closed_at` | `TIMESTAMP` | NULL |
| `notes` | `VARCHAR` | NULL |

**Status values (stored as string):** `Open`, `Closed`, `Archived`  
**Legacy:** `folio` — tied to room (`SID`) with `zakljucen` flag.  
**Business rule:** Folio primarily per room, charges split to guests at payment time.  
**Indexes:** `(booking_id)`, `(guest_id)`

---

### Table: stay_nights (`StayNight`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `folio_id` | `UUID` | NOT NULL, FK → `folios.id` |
| `date` | `TIMESTAMP` | NOT NULL (date part only) |
| `room_price` | `DECIMAL(18,2)` | NOT NULL |
| `is_comp` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` (complimentary) |
| `notes` | `VARCHAR` | NULL |

**Legacy:** `nocenja`  
Legacy columns: `RID` (FK relgostsoba), `Tarifa`, `PrijavaOdjava` (0/1 flag), `popust`.

---

### Table: charges (`Charge`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `folio_id` | `UUID` | NOT NULL, FK → `folios.id` |
| `service_catalog_id` | `UUID` | NULL, FK → `service_catalogs.id` |
| `description` | `VARCHAR(500)` | NOT NULL |
| `quantity` | `DECIMAL(18,2)` | NOT NULL, DEFAULT `1` |
| `unit_price` | `DECIMAL(18,2)` | NOT NULL |
| `total_price` | `DECIMAL(18,2)` | NOT NULL (computed: Quantity × UnitPrice) |
| `vat_amount` | `DECIMAL(18,2)` | NOT NULL |
| `charge_date` | `TIMESTAMP` | NOT NULL |
| `posted_by_id` | `UUID` | NULL, FK → `employees.id` |
| `is_taxable` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |

**Legacy:** `troskovi`  
Legacy columns: `TID` (FK troskovivrste), `zaklj` (tinyint), `iznos`, `GSID`, `SID`, `Djelimicno`, `Kolicina`, `RadnikID`, `Napomena`.  
**Indexes:** `(folio_id)`

---

### Table: payments (`Payment`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `folio_id` | `UUID` | NOT NULL, FK → `folios.id` |
| `payment_method_id` | `UUID` | NULL, FK → `payment_methods.id` |
| `amount` | `DECIMAL(18,2)` | NOT NULL |
| `payment_date` | `TIMESTAMP` | NOT NULL |
| `reference` | `VARCHAR` | NULL |
| `processed_by_id` | `UUID` | NULL, FK → `employees.id` |
| `notes` | `VARCHAR` | NULL |

**Legacy:** `placanje`  
**Indexes:** `(folio_id)`

---

### Table: expenses (`Expense`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `description` | `VARCHAR(500)` | NOT NULL |
| `category` | `VARCHAR` | NOT NULL |
| `amount` | `DECIMAL(18,2)` | NOT NULL |
| `vat_amount` | `DECIMAL(18,2)` | NOT NULL |
| `expense_date` | `TIMESTAMP` | NOT NULL |
| `supplier_name` | `VARCHAR` | NULL |
| `invoice_number` | `VARCHAR` | NULL |
| `approved_by_id` | `UUID` | NULL, FK → `employees.id` |
| `notes` | `VARCHAR` | NULL |

**Note:** Hotel operational expenses, NOT guest charges (separate from Charge/Folio system).  
**Legacy:** `troskovi` (administrative expenses)

---

### Table: outstanding_balances (`OutstandingBalance`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `date` | `TIMESTAMP` | NOT NULL |
| `folio_id` | `UUID` | NOT NULL, FK → `folios.id` |
| `balance` | `DECIMAL(18,2)` | NOT NULL |
| `due_date` | `TIMESTAMP` | NULL |
| `is_overdue` | `BOOLEAN` | NOT NULL |

**Legacy:** `neplaceni` — daily snapshot of open balances.

---

### Table: invoices (`Invoice`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `invoice_number` | `VARCHAR(50)` | NOT NULL, UNIQUE |
| `folio_id` | `UUID` | NOT NULL, FK → `folios.id` |
| `guest_id` | `UUID` | NULL, FK → `guests.id` |
| `issue_date` | `TIMESTAMP` | NOT NULL |
| `due_date` | `TIMESTAMP` | NOT NULL |
| `total_net` | `DECIMAL(18,2)` | NOT NULL |
| `total_vat` | `DECIMAL(18,2)` | NOT NULL |
| `total_gross` | `DECIMAL(18,2)` | NOT NULL |
| `status` | `VARCHAR` | NOT NULL |
| `pdf_url` | `VARCHAR` | NULL (path to stored PDF) |
| `created_at` | `TIMESTAMP` | NOT NULL |

**Status values (stored as string):** `Draft`, `Sent`, `Paid`, `Cancelled`  
**Legacy:** `printracuni` — columns: `fisrac` (fiscal receipt number), `storno` (flag), `Ime`, `DrugoIme` (guest/company).  
**Business rule:** Group invoices possible via `grupaID` (legacy).  
**Currency:** Prepared in foreign currency; fiscalized in local currency using exchange rate from `kursna` table.

---

### Table: invoice_items (`InvoiceItem`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `invoice_id` | `UUID` | NOT NULL, FK → `invoices.id` |
| `description` | `VARCHAR(500)` | NOT NULL |
| `quantity` | `DECIMAL(18,2)` | NOT NULL |
| `unit_price` | `DECIMAL(18,2)` | NOT NULL |
| `net_amount` | `DECIMAL(18,2)` | NOT NULL |
| `vat_amount` | `DECIMAL(18,2)` | NOT NULL |
| `gross_amount` | `DECIMAL(18,2)` | NOT NULL |
| `vat_percent` | `DECIMAL(5,2)` | NOT NULL |
| `sort_order` | `INT` | NOT NULL |

**Legacy:** `printracunidetalji` — stored amounts as `CHAR(10)` (migrated to `DECIMAL`).  
Legacy columns: `CijBezPdv`, `Ukupno`, `Pdv` (VAT rate), `IznosPdv` (VAT amount).  
**Cascade:** Invoice → InvoiceItem

---

### Table: prepayments (`Prepayment`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| (advance payment records — legacy `printracuniavans`) |

**Legacy:** `printracuniavans` — advance payment tracking. Applied as deduction on final invoice.

---

### Table: invoice_summaries (`InvoiceSummary`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| (total amounts and taxes — legacy `printracunifooter`) |

**Legacy:** `printracunifooter`

---

### Table: employees (`Employee`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `first_name` | `VARCHAR(100)` | NOT NULL |
| `last_name` | `VARCHAR(100)` | NOT NULL |
| `email` | `VARCHAR(255)` | NOT NULL, UNIQUE |
| `phone` | `VARCHAR(50)` | NULL |
| `role` | `VARCHAR` | NOT NULL |
| `pin_code` | `VARCHAR(6)` | NULL (for POS/tablet login) |
| `password_hash` | `VARCHAR` | NULL (BCrypt hash for web login) |
| `is_active` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `can_login` | `BOOLEAN` | NOT NULL, DEFAULT `TRUE` |
| `assigned_building_ids` | `VARCHAR` | NULL (JSON array of Building UUIDs) |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `updated_at` | `TIMESTAMP` | NOT NULL |

**Role values (stored as string):** `Admin`, `Manager`, `Reception`, `Housekeeping`, `Maintenance`, `Accountant`  
**Legacy:** `radnici`  
**Index:** `(email)` UNIQUE  
**Soft delete filter:** `IsActive == true`

---

### Table: shifts (`Shift`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `employee_id` | `UUID` | NOT NULL, FK → `employees.id` |
| `shift_date` | `TIMESTAMP` | NOT NULL (date part only) |
| `start_time` | `TIME` | NOT NULL (TimeOnly) |
| `end_time` | `TIME` | NOT NULL (TimeOnly) |
| `shift_type` | `VARCHAR` | NOT NULL |
| `notes` | `VARCHAR` | NULL |

**ShiftType values (stored as string):** `Morning`, `Afternoon`, `Night`  
**Legacy:** `smjene`  
**Index:** `(employee_id, shift_date)`

---

### Table: housekeeping_logs (`HousekeepingLog`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `room_id` | `UUID` | NOT NULL, FK → `rooms.id` |
| `employee_id` | `UUID` | NOT NULL, FK → `employees.id` |
| `action` | `VARCHAR` | NOT NULL |
| `status` | `VARCHAR` | NOT NULL |
| `scheduled_at` | `TIMESTAMP` | NULL |
| `started_at` | `TIMESTAMP` | NULL |
| `completed_at` | `TIMESTAMP` | NULL |
| `notes` | `VARCHAR` | NULL |
| `is_verified` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` |
| `verified_by_id` | `UUID` | NULL, FK → `employees.id` |

**Action values (stored as string):** `Cleaned`, `Inspected`, `Repaired`, `Restocked`, `TurnDown`  
**Status values (stored as string):** `Pending`, `InProgress`, `Completed`, `Verified`  
**Legacy:** `sobaricalog`  
**Index:** `(room_id, status)` for pending task display  
**Business rule:** On checkout, room auto-set to `Dirty` (legacy: `sobe.clean = 0`). Reception confirms cleaning via UI (legacy: `updateSobaClean` stored procedure).

---

### Table: work_orders (`WorkOrder`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK, `DEFAULT gen_random_uuid()` |
| `room_id` | `UUID` | NULL, FK → `rooms.id` |
| `reported_by_id` | `UUID` | NOT NULL, FK → `employees.id` |
| `assigned_to_id` | `UUID` | NULL, FK → `employees.id` |
| `priority` | `VARCHAR` | NOT NULL |
| `category` | `VARCHAR` | NOT NULL |
| `description` | `VARCHAR(1000)` | NOT NULL |
| `status` | `VARCHAR` | NOT NULL |
| `created_at` | `TIMESTAMP` | NOT NULL |
| `resolved_at` | `TIMESTAMP` | NULL |
| `resolution_notes` | `VARCHAR` | NULL |

**Priority values (stored as string):** `Low`, `Medium`, `High`, `Critical`  
**Category values (stored as string):** `Plumbing`, `Electrical`, `HVAC`, `Furniture`, `Other`  
**Status values (stored as string):** `Open`, `InProgress`, `Resolved`, `Closed`

**Extended legacy category mapping (from admin config):** `HVAC`, `TV`, `VODOVOD` (plumbing), `STRUJA` (electrical), `NAMJESTAJ` (furniture), `DRUGO` (other).

---

### Table: access_logs (`AccessLog`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `employee_id` | `UUID` | NULL, FK → `employees.id` |
| `action` | `VARCHAR` | NOT NULL |
| `ip_address` | `VARCHAR(45)` | NOT NULL |
| `user_agent` | `VARCHAR` | NULL |
| `timestamp` | `TIMESTAMP` | NOT NULL |
| `details` | `VARCHAR` | NULL (JSON) |

**Action values (stored as string):** `Login`, `Logout`, `AccessGranted`, `AccessDenied`, `PinVerified`  
**Legacy:** `logcont` (RFID access logs via `idkon` hardware controller)  
**Index:** `(timestamp)`  
**Note:** Append-only — never deleted.

---

### Table: audit_logs (`AuditLog`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK, `DEFAULT gen_random_uuid()` |
| `entity_type` | `VARCHAR(50)` | NOT NULL |
| `entity_id` | `UUID` | NOT NULL |
| `action` | `VARCHAR(20)` | NOT NULL |
| `old_values` | `JSONB` | NULL |
| `new_values` | `JSONB` | NULL |
| `changed_by` | `VARCHAR(100)` | NULL (employee ID or 'system') |
| `room_id` | `UUID` | NULL |
| `hotel_id` | `UUID` | NULL (for multi-tenant) |
| `created_at` | `TIMESTAMP` | NOT NULL, DEFAULT `NOW()` |

**Action values:** `create`, `update`, `delete`  
**Entity type examples:** `booking`, `payment`, `guest`, etc.  
**Indexes:** `(entity_type, entity_id)`, `(created_at DESC)`  
**Configuration:** Per-table enable/disable via admin panel. Default retention: 90 days active + 5 years archive. Circular file logging as backup.

---

### Table: phone_calls (`PhoneCall`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `extension` | `VARCHAR` | NOT NULL (room phone extension, maps to `rooms.lokal`) |
| `dialed_number` | `VARCHAR` | NOT NULL |
| `duration` | `VARCHAR` | NOT NULL (MM:SS or seconds) |
| `price` | `DECIMAL(18,2)` | NOT NULL |
| `is_paid` | `BOOLEAN` | NOT NULL, DEFAULT `FALSE` |
| `call_date` | `TIMESTAMP` | NOT NULL |
| `folio_id` | `UUID` | NULL, FK → `folios.id` (when posted to guest bill) |

**Legacy:** `telpozivi`  
Legacy columns: `Lokal`, `TelefonskiBroj`, `TrajanjePoziva`, `Cijena`, `Placeno` (flag).  
**Archive:** `phone_call_archives` (legacy `telpozivi_stara`).  
**Business rule:** Calls from non-room extensions (reception, offices) are not billed to guests.

---

### Table: phone_call_archives (`PhoneCallArchive`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| (mirrors `phone_calls` structure — periodic archival for performance) |

**Legacy:** `telpozivi_stara`

---

### Table: phone_directories (`PhoneDirectory`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `name` | `VARCHAR` | NOT NULL |
| `extension` | `VARCHAR` | NOT NULL |
| `department` | `VARCHAR` | NULL |
| `is_active` | `BOOLEAN` | NOT NULL |

**Legacy:** `telefonskiimenik`

---

### Table: webhook_subscriptions (`WebhookSubscription`)
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| `event_type` | `VARCHAR` | NOT NULL |
| `url` | `VARCHAR` | NOT NULL |
| `active` | `BOOLEAN` | NOT NULL |
| `created_at` | `TIMESTAMP` | NOT NULL |

**Predefined event types:**
```
booking.created, booking.cancelled, booking.modified,
checkin.completed, checkout.completed,
payment.received,
room.status.changed,
rate.plan.updated
```

---

### Table: exchange_rates (`ExchangeRate`) *(referenced — needs verification)*
| Column | Type | Constraints |
|---|---|---|
| `id` | `UUID` | PK |
| (legacy `kursna` table — exchange rate list for currency conversion on invoices) |

---

## ENUM VALUE REFERENCE

### RoomStatus
`Free` | `Occupied` | `Reserved` | `Dirty` | `OutOfOrder` | `OutOfService`

### BookingStatus
`Provisional` | `Confirmed` | `CheckedIn` | `CheckedOut` | `Cancelled` | `NoShow`

### PaymentStatus
`Unpaid` | `Partial` | `Paid` | `Refunded`

### BookingRoom.Status
`Pending` | `CheckedIn` | `CheckedOut` | `Cancelled`

### RoomAssignment.Status
`Tentative` | `Confirmed` | `CheckedIn` | `CheckedOut`

### Folio.Status
`Open` | `Closed` | `Archived`

### Invoice.Status
`Draft` | `Sent` | `Paid` | `Cancelled`

### GuestDocument.DocumentType
`Passport` | `IdCard` | `Visa` | `Other`

### Partner.PartnerType
`TourOperator` | `TravelAgency` | `Corporate` | `Other`

### Employee.Role
`Admin` | `Manager` | `Reception` | `Housekeeping` | `Maintenance` | `Accountant`

### Shift.ShiftType
`Morning` | `Afternoon` | `Night`

### HousekeepingLog.Action
`Cleaned` | `Inspected` | `Repaired` | `Restocked` | `TurnDown`

### HousekeepingLog.Status
`Pending` | `InProgress` | `Completed` | `Verified`

### WorkOrder.Priority
`Low` | `Medium` | `High` | `Critical`

### WorkOrder.Category
`Plumbing` | `Electrical` | `HVAC` | `Furniture` | `Other`

### WorkOrder.Status
`Open` | `InProgress` | `Resolved` | `Closed`

### AccessLog.Action
`Login` | `Logout` | `AccessGranted` | `AccessDenied` | `PinVerified`

### ServiceCatalog.Category
`Room` | `Food` | `Beverage` | `Service` | `Tax` | `Other`

### POS ↔ TID mapping (legacy `troskovivrste` IDs)
| POS Category | TID (legacy) | Name |
|---|---|---|
| RESTAURANT_MAIN | 3 | Restoran |
| BAR | 4 | Bar |
| ROOM_SERVICE | 6 | Room service |
| SPA | 7 | Spa |
| LAUNDRY | 8 | Veš |
| MINIBAR | 10 | Mini bar |

---

## RELATIONSHIP GRAPH (KEY FOREIGN KEYS)

```
buildings ──< rooms ──< room_assignments >── guests
room_types ──< rooms
room_types ──< tariffs
countries ──< guests
countries ──< partners
guests ──< guest_documents
guests ──< bookings
booking_types ──< bookings
booking_sources ──< bookings
partners ──< bookings
partners ──< sales_agents
sales_agents ──< bookings
bookings ──< booking_rooms >── rooms
bookings ──< group_bookings (master)
bookings ──< group_bookings (member)
bookings ──< booking_histories
bookings ──< folios
booking_rooms ──< folios
guests ──< folios
folios ──< stay_nights
folios ──< charges
service_catalogs ──< charges
folios ──< payments
payment_methods ──< payments
folios ──< outstanding_balances
folios ──< invoices
invoices ──< invoice_items
employees ──< shifts
employees ──< housekeeping_logs
rooms ──< housekeeping_logs
employees ──< work_orders (reported_by)
employees ──< work_orders (assigned_to)
rooms ──< work_orders
employees ──< access_logs
employees ──< bookings (created_by)
employees ──< charges (posted_by)
employees ──< payments (processed_by)
employees ──< expenses (approved_by)
employees ──< booking_histories (changed_by)
phone_calls ──< folios
```

---

## LEGACY DATA TYPE CONVERSIONS

| Legacy MySQL Type | PostgreSQL Type | Notes |
|---|---|---|
| `int AUTO_INCREMENT` | `UUID` (via mapping table) | `old_id → new_uuid` table |
| `char(10)` for amounts | `DECIMAL(18,2)` | Replace comma with dot: `REPLACE(iznos, ',', '.')::decimal` |
| `tinyint` | `BOOLEAN` | |
| `decimal(19,0)` | `INTEGER` | |
| `varchar` / `text` | `VARCHAR` / `TEXT` | Lengths preserved |

---

## KEY BUSINESS RULES (embedded in schema logic)

1. **Room status** is computed from `room_assignments` + room `OutOfOrder` flag (state machine, not SQL function).
2. **Room auto-dirty** on checkout: `RoomAssignment.status=CheckedOut` → `Room.status=Dirty`.
3. **No-Show detection**: Hourly job sets `Booking.status=NoShow` if `arrival_date < now - 1 day` and `status=Provisional`.
4. **Night Audit**: Daily job at midnight that rolls the business date and posts `StayNight` records.
5. **Folio locking**: Once `status=Closed`, no new charges can be added.
6. **Fiscalization**: Invoice is valid ONLY after fiscal printer/API confirms receipt number.
7. **Group billing**: Master guest pays accommodation; individual guests pay personal charges.
8. **All datetimes**: `Kind = Utc`, stored as `TIMESTAMP`.
9. **Enums stored as strings** (not integers) — `VARCHAR` with app-level enum constraint.
10. **Soft delete** (`IsActive`) with EF Core global query filter on: Building, RoomType, Room, Guest, Partner, Employee.
