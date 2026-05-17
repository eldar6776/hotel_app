# CANONICAL DOMAIN MODEL

Status: INITIAL_DRAFT
Date: 2026-05-17

This model is derived from initial legacy evidence. It must stay open until booking, storno, fiscal and reporting paths are fully extracted.

## Room

Responsibility: physical sellable room plus operational flags.

Legacy sources: `sobe`, `sobavrsta`, `zgrade`, `Data.PrljavaSoba`, SQL room procedures.

Key states: clean/dirty, OOO, derived occupancy/status.

Invariant: room operational state cannot be derived only from bookings; active `relgostsoba`, `nocenja`, `sobe.clean`, `sobe.ooo` all contribute.

## Booking

Responsibility: pre-arrival reservation intent.

Legacy sources: `rezervacije`, `rezervacijasobe`, `rezervacijegrupe`, `rezervacijeizvor`, `rezervacijetip`.

Key states: `prijava`, `potvrda`, `stornirana`.

Invariant: booking is separate from stay; check-in marks reservation `prijava = 1` and creates stay/ledger records.

## Stay

Responsibility: actual guest-room occupancy assignment.

Legacy sources: `relgostsoba`, `frmPrijava1.vb`, `Data.OdjavaSobe`.

Key states: active (`odjavljen = 0`), checked out (`odjavljen = 1`), reservation marker (`rezervacija`).

Invariant: a stay is not only a date range; it has worker, room, guest, tariff, group, folio PID and print/tourist flags.

## FolioSession

Responsibility: room/PID envelope for active visit/folio lifecycle.

Legacy sources: `posjetafolio`, `frmPrijava1.dodajFolio`, `Data.OdjavaSobe`.

Key states: open (`zakljucen = False`), closed (`zakljucen = True`).

Invariant: multiple active `relgostsoba` rows can share a PID.

## NightLedger

Responsibility: materialized nightly charge/period facts.

Legacy sources: `nocenja`, `Unesinocenja`, `frmPrijava1.nocenja`, `Data.OdjavaSobe`, `frmPlacanje.placanje`.

Key states: open (`PrijavaOdjava = 0`), closed (`PrijavaOdjava = 1`).

Invariant: do not replace with computed nights from dates without explicit user decision. Legacy inserts, closes, and reopens/continues night rows.

## ExpenseLedger

Responsibility: non-night charges attached mainly to room/stay before billing.

Legacy sources: `troskovi`, `troskovivrste`, `Data.gettroskovi`, `frmPlacanje`.

Key states: open (`zaklj = 0`), locked (`zaklj = 1`), partial (`Djelimicno = 1`).

Invariant: locked expense rows represent billed/closed state and may carry invoice number `Brrac`.

## PaymentLedger

Responsibility: payment header and detailed payment lines.

Legacy sources: `placanje`, `placanjedetalji`, `placanjenacin`, `placanjeslozeno`, `frmPlacanje`.

Key states: active/storno pending full extraction.

Invariant: payment details are independent ledger facts and are used by folio/prep calculations.

## InvoiceSnapshot

Responsibility: immutable printable/reportable invoice representation.

Legacy sources: `printracuni`, `printracunidetalji`, `printracunifooter`, report/export files.

Key states: active/storno/fiscalized pending full extraction.

Invariant: reports read snapshot tables; invoice cannot be only a live sum of current folio.

## FiscalWorkflow

Responsibility: device-specific fiscalization of invoice snapshot.

Legacy sources: `frmPlacanje.vb` methods for Tring/NSC/HCP/Mikroelektronika and `printracuni.fis*` fields.

Key states: UNKNOWN until device methods are fully read.

Invariant: fiscal result is persisted back to invoice snapshot.

