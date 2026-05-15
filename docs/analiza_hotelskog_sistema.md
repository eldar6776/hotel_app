# Izvještaj o analizi hotelskog sistema i prijedlozi za modernizaciju

> [!NOTE]
> Ovaj dokument sadrži analizu postojećeg izvornog koda i baze podataka legacy hotelske aplikacije, te pregled potrebnih koraka i funkcionalnosti za prelazak na modernu platformu.

## 1. Da li imamo sve potrebne detalje za novu verziju?

**Da, apsolutno.** Analizom strukture direktorija, Visual Basic (`.vb`) fajlova i MySQL dump datoteke baze podataka (`novaBazaJHotel 20150602 0848.sql`), jasno je da imamo pristup kompletnom funkcionisanju sistema. 

Aplikacija je klasična Desktop WinForms arhitektura. Svi poslovni tokovi (kreiranje rezervacije, obračun poreza, integracija sa bravama/karticama, te logika fiskalizacije) eksplicitno su definisani u bazi i kodu. Zbog toga **možemo bez problema izvršiti "reverse engineering" (reverzni inženjering)** postojeće poslovne logike i prenijeti je u modernu, estetski savremenu arhitekturu.

---

## 2. Postojeći moduli u aplikaciji

Na osnovu relacija u bazi podataka i formi u korisničkom interfejsu, sistem trenutno podržava sljedeće funkcionalne cjeline:

- **Upravljanje sobama i kapacitetima:** Definisanje zgrada, tipova soba, njihovih sadržaja i tarifnih modela (`sobe`, `sobavrsta`, `sobasadrzaji`).
- **CRM - Gosti i Partneri:** Evidencija podataka o gostima, skeniranje identifikacionih dokumenata i vođenje profila agencija/partnera (`gosti`, `partneri`, `gostdokument`).
- **Rezervacije (Booking):** Kompletno vođenje rezervacija - pojedinačnih i grupnih, uz evidentiranje izvora odakle je rezervacija došla (`rezervacije`, `rezervacijegrupe`, `relgostsoba`).
- **Recepcija i Računi (Folio sistem):** Evidentiranje troškova gostiju tokom boravka (noćenje, minibar, restoranske usluge) i izdavanje računa (`folio`, `placanje`, `troskovi`).
- **Fiskalizacija:** Hardverska integracija sa fiskalnim printerima, evidentno kroz `Tring.Fiscal.Driver.dll` i pripadajuće forme za fiskalizaciju.
- **Kartični sistem (Kontrola pristupa):** Vidljiva je hardverska integracija sa sistemom elektronskih brava i programiranjem RFID kartica za goste (Salto/Kard integracije).
- **Osoblje i Održavanje (Housekeeping):** Upravljanje smjenama radnika i logiranje rada sobarica i čišćenja soba (`radnici`, `smjene`, `sobaricalog`).
- **Telefonska centrala:** Bilježenje i naplata telefonskih poziva obavljenih iz soba (`telefonskiimenik`, `telpozivi`).
- **Izvještavanje:** Veoma obiman set izvještaja realizovan preko Crystal Reports i RDLC formata (Knjiga stranih državljana, izvještaji o taksama, statistika hotela).

---

## 3. Šta je potrebno dodati za savremeno hotelsko poslovanje?

Za transformaciju ovog sistema u moderno "Property Management System" (PMS) rješenje koje ispunjava najviše estetske i funkcionalne norme, neophodno je implementirati sljedeće module:

### Arhitektura i Estetika
> [!IMPORTANT]
> Aplikacija se mora prebaciti u **Web/Cloud arhitekturu** (npr. React/Next.js frontend + .NET Core/Node.js backend) sa modernim i responzivnim UI/UX dizajnom. Sučelje mora podržavati mračni režim rada (Dark Mode), dinamičke animacije, grafičke nadzorne ploče (dashboards) i intuitivan "Drag & Drop" interaktivni kalendar.

### Nove funkcionalnosti za savremeno poslovanje:

1. **Channel Manager (Dvosmjerna Sinhronizacija)**
   Potrebna je API integracija sa globalnim distributivnim sistemima i platformama (Booking.com, Airbnb, Expedia). To znači da kada se soba rezerviše preko Booking.com, odmah se zauzima u našem sistemu, a kada se rezerviše na recepciji, raspoloživost na internetu se automatski zatvara.

2. **Self-Service / Mobilna aplikacija za goste**
   - **Online Check-In / Check-Out:** Gosti mogu popuniti podatke prije dolaska i izbjeći gužve na recepciji.
   - **Digital Key (Mobilni ključ):** Otključavanje vrata hotelske sobe putem Bluetooth-a ili NFC-a na pametnom telefonu gosta.
   - **In-Room usluge:** Gosti trebaju preko mobitela ili web portala naručivati hranu, masaže, ili prijavljivati kvarove.

3. **Napredno dinamičko određivanje cijena (Yield & Revenue Management)**
   Umjesto statičnih cjenovnika, potreban je algoritam (ili AI modul) koji obara cijene kada je popunjenost mala i automatski diže cijene u periodima velike potražnje, sezone ili lokalnih događaja.

4. **Integracija Payment Gateway-a**
   Mogućnost autorizacije kreditnih kartica prilikom rezervisanja. Sistem treba sigurno čuvati token kreditne kartice (bez pravih brojeva) radi naplate no-show (nedolazak) situacija ili počinjene štete. Integracije sa servisima kao Stripe, PayPal ili lokalnim bankama.

5. **Housekeeping Mobilna Aplikacija**
   Zasebno mobilno sučelje koje sobarice koriste na tabletima ili telefonima. Čim sobarica završi sobu, pritišće dugme "Očišćeno" što automatski mijenja status sobe na recepciji i omogućava recepciji da ubaci novog gosta bez ikakvih radio-veza ili telefonskih poziva.

6. **Smart Hotel IoT Integracije**
   Moderni sistemi komuniciraju sa senzorima u sobi – automatsko gašenje klime/grijanja kada je soba prazna (radi uštede energije) i upozorenja o otvorenim prozorima.
