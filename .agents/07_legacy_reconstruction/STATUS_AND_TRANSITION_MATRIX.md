# STATUS AND TRANSITION MATRIX

Status: INITIAL_MATRIX
Date: 2026-05-17

## Room

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `sobe.clean = 0` | dirty/not clean | room requires cleaning after checkout | `Data.PrljavaSoba`, `frmPlacanje.vb` | room views/housekeeping pending | successful checkout/payment-triggered checkout | CONFIRMED |
| `sobe.ooo` | out of order flag | room unavailable for normal selection | OOO procedure / UI pending | `frmPrijava1.vb` room selection filters `ooo=0`; SQL `fnSobaStatus` output includes `ooo` | UNKNOWN | PARTIAL |
| `fnSobaStatus(...)` | derived room status | computed by SQL function | SQL function pending read | SQL procedure around room overview | UNKNOWN | UNKNOWN |

## Stay / Guest-Room

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `relgostsoba.odjavljen = 0` | active stay row | guest currently assigned to room | `addRelGostSoba` | check-in/payment/guest/report queries | check-in insert | CONFIRMED |
| `relgostsoba.odjavljen = 1` | checked out stay row | guest/room assignment closed | `Data.OdjavaSobe` | reports/history pending | checkout/partial checkout | CONFIRMED |
| `relgostsoba.rezervacija = 0` | real stay, not reservation placeholder | active operational stay | `addRelGostSoba` | many active stay queries | check-in | CONFIRMED |

## Night Ledger

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `nocenja.PrijavaOdjava = 0` | open night ledger | active/unclosed night charge period | `Unesinocenja`; continuation inserts | payment, checkout, reports | check-in or billing continuation | CONFIRMED |
| `nocenja.PrijavaOdjava = 1` | closed night ledger | night row closed by checkout/payment cutoff | `Data.OdjavaSobe`, `frmPlacanje.vb` | reports/history | checkout or billing without checkout | CONFIRMED |

## Folio

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `posjetafolio.zakljucen = False` | open folio session | room/PID is active | `addFolio` | check-in/payment/checkout | first active check-in for room | CONFIRMED |
| `posjetafolio.zakljucen = True` | closed folio session | full room checkout closed folio | `Data.OdjavaSobe` | history/reports pending | full checkout | CONFIRMED |

## Expense

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `troskovi.zaklj = 0` | open expense | charge still billable/editable in folio | expense entry paths pending | payment/prep queries | default before payment/checkout | CONFIRMED |
| `troskovi.zaklj = 1` | locked expense | charge billed/closed | `Data.OdjavaSobe`, `frmPlacanje.vb` | reports/history | checkout or payment/invoice | CONFIRMED |
| `troskovi.Djelimicno = 1` | partially paid/split expense | part of expense remains or was split | `frmPlacanje.vb` | pending | partial payment path | PARTIAL |

## Booking

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `rezervacije.prijava = 0` | not checked in | reservation still pending for check-in | reservation forms pending | check-in queries | initial reservation | PARTIAL |
| `rezervacije.prijava = 1` | checked in/converted | reservation has been used for check-in | `frmPrijava1.vb` | reservation views | check-in from reservation | CONFIRMED |
| `rezervacije.potvrda = 1` | confirmed | confirmed booking | reservation forms pending | SQL procedures | confirmation path pending | PARTIAL |
| `rezervacije.stornirana = 1` | cancelled/storno | reservation cancelled | reservation forms pending | SQL procedures | cancellation path pending | PARTIAL |

## Payment / Invoice / Fiscal

| Source Value | Label | Meaning | Set By | Read By | Transition Rule | Status |
|---|---|---|---|---|---|---|
| `placanje.storno = 0` | active payment | payment counts in reports | payment insert | reports/procedures | normal payment | PARTIAL |
| `placanjedetalji.storno = 0` | active payment detail | detail counts in totals | payment detail insert | `Data.pripremaRcuna`, reports | normal payment | PARTIAL |
| `printracuni.storno` | invoice storno marker | invoice snapshot cancellation state | storno path pending | reports/export | UNKNOWN | UNKNOWN |
| `printracuni.fisrac/fisvr/fisIZN` | fiscal receipt attached | fiscalized invoice has fiscal metadata | `frmPlacanje.vb` fiscal update | reports/print | fiscal device success | PARTIAL |

