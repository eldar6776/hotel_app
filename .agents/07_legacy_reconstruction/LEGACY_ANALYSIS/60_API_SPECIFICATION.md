# REST API Specification — HotelPRO v1

> Derived from legacy analysis documents 10_CHECKIN.md through 17_FISCAL_PROFORMA.md,
> cross-referenced with 30_STATUS_MATRIX.md, 35_CROSS_FLOW_DEPENDENCIES.md,
> and 40_GOLDEN_SCENARIOS.md.
>
> Every endpoint includes request/response schemas, status codes, validation rules,
> and `legacy_code/file:line` evidence.

---

## Common Conventions

### Authentication
All endpoints require JWT `Authorization: Bearer <token>`.  
RBAC roles: `admin`, `reception`, `housekeeping`, `manager`.

### Common Headers
```
Content-Type: application/json
Accept: application/json
X-Hotel-Id: <hotel_id>   (multi-tenant)
X-Request-Id: <uuid>      (idempotency)
```

### Pagination
```
GET /api/v1/rooms?page=1&pageSize=20&sortBy=name&sortDir=asc
```
Response envelope for lists:
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

### Error Response Format
```json
{
  "type": "https://hotelpro.app/errors/{error-code}",
  "title": "Human-readable message",
  "status": 400,
  "detail": "Specific context",
  "traceId": "uuid",
  "timestamp": "2026-05-18T10:00:00Z",
  "fieldErrors": [
    { "field": "checkInDate", "message": "..." }
  ]
}
```

---

## 1. Rooms API

### GET /api/v1/rooms

List rooms with computed status.

**Query Parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page (max 100) |
| sortBy | string | "name" | Sort field: name, status, floor, buildingId |
| sortDir | string | "asc" | Sort direction |
| status | int[] | null | Filter by fnSobaStatus values: 0=free, 1=occupied, 2=departing, 3=reserved_confirmed, 4=occupied_reserved, 5=ooo, 6=reserved_unconfirmed |
| clean | bool? | null | Filter by clean status |
| buildingId | int? | null | Filter by building |
| roomTypeId | int? | null | Filter by room type |
| ooo | bool? | null | Filter out-of-order |
| date | date? | today | Date for status computation (reservation occupancy) |

**Response 200:**
```json
{
  "items": [
    {
      "id": 101,
      "name": "101",
      "roomTypeId": 3,
      "roomTypeName": "Standard Double",
      "bedCount": 2,
      "buildingId": 1,
      "floor": 1,
      "controllerType": 1,
      "controllerIp": "192.168.1.45",
      "controllerPort": 5000,
      "status": {
        "code": 1,
        "label": "ZAUZETA",
        "occupied": true,
        "reserved": false,
        "departing": false,
        "ooo": false
      },
      "clean": true,
      "ooo": false,
      "oooReason": null,
      "currentGuests": [
        {
          "guestId": 55,
          "name": "Marko Marković",
          "checkInDate": "2026-05-15",
          "checkOutDate": "2026-05-18",
          "status": 1
        }
      ],
      "openFolioId": 201,
      "openExpenseCount": 3,
      "notes": ""
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

**Legacy Evidence:** `legacy_code/frmSobe.vb:65` (getSobeShema SP), `12_ROOM_STATUS.md:1.1` (fnSobaStatus), `frmSobe.vb:250-284` (status rendering)

---

### GET /api/v1/rooms/{id}

Room detail with full status, guests, expenses, folio.

**Path Parameters:**

| Param | Type | Description |
|-------|------|-------------|
| id | int | Room ID (sobe.ID) |

**Response 200:**
```json
{
  "id": 101,
  "name": "101",
  "roomTypeId": 3,
  "roomTypeName": "Standard Double",
  "bedCount": 2,
  "buildingId": 1,
  "floor": 1,
  "controllerType": 1,
  "controllerIp": "192.168.1.45",
  "controllerPort": 5000,
  "status": {
    "code": 1,
    "label": "ZAUZETA",
    "occupied": true,
    "reserved": false,
    "departing": false,
    "ooo": false
  },
  "clean": true,
  "ooo": false,
  "oooReason": null,
  "amenities": ["WiFi", "TV", "Minibar"],
  "currentGuests": [
    {
      "stayId": 1501,
      "guestId": 55,
      "firstName": "Marko",
      "lastName": "Marković",
      "documentType": "Pasos",
      "documentNumber": "12345678",
      "citizenship": "Bosna i Hercegovina",
      "status": 1,
      "checkInDate": "2026-05-15T14:30:00",
      "checkOutDate": "2026-05-18T10:00:00",
      "tariff": 55.00,
      "discount": 0,
      "discountReason": null,
      "tax": 1.50,
      "groupId": null,
      "folioId": 201,
      "printed": false,
      "eStranac": 0,
      "touristRegId": null
    }
  ],
  "folio": {
    "id": 201,
    "openDate": "2026-05-15T14:30:00",
    "closeDate": null,
    "closed": false
  },
  "openExpenses": [
    {
      "id": 3001,
      "expenseTypeId": 2,
      "expenseTypeName": "Restoran",
      "quantity": 1,
      "amount": 25.00,
      "date": "2026-05-16T12:00:00",
      "closed": false,
      "receiptNumber": null,
      "note": ""
    }
  ],
  "nightCharges": [
    {
      "id": 4001,
      "guestId": 55,
      "date": "2026-05-15",
      "tariff": 55.00,
      "discount": 0,
      "description": "",
      "active": true
    }
  ],
  "notes": ""
}
```

**Response 404:**
```json
{
  "type": "https://hotelpro.app/errors/room-not-found",
  "title": "Room not found",
  "status": 404,
  "detail": "Room with id 9999 does not exist."
}
```

**Legacy Evidence:** `12_ROOM_STATUS.md:1.1` (fnSobaStatus), `frmSobaInfo.vb:38` (getSobaPodaci SP), `frmSobaInfo.vb:496` (getSobeSadrzaji SP)

---

### PUT /api/v1/rooms/{id}/status

Change room status (OOO toggle or status override).

**Request Body:**
```json
{
  "ooo": true,
  "oooReason": "Renovation - plumbing repair",
  "clean": true
}
```

Or:
```json
{
  "ooo": false,
  "clean": true
}
```

**Validation Rules:**
| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| ooo | boolean required | ROOM_STATUS_OOO_INVALID | `12_ROOM_STATUS.md:5.3` |
| oooReason | required if ooo=true, max 500 chars | ROOM_OOO_REASON_REQUIRED | `frmSobaInfo.vb:180` (updateSobaOOO SP) |
| clean | boolean required | ROOM_CLEAN_INVALID | `12_ROOM_STATUS.md:3.1` |

**Business Rules:**
| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-ROOM-001 | Setting OOO on occupied room requires confirmation | ROOM_OOO_OCCUPIED_CONFIRM | `12_ROOM_STATUS.md:305-308` |
| BR-ROOM-002 | Setting clean=true on OOO room offers to un-OOO it | ROOM_CLEAN_OOO_CONFLICT | `12_ROOM_STATUS.md:4.1`, `frmSobaInfo.vb:2172` |
| BR-ROOM-003 | OOO rooms are excluded from check-in room lists | ROOM_OOO_NOT_AVAILABLE | `frmPrijava1.vb:105` (WHERE ooo=0) |

**Response 200:**
```json
{
  "id": 101,
  "name": "101",
  "status": {
    "code": 5,
    "label": "VAN UPOTREBE",
    "occupied": false,
    "reserved": false,
    "departing": false,
    "ooo": true
  },
  "clean": true,
  "ooo": true,
  "oooReason": "Renovation - plumbing repair"
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | ROOM_OOO_OCCUPIED_CONFIRM | Room is occupied; confirm OOO action | `12_ROOM_STATUS.md:5.3` |
| 400 | ROOM_CLEAN_OOO_CONFLICT | Room is OOO; setting clean will also unset OOO | `frmSobaInfo.vb:2172` |
| 404 | ROOM_NOT_FOUND | Room ID does not exist | — |
| 409 | ROOM_STATUS_CONFLICT | Concurrent status change detected | — |

**Legacy Evidence:** `frmSobaInfo.vb:140-208` (updateOOO), `frmSobaInfo.vb:231,282` (updateSobaClean), `12_ROOM_STATUS.md:3.1`

---

### PUT /api/v1/rooms/{id}/clean

Mark room as clean or dirty.

**Request Body:**
```json
{
  "clean": false
}
```

**Business Rules:**
| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-ROOM-004 | Check-in sets clean=1 (via updateSobaClean SP) | — | `frmPrijava1.vb:504` |
| BR-ROOM-005 | Checkout sets clean=0 (via PrljavaSoba) — outside transaction | — | `Data.vb:126` |
| BR-ROOM-006 | Room transfer sets old room clean=0 | — | `frmSobaInfo.vb:209-258` (updateClean1) |
| BR-ROOM-008 | clean=0 overrides ALL displayed status codes (shows as 7/NIJE SPREMNA) | — | `frmSobe.vb:280-284` |

**Response 200:**
```json
{
  "id": 101,
  "name": "101",
  "clean": false,
  "status": {
    "code": 7,
    "label": "NIJE SPREMNA",
    "cleanOverride": true
  }
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | ROOM_NOT_FOUND | Room ID does not exist | — |
| 409 | ROOM_CLEAN_CONFLICT | Concurrent update detected | — |

**Legacy Evidence:** `12_ROOM_STATUS.md:3.1` (updateSobaClean SP), `frmSobe.vb:280-284` (clean=0 override)

---

## 2. Guests API

### GET /api/v1/guests

Search/list guests.

**Query Parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page (max 100) |
| search | string? | null | LIKE search on surname and name |
| citizenship | string? | null | Filter by citizenship (drzave.naziv) |
| statusId | int? | null | Filter by guest status (1=adult, 3=minor, 4=child) |
| currentlyStaying | bool? | null | Filter guests currently checked in |
| documentType | string? | null | Filter by document type |

**Response 200:**
```json
{
  "items": [
    {
      "id": 55,
      "firstName": "Marko",
      "lastName": "Marković",
      "address": "Ulica 1, Sarajevo",
      "dateOfBirth": "1990-05-15",
      "gender": "M",
      "citizenship": "Bosna i Hercegovina",
      "citizenshipId": 1,
      "documentType": "Pasos",
      "documentNumber": "12345678",
      "phone": "+38761123456",
      "mobile": "+38762123456",
      "email": "marko@example.com",
      "statusId": 1,
      "statusName": "Odrasli",
      "currentStay": {
        "stayId": 1501,
        "roomId": 101,
        "roomName": "101",
        "checkInDate": "2026-05-15T14:30:00",
        "checkOutDate": "2026-05-18T10:00:00",
        "folioId": 201
      },
      "previousStaysCount": 3
    }
  ],
  "totalCount": 1250,
  "page": 1,
  "pageSize": 20,
  "totalPages": 63
}
```

**Legacy Evidence:** `10_CHECKIN.md:1.5` (guest search: LIKE on surname+name, limit 100), `20_GUESTS.md` (full guest CRUD)

---

### GET /api/v1/guests/{id}

Guest detail.

**Response 200:**
```json
{
  "id": 55,
  "firstName": "Marko",
  "lastName": "Marković",
  "address": "Ulica 1, Sarajevo",
  "dateOfBirth": "1990-05-15",
  "gender": "M",
  "citizenship": "Bosna i Hercegovina",
  "citizenshipId": 1,
  "documentType": "Pasos",
  "documentTypeId": 3,
  "documentNumber": "12345678",
  "phone": "+38761123456",
  "mobile": "+38762123456",
  "email": "marko@example.com",
  "partnerId": null,
  "stays": [
    {
      "stayId": 1501,
      "roomId": 101,
      "roomName": "101",
      "checkInDate": "2026-05-15T14:30:00",
      "checkOutDate": "2026-05-18T10:00:00",
      "checkedOut": false,
      "tariffId": 5,
      "tariffName": "Standard BB",
      "discount": 0,
      "discountReason": null,
      "status": 1,
      "statusName": "Odrasli",
      "tax": 1.50,
      "folioId": 201
    }
  ],
  "previousStays": [
    {
      "stayId": 1203,
      "roomId": 205,
      "roomName": "205",
      "checkInDate": "2025-08-10T12:00:00",
      "checkOutDate": "2025-08-15T10:00:00",
      "checkedOut": true
    }
  ]
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | GUEST_NOT_FOUND | Guest ID does not exist |

**Legacy Evidence:** `10_CHECKIN.md:1.5` (getPosjete SP for previous visits), `10_CHECKIN.md:1.6` (duplicate guest check in room)

---

### POST /api/v1/guests

Create a new guest.

**Request Body:**
```json
{
  "firstName": "Ana",
  "lastName": "Petrović",
  "address": "Ulica 2, Banja Luka",
  "dateOfBirth": "1985-03-20",
  "gender": "Z",
  "citizenshipId": 1,
  "documentTypeId": 3,
  "documentNumber": "98765432",
  "phone": "+38765123456",
  "mobile": "+38766123456",
  "email": "ana@example.com",
  "partnerId": null
}
```

**Validation Rules:**

| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| lastName | required, non-empty | GUEST_LASTNAME_REQUIRED | `frmPrijavaGostiUnos.vb:~810` ("Niste unijeli prezime!") |
| gender | required, "M" or "Z" | GUEST_GENDER_INVALID | `10_CHECKIN.md:4.16` |
| dateOfBirth | required, valid date | GUEST_DOB_INVALID | `frmPrijavaGostiUnos.vb` (mtbDatum_TypeValidationCompleted) |
| citizenshipId | must exist in drzave table | GUEST_CITIZENSHIP_INVALID | `10_CHECKIN.md:1.5` |
| documentTypeId | must exist in gostdokument table | GUEST_DOCUMENT_TYPE_INVALID | `10_CHECKIN.md:1.5` |

**Auto-computed fields (from dateOfBirth):**

| Age | statusId | Auto-discount | Legacy Evidence |
|-----|----------|---------------|-----------------|
| < 12 | 4 (child) | setings.dijecapop if dijecagod > 0 | `10_CHECKIN.md:5.9-5.10` |
| 12-17 | 3 (minor) | setings.dijecapop if dijecagod > 0 | `10_CHECKIN.md:5.9-5.10` |
| >= 18 | 1 (adult) | none | `10_CHECKIN.md:5.9` |

**Response 201:**
```json
{
  "id": 56,
  "firstName": "Ana",
  "lastName": "Petrović",
  "address": "Ulica 2, Banja Luka",
  "dateOfBirth": "1985-03-20",
  "gender": "Z",
  "citizenshipId": 1,
  "citizenship": "Bosna i Hercegovina",
  "documentTypeId": 3,
  "documentType": "Pasos",
  "documentNumber": "98765432",
  "phone": "+38765123456",
  "mobile": "+38766123456",
  "email": "ana@example.com",
  "statusId": 1,
  "statusName": "Odrasli",
  "autoDiscount": 0,
  "autoDiscountReason": null
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | GUEST_LASTNAME_REQUIRED | Last name is required | `frmPrijavaGostiUnos.vb:~810` |
| 400 | GUEST_DOB_INVALID | Invalid date of birth | `10_CHECKIN.md:5.9` |
| 400 | GUEST_CITIZENSHIP_INVALID | Citizenship ID not found | `10_CHECKIN.md:1.5` |
| 400 | GUEST_DOCUMENT_TYPE_INVALID | Document type ID not found | `10_CHECKIN.md:1.5` |

**Legacy Evidence:** `frmPrijavaGostiUnos.vb:~690` (INSERT INTO gosti), `10_CHECKIN.md:1.5` (guest data entry), `10_CHECKIN.md:5.9-5.10` (age-based status/discount)

---

### PUT /api/v1/guests/{id}

Update guest data.

**Request Body:**
```json
{
  "firstName": "Ana",
  "lastName": "Petrović-Nikolić",
  "address": "Nova ulica 5, Banja Luka",
  "dateOfBirth": "1985-03-20",
  "gender": "Z",
  "citizenshipId": 1,
  "documentTypeId": 3,
  "documentNumber": "98765432",
  "phone": "+38765123456",
  "mobile": "+38766123456",
  "email": "ana.new@example.com",
  "partnerId": null
}
```

**Response 200:** Same as POST response schema.

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 400 | GUEST_LASTNAME_REQUIRED | Last name is required |
| 404 | GUEST_NOT_FOUND | Guest ID does not exist |

**Legacy Evidence:** `frmPrijavaGostiUnos.vb:893` (promijeniGosti SP), `frmPrijavaGostiKucice.vb:~710` (UPDATE relgostsoba status/tax)

---

### POST /api/v1/guests/{id}/tourist-registration

Register guest with tourist authority (e-stranac / TZ).

**Request Body:**
```json
{
  "registrationType": "eastranac",
  "bookNumber": null,
  "notes": ""
}
```

**Response 200:**
```json
{
  "guestId": 55,
  "stayId": 1501,
  "estraneNumber": 42,
  "tid": "TZ-2026-0042",
  "registeredAt": "2026-05-18T08:30:00Z"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-GUEST-001 | estraneNumber generated via MAX()+1 — must use atomic increment | GUEST_ESTRANEC_RACE | `20_GUESTS.md:156` |
| BR-GUEST-002 | tid populated from TZ response | GUEST_TID_RESPONSE | `20_GUESTS.md:1821-1826` |

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | GUEST_NOT_FOUND | Guest ID does not exist | — |
| 409 | GUEST_ALREADY_REGISTERED | Guest already has registration number | — |

**Legacy Evidence:** `20_GUESTS.md:104-106` (estranac registration), `20_GUESTS.md:2125` (MAX(estranac)+1 race condition)

---

## 3. Reservations API

### GET /api/v1/reservations

List/search reservations.

**Query Parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page (max 100) |
| status | string? | null | Filter: active, confirmed, cancelled, expired, checkedIn |
| dateFrom | date? | null | Filter by check-in date range start |
| dateTo | date? | null | Filter by check-in date range end |
| guestId | int? | null | Filter by guest |
| roomId | int? | null | Filter by room |
| roomTypeId | int? | null | Filter by room type |
| groupId | int? | null | Filter by reservation group |

**Response 200:**
```json
{
  "items": [
    {
      "id": 2001,
      "guestId": 55,
      "guestName": "Marko Marković",
      "checkInDate": "2026-05-20",
      "checkOutDate": "2026-05-23",
      "roomTypeId": 3,
      "roomTypeName": "Standard Double",
      "confirmationNumber": 1051,
      "confirmed": true,
      "cancelled": false,
      "cancellationNumber": null,
      "cancellationReason": null,
      "confirmedAt": "2026-05-18T10:00:00Z",
      "cancelledAt": null,
      "checkedIn": false,
      "expired": false,
      "groupId": null,
      "groupName": null,
      "memo": "",
      "contactName": "",
      "contactPhone": "",
      "contactEmail": "",
      "roomAssignments": [
        {
          "id": 301,
          "roomId": 101,
          "roomName": "101",
          "tariffId": 5,
          "tariff": 55.00,
          "guestCount": 2,
          "smoker": false
        }
      ]
    }
  ],
  "totalCount": 85,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

**Legacy Evidence:** `13_RESERVATIONS.md:1.1` (reservation creation paths), `frmRezervacije.vb:74-86` (SP calls), `13_RESERVATIONS.md:4.1-4.5` (status fields)

---

### POST /api/v1/reservations

Create a new reservation.

**Request Body:**
```json
{
  "guestId": 55,
  "checkInDate": "2026-05-20",
  "checkOutDate": "2026-05-23",
  "roomTypeId": 3,
  "groupId": null,
  "typeId": 1,
  "sourceId": 0,
  "memo": "",
  "contactName": "Marko Marković",
  "contactPhone": "+38761123456",
  "contactEmail": "marko@example.com",
  "roomAssignments": [
    {
      "roomId": 101,
      "tariffId": 5,
      "guestCount": 2,
      "smoker": false
    }
  ],
  "alarm": null
}
```

**Validation Rules:**

| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| checkInDate | required, must be valid date | RESV_DATE_REQUIRED | `13_RESERVATIONS.md:5.6` |
| checkOutDate | required, must be after checkInDate | RESV_DATE_ORDER_INVALID | `13_RESERVATIONS.md:5.6` |
| guestId | required, > 0 | RESV_GUEST_REQUIRED | `frmRezervacije_unos.vb:298` |
| roomTypeId | required, must exist | RESV_ROOM_TYPE_REQUIRED | `frmRezervacije_unos.vb:292` |
| roomAssignments | at least 1, roomCount > 0 | RESV_ROOMS_REQUIRED | `13_RESERVATIONS.md:5.4` |
| room availability | no double-booking for same room/dates | RESV_ROOM_NOT_AVAILABLE | `13_RESERVATIONS.md:7.1.10` |

**Default values applied:**

| Field | Default | Legacy Evidence |
|-------|---------|-----------------|
| prijava | 0 | `frmRezervacije.vb:942` |
| stornirana | '0' | `13_RESERVATIONS.md:4.2` |
| potvrda | 0 | `13_RESERVATIONS.md:4.3` |
| brojRezSoba | from roomAssignments count | `13_RESERVATIONS.md:5.4` |

**Response 201:**
```json
{
  "id": 2001,
  "guestId": 55,
  "guestName": "Marko Marković",
  "checkInDate": "2026-05-20",
  "checkOutDate": "2026-05-23",
  "roomTypeId": 3,
  "roomTypeName": "Standard Double",
  "confirmationNumber": null,
  "confirmed": false,
  "cancelled": false,
  "checkedIn": false,
  "expired": false,
  "roomAssignments": [
    {
      "id": 301,
      "roomId": 101,
      "roomName": "101",
      "tariffId": 5,
      "tariff": 55.00,
      "guestCount": 2,
      "smoker": false
    }
  ],
  "createdAt": "2026-05-18T10:00:00Z",
  "createdBy": "radnik1"
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | RESV_DATE_ORDER_INVALID | checkOutDate must be after checkInDate | `13_RESERVATIONS.md:5.6` |
| 400 | RESV_GUEST_REQUIRED | guestId is required | `frmRezervacije_unos.vb:298` |
| 400 | RESV_ROOM_TYPE_REQUIRED | roomTypeId must be selected | `frmRezervacije_unos.vb:292` |
| 400 | RESV_ROOM_NOT_AVAILABLE | Room already booked for this date range | `13_RESERVATIONS.md:7.1.10` |
| 409 | RESV_CONFIRMATION_RACE | Concurrent confirmation number generation | `13_RESERVATIONS.md:4.4` |

**Legacy Evidence:** `frmRezervacije.vb:942,967` (quick-create), `frmRezervacije_unos.vb:338,357` (full form create), `13_RESERVATIONS.md:3.1`

---

### PUT /api/v1/reservations/{id}

Update reservation (edit).

**Request Body:**
```json
{
  "checkInDate": "2026-05-21",
  "checkOutDate": "2026-05-24",
  "roomTypeId": 3,
  "memo": "Late arrival",
  "roomAssignments": [
    {
      "roomId": 102,
      "tariffId": 6,
      "guestCount": 2,
      "smoker": false
    }
  ]
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-RESV-001 | Reservation room assignments are nuked and re-inserted (DELETE+INSERT pattern) | — | `frmRezervacije_unos.vb:413-429` |
| BR-RESV-002 | promjena (change counter) incremented on every edit | — | `frmRezervacije_unos.vb:407` |
| BR-RESV-003 | Cannot edit reservation with prijava=1 (already checked in) | RESV_ALREADY_CHECKED_IN | `13_RESERVATIONS.md:4.1` |

**Response 200:** Same schema as POST response.

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | RESV_NOT_FOUND | Reservation ID does not exist |
| 409 | RESV_ALREADY_CHECKED_IN | Reservation has prijava=1 |

**Legacy Evidence:** `frmRezervacije_unos.vb:407` (UPDATE rezervacije with promjena+1)

---

### DELETE /api/v1/reservations/{id}

Cancel (storno) a reservation. This is a logical delete — sets `stornirana=1`.

**Request Body:**
```json
{
  "reason": "Guest changed travel plans"
}
```

**Response 200:**
```json
{
  "id": 2001,
  "stornirana": true,
  "cancellationNumber": 42,
  "cancellationDate": "2026-05-18T11:00:00Z",
  "cancellationReason": "Guest changed travel plans"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-RESV-004 | brojStorna auto-generated via sequence (not MAX()+1) | — | `13_RESERVATIONS.md:4.5` (legacy uses MAX+1 race condition) |
| BR-RESV-005 | datestorno set to current timestamp | — | `frmRezervacije_unos.vb:800` |
| BR-RESV-006 | Cancelled reservations are excluded from active views (stornirana='0' filter) | — | `13_RESERVATIONS.md:4.2` |

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | RESV_NOT_FOUND | Reservation ID does not exist | — |
| 409 | RESV_ALREADY_CANCELLED | Reservation already has stornirana='1' | `13_RESERVATIONS.md:4.2` |
| 409 | RESV_ALREADY_CHECKED_IN | Reservation has prijava=1, cannot cancel | `13_RESERVATIONS.md:4.1` |

**Legacy Evidence:** `frmRezervacijePregled.vb:357-359` (storno toggle), `frmRezervacije_unos.vb:800-811` (storno/un-storno)

---

### PUT /api/v1/reservations/{id}/confirm

Confirm (potvrda) a reservation.

**Request Body:**
```json
{}
```

**Response 200:**
```json
{
  "id": 2001,
  "confirmed": true,
  "confirmationNumber": 1051,
  "confirmedAt": "2026-05-18T10:00:00Z"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-RESV-007 | brojPotvrde auto-generated via sequence (not MAX()+1) | — | `13_RESERVATIONS.md:4.4` (legacy race condition) |
| BR-RESV-008 | datepotvrda set to current timestamp | — | `frmRezervacije_unos.vb:828` |
| BR-RESV-009 | Confirmation can be toggled off (un-confirm sets potvrda=0) | — | `13_RESERVATIONS.md:4.3` |

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | RESV_NOT_FOUND | Reservation ID does not exist |
| 409 | RESV_ALREADY_CONFIRMED | Reservation already confirmed |

**Legacy Evidence:** `frmRezervacijePregled.vb:457` (confirm), `frmRezervacije_unos.vb:828` (confirm with date+number)

---

### PUT /api/v1/reservations/{id}/cancel

Cancel (storno) reservation. Alias for DELETE but as PUT.

(Same as DELETE endpoint above — included for semantic clarity.)

---

### POST /api/v1/reservations/{id}/transfer-to-checkin

Transfer reservation to check-in (Prebaci flow).

**Request Body:**
```json
{
  "mode": "byRooms",
  "assignments": [
    {
      "roomAssignmentId": 301,
      "roomId": 101,
      "guests": [
        {
          "guestId": 55,
          "tariffId": 5,
          "discount": 0,
          "discountReason": null,
          "status": 1
        }
      ]
    }
  ]
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-RESV-010 | Total guest count must not exceed bed capacity × rooms | RESV_BED_CAPACITY_EXCEEDED | `13_RESERVATIONS.md:2.2` (jednaki validation) |
| BR-RESV-011 | Creates guest records (clones from reservation guest) | — | `frmRezervacijePrebaci.vb:593-641` |
| BR-RESV-012 | Creates folio per room (posjetaFolio with zakljucen=0) | — | `13_RESERVATIONS.md:2.2` |
| BR-RESV-013 | Creates relGostSoba with rezervP=1 | — | `frmRezervacijePrebaci.vb:648` |
| BR-RESV-014 | Creates nocenja via Unesinocenja SP | — | `frmRezervacijePrebaci.vb:791-821` |
| BR-RESV-015 | Creates rezervacijaPrijava mapping record | — | `frmRezervacijePrebaci.vb:955-981` |
| BR-RESV-016 | If all rooms checked in: prijava=1; if partial: brojRezSoba decreased | — | `13_RESERVATIONS.md:2.2` |
| BR-RESV-017 | Clears gosti.Rid after check-in | — | `frmRezervacijePrebaci.vb:760` |
| BR-RESV-018 | Transaction must wrap all operations (5-8 DB writes) | — | `13_RESERVATIONS.md:7.1.5` (legacy: no transaction) |

**Response 201:**
```json
{
  "reservationId": 2001,
  "prijava": 1,
  "checkedInRooms": 1,
  "totalRooms": 1,
  "stays": [
    {
      "stayId": 1501,
      "guestId": 55,
      "roomId": 101,
      "folioId": 201,
      "checkInDate": "2026-05-18T14:30:00Z"
    }
  ]
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | RESV_BED_CAPACITY_EXCEEDED | Guest count exceeds room capacity | `13_RESERVATIONS.md:5.5` |
| 404 | RESV_NOT_FOUND | Reservation ID does not exist | — |
| 409 | RESV_ALREADY_CHECKED_IN | Reservation prijava=1 | `13_RESERVATIONS.md:4.1` |
| 409 | RESV_ROOM_OCCUPIED | Target room already occupied | — |

**Legacy Evidence:** `13_RESERVATIONS.md:1.4` (prebaci flow), `frmRezervacijePrebaci.vb:409-497` (transfer logic)

---

## 4. Check-in / Stays API

### POST /api/v1/stays

Check-in guests (direct or from reservation).

**Request Body:**
```json
{
  "roomId": 101,
  "reservationId": null,
  "checkInDate": "2026-05-18T14:30:00Z",
  "checkOutDate": "2026-05-21T10:00:00Z",
  "groupId": null,
  "guests": [
    {
      "guestId": 55,
      "tariffId": 5,
      "discount": 0,
      "discountReason": null,
      "status": 1,
      "tax": 1.50
    },
    {
      "guestId": 56,
      "tariffId": 5,
      "discount": 10,
      "discountReason": "Osoba ima 8. godina",
      "status": 4,
      "tax": 0
    }
  ]
}
```

**Validation Rules:**

| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| roomId | required, room must exist | CHECKIN_ROOM_REQUIRED | `frmPrijava1.vb:372` |
| checkInDate | required, must be valid | CHECKIN_DATE_INVALID | `frmPrijava1.vb:366` |
| checkOutDate | must be after checkInDate | CHECKIN_DATE_ORDER_INVALID | `frmPrijava1.vb:379` |
| checkInDate | must not be >20 days before last night charge | CHECKIN_DATE_TOO_OLD | `frmPrijava1.vb:376` |
| guests | at least 1 guest required | CHECKIN_GUESTS_REQUIRED | `frmPrijava1.vb:382` |
| guests | at least 1 NEW guest (not already checked in this room) | CHECKIN_NEW_GUEST_REQUIRED | `frmPrijava1.vb:385` |
| guests | no duplicate guestId in same room | CHECKIN_DUPLICATE_GUEST | `frmPrijava1.vb:224` |
| room | must be free (status=0) or reserved for these guests | CHECKIN_ROOM_NOT_AVAILABLE | `10_CHECKIN.md:5.1` |
| room | must not be OOO (ooo=1) | CHECKIN_ROOM_OOO | `10_CHECKIN.md:5.1` |

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-CI-001 | Folio: if room has open folio, reuse it; otherwise create new | — | `10_CHECKIN.md:5.4` |
| BR-CI-002 | Tariff: naplposo=0 → split per person; naplposo=1 → full tariff per person | — | `10_CHECKIN.md:5.5` |
| BR-CI-003 | Minimum charge: if calculated price = 0, use taxa + osig | — | `10_CHECKIN.md:5.6` |
| BR-CI-004 | Night charges: created via Unesinocenja (DELETE same-month + INSERT) | — | `10_CHECKIN.md:5.15` |
| BR-CI-005 | Room clean=1 set after check-in | — | `10_CHECKIN.md:3.2` |
| BR-CI-006 | From reservation: creates rezervacijaPrijava mapping, sets prijava=1 | — | `13_RESERVATIONS.md:2.2` |
| BR-CI-007 | Entire check-in wrapped in transaction | — | `10_CHECKIN.md:8.2.1` (legacy: no transaction) |

**Response 201:**
```json
{
  "roomId": 101,
  "roomName": "101",
  "folioId": 201,
  "guests": [
    {
      "stayId": 1501,
      "guestId": 55,
      "guestName": "Marko Marković",
      "checkInDate": "2026-05-18T14:30:00Z",
      "checkOutDate": "2026-05-21T10:00:00Z",
      "tariff": 55.00,
      "discount": 0,
      "discountReason": null,
      "status": 1,
      "tax": 1.50
    },
    {
      "stayId": 1502,
      "guestId": 56,
      "guestName": "Ana Petrović",
      "checkInDate": "2026-05-18T14:30:00Z",
      "checkOutDate": "2026-05-21T10:00:00Z",
      "tariff": 55.00,
      "discount": 10,
      "discountReason": "Osoba ima 8. godina",
      "status": 4,
      "tax": 0
    }
  ],
  "reservationId": null,
  "roomStatusAfter": {
    "code": 1,
    "label": "ZAUZETA"
  }
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | CHECKIN_ROOM_REQUIRED | Room type and room must be selected | `frmPrijava1.vb:366,372` |
| 400 | CHECKIN_DATE_ORDER_INVALID | checkOutDate must be after checkInDate | `frmPrijava1.vb:379` |
| 400 | CHECKIN_DATE_TOO_OLD | checkInDate > 20 days before last charge | `frmPrijava1.vb:376` |
| 400 | CHECKIN_GUESTS_REQUIRED | At least one guest required | `frmPrijava1.vb:382` |
| 400 | CHECKIN_NEW_GUEST_REQUIRED | At least one new guest required | `frmPrijava1.vb:385` |
| 400 | CHECKIN_DUPLICATE_GUEST | Same guest already in room | `frmPrijava1.vb:224` |
| 409 | CHECKIN_ROOM_OCCUPIED | Room already occupied | `30_STATUS_MATRIX.md:1.1` |
| 409 | CHECKIN_ROOM_OOO | Room is out of order | `10_CHECKIN.md:4.12` |
| 409 | CHECKIN_FOLIO_RACE | Concurrent folio creation detected | `10_CHECKIN.md:8.2.3` |

**Legacy Evidence:** `10_CHECKIN.md:1.8` (submitting check-in), `frmPrijava1.vb:365-488` (validations and processing)

---

### PUT /api/v1/stays/{id}

Modify a stay (update checkout date, tariff, discount, etc.).

**Request Body:**
```json
{
  "checkOutDate": "2026-05-22T10:00:00Z",
  "tariffId": 6,
  "discount": 5,
  "discountReason": "Loyalty discount"
}
```

**Response 200:**
```json
{
  "stayId": 1501,
  "guestId": 55,
  "roomId": 101,
  "checkInDate": "2026-05-18T14:30:00Z",
  "checkOutDate": "2026-05-22T10:00:00Z",
  "tariffId": 6,
  "tariff": 60.00,
  "discount": 5,
  "discountReason": "Loyalty discount",
  "status": 1
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | STAY_NOT_FOUND | Stay (relgostsoba) record not found |
| 409 | STAY_ALREADY_CHECKED_OUT | Stay is already checked out |

**Legacy Evidence:** `frmSobaInfoPromjena.vb:38` (promijeniDatumVrijeme SP for checkout date), `frmPrijavaGostiKucice.vb:~710` (UPDATE status/tax)

---

### POST /api/v1/stays/{id}/room-transfer

Transfer a guest to a different room (promijenaSobe).

**Request Body:**
```json
{
  "targetRoomId": 205,
  "transferAllGuests": true,
  "transferExpenses": true,
  "transferNights": true
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-XFER-001 | Transfer all guests mode: all guests in room move together | — | `12_ROOM_STATUS.md:5.2` (rbt1.Checked) |
| BR-XFER-002 | Single guest mode: if last guest, error "room can't be empty" | XFER_LAST_GUEST | `16_INVOICE_CHECKOUT.md:5.4` |
| BR-XFER-003 | Old folio closed (zakljucen=1), new folio created or target folio used | — | `12_ROOM_STATUS.md:5.2` |
| BR-XFER-004 | All open expenses (zaklj=0) transferred to new room SID | — | `12_ROOM_STATUS.md:5.2` (samoTroskovi) |
| BR-XFER-005 | Night charges re-created via Unesinocenja SP | — | `12_ROOM_STATUS.md:5.2` |
| BR-XFER-006 | Old room marked clean=0 | — | `12_ROOM_STATUS.md:5.3` |
| BR-XFER-007 | If target room occupied: merge folios | — | `frmSobaInfo.vb:1017` |
| BR-XFER-008 | Entire transfer must be atomic (transaction) | — | `12_ROOM_STATUS.md:7.1.2` |

**Response 200:**
```json
{
  "sourceRoomId": 101,
  "targetRoomId": 205,
  "guestsTransferred": 2,
  "sourceFolioId": 201,
  "sourceFolioClosed": true,
  "targetFolioId": 305,
  "sourceRoomStatus": {
    "code": 7,
    "label": "NIJE SPREMNA",
    "clean": false
  },
  "targetRoomStatus": {
    "code": 1,
    "label": "ZAUZETA",
    "clean": true
  }
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | STAY_NOT_FOUND | Stay ID not found | — |
| 404 | TARGET_ROOM_NOT_FOUND | Target room doesn't exist | `frmSobaInfo.vb:1328` |
| 409 | XFER_LAST_GUEST | Cannot transfer last guest from room | `16_INVOICE_CHECKOUT.md:5.4` |
| 409 | XFER_TARGET_OOO | Target room is OOO | — |

**Legacy Evidence:** `12_ROOM_STATUS.md:5.2` (promijenaSobe full flow), `frmSobaInfo.vb:1392` (transfer method)

---

## 5. Expenses & Nights API

### POST /api/v1/expenses

Add an expense (troskovi).

**Request Body:**
```json
{
  "stayId": 1501,
  "roomId": 101,
  "expenseTypeId": 2,
  "quantity": 1,
  "amount": 25.00,
  "note": "",
  "kasaReferenceId": null
}
```

**Validation Rules:**

| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| stayId | required, must exist in relgostsoba | EXPENSE_STAY_REQUIRED | `15_EXPENSES_NIGHTS.md:1.1` |
| roomId | required, must match stay's room | EXPENSE_ROOM_REQUIRED | `15_EXPENSES_NIGHTS.md:1.1` |
| expenseTypeId | required, must not be 1 (accommodation managed separately) | EXPENSE_TYPE_INVALID | `frmTroskovi.vb:87` (WHERE ID<>1) |
| amount | required, must be > -200 | EXPENSE_AMOUNT_INVALID | `frmTroskovi.vb:404` |
| quantity | required, must be > 0 | EXPENSE_QUANTITY_INVALID | — |
| expenseTypeName | cannot start with "osigu" or "bora" | EXPENSE_PROTECTED_NAME | `frmTroskovi.vb:357-360` |

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-EXP-001 | TID=1 (accommodation) is excluded — nights managed separately | EXPENSE_TYPE_INVALID | `frmTroskovi.vb:87` |
| BR-EXP-002 | Insurance ("osigu") and tourist tax ("bora") names cannot be manually created | EXPENSE_PROTECTED_NAME | `frmTroskovi.vb:357-360` |
| BR-EXP-003 | TID=5 (mini-bar/POS) requires KASA integration reference | EXPENSE_KASA_REQUIRED | `frmTroskovi.vb:332-346` |
| BR-EXP-004 | radnikID should be logged-in worker (legacy hardcoded to 1) | — | `15_EXPENSES_NIGHTS.md:7.1.2` |

**Response 201:**
```json
{
  "id": 3001,
  "stayId": 1501,
  "roomId": 101,
  "expenseTypeId": 2,
  "expenseTypeName": "Restoran",
  "quantity": 1,
  "amount": 25.00,
  "date": "2026-05-18T12:00:00Z",
  "closed": false,
  "closedReceiptNumber": null,
  "partialPayment": false,
  "note": "",
  "workerId": 3,
  "kasaReferenceId": null
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | EXPENSE_TYPE_INVALID | ExpenseTypeId cannot be 1 (accommodation) | `frmTroskovi.vb:87` |
| 400 | EXPENSE_AMOUNT_INVALID | Amount must be > -200 | `frmTroskovi.vb:404` |
| 400 | EXPENSE_PROTECTED_NAME | Name starts with protected prefix | `frmTroskovi.vb:357-360` |
| 404 | STAY_NOT_FOUND | Stay ID not found | — |
| 404 | ROOM_NOT_FOUND | Room ID not found | — |

**Legacy Evidence:** `15_EXPENSES_NIGHTS.md:1.1` (expense add flow), `ModuleKod.vb:903` (addTroskovi SP)

---

### PUT /api/v1/expenses/{id}

Update an expense (amount, quantity, note).

**Request Body:**
```json
{
  "quantity": 2,
  "amount": 50.00,
  "note": "Room service x2"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-EXP-005 | Cannot modify closed (zaklj=1) expenses | EXPENSE_ALREADY_CLOSED | `15_EXPENSES_NIGHTS.md:4.1` |

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | EXPENSE_NOT_FOUND | Expense ID not found |
| 409 | EXPENSE_ALREADY_CLOSED | Expense is closed (zaklj=1) — cannot modify |

**Legacy Evidence:** `frmTroskovi.vb` (inline grid edit for expense types)

---

### DELETE /api/v1/expenses/{id}

Delete an expense (only if open/zaklj=0).

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-EXP-006 | Only open expenses (zaklj=0) can be deleted | EXPENSE_ALREADY_CLOSED | `15_EXPENSES_NIGHTS.md:5.3` |

**Response 200:**
```json
{
  "id": 3001,
  "deleted": true
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | EXPENSE_NOT_FOUND | Expense ID not found |
| 409 | EXPENSE_ALREADY_CLOSED | Expense is closed (zaklj=1) — cannot delete |

**Legacy Evidence:** `15_EXPENSES_NIGHTS.md:5.3` (no physical DELETE on troskovi; storno pattern used instead — but API provides explicit delete for open expenses only)

---

### PUT /api/v1/expenses/{id}/lock

Lock (close) an expense after payment.

**Request Body:**
```json
{
  "receiptNumber": 1050,
  "partialPayment": false,
  "remainingAmount": null
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-EXP-007 | zaklj set to 1, Brrac set to receiptNumber | — | `frmPlacanje.vb:3808` |
| BR-EXP-008 | Partial payment: Djelimicno=1, iznos set to remaining | — | `frmPlacanje.vb:3812` |

**Response 200:**
```json
{
  "id": 3001,
  "closed": true,
  "closedReceiptNumber": 1050,
  "partialPayment": false,
  "remainingAmount": null
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | EXPENSE_NOT_FOUND | Expense ID not found |
| 409 | EXPENSE_ALREADY_CLOSED | Expense already closed |

**Legacy Evidence:** `15_EXPENSES_NIGHTS.md:4.1` (zaklj transition), `frmPlacanje.vb:3808,3812` (lock and partial lock)

---

### GET /api/v1/nights

Get night charges (nocenja) for a room/folio.

**Query Parameters:**

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| roomId | int? | null | Filter by room (SID) |
| folioId | int? | null | Filter by folio (PID) |
| stayId | int? | null | Filter by stay (RID) |
| active | bool? | true | Filter by PrijavaOdjava=0 (active) or 1 (closed) |
| dateFrom | date? | null | Filter from date |
| dateTo | date? | null | Filter to date |

**Response 200:**
```json
{
  "items": [
    {
      "id": 4001,
      "stayId": 1501,
      "guestId": 55,
      "date": "2026-05-15",
      "roomId": 101,
      "folioId": 201,
      "tariff": 55.00,
      "discount": 0,
      "description": "",
      "active": true,
      "checkoutDate": null,
      "receiptNumber": 0
    }
  ]
}
```

**Legacy Evidence:** `15_EXPENSES_NIGHTS.md:4.2` (nocenja.PrijavaOdjava), `frmPrikazNocenja1.vb:5` (WHERE RID AND PrijavaOdjava=0)

---

### POST /api/v1/nights

Create/replace night charges for a guest (Unesinocenja pattern).

**Request Body:**
```json
{
  "stayId": 1501,
  "roomId": 101,
  "folioId": 201,
  "tariff": 55.00,
  "discount": 0,
  "description": "",
  "date": "2026-05-18",
  "roomName": "101"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-NIGHT-001 | Deletes same-month existing night records for this RID before inserting (upsert pattern) | — | `15_EXPENSES_NIGHTS.md:4.5` |
| BR-NIGHT-002 | PrijavaOdjava is always set to 0 (active) | — | `15_EXPENSES_NIGHTS.md:4.2` |

**Response 201:**
```json
{
  "id": 4005,
  "stayId": 1501,
  "date": "2026-05-18",
  "tariff": 55.00,
  "discount": 0,
  "active": true
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 400 | NIGHT_DATE_INVALID | Date format incorrect |
| 404 | STAY_NOT_FOUND | Stay ID not found |

**Legacy Evidence:** `ModuleKod.vb:1081` (Unesinocenja SP)

---

### PUT /api/v1/nights

Update night charges (tariff edit).

**Request Body:**
```json
{
  "nightIds": [4001, 4002],
  "tariff": 50.00
}
```

**Response 200:**
```json
{
  "updatedCount": 2
}
```

**Legacy Evidence:** `frmPrikazNocenja2.vb:27` (UPDATE nocenja SET Tarifa), `frmPlacanjeTarifa.vb:46` (per-row tariff update)

---

## 6. Payments API

### POST /api/v1/payments

Record a payment for a folio.

**Request Body:**
```json
{
  "folioId": 201,
  "stayId": 1501,
  "roomId": 101,
  "amount": 165.00,
  "discount": 0,
  "paymentMethodId": 1,
  "receiptNumber": null,
  "isAdvance": false,
  "isVatRegistered": true,
  "includeTouristTax": true,
  "currency": "EUR",
  "exchangeRate": 1,
  "workerId": 3,
  "note": "",
  "expenseLines": [
    {
      "expenseTypeId": 1,
      "name": "Nocenje sa doruckom",
      "quantity": 3,
      "unitPrice": 55.00,
      "amount": 165.00,
      "discount": 0
    }
  ],
  "splitPayments": null
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-PAY-001 | Receipt number auto-generated via sequence (not MAX()+1) | — | `14_PAYMENT.md:5.1` (legacy race condition) |
| BR-PAY-002 | PDV/VAT calculated from settings.pdv | — | `14_PAYMENT.md:5.4` |
| BR-PAY-003 | Insurance = persons × nights × osig; tax = persons × nights × taxa | — | `14_PAYMENT.md:5.3` |
| BR-PAY-004 | Expenses marked as closed (zaklj=1) with receipt number | — | `14_PAYMENT.md:5.2` |
| BR-PAY-005 | Night charges closed (PrijavaOdjava=1) with receipt number | — | `14_PAYMENT.md:5.2` |
| BR-PAY-006 | Payment method 5 (Slozeno/compound) excluded from direct payment | PAY_METHOD_INVALID | `14_PAYMENT.md:1.3` |
| BR-PAY-007 | Night count: check-in <08:00 → -1 day; checkout ≥12:00 → +1 day; minimum 1 | — | `14_PAYMENT.md:5.12` |

**Response 201:**
```json
{
  "paymentId": 5001,
  "receiptNumber": 1050,
  "folioId": 201,
  "amount": 165.00,
  "paymentMethodId": 1,
  "paymentMethodName": "Gotovina",
  "date": "2026-05-18T10:30:00Z",
  "isAdvance": false,
  "isStorno": false,
  "invoiceSnapshotId": 7001,
  "expenseLines": [
    {
      "lineId": 6001,
      "expenseTypeId": 1,
      "name": "Nocenje sa doruckom",
      "quantity": 3,
      "unitPrice": 55.00,
      "amount": 165.00,
      "vatAmount": 23.93,
      "discount": 0
    }
  ]
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | PAY_AMOUNT_INVALID | Payment amount must be positive | — |
| 400 | PAY_FOLIO_NOT_FOUND | Folio ID not found | — |
| 400 | PAY_FOLIO_CLOSED | Folio is already closed (zakljucen=1) | — |
| 400 | PAY_METHOD_INVALID | Payment method 5 (compound) not allowed directly | `14_PAYMENT.md:1.3` |
| 409 | PAY_RECEIPT_DUPLICATE | Receipt number already exists | `14_PAYMENT.md:5.1` |

**Legacy Evidence:** `14_PAYMENT.md:1.1` (payment flow), `frmPlacanje.vb:4027` (INSERT placanje), `frmPlacanje.vb:3808` (mark expenses closed)

---

### POST /api/v1/payments/{id}/storno

Storno (cancel) a payment.

**Request Body:**
```json
{
  "reason": "Guest requested cancellation"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-PAY-STORNO-001 | Sets placanje.storno=1, placanjeDetalji.storno=1 | — | `frmRacuni.vb:731,734` |
| BR-PAY-STORNO-002 | Sets printracuni.storno=1, exp=2, datstor=now | — | `frmRacuni.vb:738` |
| BR-PAY-STORNO-003 | Reopens non-accommodation expenses (zaklj=0, Brrac=null WHERE TID<>1) | — | `frmRacuni.vb:741` |
| BR-PAY-STORNO-004 | Deletes accommodation expenses (DELETE WHERE TID=1) | — | `frmRacuni.vb:743` |
| BR-PAY-STORNO-005 | Must be wrapped in transaction | — | `frmRacuni.vb:711-750` |
| BR-PAY-STORNO-006 | Optionally dispatches fiscal storno command | — | `16_INVOICE_CHECKOUT.md:4.3` |

**Response 200:**
```json
{
  "paymentId": 5001,
  "receiptNumber": 1050,
  "storno": true,
  "stornoDate": "2026-05-18T14:00:00Z",
  "stornoReason": "Guest requested cancellation",
  "reopenedExpenses": [
    {
      "id": 3002,
      "expenseTypeId": 2,
      "amount": 25.00,
      "closed": false
    }
  ],
  "deletedAccommodationExpenses": [
    {
      "id": 3001,
      "expenseTypeId": 1,
      "amount": 165.00
    }
  ]
}
```

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | PAYMENT_NOT_FOUND | Payment ID not found |
| 409 | PAYMENT_ALREADY_STORNO | Payment already storned |

**Legacy Evidence:** `16_INVOICE_CHECKOUT.md:4.3` (storno flow), `frmRacuni.vb:711-750` (transaction)

---

### POST /api/v1/payments/split

Record a split (compound) payment.

**Request Body:**
```json
{
  "folioId": 201,
  "totalAmount": 165.00,
  "splitPayments": [
    { "paymentMethodId": 1, "amount": 100.00 },
    { "paymentMethodId": 3, "amount": 65.00 }
  ]
}
```

**Validation Rules:**

| Field | Rule | Error Code | Legacy Evidence |
|-------|------|-----------|-----------------|
| totalAmount | must equal sum of splitPayments amounts | PAY_SPLIT_AMOUNT_MISMATCH | `frmPlacanjeSlozeno.vb:205` |
| paymentMethodId | cannot be 5 (compound) | PAY_METHOD_INVALID | `frmPlacanjeSlozeno.vb:30` |
| remaining | must reach 0 after all splits | PAY_SPLIT_REMAINING | `frmPlacanjeSlozeno.vb:225` |

**Response 201:**
```json
{
  "paymentId": 5002,
  "receiptNumber": 1051,
  "totalAmount": 165.00,
  "splitPayments": [
    { "paymentMethodId": 1, "amount": 100.00, "paymentMethodName": "Gotovina" },
    { "paymentMethodId": 3, "amount": 65.00, "paymentMethodName": "Kartica" }
  ]
}
```

**Legacy Evidence:** `14_PAYMENT.md:1.3` (split payment), `frmPlacanjeSlozeno.vb:205-225`

---

## 7. Invoices API

### POST /api/v1/invoices

Create an invoice from folio payment data.

**Request Body:**
```json
{
  "paymentId": 5001,
  "guestId": 55,
  "roomId": 101,
  "checkInDate": "2026-05-15",
  "checkOutDate": "2026-05-18",
  "paymentMethodId": 1,
  "isVatRegistered": true,
  "includeTouristTax": true,
  "note": ""
}
```

**Response 201:**
```json
{
  "invoiceId": 7001,
  "receiptNumber": 1050,
  "guestName": "Marko Marković",
  "companyName": "",
  "periodFrom": "2026-05-15",
  "periodTo": "2026-05-18",
  "roomName": "101",
  "date": "2026-05-18T10:30:00Z",
  "lineItems": [
    {
      "lineId": 8001,
      "name": "Nocenje sa doruckom",
      "quantity": 3,
      "priceWithoutVat": 141.07,
      "vatAmount": 23.93,
      "total": 165.00,
      "discount": 0,
      "discountReason": null,
      "paymentMethod": "Gotovina",
      "currency": "EUR"
    }
  ],
  "footer": {
    "advancePayment": 0,
    "nights": 3,
    "note": "",
    "fiscalReference": null
  },
  "storno": false,
  "stornoDate": null,
  "isAdvance": false,
  "fiscalSent": false,
  "fiscalResponse": null
}
```

**Legacy Evidence:** `16_INVOICE_CHECKOUT.md:1.1` (invoice snapshot creation), `frmPlacanje.vb:3172` (printracuni INSERT), `16_INVOICE_CHECKOUT.md:3.1`

---

### GET /api/v1/invoices/{id}

Get invoice detail.

**Response 200:** Same schema as POST response above.

**Error Codes:**

| Status | Code | Description |
|--------|------|-------------|
| 404 | INVOICE_NOT_FOUND | Invoice ID not found |

**Legacy Evidence:** `frmRacuni.vb` (getPrintHeader, getPrintDetalji, getPrintFooter SPs)

---

### POST /api/v1/invoices/{id}/storno

Storno (cancel) an invoice.

**Request Body:**
```json
{
  "reason": "Duplicate invoice"
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-INV-STORNO-001 | Transaction: placanje.storno=1, placanjeDetalji.storno=1, printracuni.storno=1/exp=2, datstor=now | — | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-INV-STORNO-002 | Reopen non-accommodation expenses: troskovi.zaklj=0, Brrac=null WHERE TID<>1 | — | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-INV-STORNO-003 | Delete accommodation expenses: DELETE troskovi WHERE Brrac=@Rbr AND TID=1 | — | `16_INVOICE_CHECKOUT.md:4.3` |
| BR-INV-STORNO-004 | Advance invoice storno ONLY sets printracuniavans.storno=1 | — | `16_INVOICE_CHECKOUT.md:4.3` |

**Response 200:**
```json
{
  "invoiceId": 7001,
  "receiptNumber": 1050,
  "storno": true,
  "stornoDate": "2026-05-18T15:00:00Z",
  "stornoReason": "Duplicate invoice",
  "reopenedExpenses": [
    {
      "id": 3002,
      "expenseTypeId": 2,
      "name": "Restoran",
      "amount": 25.00
    }
  ],
  "deletedAccommodationExpenses": [
    {
      "id": 3001,
      "expenseTypeId": 1,
      "name": "Nocenje sa doruckom",
      "amount": 165.00
    }
  ],
  "fiscalStornoSent": false
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | INVOICE_NOT_FOUND | Invoice ID not found | — |
| 409 | INVOICE_ALREADY_STORNO | Invoice already storned | `16_INVOICE_CHECKOUT.md:4.1` |

**Legacy Evidence:** `16_INVOICE_CHECKOUT.md:4.3` (full storno transaction), `frmRacuni.vb:711-750`

---

### POST /api/v1/invoices/{id}/fiscalize

Send invoice to fiscal device.

**Request Body:**
```json
{
  "fiscalDeviceType": "tring",
  "testMode": false
}
```

**Response 200:**
```json
{
  "invoiceId": 7001,
  "fiscalSent": true,
  "fiscalReceiptNumber": "20260518-1050",
  "fiscalVerificationCode": "ABC123",
  "fiscalAmount": 165.00,
  "fiscalDate": "2026-05-18T10:30:15Z",
  "legalNote": "Po clanu 42 fiskalnog zakona..."
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | FISCAL_DEVICE_NOT_CONFIGURED | No fiscal device configured for this hotel | `17_FISCAL_PROFORMA.md:1.1` |
| 503 | FISCAL_DEVICE_UNAVAILABLE | Fiscal device not responding | `14_PAYMENT.md:1.1` |
| 500 | FISCAL_DEVICE_ERROR | Fiscal device returned error | `17_FISCAL_PROFORMA.md:1.2` |

**Business Rules:**

| Rule | Description | Legacy Evidence |
|------|-------------|-----------------|
| BR-FISC-001 | Fiscal device type determined by setings.fiscal (fsc array) | `17_FISCAL_PROFORMA.md:1.1` |
| BR-FISC-002 | Tax rate mapping: 17%=E (standard), 0% exempt=K, 0% non-reg=A | `17_FISCAL_PROFORMA.md:6.1` |
| BR-FISC-003 | Special PLU codes: Osiguranje=10000, Boravisna Taksa=10001 | `17_FISCAL_PROFORMA.md:6.2` |
| BR-FISC-004 | Fiscal response stored in printracuni.fisrac/fisvr/fisIZN | `ModuleKod.vb:3107-3143` |
| BR-FISC-005 | Legal note appended: "Po clanu 42 fiskalnog zakona..." | `ModuleKod.vb:3121` |

**Legacy Evidence:** `17_FISCAL_PROFORMA.md:1.2` (6 fiscal device types), `frmPlacanje.vb:2506-2512` (dispatch)

---

## 8. Checkout API

### POST /api/v1/stays/{id}/checkout

Full room checkout (all guests).

**Request Body:**
```json
{
  "checkoutDateTime": "2026-05-18T10:00:00Z",
  "workerId": 3,
  "markUnpaid": false,
  "forceCheckout": false
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-CO-001 | Transaction: close nights → checkout guests → close advances → close folio → close expenses | — | `Data.vb:142-208`, `14_PAYMENT.md:5.12` |
| BR-CO-002 | All active nights (PrijavaOdjava=0 → 1) with checkout timestamp | — | `Data.vb:172` |
| BR-CO-003 | All guests in room (odjavljen=0 → 1) with checkOutDate/checkOutRadnik | — | `Data.vb:191` |
| BR-CO-004 | All advances marked paid (Avans.placeno=1) | — | `Data.vb:196` |
| BR-CO-005 | Folio closed: zakljucen=1, vrijemeO=now | — | `Data.vb:203` |
| BR-CO-006 | All open expenses closed (zaklj=1) | — | `Data.vb:208` |
| BR-CO-007 | Room marked dirty (clean=0) — OUTSIDE transaction | — | `Data.vb:126` |
| BR-CO-008 | If unpaid and markUnpaid=true: create neplaceni records | — | `16_INVOICE_CHECKOUT.md:1.2` |
| BR-CO-009 | Night count: min 1, checkout ≥12:00 adds day, check-in <08:00 subtracts | — | `14_PAYMENT.md:5.12`, `16_INVOICE_CHECKOUT.md:5.5` |

**Response 200:**
```json
{
  "roomId": 101,
  "roomName": "101",
  "folios": [
    {
      "folioId": 201,
      "closed": true,
      "closeDate": "2026-05-18T10:00:00Z"
    }
  ],
  "checkedOutGuests": [
    {
      "stayId": 1501,
      "guestId": 55,
      "guestName": "Marko Marković",
      "checkInDate": "2026-05-15T14:30:00Z",
      "checkOutDate": "2026-05-18T10:00:00Z",
      "nights": 3
    },
    {
      "stayId": 1502,
      "guestId": 56,
      "guestName": "Ana Petrović",
      "checkInDate": "2026-05-15T14:30:00Z",
      "checkOutDate": "2026-05-18T10:00:00Z",
      "nights": 3
    }
  ],
  "roomMarkedDirty": true,
  "unpaidBalance": 0,
  "unpaidRecords": []
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 404 | STAY_NOT_FOUND | Stay ID not found | — |
| 409 | CHECKOUT_ALREADY_DONE | Stay already checked out (odjavljen=1) | `14_PAYMENT.md:4.6` |
| 409 | CHECKOUT_UNPAID_BALANCE | Room has unpaid balance — use forceCheckout=true or markUnpaid=true | `16_INVOICE_CHECKOUT.md:1.2` |
| 500 | CHECKOUT_PARTIAL_FAILURE | Transaction failed mid-way — data rolled back | `16_INVOICE_CHECKOUT.md:7.3.4` |

**Legacy Evidence:** `Data.vb:142-208` (OdjavaSobe transaction), `Data.vb:126` (PrljavaSoba)

---

### POST /api/v1/stays/{id}/partial-checkout

Partial checkout — check out a single guest from a multi-occupancy room.

**Request Body:**
```json
{
  "guestId": 56,
  "checkoutDateTime": "2026-05-18T10:00:00Z",
  "workerId": 3,
  "splitPrice": 27.50
}
```

**Business Rules:**

| Rule | Description | Error Code | Legacy Evidence |
|------|-------------|-----------|-----------------|
| BR-PCO-001 | Cannot checkout last guest in room ("room can't be empty") | CHECKOUT_LAST_GUEST | `16_INVOICE_CHECKOUT.md:5.4` |
| BR-PCO-002 | Checked-out guest's nocenja set PrijavaOdjava=1 | — | `Data.vb:175` |
| BR-PCO-003 | Remaining guests get new nocenja rows with PrijavaOdjava=0 and split tariff | — | `Data.vb:178` |
| BR-PCO-004 | Only that guest's relgostsoba set odjavljen=1 | — | `Data.vb:193` |
| BR-PCO-005 | Folio remains open (zakljucen stays 0) — only closed on full checkout | — | `Data.vb:203` |
| BR-PCO-006 | Room NOT marked dirty on partial checkout | — | `16_INVOICE_CHECKOUT.md:5.4` |

**Response 200:**
```json
{
  "checkedOutGuest": {
    "stayId": 1502,
    "guestId": 56,
    "guestName": "Ana Petrović",
    "checkInDate": "2026-05-15T14:30:00Z",
    "checkOutDate": "2026-05-18T10:00:00Z",
    "nights": 3
  },
  "remainingGuests": [
    {
      "stayId": 1501,
      "guestId": 55,
      "guestName": "Marko Marković",
      "remainingNights": 3,
      "newNightCharges": [
        {
          "id": 4010,
          "date": "2026-05-18",
          "tariff": 27.50,
          "active": true
        }
      ]
    }
  ],
  "folio": {
    "folioId": 201,
    "closed": false
  },
  "roomMarkedDirty": false
}
```

**Error Codes:**

| Status | Code | Description | Legacy Evidence |
|--------|------|-------------|-----------------|
| 400 | CHECKOUT_LAST_GUEST | Cannot checkout last guest in room | `16_INVOICE_CHECKOUT.md:5.4` |
| 404 | STAY_NOT_FOUND | Stay ID not found | — |
| 409 | CHECKOUT_ALREADY_DONE | Stay already checked out | — |

**Legacy Evidence:** `16_INVOICE_CHECKOUT.md:5.4` (partial guest checkout), `Data.vb:175-193` (single guest checkout)

---

## 9. Error Codes

Complete list of error codes derived from legacy MsgBox validations and business rules.

### 9.1 Room Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| ROOM_NOT_FOUND | 404 | Room ID does not exist | — |
| ROOM_STATUS_OOO_INVALID | 400 | Invalid OOO toggle value | `12_ROOM_STATUS.md:5.3` |
| ROOM_OOO_REASON_REQUIRED | 400 | OOO reason required when setting OOO | `frmSobaInfo.vb:180` |
| ROOM_OOO_OCCUPIED_CONFIRM | 400 | Room is occupied; confirm OOO action | `12_ROOM_STATUS.md:5.3` |
| ROOM_CLEAN_OOO_CONFLICT | 400 | Room is OOO; setting clean will also unset OOO | `frmSobaInfo.vb:2172` |
| ROOM_CLEAN_INVALID | 400 | Invalid clean status value | `12_ROOM_STATUS.md:3.1` |
| ROOM_CLEAN_CONFLICT | 409 | Concurrent room status change detected | — |
| ROOM_STATUS_CONFLICT | 409 | Concurrent status change | — |

### 9.2 Guest Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| GUEST_NOT_FOUND | 404 | Guest ID does not exist | — |
| GUEST_LASTNAME_REQUIRED | 400 | Last name is required | `frmPrijavaGostiUnos.vb:~810` |
| GUEST_DOB_INVALID | 400 | Invalid date of birth | `10_CHECKIN.md:5.9` |
| GUEST_CITIZENSHIP_INVALID | 400 | Citizenship ID not found in drzave | `10_CHECKIN.md:1.5` |
| GUEST_DOCUMENT_TYPE_INVALID | 400 | Document type ID not found | `10_CHECKIN.md:1.5` |
| GUEST_ALREADY_REGISTERED | 409 | Guest already has tourist registration | `20_GUESTS.md:2125` |
| GUEST_ESTRANEC_RACE | 409 | Concurrent estranac number generation | `20_GUESTS.md:156` |

### 9.3 Check-in Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| CHECKIN_ROOM_REQUIRED | 400 | Room type and room must be selected | `frmPrijava1.vb:366,372` |
| CHECKIN_DATE_INVALID | 400 | Invalid check-in date | `frmPrijava1.vb:366` |
| CHECKIN_DATE_ORDER_INVALID | 400 | checkOutDate must be after checkInDate | `frmPrijava1.vb:379` |
| CHECKIN_DATE_TOO_OLD | 400 | checkInDate > 20 days before last charge | `frmPrijava1.vb:376` |
| CHECKIN_GUESTS_REQUIRED | 400 | At least one guest required | `frmPrijava1.vb:382` |
| CHECKIN_NEW_GUEST_REQUIRED | 400 | At least one new guest required | `frmPrijava1.vb:385` |
| CHECKIN_DUPLICATE_GUEST | 400 | Same guest cannot be in room twice | `frmPrijava1.vb:224` |
| CHECKIN_ROOM_OCCUPIED | 409 | Room already occupied | `30_STATUS_MATRIX.md:1.1` |
| CHECKIN_ROOM_OOO | 409 | Room is out of order | `10_CHECKIN.md:4.12` |
| CHECKIN_ROOM_NOT_AVAILABLE | 409 | Room not free for check-in | `12_ROOM_STATUS.md:1.1` |
| CHECKIN_FOLIO_RACE | 409 | Concurrent folio creation detected | `10_CHECKIN.md:8.2.3` |
| CHECKIN_NEW_GUEST_ONLY | 400 | Only new guests can be removed before check-in | `frmPrijava1.vb:345` |

### 9.4 Reservation Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| RESV_NOT_FOUND | 404 | Reservation ID does not exist | — |
| RESV_DATE_REQUIRED | 400 | Check-in and check-out dates required | `13_RESERVATIONS.md:5.6` |
| RESV_DATE_ORDER_INVALID | 400 | Check-out must be after check-in | `13_RESERVATIONS.md:5.6` |
| RESV_GUEST_REQUIRED | 400 | Guest ID is required | `frmRezervacije_unos.vb:298` |
| RESV_ROOM_TYPE_REQUIRED | 400 | Room type must be selected | `frmRezervacije_unos.vb:292` |
| RESV_ROOMS_REQUIRED | 400 | At least one room assignment required | `13_RESERVATIONS.md:5.4` |
| RESV_ROOM_NOT_AVAILABLE | 400 | Room already booked for dates | `13_RESERVATIONS.md:7.1.10` |
| RESV_ALREADY_CONFIRMED | 409 | Reservation already confirmed | `13_RESERVATIONS.md:4.3` |
| RESV_ALREADY_CANCELLED | 409 | Reservation already storned | `13_RESERVATIONS.md:4.2` |
| RESV_ALREADY_CHECKED_IN | 409 | Reservation has prijava=1 | `13_RESERVATIONS.md:4.1` |
| RESV_CONFIRMATION_RACE | 409 | Concurrent confirmation number | `13_RESERVATIONS.md:4.4` |
| RESV_BED_CAPACITY_EXCEEDED | 400 | Guest count exceeds room capacity | `13_RESERVATIONS.md:5.5` |

### 9.5 Room Transfer Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| XFER_STAY_NOT_FOUND | 404 | Stay ID not found | — |
| XFER_TARGET_ROOM_NOT_FOUND | 404 | Target room doesn't exist | `frmSobaInfo.vb:1328` |
| XFER_LAST_GUEST | 409 | Cannot transfer last guest from room | `16_INVOICE_CHECKOUT.md:5.4` |
| XFER_TARGET_OOO | 409 | Target room is OOO | — |

### 9.6 Expense Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| EXPENSE_NOT_FOUND | 404 | Expense ID not found | — |
| EXPENSE_TYPE_INVALID | 400 | Cannot add accommodation (TID=1) directly | `frmTroskovi.vb:87` |
| EXPENSE_AMOUNT_INVALID | 400 | Amount must be > -200 | `frmTroskovi.vb:404` |
| EXPENSE_PROTECTED_NAME | 400 | Name starts with "osigu" or "bora" | `frmTroskovi.vb:357-360` |
| EXPENSE_KASA_REQUIRED | 400 | Mini-bar expenses require KASA reference | `frmTroskovi.vb:332-346` |
| EXPENSE_STAY_REQUIRED | 400 | Stay (relgostsoba) ID required | `15_EXPENSES_NIGHTS.md:1.1` |
| EXPENSE_ROOM_REQUIRED | 400 | Room ID required | `15_EXPENSES_NIGHTS.md:1.1` |
| EXPENSE_QUANTITY_INVALID | 400 | Quantity must be > 0 | — |
| EXPENSE_ALREADY_CLOSED | 409 | Expense is closed (zaklj=1) | `15_EXPENSES_NIGHTS.md:4.1` |

### 9.7 Payment Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| PAY_AMOUNT_INVALID | 400 | Payment amount must be positive | — |
| PAY_FOLIO_NOT_FOUND | 400 | Folio ID not found | — |
| PAY_FOLIO_CLOSED | 409 | Folio is already closed | `14_PAYMENT.md:4.5` |
| PAY_METHOD_INVALID | 400 | Payment method 5 (compound) not allowed directly | `14_PAYMENT.md:1.3` |
| PAY_RECEIPT_DUPLICATE | 409 | Receipt number already exists | `14_PAYMENT.md:5.1` |
| PAY_SPLIT_AMOUNT_MISMATCH | 400 | Split payment amounts don't total | `frmPlacanjeSlozeno.vb:205` |
| PAY_SPLIT_REMAINING | 400 | Split payment remaining must reach 0 | `frmPlacanjeSlozeno.vb:225` |
| PAY_NOT_FOUND | 404 | Payment ID not found | — |
| PAY_ALREADY_STORNO | 409 | Payment already storned | `14_PAYMENT.md:4.3` |

### 9.8 Invoice Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| INVOICE_NOT_FOUND | 404 | Invoice ID not found | — |
| INVOICE_ALREADY_STORNO | 409 | Invoice already storned | `16_INVOICE_CHECKOUT.md:4.1` |
| FISCAL_DEVICE_NOT_CONFIGURED | 400 | No fiscal device configured | `17_FISCAL_PROFORMA.md:1.1` |
| FISCAL_DEVICE_UNAVAILABLE | 503 | Fiscal device not responding | `14_PAYMENT.md:1.1` |
| FISCAL_DEVICE_ERROR | 500 | Fiscal device returned error | `17_FISCAL_PROFORMA.md:1.2` |

### 9.9 Checkout Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| CHECKOUT_NOT_FOUND | 404 | Stay ID not found | — |
| CHECKOUT_ALREADY_DONE | 409 | Stay already checked out | `14_PAYMENT.md:4.6` |
| CHECKOUT_UNPAID_BALANCE | 409 | Room has unpaid balance | `16_INVOICE_CHECKOUT.md:1.2` |
| CHECKOUT_LAST_GUEST | 400 | Cannot checkout last guest in room | `16_INVOICE_CHECKOUT.md:5.4` |
| CHECKOUT_PARTIAL_FAILURE | 500 | Transaction failed mid-way | `16_INVOICE_CHECKOUT.md:7.3.4` |

### 9.10 Night Charge Errors

| Code | HTTP | Description | Legacy Source |
|------|------|-------------|---------------|
| NIGHT_NOT_FOUND | 404 | Night charge ID not found | — |
| NIGHT_DATE_INVALID | 400 | Invalid date format | — |
| NIGHT_STAY_NOT_FOUND | 404 | Stay ID for night charge not found | — |

### 9.11 General Errors

| Code | HTTP | Description |
|------|------|-------------|
| UNAUTHORIZED | 401 | Invalid or expired JWT token |
| FORBIDDEN | 403 | Insufficient role permissions |
| VALIDATION_ERROR | 400 | Request body validation failed |
| CONCURRENCY_CONFLICT | 409 | Optimistic concurrency conflict — resource modified by another user |
| INTERNAL_ERROR | 500 | Unexpected server error |
| SERVICE_UNAVAILABLE | 503 | Database or fiscal device unavailable |

---

## Appendix A: Legacy-to-API Mapping Reference

| Legacy Form | API Endpoint(s) | Primary Legacy Evidence |
|-------------|----------------|----------------------|
| frmPrijava1 | POST /api/v1/stays, POST /api/v1/guests | `10_CHECKIN.md:1.8` |
| frmSobe, frmSobaInfo | GET/PUT /api/v1/rooms/*, POST /api/v1/stays/{id}/room-transfer | `12_ROOM_STATUS.md:1.1-1.5` |
| frmRezervacije*, frmRezervacijePrebaci | CRUD /api/v1/reservations/*, POST transfer-to-checkin | `13_RESERVATIONS.md:1.1-1.4` |
| frmPlacanje, frmPlati1, frmPlacanjeSlozeno | POST /api/v1/payments/* | `14_PAYMENT.md:1.1-1.4` |
| frmTroskovi, frmTroskoviNoc | POST/PUT/DELETE /api/v1/expenses/*, GET/POST/PUT /api/v1/nights | `15_EXPENSES_NIGHTS.md:1.1-1.2` |
| frmRacuni | POST /api/v1/invoices/*, POST storno, POST fiscalize | `16_INVOICE_CHECKOUT.md:1.1,4.3` |
| frmOdjava1 | POST /api/v1/stays/{id}/checkout, POST partial-checkout | `16_INVOICE_CHECKOUT.md:1.2,5.4` |
| Data.vb (OdjavaSobe, PrljavaSoba) | (internal service calls) | `Data.vb:142-208` |
| ModuleKod.vb (Unesinocenja, addTroskovi, etc.) | (internal service calls) | `ModuleKod.vb:1081,903` |

---

## Appendix B: Status Code Mapping (Legacy → API)

| Legacy Value | Legacy Table.Field | API Representation | Legacy Evidence |
|-------------|-------------------|-------------------|-----------------|
| 0 | fnSobaStatus | `status.code: 0, label: "FREE"` | `12_ROOM_STATUS.md:1.1` |
| 1 | fnSobaStatus | `status.code: 1, label: "OCCUPIED"` | `12_ROOM_STATUS.md:1.1` |
| 2 | fnSobaStatus | `status.code: 2, label: "DEPARTING"` | `12_ROOM_STATUS.md:1.1` |
| 3 | fnSobaStatus | `status.code: 3, label: "RESERVED_CONFIRMED"` | `12_ROOM_STATUS.md:1.1` |
| 4 | fnSobaStatus | `status.code: 4, label: "OCCUPIED_RESERVED"` | `12_ROOM_STATUS.md:1.1` |
| 5 | sobe.ooo | `status.code: 5, label: "OUT_OF_ORDER"` | `12_ROOM_STATUS.md:1.1` |
| 6 | fnSobaStatus | `status.code: 6, label: "RESERVED_UNCONFIRMED"` | `12_ROOM_STATUS.md:1.1` |
| 7 | sobe.clean (override) | `status.code: 7, label: "NOT_READY"`, `cleanOverride: true` | `frmSobe.vb:280-284` |
| 0 | relgostsoba.odjavljen | `checkedOut: false` | `30_STATUS_MATRIX.md:2.1` |
| 1 | relgostsoba.odjavljen | `checkedOut: true` | `30_STATUS_MATRIX.md:2.1` |
| 1 | relgostsoba.status | `guestStatus: "ADULT"` | `10_CHECKIN.md:4.5` |
| 3 | relgostsoba.status | `guestStatus: "MINOR"` | `10_CHECKIN.md:4.5` |
| 4 | relgostsoba.status | `guestStatus: "CHILD"` | `10_CHECKIN.md:4.5` |
| 0 | rezervacije.prijava | `checkedIn: false` | `30_STATUS_MATRIX.md:3.1` |
| 1 | rezervacije.prijava | `checkedIn: true` | `30_STATUS_MATRIX.md:3.1` |
| 2 | rezervacije.prijava | `expired: true` | `30_STATUS_MATRIX.md:3.1` |
| '0'/'1' | rezervacije.stornirana | `cancelled: false/true` | `30_STATUS_MATRIX.md:3.2` |
| 0 | troskovi.zaklj | `closed: false` | `30_STATUS_MATRIX.md:6.1` |
| 1 | troskovi.zaklj | `closed: true` | `30_STATUS_MATRIX.md:6.1` |
| 0 | nocenja.PrijavaOdjava | `active: true` | `30_STATUS_MATRIX.md:5.1` |
| 1 | nocenja.PrijavaOdjava | `active: false` | `30_STATUS_MATRIX.md:5.1` |
| 0 | posjetafolio.zakljucen | `open: true` | `30_STATUS_MATRIX.md:4.1` |
| 1 | posjetafolio.zakljucen | `open: false` | `30_STATUS_MATRIX.md:4.1` |
| 0 | placanje.storno | `storno: false` | `30_STATUS_MATRIX.md:7.1` |
| 1 | placanje.storno | `storno: true` | `30_STATUS_MATRIX.md:7.1` |
| 0 | printracuni.storno | `storno: false` | `30_STATUS_MATRIX.md:8.1` |
| 1 | printracuni.storno | `storno: true` | `30_STATUS_MATRIX.md:8.1` |