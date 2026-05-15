# REBUILD_FROM_ZERO

Status: AUTHORITATIVE
Last validated: 2026-05-15

Ovaj dokument opisuje kako AI agent ili programer moze ponovo sastaviti i pokrenuti HotelPRO od nule.

## 1. Stvarne komponente sistema

Repo `hotel_app` sadrzi:

- `frontend/` — Next.js web aplikacija (Recepcija UI, Admin Panel, Dashboards)
- `backend/` — .NET 8 Web API (poslovni API, EF Core, autentifikacija)
- `hardware_bridge/` — Cross-platform servis za fizicke uredjaje
- `iot_services/` — MQTT broker konekcije, senzori, pametne brave
- `infrastructure/` — Docker Compose, PostgreSQL setup, deployment skripte
- `legacy_app/` — Stari VB.NET kod (referenca, ne izvrsavaj)

## 2. Preduslovi

- Node.js 20+ (za frontend)
- .NET 8 SDK (za backend)
- Docker Desktop (za PostgreSQL i kontejnerizaciju)
- Git

## 3. Obavezne environment varijable

- `HOTEL_DB_CONN` — PostgreSQL connection string
- `HOTEL_JWT_SECRET` — JWT token secret za autentifikaciju
- `HOTEL_STRIPE_KEY` — Stripe API key (samo za produkciju)

## 4. Redoslijed bootstrapa

1. Procitaj `REPO_REALITY_MAP.md`
2. Pokreni `docker compose up -d` iz `infrastructure/`
3. Pokreni `dotnet ef database update` iz `backend/`
4. Pokreni `dotnet build` iz `backend/`
5. Pokreni `dotnet test` iz `backend/`
6. Pokreni `npm install` iz `frontend/`
7. Pokreni `npm run dev` iz `frontend/`
8. Otvori `http://localhost:3000` u browseru

## 5. Preporuceni lokalni tok

### Baza i backend

```powershell
cd infrastructure
docker compose up -d
cd ..\backend
dotnet ef database update
dotnet run
```

### Frontend

```powershell
cd frontend
npm install
npm run dev
```

## 6. Build i verifikacija

Backend build:

```powershell
dotnet build backend\HotelPro.sln
```

Backend testovi:

```powershell
dotnet test backend\HotelPro.sln
```

Frontend build:

```powershell
cd frontend
npm run build
```

## 7. Minimalni smoke scenario

1. Potvrdi da je PostgreSQL dostupan (Docker).
2. Pokreni backend API.
3. Pokreni frontend dev server.
4. Otvori `http://localhost:3000`.
5. Potvrdi da se login stranica ucitava.
6. Potvrdi da API vraca odgovor na `GET /api/health`.

Ako smoke scenario ne prodje, ne nastavljaj na naredne taskove dok osnova nije stabilna.

## 8. Kako odabrati sta jos treba implementirati

1. Procitaj `../STATUS.md`
2. Identifikuj prvi slobodan `PENDING` task
3. Procitaj task dokument u `../04_tasks/`
4. Procitaj zavisni FSD u `../03_specs/fsd/`
5. Tek onda mijenjaj kod
