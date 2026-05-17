# SQL WRITE MAP

Status: INITIAL_MAP
Date: 2026-05-17

## Confirmed Writes

| ID | File | Function/Procedure | Operation | Table | Columns | Condition | Business Meaning | Rule ID | Scenario ID | Risk |
|---|---|---|---|---|---|---|---|---|---|---|
| SQLWRITE-001 | `Data.vb` | `PrljavaSoba` | UPDATE | `sobe` | `clean = 0` | `ID = sobaid` | After checkout/payment, room is marked dirty/not clean. | RULE-ROOM-001 | SCENARIO-CHECKOUT-001 | P0 |
| SQLWRITE-002 | `Data.vb` | `OdjavaSobe` | UPDATE | `nocenja` | `PrijavaOdjava = 1`, `datumodj = @checkOutDate` | `SID = @SID AND PID = @PID`, optionally `rid = @gid` | Closes active night ledger rows at checkout or partial checkout. | RULE-CHECKOUT-001 | SCENARIO-CHECKOUT-001 | P0 |
| SQLWRITE-003 | `Data.vb` | `OdjavaSobe` | INSERT SELECT | `nocenja` | `RID`, `DatumP`, `SID`, `PID`, `PrijavaOdjava`, `Tarifa`, `popust`, `opis`, `soba` | copied from same `SID/PID`, excluding `rid = @gid` | Partial checkout creates continuation night rows for guests staying. | RULE-CHECKOUT-002 | SCENARIO-PARTIAL-CHECKOUT-001 | P0 |
| SQLWRITE-004 | `Data.vb` | `OdjavaSobe` | UPDATE | `relgostsoba` | `odjavljen`, `checkOutDate`, `checkOutRadnik`, `pl` | room-wide or single `id = @gid` | Marks stay/room assignment as checked out. | RULE-CHECKOUT-001 | SCENARIO-CHECKOUT-001 | P0 |
| SQLWRITE-005 | `Data.vb` | `OdjavaSobe` | UPDATE | `posjetaFolio` | `vrijemeO`, `zakljucen` | `ID = @PID` | Closes folio for full room checkout. | RULE-FOLIO-001 | SCENARIO-CHECKOUT-001 | P0 |
| SQLWRITE-006 | `Data.vb` | `OdjavaSobe` | UPDATE | `troskovi` | `zaklj = 1` | `SID = @SID AND zaklj = 0` | Locks open expenses during full checkout. | RULE-EXPENSE-001 | SCENARIO-CHECKOUT-001 | P0 |
| SQLWRITE-007 | `frmPrijava1.vb` / SQL dump | `addFolio` | INSERT | `posjetafolio` | `SID`, `vrijemeD`, `vrijemeO`, `zakljucen` | none | Creates folio session on first check-in to a room without active PID. | RULE-FOLIO-001 | SCENARIO-CHECKIN-001 | P0 |
| SQLWRITE-008 | `frmPrijava1.vb` / SQL dump | `addRelGostSoba` | INSERT | `relgostsoba` | guest, room, dates, worker, flags, group, tariff, PID, notes | none | Creates guest-room stay assignment. | RULE-STAY-001 | SCENARIO-CHECKIN-001 | P0 |
| SQLWRITE-009 | `frmPrijava1.vb` / SQL dump | `Unesinocenja` | DELETE | `nocenja` | row by `RID` and `DatumP` | same RID and date | Prevents duplicate night ledger line for same stay/date. | RULE-NIGHT-001 | SCENARIO-CHECKIN-001 | P0 |
| SQLWRITE-010 | `frmPrijava1.vb` / SQL dump | `Unesinocenja` | INSERT | `nocenja` | `RID`, `DatumP`, `Tarifa`, `SID`, `PID`, `PrijavaOdjava`, `opis`, `popust`, `soba` | none | Materializes initial night ledger line at check-in. | RULE-NIGHT-001 | SCENARIO-CHECKIN-001 | P0 |
| SQLWRITE-011 | `frmPrijava1.vb` | reservation check-in path | UPDATE | `rezervacije` | `prijava = 1` | `id = selected reservation id` | Marks reservation as converted/checked in. | RULE-BOOKING-001 | SCENARIO-BOOKING-CHECKIN-001 | P0 |
| SQLWRITE-012 | `frmPlacanje.vb` | `placanje` | UPDATE | `troskovi` | `zaklj = 1`, `Brrac = txtBrojRacuna` | `ID = idtrr` | Locks invoiced expense line and links it to invoice number. | RULE-EXPENSE-001 | SCENARIO-PAYMENT-001 | P0 |
| SQLWRITE-013 | `frmPlacanje.vb` | `placanje` | UPDATE | `nocenja` | `PrijavaOdjava = 1`, `datumodj`, `brrac` | `SID = selected AND PrijavaOdjava = 0` | Closes night ledger for billed period when payment occurs without checkout. | RULE-NIGHT-002 | SCENARIO-PAYMENT-NO-CHECKOUT-001 | P0 |
| SQLWRITE-014 | `frmPlacanje.vb` | `placanje` | INSERT SELECT | `nocenja` | continuation night row | same `SID/PID` | Reopens/continues active night ledger after billing cutoff without checkout. | RULE-NIGHT-002 | SCENARIO-PAYMENT-NO-CHECKOUT-001 | P0 |
| SQLWRITE-015 | `frmPlacanje.vb` | `placanje(Integer)` | INSERT | `placanjedetalji` | payment line fields | current payment number | Stores payment/invoice detail line. | RULE-PAYMENT-001 | SCENARIO-PAYMENT-001 | P0 |
| SQLWRITE-016 | `frmPlacanje.vb` | `dodajPlacanje` / group variant | INSERT | `placanje` | header fields including `broj`, `relgostsobaID`, `iznos`, `nacin`, `PID` | none | Stores payment header. | RULE-PAYMENT-001 | SCENARIO-PAYMENT-001 | P0 |
| SQLWRITE-017 | `frmPlacanje.vb` | report generation | INSERT | `printracuni` | invoice snapshot header fields | none | Stores printable invoice header snapshot. | RULE-INVOICE-001 | SCENARIO-PAYMENT-001 | P0 |
| SQLWRITE-018 | `frmPlacanje.vb` | fiscal update | UPDATE | `printracuni` | `fisrac`, `fisvr`, `fisIZN` | `BrojRacuna = id` | Attaches fiscal device result to invoice snapshot. | RULE-FISCAL-001 | SCENARIO-FISCAL-001 | P0 |
| SQLWRITE-019 | SQL dump | `addPlacanjeSlozeno` | INSERT | `placanjeslozeno` | `rbr`, `nacin`, `iznos` | none | Persists split/compound payment method line for payment number. | RULE-PAYMENT-001 | SCENARIO-PAYMENT-001 | P0 |
| SQLWRITE-020 | SQL dump | `addTroskovi` | INSERT | `troskovi` | `GSID`, `SID`, `TID`, `vrijeme`, `kolicina`, `iznos`, `radnikID` | none | Creates open expense charge. | RULE-EXPENSE-001 | SCENARIO-EXPENSE-001 | P0 |
| SQLWRITE-021 | SQL dump | `unesiPojedinacne` | UPDATE | `troskovi` | `SID = noviSID` | `SID = stariSID AND zaklj = 0 AND ID = ID` | Moves an unlocked individual expense to another room. | RULE-EXPENSE-002 | SCENARIO-ROOM-TRANSFER-001 | P0 |

## Pending

- Complete all `INSERT/UPDATE/DELETE` writes from `frmRacun.vb`, `frmRacuni.vb`, reservation forms, `frmGosti.vb`, `frmTroskovi*.vb`, and SQL procedures.
- Classify migration/config writes separately from business writes.
