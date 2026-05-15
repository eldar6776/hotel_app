# REPO_REALITY_MAP

Status: AUTHORITATIVE
Last validated: 2026-05-15

## Stvarni projekti (novi kod)

- `frontend/` — Next.js (React) + Tailwind CSS web aplikacija
- `backend/` — .NET 8 (C#) Web API
- `hardware_bridge/` — Lokalni servis za USB/Serial uredjaje (fiskalne kase, RFID citaci kartica)
- `iot_services/` — IoT integracije, MQTT broker, pametne brave i senzori
- `infrastructure/` — Docker Compose, deployment skripte, CI/CD

## Referentni kod (ne mijenjaj)

- `legacy_app/` — Stari VB.NET WinForms + MySQL (Radna.sln) — samo za reverzni inzenjering

## Gdje agent cita dokumente

- status: `.agents/STATUS.md`
- pravila: `.agents/01_governance/`
- specifikacije: `.agents/03_specs/`
- taskovi: `.agents/04_tasks/`
- dokumentacija: `docs/`

## Operativne cinjenice

- Baza: PostgreSQL 18
- Frontend framework: Next.js (React) + Tailwind CSS
- Backend framework: .NET 8 (C#) Web API + EF Core
- Stara baza (MySQL dump): `legacy_app/novaBazaJHotel 20150602 0848.sql`
- Stara aplikacija: `legacy_app/Radna.sln`
- Hosting: Docker — moze AWS cloud ili in-house server
- Cross-platform: mora raditi identično na Windows i macOS
