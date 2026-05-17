# BUSINESS RULES CATALOG

Status: INITIAL_P0_RULES
Date: 2026-05-17

## RULE-ROOM-001

Name: Checkout marks room dirty.

Legacy reference: `Data.vb`, `PrljavaSoba`; called from `frmPlacanje.vb` after successful `OdjavaSobe`.

Rule: after successful checkout/payment-triggered checkout, the room row is updated with `sobe.clean = 0`.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| UPDATE | `sobe` | `clean = 0` | `ID = sobaid` |

Open questions: numeric meaning of all `clean` values must be completed from housekeeping screens and reports.

## RULE-ROOM-002

Name: Room status is derived from active stays/reservation placeholders and OOO override.

Legacy reference: SQL dump, function `fnSobaStatus(SoID, datumP, datumK, tod)`; room overview procedure calls it when listing rooms.

Rule: if no active `relgostsoba` exists for room, status is `0`. If active non-reservation stay exists and `tod < checkOutDate`, status is `1`; if `tod >= checkOutDate`, status is `2`. Reservation placeholder rows (`rezervacija = 1`) produce `6` when `rezervP = 0` and `3` when `rezervP = 1`. If `sobe.ooo = 1`, status is overridden to `5`.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| SELECT | `relgostsoba` | `sobaID`, `odjavljen`, `rezervacija`, `rezervP`, `checkOutDate` | status derivation |
| SELECT | `sobe` | `ooo` | OOO override |

Open questions:

- Function contains a branch that can set `stat1 = 4`, but the local logic appears internally inconsistent. Treat status `4` as `UNKNOWN/POSSIBLE_BUG` until runtime behavior or more callers clarify it.

## RULE-STAY-001

Name: Check-in creates a guest-room stay assignment.

Legacy reference: `frmPrijava1.vb`, `btnPrijava_Click`, `unesi`; stored procedure `addRelGostSoba` in SQL dump.

Rule: after user selects room type, room, dates, and at least one guest, check-in inserts one `relgostsoba` row per new guest with room, dates, worker, tariff, group, PID/folio, flags, notes, service, tax/status fields.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| INSERT | `relgostsoba` | guest/room/date/worker/status/tariff/PID fields | one row per new guest |

Edge cases:

- Missing room type, room, guest list, or new guests aborts with user message.
- Check-in date must not be before the last relevant date by more than the legacy threshold.
- `PID` is reused if active guest-room rows already exist for the room.

## RULE-FOLIO-001

Name: First active room check-in creates folio; full checkout closes it.

Legacy reference: `frmPrijava1.vb`, `provjeriPID`, `dodajFolio`; `Data.vb`, `OdjavaSobe`; procedures `addFolio`.

Rule: check-in checks whether the room already has an active `relgostsoba.PID`; if not, it creates `posjetafolio` with start time and `zakljucen = False`. Full room checkout sets `vrijemeO = now` and `zakljucen = True`.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| SELECT | `relgostsoba` | `PID` | active room rows |
| INSERT | `posjetafolio` | `SID`, `vrijemeD`, `vrijemeO`, `zakljucen` | no active PID |
| UPDATE | `posjetafolio` | `vrijemeO`, `zakljucen` | full checkout by `PID` |

## RULE-NIGHT-001

Name: Check-in materializes night ledger rows.

Legacy reference: `frmPrijava1.vb`, `nocenja`, `vratiRID`, `dodajnocenja`; procedure `Unesinocenja`.

Rule: check-in creates `nocenja` rows after `relgostsoba` rows exist. The tariff can be split by guest count depending on `setings.naplposo`; if computed price is zero, tax/insurance settings are used. Procedure deletes same RID/date row before inserting, so ledger date/RID uniqueness is enforced operationally.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| DELETE | `nocenja` | row | same `RID` and `DatumP` |
| INSERT | `nocenja` | `RID`, `DatumP`, `Tarifa`, `SID`, `PID`, `PrijavaOdjava`, `opis`, `popust`, `soba` | one per stay row |

## RULE-CHECKOUT-001

Name: Checkout closes night rows, stay rows, folio, and open expenses.

Legacy reference: `Data.vb`, `OdjavaSobe`.

Rule: checkout runs in a MySQL transaction. It closes active `nocenja`, marks `relgostsoba` as `odjavljen = 1`, closes `posjetafolio`, and locks open `troskovi` for full room checkout.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| UPDATE | `nocenja` | `PrijavaOdjava = 1`, `datumodj` | `SID/PID`, optionally `RID` |
| UPDATE | `relgostsoba` | `odjavljen`, `checkOutDate`, `checkOutRadnik`, `pl` | room or single stay row |
| UPDATE | `posjetafolio` | `vrijemeO`, `zakljucen` | `PID`, full checkout only |
| UPDATE | `troskovi` | `zaklj = 1` | room open expenses, full checkout only |

## RULE-CHECKOUT-002

Name: Partial checkout preserves remaining guests via continuation night row.

Legacy reference: `Data.vb`, `OdjavaSobe`, branch `gid <> 0`.

Rule: when only one guest/stay row is checked out, the matching `nocenja` rows are closed, and new open `nocenja` rows are inserted for other `RID` values in the same room/PID. This prevents partial checkout from closing the entire room ledger.

Open questions: exact UI path for `gid <> 0` needs caller proof.

## RULE-EXPENSE-001

Name: Expenses are open until locked by checkout or invoice.

Legacy reference: `Data.vb`, `OdjavaSobe`; `frmPlacanje.vb`, `placanje`.

Rule: `troskovi.zaklj = 0` means open/unlocked. Checkout or invoice payment sets `zaklj = 1`; invoice paths also set `Brrac` to the invoice/account number. Partial payment may set `Djelimicno = 1`.

## RULE-PAYMENT-001

Name: Payment is persisted as header and detail ledger.

Legacy reference: `frmPlacanje.vb`, `dodajPlacanje`, `placanje(Integer)`.

Rule: payment creates a `placanje` header and one or more `placanjedetalji` rows. Payment detail may represent stay/night charges, expense charges, or prior night payments.

Open questions: full distinction of `art` values and `ranijeUplate` needs complete payment review.

## RULE-INVOICE-001

Name: Invoice is a print snapshot, not just current folio sum.

Legacy reference: `frmPlacanje.vb`, report generation; tables `printracuni`, `printracunidetalji`, `printracunifooter`.

Rule: invoice generation writes printable header/footer/detail snapshot rows. Reports and export read snapshot tables, so the new system must preserve invoice snapshot semantics.

## RULE-FISCAL-001

Name: Fiscal result is attached to invoice snapshot.

Legacy reference: `frmPlacanje.vb`, fiscal methods and `UPDATE printracuni`.

Rule: after fiscal device interaction, the invoice row is updated with fiscal receipt data (`fisrac`, `fisvr`, `fisIZN`).

Open questions: Tring/NSC/HCP/Mikroelektronika device-specific success/failure transitions need deeper review.

## RULE-EXPENSE-002

Name: Open individual expenses can be moved to another room before locking.

Legacy reference: SQL dump procedure `unesiPojedinacne(noviSID, ID, stariSID)`.

Rule: an expense row can be moved from old room to new room only when `troskovi.zaklj = 0` and the specific `troskovi.ID` matches. Locked expenses are not moved by this procedure.

Database effects:

| Operation | Table | Columns | Condition |
|---|---|---|---|
| UPDATE | `troskovi` | `SID = noviSID` | `SID = stariSID AND zaklj = 0 AND ID = ID` |

Modern mapping: ExpenseLedger transfer operation, gated by open/unlocked state.
