<#
.SYNOPSIS
    Restores a PostgreSQL backup for HotelPRO.
.DESCRIPTION
    Restores a .dump or .sql.gz backup file to the HotelPRO PostgreSQL database.
    Supports both Docker and direct PostgreSQL connections.
.PARAMETER BackupFile
    Path to the backup file (.dump or .sql.gz).
.PARAMETER Database
    Target database name (default: hotelpro).
.PARAMETER Host
    PostgreSQL host (default: localhost).
.PARAMETER Port
    PostgreSQL port (default: 5432).
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,

    [string]$Database = "hotelpro",
    [string]$Host = "localhost",
    [int]$Port = 5432
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $BackupFile)) {
    Write-Error "Backup file not found: $BackupFile"
    exit 1
}

$extension = [System.IO.Path]::GetExtension($BackupFile).ToLower()

if ($extension -eq ".gz") {
    $decodedFile = "$BackupFile".Replace(".gz", "")
    Write-Host "Decompressing .gz file..."
    if ($IsWindows -or $PSVersionTable.PSVersion.Major -le 5) {
        # Use gunzip if available, otherwise use .NET
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $inputStream = New-Object System.IO.FileStream($BackupFile, [System.IO.FileMode]::Open)
        $gzipStream = New-Object System.IO.Compression.GZipStream($inputStream, [System.IO.Compression.CompressionMode]::Decompress)
        $outputStream = New-Object System.IO.FileStream($decodedFile, [System.IO.FileMode]::Create)
        $gzipStream.CopyTo($outputStream)
        $gzipStream.Close()
        $outputStream.Close()
        $inputStream.Close()
    } else {
        tar -xzf $BackupFile -C (Split-Path $BackupFile -Parent)
    }
    $BackupFile = $decodedFile
    $extension = [System.IO.Path]::GetExtension($BackupFile).ToLower()
}

Write-Host ""
Write-Host "WARNING: Database '$Database' will be dropped and replaced." -ForegroundColor Yellow
Write-Host "Continue? (y/N)" -ForegroundColor Yellow
$confirmation = Read-Host

if ($confirmation -ne "y" -and $confirmation -ne "Y") {
    Write-Host "Restore cancelled."
    exit 0
}

Write-Host "Restoring backup: $BackupFile"

# Validate Docker context
$dockerComposeRunning = docker compose ps --format json 2>$null
if (-not $dockerComposeRunning -or $LASTEXITCODE -ne 0) {
    Write-Error "Docker Compose services are not running. Start them first: docker compose up -d"
    exit 1
}

# Check if postgres container is healthy
$pgStatus = docker compose ps postgres --format "{{.Status}}" 2>$null
if ($pgStatus -notmatch "healthy") {
    Write-Error "PostgreSQL container is not healthy. Status: $pgStatus"
    exit 1
}

# Copy backup file to Docker volume if not already accessible
$backupFileName = Split-Path $BackupFile -Leaf
$backupInContainer = "/backups/$backupFileName"

# Check if file is already in the backups volume
$fileExists = docker compose exec -T postgres test -f $backupInContainer 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Copying backup file to Docker volume..."
    docker cp $BackupFile "hotelpro-backup:$backupInContainer"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to copy backup file to Docker volume."
        exit 1
    }
}

if ($extension -eq ".dump") {
    & docker compose exec -T postgres dropdb -U hotel --if-exists $Database 2>$null
    & docker compose exec -T postgres createdb -U hotel $Database
    & docker compose exec -T postgres pg_restore -U hotel -d $Database -c --if-exists $backupInContainer
} elseif ($extension -eq ".sql") {
    & docker compose exec -T postgres psql -U hotel -d postgres -c "DROP DATABASE IF EXISTS $Database;"
    & docker compose exec -T postgres psql -U hotel -d postgres -c "CREATE DATABASE $Database;"
    Get-Content $BackupFile | & docker compose exec -T postgres psql -U hotel -d $Database
} else {
    Write-Error "Unsupported backup format: $extension"
    exit 1
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "Restore completed successfully." -ForegroundColor Green
} else {
    Write-Error "Restore failed with exit code: $LASTEXITCODE"
    exit 1
}
