<#
.SYNOPSIS
    Syncs local PostgreSQL backups to AWS S3.
.DESCRIPTION
    Uses aws s3 sync to upload local backup files to an S3 bucket.
    Requires AWS CLI to be installed and configured.
.PARAMETER DryRun
    If specified, shows what would be uploaded without actually uploading.
#>

param(
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$awsAccessKey = $env:AWS_ACCESS_KEY_ID
$awsSecretKey = $env:AWS_SECRET_ACCESS_KEY
$s3Bucket = $env:S3_BUCKET
$s3Prefix = $env:S3_PREFIX
$backupDir = Join-Path $PSScriptRoot "..\backups"

if (-not $awsAccessKey -or -not $awsSecretKey -or -not $s3Bucket) {
    Write-Error "AWS credentials not configured. Set AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, and S3_BUCKET environment variables."
    exit 1
}

$env:AWS_ACCESS_KEY_ID = $awsAccessKey
$env:AWS_SECRET_ACCESS_KEY = $awsSecretKey

$s3Path = "s3://$s3Bucket"
if ($s3Prefix) {
    $s3Path = "$s3Path/$s3Prefix"
}

$awsArgs = @("s3", "sync", $backupDir, $s3Path, "--storage-class", "STANDARD_IA", "--checksum-mode", "ENABLED")

if ($DryRun) {
    $awsArgs += "--dryrun"
    Write-Host "DRY RUN - showing what would be uploaded:"
} else {
    Write-Host "Syncing backups to $s3Path ..."
}

& aws @awsArgs

if ($LASTEXITCODE -eq 0) {
    if ($DryRun) {
        Write-Host "Dry run completed. No files were uploaded." -ForegroundColor Green
    } else {
        Write-Host "S3 sync completed successfully with checksum verification." -ForegroundColor Green
    }
} else {
    Write-Error "S3 sync failed with exit code: $LASTEXITCODE"
    exit 1
}
