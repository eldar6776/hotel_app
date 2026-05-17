# LEGACY TO NEW MAPPING

Status: INITIAL_MAPPING
Date: 2026-05-17

| Legacy Table/Function/Form | Extracted Business Rule | Canonical Domain Concept | New Entity/Service/API | Acceptance Test | Implementation Status |
|---|---|---|---|---|---|
| `sobe.clean`, `Data.PrljavaSoba` | RULE-ROOM-001 | Room / RoomOperationalState | Room housekeeping status service/API | SCENARIO-CHECKOUT-001 | SPEC_READY |
| `frmPrijava1.btnPrijava_Click` | RULE-STAY-001 | Stay / CheckInWorkflow | CheckInService | SCENARIO-CHECKIN-001 | SPEC_READY |
| `posjetafolio`, `addFolio`, `Data.OdjavaSobe` | RULE-FOLIO-001 | FolioSession | FolioSessionService | SCENARIO-CHECKIN-001, SCENARIO-CHECKOUT-001 | SPEC_READY |
| `relgostsoba`, `addRelGostSoba` | RULE-STAY-001 | Stay / RoomAssignment | StayRepository, RoomAssignment aggregate | SCENARIO-CHECKIN-001 | SPEC_READY |
| `nocenja`, `Unesinocenja` | RULE-NIGHT-001 | NightLedger | NightLedgerService | SCENARIO-CHECKIN-001 | SPEC_READY |
| `Data.OdjavaSobe` | RULE-CHECKOUT-001, RULE-CHECKOUT-002 | CheckoutWorkflow | CheckoutService | SCENARIO-CHECKOUT-001, SCENARIO-PARTIAL-CHECKOUT-001 | SPEC_READY |
| `troskovi.zaklj`, `frmPlacanje` | RULE-EXPENSE-001 | ExpenseLedger | ExpenseLedgerService | SCENARIO-CHECKOUT-001, SCENARIO-PAYMENT-001 | ANALYZED |
| `placanje`, `placanjedetalji` | RULE-PAYMENT-001 | PaymentLedger | PaymentService | SCENARIO-PAYMENT-001 | ANALYZED |
| `printracuni`, `printracunidetalji` | RULE-INVOICE-001 | InvoiceSnapshot | InvoiceService | SCENARIO-PAYMENT-001 | ANALYZED |
| `printracuni.fis*`, fiscal methods | RULE-FISCAL-001 | FiscalWorkflow | FiscalizationService / bridge adapter | SCENARIO-FISCAL-001 | ANALYZED |
| `rezervacije.prijava` | RULE-BOOKING-001 | Booking | BookingCheckIn adapter | SCENARIO-BOOKING-CHECKIN-001 | ANALYZED |

## Mapping Rules

- Existing backend implementation is not treated as proof.
- New services listed here are target shape only; implementation tasks are not valid until all relevant scenarios are complete.
- Any row with `ANALYZED` still requires deeper extraction before implementation.

