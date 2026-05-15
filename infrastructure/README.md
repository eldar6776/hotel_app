# HotelPRO Infrastructure

## Pokretanje razvojne baze

### Start

```bash
docker compose up -d
```

Ovo ce pokrenuti PostgreSQL 18 kontejner na portu 5432 sa automatskim healthcheck-om.

### Zaustavljanje

```bash
docker compose down
```

Zaustavlja kontejnere bez brisanja podataka.

### Reset baze (brise sve podatke!)

```bash
docker compose down -v
```

Ovo ce obrisati named volume i sve podatke u bazi. Koristiti samo za razvoj.

### Provjera statusa

```bash
docker compose ps
docker compose logs postgres
```

### Healthcheck

PostgreSQL kontejner ima ugradjen healthcheck koji koristi `pg_isready`:

```bash
docker compose exec postgres pg_isready -U hotel -d hotelpro
```

## Backup

Backup servis automatski kreira daily backup u `./backups/` folderu u 03:00. Backup fajlovi se cuvaju 30 dana.

### Restore

```bash
docker compose exec -T postgres pg_restore -U hotel -d hotelpro /backups/<backup-file>.dump
```

Ili koristite PowerShell skriptu:

```powershell
./backup/restore.ps1 -BackupFile <path>
```

## Environment varijable

Kopirajte `.env.example` u `.env` i prilagodite vrijednosti:

```bash
cp .env.example .env
```

Za generisanje sigurnih secret vrijednosti:

```powershell
./scripts/generate-secrets.ps1 -WriteToFile
```

## Troubleshooting

### PostgreSQL se ne pokrece

```bash
docker compose logs postgres
```

Provjerite da port 5432 nije vec u upotrebi.

### Healthcheck fails

```bash
docker compose exec postgres pg_isready -U hotel -d hotelpro
```

Ako vrati "no response", provjerite da li je kontejner pokrenut i da li su environment varijable ispravne.

### Backup ne radi

```bash
docker compose logs backup
```

Provjerite da postgres kontejner ima status "healthy" prije nego backup servis pokrene.

### Reset everything

```bash
docker compose down -v
rm -f .env
cp .env.example .env
docker compose up -d
```
