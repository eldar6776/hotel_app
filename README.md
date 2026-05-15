# Hotel App

Modernizacija legacy hotelskog informacionog sistema u savremenu web/cloud platformu za upravljanje hotelskim poslovanjem.

## Status projekta

> **Faza:** planiranje i analiza  
> Ovaj repozitorij trenutno sadrži analizu postojećeg sistema, prijedloge arhitekture, funkcionalne zahtjeve i plan proširenja.  
> Implementacija nove aplikacije još nije započeta ili nije objavljena u ovom repozitoriju.

## Cilj projekta

Cilj projekta je transformacija postojećeg legacy hotelskog softvera u moderni **Property Management System (PMS)** koji će podržavati:

- upravljanje sobama i kapacitetima
- evidenciju gostiju i partnera
- rezervacije i grupne rezervacije
- recepcijske procese i folio sistem
- naplatu i fiskalizaciju
- housekeeping i održavanje
- izvještavanje
- integracije sa eksternim servisima i hardverom

## Vizija budućeg sistema

Planirana nova verzija sistema treba biti:

- web-orijentisana i cloud-ready
- modularna i proširiva
- prilagođena radu na desktopu, tabletu i mobilnim uređajima
- spremna za integracije sa:
  - channel manager platformama
  - payment gateway servisima
  - OCR skenerima dokumenata
  - sistemima elektronskih brava / kartica
  - turističkim i zakonskim prijavama gostiju

## Postojeća dokumentacija

Projektna dokumentacija se nalazi u direktoriju `docs/`:

- `docs/analiza_hotelskog_sistema.md` — analiza legacy sistema i prijedlozi za modernizaciju
- `docs/dobre_prakse_hotelski_softver.md` — pregled dobrih praksi i standardnih funkcionalnosti modernih PMS sistema
- `docs/integracije_prosirenja.md` — prijedlozi integracija i proširenja
- `docs/otvorena_pitanja.md` — otvorena pitanja za dalju razradu

## Planirani funkcionalni moduli

Prema dosadašnjoj analizi, novi sistem bi trebao obuhvatiti sljedeće cjeline:

### Core moduli
- upravljanje sobama, tipovima soba i sadržajima
- upravljanje gostima i dokumentima
- rezervacije i raspored smještaja
- check-in / check-out procesi
- folio i naplata troškova
- fiskalizacija
- izvještaji i operativna statistika

### Operativni moduli
- housekeeping
- održavanje / work orders
- grupne rezervacije i eventi
- POS integracija (restoran / bar)
- loyalty program
- analytics i forecasting

### Integracije i proširenja
- OCR skeniranje pasoša i ličnih karata
- turističke prijave gostiju
- channel manager integracije
- payment gateway integracije
- smart room funkcionalnosti
- plugin arhitektura za prilagodbu po hotelu

## Predložena arhitektura

U dokumentaciji se kao ciljna arhitektura navodi prelazak sa legacy desktop pristupa na modernu arhitekturu, npr:

- **Frontend:** React / Next.js
- **Backend:** .NET Core ili Node.js
- **Baza podataka:** relacijska baza (zavisno od finalne odluke)
- **Integracije:** REST API, plugin sistem, event-driven tokovi gdje je potrebno

> Napomena: Tehnološki stack još nije finalno zaključen i podložan je promjenama tokom faze planiranja.

## Trenutno stanje repozitorija

Repozitorij je trenutno primarno namijenjen za:

- analizu legacy sistema
- planiranje nove arhitekture
- definisanje funkcionalnih zahtjeva
- pripremu implementacionih faza

Trenutno ne predstavlja završenu niti produkcionu aplikaciju.

## Naredni koraci

Predloženi naredni koraci:

1. finalizovati scope MVP verzije
2. zaključiti tehnološki stack
3. definisati domenski model i module
4. pripremiti tehničku specifikaciju po fazama
5. očistiti repozitorij od suvišnih binarnih/privremenih fajlova
6. započeti implementaciju osnovnih modula

## Napomene

- Legacy aplikacija se tretira kao referentni izvor poslovne logike i funkcionalnih tokova.
- Dio dokumentacije već pokriva napredne funkcionalnosti koje mogu biti implementirane postepeno.
- Otvorena pitanja i nedorečene stavke evidentirane su u `docs/otvorena_pitanja.md`.

## Autor / vlasništvo

Repozitorij: `eldar6776/hotel_app`
