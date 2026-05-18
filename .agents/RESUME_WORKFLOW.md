# RESUME_WORKFLOW — Self-Contained Context for AI Agents

**Version:** 1.0
**Last updated:** 2026-05-18
**Purpose:** Minimum context needed to resume work without full re-analysis

---

## HOW TO USE THIS DOCUMENT

When starting a new session:
1. Read THIS document first
2. Read `P0_REBUILD_PLAN.md` for current task details
3. Read `STATUS_REALISTIC.md` for module statuses
4. Then read ONLY the specific files referenced by your current task
5. Do NOT re-read `.agents/06_audit/` or `.agents/07_legacy_reconstruction/` unless you need a specific line reference

---

## PROJECT STATE SUMMARY

### What This Project Is

Hotel PMS (Property Management System) migrating from VB.NET/MySQL to .NET 8/Next.js/PostgreSQL.
Legacy app is operational. New app has a solid technical foundation but incomplete business logic.

### What's Built and Working (KEEP)

| Component | Status | Do NOT Touch |
|-----------|--------|-------------|
| Frontend layout/sidebar/navbar | Good | Keep as-is |
| Login/JWT auth flow | Working | Keep |
| Dashboard KPI cards + Chart.js | Working | Keep |
| Room grid/floor plan + SignalR | Working | Keep, adapt API |
| Gantt calendar drag&drop | Working | Keep, adapt API |
| Design system (colors, typography) | Good | Keep |
| Help system (tooltips, tours) | Good | Keep |
| Docker Compose + PostgreSQL | Working | Keep |
| EF Core migrations (1 initial) | Working | Keep, add new migrations |
| Audit log interceptor | Working | Keep |
| Multi-tenant middleware | Working | Keep |
| Feature flags service | Working | Keep |
| API versioning (v1/v2) | Working | Keep |
| Background jobs (8 registered) | Working | Keep, improve logic |
| Email service (MailKit) | Working | Keep |
| 57 unit tests | Passing | Keep, expand |

### What Needs Rebuild (P0 CORE — Business Logic)

These services have CRUD shells but WRONG business logic for a hotel PMS:

| Service | Problem | Rebuild From |
|---------|---------|-------------|
| `RoomService` | Missing 7-status `fnSobaStatus` logic, clean override | `07/LEGACY_ANALYSIS/12_ROOM_STATUS.md` + `30_STATUS_MATRIX.md` |
| `CheckInService` | Missing multi-guest, `relgostsoba` semantics, PID/folio creation | `07/LEGACY_ANALYSIS/10_CHECKIN.md` + `40_GOLDEN_SCENARIOS.md` scenario 1 |
| `CheckOutService` | Single booking, hardcoded discounts, no partial checkout | `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT.md` + `40_GOLDEN_SCENARIOS.md` scenario 6 |
| `FolioService` | Uses `Balance` field, not ledger aggregation | `07/LEGACY_ANALYSIS/14_PAYMENT.md` + `15_EXPENSES_NIGHTS.md` |
| `InvoiceGenerator` | Hardcoded PDV 25%, `FolioId=Guid.Empty`, no storno reversal | `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT_DEEP.md` + `40_GOLDEN_SCENARIOS.md` scenario 7 |
| `NightAuditService` | Generates nights from Booking dates, not materialized ledger | `07/LEGACY_ANALYSIS/15_EXPENSES_NIGHTS.md` |
| `BookingService` | Missing confirmation/cancellation audit trail, header/detail | `07/LEGACY_ANALYSIS/13_RESERVATIONS.md` |
| `PaymentService` | Single Payment, no split payment, no advance handling | `07/LEGACY_ANALYSIS/14_PAYMENT.md` |

### What's MOCK (Keep, Mark Clearly)

| Controller | Mock Behavior | Real Implementation |
|------------|---------------|---------------------|
| `ChannelManagerController` | Returns `status: "mock"` | Needs Booking.com/Airbnb API keys → Admin panel config |
| `IoTController` | Returns `mqttBroker: "disconnected"` | Needs Mosquitto broker + devices → Optional module |
| `PaymentGatewayController` | `Guid.NewGuid` as token | Needs Stripe API keys → Admin panel config |
| `GuestPortalController` | Static demo data | Needs guest-scoped auth flow |
| `BridgeService` | `HardwareMode: "Mock"`, fake JIR/RFID/PABX | Needs real hardware or vendor SDK → Admin panel config |
| `RevenueController` | Static pricing suggestions | Needs rate plan engine → Post-P0 |
| `AdminController.SecurityAudit` | `owaspStatus: "not_scanned"` | Needs actual scan |

### What's EMPTY

| Directory | Status |
|-----------|--------|
| `hardware_bridge/` | Empty, no code |
| `iot_services/` | Empty, no code |

---

## NEW ENTITIES AND SERVICES NEEDED

### New Domain Services (replace or extend existing)

| Service | Purpose | Source Spec |
|---------|---------|-------------|
| `RoomOccupancyPolicy` | 7-room status from stays+reservations+OOO+clean | `30_STATUS_MATRIX.md` section 1 |
| `StayLifecycleService` | Check-in with multi-guest, PID, folio | `40_GOLDEN_SCENARIOS.md` scenario 1 |
| `NightLedgerService` | Materialized nights, idempotent per stay/date | `40_GOLDEN_SCENARIOS.md` scenario 5 |
| `CheckOutWorkflowService` | Full + partial checkout, close folio+expenses | `40_GOLDEN_SCENARIOS.md` scenario 6 |
| `FolioLedgerService` | Nights + expenses + payments ledger aggregation | `15_EXPENSES_NIGHTS.md` + `14_PAYMENT.md` |
| `InvoiceWorkflowService` | Snapshot, storno reversal, PDV rules, fiscal | `40_GOLDEN_SCENARIOS.md` scenario 7 |
| `PaymentAllocationService` | Split payment, advance, proforma | `14_PAYMENT.md` section 4.4 |
| `ReservationPolicyService` | Confirmation, cancellation, auto-expire | `13_RESERVATIONS.md` + `30_STATUS_MATRIX.md` section 3 |
| `ConfigurationService` | Admin panel config (API keys, settings, feature flags) | New — replace hardcoded secrets |

### New/Extended Entity Fields

Based on `50_DOMAIN_MODEL_MAPPING.md` and `30_STATUS_MATRIX.md`:

```
Room: add computed status (7 statuses + clean override)
Stay (new entity, replaces simple BookingRoom): add PID, RID, checkInRadnik, checkOutRadnik,
      odjavljen, stampanaPrijava, rezervP, status(guest category), taksa, popust, pl
Folio: add -> Stay (1:N), isClosed (zakljucen), closedAt
NightCharge (new entity): RID, date, tariff, SID, PID, isClosed, discount, description
Expense: add isLocked (zaklj), receiptNumber (Brrac), expenseType (TID), isPartial (Djelimicno)
Payment: add -> PaymentDetail (1:N), PaymentSplit, advance flag, storno flag
Invoice: add -> InvoiceDetail (1:N), InvoiceFooter, advance invoice, storno workflow, fiscal fields
Reservation: add potvrda, stornirana, brojPotvrde, brojStorna, confirmationNumber, cancellationNumber
HotelConfig (new): VAT rate, reduced VAT, tourist tax, billing mode, check-in/out hours, currency
```

---

## CONFIGURATION / ADMIN PANEL APPROACH

All API keys, feature flags, and hotel settings go into `HotelConfig` entity + admin API.

### Principle: If key is set, feature works. If not, feature is skipped or shows config prompt.

```
Admin Panel -> Settings:
  - Stripe: ApiKey, WebhookSecret, TestMode
  - Booking.com: ApiKey, HotelCode, WebhookUrl
  - Airbnb: ClientId, ClientSecret
  - Fiscal: DriverType (Mock/Tring/HCP/NSC), SerialPort, BaudRate
  - RFID: DriverType (Mock/LuxM), IpAddress, Port
  - PABX: DriverType (Mock/IP-PBX), IpAddress, Port
  - MQTT: BrokerUrl, Username, Password, Enabled (bool)
  - SMTP: Host, Port, Username, Password, FromAddress, UseTls
  - TZ: ApiUrl, Username, Password, Enabled (bool)

Each integration:
  [Switch: Enabled/Disabled]
  [Config fields when enabled]
  [Test Connection button]
  [Status indicator: Connected/Disconnected/Not Configured]
```

Implementation:
- `HotelConfig` entity with JSONB columns for nested settings
- `IConfigurationProvider` interface
- Each integration checks `IsEnabled` before doing anything
- Mock responses return `{ status: "not_configured", message: "Configure in Settings" }`

---

## KEY BUSINESS RULES (Quick Reference)

From `40_GOLDEN_SCENARIOS.md` and `30_STATUS_MATRIX.md`:

### Room Status (7 statuses + override)

| Value | Name | Condition |
|-------|------|-----------|
| 0 | Free | No guests, no reservations |
| 1 | Occupied | Active guests, `odjavljen=0`, `rezervacija=0` |
| 2 | Departing | Guests with `checkOutDate <= today` |
| 3 | Reserved Confirmed | Confirmed reservation, no guests |
| 4 | Occupied+Reserved | Confirmed reservation AND active guests |
| 5 | Out of Order | `ooo=1` |
| 6 | Reserved Unconfirmed | Unconfirmed reservation, no guests |
| Override | Dirty | `clean=0` overrides ALL to display as "not ready" |

### Check-in Creates

- `relgostsoba` record per guest (Stay)
- `posjetafolio` if not exists (Folio)
- `nocenja` materialized night charge (NightCharge)
- Room `clean=1`

### Checkout Closes (full room)

- `nocenja.PrijavaOdjava → 1` (close nights)
- `relgostsoba.odjavljen → 1` (guest left)
- `posjetafolio.zakljucen → 1` (close folio)
- `troskovi.zaklj → 1` (lock expenses)
- `sobe.clean → 0` (mark dirty)
- If unpaid: create `neplaceni` records

### Partial Checkout (single guest)

- Close ONLY that guest's nights and stay
- INSERT new night records for remaining guests (split)
- Folio stays OPEN until last guest leaves

### Storno Reversal

- `placanje.storno → 1`
- `placanjedetalji.storno → 1`
- `printracuni.storno → 1, exp → 2`
- `troskovi.zaklj → 0, Brrac → null` (reopen non-accommodation)
- `DELETE FROM troskovi WHERE TID=1` (remove accommodation charges)
- Fiscal storno if device present

### Split Payment

- One payment can have multiple methods
- Each method gets its own `placanjedetalji` record
- Sum must equal total invoice

---

## CURRENT TASK SEQUENCE

See `P0_REBUILD_PLAN.md` for detailed task definitions.

Current position: **START — Pre-work (Faza A)**

1. [ ] A.1: Create `STATUS_REALISTIC.md` with corrected module statuses
2. [ ] A.2: Add `[Mock]` attribute to mock controllers + feature flag check
3. [ ] A.3: Create `HotelConfig` entity + admin settings API
4. [ ] B.1: `RoomOccupancyPolicy` — 7 statuses + clean override
5. [ ] B.2: `StayLifecycleService` — check-in with multi-guest
6. [ ] B.3: `NightLedgerService` — materialized nights
7. [ ] B.4: `CheckOutWorkflowService` — full + partial checkout
8. [ ] B.5: `FolioLedgerService` — ledger aggregation
9. [ ] B.6: `InvoiceWorkflowService` — snapshot + storno
10. [ ] B.7: `PaymentAllocationService` — split payment
11. [ ] B.8: `ReservationPolicyService` — confirmation/cancellation
12. [ ] C: EF Core migrations for new entities/fields
13. [ ] D: Frontend API adaptation

---

## FILE REFERENCE INDEX

When working on a specific task, read ONLY these files:

| Task | Read These |
|------|-----------|
| Room status | `07/LEGACY_ANALYSIS/12_ROOM_STATUS.md`, `30_STATUS_MATRIX.md` sections 1-2 |
| Check-in | `07/LEGACY_ANALYSIS/10_CHECKIN.md`, `40_GOLDEN_SCENARIOS.md` scenario 1 |
| Checkout | `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT.md`, `40_GOLDEN_SCENARIOS.md` scenario 6 |
| Nights | `07/LEGACY_ANALYSIS/15_EXPENSES_NIGHTS.md`, `40_GOLDEN_SCENARIOS.md` scenario 5 |
| Payment | `07/LEGACY_ANALYSIS/14_PAYMENT.md`, `40_GOLDEN_SCENARIOS.md` scenario 4 |
| Invoice/Storno | `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT_DEEP.md`, `40_GOLDEN_SCENARIOS.md` scenario 7 |
| Reservation | `07/LEGACY_ANALYSIS/13_RESERVATIONS.md`, `30_STATUS_MATRIX.md` section 3 |
| Folio | `07/LEGACY_ANALYSIS/14_PAYMENT.md` lines on `posjetafolio` |
| Domain mapping | `07/LEGACY_ANALYSIS/50_DOMAIN_MODEL_MAPPING.md` |
| Status codes | `07/LEGACY_ANALYSIS/30_STATUS_MATRIX.md` |
| Entity fields | `07/LEGACY_ANALYSIS/00_DATABASE_SCHEMA.md` |
| Stored procedures | `07/LEGACY_ANALYSIS/03_STORED_PROCEDURES.md` |
| Settings/Config | `07/LEGACY_ANALYSIS/25_SETTINGS_CONFIGURATION.md` |
| Cross-flow deps | `07/LEGACY_ANALYSIS/35_CROSS_FLOW_DEPENDENCIES.md` |
| API spec | `07/LEGACY_ANALYSIS/60_API_SPECIFICATION.md` |
| Risk matrix | `06_audit/RISK_MATRIX.md` (for legacy security issues) |

---

## VERIFICATION COMMANDS

After every task:

```powershell
# Backend
dotnet build backend\HotelPro.sln
dotnet test backend\HotelPro.sln

# Frontend
cd frontend; npm run build; npm run lint

# Check for hardcoded secrets
rg -i "hardcoded|Hardcoded|secret|password|api.key" backend/src --type cs
```

---

## DO NOT RE-READ UNLESS NEEDED

These files were read during full analysis. Do NOT re-read for context:
- `.agents/06_audit/` — only reference specific line numbers if needed
- `.agents/07_legacy_reconstruction/LEGACY_ANALYSIS/` — use index above to read ONLY relevant file
- `.agents/03_specs/fsd/` — superseded by 07 analysis for business logic
- `.agents/04_tasks/` — superseded by P0_REBUILD_PLAN.md
- `legacy_app/` — NEVER read unless specifically referenced

---

## SESSION PROTOCOL

1. Read THIS file
2. Read `P0_REBUILD_PLAN.md` for current task
3. Pick next unfinished task from sequence
4. Claim task in `STATUS_REALISTIC.md` (change `[ ]` to `[-]`)
5. Read ONLY the reference files listed in INDEX above
6. Create checkpoint commit before edits
7. Implement
8. Run verification commands
9. Mark task `[x]` in `STATUS_REALISTIC.md`
10. Commit with descriptive message