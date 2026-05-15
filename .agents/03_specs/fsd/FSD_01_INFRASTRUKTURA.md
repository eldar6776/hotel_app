# FSD 1: Infrastruktura i DevOps

Status: AUTHORITATIVE
Last validated: 2026-05-15

## 1. Cilj

Postaviti razvojnu infrastrukturu koja omogucava pokretanje kompletnog sistema lokalno sa jednom komandom (`docker compose up`), i koja je identicna za Windows i macOS.

## 2. Komponente

### 2.1 Docker Compose

Fajl: `infrastructure/docker-compose.yml`

Servisi:
- `postgres` — PostgreSQL 18, port 5432, volume za perzistenciju
- `redis` — Redis za session/cache (opcionalno, Faza 3+)

### 2.2 Environment

Fajl: `infrastructure/.env.example`

Obavezne varijable:
- `HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=hotel;Password=hotel_dev_2026`
- `HOTEL_JWT_SECRET=<generisati-prilikom-prvog-pokretanja>`
- `ASPNETCORE_ENVIRONMENT=Development`

### 2.3 .gitignore

Globalni `.gitignore` u root-u mora ignorisati:
- `node_modules/`, `.next/`, `out/`
- `bin/`, `obj/`
- `.env` (ali NE `.env.example`)
- `*.suo`, `*.user`, `Thumbs.db`
- `legacy_app/bin/`, `legacy_app/obj/`

## 3. Automatski backup (in-house instalacije)

In-house hoteli nemaju IT osoblje. Backup mora biti potpuno automatski.

### 3.1 PostgreSQL backup (Docker)

```yaml
# infrastructure/docker-compose.yml
services:
  backup:
    image: prodrigestivill/postgres-backup-local:18
    environment:
      POSTGRES_HOST: postgres
      POSTGRES_DB: hotelpro
      POSTGRES_USER: hotel
      POSTGRES_PASSWORD: ${DB_PASSWORD}
      SCHEDULE: "@daily"              # backup svaki dan u 03:00
      BACKUP_DIR: /backups
      BACKUP_KEEP_DAYS: 30            # čuvaj 30 dana
    volumes:
      - pgbackups:/backups
```

### 3.2 Off-site backup

```yaml
services:
  backup-upload:
    image: alpine
    entrypoint: |
      sh -c '
      apk add aws-cli
      while true; do
        sleep 86400
        aws s3 sync /backups s3://hotel-backups/${HOTEL_ID}/
      done'
    volumes:
      - pgbackups:/backups
    environment:
      AWS_ACCESS_KEY_ID: ${AWS_ACCESS_KEY}
      AWS_SECRET_ACCESS_KEY: ${AWS_SECRET}
```

### 3.3 Restore procedura

```powershell
# restore.ps1
docker compose exec -T postgres pg_restore -U hotel -d hotelpro /backups/hotelpro_20260515.dump
```

### 3.4 Backup destinacija
- **In-house**: Lokalni disk (default) + opciono S3
- **Cloud (SaaS)**: S3 (preporuceno)
- Konfigurabilno u admin panelu (Settings ? System ? Backup)
## 4. Restrikcije

- Docker Compose MORA koristiti named volumes, ne bind mounts za bazu
- PostgreSQL MORA biti verzija 18
- Ne koristiti Docker za backend/frontend u dev modu (samo za bazu)
- Backup MORA biti testiran najmanje jednom mjesecno
