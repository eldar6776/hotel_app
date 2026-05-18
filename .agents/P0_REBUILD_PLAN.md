# P0 REBUILD PLAN — Definitive Implementation Plan

**Version:** 1.0
**Last updated:** 2026-05-18
**Purpose:** Exact task sequence for P0 core business logic rebuild
**Predecessor:** `RESUME_WORKFLOW.md` (READ FIRST)

---

## PRINCIPLES

1. **Legacy business logic is source of truth** — `07/LEGACY_ANALYSIS/` documents are the spec
2. **Keep frontend and API surface** — rebuild backend services, adapt API responses
3. **No hardcoded secrets** — all keys and settings go to `HotelConfig`
4. **Mock integrations stay mock** — but clearly marked with `[Mock]` attribute and feature flag
5. **IoT/MQTT is optional module** — placeholder only, controlled by config
6. **Each task has verification** — build + test must pass before marking complete
7. **One task per commit** — descriptive commit messages

---

## FAZA A: PRE-WORK (No code changes to business logic yet)

### A.1: Create STATUS_REALISTIC.md

**What:** New file `.agents/STATUS_REALISTIC.md` with corrected statuses

**Changes:**
- Faze 1-3: ACCEPTED (infrastructure works)
- Faza 4: ACCEPTED (frontend foundation works)
- Faza 5: PARTIAL (rooms CRUD works, but status logic is wrong)
- Faza 6: PARTIAL (booking CRUD works, but policy is incomplete)
- Faza 7: PARTIAL (check-in/out exist but business logic is wrong)
- Faza 8: PARTIAL (guests CRUD works, but ETL/mapping incomplete)
- Faza 9: REBUILD NEEDED (invoice/folio/storno logic fundamentally wrong)
- Faza 10: PARTIAL (reports exist but not legacy-parity)
- Faza 11: IN_PROGRESS (T11.1 PARTIAL, T11.2-T11.4 PENDING)
- Faze 12: MOCK (hardware bridge is mock)
- Faze 13: MOCK (channel manager is mock)
- Faza 14: MOCK/MISSING (IoT is placeholder)
- Faza 15: MOCK (revenue is static)
- Faza 16: MOCK (guest portal is demo)
- Faza 17: MOCK (payment gateway is fake)
- Faza 18: NOT STARTED (audit/release needs real work)

**Verify:** File exists with correct statuses

---

### A.2: Add Mock Attribute + Feature Flag Check

**What:** Mark all mock controllers with `[Mock]` attribute and feature flag

**Files to change:**
- `backend/src/HotelPro.Api/Controllers/ChannelManagerController.cs`
- `backend/src/HotelPro.Api/Controllers/IoTController.cs`
- `backend/src/HotelPro.Api/Controllers/PaymentGatewayController.cs`
- `backend/src/HotelPro.Api/Controllers/GuestPortalController.cs`
- `backend/src/HotelPro.Api/Controllers/RevenueController.cs`
- `backend/src/HotelPro.Api/Controllers/AdminController.cs` (security/PCI endpoints)

**Implementation:**
1. Create `[Mock]` attribute in `HotelPro.Core/Attributes/MockAttribute.cs`
2. Add `[Mock]` attribute to each mock controller class
3. Add `[FeatureGate("ChannelManager")]`, `[FeatureGate("IoT")]`, etc.
4. When feature is disabled or key not configured: return `{ status: "not_configured", message: "Configure in Admin > Settings" }`
5. When feature is enabled but key missing: return `{ status: "missing_api_key", message: "Enter API key in Admin > Settings" }`

**Verify:** `dotnet build` passes, mock controllers return config messages

---

### A.3: Create HotelConfig Entity + Admin Settings API

**What:** Entity for all configurable settings, admin API to manage them

**Files to create:**
- `HotelPro.Core/Entities/HotelConfig.cs` — entity with JSONB settings
- `HotelPro.Core/DTOs/HotelConfigDto.cs` — DTO for settings
- `HotelPro.Infrastructure/Data/Configurations/HotelConfigConfiguration.cs`
- `HotelPro.Infrastructure/Services/ConfigurationService.cs` — cached settings provider
- `HotelPro.Api/Controllers/HotelConfigController.cs` — admin CRUD for settings

**Entity design:**
```csharp
public class HotelConfig : IHaveHotelId
{
    public Guid Id { get; set; }
    public Guid HotelId { get; set; }
    public string Key { get; set; }      // e.g. "Stripe_ApiKey", "Fiscal_DriverType"
    public string Value { get; set; }     // encrypted value
    public string Category { get; set; }  // "Payment", "Fiscal", "Channel", "IoT", "Email", "TZ"
    public string Description { get; set; }
    public bool IsSecret { get; set; }    // if true, mask in API responses
    public bool IsEnabled { get; set; }   // feature on/off switch
    public DateTime UpdatedAt { get; set; }
}
```

**Key categories:**
- `Payment`: Stripe_ApiKey, Stripe_WebhookSecret, Stripe_TestMode
- `Channel`: BookingCom_ApiKey, BookingCom_HotelCode, Airbnb_ClientId, Airbnb_ClientSecret
- `Fiscal`: DriverType (Mock/Tring/HCP/NSC), SerialPort, BaudRate
- `Hardware`: Rfid_DriverType (Mock/LuxM), Rfid_IpAddress, Rfid_Port
- `Pabx`: DriverType (Mock/IP-PBX), IpAddress, Port
- `IoT`: Mqtt_BrokerUrl, Mqtt_Username, Mqtt_Password, Mqtt_Enabled
- `Email`: Smtp_Host, Smtp_Port, Smtp_Username, Smtp_Password, Smtp_FromAddress, Smtp_UseTls
- `Tourism`: Tz_ApiUrl, Tz_Username, Tz_Password, Tz_Enabled
- `Hotel`: VatRate, ReducedVatRate, TouristTax, CheckInHour, CheckOutHour, CurrencyCode, BillingMode

**Seed data:** Create migration that inserts default config entries (all disabled/mock by default)

**API endpoints:**
- `GET /api/v2/admin/config` — all settings (secrets masked)
- `GET /api/v2/admin/config/{category}` — settings by category
- `PUT /api/v2/admin/config/{key}` — update setting
- `POST /api/v2/admin/config/{key}/test` — test connection (for APIs that support it)
- `GET /api/v2/admin/config/public` — public settings (currency, hours, feature flags)

**Verify:** `dotnet build` + `dotnet test` pass, new migration created

---

## FAZA B: P0 CORE REBUILD

### B.1: RoomOccupancyPolicy

**Task:** Replace `RoomStatusTransitions` with full `fnSobaStatus` logic

**Read:** `07/LEGACY_ANALYSIS/12_ROOM_STATUS.md`, `30_STATUS_MATRIX.md` sections 1-2

**Implementation:**

1. Create `HotelPro.Core/Services/RoomOccupancyPolicy.cs`:
   - `GetRoomStatusAsync(roomId, date)` → computes status from stays + reservations + OOO + clean
   - 7 statuses: Free, Occupied, Departing, ReservedConfirmed, OccupiedReserved, OutOfOrder, ReservedUnconfirmed
   - Clean override: if `clean=false`, status becomes Dirty regardless of computed status
   - `GetRoomStatusForAllRoomsAsync(date)` → batch computation

2. Update `RoomStatusTransitions` to use `RoomOccupancyPolicy` as source of truth
   - Remove hardcoded state machine
   - Add transition validation (can only go from X to Y)

3. Update `RoomsController` to use computed status
   - `GET /api/v2/rooms` returns computed status per room
   - `GET /api/v2/rooms/{id}/status` returns detailed status with reason

4. Add SignalR broadcast for status changes (already exists, adapt payload)

5. Update frontend `RoomCard` component to display all 8 statuses (7 + Dirty)

6. Write tests:
   - Room with no guests/reservations → Free
   - Room with active stay → Occupied
   - Room with checkout today → Departing
   - Room with confirmed reservation no guests → ReservedConfirmed
   - Room with confirmed reservation + guests → OccupiedReserved
   - Room OOO → OutOfOrder (regardless)
   - Room dirty → Dirty (overrides Occupied/Reserved)
   - Room OOO + dirty → OutOfOrder (OOO takes precedence over Dirty)

**Verify:** `dotnet build` + `dotnet test` pass, all 8 status tests green

---

### B.2: StayLifecycleService (Check-in)

**Task:** Replace `CheckInService` with full legacy check-in workflow

**Read:** `07/LEGACY_ANALYSIS/10_CHECKIN.md`, `40_GOLDEN_SCENARIOS.md` scenario 1

**Implementation:**

1. Create `HotelPro.Core/Entities/Stay.cs` (replaces reliance on simple BookingRoom):
   - Map from `relgostsoba` fields: GuestId, RoomId, CheckInDate, CheckOutDate, CheckedInBy, CheckedOutBy, IsCheckedOut, IsReservation, IsFromConfirmedReservation, GroupId, TariffId, DiscountPercent, GuestCategoryStatus, TaxOverride, PaymentFlag, FolioId

2. Create `HotelPro.Core/Services/IStayLifecycleService.cs` + implementation:
   - `CheckInAsync(CheckInCommand)` — Golden Scenario 1.1:
     - Validate room is Free or ReservedConfirmed
     - Get or create Folio (`provjeriPID` logic)
     - Create Stay record per guest (`addRelGostSoba` logic)
     - Create NightCharge records (`Unesinocenja` logic)
     - Set room clean=1
     - If from reservation: mark reservation prijava=1, clear Rid
     - Return transactional result (all-or-nothing)

3. Create `HotelPro.Core/Entities/NightCharge.cs`:
   - Map from `nocenja`: StayId (RID), Date, Tariff, RoomId (SID), FolioId (PID), IsClosed (PrijavaOdjava), Discount, Description, RoomName

4. EF Core migration for Stay and NightCharge entities

5. Write tests:
   - Check-in free room → 1 guest → Stay created, Folio created, NightCharge created, Room → Occupied
   - Check-in reserved room → Stay with IsFromConfirmedReservation=true
   - Check-in multiple guests → same Folio, separate Stay records
   - Check-in with discount → discount applied
   - Check-in room already occupied → error
   - Check-in room OOO → error
   - Tariff calculation per `setings.naplposo` billing mode

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.3: NightLedgerService

**Task:** Implement materialized night charges (not derived from Booking dates)

**Read:** `07/LEGACY_ANALYSIS/15_EXPENSES_NIGHTS.md`, `40_GOLDEN_SCENARIOS.md` scenario 5

**Implementation:**

1. Create `HotelPro.Core/Services/INightLedgerService.cs` + implementation:
   - `GenerateNightChargesAsync(stayId, dateRange)` — equivalent of `Unesinocenja`
   - `ModifyNightChargeAsync(nightChargeId, command)` — adjust tariff/discount
   - `CloseNightChargesAsync(stayId, checkoutDate)` — set `IsClosed=true`
   - `GetNightChargesForFolioAsync(folioId)` — query for invoice
   - Idempotent: DELETE existing for same RID/date range, then INSERT

2. Night Audit job must call `GenerateNightChargesAsync` for all active stays

3. Write tests:
   - Generate for 3-night stay → 3 NightCharge records
   - Regenerate for same stay → DELETE old, INSERT new (idempotent)
   - Close on checkout → set IsClosed=true
   - Get for folio → sum of charges

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.4: CheckOutWorkflowService

**Task:** Replace `CheckOutService` with full legacy checkout workflow

**Read:** `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT.md`, `40_GOLDEN_SCENARIOS.md` scenario 6

**Implementation:**

1. Create `HotelPro.Core/Services/ICheckOutWorkflowService.cs` + implementation:
   - `CheckOutRoomAsync(roomId, command)` — Golden Scenario 6.2:
     - Transaction: close nights, close stays, close advances, close folio, close expenses
     - Mark room dirty
     - Handle unpaid balance (create neplaceni records)
   - `CheckOutGuestAsync(guestId, roomId, command)` — Golden Scenario 6.3:
     - Partial checkout: close only that guest's nights
     - INSERT new NightCharge records for remaining guests (split share)
     - Stay open: folio stays open until last guest
   - `ReturnToRoomAsync(folioId)` — reverse checkout:
     - Reopen folio
     - Reopen expenses
     - Mark room Occupied again

2. Write tests:
   - Full room checkout → all nights closed, all stays closed, folio closed, room dirty
   - Partial guest checkout → only that guest closed, new nights for remaining guests
   - Return to room → folio reopened, expenses reopened
   - Unpaid checkout → neplaceni records created

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.5: FolioLedgerService

**Task:** Replace `FolioService` (Balance-based) with Ledger-based aggregation

**Read:** `07/LEGACY_ANALYSIS/14_PAYMENT.md`, `07/LEGACY_ANALYSIS/15_EXPENSES_NIGHTS.md`

**Implementation:**

1. Create `HotelPro.Core/Services/IFolioLedgerService.cs` + implementation:
   - `GetFolioBalanceAsync(folioId)` — aggregate from:
     - NightCharges: SUM where FolioId and IsClosed=false (or all for period)
     - Expenses: SUM where GSID matches stay and IsLocked=false
     - Payments: SUM where FolioId
   - `AddExpenseAsync(folioId, expenseCommand)` — add charge to folio
   - `CloseExpensesAsync(folioId, receiptNumber)` — set IsLocked=true, set Brrac
   - `ReopenExpensesOnStornoAsync(receiptNumber)` — set IsLocked=false, clear Brrac for TID<>1; DELETE TID=1

2. Extend `Charge` entity:
   - Add `IsLocked` (zaklj), `ReceiptNumber` (Brrac), `ExpenseTypeId` (TID), `IsPartial` (Djelimicno)

3. Write tests:
   - Empty folio → balance 0
   - Add night charge → balance increases
   - Add expense → balance increases
   - Pay partial → balance decreases by payment amount
   - Close expenses on receipt → locked with receipt number
   - Storno: reopen non-accommodation expenses, delete accommodation

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.6: InvoiceWorkflowService

**Task:** Replace `InvoiceGenerator` with full invoice lifecycle

**Read:** `07/LEGACY_ANALYSIS/16_INVOICE_CHECKOUT_DEEP.md`, `40_GOLDEN_SCENARIOS.md` scenario 7

**Implementation:**

1. Create `HotelPro.Core/Services/IInvoiceWorkflowService.cs` + implementation:
   - `CreateInvoiceFromFolioAsync(folioId, command)` — snapshot:
     - Read all charges, night charges, payments for folio
     - Create immutable Invoice, InvoiceDetail, InvoiceFooter records
     - Apply PDV from HotelConfig (not hardcoded)
     - Split per payment method
   - `StornoInvoiceAsync(invoiceId, reason)` — Golden Scenario 7.1:
     - Transaction: mark payment storno, mark details storno, mark invoice storno/exp=2
     - Reopen non-accommodation expenses (zaklj→0, Brrac→null)
     - DELETE accommodation expenses (TID=1)
     - Attempt fiscal storno if device configured
   - `CreateAdvanceInvoiceAsync(folioId, amount)` — advance invoice
   - `StornoAdvanceInvoiceAsync(invoiceId)` — advance storno (flag only, no reversal)
   - `ReprintInvoiceAsync(invoiceId)` — reprint from saved data (no re-fiscalization)

2. Extend Invoice entities:
   - `Invoice`: add Storno, ExportStatus, FiscalNumber, FiscalTime, FiscalAmount, CancelledAt
   - `InvoiceDetail`: add ExpenseTypeId, PaymentMethod, VATRate, PriceExVAT, VATAmount, Discount, DiscountReason
   - `InvoiceFooter`: add AdvanceAmount, NightCount, Notes
   - `AdvanceInvoice`: new entity with full fields from `printracuniavans`

3. Remove hardcoded PDV (25%) and hotel VAT from InvoiceGenerator — use HotelConfig

4. Write tests:
   - Create invoice from folio with charges → correct totals with PDV
   - Create invoice with split payment → detail per payment method
   - Storno regular invoice → expenses reopened, accommodation deleted
   - Storno advance invoice → only flag set
   - PDV from config → not hardcoded

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.7: PaymentAllocationService

**Task:** Extend Payment for split payment and advance

**Read:** `07/LEGACY_ANALYSIS/14_PAYMENT.md`, `40_GOLDEN_SCENARIOS.md` scenario 4

**Implementation:**

1. Extend `Payment` entity:
   - Add `Storno` (bool), `BusinessUnit` (string), `IsAdvance` (bool), `ProformaId` (Guid?)

2. Create `PaymentDetail` entity (maps to `placanjedetalji`):
   - PaymentId, ArticleType (art: 1=accommodation, other=expense), Quantity, UnitPrice, TotalAmount, NightCount, FolioId, IsStorno, RoomName, GuestName

3. Create `PaymentAllocation` entity (maps to `placanjeslozeno`):
   - PaymentId, Method (Cash/Transfer/Card/Gratis), Amount

4. Create `HotelPro.Core/Services/IPaymentAllocationService.cs`:
   - `RecordPaymentAsync(folioId, command)` — full payment flow:
     - Validate total matches folio balance
     - Create Payment + PaymentDetails + PaymentAllocations
     - Close expenses (set IsLocked=true, ReceiptNumber)
     - Create Invoice from snapshot
     - Handle partial payments if amount < total
   - `RecordSplitPaymentAsync(folioId, allocations)` — multiple methods
   - `RecordAdvancePaymentAsync(folioId, amount)` — advance payment

5. Write tests:
   - Single method payment → one PaymentAllocation
   - Split payment (cash + card) → two PaymentAllocations, sum = total
   - Partial payment → expense IsPartial=true
   - Advance payment → separate advance tracking

**Verify:** `dotnet build` + `dotnet test` pass

---

### B.8: ReservationPolicyService

**Task:** Extend Booking with reservation confirmation/cancellation semantics

**Read:** `07/LEGACY_ANALYSIS/13_RESERVATIONS.md`, `30_STATUS_MATRIX.md` section 3

**Implementation:**

1. Extend `Booking` entity:
   - Add `IsConfirmed` (potvrda), `ConfirmationNumber` (brojPotvrde), `IsCancelled` (stornirana), `CancellationNumber` (brojStorna), `CancelledAt`, `CancellationReason`, `ConfirmedAt`

2. Extend `BookingRoom` entity:
   - Add `IsFromConfirmedReservation` (rezervP)

3. Create `HotelPro.Core/Services/IReservationPolicyService.cs`:
   - `ConfirmReservationAsync(bookingId)` — set IsConfirmed=true, generate ConfirmationNumber
   - `CancelReservationAsync(bookingId, reason)` — set IsCancelled=true, generate CancellationNumber
   - `AutoExpireReservationsAsync()` — set prijava=2 for expired unconfirmed
   - `TransferReservationToCheckInAsync(bookingId)` — create Stay records from reservation

4. Use sequence generator for ConfirmationNumber and CancellationNumber (not MAX+1)

5. Write tests:
   - Confirm → IsConfirmed=true, ConfirmationNumber assigned
   - Cancel → IsCancelled=true, CancellationNumber assigned
   - Auto-expire → unconfirmed past-date reservations get expired
   - Transfer to check-in → Stay records created, Reservation marked as checked-in

**Verify:** `dotnet build` + `dotnet test` pass

---

## FAZA C: EF CORE MIGRATIONS

After all B tasks are done:

1. Create ONE migration for all new entities and fields:
   - Stay entity
   - NightCharge entity
   - PaymentDetail entity
   - PaymentAllocation entity
   - AdvanceInvoice entity
   - HotelConfig entity
   - Extended fields on Room, Booking, BookingRoom, Charge, Payment, Invoice, InvoiceItem, Folio

2. Update `LegacyMigrator` to map legacy columns to new fields

3. Verify: `dotnet ef database update` succeeds on clean database

---

## FAZA D: FRONTEND ADAPTATION

After all C tasks are done:

1. Update API client types to match new entity fields
2. Update RoomStatus enum to 8 values (7 + Dirty)
3. Update check-in form to support multi-guest
4. Update checkout form to support partial checkout
5. Update folio display to show ledger (nights + expenses + payments)
6. Update invoice display to show detailed breakdown with PDV
7. Update payment form to support split payment
8. Add admin settings page for HotelConfig (API keys, feature toggles)
9. Feature-gate mock integrations in UI (show "Configure in Settings" when not set up)

---

## FAZA E: INTEGRATION CONFIGURATION

After D is done:

1. Make each mock integration check `HotelConfig.IsEnabled` before executing
2. Change mock responses to configuration-aware:
   - Not configured → `{ status: "not_configured", message: "Configure in Admin > Settings" }`
   - Configured but error → `{ status: "error", message: "Check API key in Settings" }`
   - Working → real integration response
3. Add "Test Connection" buttons in admin settings for each integration
4. IoT module: make entirely optional, controlled by `IoT_Mqtt_Enabled` flag

---

## TASK DEPENDENCY GRAPH

```
A.1 ─┐
A.2 ─┤
A.3 ─┘── B.1 ── B.2 ── B.3 ─┐
                              ├── B.4 ── B.5 ─┐
B.6 ──────────────────────────┤                ├── B.7 ─┐
B.8 (independent)              │                        │
                               │                        ▼
                               └── C ── D ── E
```

B.1-B.3 can be done sequentially (B.2 depends on B.1 entities, B.3 depends on B.2)
B.4-B.5 must be done after B.2-B.3 (checkout needs stays and nights)
B.6 can be done in parallel with B.4-B.5 (invoice is independent of checkout workflow)
B.7 can be done after B.5 (payment needs folio)
B.8 can be done in parallel with everything else (reservation is independent)

---

## ESTIMATED EFFORT

| Task | Est. Hours | Priority |
|------|-----------|----------|
| A.1 | 0.5 | P0 |
| A.2 | 2 | P0 |
| A.3 | 3 | P0 |
| B.1 | 3 | P0 |
| B.2 | 4 | P0 |
| B.3 | 3 | P0 |
| B.4 | 4 | P0 |
| B.5 | 3 | P0 |
| B.6 | 4 | P0 |
| B.7 | 3 | P0 |
| B.8 | 2 | P1 |
| C | 2 | P0 |
| D | 8 | P1 |
| E | 4 | P1 |
| **Total** | **~43 hours** | |

---

## VERIFICATION CHECKLIST

After each B task:
- [ ] `dotnet build` passes with 0 errors
- [ ] `dotnet test` passes all tests
- [ ] New tests cover the specific legacy scenario
- [ ] No hardcoded secrets or magic numbers
- [ ] HotelConfig used for settings (not hardcoded values)
- [ ] Status in `STATUS_REALISTIC.md` updated

After C:
- [ ] `dotnet ef database update` succeeds
- [ ] LegacyMigrator maps new fields correctly

After D:
- [ ] `npm run build` passes
- [ ] `npm run lint` passes
- [ ] Frontend shows all 8 room statuses
- [ ] Admin settings page renders correctly

After E:
- [ ] Mock integrations return "not_configured" when keys are missing
- [ ] Mock integrations return real errors when keys are wrong
- [ ] Feature flags work correctly