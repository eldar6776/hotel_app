# ATOMIC TASKS DRAFT

Status: DRAFT_NOT_READY_FOR_IMPLEMENTATION
Date: 2026-05-17

These tasks are intentionally marked draft. They become ready only after full extraction and review.

## TASK-PMS-001: Legacy-derived room clean/dirty transition

Legacy references:

- `Data.vb`, `PrljavaSoba`
- `frmPlacanje.vb`, checkout branch calling `PrljavaSoba`

Affected domain concept: Room / RoomOperationalState.

Expected behavior: successful checkout marks room dirty equivalent to `sobe.clean = 0`.

Acceptance scenario: `SCENARIO-CHECKOUT-001`.

Files/modules to touch: new PMS room status service/tests only.

Files/modules not to touch: existing fiscal/payment implementation until checkout workflow is extracted.

Validation command: backend test command pending project decision.

Rollback note: use checkpoint commit `693d1d1` for pre-analysis state if requested.

## TASK-PMS-002: Check-in creates Stay and FolioSession

Legacy references:

- `frmPrijava1.vb`, `btnPrijava_Click`, `provjeriPID`, `dodajFolio`, `unesi`
- SQL procedures `addFolio`, `addRelGostSoba`

Affected domain concept: Stay, RoomAssignment, FolioSession.

Expected behavior: check-in reuses active room PID if present, otherwise creates open folio; inserts one stay row per guest.

Acceptance scenario: `SCENARIO-CHECKIN-001`.

Files/modules to touch: new PMS check-in service/tests only.

Files/modules not to touch: existing generic booking CRUD until booking lifecycle is extracted.

Validation command: pending.

Rollback note: use checkpoint commit `693d1d1` if requested.

## TASK-PMS-003: Materialized NightLedger on check-in

Legacy references:

- `frmPrijava1.vb`, `nocenja`, `dodajnocenja`
- SQL procedure `Unesinocenja`

Affected domain concept: NightLedger.

Expected behavior: create open night ledger row per stay, preserving duplicate-prevention behavior by RID/date.

Acceptance scenario: `SCENARIO-CHECKIN-001`.

Files/modules to touch: new NightLedger service/tests only.

Files/modules not to touch: invoice/payment until their scenarios are complete.

Validation command: pending.

Rollback note: use checkpoint commit `693d1d1` if requested.

## TASK-PMS-004: Full checkout closure

Legacy references:

- `Data.vb`, `OdjavaSobe`
- `frmPlacanje.vb`, checkout branch

Affected domain concept: CheckoutWorkflow, NightLedger, FolioSession, ExpenseLedger.

Expected behavior: close active nights, stays, folio and open expenses in one transaction; mark room dirty after success.

Acceptance scenario: `SCENARIO-CHECKOUT-001`.

Files/modules to touch: new Checkout service/tests only.

Files/modules not to touch: storno until storno extraction completes.

Validation command: pending.

Rollback note: use checkpoint commit `693d1d1` if requested.

## TASK-PMS-005: Partial checkout continuation ledger

Legacy references:

- `Data.vb`, `OdjavaSobe`, `gid <> 0` branch

Affected domain concept: CheckoutWorkflow, NightLedger.

Expected behavior: close one guest/stay and insert continuation night rows for guests remaining in same room/PID.

Acceptance scenario: `SCENARIO-PARTIAL-CHECKOUT-001`.

Files/modules to touch: new Checkout service/tests only.

Files/modules not to touch: room-wide folio close behavior except as tested.

Validation command: pending.

Rollback note: use checkpoint commit `693d1d1` if requested.

## Not Ready Tasks

- Booking confirmation/storno tasks: reservation lifecycle not fully extracted.
- Invoice snapshot tasks: enough evidence for model, not enough for line/tax details.
- Fiscal tasks: device-specific transitions not fully extracted.
- Storno invoice tasks: paths not yet read.

