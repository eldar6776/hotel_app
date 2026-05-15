<#
.SYNOPSIS
    Generates secure random values for HotelPRO environment variables.
.DESCRIPTION
    Generates random strings for JWT_SECRET, ADMIN_PASSWORD, and SMTP_PASSWORD.
    Compatible with PowerShell 5.1 (Windows) and PowerShell Core (macOS/Linux).
#>

function Generate-RandomString {
    param(
        [int]$Length = 32,
        [string]$CharSet = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*'
    )
    $bytes = New-Object byte[] ($Length)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $result = ''
    for ($i = 0; $i -lt $Length; $i++) {
        $result += $CharSet[$bytes[$i] % $CharSet.Length]
    }
    return $result
}

$jwtSecret = Generate-RandomString -Length 32
$adminPassword = Generate-RandomString -Length 16
$smtpPassword = Generate-RandomString -Length 12

Write-Host "Generated secrets for HotelPRO:"
Write-Host ""
Write-Host "HOTEL_JWT_SECRET=$jwtSecret"
Write-Host "HOTEL_ADMIN_PASSWORD=$adminPassword"
Write-Host "SMTP_PASSWORD=$smtpPassword"
Write-Host ""
Write-Host "Copy these values to infrastructure/.env file."
Write-Host "NEVER commit .env to version control."
