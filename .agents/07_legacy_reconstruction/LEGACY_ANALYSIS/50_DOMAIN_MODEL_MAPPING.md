# Domain Model Mapping — Legacy Database to Modern Concepts

> This document maps every legacy table and column to modern domain entities, defines status enums, records foreign key relationships, identifies redundant/duplicate columns, flags columns to ignore, and presents a text-based ER diagram.

---

## 1. Modern Domain Entities

### Room

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `sobe` | `ID` | `id` | int | PK |
| `sobe` | `naziv` | `number` | string | Room number/label (e.g. "101") |
| `sobe` | `vrsta` | `roomTypeId` | int | FK → RoomType.id |
| `sobe` | `lokal` | `phoneExtension` | int | Internal phone extension |
| `sobe` | `ooo` | `outOfOrder` | bool | 0=in service, 1=OOO |
| `sobe` | `razlog` | `outOfOrderReason` | string | Reason for OOO |
| `sobe` | `zgradaID` | `buildingId` | int | FK → Building.id |
| `sobe` | `clean` | `isClean` | bool | 1=clean, 0=dirty — overrides status display |
| `sobe` | `tekst` | `tooltipText` | string | Room label text (default "Soba") |
| `sobe` | `idvrsta1` | `altRoomTypeId` | int? | Alternate room type classification |
| `sobe` | `sprat` | `floor` | string | Floor number |
| `sobe` | `idkon` | `controllerTypeId` | int | 0=mechanical key, 1=electronic card |
| `sobe` | `redulko` | `sortOrder` | int | Display sort order |
| `sobe` | computed | `status` | enum RoomStatus | Derived by fnSobaStatus — not stored |
| `sobavrsta` | `ID` | `id` | int | PK |
| `sobavrsta` | `naziv` | `name` | string | e.g. "Prvi sprat", "Single" |
| `sobavrsta` | `brojKreveta` | `bedCount` | int | Number of beds |
| `sobavrsta` | `defaultTarifa` | `defaultTariffId` | int | FK → Tariff.id |
| `sobasadrzaji` | `ID` | `id` | int | PK |
| `sobasadrzaji` | `naziv` | `name` | string | Amenity name |
| `sobasadrzaji` | `defaultTarifa` | `defaultTariffId` | int? | FK → sadrzajtarifa.ID |
| `relsobavrstasadrzaj` | `sobaID` / `sobaSadrzajID` | junction | — | Many-to-many Room ↔ Amenity |
| `relsobavrstasobatarifa` | `sobaVrstaID` / `sobaTarifaID` | junction | — | Many-to-many RoomType ↔ Tariff |
| `zgrade` | `ID` | `id` | int | PK |
| `zgrade` | `naziv` | `name` | string | Building name |
| `zgrade` | `opis` | `description` | string | Building description |

---

### Guest

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `gosti` | `ID` | `id` | int | PK |
| `gosti` | `ime` | `firstName` | string? | May be NULL |
| `gosti` | `prezime` | `lastName` | string | NOT NULL — primary identifier |
| `gosti` | `adresa` | `address` | string? | |
| `gosti` | `datumRodjenja` | `dateOfBirth` | DateTime? | Drives age-based status |
| `gosti` | `pol` | `gender` | string? | "M" / "Z" |
| `gosti` | `drzavljanstvo` | `citizenship` | string? | Free text nationality |
| `gosti` | `dokument` | `documentTypeId` | int? | FK → GuestDocumentType.id |
| `gosti` | `brDokument` | `documentNumber` | string? | |
| `gosti` | `telefon` | `phone` | string? | |
| `gosti` | `mobitel` | `mobile` | string? | |
| `gosti` | `email` | `email` | string? | |
| `gosti` | `mjestodrzavaR` | `placeOfResidence` | string? | |
| `gosti` | `DID` | `countryId` | int? | FK → Country.id |
| `gosti` | `Rid` | `reservationId` | int? | FK → Reservation.id (cleared after check-in) |
| `gostdokument` | `ID` / `naziv` | `id` / `name` | int / string | Lookup: Pasos, Licna, Vozacka, Ostalo |
| `goststatus` | `id` | `id` | int | PK |
| `goststatus` | `naziv` | `name` | string | e.g. "Turist", "Vlasnik kuće" |
| `goststatus` | `del` | `isDeleted` | bool | Soft delete |
| `goststatus` | `taksa` | `taxAmount` | decimal | Tax amount per status |
| `drzave` | `id` | `id` | int | PK |
| `drzave` | `naziv` | `name` | string | Country name |
| `drzave` | `skr` | `abbreviation` | string? | |
| `drzave` | `cod` | `code` | string? | |
| `drzave` | `br` | `?` | int? | |
| `drzave` | `pozbr` | `phonePrefix` | string? | |
| `drzave` | `domaca` | `isDomestic` | bool | 1=domestic country (BiH) |
| `drzave` | `sifra` | `sifra` | string? | Country code |

---

### Reservation

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `rezervacije` | `ID` | `id` | int | PK |
| `rezervacije` | `GID` | `guestId` | decimal? | FK → Guest.id |
| `rezervacije` | `checkInDate` | `checkInDate` | DateTime? | |
| `rezervacije` | `checkOutDate` | `checkOutDate` | DateTime? | |
| `rezervacije` | `potvrda` | `isConfirmed` | bool | 0=unconfirmed, 1=confirmed |
| `rezervacije` | `brojPotvrde` | `confirmationNumber` | int? | MAX+1 — race condition risk |
| `rezervacije` | `blokID` | `groupId` | int | FK → ReservationGroup.id (1=no group) |
| `rezervacije` | `tipID` | `sourceTypeId` | int | FK → ReservationSource.id |
| `rezervacije` | `izvorID` | `originId` | int | FK → ReservationOrigin.id |
| `rezervacije` | `sobaVrstaID` | `roomTypeId` | int? | FK → RoomType.id |
| `rezervacije` | `stornirana` | `isCancelled` | bool | '0'=active, '1'=cancelled — bidirectional toggle |
| `rezervacije` | `brojStorna` | `cancellationNumber` | int? | |
| `rezervacije` | `brojRezSoba` | `roomCount` | int? | Number of reserved rooms |
| `rezervacije` | `godina` | `year` | string? | |
| `rezervacije` | `prijava` | `checkInStatus` | int | 0=reserved, 1=checked-in, 2=expired |
| `rezervacije` | `tarifa` | `tariffId` | int? | FK → Tariff.id |
| `rezervacije` | `memo` | `memo` | text? | |
| `rezervacije` | `radnik` | `workerName` | string? | |
| `rezervacije` | `radnikID` | `workerId` | int? | FK → Worker.id |
| `rezervacije` | `vrijeme` | `createdAt` | DateTime? | |
| `rezervacije` | `gost` | `guestName` | string? | Redundant with GID → Guest |
| `rezervacije` | `napomena` | `note` | text? | |
| `rezervacije` | `alarmid` | `alarmId` | int? | FK → Alarm.id |
| `rezervacie` | `gostgrupa` | `groupName` | string? | |
| `rezervacije` | `promjena` | `changeCount` | int | Incremented on each edit |
| `rezervacije` | `promjenat` | `changeNote` | text? | |
| `rezervacije` | `kontakt` | `contactName` | string? | |
| `rezervacije` | `kontakttel` | `contactPhone` | string? | |
| `kontaktfax` | `contactFax` | string? | |
| `kontaktmob` | `contactMobile` | string? | |
| `kontaktmail` | `contactEmail` | string? | |
| `rezervacije` | `plac` | `paymentMethodId` | int? | |
| `rezervacije` | `placanje` | `paymentMethodName` | string? | Redundant with plac |
| `rezervacije` | `firma` | `companyName` | string? | |
| `rezervacije` | `firmaid` | `companyId` | int? | FK → Partner.id |
| `rezervacije` | `agencija` | `agencyName` | string? | |
| `rezervacije` | `agencijaid` | `agencyId` | int? | FK → Partner.id |
| `rezervacije` | `komerc` | `agentName` | string? | |
| `rezervacije` | `komercid` | `agentId` | int? | FK → Komercijalista.id |
| `rezervacije` | `brosoba` | `adultCount` | int | Default 1 |
| `rezervacije` | `brdjeca` | `childCount` | int | Default 1 |
| `rezervacije` | `dateizmjena` | `lastModifiedAt` | DateTime? | |
| `rezervacije` | `datestorno` | `cancelledAt` | DateTime? | |
| `rezervacije` | `datepotvrda` | `confirmedAt` | DateTime? | |
| `rezervacije` | `razlogst` | `cancellationReason` | string? | |
| — | — | — | — | — |
| `rezervacijasobe` | `id` | `id` | int | PK |
| `rezervacijasobe` | `rezid` | `reservationId` | int? | FK → Reservation.id |
| `rezervacijasobe` | `sobtid` | `roomTypeId` | int? | FK → RoomType.id |
| `rezervacijasobe` | `sobatip` | `roomTypeName` | string? | Redundant with sobtid |
| `rezervacijasobe` | `sid` | `roomId` | int? | FK → Room.id |
| `rezervacijasobe` | `soba` | `roomName` | string? | Redundant with sid |
| `rezervacijasobe` | `tid` | `tariffId` | int? | FK → Tariff.id |
| `rezervacijasobe` | `tarifa` | `tariffAmount` | double? | Redundant with tid |
| `rezervacijasobe` | `gid` | `guestId` | int? | FK → Guest.id |
| `rezervacijasobe` | `gost` | `guestName` | string? | Redundant with gid |
| `rezervacijasobe` | `brgost` | `guestCount` | int? | |
| `rezervacijasobe` | `gost1` | `guestName2` | string? | |
| `rezervacijasobe` | `pusac` | `isSmoker` | int? | 0/1 |
| `rezervacijasobe` | `cjenovnik` | `priceListId` | int? | |
| — | — | — | — | — |
| `rezervacijaprijava` | `ID` | `id` | int | PK |
| `rezervacijaprijava` | `IDrez` | `reservationId` | int? | FK → Reservation.id |
| `rezervacijaprijava` | `IDgost` | `guestId` | int? | FK → Guest.id |
| `rezervacijaprijava` | `sobaID` | `roomId` | int? | FK → Room.id |
| — | — | — | — | — |
| `rezervacijegrupe` | `ID` | `id` | int | PK |
| `rezervacijegrupe` | `naziv` | `name` | string? | |
| `rezervacijegrupe` | `odjavljena` | `isClosed` | bool | 0=active, 1=closed |
| — | — | — | — | — |
| `rezervacijetip` | `ID` / `naziv` | `id` / `name` | int / string | Lookup: 1=Nema podataka |
| `rezervacijeizvor` | `ID` / `naziv` | `id` / `name` | int / string | Lookup: 1=Nema, 2=Mail, 3=Fax, 4=Telefon |

---

### Stay (Check-in/Assignment)

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `relgostsoba` | `ID` | `id` | int | PK — **THE CORE TABLE** |
| `relgostsoba` | `gostID` | `guestId` | decimal | FK → Guest.id |
| `relgostsoba` | `sobaID` | `roomId` | int | FK → Room.id |
| `relgostsoba` | `checkInDate` | `checkInAt` | DateTime | |
| `relgostsoba` | `checkOutDate` | `checkOutAt` | DateTime? | NULL until checkout |
| `relgostsoba` | `checkInRadnik` | `checkedInBy` | int | FK → Worker.id |
| `relgostsoba` | `checkOutRadnik` | `checkedOutBy` | int? | FK → Worker.id |
| `relgostsoba` | `stampanaPrijava` | `isRegistrationPrinted` | bool | 0/1 |
| `relgostsoba` | `odjavljen` | `isCheckedOut` | bool | 0=active stay, 1=checked out |
| `relgostsoba` | `rezervacija` | `isReservationLink` | bool | 0=actual stay (always inserted as 0) |
| `relgostsoba` | `grupaID` | `groupId` | int | Default 1 |
| `relgostsoba` | `brojDana` | `dayCount` | int | Default 0 |
| `relgostsoba` | `tarifaID` | `tariffId` | int? | FK → Tariff.id |
| `relgostsoba` | `popust` | `discountPercent` | double | 0=no discount, or child discount |
| `relgostsoba` | `ostaliTroskovi` | `otherExpenses` | decimal? | |
| `relgostsoba` | `PID` | `folioId` | bigint? | FK → Folio.id |
| `relgostsoba` | `print1` | `isReportR1Printed` | bool? | |
| `relgostsoba` | `print2` | `isReportR2Printed` | bool? | |
| `relgostsoba` | `rezervP` | `isFromConfirmedReservation` | bool | 0=direct, 1=from confirmed reservation |
| `relgostsoba` | `redniBroj` | `orderNumber` | int? | |
| `relgostsoba` | `PopustRazlog` | `discountReason` | string? | |
| `relgostsoba` | `pl` | `isAccommodationPaid` | int | 0=unpaid, 1=paid |
| `relgostsoba` | `napomenapl` | `paymentNote` | text? | |
| `relgostsoba` | `napomena` | `stayNote` | text? | |
| `relgostsoba` | `usluga` | `serviceNote` | text? | |
| `relgostsoba` | `taksa` | `taxOverride` | int? | Guest status tax amount |
| `relgostsoba` | `status` | `guestStatusId` | int | 0=unknown, 1=adult, 3=minor, 4=child |
| `relgostsoba` | `tid` | `touristRegId` | int? | Tourist registration response ID |
| — | — | — | — | — |
| `posjetafolio` | `ID` | `id` | bigint | PK |
| `posjetafolio` | `SID` | `roomId` | int | FK → Room.id |
| `posjetafolio` | `vrijemeD` | `openedAt` | DateTime? | Folio open time (= check-in time) |
| `posjetafolio` | `vrijemeO` | `closedAt` | DateTime? | NULL until checkout |
| `posjetafolio` | `zakljucen` | `isClosed` | bool | 0=open, 1=closed |

---

### Night (Accommodation)

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `nocenja` | `ID` | `id` | int | PK, AUTO_INCREMENT |
| `nocenja` | `RID` | `stayId` | int? | FK → Stay.id (relgostsoba.ID) |
| `nocenja` | `DatumP` | `date` | DateTime? | Night charge date |
| `nocenja` | `DatumOdj` | `dateOut` | DateTime? | Checkout date for this night row |
| `nocenja` | `SID` | `roomId` | int? | FK → Room.id |
| `nocenja` | `PID` | `folioId` | int? | FK → Folio.id |
| `nocenja` | `PrijavaOdjava` | `isClosed` | bool | 0=active, 1=closed at checkout |
| `nocenja` | `Tarifa` | `tariffAmount` | decimal? | Price per night |
| `nocenja` | `popust` | `discountPercent` | int? | |
| `nocenja` | `opis` | `description` | string? | |
| `nocenja` | `soba` | `roomName` | string? | Redundant with SID |

---

### Expense

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `troskovi` | `ID` | `id` | int | PK |
| `troskovi` | `GSID` | `stayId` | int? | FK → Stay.id (relgostsoba.ID) |
| `troskovi` | `SID` | `roomId` | int? | FK → Room.id |
| `troskovi` | `TID` | `expenseTypeId` | int? | FK → ExpenseType.id |
| `troskovi` | `vrijeme` | `timestamp` | DateTime | NOT NULL |
| `troskovi` | `kolicina` | `quantity` | int? | |
| `troskovi` | `iznos` | `amount` | decimal | NOT NULL |
| `troskovi` | `radnikID` | `workerId` | int? | FK → Worker.id (hardcoded to 1 often) |
| `troskovi` | `napomena` | `note` | string? | |
| `troskovi` | `zaklj` | `isLocked` | bool | 0=open, 1=closed/paid |
| `troskovi` | `Brrac` | `receiptNumber` | decimal? | FK → Payment number; NULL if open |
| `troskovi` | `Djelimicno` | `isPartialPayment` | bool? | 0=full, 1=partially paid |
| `troskovi` | `iddzid` | `externalId` | string? | |
| `troskovi` | `idzid` | `externalId2` | string? | |
| `troskovi` | `loc` | `location` | string? | |
| `troskovi` | `zidbr` | `exitNumber` | string? | |
| `troskovi` | `fisbr` | `fiscalNumber` | text? | |
| `troskovi` | `stan` | `statusFlag` | int | Default 0 — meaning unclear |
| `troskovi` | `opis` | `description` | text? | |
| `troskovi` | `fis` | `fiscalSentFlag` | int | Default 0 |
| — | — | — | — | — |
| `troskovivrste` | `ID` | `id` | decimal | PK |
| `troskovivrste` | `naziv` | `name` | string | e.g. "Nocenje", "Mini Bar" |
| `troskovivrste` | `cijenaID` | `priceId` | int? | FK → sifarnik or pricing |
| `troskovivrste` | `tip` | `category` | int | 0=one-time expense, 1=per-night (accommodation) |
| `troskovivrste` | `del` | `isDeleted` | bool | Soft delete |
| — | — | — | — | — |
| `troskovipojedinacni` | `Auto` | `id` | bigint | PK |
| `troskovipojedinacni` | `IDtroska` | `expenseId` | bigint | FK → Expense.id |
| `troskovipojedinacni` | `datum` | `date` | DateTime? | |
| `troskovipojedinacni` | `iznos` | `amount` | decimal? | Individual cost item amount |

---

### Payment

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `placanje` | `ID` | `id` | int | PK |
| `placanje` | `broj` | `receiptNumber` | decimal | Business receipt number (MAX+1 race condition) |
| `placanje` | `relGostSobaID` | `stayId` | decimal | FK → Stay.id |
| `placanje` | `iznos` | `amount` | decimal | Default 0.0000 |
| `placanje` | `popust` | `discount` | decimal? | |
| `placanje` | `datum` | `date` | DateTime | NOT NULL |
| `placanje` | `nacin` | `paymentMethodId` | int | FK → PaymentMethod.id |
| `placanje` | `radnikID` | `workerId` | int | FK → Worker.id |
| `placanje` | `naziv` | `description` | string? | Up to 4000 chars |
| `placanje` | `PID` | `folioId` | decimal | FK → Folio.id |
| `placanje` | `uplaceno` | `amountPaid` | decimal | |
| `placanje` | `brdana` | `nightCount` | int | |
| `placanje` | `datumOD` | `periodFrom` | DateTime? | |
| `placanje` | `datumDO` | `periodTo` | DateTime? | |
| `placanje` | `placanjeID` | `paymentLinkId` | int? | Default 1 |
| `placanje` | `poslovna` | `businessUnit` | string? | |
| `placanje` | `storno` | `isCancelled` | bool | 0=active, 1=storno |
| `placanje` | `folio` | `folioNumber` | int? | |
| `placanje` | `idgost` | `guestId` | int? | FK → Guest.id |
| `placanje` | `predracun` | `proformaId` | int? | FK → ProformaInvoice.id |
| `placanje` | `posjeta` | `visitId` | int? | |
| `placanje` | `firma` | `companyId` | int? | |
| `placanje` | `tip` | `type` | int? | Default 0 |
| `placanje` | `racn` | `invoiceRef` | string? | |
| `placanje` | `racime` | `invoiceName` | string? | |
| `placanje` | `pdv` | `isVatRegistrant` | int | 0=exempt, 1=standard PDV |
| `placanje` | `ctax` | `isTouristTaxApplied` | int | 0=no, 1=yes |
| `placanje` | `sobar` | `roomList` | string? | |
| `placanje` | `perio` | `period` | string? | |
| `placanje` | `veza` | `link` | string? | |
| `placanje` | `napom` | `noteShort` | text? | |
| `placanje` | `napokraj` | `noteEnd` | text? | |
| `placanje` | `napomena` | `note` | text? | |
| `placanje` | `fiskalni` | `fiscalNumber` | string? | |
| `placanje` | `fiskal` | `fiscalSent` | int | Default 0 |
| `placanje` | `fiskalizn` | `fiscalAmount` | string? | |
| `placanje` | `fiskalvr` | `fiscalTime` | string? | |
| `placanje` | `fiskalrek` | `fiscalReceipt` | int | Default 0 |
| `placanje` | `fiskalnrekvr` | `fiscalReceiptTime` | string? | |
| `placanje` | `placnaz` | `paymentName` | string? | |
| `placanje` | `uplatetex` | `paymentText` | string? | |
| `placanje` | `hotelid` | `hotelId` | string? | |
| `placanje` | `idd` | `uniqueId` | string? | |
| — | — | — | — | — |
| `placanjedetalji` | `ID` | `id` | int | PK |
| `placanjedetalji` | `brojid` | `paymentHeaderId` | int? | FK → Payment.id |
| `placanjedetalji` | `art` | `articleType` | int? | 1=accommodation |
| `placanjedetalji` | `kolicina` | `quantity` | double? | |
| `placanjedetalji` | `cijena` | `unitPrice` | decimal | |
| `placanjedetalji` | `iznos` | `totalAmount` | decimal | |
| `placanjedetalji` | `napomena` | `note` | string? | |
| `placanjedetalji` | `brojNocenja` | `nightCount` | int? | |
| `placanjedetalji` | `PID` | `folioId` | int? | |
| `placanjedetalji` | `storno` | `isCancelled` | bool? | |
| `placanjedetalji` | `ranijeUplate` | `isPreviousPayment` | bool? | |
| `placanjedetalji` | `rid` | `stayId` | int? | |
| `placanjedetalji` | `sid` | `roomId` | int? | |
| `placanjedetalji` | `gid` | `guestId` | int? | |
| `placanjedetalji` | `soba` | `roomName` | string? | |
| `placanjedetalji` | `sobavr` | `roomTypeName` | string? | |
| `placanjedetalji` | `sobavrid` | `roomTypeId` | string? | |
| `placanjedetalji` | `periodod` | `periodFrom` | DateTime? | |
| `placanjedetalji` | `perioddo` | `periodTo` | DateTime? | |
| `placanjedetalji` | `period` | `periodText` | string? | |
| `placanjedetalji` | `ime` | `guestName` | text? | |
| `placanjedetalji` | `usluga` | `serviceName` | text? | |
| `placanjedetalji` | `popust` | `discount` | double? | |
| `placanjedetalji` | `pdv` | `vatAmount` | double? | |
| `placanjedetalji` | `hotelid` | `hotelId` | string? | |
| `placanjedetalji` | `idd` | `uniqueId` | string? | |
| — | — | — | — | — |
| `placanjenacin` | `ID` | `id` | decimal | PK |
| `placanjenacin` | `nacin` | `name` | string | Cash, Virman, Kartica, Gratis, Slozeno |
| `placanjenacin` | `konto` | `accountCode` | string? | |
| `placanjenacin` | `partner` | `partnerCode` | string? | |
| — | — | — | — | — |
| `placanjeslozeno` | `id` | `id` | int | PK |
| `placanjeslozeno` | `rbr` | `lineNumber` | int | |
| `placanjeslozeno` | `nacin` | `paymentMethodId` | int | FK → PaymentMethod.id |
| `placanjeslozeno` | `iznos` | `amount` | double | |

---

### Invoice

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `printracuni` | `BrojRacuna` | `id` | int | PK, AUTO_INCREMENT |
| `printracuni` | `Poslovna` | `businessUnit` | string? | |
| `printracuni` | `Ime` | `guestName` | string? | |
| `printracuni` | `DrugoIme` | `guestName2` | string? | |
| `printracuni` | `PeriodOd` | `periodFrom` | string? | char(10) date format |
| `printracuni` | `PeriodDo` | `periodTo` | string? | char(10) date format |
| `printracuni` | `TipPlacanja` | `paymentType` | string? | |
| `printracuni` | `BrojSobe` | `roomNumber` | string? | |
| `printracuni` | `storno` | `isCancelled` | bool | 0=active, 1=storno |
| `printracuni` | `fisrac` | `fiscalReceipt` | string? | |
| `printracuni` | `fisvr` | `fiscalTime` | string? | |
| `printracuni` | `fisizn` | `fiscalAmount` | string? | |
| `printracuni` | `racin` | `invoiceNumber` | string? | |
| `printracuni` | `napo` | `note` | text? | |
| `printracuni` | `datr` | `dateText` | string? | |
| `printracuni` | `peri` | `periodText` | string? | |
| `printracuni` | `rad` | `workerName` | string? | |
| `printracuni` | `dat` | `createdAt` | DateTime? | |
| `printracuni` | `knj` | `isBooked` | int? | Default 0 |
| `printracuni` | `printime` | `printTime` | string? | |
| `printracuni` | `kid` | `companyId` | int? | |
| `printracuni` | `exp` | `exportStatus` | int | 0=normal, 2=cancelled, 3=exported |
| `printracuni` | `datstor` | `cancelledAt` | DateTime? | |
| — | — | — | — | — |
| `printracunidetalji` | `ID` | `id` | int | PK |
| `printracunidetalji` | `BrojRacuna` | `invoiceId` | int? | FK → Invoice.id |
| `printracunidetalji` | `Trosak` | `expenseName` | string? | |
| `printracunidetalji` | `Kol` | `quantity` | string? | char(10) |
| `printracunidetalji` | `CijBezPdv` | `priceExVat` | string? | char(10) |
| `printracunidetalji` | `UkupnoBezPdv` | `totalExVat` | string? | |
| `printracunidetalji` | `Pdv` | `vatRate` | string? | char(10) |
| `printracunidetalji` | `IznosPdv` | `vatAmount` | string? | |
| `printracunidetalji` | `Ukupno` | `total` | string? | |
| `printracunidetalji` | `Nacin` | `paymentMethod` | string? | |
| `printracunidetalji` | `Valuta` | `currency` | string? | char(10) |
| `printracunidetalji` | `OznakaValute` | `currencyCode` | string? | |
| `printracunidetalji` | `Popust` | `discount` | int? | Default 0 |
| `printracunidetalji` | `popust1` | `discountText` | string? | |
| `printracunidetalji` | `razlogp` | `discountReason` | string? | |
| `printracunidetalji` | `pop` | `discountFlag` | string? | |
| `printracunidetalji` | `trosakId` | `expenseTypeId` | int? | |
| `printracunidetalji` | `nacinid` | `paymentMethodId` | int? | |
| — | — | — | — | — |
| `printracunifooter` | `ID` | `id` | int | PK |
| `printracunifooter` | `BrojRacuna` | `invoiceId` | int? | FK → Invoice.id |
| `printracunifooter` | `Avansno` | `advanceAmount` | decimal | Default 0 |
| `printracunifooter` | `Nocenja` | `nightCount` | decimal | Default 0 |
| `printracunifooter` | `nap` | `note` | text? | |
| `printracunifooter` | `pri` | `total` | int? | |
| — | — | — | — | — |
| `printracuniavans` | `BrojRacuna` | `id` | int | PK |
| `printracuniavans` | `Poslovna` | `businessUnit` | string? | |
| `printracuniavans` | `Ime` | `guestName` | string? | |
| `printracuniavans` | `DrugoIme` | `guestName2` | string? | |
| `printracuniavans` | `PeriodOd` | `periodFrom` | string? | |
| `printracuniavans` | `PeriodDo` | `periodTo` | string? | |
| `printracuniavans` | `TipPlacanja` | `paymentType` | string? | |
| `printracuniavans` | `BrojSobe` | `roomNumber` | string? | |
| `printracuniavans` | `storno` | `isCancelled` | bool | 0=active, 1=storno |
| `printracuniavans` | `folio` | `folioId` | int? | |
| `printracuniavans` | `idgost` | `guestId` | int? | |
| `printracuniavans` | `predracun` | `proformaId` | int? | |
| `printracuniavans` | `posjeta` | `visitId` | int? | |
| (misc columns) | — | — | — | Advance invoice snapshot data, similar to printracuni |

---

### Partner/Agency

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `partneri` | `id` | `id` | int | PK |
| `partneri` | `naziv` | `name` | string? | |
| `partneri` | `mjesto` | `city` | string? | |
| `partneri` | `posta` | `postalCode` | string? | |
| `partneri` | `ulica` | `street` | string? | |
| `partneri` | `zemlja` | `country` | string? | |
| `partneri` | `telefon` | `phone` | string? | |
| `partneri` | `fax` | `fax` | string? | |
| `partneri` | `email` | `email` | string? | |
| `partneri` | `www` | `website` | string? | |
| `partneri` | `zrac1` | `bankAccount1` | string? | |
| `partneri` | `zrac2` | `bankAccount2` | string? | |
| `partneri` | `zrac3` | `bankAccount3` | string? | |
| `partneri` | `zrac4` | `bankAccount4` | string? | |
| `partneri` | `pdv` | `vatNumber` | string? | |
| `partneri` | `idd` | `registrationNumber` | string? | |
| `partneri` | `rjesenje` | `registrationDoc` | string? | |
| `partneri` | `kosoba` | `contactPerson` | string? | |
| `partneri` | `rabat` | `discount` | int? | |
| `partneri` | `brdanodg` | `responseDays` | int? | |
| `partneri` | `vr_upis` | `createdAt` | DateTime? | |
| `partneri` | `filter` | `filterTag` | string? | |
| `partneri` | `napomena` | `note` | string? | |
| `partneri` | `sifra1` | `code` | int? | |
| `partneri` | `sifraex` | `externalCode` | string? | |
| — | — | — | — | — |
| `komercijalista` | `id` | `id` | int | PK |
| `komercijalista` | `gostiju` | `guestCount` | int? | Default 0 |
| `komercijalista` | `ime` | `firstName` | string? | |
| `komercijalista` | `prezime` | `lastName` | string? | |
| `komercijalista` | `telefon` | `phone` | string? | |
| `komercijalista` | `mobitel` | `mobile` | string? | |
| `komercijalista` | `napomena` | `note` | string? | |
| `komercijalista` | `tarifaid` | `tariffId` | int? | |
| `komercijalista` | `cjenovnik` | `priceListId` | int? | |

---

### Tariff/Price

| Legacy Table | Legacy Column | Modern Attribute | Type | Notes |
|-------------|---------------|-----------------|------|-------|
| `sobatarifa` | `ID` | `id` | int | PK |
| `sobatarifa` | `naziv` | `amount` | decimal(18,2) | **NOT a name — stores the tariff PRICE** |
| `sobatarifa` | `naziv2` | `name` | string? | Actual tariff name/label |
| `sobatarifa` | `uslov` | `condition` | string? | |
| `sobatarifa` | `del` | `isDeleted` | bool | Soft delete |
| — | — | — | — | — |
| `sadrzajtarifa` | `ID` | `id` | int | PK |
| `sadrzajtarifa` | `naziv` | `name` | string? | |
| `sadrzajtarifa` | `uslov` | `condition` | string? | |

---

## 2. Status Enum Mappings

### RoomStatus

| Enum Value | Int | Legacy Meaning | Legacy Source | Modern Meaning |
|------------|-----|----------------|---------------|----------------|
| `Free` | 0 | SLOBODNA | fnSobaStatus: no guests, no reservation | Room available for check-in |
| `Occupied` | 1 | ZAUZETA | fnSobaStatus: active guests, odjavljen=0, rezervacija=0 | Room has active guests |
| `Departing` | 2 | ZAUZETA (departing) | fnSobaStatus: guests with checkout ≤ today | Guest should check out today |
| `ReservedConfirmed` | 3 | REZERVISANA - potvrdjeno | fnSobaStatus: confirmed reservation, no guests | Confirmed reservation, room empty |
| `OccupiedReserved` | 4 | ZAUZETA i REZERVISANA | fnSobaStatus: confirmed reservation + active guests | Room occupied AND has reservation |
| `OutOfOrder` | 5 | VAN UPOTREBE | fnSobaStatus: sobe.ooo=1 | Room out of service |
| `ReservedUnconfirmed` | 6 | REZERVISANA - nepotvrdjeno | fnSobaStatus: unconfirmed reservation, no guests | Unconfirmed reservation |
| `NotClean` | 7 | NIJE SPREMNA | UI override: sobe.clean=0 | Room needs housekeeping |

### ReservationStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Reserved` | 0 | `rezervacije.prijava` | Not checked in | Normal reservation |
| `CheckedIn` | 1 | `rezervacije.prijava` | Guest checked in | Reservation realized |
| `Expired` | 2 | `rezervacije.prijava` | Auto-expired past date | Reservation expired |

### ReservationConfirmation

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Unconfirmed` | 0 | `rezervacije.potvrda` | Not confirmed | Awaiting confirmation |
| `Confirmed` | 1 | `rezervacije.potvrda` | Confirmed | Reservation confirmed |

### ReservationCancellation

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `rezervacije.stornirana` | Not cancelled | Active reservation |
| `Cancelled` | 1 | `rezervacije.stornirana` | Cancelled/storno | Cancelled reservation |

### StayStatus (relgostsoba)

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `relgostsoba.odjavljen` | Checked in, active | Guest is in house |
| `CheckedOut` | 1 | `relgostsoba.odjavljen` | Checked out | Guest has departed |

### GuestCategory (relgostsoba.status)

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Unknown` | 0 | `relgostsoba.status` | Default/unknown | No age info |
| `Adult` | 1 | `relgostsoba.status` | Adult (≥18) | Standard rate |
| `Minor` | 3 | `relgostsoba.status` | Minor (<18, ≥12) | Reduced rate |
| `Child` | 4 | `relgostsoba.status` | Child (<12) | Child rate with auto-discount |

### FolioStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Open` | 0 | `posjetafolio.zakljucen` | Folio open for charges | Active stay billing |
| `Closed` | 1 | `posjetafolio.zakljucen` | Folio closed | Checkout complete |

### NightStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `nocenja.PrijavaOdjava` | Active accommodation charge | Night charge pending |
| `Closed` | 1 | `nocenja.PrijavaOdjava` | Closed at checkout | Night charge settled |

### ExpenseLockStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Open` | 0 | `troskovi.zaklj` | Open/active expense | Charge not yet paid |
| `Closed` | 1 | `troskovi.zaklj` | Closed/paid | Charge settled |

### ExpensePartialPaymentStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Full` | 0 | `troskovi.Djelimicno` | Not partially paid | Full amount pending or settled |
| `Partial` | 1 | `troskovi.Djelimicno` | Partially paid | Partial payment recorded |

### ExpenseCategory

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `OneTime` | 0 | `troskovivrste.tip` | Regular expense | Mini bar, restaurant, etc. |
| `PerNight` | 1 | `troskovivrste.tip` | Accommodation type | "Nocenje sa doruckom" |

### PaymentMethod

| Enum Value | Int | Legacy Source | Modern Meaning |
|------------|-----|--------------|----------------|
| `Cash` | 1 | `placanjenacin` | Gotovina |
| `Transfer` | 2 | `placanjenacin` | Virman (bank transfer) |
| `Card` | 3 | `placanjenacin` | Kartica (credit/debit card) |
| `Gratis` | 4 | `placanjenacin` | Gratis (complimentary) |
| `Compound` | 5 | `placanjenacin` | Slozeno (split/compound payment) |

### PaymentStornoStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `placanje.storno` | Normal payment | Active payment |
| `Cancelled` | 1 | `placanje.storno` | Storno/cancelled | Payment reversed |

### InvoiceStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `printracuni.storno` | Normal invoice | Active invoice |
| `Cancelled` | 1 | `printracuni.storno` | Stornirano | Invoice cancelled |

### InvoiceExportStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Normal` | 0 | `printracuni.exp` | Not exported | Local invoice only |
| `CancelledPendingExport` | 2 | `printracuni.exp` | Storno, awaiting export | Cancelled, pending fiscal export |
| `Exported` | 3 | `printracuni.exp` | Processed/exported | Fiscal device confirmed |

### AdvanceInvoiceStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `printracuniavans.storno` | Normal advance invoice | Active advance |
| `Cancelled` | 1 | `printracuniavans.storno` | Storno | Advance invoice cancelled |

### RoomCleanStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Clean` | 1 | `sobe.clean` | Clean | Room ready for guest |
| `Dirty` | 0 | `sobe.clean` | Dirty/needs cleaning | Room needs housekeeping |

### RoomOutOfOrderStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `InService` | 0 | `sobe.ooo` | Available | Room available |
| `OutOfOrder` | 1 | `sobe.ooo` | VAN UPOTREBE | Room out of service |

### ReservationGroupStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `rezervacijegrupe.odjavljena` | Active group | Group active |
| `Closed` | 1 | `rezervacijegrupe.odjavljena` | Closed group | Group closed |

### AlarmStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `alarm.storno` | Active alarm | Reminder active |
| `Cancelled` | 1 | `alarm.storno` | Cancelled alarm | Reminder dismissed |

### UnpaidStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Unpaid` | 0 | `neplaceni.placeno` | Not yet paid | Outstanding balance |
| `Paid` | 1 | `neplaceni.placeno` | Paid | Balance settled |

### WorkerDisabledStatus

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Active` | 0 | `radnici.disabled` | Active worker | Can log in |
| `Disabled` | 1 | `radnici.disabled` | Disabled worker | Cannot log in |

### WorkerAccessLevel

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `Limited` | 3 | `radnici.nivo` | Reservation-only access | Minimal access |
| `Standard` | 5 | `radnici.nivo` | Standard receptionist | Normal access |
| `Admin` | 9 | `radnici.nivo` | Super admin | Full access |

### BillingMode

| Enum Value | Int | Legacy Field | Legacy Meaning | Modern Meaning |
|------------|-----|-------------|----------------|----------------|
| `SplitPerPerson` | 0 | `setings.naplposo` | Tariff divided by guest count | Per-person split billing |
| `FullPerPerson` | 1 | `setings.naplposo` | Full tariff per person | Each guest pays full rate |

---

## 3. Relationships (Foreign Keys to Modern Navigation)

| Legacy FK | Source Table | Source Column | Target Table | Target Column | Modern Relationship | Notes |
|-----------|-------------|---------------|--------------|---------------|---------------------|-------|
| FK1 | `relgostsoba` | `gostID` | `gosti` | `ID` | Stay → Guest | Many stays per guest |
| FK2 | `relgostsoba` | `sobaID` | `sobe` | `ID` | Stay → Room | Many stays per room (historical) |
| FK3 | `relgostsoba` | `checkInRadnik` | `radnici` | `ID` | Stay → Worker (check-in) | |
| FK4 | `relgostsoba` | `checkOutRadnik` | `radnici` | `ID` | Stay → Worker (check-out) | Nullable |
| FK5 | `relgostsoba` | `PID` | `posjetafolio` | `ID` | Stay → Folio | Groups stays for billing |
| FK6 | `relgostsoba` | `tarifaID` | `sobatarifa` | `ID` | Stay → Tariff | |
| FK7 | `nocenja` | `RID` | `relgostsoba` | `ID` | Night → Stay | |
| FK8 | `nocenja` | `SID` | `sobe` | `ID` | Night → Room | |
| FK9 | `nocenja` | `PID` | `posjetafolio` | `ID` | Night → Folio | |
| FK10 | `troskovi` | `GSID` | `relgostsoba` | `ID` | Expense → Stay | |
| FK11 | `troskovi` | `SID` | `sobe` | `ID` | Expense → Room | |
| FK12 | `troskovi` | `TID` | `troskovivrste` | `ID` | Expense → ExpenseType | |
| FK13 | `placanje` | `relGostSobaID` | `relgostsoba` | `ID` | Payment → Stay | |
| FK14 | `placanje` | `PID` | `posjetafolio` | `ID` | Payment → Folio | |
| FK15 | `placanje` | `nacin` | `placanjenacin` | `ID` | Payment → PaymentMethod | |
| FK16 | `placanje` | `radnikID` | `radnici` | `ID` | Payment → Worker | |
| FK17 | `posjetafolio` | `SID` | `sobe` | `ID` | Folio → Room | |
| FK18 | `sobe` | `vrsta` | `sobavrsta` | `ID` | Room → RoomType | |
| FK19 | `sobe` | `zgradaID` | `zgrade` | `ID` | Room → Building | |
| FK20 | `gosti` | `DID` | `drzave` | `id` | Guest → Country | |
| FK21 | `gosti` | `dokument` | `gostdokument` | `ID` | Guest → DocumentType | |
| FK22 | `placanjedetalji` | `brojid` | `placanje` | `ID` | PaymentDetail → Payment | |
| FK23 | `printracunidetalji` | `BrojRacuna` | `printracuni` | `BrojRacuna` | InvoiceLineItem → Invoice | |
| FK24 | `printracunifooter` | `BrojRacuna` | `printracuni` | `BrojRacuna` | InvoiceFooter → Invoice | |
| FK25 | `printracuniavans` | `folio` | `posjetafolio` | `ID` | AdvanceInvoice → Folio | |
| FK26 | `printracuniavans` | `idgost` | `gosti` | `ID` | AdvanceInvoice → Guest | |
| FK27 | `rezervacije` | `GID` | `gosti` | `ID` | Reservation → Guest | Decimal type — odd |
| FK28 | `rezervacije` | `sobaVrstaID` | `sobavrsta` | `ID` | Reservation → RoomType | |
| FK29 | `rezervacije` | `tipID` | `rezervacijetip` | `ID` | Reservation → ReservationType | |
| FK30 | `rezervacije` | `izvorID` | `rezervacijeizvor` | `ID` | Reservation → ReservationSource | |
| FK31 | `rezervacije` | `radnikID` | `radnici` | `ID` | Reservation → Worker | |
| FK32 | `rezervacije` | `blokID` | `rezervacijegrupe` | `ID` | Reservation → Group | |
| FK33 | `rezervacijasobe` | `rezid` | `rezervacije` | `ID` | ReservationRoom → Reservation | |
| FK34 | `rezervacijasobe` | `sid` | `sobe` | `ID` | ReservationRoom → Room | |
| FK35 | `rezervacijasobe` | `gid` | `gosti` | `ID` | ReservationRoom → Guest | |
| FK36 | `rezervacijaPrijava` | `IDrez` | `rezervacije` | `ID` | CheckInMapping → Reservation | |
| FK37 | `rezervacijaPrijava` | `IDgost` | `gosti` | `ID` | CheckInMapping → Guest | |
| FK38 | `rezervacijaPrijava` | `sobaID` | `sobe` | `ID` | CheckInMapping → Room | |
| FK39 | `relsobavrstasadrzaj` | `sobaID` | `sobe` | `ID` | RoomAmenity → Room | |
| FK40 | `relsobavrstasadrzaj` | `sobaSadrzajID` | `sobasadrzaji` | `ID` | RoomAmenity → Amenity | |
| FK41 | `relsobavrstasobatarifa` | `sobaVrstaID` | `sobavrsta` | `ID` | RoomTypeTariff → RoomType | |
| FK42 | `relsobavrstasobatarifa` | `sobaTarifaID` | `sobatarifa` | `ID` | RoomTypeTariff → Tariff | |
| FK43 | `neplaceni` | `PID` | `posjetafolio` | `ID` | Unpaid → Folio | |
| FK44 | `neplaceni` | `SID` | `sobe` | `ID` | Unpaid → Room | |
| FK45 | `neplaceni` | `TID` | `troskovivrste` | `ID` | Unpaid → ExpenseType | |
| FK46 | `kard` | `sobaricaid` | `kontroler` | `id` | KeyCard → Controller | |
| FK47 | `logcont` | `idkont` | `kontroler` | `id` | ControllerLog → Controller | |
| FK48 | `logradnici` | `radnikID` | `radnici` | `ID` | WorkerLog → Worker | |
| FK49 | `smjene` | `radnik` | `radnici` | `ID` | Shift → Worker | |
| FK50 | `sobaricalog` | `sobaricaid` | `kontroler` | `id` | KeyCardLog → Controller | |

---

## 4. Redundant/Duplicate Columns

These columns store data that is also derivable from FK relationships or duplicated across tables:

| Redundant Column | Table(s) | Also Available Via | Notes |
|-----------------|----------|-------------------|-------|
| `gost` (guest name) | `rezervacije`, `rezervacijasobe` | `gosti.ID` → `gosti.ime` + `gosti.prezime` | Denormalized for display |
| `soba` (room name) | `rezervacijasobe`, `nocenja` | `sobe.ID` → `sobe.naziv` | Denormalized for display |
| `soba` (room name) | `troskovi` (as `loc`) | `sobe.ID` → `sobe.naziv` | Stored in `loc` column |
| `sobatip` (room type name) | `rezervacijasobe` | `sobavrsta.ID` → `sobavrsta.naziv` | Denormalized |
| `tarifa` (tariff amount) | `rezervacijasobe` | `sobatarifa.ID` → `sobatarifa.naziv` | Amount stored as double |
| `tarifaID` in relgostsoba | `relgostsoba` | Redundant with `sobe.vrsta` → `sobavrsta.defaultTarifa` | May differ per stay |
| `gost1` (second guest name) | `rezervacijasobe` | Second `gid` → `gosti` | Partial redundancy |
| `brgost` (guest count) | `rezervacijasobe` | Count of `relgostsoba` rows | Can be computed |
| `brosoba` (adult count) | `rezervacije` | Count of reserved rooms | Can be computed |
| `brdjeca` (child count) | `rezervacije` | Count from `relgostsoba.status` | Can be computed |
| `placanje` (payment method name) | `rezervacije` | `plac` → `placanjenacin.nacin` | Denormalized |
| `firma` (company name) | `rezervacije`, `placanje` | `firmaid` → `partneri.naziv` | Denormalized |
| `agencija` (agency name) | `rezervacije` | `agencijaid` → `partneri.naziv` | Denormalized |
| `komerc` (agent name) | `rezervacije` | `komercid` → `komercijalista.ime+prezime` | Denormalized |
| `radnik` (worker name) | `rezervacije` | `radnikID` → `radnici.ime` | Denormalized |
| `relgostsoba.PID = 0` | `relgostsoba` | Missing FK → `posjetafolio` | Used as "no folio" sentinel (separate query pattern) |
| `placanje.naziv` | `placanje` | Computed from details | Payment description text (4000 chars) |
| `placanje.sobar` | `placanje` | `relGostSobaID` → room | Room list text |
| `placanje.perio` | `placanje` | `datumOD`/`datumDO` | Period text representation |
| `placanje.placnaz` | `placanje` | `nacin` → `placanjenacin.nacin` | Denormalized method name |
| `placanje.hotelid` | `placanje` | `setings` | Hotel identifier (multi-hotel) |
| `placanjedetalji.soba` | `placanjedetalji` | `sid` → `sobe.naziv` | Denormalized |
| `placanjedetalji.sobavr` | `placanjedetalji` | `sobavrid` → `sobavrsta.naziv` | Denormalized |
| `placanjedetalji.ime` | `placanjedetalji` | `gid` → `gosti.ime+prezime` | Denormalized |
| `placanjedetalji.usluga` | `placanjedetalji` | `art` + lookup | Denormalized service name |
| `printracuni.Ime` / `DrugoIme` | `printracuni` | `idgost` → `gosti` | Snapshot denormalization |
| `printracuni.BrojSobe` | `printracuni` | room lookup | Snapshot denormalization |
| `printracuniavans` (many columns) | `printracuniavans` | Various FKs | Entire table is a snapshot |
| `rac` (entire table) | `rac` | Various FKs | Alternate invoice table with all denormalized columns |
| `predracuni` + `predracunidet` | `predracuni` | Various FKs | Proforma invoice with denormalized columns |
| `setings.sobekuc` / `setings.sobegrupa` | `setings` | Housekeeping config | Semicolon-delimited compound values |
| `troskovi.loc` | `troskovi` | `SID` → `sobe.naziv` | Room name stored separately |
| Year-based DB separation | Multiple tables | `ConnStr` year suffix | Entire database can switch per year — `godine` and `racunigod` tables manage per-year data |

---

## 5. Columns to Ignore in Modern System

| Column | Table | Reason to Ignore |
|--------|-------|-----------------|
| `idd` | `placanje`, `placanjedetalji`, `rezervacije`, `rezervacijasobe`, `sobaricalog`, `kard`, `komercijalista`, `partneri` | Opaque unique ID string — generated locally, no business meaning; likely GUID-style identity for offline sync |
| `promjena`, `promjenat` | `rezervacijasobe` | Change counter and change note — not used in modern audit (use proper audit log instead) |
| `pom`, `pom1` | `rezervacijasobe`, `komercijalista` | Placeholder/helper columns with no business logic |
| `d1`, `d2`, `d3` | `predracuni`, `printracuni`, `partneri` | Generic string/int columns — no defined purpose |
| `sifra1` | `predracuni`, `printracuni`, `partneri` | Generic code column — no defined purpose |
| `filter` | `partneri` | UI filter string without business meaning |
| `racn`, `racime` | `placanje` | Invoice number references in payment — duplicate of `broj` |
| `napokraj` | `placanje` | Notes at end of receipt — transient print formatting |
| `uplatetex` | `placanje` | Payment text — transient |
| `fiskalni`, `fiskalizn`, `fiskalvr`, `fiskalrek`, `fiskalnrekvr` | `placanje` | Fiscal device response fields — replace with proper integration table |
| `fisc` | (standalone table) | Heap table with no PK — fiscal receipt number tracking; replace with proper sequence |
| `fisbr`, `fis` | `troskovi` | Fiscal device flags — modernize with integration |
| `stan` | `troskovi` | Always 0, no known business logic |
| `poslovna` | `placanje` | Business unit identifier — move to settings |
| `racin` | `printracuni` | Internal invoice reference — no business meaning |
| `printime` | `printracuni` | Print timestamp string — derive from `dat` |
| `kid` | `printracuni` | Unknown reference — appears unused |
| `textpr`, `textiz`, `napom` | `printracuniavans` | Transient print formatting |
| `p1`, `p2`, `p3`, `p4`, `p5` | `printracuniavans` | Generic placeholder columns |
| `d1`, `d2`, `d3` | `printracspec` | Generic placeholder columns |
| `sifra1` | `printracspec` | Generic code column |
| `sifra`, `kol`, `cij`, `ukupno`, `racu`, `racun`, `dod`, `dod1`, `dod2`, `dod3` | `sifarnik` | Old POS/product codebook — replace with modern product catalog |
| `n1`, `n2`, `n3`, `i1` | `godine` | Generic columns for year definitions — unclear purpose |
| `upd` | `konta`, `tarifatxe` | Update counter — replace with proper audit |
| `del` | `goststatus`, `sobatarifa`, `troskovivrste`, `gostiknjiga`, `drzave` | Soft delete flag — preserve pattern or convert to active/inactive enum |
| `prim` | `drzave` | Always 1 — no business meaning |
| `br` | `drzave` | Unknown purpose |
| `verpr`, `keyk`, `izmjver`, `rad`, `naplposo`, `pribora`, `cultur`, `racunbr`, `lokac`, `minchi`, `maxcho`, `stan` | `setings` | Move to modern settings/config per key (see below) |
| `vrijU`, `verbaz`, `t1`, `t2`, `t3` | `setings` | App version/timestamp fields — not business data |
| `Opis1`, `vrijeme1`, `chk`, `rpt`, `week`, `pon`-`ned`, `radnik`, `soba`, `storno`, `vr_upis`, `vr_potvrde`, `radnStorn`, `radnCHK`, `radnCHKvr`, `stornovr` | `alarm` | Alarm/reminder columns — modernize as notification system |
| `rad`, `vrijeme` | `napomena`, `napomenad` | Notes with worker/timestamp — merge into single notes table |
| `Datum`, `Vrijeme`, `Izlaz`, `TrajanjePoziva`, `PozBroj`, `Drzava`, `Cost`, `Placeno`, `SpecPozivni`, `SpecDrzava` | `telpozivi`, `telpozivi_stara` | Phone call logs — replace with modern phone integration |
| `sadrzaj` columns | `konta` | Chart of accounts codes — legacy accounting integration |
| `Farbe`, `Dad` columns | Not present | (No such columns exist in schema) |
| `printracspec` (whole table) | `printracspec` | Special print format invoices — snapshot, can be regenerated |
| `export` (whole table) | `export` | Export tracking — replace with modern export log |
| `logloby`, `logrestoran` | (whole tables) | Lobby and restaurant action logs — replace with unified audit log |
| `mailkonfig` (whole table) | `mailkonfig` | Email config — move to modern settings |
| `telefonskiimenik` (whole table) | `telefonskiimenik` | Phone directory — low priority |
| `posjete` (whole table) | `posjete` | Auxiliary visit tracking — unclear business value |
| `rezervacijasobe1`, `rezervacije1`, `sobavrsta1` | Staging tables | Exact duplicates of main tables — for data import |
| `kontroler` | Device registry | Replace with proper device management |
| `logcont` | Controller logs | Replace with structured logging |
| `sobaricalog` | Keycard logs | Replace with modern keycard integration |

**Settings (`setings`) columns that should become modern configuration keys:**

| Legacy Column | Modern Config Key | Type | Notes |
|---------------|-------------------|------|-------|
| `pdv` | `vat_rate` | double | VAT percentage (17%) |
| `pdvo` | `vat_rate_reduced` | double | Reduced VAT rate |
| `pdvtax` | `vat_on_tourist_tax` | double | VAT on tourist tax |
| `pdvtr` | `vat_transport` | double | VAT on transport |
| `osig` | `insurance_amount` | double | Insurance per night |
| `taxa` | `tourist_tax_amount` | double | Tourist tax per night |
| `cijt` | `price_includes_tax` | int | 0=excl, 1=incl |
| `naplposo` | `billing_mode` | int | 0=split, 1=full per person |
| `dijecagod` | `child_age_threshold` | double | Age threshold for child discount |
| `dijecapop` | `child_discount_percent` | double | Discount percentage for children |
| `minchi` | `check_in_hour` | int | Earliest check-in hour (default 8) |
| `maxcho` | `check_out_hour` | int | Latest check-out hour (default 12) |
| `decim` | `decimal_places` | int | Currency decimal places (default 2) |
| `valuta` | `currency_code` | string | e.g. "KM" |
| `sobegrupa` | `room_groups` | string | Semicolon-delimited room group codes |
| `sobekuc` | `apartment_codes` | string | Semicolon-delimited apartment codes |
| `fiscal` | `fiscal_device_config` | string | Star-delimited config string for fiscal device |
| `racunbr` | `receipt_number_format` | string | Receipt number format |
| `stan` | `station_id` | int | Terminal station number |

---

## 6. Modern Entity Diagram

```
┌────────────────────────────────────────────────────────────────────────────────┐
│                              SETTINGS                                          │
│  vat_rate, billing_mode, child_age_threshold, check_in/out_hours,             │
│  tourist_tax, insurance, currency, fiscal_device_config, ...                   │
└────────────────────────────┬───────────────────────────────────────────────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
   ┌──────────┐      ┌──────────────┐     ┌──────────────┐
   │ Building │      │  RoomType    │     │ Tariff        │
   ├──────────┤      ├──────────────┤     ├──────────────┤
   │ id       │      │ id           │     │ id           │
   │ name     │      │ name         │     │ amount◎      │
   │ desc     │      │ bedCount     │     │ label        │
   └────┬─────┘      │ defaultTariff│────▶│ condition    │
        │            └──────┬───────┘     │ isDeleted   │
        │                   │              └──────────────┘
        │                   │
        ▼                   ▼
   ┌──────────────────────────────┐       ┌──────────────────┐
   │           Room               │       │ RoomTypeTariff   │
   ├──────────────────────────────┤       ├──────────────────┤
   │ id                          │       │ roomTypeId       │
   │ number                      │       │ tariffId         │
   │ roomTypeId ──────────────▶  │       └──────────────────┘
   │ buildingId                  │
   │ outOfOrder                  │       ┌──────────────────┐
   │ outOfOrderReason            │       │ RoomAmenity      │
   │ isClean                     │       ├──────────────────┤
   │ controllerType              │       │ id               │
   │ floor                       │       │ name             │
   │ sortOrder                   │       │ defaultTariffId  │
   │ tooltipText                 │       └──────────────────┘
   │ altRoomTypeId               │
   │ phoneExtension              │       ┌──────────────────┐
   └──────────────────────────────┘       │ RoomRoomAmenity │
                                          ├──────────────────┤
   ┌──────────────┐                        │ roomId           │
   │ Country      │                        │ amenityId        │
   ├──────────────┤                        └──────────────────┘
   │ id           │
   │ name         │
   │ isoCode      │
   │ phonePrefix  │
   │ isDomestic   │
   └──────┬───────┘
          │
          ▼
   ┌──────────────────────────────────────────────────────────────┐
   │                         Guest                                 │
   ├──────────────────────────────────────────────────────────────┤
   │ id                                                           │
   │ firstName                                                    │
   │ lastName                                                     │
   │ address                                                      │
   │ dateOfBirth                                                  │
   │ gender (M/Z)                                                 │
   │ citizenship (→ Country)                                      │
   │ documentTypeId (→ GuestDocumentType)                          │
   │ documentNumber                                               │
   │ phone / mobile / email                                       │
   │ placeOfResidence                                             │
   │ reservationId (→ Reservation, cleared after check-in)        │
   └──────────────────────────────────────────────────────────────┘
          │
          │ (1:N stays)
          ▼
   ┌──────────────────────────────────────────────────────────────┐
   │                    Stay (relgostsoba)                         │
   ├──────────────────────────────────────────────────────────────┤
   │ id                                                           │
   │ guestId ──────────▶ Guest                                    │
   │ roomId ──────────▶ Room                                      │
   │ checkInAt                                                    │
   │ checkOutAt                                                   │
   │ checkedInBy (→ Worker)                                      │
   │ checkedOutBy (→ Worker)                                     │
   │ isRegistrationPrinted                                       │
   │ isCheckedOut                                                 │
   │ isFromConfirmedReservation                                  │
   │ groupId                                                      │
   │ dayCount                                                     │
   │ tariffId ─────────▶ Tariff                                   │
   │ discountPercent                                              │
   │ discountReason                                               │
   │ otherExpenses                                                │
   │ folioId ─────────▶ Folio                                    │
   │ isAccommodationPaid                                          │
   │ paymentNote                                                  │
   │ stayNote                                                     │
   │ serviceNote                                                  │
   │ taxOverride                                                  │
   │ guestStatusId ──▶ GuestCategory                             │
   │ touristRegId                                                 │
   │ isReportR1Printed                                            │
   │ isReportR2Printed                                            │
   └──────────────────────────────────────────────────────────────┘
          │
          │ (1:N nights, 1:N expenses, 1:N payments)
          ├─────────────────────┬─────────────────────┐
          │                     │                     │
          ▼                     ▼                     ▼
   ┌──────────────┐   ┌────────────────┐   ┌──────────────────┐
   │    Night      │   │    Expense      │   │    Payment       │
   │  (nocenja)    │   │  (troskovi)     │   │  (placanje)      │
   ├──────────────┤   ├────────────────┤   ├──────────────────┤
   │ id           │   │ id             │   │ id               │
   │ stayId       │   │ stayId         │   │ receiptNumber    │
   │ date         │   │ roomId         │   │ stayId           │
   │ dateOut      │   │ expenseTypeId  │   │ amount           │
   │ roomId       │   │ timestamp      │   │ discount         │
   │ folioId      │   │ quantity       │   │ date             │
   │ isClosed     │   │ amount         │   │ paymentMethodId  │
   │ tariffAmount │   │ workerId       │   │ workerId         │
   │ discountPct  │   │ note           │   │ description      │
   │ description  │   │ isLocked       │   │ folioId          │
   │ roomName     │   │ receiptNumber  │   │ amountPaid       │
   └──────────────┘   │ isPartial      │   │ nightCount       │
                      │ statusFlag     │   │ periodFrom       │
                      │ fiscalSent     │   │ periodTo         │
                      └────────────────┘   │ isCancelled      │
                                           │ isVatRegistrant  │
   ┌──────────────┐                        │ isTouristTax     │
   │ ExpenseType   │                        │ companyId        │
   │(troskovivrste)│   ┌─────────────────┐  │ guestId          │
   ├──────────────┤   │  PaymentDetail   │  └────────┬─────────┘
   │ id           │   │(placanjedetalji) │           │
   │ name         │   ├─────────────────┤           │
   │ priceId      │   │ id              │           │
   │ category     │   │ paymentId       │◀──────────┘
   │ isDeleted    │   │ articleType     │
   └──────────────┘   │ quantity        │
                      │ unitPrice       │
   ┌─────────────┐    │ totalAmount     │
   │Folio         │    │ nightCount      │
   │(posjetafolio)│    │ folioId         │
   ├─────────────┤    │ isCancelled     │
   │ id           │    │ stayId          │
   │ roomId       │────│ roomId          │
   │ openedAt     │    │ guestId         │
   │ closedAt     │    │ roomName        │
   │ isClosed     │    │ periodFrom/To   │
   └─────────────┘    │ serviceName     │
                      │ discount        │
   ┌─────────────────┐│ vatAmount       │
   │PaymentMethod     │└─────────────────┘
   │(placanjenacin)   │
   ├─────────────────┤   ┌───────────────────────┐
   │ id              │   │ PaymentMethodSplit    │
   │ name            │   │ (placanjeslozeno)     │
   │ accountCode     │   ├───────────────────────┤
   │ partnerCode     │   │ id                    │
   └─────────────────┘   │ lineNumber            │
                         │ paymentMethodId       │
   ┌─────────────────────┐│ amount                │
   │    Reservation       │└───────────────────────┘
   │    (rezervacije)     │
   ├─────────────────────┤   ┌───────────────────┐
   │ id                   │   │ ReservationRoom   │
   │ guestId ───────▶    │   │(rezervacijasobe)  │
   │ checkInDate          │   ├───────────────────┤
   │ checkOutDate         │   │ id                │
   │ isConfirmed          │   │ reservationId     │
   │ confirmationNumber   │   │ roomTypeId        │
   │ groupId              │   │ roomId            │
   │ sourceTypeId         │   │ tariffId          │
   │ roomTypeId           │   │ guestId           │
   │ isCancelled          │   │ tariffAmount      │
   │ cancellationNumber   │   │ isSmoker          │
   │ roomCount            │   │ guestCount        │
   │ checkInStatus        │   └───────────────────┘
   │ tariffId              │
   │ workerId             │   ┌───────────────────┐
   │ createdAt            │   │ CheckInMapping    │
   │ guestName (denorm)   │   │(rezervacijaPrij.)│
   │ companyId            │   ├───────────────────┤
   │ agencyId             │   │ reservationId     │
   │ agentId               │   │ guestId           │
   │ adultCount            │   │ roomId            │
   │ childCount            │   └───────────────────┘
   │ lastModifiedAt       │
   │ cancelledAt          │   ┌───────────────────┐
   │ confirmedAt          │   │ ReservationGroup  │
   │ cancellationReason   │   │(rezervacijegrupe) │
   │ note                 │   ├───────────────────┤
   │ alarmId               │   │ id                │
   │ changeCount          │   │ name              │
   │ contactName/Phone/   │   │ isClosed          │
   │   Fax/Mobile/Email   │   └───────────────────┘
   └─────────────────────┘
                          ┌──────────────────┐  ┌──────────────────┐
                          │ ReservationType  │  │ ReservationSource │
                          │(rezervacijetip)  │  │(rezervacijeizvor)│
                          ├──────────────────┤  ├──────────────────┤
                          │ id=1: Nema       │  │ id=1: Nema       │
                          │ id=2: Mail       │  │ id=2: Mail       │
                          │ id=3: Fax        │  │ id=3: Fax        │
                          │ id=4: Telefon    │  │ id=4: Telefon    │
                          └──────────────────┘  └──────────────────┘

   ┌──────────────────────┐    ┌──────────────────────┐
   │     Invoice           │    │   AdvanceInvoice      │
   │   (printracuni)       │    │  (printracuniavans)   │
   ├──────────────────────┤    ├──────────────────────┤
   │ id (BrojRacuna)      │    │ id (BrojRacuna)      │
   │ businessUnit         │    │ businessUnit         │
   │ guestName            │    │ guestName            │
   │ periodFrom           │    │ periodFrom           │
   │ periodTo             │    │ periodTo             │
   │ paymentType          │    │ paymentType          │
   │ roomNumber           │    │ roomNumber           │
   │ isCancelled          │    │ isCancelled          │
   │ exportStatus          │    │ folioId              │
   │ cancelledAt           │    │ guestId              │
   │ ←─ InvoiceLineItem   │    │ isCancelled          │
   │ ←─ InvoiceFooter     │    └──────────────────────┘
   └──────────────────────┘
          │ 1:N
          ▼
   ┌──────────────────────┐    ┌──────────────────────┐
   │  InvoiceLineItem      │    │  InvoiceFooter       │
   │(printracunidetalji)   │    │(printracunifooter)   │
   ├──────────────────────┤    ├──────────────────────┤
   │ id                   │    │ id                   │
   │ invoiceId            │    │ invoiceId            │
   │ expenseName          │    │ advanceAmount        │
   │ quantity             │    │ nightCount           │
   │ priceExVat           │    │ note                 │
   │ totalExVat           │    │ total                │
   │ vatRate              │    └──────────────────────┘
   │ vatAmount            │
   │ total                │    ┌──────────────────────┐
   │ paymentMethod        │    │  Partner/Agency      │
   │ currency             │    │    (partneri)         │
   │ discount             │    ├──────────────────────┤
   │ expenseTypeId        │    │ id                   │
   │ paymentMethodId      │    │ name                 │
   └──────────────────────┘    │ city / postal / street│
                               │ country              │
   ┌──────────────────────┐    │ phone / fax / email  │
   │    Partner            │    │ vatNumber            │
   │  (komercijalista)    │    │ registrationNumber   │
   ├──────────────────────┤    │ bankAccount1-4       │
   │ id                   │    │ contactPerson        │
   │ firstName / lastName │    │ discount             │
   │ phone / mobile       │    │ note                 │
   │ tariffId             │    └──────────────────────┘
   │ priceListId          │
   │ note                 │
   └──────────────────────┘

   ┌──────────────────────┐    ┌──────────────────────┐
   │     Worker            │    │      Alarm            │
   │    (radnici)          │    │     (alarm)           │
   ├──────────────────────┤    ├──────────────────────┤
   │ id                   │    │ id                   │
   │ name                 │    │ description           │
   │ jmbg                 │    │ description1         │
   │ address               │    │ answer               │
   │ phone                 │    │ time                 │
   │ username             │    │ time1                │
   │ disabled              │    │ type                 │
   │ accessLevel           │    │ checked              │
   └──────────────────────┘    │ repeat               │
                               │ weekDay flags        │
   ┌──────────────────────┐    │ worker               │
   │ GuestDocumentType     │    │ room                 │
   │   (gostdokument)      │    │ isCancelled          │
   ├──────────────────────┤    │ createdAt            │
   │ id                   │    │ confirmedAt          │
   │ name                 │    │ cancelledAt          │
   │ (Pasos,Licna,etc)   │    │ uniqueId             │
   └──────────────────────┘    └──────────────────────┘

   ┌──────────────────────┐    ┌──────────────────────┐
   │  GuestStatus          │    │   GuestRegistration  │
   │   (goststatus)        │    │   (gostiknjiga)      │
   ├──────────────────────┤    ├──────────────────────┤
   │ id                   │    │ id                   │
   │ name                 │    │ registrationNumber  │
   │ isDeleted            │    │ guestId             │
   │ taxAmount            │    │ firstName/lastName   │
   │ (Turist,Vlasnik,    │    │ documentType         │
   │  Dijete do 12, etc) │    │ stayStart           │
   └──────────────────────┘    │ citizenship          │
                               │ status              │
   ┌──────────────────────┐    │ stayEnd              │
   │  UnpaidTracking       │    │ visaExpiry           │
   │   (neplaceni)         │    │ entryDate            │
   ├──────────────────────┤    │ entryPoint           │
   │ id                   │    │ birthPlace           │
   │ folioId              │    └──────────────────────┘
   │ departureDate        │
   │ roomId               │    ┌──────────────────────┐
   │ expenseTypeId        │    │  ExchangeRate        │
   │ isPaid               │    │    (kursna)           │
   └──────────────────────┘    ├──────────────────────┤
                               │ id                   │
   ┌──────────────────────┐    │ currency             │
   │  UnpaidPayment        │    │ rate                 │
   │  (neplaceniplacanja)  │    └──────────────────────┘
   ├──────────────────────┤
   │ id                   │    ┌──────────────────────┐
   │ folioId              │    │  WorkerShift          │
   │ totalDue             │    │    (smjene)           │
   │ advancePaid          │    ├──────────────────────┤
   └──────────────────────┘    │ id                   │
                               │ workerId             │
   ┌──────────────────────┐    │ startTime            │
   │ AuditLog (logs)      │    │ endTime              │
   ├──────────────────────┤    └──────────────────────┘
   │ id                   │
   │ action               │    ┌──────────────────────┐
   │ timestamp            │    │  CurrencyRate         │
   │ description          │    │    (kursna)           │
   │ description1         │    ├──────────────────────┤
   │ workerName           │    │ id                   │
   │ workerId             │    │ currencyName         │
   └──────────────────────┘    │ rate                 │
                               └──────────────────────┘

   ┌──────────────────────────────────────────────────────────────────────┐
   │                     KEY RELATIONSHIPS SUMMARY                         │
   │                                                                      │
   │  Guest 1──N Stay ──1──N Night                                        │
   │  Guest 1──N Stay ──1──N Expense                                     │
   │  Room  1──N Stay                                                       │
   │  Room  1──N Folio                                                      │
   │  Room  1──N Night                                                      │
   │  Room  1──N Expense                                                    │
   │  Folio 1──N Stay                                                     │
   │  Folio 1──N Night                                                    │
   │  Folio 1──N Payment                                                  │
   │  Stay  1──N Expense                                                   │
   │  Stay  1──N Payment                                                   │
   │  ExpenseType 1──N Expense                                             │
   │  PaymentMethod 1──N Payment                                          │
   │  Reservation 1──N ReservationRoom                                     │
   │  Reservation 1──1 ReservationGroup                                    │
   │  Reservation 0──1 Guest                                               │
   │  RoomType 1──N Room                                                   │
   │  RoomType M──N Tariff (via junction)                                 │
   │  Room 1──N KeyCard                                                    │
   │  Building 1──N Room                                                   │
   │  Worker 1──N Stay (check-in/out)                                     │
   │  Worker 1──N Payment                                                  │
   │  Worker 1──N Expense                                                  │
   │  Partner 1──N Reservation                                             │
   │  Invoice 1──N InvoiceLineItem                                         │
   │  Invoice 1──1 InvoiceFooter                                            │
   └──────────────────────────────────────────────────────────────────────┘
```

---

## Appendix: Key Migration Notes

### A.1 Computed vs Stored Status
- `Room.status` is **computed** by `fnSobaStatus` from live data — it should remain a computed property in the modern system, not stored
- `Room.isClean` **overrides** all other status displays — must be checked before showing room status
- `Room.outOfOrder` is a stored flag that short-circuits `fnSobaStatus` → always returns status 5

### A.2 Central Tables
- `relgostsoba` is the **core table** — links guests to rooms, represents stays
- `posjetafolio` is the **billing anchor** — one per room per stay period
- `nocenja` is the **night charge ledger** — one row per guest per night period per stay
- `troskovi` is the **expense ledger** — one row per charge
- `placanje` is the **payment record** — one row per receipt

### A.3 Denormalization Patterns
The legacy system heavily denormalizes guest names, room names, and tariff amounts into child tables (`rezervacijasobe`, `placanjedetalji`, `printracuni`, etc.). The modern system should:
1. Store FK references only
2. Compute display names via JOINs
3. Use snapshot tables (`printracuni*`) only for immutable fiscal records

### A.3 Soft Delete Pattern
Multiple tables use `del` or `storno` flags instead of physical deletion:
- `goststatus.del`, `sobatarifa.del`, `troskovivrste.del`, `drzave.del`, `partneri.del`
- `placanje.storno`, `printracuni.storno`, `printracuniavans.storno`
- `rezervacije.stornirana`
- `troskovi` TID=1 rows are **physically deleted** on storno (not soft-deleted)

### A.4 Year-Based Database Separation
The legacy system uses `ConnStr` with a year suffix to switch between year-specific databases. The modern system should use a single database with year-based partitioning or date-filtered queries.

### A.5 Race Conditions to Eliminate
1. `brojPotvrde` and `brojStorna`: MAX()+1 without locking → use database sequences
2. `placanje.broj`: MAX()+1 without locking → use database sequences
3. `gostiknjiga.estranac`: MAX()+1 without locking → use database sequences
4. Folio creation: SELECT then INSERT → use INSERT with duplicate check
5. Global `ds` DataSet: shared mutable state across MDI forms → eliminate global state

### A.6 Critical Business Rules to Preserve
1. **BR-01**: Night charges are per-guest; tariff split based on `billing_mode`
2. **BR-02**: Minimum charge = `tourist_tax + insurance` if calculated price = 0
3. **BR-03**: No duplicate guest in same room
4. **BR-05**: Folio is shared across all guests in a room during same stay
5. **BR-08**: Room clean status overrides ALL other status displays
6. **BR-16**: Accommodation expenses (TID=1) are DELETED on storno, not reopened
7. **BR-17**: Non-accommodation expenses (TID≠1) are REOPENED on storno
8. **BR-18**: Advance invoice storno only sets flag — does NOT reverse payments
9. **BR-19**: Night count: check-in <08:00 → shift -1 day; checkout ≥12:00 → shift +1 day; minimum 1
10. **BR-21**: Partial guest checkout splits night records (INSERT new rows for remaining guests)
11. **BR-24**: Unpaid checkout creates `neplaceni` records (TID=25 in troskovi)
12. **BR-25**: "Return to room" reopens folio, moves expenses, adds new nights