# IMPLEMENTATION PLAN

Status: BLOCKED_UNTIL_FULL_EXTRACTION
Date: 2026-05-17

Per `README.md`, production implementation must not start until `GOLDEN_SCENARIOS.md` and `LEGACY_TO_NEW_MAPPING.md` exist and are reviewed. Initial drafts now exist, but extraction is not complete enough for coding.

## Vertical Plan Draft

1. Room status read model
   - Inputs: `sobe`, active `relgostsoba`, `nocenja`, `sobe.clean`, `sobe.ooo`, SQL `fnSobaStatus`.
   - Validation: room status scenarios after check-in, checkout, OOO, dirty/clean.

2. Stay/check-in ledger
   - Inputs: `frmPrijava1`, `addFolio`, `addRelGostSoba`, `Unesinocenja`.
   - Validation: `SCENARIO-CHECKIN-001`.

3. Night ledger
   - Inputs: `nocenja`, `Unesinocenja`, `Data.OdjavaSobe`, payment cutoff behavior.
   - Validation: check-in, checkout, payment-without-checkout, partial checkout.

4. Folio preparation
   - Inputs: `Data.pripremaRcuna`, `gettroskovi`, `placanjedetalji` prior payments.
   - Validation: open expenses + prior night payments combine like legacy.

5. Invoice snapshot
   - Inputs: `frmPlacanje.genenisanjeReporta` paths, `printracuni*`.
   - Validation: snapshot persists header/detail/footer independent of current folio.

6. Storno restore
   - Inputs: `frmRacun.vb`, `frmRacuni.vb`, report storno files.
   - Validation: storno effects on invoice/payment/expense/night rows.

7. Checkout closure
   - Inputs: `Data.OdjavaSobe`, `frmPlacanje` checkout prompts, `frmOdjava1`.
   - Validation: full and partial checkout scenarios.

8. Reports
   - Inputs: Crystal/RDLC report wrappers and SQL procedures.
   - Validation: reports use snapshot/ledger tables, not recomputed current state unless legacy does so.

## Blockers Before Coding

- Complete P0 function caller map.
- Complete SQL write map.
- Extract `fnSobaStatus` and all room status codes.
- Extract storno paths.
- Extract reservation create/confirm/cancel lifecycle.
- Review draft scenarios with user.

