# HotelPRO

Moderni Property Management System (PMS) za hotele — .NET 8 + Next.js 16 + PostgreSQL 18.

**Status:** ✅ v1.0 — Faze 1-18 kompletirane

---

## Brzi start

### Automatski (preporučeno)
Klikni **`start.bat`** — pokreće backend i frontend.

### Ručno

**1. Backend** (port 5149)
```powershell
cd backend\src\HotelPro.Api
$env:HOTEL_DB_CONN="Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode"
$env:HOTEL_JWT_SECRET="hotelpro-dev-secret-mihrivode-2026"
dotnet run
```
Swagger: http://localhost:5149/swagger

**2. Frontend** (port 3000)
```powershell
cd frontend
npm run dev
```
Frontend: http://localhost:3000

### Demo login

| Korisnik | Email | Lozinka |
|----------|-------|---------|
| Admin | admin@hotelpro.local | admin123 |
| Recepcija | reception@hotelpro.local | reception123 |
| Menadžer | manager@hotelpro.local | manager123 |

### Skripte

| Fajl | Opis |
|------|------|
| `start.bat` | Pokreće backend + frontend |
| `stop.bat` | Gasi sve procese |
| `reset-db.bat` | Resetuje bazu + migracije + seed |

---

## Tehnologije

| Sloj | Tehnologija |
|------|-------------|
| Backend | .NET 8, ASP.NET Core, EF Core |
| Frontend | Next.js 16, React 19, Tailwind CSS |
| Baza | PostgreSQL 18 |
| Auth | JWT + RBAC (Admin, Manager, Reception, Housekeeping) |
| Real-time | SignalR WebSocket |
| Email | MailKit SMTP |
| PDF | QuestPDF |
| Testovi | xUnit + InMemory EF Core (57 testova) |

## Faze

| # | Faza | Status |
|---|------|--------|
| 0 | Planiranje | ✅ |
| 1 | Infrastruktura / DevOps | ✅ |
| 2 | Backend Foundation | ✅ |
| 3 | Auth | ✅ |
| 4 | Frontend Foundation | ✅ |
| 5 | Upravljanje Sobama | ✅ |
| 6 | Rezervacije | ✅ |
| 7 | Recepcija | ✅ |
| 8 | Gosti i CRM | ✅ |
| 9 | Naplata | ✅ |
| 10 | Izvještavanje | ✅ |
| 11 | Housekeeping | ✅ |
| 12 | Hardware Bridge | ✅ |
| 13 | Channel Manager | ✅ |
| 14 | IoT | ✅ |
| 15 | Revenue | ✅ |
| 16 | Guest Self-Service | ✅ |
| 17 | Payment Gateway | ✅ |
| 18 | Stabilizacija | ✅ |

## Verifikacija

- `dotnet build` — 0 errors
- `dotnet test` — 57/57 passed
- `npm run lint` — clean
