# HotelPRO Backup Documentation

## Automatic Backup

The `docker-compose.yml` includes a backup service (`prodrigestivill/postgres-backup-local:18`) that:

- Runs daily at 03:00 (`@daily` schedule)
- Stores backups in `./backups/` folder
- Keeps backups for 30 days (`BACKUP_KEEP_DAYS=30`)
- Produces `.dump` format files

## Backup Folder Structure

```
infrastructure/backups/
  daily/
    hotelpro_2026-05-15_030001.dump.gz
    hotelpro_2026-05-16_030001.dump.gz
    ...
  weekly/
    (symlink or copy of last backup from each week)
```

Note: The backup image writes directly to `/backups/`. The `daily/` subfolder structure is managed by the backup retention policy. Files are automatically deleted after `BACKUP_KEEP_DAYS` (30 days).

## Off-site S3 Backup (Optional)

### Prerequisites

- AWS CLI installed
- AWS S3 bucket created
- IAM user with S3 write permissions

### Environment Variables

```
AWS_ACCESS_KEY_ID=<your-access-key>
AWS_SECRET_ACCESS_KEY=<your-secret-key>
S3_BUCKET=hotelpro-backups
S3_PREFIX=<hotel-id>
```

### Usage

Run the sync script manually or schedule it:

```powershell
./backup/sync-to-s3.ps1
```

### Scheduled Task (Windows)

```powershell
$action = New-ScheduledTaskAction -Execute "pwsh" -Argument "-File C:\path\to\sync-to-s3.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 4am
Register-ScheduledTask -TaskName "HotelPRO-S3-Backup" -Action $action -Trigger $trigger
```

## Restore Procedure

### Using restore.ps1

```powershell
./backup/restore.ps1 -BackupFile .\backups\hotelpro_2026-05-15.dump
```

### Manual Restore

```bash
# For .dump files
docker compose exec -T postgres pg_restore -U hotel -d hotelpro /backups/filename.dump

# For .sql files
cat backup.sql | docker compose exec -T postgres psql -U hotel -d hotelpro

# For .sql.gz files
docker compose exec -T postgres gunzip -c /backups/backup.sql.gz | psql -U hotel -d hotelpro
```

### Testing Backups

Test your backup at least once a month:

1. Create a fresh database: `docker compose down -v && docker compose up -d`
2. Restore the latest backup
3. Verify data integrity
