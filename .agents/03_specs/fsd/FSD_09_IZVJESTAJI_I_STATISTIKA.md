# FSD 09: Izvještaji i Statistika

## Status analize
- **Fajlovi za analizu:** `frmIzvjestaji.vb`, `frmIzvjestajiDnevni.vb`, `OdjavaReport.vb`, `TelReport.vb`, `ModuleKod.vb` (SQL funkcije)
- **Tabele za analizu:** `printracuni`, `printracunidetalji`, `printracuniavans`, `nocenja`, `relgostsoba`
- **Status:** AUTHORITATIVE
- **Analizirao:** 2026-05-15

## 1. Pregled modula
Modul za izvještavanje i statistiku agregira podatke iz svih ostalih modula (prodaja, recepcija, centrala) kako bi pružio uvid u popunjenost, prihode po različitim kanalima, te zakonske izvještaje.

## 2. Crystal Reports migracija
46 legacy .rpt fajlova se ne prenosi u novi sistem. Zamjena:
- **PDF izvještaji** (računi, predračuni, odjavni listovi): QuestPDF (.NET biblioteka)
- **Dashboard i grafikoni**: Chart.js (frontend)
- **Excel/CSV export**: ClosedXML (Excel) + built-in CSV generator
- **Printanje**: Browser native print (Ctrl+P) za sve što je prikazano na ekranu

## 3. Ključni izvještaji

| Naziv | Tehnologija | Schedule |
|-------|------------|----------|
| Račun / Faktura | QuestPDF | Na zahtjev (print/download) |
| Dnevni promet | Chart.js + tabela | Po zahtjevu + auto 06:00 |
| Knjiga gostiju | HTML tabela → PDF/Excel | Po zahtjevu |
| Sobarica shema | HTML (printabilno) | Po zahtjevu |
| Statistika popunjenosti | Chart.js | Real-time na dashboardu |

## 4. Dashboard KPI (glavna strana)

| KPI | Izvor | Prikaz |
|-----|-------|--------|
| Popunjenost (%) | `fnBrojZauzetihSoba / fnBrojSoba * 100` | Broj + trend graf |
| ADR (Average Daily Rate) | `SUM(placanje) / brojNocenja` | Broj + prošla godina |
| RevPAR | `ADR * popunjenost` | Broj |
| Današnji check-in | `relgostsoba WHERE checkInDate = TODAY` | Broj + lista |
| Današnji check-out | `relgostsoba WHERE checkOutDate = TODAY` | Broj + lista |
| Nenaplaćena potraživanja | SUM(neplaceni) | Broj + upozorenje |
| Slobodne sobe danas | COUNT(sobe) - COUNT(zauzete) | Broj + sprat |

Dashboard je responzivan (radi na mobitelu i desktopu). Nije potrebna posebna mobilna aplikacija.

## 5. Automatsko slanje izvještaja
- Dnevni promet: automatski na email menadžmenta u 06:00 (konfigurabilno)
- Schedule se podešava u admin panelu (Settings → Reports → Schedule)
- Format: PDF attachment + tabela u tijelu emaila

## 6. Turistička zajednica
- Legacy: papirni obrasci + XML export
- Novi sistem: automatska elektronska prijava kroz TZ plugin (dokument: `docs/integracije_prosirenja.md`)
- Papirni obrasci ostaju kao fallback opcija (toggle u admin panelu)

## 7. Poslovna pravila

### 7.1 Storno
Izvještaji filtrirani sa `storno = 0`. Stornirani računi se vide u zasebnoj listi.

### 7.2 No-Show u statistici
Ne broje se u popunjenost. Vode se zasebno kao "izgubljeni prihod".

### 7.3 Gratis noćenja
Označavaju se u bazi (`tarifa = 0`). Ne ulaze u ADR/RevPAR obračun ali se broje u popunjenost.
