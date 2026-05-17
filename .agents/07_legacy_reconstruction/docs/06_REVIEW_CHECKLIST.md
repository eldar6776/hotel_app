# REVIEW CHECKLIST

Koristiti ovu checklistu prije nego se prihvati bilo koji dokument nastao iz legacy analize.

## Inventar

- [ ] Popisani su svi relevantni VB.NET fajlovi.
- [ ] Popisane su forme, moduli, klase i event handleri.
- [ ] Veliki/rizicni fajlovi imaju status bar `TARGETED_READ`.
- [ ] Nije tvrdeno da je fajl procitan ako nije.

## SQL I Baza

- [ ] Popisani su svi pronadjeni INSERT/UPDATE/DELETE upiti.
- [ ] Svaki WRITE ima poslovno znacenje.
- [ ] Popisane su implicitne relacije iz SQL-a.
- [ ] Identifikovane su status/flag kolone.
- [ ] Identifikovane su finansijske i datumske kolone.

## Poslovna Pravila

- [ ] Svako pravilo ima stabilan ID.
- [ ] Svako pravilo ima legacy reference.
- [ ] Svako pravilo opisuje ulazne uslove i rezultat.
- [ ] Edge case-ovi nisu preskoceni.
- [ ] Magic numbers/status kodovi su objasnjeni ili oznaceni UNKNOWN.

## Golden Scenarios

- [ ] Svaki P0 tok ima barem jedan golden scenario.
- [ ] Scenario ima pocetno stanje baze.
- [ ] Scenario ima ocekivane promjene po tabelama.
- [ ] Scenario navodi sta ne smije da se desi.
- [ ] Scenario je dovoljno konkretan za automated test.

## Domain Model

- [ ] Booking i Stay nisu pomijesani bez objasnjenja.
- [ ] Nocenja su tretirana kao ledger ako legacy tako radi.
- [ ] Troskovi imaju otvoreno/zakljucano stanje ako legacy tako radi.
- [ ] Racun je snapshot, ne samo trenutni zbir.
- [ ] Storno vraca operativno stanje gdje legacy to radi.

## Task Readiness

- [ ] Svaki task ima legacy referencu.
- [ ] Svaki task ima acceptance scenario.
- [ ] Svaki task ima validation plan.
- [ ] Task ne zavisi od pretpostavke oznacene UNKNOWN.
- [ ] Mock nije prihvacen kao zavrsena integracija.

