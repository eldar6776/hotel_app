# MASTER_INSTRUCTIONS

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Osnova

- Platforma: Web (Cross-platform — Windows + macOS)
- Frontend: Next.js (React) + Tailwind CSS u `/frontend`
- Backend: .NET 8 (C#) Web API u `/backend`
- Baza: PostgreSQL 18
- Hardware Bridge: `/hardware_bridge` — lokalni servis za USB/Serial uredjaje
- IoT: `/iot_services` — MQTT integracije za pametne brave i senzore
- Legacy referenca: `/legacy_app` — stari VB.NET kod, NE MIJENJAJ

## Pravila

- ne hardkodirati tajne, koristiti environment varijable
- koristiti `HOTEL_DB_CONN` za bazu
- koristiti `HOTEL_JWT_SECRET` za autentifikaciju
- prije izmjena procitati task i FSD
- nakon izmjena testirati (backend: `dotnet test`, frontend: `npm run build`)
- nakon rada azurirati `STATUS.md`
- sve UI komponente moraju biti responzivne i podrzavati Dark Mode
- dizajn mora biti "premium" — koristiti moderne animacije, gradijente i tipografiju
- ne koristiti Crystal Reports — koristiti moderan reporting (HTML/PDF export)
- legacy kod u `/legacy_app` sluzi SAMO za razumijevanje poslovne logike
- tretirati hardkodirane stringove, magic numbere i plaintext sigurnosne vrijednosti kao defect

## Ako dokumentacija i kod nisu isti

Prati stvarni kod i evidentiraj razliku u `STATUS.md` kada je bitna za rad.
