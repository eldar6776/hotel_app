# UPITNIK — Odluke potrebne za nastavak razvoja

Odgovori na ova pitanja su potrebna da bih mogao tačno implementirati preostale funkcionalnosti.
Odgovore upiši direktno ispod svakog pitanja.

---

## 1. Multi-currency (T9.4)

**Q1:** Koji exchange rate servis želiš koristiti?
- [ ] Frankfurter API (potpuno free, ECB valute, bez API key — preporučujem za početak)
- [ ] Open Exchange Rates (free tier, 1000 req/mes, treba API key)
- [ ] Fixer.io (free tier, EUR base only, treba API key)
- [ ] Drugi: _______________

**Q2:** Koliko često treba automatski osvježavati kurseve?
- [ ] Dnevno (jednom dnevno, npr. u 06:00 — dovoljno za hotel)
- [ ] Hourly
- [ ] Real-time (svakih 60 sekundi)

**Q3:** Koje valute treba podržati osim EUR?
- [ ] BAM (KM) — obavezno za BiH
- [ ] USD
- [ ] GBP
- [ ] HRK (ili EUR za Hrvatsku)
- [ ] Drugi: _______________

---

## 2. Hotel konfiguracija (za seed podatke)

**Q4:** Naziv hotela? _______________

**Q5:** Hotel code (kratki kod za multi-tenant, npr. "HOTEL1")? _______________

**Q6:** Adresa hotela? _______________

**Q7:** Podrazumjevani PDV stopa? (trenutno 17% — da li je tačno za BiH?)
- [ ] 17% (tačno)
- [ ] Drugo: ____%

**Q8:** Turistička taksa po noćenju? (trenutno 0 — iznosi za BiH?)
- [ ] 0 (nema)
- [ ] Drugo: ____ KM/EUR

---

## 3. Sobe i spratovi (za seed podatke)

**Q9:** Konfiguracija spratova i soba — da li ti odgovara ovaj raspored?

Sprat 1 (Recepcija + 10 soba): 101-110
Sprat 2 (10 soba): 201-210
Sprat 3 (10 soba): 301-310
Sprat 4 (10 soba + Suite): 401-410 + 401S-404S

Tipovi soba:
- Single (1+1, 80 EUR) — 8 soba
- Double/Twin (2+1, 120 EUR) — 16 soba
- Triple (3+1, 160 EUR) — 8 soba
- Suite (2+2, 250 EUR) — 4 sobe
- Apartment (4+2, 350 EUR) — 4 sobe

- [ ] Da, ovako je OK
- [ ] Ne, želim drugačije: _______________

---

## 4. Poslovna pravila

**Q10:** Check-in / Check-out vrijeme?
- [ ] Check-in: 14:00, Check-out: 12:00 (standardno)
- [ ] Check-in: 15:00, Check-out: 11:00
- [ ] Drugo: Check-in ____:____, Check-out ____:____

**Q11:** Late check-out politika?
- [ ] Besplatno do 14:00, poslije 50% cijene po noćenju
- [ ] Besplatno do 15:00, poslije puna cijena
- [ ] Drugo: _______________

**Q12:** Child discount pravila?
- [ ] 0-2 god (Infant): besplatno
- [ ] 3-11 god (Child): 50% popusta
- [ ] 12+ god (Adult): puna cijena
- [ ] Drugo: _______________

**Q13:** No-show politika — da li se naplaćuje prva noć?
- [ ] Da, prva noć se naplaćuje automatski
- [ ] Ne, samo se označi kao NoShow
- [ ] Drugo: _______________

---

## 5. Fiskalni i pravni zahtjevi (BiH)

**Q14:** Da li hotel koristi fiskalni printer (JIR)?
- [ ] Da — koji model/marka? _______________
- [ ] Ne, koristi se samo račun iz sistema
- [ ] Planira se u budućnosti

**Q15:** Da li treba KNJIGA STRANIH DRŽAVLJANA (MUP obrazac)?
- [ ] Da, obavezno — treba export u PDF/XML za MUP
- [ ] Ne treba
- [ ] Nisam siguran

---

## 6. Integracije

**Q16:** Booking.com — da li hotel stvarno koristi Booking.com?
- [ ] Da — imamo hotel ID: _______________
- [ ] Ne, ali planiramo
- [ ] Ne treba

**Q17:** Stripe — da li želiš pravu Stripe integraciju za online plaćanje?
- [ ] Da — imamo Stripe account
- [ ] Ne, plaćanje samo na recepciji (gotovina/kartica)
- [ ] U budućnosti

**Q18:** Email — koji SMTP servis koristiti za slanje potvrda?
- [ ] Gmail SMTP (free, limitiran)
- [ ] SendGrid (free tier 100 emaila/dan)
- [ ] Brevo (free tier 300 emaila/dan)
- [ ] Drugi: _______________
- [ ] Ne treba za sada

---

## 7. Jezici i lokalizacija

**Q19:** Koje jezike treba podržati u interfejsu?
- [ ] Bosanski/hrvatski/srpski (trenutno)
- [ ] Engleski
- [ ] Njemački
- [ ] Turski
- [ ] Drugi: _______________

**Q20:** Valuta prikaza u interfejsu?
- [ ] EUR
- [ ] BAM (KM)
- [ ] Korisnik bira

---

*Nakon što popuniš, javi mi i nastaviću sa implementacijom na osnovu tvojih odgovora.*
*Za sva pitanja na koja ne odgovoriš, koristiću podrazumjevane vrijednosti (označene preporučenim opcijama).*
