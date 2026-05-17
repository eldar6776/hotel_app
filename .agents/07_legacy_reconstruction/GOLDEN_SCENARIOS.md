# GOLDEN SCENARIOS

Status: INITIAL_DRAFT
Date: 2026-05-17

These are draft scenarios from confirmed legacy paths. They are not yet executable tests.

## SCENARIO-CHECKIN-001: Check-in creates stay, folio, and initial night ledger

Legacy references:

- `frmPrijava1.vb`, `btnPrijava_Click`
- `frmPrijava1.vb`, `provjeriPID`, `dodajFolio`, `unesi`, `nocenja`, `dodajnocenja`
- SQL procedures `addFolio`, `addRelGostSoba`, `Unesinocenja`

Initial database state:

- Room exists in `sobe` with `ooo = 0`.
- No active `relgostsoba` for selected room, so `provjeriPID(room)` returns `0`.
- At least one guest is present in the check-in guest grid.

User action:

- Reception selects room type, room, arrival/departure dates, tariff, guest(s), then clicks Prijava.

Expected database changes:

- Insert one `posjetafolio` row with selected `SID`, `vrijemeD = datePrijava`, `vrijemeO = NULL`, `zakljucen = False`.
- Insert one `relgostsoba` row per guest with `odjavljen = 0`, `rezervacija = 0`, selected room, dates, tariff, group and created/reused `PID`.
- For each inserted stay row, delete any existing `nocenja` row for same `RID` and `DatumP`, then insert a `nocenja` row with `PrijavaOdjava = 0`.

Must not happen:

- Check-in must not continue without room type, room, guest rows, or new guests.
- New backend must not calculate active stay only from date range; `relgostsoba` and `nocenja` rows are legacy facts.

## SCENARIO-CHECKOUT-001: Full checkout closes ledger, folio, expenses and marks room dirty

Legacy references:

- `frmPlacanje.vb`, `placanje`
- `Data.vb`, `OdjavaSobe`
- `Data.vb`, `PrljavaSoba`

Initial database state:

- Active `relgostsoba` rows exist for selected room with same `PID`.
- Active `nocenja` rows exist with `PrijavaOdjava = 0`.
- Open expenses may exist in `troskovi` with `zaklj = 0`.
- `posjetafolio` row exists with `zakljucen = False`.

User action:

- Payment flow completes and user confirms room checkout.

Expected database changes:

- `nocenja` active rows for `SID/PID` are updated to `PrijavaOdjava = 1` and `datumodj = checkout timestamp`.
- `relgostsoba` active rows for the room are updated to `odjavljen = 1`, checkout date/worker set.
- `posjetafolio` is updated with `vrijemeO = now`, `zakljucen = True`.
- `troskovi` open rows for the room are updated to `zaklj = 1`.
- `sobe.clean` for the room is updated to `0`.

Must not happen:

- Expenses must not remain open after full checkout.
- Room must not remain clean after checkout.

## SCENARIO-PARTIAL-CHECKOUT-001: Partial checkout closes one guest and preserves remaining guests

Legacy reference:

- `Data.vb`, `OdjavaSobe`, branch `gid <> 0`

Initial database state:

- Multiple active `relgostsoba` rows exist for same room and `PID`.
- Active `nocenja` rows exist for those `RID` values.

User action:

- A single `relgostsoba.id` is checked out.

Expected database changes:

- `nocenja` for the selected `RID` closes with `PrijavaOdjava = 1`, `datumodj = checkout timestamp`.
- `relgostsoba` selected `id` is marked `odjavljen = 1`.
- New open `nocenja` continuation rows are inserted for other `RID` values in same room/PID with `DatumP = checkout timestamp`.
- Full `posjetafolio` close and room-wide expense lock are not executed in this branch.

Must not happen:

- Remaining guests must not be checked out.
- Remaining active night ledger must not disappear.

## SCENARIO-PAYMENT-NO-CHECKOUT-001: Payment can close billed nights and continue stay

Legacy reference:

- `frmPlacanje.vb`, `placanje`, user declines checkout after payment

Initial database state:

- Active stay and active `nocenja` rows exist.
- Payment is generated for a period ending at selected checkout/payment cutoff timestamp.

User action:

- User completes payment but answers No to room checkout.

Expected database changes:

- Active `nocenja` rows for selected room close with `PrijavaOdjava = 1`, `datumodj = cutoff`, `brrac = invoice number`.
- Continuation `nocenja` rows are inserted for same room/PID with `DatumP = cutoff`, `PrijavaOdjava = 0`.

Must not happen:

- `relgostsoba` must not be marked `odjavljen = 1`.
- `posjetafolio` must not be closed solely because payment happened.

