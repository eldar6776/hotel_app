<#
.SYNOPSIS
    Syncs local PostgreSQL backups to AWS S3.
.DESCRIPTION
    Uses aws s3 sync to upload local backup files to an S3 bucket.
    Requires AWS CLI to be installed and configured.
#>

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

Write-Host "Syncing backups to $s3Path ..."
aws s3 sync $backupDir $s3Path --storage-class STANDARD_IA

if ($LASTEXITCODE -eq 0) {
    Write-Host "S3 sync completed successfully." -ForegroundColor Green
} else {
    Write-Error "S3 sync failed with exit code: $LASTEXITCODE"
    exit 1
}
